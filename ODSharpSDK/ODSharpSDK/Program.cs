// <copyright file="Program.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

namespace ODSharpSDK
{
    using System;
    using System.ComponentModel.Composition;
    using System.Windows.Input;
    using System.Reflection;

    using Ensage;
    using Ensage.Common.Menu;
    using Ensage.Common;
    using Ensage.Common.Extensions;
    using Ensage.SDK.Input;
    using Ensage.SDK.Inventory;
    using Ensage.SDK.Inventory.Metadata;
    using Ensage.SDK.Orbwalker;
    using Ensage.SDK.Prediction;
    using Ensage.SDK.Service;
    using Ensage.SDK.Service.Metadata;
    using Ensage.SDK.TargetSelector;
    using Ensage.SDK.Helpers;
    using System.Linq;

    using SharpDX;
    using log4net;
    using PlaySharp.Toolkit.Logging;

    [ExportPlugin("ODSharpSDK", HeroId.npc_dota_hero_obsidian_destroyer)]
    public class Program : Plugin
    {

        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IServiceContext context;

        [ImportingConstructor]
        public Program([Import] IServiceContext context)
        {
            this.context = context;
        }

        public OdSharpConfig Config { get; private set; }

        public ODSharp OrbwalkerMode { get; private set; }

        private float scaleX, scaleY;


        protected override void OnActivate()
        {
            this.Config = new OdSharpConfig();
            var key = KeyInterop.KeyFromVirtualKey((int)this.Config.Key.Value.Key);
            this.Config.Key.Item.ValueChanged += this.HotkeyChanged;

            this.OrbwalkerMode = new ODSharp(key, this.Config, this.context);

            scaleX = ((float)Drawing.Width / 1920);
            scaleY = ((float)Drawing.Height / 1080);

            this.context.Orbwalker.RegisterMode(this.OrbwalkerMode);

            Drawing.OnDraw += Drawing_OnDraw;
        }

        protected override void OnDeactivate()
        {
            this.context.Orbwalker.UnregisterMode(this.OrbwalkerMode);
            this.Config.Key.Item.ValueChanged -= this.HotkeyChanged;
            Drawing.OnDraw -= Drawing_OnDraw;
            this.Config.Dispose();
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!Config.Drawings)
            {
                return;
            }

            var enemies =
                EntityManager<Hero>.Entities.Where(
                                       x =>
                                           x.IsValid &&
                                           x.UnitState != UnitState.MagicImmune &&
                                           x.IsAlive &&
                                           !x.IsIllusion &&
                                           x.Team != this.context.Owner.Team).ToList();

            if (enemies.Any())
            {
                return;
            }

            foreach (var enemy in enemies)
            {
                var enemyHealth = enemy.Health;
                var enemyMaxHealth = enemy.MaximumHealth;

                var me = this.context.Owner as Hero;

                var meInt = Math.Floor(me.TotalIntelligence);
                var enemyInt = Math.Floor(enemy.TotalIntelligence);
                var ultiMultiplier = me.Spellbook.SpellR.GetAbilityData("damage_multiplier");

                var damage = Math.Floor((ultiMultiplier * (meInt - enemyInt)) * (1 - enemy.MagicDamageResist));

                Vector2 hbarpos;
                hbarpos = HUDInfo.GetHPbarPosition(enemy);

                Vector2 screenPos;
                var enemyPos = enemy.Position + new Vector3(0, 0, enemy.HealthBarOffset);
                if (!Drawing.WorldToScreen(enemyPos, out screenPos)) continue;

                var start = screenPos;

                hbarpos.X = start.X + (HUDInfo.GetHPBarSizeX(enemy) / 2);
                hbarpos.Y = start.Y;
                float a = (float)Math.Round((damage * HUDInfo.GetHPBarSizeX(enemy)) / (enemy.MaximumHealth));
                var position = hbarpos - new Vector2(a, 32 * scaleY);

                try
                {
                    float left = (float)Math.Round(damage / 7);
                    Drawing.DrawRect(
                        position,
                        new Vector2(a, (float)(HUDInfo.GetHpBarSizeY(enemy))),
                        (enemy.Health > 0) ? new Color(150, 225, 150, 80) : new Color(70, 225, 150, 225));

                    Drawing.DrawRect(position, new Vector2(a, (float)(HUDInfo.GetHpBarSizeY(enemy))), Color.Black, true);
                }
                catch (Exception e)
                {
                    Log.Debug($"{e}");
                }
            }
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