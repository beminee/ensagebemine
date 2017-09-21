namespace PugnaSharpSDK
{
    using System;
    using System.Collections.Generic;

    using Ensage.Common.Menu;
    using Ensage.SDK.Menu;

    public class PugnaSharpConfig
    {
        private bool _disposed;

        public PugnaSharpConfig()
        {
            var itemDict = new Dictionary<string, bool>
                           {
                               { "item_bloodthorn", true },
                               { "item_sheepstick", true },
                               { "item_shivas_guard", true },
                               { "item_dagon_5", true },
                               { "item_hurricane_pike", true },
                               { "item_blink", true },
                               { "item_orchid", true },
                               { "item_rod_of_atos", true },
                               { "item_veil_of_discord", true },
                               { "item_mjollnir", true }
                           };

            var spellDict = new Dictionary<string, bool>
                           {
                               { "pugna_decrepify", true },
                               { "pugna_nether_blast", true },
                               { "pugna_nether_ward", true },
                               { "pugna_life_drain", true }
                           };

            this.Menu = MenuFactory.Create("PugnaSharpSDK");
            this.Key = this.Menu.Item("Combo Key", new KeyBind(32));
            this.Key.Item.Tooltip = "Hold this key to start combo mode.";
            this.KillStealEnabled = this.Menu.Item("Killsteal toggle", true);
            this.KillStealEnabled.Item.Tooltip = "Setting this to false will disable killsteal.";
            this.UseBlinkPrediction = this.Menu.Item("Blink Prediction", new Slider(200, 0, 600));
            this.UseBlinkPrediction.Item.Tooltip = "Will blink to set distance. Set to 0 if you want to disable it.";
            this.DrainHP = this.Menu.Item("Heal ally HP", new Slider(30, 0, 100));
            this.DrainHP.Item.Tooltip = "Allies HP to begin healing";
            this.SelfHPDrain = this.Menu.Item("Min HP to Heal", new Slider(60, 0, 100));
            this.SelfHPDrain.Item.Tooltip = "HP threshold to start healing";
            this.PostDrainHP = this.Menu.Item("Post drain HP", new Slider(30, 0, 100));
            this.PostDrainHP.Item.Tooltip = "HP threshold to stop healing. (this value must be higher than post drain HP)";
            this.HealAllyTo = this.Menu.Item("Post drain HP for ally", new Slider(100, 0, 100));
            this.HealAllyTo.Item.Tooltip = "Heal ally to this hp (this value must be higher than heal ally HP)";
            this.WardTargets = this.Menu.Item("Targets for ward", new Slider(0, 1, 5));
            this.WardTargets.Item.Tooltip = "Targets in range of ward for usage";
            this.AbilityToggler = this.Menu.Item("Ability Toggler", new AbilityToggler(spellDict));
            this.ItemToggler = this.Menu.Item("Item Toggler", new AbilityToggler(itemDict));
        }

        public MenuFactory Menu { get; }

        public MenuItem<bool> KillStealEnabled { get; }

        public MenuItem<Slider> UseBlinkPrediction { get; }

        public MenuItem<AbilityToggler> AbilityToggler { get; }

        public MenuItem<KeyBind> Key { get; }

        public MenuItem<AbilityToggler> ItemToggler { get; }

        public MenuItem<Slider> DrainHP { get; }

        public MenuItem<Slider> WardTargets { get; }

        public MenuItem<Slider> SelfHPDrain{ get; }

        public MenuItem<Slider> PostDrainHP { get; }

        public MenuItem<Slider> HealAllyTo { get; }

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