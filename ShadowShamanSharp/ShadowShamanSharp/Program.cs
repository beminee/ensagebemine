namespace ShadowShamanSharp
{
    using System.ComponentModel.Composition;
    using System.Windows.Input;
    using Ensage;
    using Ensage.Common.Menu;
    using Ensage.SDK.Service;
    using Ensage.SDK.Service.Metadata;

    [ExportPlugin("ShadowShamanSharp", StartupMode.Auto, "beminee", "1.0.0.0", "Fully Functional Shadow-Shaman Assembly", 500,
        HeroId.npc_dota_hero_shadow_shaman)]
    public class Program : Plugin
    {
        private readonly IServiceContext context;

        [ImportingConstructor]
        public Program(
            [Import] IServiceContext context)
        {
            this.context = context;
        }

        public ShamanConfig Config { get; private set; }

        public ShadowShamanSharp OrbwalkerMode { get; private set; }

        protected override void OnActivate()
        {
            this.Config = new ShamanConfig();
            this.Config.Key.Item.ValueChanged += this.HotkeyChanged;

            this.OrbwalkerMode = new ShadowShamanSharp(this.Config.Key, this.Config, this.context);

            this.context.Orbwalker.RegisterMode(this.OrbwalkerMode);
        }

        protected override void OnDeactivate()
        {
            this.context.Orbwalker.UnregisterMode(this.OrbwalkerMode);
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