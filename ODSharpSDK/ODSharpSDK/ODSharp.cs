// <copyright file="ODSharp.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

namespace ODSharpSDK
{
    using System;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using Ensage;
    using Ensage.Common.Menu;
    using Ensage.Common.Enums;
    using Ensage.Common.Extensions;
    using Ensage.Common.Threading;
    using Ensage.SDK.Extensions;
    using Ensage.SDK.Handlers;
    using Ensage.SDK.Helpers;
    using Ensage.SDK.TargetSelector;
    using Ensage.SDK.Input;
    using Ensage.SDK.Inventory;
    using Ensage.SDK.Prediction;
    using Ensage.SDK.Prediction.Collision;
    using Ensage.SDK.Orbwalker;
    using Ensage.SDK.Orbwalker.Modes;

    using log4net;

    using PlaySharp.Toolkit.Logging;

    using SharpDX;

    using AbilityId = Ensage.AbilityId;
    using UnitExtensions = Ensage.SDK.Extensions.UnitExtensions;

    public class ODSharp : KeyPressOrbwalkingModeAsync
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly OdSharpConfig Config;

        private readonly TaskHandler KillStealHandler;

        private readonly IInventoryManager inventoryMgr;

        private readonly ITargetSelector targetSelector;

        private IPrediction Prediction { get; }

        private readonly Unit owner;

        private InventoryItem BloodThorn { get; set; }

        private InventoryItem SheepStick { get; set; }

        private InventoryItem HurricanePike { get; set; }

        private InventoryItem BlinkDagger { get; set; }

        private InventoryItem Orchid { get; set; }

        private InventoryItem RodofAtos { get; set; }

        private InventoryItem VeilofDiscord { get; set; }

        private Ability Imprison { get; set; }

        private Ability Orb { get; set; }

        private Ability Ulti { get; set; }

        public ODSharp(IOrbwalker orbwalker, IInputManager input, Key key, OdSharpConfig config, IInventoryManager inventoryMgr, ITargetSelector targetselector, IPrediction prediction)
            : base(orbwalker, input, key)
        {
            this.Config = config;
            this.Key = Config.Key.Item.GetValue<Key>(); // wat i've done
            this.targetSelector = targetselector;
            this.inventoryMgr = inventoryMgr;
            this.Prediction = Prediction;
            this.KillStealHandler = UpdateManager.Run(this.KillStealAsync, false);

            this.owner = orbwalker.Context.Owner;
        }

