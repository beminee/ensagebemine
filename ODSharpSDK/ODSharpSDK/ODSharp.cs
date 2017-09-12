// <copyright file="ODSharp.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

using Ensage.SDK.Abilities;

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
    using Ensage.SDK.Abilities.Items;
    using Ensage.SDK.Inventory.Metadata;
    using Ensage.SDK.Service;
    using Ensage.SDK.Service.Metadata;

    using log4net;

    using PlaySharp.Toolkit.Helper.Annotations;
    using PlaySharp.Toolkit.Logging;

    using SharpDX;

    using AbilityId = Ensage.AbilityId;
    using UnitExtensions = Ensage.SDK.Extensions.UnitExtensions;

    [PublicAPI]
    public class ODSharp : KeyPressOrbwalkingModeAsync
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public OdSharpConfig Config { get; }

        private readonly IServiceContext context;

        private IPrediction Prediction { get; }

        private ITargetSelectorManager TargetSelector { get; }

        private TaskHandler KillStealHandler { get; set; }

        private Ability Orb { get; set; }

        public Ability Ulti { get; set; }

        private Ability Imprison { get; set; }

        public ODSharp(
            Key key,
            OdSharpConfig config,
            IServiceContext context)
            : base(context, key)
        {
            this.context = context;
            this.Config = config;
            this.TargetSelector = context.TargetSelector;
            this.Prediction = context.Prediction;
        }


        [ItemBinding]
        public item_orchid Orchid { get; private set; }

        [ItemBinding]
        public item_blink BlinkDagger { get; private set; }

        [ItemBinding]
        public item_bloodthorn BloodThorn { get; private set; }

        [ItemBinding]
        public item_hurricane_pike HurricanePike { get; private set; }

        [ItemBinding]
        public item_rod_of_atos RodofAtos { get; private set; }

        [ItemBinding]
        public item_sheepstick SheepStick { get; private set; }

        [ItemBinding]
        public item_shivas_guard ShivasGuard { get; private set; }

        [ItemBinding]
        public item_veil_of_discord VeilofDiscord { get; private set; }

        public override async Task ExecuteAsync(CancellationToken token)
        {

            var target = this.TargetSelector.Active.GetTargets().FirstOrDefault(x => !x.IsInvulnerable() && x.Distance2D(this.Owner) <= this.Owner.AttackRange * 2);

            var silenced = UnitExtensions.IsSilenced(this.Owner);

            var sliderValue = this.Config.UseBlinkPrediction.Item.GetValue<Slider>().Value;

            var modifier = Ensage.SDK.Extensions.UnitExtensions.HasModifier(this.Owner, HurricanePike.ModifierName);

            if ((this.BlinkDagger != null) &&
            (this.BlinkDagger.Item.IsValid) &&
            target != null && Owner.Distance2D(target) <= 1200 + sliderValue && !(Owner.Distance2D(target) <= 400) &&
            this.BlinkDagger.Item.CanBeCasted(target) &&
            this.Config.ItemToggler.Value.IsEnabled(this.BlinkDagger.Item.Name))
            {
                var l = (this.Owner.Distance2D(target) - sliderValue) / sliderValue;
                var posA = this.Owner.Position;
                var posB = target.Position;
                var x = (posA.X + (l * posB.X)) / (1 + l);
                var y = (posA.Y + (l * posB.Y)) / (1 + l);
                var position = new Vector3((int)x, (int)y, posA.Z);

                Log.Debug("Using BlinkDagger");
                this.BlinkDagger.UseAbility(position);
                await Await.Delay(this.GetItemDelay(target), token);
            }

            if (!silenced)
            {
                try
                {

                var targets =
                    EntityManager<Hero>.Entities.Where(
                            x => x.IsValid && x.Team != this.Owner.Team && !x.IsIllusion && x.Distance2D(this.Owner) <= 700)
                        .ToList();
                var me = this.Owner as Hero;

                foreach (var ultiTarget in targets)
                {
                    if (this.Config.AbilityToggler.Value.IsEnabled(this.Ulti.Name) && this.Ulti.CanBeCasted(ultiTarget))
                    {

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
                                CollisionTypes = CollisionTypes.None
                            };

                        // Log.Debug($"Owner: {input.Owner.Name}");
                        // Log.Debug($"Delay: {input.Delay}");
                        // Log.Debug($"Range: {input.Range}");
                        // Log.Debug($"Speed: {input.Speed}");
                        // Log.Debug($"Radius: {input.Radius}");
                        // Log.Debug($"Type: {input.PredictionSkillshotType}");
                        var output = this.Prediction.GetPrediction(input);
                        var amount = output.AoeTargetsHit.Count;

                       // Log.Debug($"{output.HitChance}");

                        if (output.HitChance >= HitChance.Medium && this.Config.MinimumTargetToUlti.Item.GetValue<int>() >= amount)
                        {
                            Log.Debug(
                                $"Using Ulti on {amount}!");
                            this.Ulti.UseAbility(output.CastPosition);
                            await Await.Delay(delay + (int)Game.Ping, token);
                        }

                    }
                }
                }
                catch (Exception e)
                {
                    Log.Debug($"{e}");
                }

                if (this.Orb != null && this.Orb.IsValid && this.Config.AbilityToggler.Value.IsEnabled(this.Orb.Name) && this.Orb.CanBeCasted(target) && !this.Orb.IsAutoCastEnabled)
                {
                    Log.Debug($"Toggling Arcane Orb on because {target != null}");
                    this.Orb.ToggleAutocastAbility();
                    await Await.Delay(100 + (int)Game.Ping, token);
                }

                // Toggle off if target is null
                else if (this.Orb != null && this.Orb.IsValid && this.Config.AbilityToggler.Value.IsEnabled(this.Orb.Name) && target == null && this.Orb.IsAutoCastEnabled)
                {
                    Log.Debug($"Toggling Arcane Orb off because target is null");
                    this.Orb.ToggleAutocastAbility();
                    await Await.Delay(100 + (int)Game.Ping, token);
                }
            }

            if ((this.BloodThorn != null) &&
                this.BloodThorn.Item.IsValid &&
                target != null &&
                this.BloodThorn.Item.CanBeCasted(target) &&
                this.Config.ItemToggler.Value.IsEnabled(this.BloodThorn.Item.Name))
            {
                Log.Debug("Using Bloodthorn");
                this.BloodThorn.UseAbility(target);
                await Await.Delay(this.GetItemDelay(target), token);
            }

            if ((this.SheepStick != null) &&
                this.SheepStick.Item.IsValid &&
                target != null &&
                this.SheepStick.Item.CanBeCasted(target) &&
                this.Config.ItemToggler.Value.IsEnabled("item_sheepstick"))
            {
                Log.Debug("Using Sheepstick");
                this.SheepStick.UseAbility(target);
                await Await.Delay(this.GetItemDelay(target), token);
            }

            if ((this.Orchid != null) &&
                this.Orchid.Item.IsValid && target != null &&
                this.Orchid.Item.CanBeCasted(target) &&
                this.Config.ItemToggler.Value.IsEnabled("item_orchid"))
            {
                Log.Debug("Using Orchid");
                this.Orchid.UseAbility(target);
                await Await.Delay(this.GetItemDelay(target), token);
            }

            if ((this.RodofAtos != null) &&
                this.RodofAtos.Item.IsValid &&
                target != null &&
                this.RodofAtos.Item.CanBeCasted(target) &&
                this.Config.ItemToggler.Value.IsEnabled("item_rod_of_atos"))
            {
                Log.Debug("Using RodofAtos");
                this.RodofAtos.UseAbility(target);
                await Await.Delay(this.GetItemDelay(target), token);
            }

            if ((this.VeilofDiscord != null) &&
                this.VeilofDiscord.Item.IsValid &&
                target != null &&
                this.VeilofDiscord.Item.CanBeCasted() &&
                this.Config.ItemToggler.Value.IsEnabled("item_veil_of_discord"))
            {
                Log.Debug("Using VeilofDiscord");
                this.VeilofDiscord.UseAbility(target.Position);
                await Await.Delay(this.GetItemDelay(target), token);
            }

            if (this.HurricanePike != null)
            {
<<<<<<< HEAD
				if (modifier)
=======
				if (this.Owner.HasModifier(this.HurricanePike.ModifierName))
>>>>>>> origin/master
				{
					this.Owner.Attack(target);
                    await Task.Delay(100, token);
                    return;
				}
				
				if ((double)(this.Owner.Health / this.Owner.MaximumHealth) * 100 <= 
                (double)Config.HurricanePercentage.Item.GetValue<Slider>().Value &&
                this.HurricanePike.Item.IsValid &&
                target != null &&
                this.HurricanePike.Item.CanBeCasted() &&
                this.Config.ItemToggler.Value.IsEnabled("item_hurricane_pike"))
				{
                Log.Debug("Using HurricanePike");
                this.HurricanePike.UseAbility(target);
                await Await.Delay(this.GetItemDelay(target), token);
				return;
				}
            }

            if ((this.ShivasGuard != null) &&
                this.ShivasGuard.Item.IsValid &&
                target != null && this.Owner.Distance2D(target) <= 900 &&
                this.ShivasGuard.Item.CanBeCasted() &&
                this.Config.ItemToggler.Value.IsEnabled("item_shivas_guard"))
            {
                Log.Debug("Using Shivas");
                this.ShivasGuard.UseAbility();
                await Await.Delay(20 + (int)Game.Ping, token);
            }

            if (this.Orbwalker.OrbwalkTo(target))
            {
                
                return;
            }

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
            base.OnActivate();

            KillStealHandler = UpdateManager.Run(KillStealAsync, true, true);

            this.context.Inventory.Attach(this);

            this.Imprison = UnitExtensions.GetAbilityById(this.Owner, AbilityId.obsidian_destroyer_astral_imprisonment);
            this.Orb = UnitExtensions.GetAbilityById(this.Owner, AbilityId.obsidian_destroyer_arcane_orb);
            this.Ulti = UnitExtensions.GetAbilityById(this.Owner, AbilityId.obsidian_destroyer_sanity_eclipse);
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();

            KillStealHandler.Cancel();

            this.context.Inventory.Detach(this);
        }

        public virtual async Task KillStealAsync(CancellationToken args)
        {

            if (!Config.KillStealEnabled || Game.IsPaused || !Owner.IsAlive || UnitExtensions.IsChanneling(Owner))
            {
                return;
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
                return;
            }

            foreach (var enemy in enemies)
            {
                if (enemy.IsValid && this.Config.AbilityToggler.Value.IsEnabled(this.Imprison.Name) && this.Imprison.CanBeCasted())
                {
                    Log.Debug($"Using Imprison because enemy can be ks'ed.");
                    this.Imprison.UseAbility(enemy);
                    await Await.Delay(this.GetAbilityDelay(enemy, this.Imprison));
                    return;
                }
            }

            await Await.Delay(250, args);
            return;
        }
    }
}
