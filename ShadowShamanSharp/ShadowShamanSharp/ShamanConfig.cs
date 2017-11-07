namespace ShadowShamanSharp
{
    using System;
    using System.Collections.Generic;
    using Ensage.Common.Menu;
    using Ensage.SDK.Menu;

    public class ShamanConfig
    {
        private bool _disposed;

        public ShamanConfig()
        {

            var itemDict = new Dictionary<string, bool>
            {
                {"item_bloodthorn", true},
                {"item_sheepstick", true},
                {"item_shivas_guard", true},
                {"item_dagon_5", true},
                {"item_hurricane_pike", true},
                {"item_blink", true},
                {"item_orchid", true},
                {"item_rod_of_atos", true},
                {"item_veil_of_discord", true},
                {"item_mjollnir", true}
            };

            var spellDict = new Dictionary<string, bool>
            {
                {"shadow_shaman_ether_shock", true},
                {"shadow_shaman_voodoo", true},
                {"shadow_shaman_shackles", true},
                {"shadow_shaman_mass_serpent_ward", true}
            };

            this.Menu = MenuFactory.Create("ShadowShamanSharp");
            this.Key = this.Menu.Item("Combo Key", new KeyBind(32));
            this.Key.Item.Tooltip = "Hold this key to start combo mode.";
            this.KillStealEnabled = this.Menu.Item("Killsteal toggle", true);
            this.KillStealEnabled.Item.Tooltip = "Setting this to false will disable killsteal.";
            this.UseBlinkPrediction = this.Menu.Item("Blink Prediction", new Slider(200, 0, 600));
            this.UseBlinkPrediction.Item.Tooltip = "Will blink to set distance. Set to 0 if you want to disable it.";
            this.AbilityToggler = this.Menu.Item("Ability Toggler", new AbilityToggler(spellDict));
            this.ItemToggler = this.Menu.Item("Item Toggler", new AbilityToggler(itemDict));
        }

        public MenuFactory Menu { get; }

        public MenuItem<bool> KillStealEnabled { get; }

        public MenuItem<Slider> UseBlinkPrediction { get; }

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