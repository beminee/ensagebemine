namespace HuskarSharpSDK 
{
    using System.ComponentModel.Composition;
    using System.Windows.Input;
    using Ensage;
    using Ensage.Common;
    using Ensage.Common.Extensions;
    using Ensage.SDK.Menu;
    using Ensage.SDK.Orbwalker;
    using Ensage.SDK.Orbwalker.Metadata;
    using Ensage.SDK.Orbwalker.Modes;
    using Ensage.SDK.TargetSelector;

    [ExportOrbwalkingMode]
   public class Test : AutoAttackMode
    {
        [ImportingConstructor]
        public Test(IOrbwalker orbwalker, ITargetSelectorManager targetSelector)
            : base(orbwalker, targetSelector)
        {
            this.Config = new MyHeroConfig();
            this.Ulti = this.Owner.Spellbook.SpellR;
            this.Heal = this.Owner.Spellbook.SpellQ;
            this.Spear = this.Owner.Spellbook.SpellW;
            this.BloodThorn = this.Owner.FindItem("item_bloodthorn");
            this.Satanic = this.Owner.FindItem("item_satanic");
            this.SolarCrest = this.Owner.FindItem("item_solar_crest");
            this.Halberd = this.Owner.FindItem("item_heavens_halberd");
        }

        public override bool CanExecute => Game.IsKeyDown(Key.Space);

        public MyHeroConfig Config { get; }
        private Ability Ulti { get; }
        private Ability Spear { get; }
        private Ability Heal { get; }
        private Item BloodThorn { get; }
        private Item Satanic { get; }
        private Item SolarCrest { get; }
        private Item Halberd { get; }

        public override void Execute()
        {

            var target = this.GetTarget();

            if (target != null && this.Config.Ulti && this.Ulti.CanBeCasted(target) && Utils.SleepCheck("HuskarSharpSDK.Ulti"))
            {
                this.Ulti.UseAbility(target);
                Utils.Sleep(100, "HuskarSharpSDK.Ulti");
            }

            if (this.Ulti.IsInAbilityPhase && this.Config.Heal && this.Heal.CanBeCasted() && Utils.SleepCheck("HuskarSharpSDK.Heal"))
            {
                this.Heal.UseAbility(Owner);
                Utils.Sleep(100, "HuskarSharpSDK.Heal");
            }

            if (this.CanExecute && this.Config.Spear && this.Spear.CanBeCasted(target) && !this.Spear.IsAutoCastEnabled && Utils.SleepCheck("HuskarSharpSDK.Spear"))
            {
                this.Spear.ToggleAutocastAbility();
                Utils.Sleep(150, "HuskarSharpSDK.Spear");
            }
            else if (this.Config.Spear && target == null && this.Spear.IsAutoCastEnabled && Utils.SleepCheck("HuskarSharpSDK.Spear2"))
            {
                this.Spear.ToggleAutocastAbility();
                Utils.Sleep(150, "HuskarSharpSDK.Spear2");
            }

            if (target != null && this.BloodThorn.CanBeCasted(target) && Utils.SleepCheck("HuskarSharpSDK.BT"))
            {
                this.BloodThorn.UseAbility(target);
                Utils.Sleep(150, "HuskarSharpSDK.BT");
            }

            if (target != null && Owner.MaximumHealth / Owner.Health <= 0.2 && this.Satanic.CanBeCasted() && Utils.SleepCheck("HuskarSharpSDK.Satanic"))
            {
                this.Satanic.UseAbility();
                Utils.Sleep(150, "HuskarSharpSDK.Satanic");
            }

            if (target != null && this.SolarCrest.CanBeCasted(target) && Utils.SleepCheck("HuskarSharpSDK.SolarCrest"))
            {
                this.SolarCrest.UseAbility(target);
                Utils.Sleep(150, "HuskarSharpSDK.SolarCrest");
            }

            if (target != null && this.Halberd.CanBeCasted(target) && Utils.SleepCheck("HuskarSharpSDK.Halberd"))
            {
                this.Halberd.UseAbility(target);
                Utils.Sleep(150, "HuskarSharpSDK.Halberd");
            }

            base.Execute();
        }

    }

    public class MyHeroConfig
    {
        public MyHeroConfig()
        {
            this.Factory = MenuFactory.Create("HuskarSharpSDK");
            this.Heal = this.Factory.Item("Use Heal?", true);
            this.Ulti = this.Factory.Item("Use Ulti?", true);
            this.Spear = this.Factory.Item("Activate Spear on Combo?", true);
        }

        public MenuFactory Factory { get; }
        public MenuItem<bool> Heal { get; }
        public MenuItem<bool> Ulti { get; }
        public MenuItem<bool> Spear { get; }
    }
}
