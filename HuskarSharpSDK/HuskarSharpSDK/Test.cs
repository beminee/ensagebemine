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
    using System.Collections.Generic;
    using Ensage.Common.Menu;

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

            if (!Config.TogglerSet)
            {
                Config.menuValue = this.Config.Toggler.Value;
                Config.TogglerSet = true;
            }
        }

        public override bool CanExecute => Config.Key;

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
            // We don't want to use heal first then miss opportunity to ulti nor use heal then miss a hit. So we use heal in ulti ability phase.
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

            if (target != null && this.BloodThorn.CanBeCasted(target) && Utils.SleepCheck("HuskarSharpSDK.BT") && Config.menuValue.IsEnabled(this.BloodThorn.Name))
            {
                this.BloodThorn.UseAbility(target);
                Utils.Sleep(150, "HuskarSharpSDK.BT");
            }

            if (target != null && Owner.Health / Owner.MaximumHealth <= 0.2 && this.Satanic.CanBeCasted() && Utils.SleepCheck("HuskarSharpSDK.Satanic") && Config.menuValue.IsEnabled(this.Satanic.Name))
            {
                this.Satanic.UseAbility();
                Utils.Sleep(150, "HuskarSharpSDK.Satanic");
            }

            if (target != null && this.SolarCrest.CanBeCasted(target) && Utils.SleepCheck("HuskarSharpSDK.SolarCrest") && Config.menuValue.IsEnabled(this.SolarCrest.Name))
            {
                this.SolarCrest.UseAbility(target);
                Utils.Sleep(150, "HuskarSharpSDK.SolarCrest");
            }

            if (target != null && this.Halberd.CanBeCasted(target) && Utils.SleepCheck("HuskarSharpSDK.Halberd") && Config.menuValue.IsEnabled(this.Halberd.Name))
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
            var dict = new Dictionary<string, bool>
            {
                { "item_bloodthorn", true },
                { "item_satanic", true },
                { "item_solar_crest", true },
                { "item_heavens_halberd", true }
            };

            this.Factory = MenuFactory.Create("HuskarSharpSDK");
            this.Key = this.Factory.Item("Combo Key", new KeyBind(32, KeyBindType.Press));
            this.Heal = this.Factory.Item("Use Heal?", true);
            this.Ulti = this.Factory.Item("Use Ulti?", true);
            this.Spear = this.Factory.Item("Activate Spear on Combo?", true);
            this.Toggler = this.Factory.Item("Items to use", new AbilityToggler(dict));
        }

        public MenuFactory Factory { get; }
        public MenuItem<KeyBind> Key { get; }
        public MenuItem<bool> Heal { get; }
        public MenuItem<bool> Ulti { get; }
        public MenuItem<bool> Spear { get; }
        public MenuItem<AbilityToggler> Toggler { get; }
        public bool TogglerSet = false;
        public AbilityToggler menuValue;
    }
}
