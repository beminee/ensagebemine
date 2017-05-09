// <copyright file="Test.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

namespace HuskarSharpSDK
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel.Composition;
    using System.Linq;
    using Ensage;
    using Ensage.Common;
    using Ensage.Common.Enums;
    using Ensage.Common.Extensions;
    using Ensage.Common.Menu;
    using Ensage.SDK.Inventory;
    using Ensage.SDK.Menu;
    using Ensage.SDK.Orbwalker;
    using Ensage.SDK.TargetSelector;
    using Ensage.SDK.Service;
    using Ensage.SDK.Service.Metadata;

    [ExportPlugin("HuskarSharpSDK", HeroId.npc_dota_hero_huskar)]
    public class Test : Plugin, IOrbwalkingMode
    {
        private readonly Lazy<IOrbwalkerManager> orbwalkerManager;

        private readonly Lazy<ITargetSelectorManager> targetManager;

        [ImportingConstructor]
        public Test([Import] Lazy<IOrbwalkerManager> orbManager, [Import] Lazy<ITargetSelectorManager> targetManager, [Import] IInventoryManager inventory) // import IOrbwalker, ITargetSelectorManager and IInventoryManager
        {

            this.orbwalkerManager = orbManager;
            this.targetManager = targetManager;

            // this.BloodThorn = this.Owner.FindItem("item_bloodthorn");
            // this.Satanic = this.Owner.FindItem("item_satanic");
            // this.SolarCrest = this.Owner.FindItem("item_solar_crest");
            // this.Halberd = this.Owner.FindItem("item_heavens_halberd");


            // inventory setter
            this.Inventory = inventory;
        }
            

        public MyHeroConfig Config { get; private set; }

        private IOrbwalker Orbwalker => this.orbwalkerManager.Value.Active;

        private ITargetSelector TargetSelector => this.targetManager.Value.Active;

        public IInventoryManager Inventory { get; }

        private Item BloodThorn { get; set; }

        private Item Halberd { get; set; }

        private Ability Heal { get; set; }

        private Item Satanic { get; set; }

        private Item SolarCrest { get; set; }

        private Ability Spear { get; set; }

        private Ability Ulti { get; set; }

        public bool CanExecute => this.Config.Enabled && this.Config.Key;

        public void Execute()
        {
            var target = this.TargetSelector.GetTargets().FirstOrDefault();
            var me = ObjectManager.LocalHero;

            Ulti = me.Spellbook.SpellR;
            Heal = me.Spellbook.SpellQ;
            Spear = me.Spellbook.SpellW;

        if (!me.IsSilenced())
        {
            if (target != null && this.Config.Ulti && this.Ulti.CanBeCasted(target) && Utils.SleepCheck("HuskarSharpSDK.Ulti"))
            {
                this.Ulti.UseAbility(target);
                Utils.Sleep(100, "HuskarSharpSDK.Ulti");
            }

            // We don't want to use heal first then miss opportunity to ulti nor use heal then miss a hit. So we use heal in ulti ability phase.
            if (this.Ulti.IsInAbilityPhase && this.Config.Heal && this.Heal.CanBeCasted() && Utils.SleepCheck("HuskarSharpSDK.Heal"))
            {
                this.Heal.UseAbility(me);
                Utils.Sleep(100, "HuskarSharpSDK.Heal");
            }
        }
            // Toggle on if comboing and target is not null
            if (this.CanExecute && this.Config.Spear && this.Spear.CanBeCasted(target) && !this.Spear.IsAutoCastEnabled && Utils.SleepCheck("HuskarSharpSDK.Spear"))
            {
                this.Spear.ToggleAutocastAbility();
                Utils.Sleep(150, "HuskarSharpSDK.Spear");
            }
            // Toggle off if target is null
            else if (this.Config.Spear && target == null && this.Spear.IsAutoCastEnabled && Utils.SleepCheck("HuskarSharpSDK.Spear2"))
            {
                this.Spear.ToggleAutocastAbility();
                Utils.Sleep(150, "HuskarSharpSDK.Spear2");
            }

            if (this.BloodThorn != null && this.BloodThorn.IsValid && target != null && this.BloodThorn.CanBeCasted(target) && Utils.SleepCheck("HuskarSharpSDK.BT")
                && this.Config.MenuValue.IsEnabled(this.BloodThorn.Name))
            {
                this.BloodThorn.UseAbility(target);
                Utils.Sleep(150, "HuskarSharpSDK.BT");
            }

            if (this.Satanic != null && this.Satanic.IsValid && target != null && me.Health / me.MaximumHealth <= 0.2 && this.Satanic.CanBeCasted()
                && Utils.SleepCheck("HuskarSharpSDK.Satanic") && this.Config.MenuValue.IsEnabled(this.Satanic.Name))
            {
                this.Satanic.UseAbility();
                Utils.Sleep(150, "HuskarSharpSDK.Satanic");
            }

            if (this.SolarCrest != null && this.SolarCrest.IsValid && target != null && this.SolarCrest.CanBeCasted(target) && Utils.SleepCheck("HuskarSharpSDK.SolarCrest")
                && this.Config.MenuValue.IsEnabled(this.SolarCrest.Name))
            {
                this.SolarCrest.UseAbility(target);
                Utils.Sleep(150, "HuskarSharpSDK.SolarCrest");
            }

            if (this.Halberd != null && this.Halberd.IsValid && target != null && this.Halberd.CanBeCasted(target) && Utils.SleepCheck("HuskarSharpSDK.Halberd")
                && this.Config.MenuValue.IsEnabled(this.Halberd.Name))
            {
                this.Halberd.UseAbility(target);
                Utils.Sleep(150, "HuskarSharpSDK.Halberd");
            }

            if (this.Orbwalker.OrbwalkTo(target))
            {
                // do nothing (?)
            }

        }

        protected override void OnActivate()
        {
            this.Config = new MyHeroConfig(); // create menus

            if (!this.Config.TogglerSet)
            {
                this.Config.MenuValue = this.Config.Toggler.Value;
                this.Config.TogglerSet = true;
            }

            this.Orbwalker.RegisterMode(this);
            this.Inventory.CollectionChanged += this.OnInventoryChanged; // sub to inventory changed
        }

        protected override void OnDeactivate()
        {
            this.Orbwalker.UnregisterMode(this);
            this.Config.Dispose();
            this.Inventory.CollectionChanged -= this.OnInventoryChanged;
        }

        private void OnInventoryChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in args.NewItems.OfType<InventoryItem>())
                {
                    // new items
                    switch (item.Id)
                    {
                        case ItemId.item_bloodthorn:
                            this.BloodThorn = item.Item;
                            break;

                        case ItemId.item_satanic:
                            this.Satanic = item.Item;
                            break;

                        case ItemId.item_solar_crest:
                            this.SolarCrest = item.Item;
                            break;

                        case ItemId.item_heavens_halberd:
                            this.Halberd = item.Item;
                            break;
                    }
                }
            }

            if (args.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in args.OldItems.OfType<InventoryItem>())
                {
                    // removed items
                    switch (item.Id)
                    {
                        case ItemId.item_bloodthorn:
                            this.BloodThorn = null;
                            break;

                        case ItemId.item_satanic:
                            this.Satanic = null;
                            break;

                        case ItemId.item_solar_crest:
                            this.SolarCrest = null;
                            break;

                        case ItemId.item_heavens_halberd:
                            this.Halberd = null;
                            break;
                    }
                }
            }
        }
    }

    public class MyHeroConfig : IDisposable
    {
        public AbilityToggler MenuValue;

        public bool TogglerSet = false;

        public MyHeroConfig()
        {
            var dict = new Dictionary<string, bool>
                           {
                               { "item_bloodthorn", true },
                               { "item_satanic", true },
                               { "item_solar_crest", true },
                               { "item_heavens_halberd", true }
                           };

            this.Menu = MenuFactory.Create("HuskarSharpSDK");
            this.Enabled = this.Menu.Item("Enabled?", true);
            this.Key = this.Menu.Item("Combo Key", new KeyBind(32, KeyBindType.Press));
            this.Heal = this.Menu.Item("Use Heal?", true);
            this.Ulti = this.Menu.Item("Use Ulti?", true);
            this.Spear = this.Menu.Item("Activate Spear on Combo?", true);
            this.Toggler = this.Menu.Item("Items to use", new AbilityToggler(dict));
        }

        public void Dispose()
        {
            this.Menu?.Dispose();
        }

        public MenuFactory Menu { get; }

        public MenuItem<bool> Enabled { get; }

        public MenuItem<bool> Heal { get; }

        public MenuItem<KeyBind> Key { get; }

        public MenuItem<bool> Spear { get; }

        public MenuItem<AbilityToggler> Toggler { get; }

        public MenuItem<bool> Ulti { get; }
    }
}