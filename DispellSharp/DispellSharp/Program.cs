using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using SharpDX;

namespace DispellSharp
{

    internal class Program
    {

        private static Item manta, diff, dust, orchid, ghost, repel, ga, As;
        private static readonly Menu Menu = new Menu("DispellSharp", "dispellsharp", true, "item_diffusal_blade", true);
        private static Hero me;
        private static bool enable;


        private static void Main()
        {
            Game.OnUpdate += Game_OnUpdate;
            Game.OnWndProc += Game_OnWndProc;
            Console.WriteLine("DispellSharp Loaded");

            var options = new Menu("Options", "opt");
            options.AddItem(new MenuItem("enable", "Active?").SetValue(true));

            Menu.AddSubMenu(options);
            Menu.AddToMainMenu();

        }
        public static void Game_OnUpdate(EventArgs args)
        {
            if (!Game.IsInGame || Game.IsPaused || Game.IsWatchingGame)
                return;

            me = ObjectManager.LocalHero;
            if (me == null) return;

            if (manta == null)
                manta = me.FindItem("item_manta");

            if (diff == null)
                diff = me.Inventory.Items.FirstOrDefault(item => item.Name.Contains("item_diffusal_blade"));


            if (enable)
            {
                var enemies =
                ObjectManager.GetEntities<Hero>()
                    .Where(y => y.Team != me.Team && y.IsAlive && y.IsVisible && !y.IsIllusion)
                    .ToList();

                foreach (var enemy in enemies)
                {

                    if (enemy.Modifiers.Any(x => x.Name == "modifier_ghost_state") || enemy.Modifiers.Any(x => x.Name == "modifier_item_ethereal_blade_ethereal"))
                    {

                        if (diff != null && diff.CurrentCharges > 0 && diff.Cooldown <= 0 && me.Distance2D(enemy) < 500 && !me.IsChanneling() && me.CanAttack() && Utils.SleepCheck("diff"))
                        {
                            diff.UseAbility(enemy);
                            Utils.Sleep(200, "diff");
                        }
                    }

                    if (enemy.Modifiers.Any(x => x.Name == "modifier_omniknight_repel"))
                    {
                        if (diff != null && diff.CurrentCharges > 0 && diff.Cooldown <= 0 && me.Distance2D(enemy) < 500 && !me.IsChanneling() && me.CanAttack() && Utils.SleepCheck("diff"))
                        {
                            diff.UseAbility(enemy);
                            Utils.Sleep(200, "diff");
                        }
                    }

                    if (enemy.Modifiers.Any(x => x.Name == "modifier_omninight_guardian_angel"))
                    {
                        if (diff != null && diff.CurrentCharges > 0 && diff.Cooldown <= 0 && me.Distance2D(enemy) < 500 && !me.IsChanneling() && me.CanAttack() && Utils.SleepCheck("diff"))
                        {
                            diff.UseAbility(enemy);
                            Utils.Sleep(200, "diff");
                        }
                    }

                    if (me.Modifiers.Any(x => x.Name == "modifier_item_dustofappearance"))
                    {

                        if (manta != null && manta.Cooldown <= 0 && !me.IsChanneling() && Utils.SleepCheck("manta"))
                        {
                            manta.UseAbility();
                            Utils.Sleep(200, "manta");
                            Utils.Sleep(400, "diff");
                        }

                        else if ((manta == null || manta.Cooldown > 0) && diff != null && diff.CurrentCharges > 0 && diff.Cooldown <= 0 && !me.IsChanneling() && Utils.SleepCheck("diff"))
                        {
                            diff.UseAbility(me);
                            Utils.Sleep(200, "diff");
                        }
                    }

                    if (me.Modifiers.Any(x => x.Name == "modifier_item_orchid_malevolence"))
                    {

                        if (manta != null && manta.Cooldown <= 0 && !me.IsChanneling() && Utils.SleepCheck("manta"))
                        {
                            manta.UseAbility();
                            Utils.Sleep(200, "manta");
                            Utils.Sleep(400, "diff");
                        }

                        else if ((manta == null || manta.Cooldown > 0) && diff != null && me.CanCast() && diff.CurrentCharges > 0 && diff.Cooldown <= 0 && !me.IsChanneling() && Utils.SleepCheck("diff"))
                        {
                            diff.UseAbility(me);
                            Utils.Sleep(200, "diff");
                        }
                    }

                    if (me.Modifiers.Any(x => x.Name == "modifier_skywrath_mage_ancient_seal"))
                    {

                        if (manta != null && manta.Cooldown <= 0 && !me.IsChanneling() && Utils.SleepCheck("manta"))
                        {
                            manta.UseAbility();
                            Utils.Sleep(200, "manta");
                            Utils.Sleep(400, "diff");
                        }

                        else if ((manta == null || manta.Cooldown > 0) && diff != null && diff.CurrentCharges > 0 && diff.Cooldown <= 0 && !me.IsChanneling() && Utils.SleepCheck("diff"))
                        {
                            diff.UseAbility(me);
                            Utils.Sleep(200, "diff");
                        }
                    }
                }
            }
        }

        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (!Game.IsChatOpen)
            {

                if (Menu.Item("enable").GetValue<bool>())
                {
                    enable = true;
                }
                else
                {
                    enable = false;
                }
            }
        }
    }
}