        public override async Task ExecuteAsync(CancellationToken token)
        {
            var target = targetSelector.GetTargets().FirstOrDefault();

            if (!await MoveOrBlinkToEnemy(target, token))
            {
                Log.Debug($"return move or blink");
                return;
            }

            var silenced = Ensage.SDK.Extensions.UnitExtensions.IsSilenced(this.owner);

            if (!silenced)
            {
                var targets = targetSelector.GetTargets().Cast<Hero>();
                var me = owner as Hero;

                foreach (var ultiTarget in targets)
                {
                    if (ultiTarget != null && this.Config.AbilityToggler.Value.IsEnabled(this.Ulti.Name) && this.Ulti.CanBeCasted(ultiTarget))
                    {
                        if (me == null) return;
                        var ultiDamage =
                            Math.Floor(this.Ulti.GetAbilitySpecialData("damage_multiplier") *
                                       (me.TotalIntelligence - ultiTarget.TotalIntelligence) * (1 - ultiTarget.MagicDamageResist));

                        if (ultiTarget.Health > ultiDamage) return;

                        var delay = GetAbilityDelay(ultiTarget, this.Ulti);
                        var radius = this.Ulti.GetAbilitySpecialData("radius");
                        var input =
                            new PredictionInput(owner, ultiTarget, delay, float.MaxValue, 700, radius,
                                PredictionSkillshotType.SkillshotCircle, true)
                            {
                                CollisionTypes = CollisionTypes.EnemyHeroes
                            };

                        // Log.Debug($"Owner: {input.Owner.Name}");
                        // Log.Debug($"Delay: {input.Delay}");
                        // Log.Debug($"Range: {input.Range}");
                        // Log.Debug($"Speed: {input.Speed}");
                        // Log.Debug($"Radius: {input.Radius}");
                        // Log.Debug($"Type: {input.PredictionSkillshotType}");

                        var output = this.Prediction.GetPrediction(input);
                        var amount = output.AoeTargetsHit.Count;

                        if (output.HitChance >= HitChance.Medium && this.Config.MinimumTargetToUlti.Item.GetValue<int>() <= amount)
                        {
                            Log.Debug($"Using Ulti on {output.CastPosition} because {ultiTarget.Health} < {ultiDamage} and {this.Config.MinimumTargetToUlti.Item.GetValue<int>()} <= {amount}");
                            this.Ulti.UseAbility(output.CastPosition);
                            await Await.Delay(delay + (int) Game.Ping, token);
                        }
                    }
                }

                if (this.CanExecute && this.Config.AbilityToggler.Value.IsEnabled(this.Orb.Name) && this.Orb.CanBeCasted(target) && !this.Orb.IsAutoCastEnabled)
                {
                    Log.Debug($"Toggling Arcane Orb on because {target != null}");
                    this.Orb.ToggleAutocastAbility();
                    await Await.Delay(125 + (int)Game.Ping, token);
                }
                // Toggle off if target is null
                else if (this.Config.AbilityToggler.Value.IsEnabled(this.Orb.Name) && target == null && this.Orb.IsAutoCastEnabled)
                {
                    Log.Debug($"Toggling Arcane Orb off because {target is null}");
                    this.Orb.ToggleAutocastAbility();
                    await Await.Delay(125 + (int)Game.Ping, token);
                }

            }

            if (this.BloodThorn != null && this.BloodThorn.IsValid && target != null && this.BloodThorn.Item.CanBeCasted(target) && this.Config.AbilityToggler.Value.IsEnabled("item_bloodthorn"))
            {
                Log.Debug("Using Bloodthorn");
                this.BloodThorn.Item.UseAbility(target);
                await Await.Delay(GetItemDelay(target) + (int) Game.Ping, token);
            }

            if (this.SheepStick != null && this.SheepStick.IsValid && target != null && this.SheepStick.Item.CanBeCasted(target) && this.Config.AbilityToggler.Value.IsEnabled("item_sheepstick"))
            {
                Log.Debug("Using Sheepstick");
                this.SheepStick.Item.UseAbility(target);
                await Await.Delay(GetItemDelay(target) + (int)Game.Ping, token);
            }

            if (this.Orchid != null && this.Orchid.IsValid && target != null && this.Orchid.Item.CanBeCasted(target) && this.Config.AbilityToggler.Value.IsEnabled("item_orchid"))
            {
                Log.Debug("Using Orchid");
                this.Orchid.Item.UseAbility(target);
                await Await.Delay(GetItemDelay(target) + (int)Game.Ping, token);
            }

            if (this.RodofAtos != null && this.RodofAtos.IsValid && target != null && this.RodofAtos.Item.CanBeCasted(target) && this.Config.AbilityToggler.Value.IsEnabled("item_rod_of_atos"))
            {
                Log.Debug("Using RodofAtos");
                this.RodofAtos.Item.UseAbility(target);
                await Await.Delay(GetItemDelay(target) + (int)Game.Ping, token);
            }

            if (this.VeilofDiscord != null && this.VeilofDiscord.IsValid && target != null && this.VeilofDiscord.Item.CanBeCasted() && this.Config.AbilityToggler.Value.IsEnabled("item_veil_of_discord"))
            {
                Log.Debug("Using VeilofDiscord");
                this.VeilofDiscord.Item.UseAbility(target.Position);
                await Await.Delay(GetItemDelay(target) + (int)Game.Ping, token);
            }

            if (this.HurricanePike != null && this.HurricanePike.IsValid && target != null && this.HurricanePike.Item.CanBeCasted() && this.Config.AbilityToggler.Value.IsEnabled("item_hurricane_pike"))
            {
                Log.Debug("Using HurricanePike");
                this.HurricanePike.Item.UseAbility(target);
                await Await.Delay(GetItemDelay(target) + (int)Game.Ping, token);
            }

            this.KillStealHandler.RunAsync();
            await Await.Delay(125, token);
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            this.Imprison = UnitExtensions.GetAbilityById(this.owner, AbilityId.obsidian_destroyer_astral_imprisonment);
            this.Orb = UnitExtensions.GetAbilityById(this.owner, AbilityId.obsidian_destroyer_arcane_orb);
            this.Ulti = UnitExtensions.GetAbilityById(this.owner, AbilityId.obsidian_destroyer_sanity_eclipse);

            this.inventoryMgr.CollectionChanged += this.OnInventoryChanged;
        }

        protected override void OnDeactivate()
        {
            this.inventoryMgr.CollectionChanged -= this.OnInventoryChanged;

            base.OnDeactivate();
        }

