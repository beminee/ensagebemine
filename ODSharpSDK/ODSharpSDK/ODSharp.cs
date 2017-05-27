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
    using Ensage.Common.Enums;
    using Ensage.Common.Extensions;
    using Ensage.Common.Menu;
    using Ensage.Common.Threading;
    using Ensage.SDK.Extensions;
    using Ensage.SDK.Handlers;
    using Ensage.SDK.Helpers;
    using Ensage.SDK.Input;
    using Ensage.SDK.Inventory;
    using Ensage.SDK.Orbwalker;
    using Ensage.SDK.Orbwalker.Modes;
    using Ensage.SDK.Prediction;
    using Ensage.SDK.Prediction.Collision;
    using Ensage.SDK.TargetSelector;

    using log4net;

    using PlaySharp.Toolkit.Logging;

    using SharpDX;

    using AbilityId = Ensage.AbilityId;
    using UnitExtensions = Ensage.SDK.Extensions.UnitExtensions;

    public class ODSharp : KeyPressOrbwalkingModeAsync
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ODSharp(
            Key key,
            OdSharpConfig config,
            Lazy<IOrbwalkerManager> orbwalker,
            Lazy<IInputManager> input,
            Lazy<IInventoryManager> inventory,
            Lazy<ITargetSelectorManager> targetselector,
            Lazy<IPrediction> prediction)
            : base(orbwalker.Value, input.Value, key)
        {
            this.Config = config;
            this.TargetSelector = targetselector;
            this.Inventory = inventory;
            this.Prediction = prediction;
        }

        public OdSharpConfig Config { get; }

        private Item BlinkDagger { get; set; }

        private Item BloodThorn { get; set; }

        private Item HurricanePike { get; set; }

        private Ability Imprison { get; set; }

        private Lazy<IInventoryManager> Inventory { get; }

        private TaskHandler KillStealHandler { get; set; }

        private Ability Orb { get; set; }

        private Item Orchid { get; set; }

        private Lazy<IPrediction> Prediction { get; }

        private Item RodofAtos { get; set; }

        private Item SheepStick { get; set; }

        private Lazy<ITargetSelectorManager> TargetSelector { get; }

        private Ability Ulti { get; set; }

        private Item VeilofDiscord { get; set; }

        public override async Task ExecuteAsync(CancellationToken token)
        {
            var target = this.TargetSelector.Value.Active.GetTargets().FirstOrDefault();

            var silenced = UnitExtensions.IsSilenced(this.Owner);

            var sliderValue = this.Config.UseBlinkPrediction.Item.GetValue<Slider>().Value;

            if (this.BlinkDagger != null &&
            this.BlinkDagger.IsValid &&
            target != null && Owner.Distance2D(target) <= 1200 + sliderValue &&
            this.BlinkDagger.CanBeCasted(target) &&
            this.Config.ItemToggler.Value.IsEnabled(this.BlinkDagger.Name))
            {
                var l = (this.Owner.Distance2D(target) - sliderValue) / sliderValue;
                var posA = this.Owner.Position;
                var posB = target.Position;
                var x = (posA.X + (l * posB.X)) / (1 + l);
                var y = (posA.Y + (l * posB.Y)) / (1 + l);
                var position = new Vector3((int)x, (int)y, posA.Z);

                Log.Debug("Using BlinkDagger");
                this.BlinkDagger.UseAbility(position);
                await Await.Delay(this.GetItemDelay(target) + (int)Game.Ping, token);
            }

            if (!silenced)
            {
                var targets = this.TargetSelector.Value.Active.GetTargets().Cast<Hero>();
                var me = this.Owner as Hero;

                foreach (var ultiTarget in targets)
                {

                    if (ultiTarget != null && this.Config.AbilityToggler.Value.IsEnabled(this.Ulti.Name) && this.Ulti.CanBeCasted(ultiTarget))
                    {

                        if (me == null)
                        {
                            return;
                        }

                        var ultiDamage =
                                Math.Floor(
                                    this.Ulti.GetAbilitySpecialData("damage_multiplier") *
                                    (me.TotalIntelligence - ultiTarget.TotalIntelligence) *
                                    (1 - ultiTarget.MagicDamageResist));


                        if (ultiTarget.Health > ultiDamage)
                        {
                            continue;
                        }

                        var delay = this.GetAbilityDelay(ultiTarget, this.Ulti);
                        var radius = this.Ulti.GetAbilitySpecialData("radius");
                        var input =
                            new PredictionInput(
                                this.Owner,
                                ultiTarget,
                                delay,
                                float.MaxValue,
                                700,
                                radius,
                                PredictionSkillshotType.SkillshotCircle,
                                true)
                            {
                                CollisionTypes = CollisionTypes.EnemyHeroes
                            };

                        // Log.Debug($"Owner: {input.Owner.Name}");
                        // Log.Debug($"Delay: {input.Delay}");
                        // Log.Debug($"Range: {input.Range}");
                        // Log.Debug($"Speed: {input.Speed}");
                        // Log.Debug($"Radius: {input.Radius}");
                        // Log.Debug($"Type: {input.PredictionSkillshotType}");
                        var output = this.Prediction.Value.GetPrediction(input);
                        var amount = output.AoeTargetsHit.Count;

                        Game.PrintMessage(output.HitChance.ToString());

                        if (output.HitChance >= HitChance.Medium && this.Config.MinimumTargetToUlti.Item.GetValue<int>() <= amount)
                        {
                            Log.Debug(
                                $"Using Ulti on {output.CastPosition} because {ultiTarget.Health} < {ultiDamage} and {this.Config.MinimumTargetToUlti.Item.GetValue<int>()} <= {amount}");
                            this.Ulti.UseAbility(output.CastPosition);
                            await Await.Delay(delay + (int)Game.Ping, token);
                            break;
                        }
                    }
                }

                if (this.CanExecute && this.Config.AbilityToggler.Value.IsEnabled(this.Orb.Name) && this.Orb.CanBeCasted(target) && !this.Orb.IsAutoCastEnabled)
                {
                    Log.Debug($"Toggling Arcane Orb on because {target != null}");
                    this.Orb.ToggleAutocastAbility();
                    await Await.Delay(100 + (int)Game.Ping, token);
                }

                // Toggle off if target is null
                else if (this.Config.AbilityToggler.Value.IsEnabled(this.Orb.Name) && target == null && this.Orb.IsAutoCastEnabled)
                {
                    Log.Debug($"Toggling Arcane Orb off because target is null");
                    this.Orb.ToggleAutocastAbility();
                    await Await.Delay(100 + (int)Game.Ping, token);
                }
            }

            if (this.BloodThorn != null &&
                this.BloodThorn.IsValid &&
                target != null &&
                this.BloodThorn.CanBeCasted(target) &&
                this.Config.ItemToggler.Value.IsEnabled(this.BloodThorn.Name))
            {
                Log.Debug("Using Bloodthorn");
                this.BloodThorn.UseAbility(target);
                await Await.Delay(this.GetItemDelay(target) + (int)Game.Ping, token);
            }

            if (this.SheepStick != null &&
                this.SheepStick.IsValid &&
                target != null &&
                this.SheepStick.CanBeCasted(target) &&
                this.Config.ItemToggler.Value.IsEnabled("item_sheepstick"))
            {
                Log.Debug("Using Sheepstick");
                this.SheepStick.UseAbility(target);
                await Await.Delay(this.GetItemDelay(target) + (int)Game.Ping, token);
            }

            if (this.Orchid != null && this.Orchid.IsValid && target != null && this.Orchid.CanBeCasted(target) && this.Config.ItemToggler.Value.IsEnabled("item_orchid"))
            {
                Log.Debug("Using Orchid");
                this.Orchid.UseAbility(target);
                await Await.Delay(this.GetItemDelay(target) + (int)Game.Ping, token);
            }

            if (this.RodofAtos != null &&
                this.RodofAtos.IsValid &&
                target != null &&
                this.RodofAtos.CanBeCasted(target) &&
                this.Config.ItemToggler.Value.IsEnabled("item_rod_of_atos"))
            {
                Log.Debug("Using RodofAtos");
                this.RodofAtos.UseAbility(target);
                await Await.Delay(this.GetItemDelay(target) + (int)Game.Ping, token);
            }

            if (this.VeilofDiscord != null &&
                this.VeilofDiscord.IsValid &&
                target != null &&
                this.VeilofDiscord.CanBeCasted() &&
                this.Config.ItemToggler.Value.IsEnabled("item_veil_of_discord"))
            {
                Log.Debug("Using VeilofDiscord");
                this.VeilofDiscord.UseAbility(target.Position);
                await Await.Delay(this.GetItemDelay(target) + (int)Game.Ping, token);
            }

            if (this.HurricanePike != null &&
                this.HurricanePike.IsValid &&
                target != null &&
                this.HurricanePike.CanBeCasted() &&
                this.Config.ItemToggler.Value.IsEnabled("item_hurricane_pike"))
            {
                Log.Debug("Using HurricanePike");
                this.HurricanePike.UseAbility(target);
                await Await.Delay(this.GetItemDelay(target) + (int)Game.Ping, token);
            }

            if (this.Orbwalker.OrbwalkTo(target))
            {

            }

            this.KillStealHandler.RunAsync();
            await Await.Delay(125, token);
        }

        protected int GetAbilityDelay(Unit unit, Ability ability)
        {
            return (int)(((ability.FindCastPoint() + this.Owner.GetTurnTime(unit)) * 1000.0) + Game.Ping) + 50;
        }

        protected int GetImprisonDamage(Unit unit)
        {
            return (int)Math.Floor((this.Imprison.GetAbilitySpecialData("damage") * (1 - unit.MagicDamageResist)) - (unit.HealthRegeneration * 5));
        }

        protected int GetItemDelay(Unit unit)
        {
            return (int)((this.Owner.GetTurnTime(unit) * 1000.0) + Game.Ping) + 100;
        }

        protected int GetItemDelay(Vector3 pos)
        {
            return (int)((this.Owner.GetTurnTime(pos) * 1000.0) + Game.Ping) + 100;
        }

        protected override void OnActivate()
        {
            this.KillStealHandler = UpdateManager.Run(this.KillStealAsync, false);

            this.Imprison = UnitExtensions.GetAbilityById(this.Owner, AbilityId.obsidian_destroyer_astral_imprisonment);
            this.Orb = UnitExtensions.GetAbilityById(this.Owner, AbilityId.obsidian_destroyer_arcane_orb);
            this.Ulti = UnitExtensions.GetAbilityById(this.Owner, AbilityId.obsidian_destroyer_sanity_eclipse);

            this.Inventory.Value.CollectionChanged += this.OnInventoryChanged;

            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
            this.Inventory.Value.CollectionChanged -= this.OnInventoryChanged;
        }

        public virtual async Task<bool> KillStealAsync(CancellationToken args)
        {
            if (!Config.KillStealEnabled)
            {
                return false;
            }
            var enemies =
                EntityManager<Hero>.Entities.Where(
                                       x =>
                                           x.IsValid &&
                                           x.UnitState != UnitState.MagicImmune &&
                                           x.IsAlive &&
                                           !x.IsIllusion &&
                                           x.Team != this.Owner.Team &&
                                           x.Distance2D(this.Owner) <= this.Imprison.CastRange &&
                                           this.GetImprisonDamage(x) >= (int)x.Health)
                                   .ToList();

            if (!enemies.Any())
            {
                return false;
            }

            foreach (var enemy in enemies)
            {
                if (enemy.IsValid && this.Config.AbilityToggler.Value.IsEnabled(this.Imprison.Name) && this.Imprison.CanBeCasted())
                {
                    Log.Debug($"Using Imprison because enemy can be ks'ed.");
                    this.Imprison.UseAbility(enemy);
                    await Await.Delay(this.GetAbilityDelay(enemy, this.Imprison), args);
                    return true;
                }
            }

            await Await.Delay(250, args);
            return false;
        }

        private void OnInventoryChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in args.NewItems.OfType<InventoryItem>())
                {
                    switch (item.Id)
                    {
                        case ItemId.item_bloodthorn:
                            this.BloodThorn = item.Item;
                            break;

                        case ItemId.item_sheepstick:
                            this.SheepStick = item.Item;
                            break;

                        case ItemId.item_hurricane_pike:
                            this.HurricanePike = item.Item;
                            break;

                        case ItemId.item_blink:
                            this.BlinkDagger = item.Item;
                            break;

                        case ItemId.item_orchid:
                            this.Orchid = item.Item;
                            break;
                        case ItemId.item_rod_of_atos:
                            this.RodofAtos = item.Item;
                            break;

                        case ItemId.item_veil_of_discord:
                            this.VeilofDiscord = item.Item;
                            break;
                    }
                }
            }
            else if (args.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in args.OldItems.OfType<InventoryItem>())
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
    }
}