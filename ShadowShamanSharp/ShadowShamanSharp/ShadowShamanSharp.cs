namespace ShadowShamanSharp
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Ensage;
    using Ensage.Common.Extensions;
    using Ensage.Common.Menu;
    using Ensage.Common.Threading;
    using Ensage.SDK.Extensions;
    using Ensage.SDK.Helpers;
    using Ensage.SDK.Inventory.Metadata;
    using Ensage.SDK.Orbwalker.Modes;
    using Ensage.SDK.TargetSelector;
    using Ensage.SDK.Abilities.Items;
    using Ensage.SDK.Abilities.Aggregation;
    using Ensage.SDK.Service;
    using log4net;
    using PlaySharp.Toolkit.Logging;
    using PlaySharp.Toolkit.Helper.Annotations;
    using SharpDX;
    using AbilityId = Ensage.AbilityId;
    using UnitExtensions = Ensage.SDK.Extensions.UnitExtensions;

    [PublicAPI]
    public class ShadowShamanSharp : KeyPressOrbwalkingModeAsync
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IServiceContext context;

        private ITargetSelectorManager TargetSelector { get; }

        public ShamanConfig Config { get; }

        public ShadowShamanSharp(
            Key key,
            ShamanConfig config,
            IServiceContext context)
            : base(context, key)
        {
            this.Config = config;
            this.context = context;
            this.TargetSelector = context.TargetSelector;
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


        private Ability Ethershock { get; set; }

        private Ability Hex { get; set; }

        private Ability Shackles { get; set; }

        private Ability Wards { get; set; }

        private Unit Target { get; set; }

        public override async Task ExecuteAsync(CancellationToken token)
        {
            Target = this.TargetSelector.Active.GetTargets().FirstOrDefault(x => !x.IsInvulnerable());

            var silenced = UnitExtensions.IsSilenced(this.Owner);

            var sliderValue = this.Config.UseBlinkPrediction.Item.GetValue<Slider>().Value;

            if (!silenced)
            {
                try
                {
                    if ((this.BlinkDagger != null) &&
                        (this.BlinkDagger.Item.IsValid) &&
                        Target != null && Owner.Distance2D(Target) <= 1200 + sliderValue &&
                        !(Owner.Distance2D(Target) <= 400) &&
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

                    if (Hex != null && Hex.IsValid && Hex.CanBeCasted(Target) &&
                        !UnitExtensions.IsChanneling(this.Owner) &&
                        this.Config.AbilityToggler.Value.IsEnabled(this.Hex.Name))
                    {
                        Log.Debug($"Using Hex");
                        Hex.UseAbility(Target);
                        await Await.Delay(GetAbilityDelay(this.Owner, Hex), token);
                    }

                    if (Ethershock != null && Ethershock.IsValid && Ethershock.CanBeCasted(Target) &&
                        !UnitExtensions.IsChanneling(this.Owner) &&
                        this.Config.AbilityToggler.Value.IsEnabled(this.Ethershock.Name))
                    {
                        Log.Debug($"Using Ethershock!");
                        Ethershock.UseAbility(Target);
                        await Await.Delay(GetAbilityDelay(this.Owner, Ethershock), token);
                    }
                }
                catch (TaskCanceledException)
                {
                    // ignore
                }
                catch (Exception e)
                {
                    Log.Error($"{e}");
                }
            }

            if (!silenced && Target != null)
            {
                try
                {
                    if (Wards != null && Wards.IsValid && Wards.CanBeCasted() &&
                        !UnitExtensions.IsChanneling(this.Owner) &&
                        this.Config.AbilityToggler.Value.IsEnabled(this.Wards.Name))
                    {
                        if (!Target.IsMoving || Target.MovementSpeed <= 150)
                        {
                            Log.Debug($"Using Wards");
                            Wards.UseAbility(Ensage.Common.Prediction.InFront(Target, 10));
                            await Await.Delay(GetAbilityDelay(this.Owner, Wards), token);
                        }
                        else
                        {
                            var targetSpeed = Target.MovementSpeed;
                            var pos = targetSpeed * (Wards.GetCastPoint() + (Game.Ping / 1000));
                            Log.Debug($"Predicting Wards");
                            Wards.UseAbility(Ensage.Common.Prediction.InFront(Target, pos));
                            await Await.Delay(GetAbilityDelay(this.Owner, Wards), token);
                        }
                    }
                }
                catch (TaskCanceledException)
                {
                    // ignored
                }
                catch (Exception e)
                {
                    Log.Error($"{e}");
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
                this.Config.ItemToggler.Value.IsEnabled(Dagon5.Item.Name))
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

            if (Shackles != null && Shackles.IsValid && Shackles.CanBeCasted(Target) &&
                !UnitExtensions.IsChanneling(this.Owner) &&
                this.Config.AbilityToggler.Value.IsEnabled(this.Shackles.Name))
            {
                Log.Debug($"Using Shackles!");
                Shackles.UseAbility(Target);
                await Await.Delay(GetAbilityDelay(this.Owner, Shackles) + 500, token);
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
            var damageEtherShock = Ethershock.GetAbilityData("damage");
            var talent = UnitExtensions.GetAbilityById(this.Owner, AbilityId.special_bonus_unique_shadow_shaman_3);
            if (talent?.Level > 0)
            {
                damageEtherShock += talent.GetAbilitySpecialData("value");
            }

            damageEtherShock *= GetSpellAmp();

            var killstealTarget =
                EntityManager<Hero>.Entities.FirstOrDefault(
                    x =>
                        x.IsAlive && x.Team != this.Owner.Team && !x.IsIllusion
                        && x.Health < damageEtherShock * (1 - x.MagicDamageResist)
                        && Ethershock.CanBeCasted() && !UnitExtensions.IsMagicImmune(x) &&
                        Ensage.SDK.Extensions.EntityExtensions.Distance2D(Owner, x.NetworkPosition) <=
                        Ethershock.CastRange);

            if (killstealTarget != null)
            {
                Ethershock.UseAbility(killstealTarget);
                await Await.Delay(GetAbilityDelay(killstealTarget, Ethershock));
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
            this.Ethershock = UnitExtensions.GetAbilityById(this.Owner, AbilityId.shadow_shaman_ether_shock);
            this.Hex = UnitExtensions.GetAbilityById(this.Owner, AbilityId.shadow_shaman_voodoo);
            this.Shackles = UnitExtensions.GetAbilityById(this.Owner, AbilityId.shadow_shaman_shackles);
            this.Wards = UnitExtensions.GetAbilityById(this.Owner, AbilityId.shadow_shaman_mass_serpent_ward);

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