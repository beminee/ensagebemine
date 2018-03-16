namespace PugnaSharpSDK
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Ensage;
    using Ensage.Heroes;
    using Ensage.Common.Enums;
    using Ensage.Common.Extensions;
    using Ensage.Common.Menu;
    using Ensage.Common.Threading;
    using Ensage.SDK.Extensions;
    using Ensage.SDK.Helpers;
    using Ensage.SDK.Inventory.Metadata;
    using Ensage.SDK.Orbwalker.Modes;
    using Ensage.SDK.Prediction;
    using Ensage.SDK.Prediction.Collision;
    using Ensage.SDK.TargetSelector;
    using Ensage.SDK.Abilities.Items;
    using Ensage.SDK.Abilities;
    using Ensage.SDK.Abilities.Aggregation;
    using Ensage.SDK.Service;
    using log4net;
    using PlaySharp.Toolkit.Logging;
    using PlaySharp.Toolkit.Helper.Annotations;
    using SharpDX;
    using AbilityId = Ensage.AbilityId;
    using UnitExtensions = Ensage.SDK.Extensions.UnitExtensions;

    [PublicAPI]
    public class PugnaSharp : KeyPressOrbwalkingModeAsync
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IServiceContext context;

        private IPrediction Prediction { get; }

        private ITargetSelectorManager TargetSelector { get; }

        public PugnaSharpConfig Config { get; }

        public PugnaSharp(
            Key key,
            PugnaSharpConfig config,
            IServiceContext context)
            : base(context, key)
        {
            this.Config = config;
            this.context = context;
            this.TargetSelector = context.TargetSelector;
            this.Prediction = context.Prediction;
        }


        [ItemBinding]
        public item_blink BlinkDagger { get; private set; }

        [ItemBinding]
        public item_bloodthorn BloodThorn { get; private set; }

        [ItemBinding]
        public item_hurricane_pike HurricanePike { get; private set; }

        [ItemBinding]
        public item_shivas_guard ShivasGuard { get; private set; }

        [ItemBinding]
        public item_mjollnir Mjollnir { get; private set; }

        [ItemBinding]
        public item_veil_of_discord VeilofDiscord { get; private set; }

        [ItemBinding]
        public item_rod_of_atos RodofAtos { get; private set; }

        [ItemBinding]
        public item_sheepstick SheepStick { get; private set; }

        [ItemBinding]
        public item_orchid Orchid { get; set; }

        [ItemBinding]
        public item_dagon Dagon1 { get; set; }

        [ItemBinding]
        public item_dagon_2 Dagon2 { get; set; }

        [ItemBinding]
        public item_dagon_3 Dagon3 { get; set; }

        [ItemBinding]
        public item_dagon_4 Dagon4 { get; set; }

        [ItemBinding]
        public item_dagon_5 Dagon5 { get; set; }

        public Dagon Dagon => Dagon1 ?? Dagon2 ?? Dagon3 ?? Dagon4 ?? (Dagon) Dagon5;


        private Ability Decrepify { get; set; }

        private Ability Ward { get; set; }

        private Ability Drain { get; set; }

        private Ability Blast { get; set; }

        private Unit Target { get; set; }

        protected bool IsHealing { get; set; }

        protected Unit HealTarget { get; set; }

        public override async Task ExecuteAsync(CancellationToken token)
        {
            Target = this.TargetSelector.Active.GetTargets().FirstOrDefault(x => !x.IsInvulnerable());

            var allTargets = this.TargetSelector.Active.GetTargets().FirstOrDefault();

            var silenced = UnitExtensions.IsSilenced(this.Owner);

            var sliderValue = this.Config.UseBlinkPrediction.Item.GetValue<Slider>().Value;

            var myHpThreshold = this.Config.SelfHPDrain.Item.GetValue<Slider>().Value;

            var postDrainHp = this.Config.PostDrainHP.Item.GetValue<Slider>().Value;

            var allyPostDrain = this.Config.HealAllyTo.Item.GetValue<Slider>().Value;

            var healThreshold = this.Config.DrainHP.Item.GetValue<Slider>().Value;

            var wardTars = this.Config.WardTargets.Item.GetValue<Slider>().Value;

            //warnings
            if (myHpThreshold < postDrainHp && this.Config.AbilityToggler.Value.IsEnabled(this.Drain.Name))
            {
                Log.Debug(
                    "Post drain hp is higher than your hp threshold to begin healing, please change this or the script won't work.");
                return;
            }

            if (healThreshold > allyPostDrain && this.Config.AbilityToggler.Value.IsEnabled(this.Drain.Name))
            {
                Log.Debug("Your ally's post heal threshold is lower than their heal threshold, please fix this.");
                return;
            }

            if (!silenced)
            {
                try
                {
                    var tempHealTarget =
                        EntityManager<Hero>.Entities.FirstOrDefault(
                            x =>
                                x.IsAlive && x.Team == this.Owner.Team && x != Owner && !x.IsIllusion
                                && ((float) x.Health / (float) x.MaximumHealth) * 100 < healThreshold
                                && !UnitExtensions.IsMagicImmune(x) && Config.HealTargetHeroes.Value.IsEnabled(x.Name));

                    var myHealth = (float) Owner.Health / (float) Owner.MaximumHealth * 100;

                    if (tempHealTarget != null)
                    {
                        HealTarget = tempHealTarget;
                    }

                    if (HealTarget != null)
                    {
                        if (HealTarget != null && this.Config.AbilityToggler.Value.IsEnabled(this.Drain.Name)
                            && !UnitExtensions.IsChanneling(Owner) && myHealth >= myHpThreshold
                            && HealTarget.Distance2D(this.Owner) <= Drain.CastRange
                            && HealTarget.HealthPercent() * 100 < healThreshold)
                        {
                            this.Drain.UseAbility(HealTarget);
                            IsHealing = true;
                            await Await.Delay(GetAbilityDelay(HealTarget, Drain), token);
                        }

                        //Stop Healing; There is no hidden modifier/any way to check if we are healing a target.
                        if ((UnitExtensions.IsChanneling(Owner) && myHealth <= postDrainHp) && IsHealing)
                        {
                            Owner.Stop();
                            IsHealing = false;
                        }

                        if (HealTarget != null && IsHealing &&
                            (HealTarget.HealthPercent() >= ((float) allyPostDrain / 100)))
                        {
                            Owner.Stop();
                            IsHealing = false;
                        }

                        if (HealTarget == null && IsHealing)
                        {
                            Owner.Stop();
                            IsHealing = false;
                        }
                    }
                }
                catch (Exception)
                {
                    // ignore
                }
            }

            if (IsHealing) return;

            if ((this.BlinkDagger != null) &&
                (this.BlinkDagger.Item.IsValid) &&
                Target != null && Owner.Distance2D(Target) <= 1200 + sliderValue && !(Owner.Distance2D(Target) <= 400) &&
                this.BlinkDagger.Item.CanBeCasted(Target) &&
                this.Config.ItemToggler.Value.IsEnabled(this.BlinkDagger.Item.Name)
                && !UnitExtensions.IsChanneling(Owner))
            {
                var l = (this.Owner.Distance2D(Target) - sliderValue) / sliderValue;
                var posA = this.Owner.Position;
                var posB = Target.Position;
                var x = (posA.X + (l * posB.X)) / (1 + l);
                var y = (posA.Y + (l * posB.Y)) / (1 + l);
                var position = new Vector3((int) x, (int) y, posA.Z);

                Log.Debug("Using BlinkDagger");
                this.BlinkDagger.UseAbility(position);
                await Await.Delay(this.GetItemDelay(position) + (int) Game.Ping, token);
            }


            if (!silenced && Target != null)
            {
                var targets =
                    EntityManager<Hero>.Entities.Where(
                            x =>
                                x.IsValid && x.Team != this.Owner.Team && !x.IsIllusion &&
                                !UnitExtensions.IsMagicImmune(x) &&
                                x.Distance2D(this.Owner) <= Ward.GetAbilityData("radius"))
                        .ToList();

                if (targets.Count >= wardTars && this.Ward.CanBeCasted() && !UnitExtensions.IsChanneling(Owner) &&
                    this.Config.AbilityToggler.Value.IsEnabled(this.Ward.Name))
                {
                    Log.Debug($"Using Ward");
                    Ward.UseAbility(Owner.NetworkPosition);
                    await Await.Delay(GetAbilityDelay(Owner, Ward), token);
                }

                try
                {
                    // var thresholdTars = this.Config.WardTargets.Item.GetValue<Slider>();
                    var manaDecrepify = Decrepify.GetManaCost(Decrepify.Level - 1);
                    var manaBlast = Blast.GetManaCost(Blast.Level - 1);
                    // var manaDrain = Drain.GetManaCost(Drain.Level - 1);

                    if (Decrepify.CanBeCasted() && Target != null && Decrepify.CanHit(Target)
                        && this.Config.AbilityToggler.Value.IsEnabled(this.Decrepify.Name)
                        && this.Owner.Mana >= manaBlast + manaDecrepify
                        && !UnitExtensions.IsChanneling(Owner)
                        && Target.IsAlive)
                    {
                        this.Decrepify.UseAbility(Target);
                        await Await.Delay(GetAbilityDelay(Target, Decrepify), token);
                    }

                    if (this.Blast.CanBeCasted()
                        && this.Config.AbilityToggler.Value.IsEnabled(this.Blast.Name)
                        && (!this.Decrepify.CanBeCasted() || manaBlast > Owner.Mana - manaDecrepify)
                        && !UnitExtensions.IsChanneling(Owner)
                        && Target != null && Target.IsAlive)
                    {
                        var delay = Blast.GetAbilityData("delay") + Blast.GetCastPoint();
                        var blastTargets =
                            EntityManager<Hero>.Entities.OrderBy(x => x == allTargets).Where(
                                x =>
                                    x.IsValid && x.IsVisible && x.Team != Owner.Team && !x.IsIllusion &&
                                    !UnitExtensions.IsMagicImmune(x)).ToList();
                        var blastCastRange = Blast.CastRange;

                        if (blastTargets == null) return;
                        var input =
                            new PredictionInput(
                                Owner,
                                Target,
                                delay,
                                float.MaxValue,
                                blastCastRange,
                                400,
                                PredictionSkillshotType.SkillshotCircle,
                                true,
                                blastTargets)
                            {
                                CollisionTypes = CollisionTypes.None
                            };

                        var output = Prediction.GetPrediction(input);

                        if (output.HitChance >= HitChance.Medium)
                        {
                            Log.Debug($"Using Blast");
                            this.Blast.UseAbility(output.CastPosition);
                            await Await.Delay(GetAbilityDelay(Target.Position, this.Blast), token);
                        }
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            if (this.BloodThorn != null &&
                this.BloodThorn.Item.IsValid &&
                Target != null && !UnitExtensions.IsChanneling(Owner) &&
                this.BloodThorn.Item.CanBeCasted(Target) &&
                this.Config.ItemToggler.Value.IsEnabled(this.BloodThorn.Item.Name))
            {
                Log.Debug("Using Bloodthorn");
                this.BloodThorn.UseAbility(Target);
                await Await.Delay(this.GetItemDelay(Target), token);
            }

            if ((this.SheepStick != null) &&
                (this.SheepStick.Item.IsValid) &&
                Target != null && !UnitExtensions.IsChanneling(Owner) &&
                this.SheepStick.Item.CanBeCasted(Target) &&
                this.Config.ItemToggler.Value.IsEnabled("item_sheepstick"))
            {
                Log.Debug("Using Sheepstick");
                this.SheepStick.UseAbility(Target);
                await Await.Delay(this.GetItemDelay(Target), token);
            }

            if (this.Dagon != null &&
                this.Dagon.Item.IsValid &&
                Target != null && !UnitExtensions.IsChanneling(Owner) &&
                this.Dagon.Item.CanBeCasted(Target) &&
                this.Config.ItemToggler.Value.IsEnabled("item_dagon_5"))
            {
                Log.Debug("Using Dagon");
                this.Dagon.UseAbility(Target);
                await Await.Delay(this.GetItemDelay(Target), token);
            }

            if (this.Orchid != null &&
                this.Orchid.Item.IsValid &&
                Target != null && !UnitExtensions.IsChanneling(Owner) &&
                this.Orchid.Item.CanBeCasted(Target) &&
                this.Config.ItemToggler.Value.IsEnabled("item_orchid"))
            {
                Log.Debug("Using Orchid");
                this.Orchid.UseAbility(Target);
                await Await.Delay(this.GetItemDelay(Target), token);
            }

            if (this.RodofAtos != null &&
                this.RodofAtos.Item.IsValid &&
                Target != null && !UnitExtensions.IsChanneling(Owner) &&
                this.RodofAtos.Item.CanBeCasted(Target) &&
                this.Config.ItemToggler.Value.IsEnabled("item_rod_of_atos"))
            {
                Log.Debug("Using RodofAtos");
                this.RodofAtos.UseAbility(Target);
                await Await.Delay(this.GetItemDelay(Target), token);
            }

            if (this.VeilofDiscord != null &&
                this.VeilofDiscord.Item.IsValid &&
                Target != null && !UnitExtensions.IsChanneling(Owner) &&
                this.VeilofDiscord.Item.CanBeCasted() &&
                this.Config.ItemToggler.Value.IsEnabled("item_veil_of_discord"))
            {
                Log.Debug("Using VeilofDiscord");
                this.VeilofDiscord.UseAbility(Target.Position);
                await Await.Delay(this.GetItemDelay(Target), token);
            }

            if (this.HurricanePike != null &&
                this.HurricanePike.Item.IsValid &&
                Target != null && !UnitExtensions.IsChanneling(Owner) &&
                this.HurricanePike.Item.CanBeCasted() &&
                this.Config.ItemToggler.Value.IsEnabled("item_hurricane_pike"))
            {
                Log.Debug("Using HurricanePike");
                this.HurricanePike.UseAbility(Target);
                await Await.Delay(this.GetItemDelay(Target), token);
            }

            if (this.ShivasGuard != null &&
                this.ShivasGuard.Item.IsValid &&
                Target != null && !UnitExtensions.IsChanneling(Owner) &&
                this.ShivasGuard.Item.CanBeCasted() &&
                Owner.Distance2D(Target) <= 900 &&
                this.Config.ItemToggler.Value.IsEnabled("item_shivas_guard"))
            {
                Log.Debug("Using Shiva's Guard");
                this.ShivasGuard.UseAbility();
                await Await.Delay((int) Game.Ping, token);
            }

            if (this.Mjollnir != null &&
                this.Mjollnir.Item.IsValid &&
                Target != null && !UnitExtensions.IsChanneling(Owner) &&
                this.Mjollnir.Item.CanBeCasted() &&
                this.Config.ItemToggler.Value.IsEnabled("item_mjollnir"))
            {
                Log.Debug("Using Mjollnir");
                this.Mjollnir.UseAbility(Owner);
                await Await.Delay(this.GetItemDelay(Target), token);
            }
            if (this.Config.AbilityToggler.Value.IsEnabled(this.Blast.Name) &&
                this.Config.AbilityToggler.Value.IsEnabled(this.Decrepify.Name))
            {
                if (!silenced && this.Drain.CanBeCasted() &&
                    !this.Blast.CanBeCasted() && !this.Decrepify.CanBeCasted()
                    && this.Config.AbilityToggler.Value.IsEnabled(this.Drain.Name)
                    && !UnitExtensions.IsChanneling(Owner)
                    && Target != null && Target.IsAlive)
                {
                    Log.Debug($"Using Drain 1");
                    this.Drain.UseAbility(Target);
                    await Await.Delay(GetAbilityDelay(Target, Drain) + 50, token);
                }
            }
            else
            {
                if (!silenced && this.Drain.CanBeCasted()
                    && this.Config.AbilityToggler.Value.IsEnabled(this.Drain.Name)
                    && !UnitExtensions.IsChanneling(Owner)
                    && Target != null && Target.IsAlive)
                {
                    Log.Debug($"Using Drain 2");
                    this.Drain.UseAbility(Target);
                    await Await.Delay(GetAbilityDelay(Target, Drain) + 50, token);
                }
            }

            if (Target != null && !Owner.IsValidOrbwalkingTarget(Target) && !UnitExtensions.IsChanneling(this.Owner))
            {
                Orbwalker.Move(Game.MousePosition);
                await Await.Delay(50, token);
            }
            else
            {
                Orbwalker.OrbwalkTo(Target);
            }

            await Await.Delay(50, token);
        }

        protected float GetSpellAmp()
        {
            // spell amp
            var me = Context.Owner as Hero;

            var spellAmp = (100.0f + me.TotalIntelligence / 15.0f) / 100.0f;

            var kaya = Owner.GetItemById(AbilityId.item_trident);
            if (kaya != null)
            {
                spellAmp += kaya.AbilitySpecialData.First(x => x.Name == "spell_amp").Value / 100.0f;
            }

            return spellAmp;
        }

        public virtual async Task KillStealAsync()
        {
            var damageBlast = Blast.GetAbilityData("blast_damage");
            damageBlast *= GetSpellAmp();

            bool comboMana = Blast.GetManaCost(Blast.Level - 1) + Decrepify.GetManaCost(Decrepify.Level - 1) <
                             Owner.Mana;

            var decrepifyKillable =
                EntityManager<Hero>.Entities.FirstOrDefault(
                    x =>
                        x.IsAlive && x.Team != this.Owner.Team && !x.IsIllusion
                        && x.Health < damageBlast * (1 - x.MagicDamageResist)
                        && Blast != null && Blast.IsValid && x.Distance2D(this.Owner) <= 900
                        && Decrepify.CanBeCasted(x) && Blast.CanBeCasted()
                        && !UnitExtensions.IsMagicImmune(x) && comboMana);

            var blastKillable =
                EntityManager<Hero>.Entities.FirstOrDefault(
                    x =>
                        x.IsAlive && x.Team != this.Owner.Team && !x.IsIllusion
                        && x.Health < damageBlast * (1 - x.MagicDamageResist)
                        && Blast.CanBeCasted() && !UnitExtensions.IsMagicImmune(x) && Blast.CanHit(x)
                        && Ensage.SDK.Extensions.EntityExtensions.Distance2D(Owner, x.NetworkPosition) <= 800);

            if (!UnitExtensions.IsChanneling(this.Owner))
            {

                if (decrepifyKillable != null)
                {
                    Decrepify.UseAbility(decrepifyKillable);
                    await Await.Delay(GetAbilityDelay(decrepifyKillable, Decrepify));

                    if (Blast.CanHit(decrepifyKillable))
                    {
                        Blast.UseAbility(decrepifyKillable);
                        await Await.Delay(GetAbilityDelay(decrepifyKillable, Blast));
                    }
                }

                if (blastKillable != null)
                {
                    Blast.UseAbility(blastKillable.NetworkPosition);
                    await Await.Delay(GetAbilityDelay(blastKillable, Blast));
                }

            }
            else if (Target != null && UnitExtensions.HasModifier(Target, "modifier_pugna_life_drain"))
            {
                if (decrepifyKillable != null)
                {
                    Decrepify.UseAbility(decrepifyKillable);
                    await Await.Delay(GetAbilityDelay(decrepifyKillable, Decrepify));

                    if (Blast.CanHit(decrepifyKillable))
                    {
                        Blast.UseAbility(decrepifyKillable);
                        await Await.Delay(GetAbilityDelay(decrepifyKillable, Blast));
                    }
                }

                if (blastKillable != null)
                {
                    Blast.UseAbility(blastKillable.NetworkPosition);
                    await Await.Delay(GetAbilityDelay(blastKillable, Blast));
                }
            }

        }

        protected int GetAbilityDelay(Unit unit, Ability ability)
        {
            return (int) (((ability.FindCastPoint() + this.Owner.GetTurnTime(unit)) * 1000.0) + Game.Ping) + 50;
        }

        protected int GetAbilityDelay(Vector3 pos, Ability ability)
        {
            return (int) (((ability.FindCastPoint() + this.Owner.GetTurnTime(pos)) * 1000.0) + Game.Ping) + 50;
        }

        protected int GetItemDelay(Unit unit)
        {
            return (int) ((this.Owner.GetTurnTime(unit) * 1000.0) + Game.Ping) + 100;
        }

        protected int GetItemDelay(Vector3 pos)
        {
            return (int) ((this.Owner.GetTurnTime(pos) * 1000.0) + Game.Ping) + 100;
        }

        private void GameDispatcher_OnIngameUpdate(EventArgs args)
        {
            if (!this.Config.KillStealEnabled.Value)
            {
                return;
            }

            if (!Game.IsPaused && Owner.IsAlive)
            {
                Await.Block("MyKillstealer", KillStealAsync);
            }
        }

        protected override void OnActivate()
        {
            GameDispatcher.OnIngameUpdate += GameDispatcher_OnIngameUpdate;
            this.Decrepify = UnitExtensions.GetAbilityById(this.Owner, AbilityId.pugna_decrepify);
            this.Blast = UnitExtensions.GetAbilityById(this.Owner, AbilityId.pugna_nether_blast);
            this.Ward = UnitExtensions.GetAbilityById(this.Owner, AbilityId.pugna_nether_ward);
            this.Drain = UnitExtensions.GetAbilityById(this.Owner, AbilityId.pugna_life_drain);

            this.Context.Inventory.Attach(this);

            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            GameDispatcher.OnIngameUpdate -= GameDispatcher_OnIngameUpdate;
            base.OnDeactivate();
            this.Context.Inventory.Detach(this);
        }
    }
}