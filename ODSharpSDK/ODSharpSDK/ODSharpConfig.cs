// <copyright file="OdSharpConfig.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

namespace ODSharpSDK
{
    using System;
    using System.Collections.Generic;

    using Ensage.Common.Menu;
    using Ensage.SDK.Menu;

    public class OdSharpConfig
    {
        private bool _disposed;

        public OdSharpConfig()
        {
            var itemDict = new Dictionary<string, bool>
                           {
                               { "item_bloodthorn", true },
                               { "item_sheepstick", true },
                               { "item_shivas_guard", true },
                               { "item_hurricane_pike", true },
                               { "item_blink", true },
                               { "item_orchid", true },
                               { "item_rod_of_atos", true },
                               { "item_veil_of_discord", true },
                           };

            var spellDict = new Dictionary<string, bool>
                           {
                               { "obsidian_destroyer_arcane_orb", true },
                               { "obsidian_destroyer_astral_imprisonment", true },
                               { "obsidian_destroyer_sanity_eclipse", true }
                           };

            this.Menu = MenuFactory.Create("ODSharpSDK");
            this.Key = this.Menu.Item("Combo Key", new KeyBind(32));
            this.Key.Item.Tooltip = "Hold this key to start combo mode.";
            this.KillStealEnabled = this.Menu.Item("Killsteal toggle", true);
            this.KillStealEnabled.Item.Tooltip = "Setting this to false will disable killsteal.";
            this.UseBlinkPrediction = this.Menu.Item("Blink Prediction" ,new Slider(0, 200, 600));
            this.UseBlinkPrediction.Item.Tooltip = "Will blink to set distance. Set to 0 if you want to disable it.";
            this.MinimumTargetToUlti = this.Menu.Item("Ulti Target Count", new Slider(1, 1, 5));
            this.MinimumTargetToUlti.Item.Tooltip =
                "Minimum required enemy heroes to cast ulti. Atleast 1 of them should die too.";
            this.AbilityToggler = this.Menu.Item("Ability Toggler", new AbilityToggler(spellDict));
            this.ItemToggler = this.Menu.Item("Item Toggler", new AbilityToggler(itemDict));
        }

        public MenuFactory Menu { get; }

        public MenuItem<bool> KillStealEnabled { get; }

        public MenuItem<Slider> UseBlinkPrediction { get; }

        public MenuItem<Slider> MinimumTargetToUlti { get; }

        public MenuItem<PriorityChanger> DisablePriority { get; set; }

        public MenuItem<AbilityToggler> AbilityToggler { get; }

        public MenuItem<KeyBind> Key { get; }

        public MenuItem<AbilityToggler> ItemToggler { get; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this._disposed)
            {
                return;
            }

            if (disposing)
            {
                this.Menu.Dispose();
            }

            this._disposed = true;
        }
    }
}