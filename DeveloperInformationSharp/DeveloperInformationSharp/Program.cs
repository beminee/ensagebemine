using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using SharpDX;

namespace DeveloperInformationSharp
{
    class Program
    {
        private static bool printModif = false;
        private static bool findCoords = false;
        private static readonly Menu Menu = new Menu("InformationSharp", "info", true, "", true);
        private static Hero me, target;
        private static double tickRate;

        static void Main(string[] args)
        {
            Game.OnUpdate += Game_OnUpdate;
            Game.OnWndProc += Game_OnWndProc;

            Menu.AddItem(new MenuItem("printModif", "printModif").SetValue(new KeyBind('P', KeyBindType.Press)));
            Menu.AddItem(new MenuItem("findCoords", "findCoords").SetValue(new KeyBind('F', KeyBindType.Press)));
            Menu.AddItem(new MenuItem("tickrate", "Tickrate").SetValue(new Slider(1000, 0, 5000)));
            Menu.AddToMainMenu();
        }

        static void Game_OnWndProc(WndEventArgs args)
        {
            if (!Game.IsChatOpen)
            {
                if (Menu.Item("printModif").GetValue<KeyBind>().Active)
                {
                    printModif = true;
                }
                else
                {
                    printModif = false;
                }

                if (Menu.Item("findCoords").GetValue<KeyBind>().Active)
                {
                    findCoords = true;
                }
                else
                {
                    findCoords = false;
                }

                if (Menu.Item("tickrate").GetValue<Slider>().Value != 1000)
                {
                    tickRate = Menu.Item("tickrate").GetValue<Slider>().Value;
                }
                else
                {
                    tickRate = 1000;
                }
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (!Game.IsInGame || Game.IsPaused || Game.IsWatchingGame) return;

            var me = ObjectManager.LocalHero;
            if (me == null) return;

            if (findCoords && Utils.SleepCheck("printModifs"))

            { 
                //Credits: Moones
            var mousePosition = Game.MousePosition;
            var mouseX = mousePosition.X;
            var mouseY = mousePosition.Y;
            var mouseZ = mousePosition.Z;
            Console.WriteLine("position" + mouseX + mouseY + mouseZ);
            Utils.Sleep(tickRate, "printModifs");
            }

            if (printModif && Utils.SleepCheck("findCoords"))
            {
                PrintModifiers(me);
                Utils.Sleep(tickRate, "findCoords");
            }

        }

        private static void PrintModifiers(Unit unit) //Credits: DaPipex
        {
            var buffs = unit.Modifiers.ToList();
            if (buffs.Any())
            {
                foreach (var buff in buffs)
                {
                    Console.WriteLine(unit.Name + " has modifier: " + buff.Name);
                }
            }
            else
            {
                Console.WriteLine(unit.Name + " does not have any buff");
            }
        }

        private static void CopyPastaArrays()
        {
            var enemy = ObjectManager.GetEntities<Hero>().Where
                (e => e.Team != me.Team && e.IsAlive && e.IsVisible && !e.IsIllusion && !e.UnitState.HasFlag(UnitState.MagicImmune)
                    && me.Distance2D(e) <= 1000).ToList(); //Adds all enemy heroes that are not magic immune closer to you than 1000 range to list.

            var creeps =
                        ObjectManager.GetEntities<Unit>()
                            .Where(
                                x =>
                                    x.IsAlive && x.IsVisible && x.Team != me.Team
                                    && (x.ClassID == ClassID.CDOTA_BaseNPC_Creep_Lane
                                    || x.ClassID == ClassID.CDOTA_BaseNPC_Creep
                                    || x.ClassID == ClassID.CDOTA_BaseNPC_Creep_Neutral
                                    || x.ClassID == ClassID.CDOTA_BaseNPC_Creep_Siege
                                     ) && me.Distance2D(x) <= me.AttackRange).ToList(); //Adds all creeps that are closer to you than your attack range to list.

            var thinker =
                   ObjectManager.GetEntities<Unit>().Where(unit => unit.Name == "npc_dota_thinker" && unit.Team != me.Team).ToList(); //Finds enemy thinkers and add them to list.

            // All talents list Spellbook; http://pastebin.com/i6Mu8FJN
            // Most built-in names; https://developer.valvesoftware.com/wiki/Dota_2_Workshop_Tools/Scripting

        }

        private static void MenuCopyPastas()
        {
     /*     
            Menu.AddItem(new MenuItem("Press", "Press").SetValue(new KeyBind('X', KeyBindType.Press)));
            Menu.AddItem(new MenuItem("Toggle", "Toggle").SetValue(new KeyBind('X', KeyBindType.Toggle)));
            Menu.AddItem(new MenuItem("Slider", "Slider").SetValue(new Slider(0, 0, 100)));
            Menu.AddItem(new MenuItem("StringList", "StringList").SetValue(new StringList(new[] { "1", "2", "3", "4" })));
            Menu.AddItem(new MenuItem("BoolToggler", "BoolToggler").SetValue(true));
            var newMenu = new Menu("AddNewMenu", "AddNewMenu");
            Menu.AddToMainMenu();
            Menu.AddSubMenu(newMenu);       
      */

        }

        private static int CountNeutrals(float radius) //Count all neutral creeps in choosen radius on your hero and returns an integral
        {
            var a = 0;
                var neutrals = ObjectManager.GetEntities<Unit>()
                        .Where(x => x.Team == Team.Neutral && x.IsSpawned && x.IsVisible && me.Distance2D(x) <= radius)
                        .ToList();
                a = a + neutrals.Count;

            return a;
        }

        private static int CountEnemyCreeps(float radius) //Count all enemy creeps in choosen radius on your hero and returns an integral
        {
            var a = 0;
            var enemycreeps = ObjectManager.GetEntities<Unit>()
                    .Where(x => x.Team != me.Team && x.Team != Team.Neutral && x.IsSpawned && x.IsVisible && me.Distance2D(x) <= radius)
                    .ToList();
            a = a + enemycreeps.Count;

            return a;
        }

        public static float GetLaserDamage() //Calculate spell damage with talent additions. Credits: Zynox and ObiXah
        {

            var laserDamage = 0.0f;
            var totalSpellAmp = 0.0f;

            if (me == null || me.ClassID != ClassID.CDOTA_Unit_Hero_Tinker)
            {
                return 0;
            }

            var laser = me.Spellbook.SpellQ;
            if (laser.Level > 0)
            {
                laserDamage += laser.AbilitySpecialData.First(x => x.Name == "laser_damage").GetValue(laser.Level - 1);
            }

            var talent25 = me.Spellbook.Spells.First(x => x.Name == "special_bonus_unique_tinker");
            if (talent25.Level > 0)
            {
                laserDamage += talent25.AbilitySpecialData.First(x => x.Name == "value").Value;
            }

            //Spell Amplification Calculation (addition)
            var talent15 = me.Spellbook.Spells.First(x => x.Name == "special_bonus_spell_amplify_4");
            if (talent15.Level > 0)
            {
                totalSpellAmp += (talent15.AbilitySpecialData.First(x => x.Name == "value").Value) / 100.0f;
            }

            var aetherLens = me.Inventory.Items.FirstOrDefault(x => x.ClassID == ClassID.CDOTA_Item_Aether_Lens);
            if (aetherLens != null)
            {
                totalSpellAmp += (aetherLens.AbilitySpecialData.First(x => x.Name == "spell_amp").Value) / 100.0f;
            }

            totalSpellAmp += (100.0f + me.TotalIntelligence / 16.0f) / 100.0f;

            laserDamage *= totalSpellAmp;

            return laserDamage;
        }

        private static bool isFront(Unit unit, Unit creep) //Check if the unit is infront of a creep. Credits: Sunneeeeee
        {
            var ray = new Ray(creep.NetworkPosition, creep.Vector3FromPolarAngle());
            BoundingSphere unitPos = new BoundingSphere(unit.NetworkPosition, 30);
            if (ray.Intersects(unitPos) && Math.Max(0, creep.Distance2D(unit)) < (creep.AttackRange + 50))
            {
                return true;
            }
            return false;
        }


        
    }
}