        private void OnInventoryChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems.OfType<InventoryItem>())
                {
                    switch (item.Id)
                    {
                        case ItemId.item_bloodthorn:
                            this.BloodThorn = item;
                            break;

                        case ItemId.item_sheepstick:
                            this.SheepStick = item;
                            break;

                        case ItemId.item_hurricane_pike:
                            this.HurricanePike = item;
                            break;

                        case ItemId.item_blink:
                            this.BlinkDagger = item;
                            break;

                        case ItemId.item_orchid:
                            this.Orchid = item;
                            break;
                        case ItemId.item_rod_of_atos:
                            this.RodofAtos = item;
                            break;

                        case ItemId.item_veil_of_discord:
                            this.VeilofDiscord = item;
                            break;
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems.OfType<InventoryItem>())
                {
                    switch (item.Id)
                    {
                        case ItemId.item_bloodthorn:
                            this.BloodThorn = null;
                            break;

                        case ItemId.item_sheepstick:
                            this.SheepStick = null;
                            break;

                        case ItemId.item_hurricane_pike:
                            this.HurricanePike = null;
                            break;

                        case ItemId.item_blink:
                            this.BlinkDagger = null;
                            break;

                        case ItemId.item_orchid:
                            this.Orchid = null;
                            break;
                        case ItemId.item_rod_of_atos:
                            this.RodofAtos = null;
                            break;

                        case ItemId.item_veil_of_discord:
                            this.VeilofDiscord = null;
                            break;
                    }
                }
            }
        }


        private async Task KillStealAsync(CancellationToken args)
        {

            var enemies =
                EntityManager<Hero>.Entities.Where(
                        x =>
                            x.IsValid && x.UnitState != UnitState.MagicImmune && x.IsAlive && !x.IsIllusion &&
                            x.Team != owner.Team && x.Distance2D(this.owner) <= this.Imprison.CastRange &&
                            GetImprisonDamage(x) >= (int)x.Health)
                    .ToList();

            if (!enemies.Any())
            {
                return;
            }
            foreach (var enemy in enemies)
            {
                if (enemy.IsValid && this.Config.AbilityToggler.Value.IsEnabled(this.Imprison.Name) && this.Imprison.CanBeCasted())
                {
                    this.Imprison.UseAbility(enemy);
                    await Await.Delay(GetAbilityDelay(enemy, Imprison), args);
                }
            }

            await Await.Delay(250, args);
        }

        // Credits: Zynox
        protected async Task<bool> MoveOrBlinkToEnemy(Unit target, CancellationToken token, float minimumRange = 0.0f, float maximumRange = 0.0f)
        {
            var distance = owner.Distance2D(target) - target.HullRadius - owner.HullRadius;
            var sliderValue = Config.UseBlinkPrediction.Item.GetValue<Slider>().Value;

            var testRange = Math.Abs(maximumRange) < 0.0f ? owner.GetAttackRange() : maximumRange;
            if (distance <= testRange)
            {
                return true;
            }

            if (owner.IsMuted()) return false;
            if (!Config.ItemToggler.Value.IsEnabled("item_blink")) return false;
            if (this.BlinkDagger == null || !this.BlinkDagger.Item.CanBeCasted()) return false;
            var blinkRange = this.BlinkDagger.Item.AbilitySpecialData.First(x => x.Name == "blink_range").Value;
            if (distance <= blinkRange)
            {
                if (Math.Abs(minimumRange) < 0.0f)
                {
                    minimumRange = owner.GetAttackRange() / 2;
                }

                var pos = (target.NetworkPosition - owner.NetworkPosition).Normalized();
                pos *= minimumRange;
                pos = target.NetworkPosition - pos;
                if (sliderValue != 0)
                {
                    var l = (owner.Distance2D(target) - sliderValue) / sliderValue;
                    var posA = owner.Position;
                    var posB = target.Position;
                    var x = (posA.X + l * posB.X) / (1 + l);
                    var y = (posA.Y + l * posB.Y) / (1 + l);
                    var position = new Vector3((int)x, (int)y, posA.Z);
                    this.BlinkDagger.Item.UseAbility(position);
                    await Await.Delay(GetItemDelay(pos), token);
                    return false;
                }
                else if (sliderValue == 0)
                {
                    this.BlinkDagger.Item.UseAbility(pos);
                    await Await.Delay(GetItemDelay(pos), token);
                    return false;
                }

            }
            return false;
        }

        protected int GetAbilityDelay(Unit unit, Ability ability)
        {
            return (int)((ability.FindCastPoint() + owner.GetTurnTime(unit)) * 1000.0 + Game.Ping) + 50;
        }

        protected int GetItemDelay(Unit unit)
        {
            return (int)(owner.GetTurnTime(unit) * 1000.0 + Game.Ping) + 100;
        }

        protected int GetItemDelay(Vector3 pos)
        {
            return (int)(owner.GetTurnTime(pos) * 1000.0 + Game.Ping) + 100;
        }

        protected int GetImprisonDamage(Unit target)
        {
            return (int)Math.Floor((this.Imprison.GetAbilitySpecialData("damage") * (1 - target.MagicDamageResist)) - (target.HealthRegeneration * 5));
        }

    }
}