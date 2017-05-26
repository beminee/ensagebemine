// <copyright file="Program.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>


namespace ODSharpSDK
{
    using System;
    using System.ComponentModel.Composition;
    using System.Windows.Input;

    using Ensage;
    using Ensage.Common.Menu;
    using Ensage.SDK.Input;
    using Ensage.SDK.Inventory;
    using Ensage.SDK.Orbwalker;
    using Ensage.SDK.TargetSelector;
    using Ensage.SDK.Service;
    using Ensage.SDK.Service.Metadata;
    using Ensage.SDK.Prediction;

    [ExportPlugin("ODSharpSDK", HeroId.npc_dota_hero_obsidian_destroyer)]
    public class Program : Plugin
    {
        private readonly Lazy<IInputManager> input;

        private readonly Lazy<IInventoryManager> inventoryMgr;

        private readonly Lazy<IOrbwalkerManager> orbwalkerManager;

        private readonly Lazy<ITargetSelectorManager> targetManager;

        private IPrediction Prediction { get; }


        private OdSharpConfig Config;

        [ImportingConstructor]
        public Program([Import] Lazy<IInventoryManager> inventoryMgr, [Import] Lazy<IInputManager> input, [Import] Lazy<IOrbwalkerManager> orbwalkerManager, 
            [Import] Lazy<ITargetSelectorManager> targetManager, [Import] IPrediction prediction)
        {
            this.inventoryMgr = inventoryMgr;
            this.input = input;
            this.orbwalkerManager = orbwalkerManager;
            this.Prediction = prediction;
            this.targetManager = targetManager;
        }

        public ODSharp OrbwalkerMode { get; private set; }

        private void HotkeyChanged(object sender, OnValueChangeEventArgs e)
        {
            var keyCode = e.GetNewValue<KeyBind>().Key;
            if (keyCode == e.GetOldValue<KeyBind>().Key)
            {
                return;
            }

            var key = KeyInterop.KeyFromVirtualKey((int)keyCode);
            this.OrbwalkerMode.Key = key;
        }

        protected override void OnActivate()
        {
            this.Config = new OdSharpConfig();

            var key = KeyInterop.KeyFromVirtualKey((int)this.Config.Key.Value.Key);
            this.OrbwalkerMode = new ODSharp(this.orbwalkerManager.Value, this.input.Value, key, this.Config, this.inventoryMgr.Value, this.targetManager.Value.Active, this.Prediction);

            this.Config.Key.Item.ValueChanged += this.HotkeyChanged;

            this.orbwalkerManager.Value.RegisterMode(this.OrbwalkerMode);
        }

        protected override void OnDeactivate()
        {
            this.orbwalkerManager.Value.UnregisterMode(this.OrbwalkerMode);

            this.Config.Key.Item.ValueChanged -= this.HotkeyChanged;

            this.Config?.Dispose();
        }
    }
}