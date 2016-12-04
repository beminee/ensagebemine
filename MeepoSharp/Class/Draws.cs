using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common;
using SharpDX;

namespace MeepoSharp.Class
{
    class Draws
    {
        public static void DrawUltiDamage(EventArgs args)
        {
            if (!Game.IsInGame || Game.IsPaused || Game.IsWatchingGame)
            {
                return;
            }

            var me = ObjectMgr.LocalHero;
            if (me == null || me.ClassID != ClassID.CDOTA_Unit_Hero_Obsidian_Destroyer) return;

            var ultLvl = me.Spellbook.SpellR.Level;
            var enemy =
                ObjectMgr.GetEntities<Hero>()
                    .Where(y => y.Team != me.Team && y.IsAlive && y.IsVisible && !y.IsIllusion)
                    .ToList();

            foreach (var v in enemy)
            {
                if (!v.IsVisible || !v.IsAlive) continue;

                var meInt = Math.Floor(me.TotalIntelligence);
                var enemyInt = Math.Floor(v.TotalIntelligence);
                var damage = Math.Floor((ult[ultLvl] * (meInt - enemyInt)) * (1 - v.MagicDamageResist));
                var dmg = v.Health - damage;
                var canKill = dmg <= 0;

                var screenPos = HUDInfo.GetHPbarPosition(v);
                if (!OnScreen(v.Position)) continue;

                var text = canKill ? "Yes" : "No, damage:" + Math.Floor(damage);
                var size = new Vector2(15, 15);
                var textSize = Draws.MeasureText(text, "Arial", size, FontFlags.AntiAlias);
                var position = new Vector2(screenPos.X - textSize.X - 2, screenPos.Y - 3);
                Draws.DrawText(
                    text,
                    position,
                    size,
                    (canKill ? Color.LawnGreen : Color.Red),
                    FontFlags.AntiAlias);

            }

        }

        private static bool OnScreen(Vector3 v)
        {
            return !(Draws.WorldToScreen(v).X < 0 || Draws.WorldToScreen(v).X > Draws.Width || Draws.WorldToScreen(v).Y < 0 || Draws.WorldToScreen(v).Y > Draws.Height);
        }

    }
}
