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
    using Ensage.SDK.Prediction;
    using Ensage.SDK.Service;
    using Ensage.SDK.Service.Metadata;
    using Ensage.SDK.TargetSelector;

    [ExportPlugin("ODSharpSDK", HeroId.npc_dota_hero_obsidian_destroyer)]
    public class Program : Plugin
    {
        private readonly Lazy<IInputManager> input;

        private readonly Lazy<IInventoryManager> inventoryManager;

        private readonly Lazy<IOrbwalkerManager> orbwalkerManager;

        private readonly Lazy<IPrediction> prediction;

        private readonly Lazy<ITargetSelectorManager> targetManager;

        [ImportingConstructor]
        public Program(
            [Import] IServiceContext context)
        {
            this.inventoryManager = context.Inventory;
            this.input = context.Input;
            this.orbwalkerManager = context.Orbwalker;
            this.prediction = Context.Prediction;
            this.targetManager = context.TargetSelector;
        }

        public OdSharpConfig Config { get; private set; }

        public ODSharp OrbwalkerMode { get; private set; }

        protected override void OnActivate()
        {
            this.Config = new OdSharpConfig();
            this.Config.Key.Item.ValueChanged += this.HotkeyChanged;

            this.OrbwalkerMode = new ODSharp(
                KeyInterop.KeyFromVirtualKey((int)this.Config.Key.Value.Key),
                this.Config,
                this.orbwalkerManager,
                this.input,
                this.inventoryManager,
                this.targetManager,
                this.prediction);

            this.orbwalkerManager.Value.RegisterMode(this.OrbwalkerMode);
        }

        protected override void OnDeactivate()
        {
            this.orbwalkerManager.Value.UnregisterMode(this.OrbwalkerMode);
            this.Config.Key.Item.ValueChanged -= this.HotkeyChanged;
            this.Config.Dispose();
        }

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
    }
}