using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Threading.Tasks;
using Ensage;
using SharpDX;
using Ensage.Common.Extensions;
using Ensage.Common;
using Ensage.Common.Menu;
using Ensage.Common.Objects.UtilityObjects;
using Ensage.Heroes;
using MeepoSharp.Class;

namespace MeepoSharp
{

    internal class Program
    {
        private static Ability Ww, poof, meepossPoof, meepossNet, meNet;
        private static Item travelss, travels, hex, dagger, ethereal, abyssal, meka;
        private static Hero me, target;
        private static Unit closestNeutral, clos7EstCreep;
        private static Dictionary<float, Orbwalker> orbwalkerDictionary = new Dictionary<float, Orbwalker>();
        private static readonly Menu Menu = new Menu("MeepoSharp", "MeepoSharp", true, "npc_dota_hero_meepo", true);
        private static bool isloaded;
        private static AbilityToggler _menuValue, _comboValue;
        private static int creepscount;
        private static bool runAway, combo, pooferino;
        private static Vector3 stackPosition;

        private static readonly List<CreepWaves> CreepWaves = new List<CreepWaves>();
        private static readonly List<JungleCamps> JungleCamps = new List<JungleCamps>();

        private static List<Unit> _neutrals;
        private static List<Meepo> meepos;
        private static List<Hero> meeposs;

        private static void Main(string[] args)
        {
            Game.OnUpdate += Game_OnUpdate;
            Game.OnUpdate += Dodge;
            Game.OnUpdate += Combo;
            Game.OnWndProc += Game_OnWndProc;

            var dict = new Dictionary<string, bool>
            {
                { "meepo_poof", true },
            };

            var combodict = new Dictionary<string, bool>
            {
                { "item_sheepstick", true },
                { "item_ethereal_blade", true },
                { "item_abyssal_blade", true },
                { "item_mekansm", true }

            };


            #region CreepPositions

            CreepWaves.Add(new CreepWaves
            {
                Name = "Top",
                meepo = null,
                Creeps = new List<Unit>(),
                Coords = new List<Vector3>
                {
                    new Vector3(-6625, -3070, 383),
                    new Vector3(-6599, -1813, 373),
                    new Vector3(-6484, -291, 384),
                    new Vector3(-6409, 1851, 384),
                    new Vector3(-6308, 3979, 384),
                    new Vector3(-5902, 5526, 384),
                    new Vector3(-4703, 5875, 384),
                    new Vector3(-2883, 5956, 384),
                    new Vector3(-954, 6014, 384),
                    new Vector3(1009, 5922, 384),
                    new Vector3(2494, 5772, 264),
                    new Vector3(3395, 5801, 384)
                }
            });
            CreepWaves.Add(new CreepWaves
            {
                Name = "Mid",
                meepo = null,
                Creeps = new List<Unit>(),
                Coords = new List<Vector3>
                {
                new Vector3(-4531, -4160, 384),
                new Vector3(-3928, -3539, 264),
                new Vector3(-3334, -2907, 256),
                new Vector3(-2436, -2076, 255),
                new Vector3(-1708, -1393, 256),
                new Vector3(-504, -332, 128),
                new Vector3(581, 309, 256),
                new Vector3(1883, 1204, 256),
                new Vector3(2771, 2034, 256),
                new Vector3(3377, 2707, 256),
                new Vector3(4180, 3649, 384)
                }
            });
            CreepWaves.Add(new CreepWaves
            {
                Name = "Bot",
                meepo = null,
                Creeps = new List<Unit>(),
                Coords = new List<Vector3>
                { 
                new Vector3(-3827, -6198, 384),
                new Vector3(-2897, -6140, 264),
                new Vector3(-1423, -6323, 312),
                new Vector3(-460, -6306, 384),
                new Vector3(1024, -6415, 384),
                new Vector3(2838, -6249, 384),
                new Vector3(4827, -5903, 383),
                new Vector3(5709, -5289, 384),
                new Vector3(6135, -4057, 384),
                new Vector3(6190, -2062, 384),
                new Vector3(6193, -694, 384),
                new Vector3(6230, 691, 383),
                new Vector3(6337, 1932, 264),
                new Vector3(6352, 2922, 384)}
            });

            JungleCamps.Add(new JungleCamps
            {
                Position = new Vector3(-1708, -4284, 256),
                StackPosition = new Vector3(-1816, -2684, 256),
                WaitPosition = new Vector3(-1867, -4033, 256),
                Team = Team.Radiant,
                Id = 1,
                Empty = false,
                Stacked = false,
                Starttime = 55
            });
            JungleCamps.Add(new JungleCamps
            {
                Position = new Vector3(-266, -3176, 256),
                StackPosition = new Vector3(-522, -1351, 256),
                WaitPosition = new Vector3(-306, -2853, 256),
                Team = Team.Radiant,
                Id = 2,
                Empty = false,
                Stacked = false,
                Starttime = 55
            });
            JungleCamps.Add(new JungleCamps
            {
                Position = new Vector3(1656, -3714, 384),
                StackPosition = new Vector3(48, -4225, 384),
                WaitPosition = new Vector3(1637, -4009, 384),
                Team = Team.Radiant,
                Id = 3,
                Empty = false,
                Stacked = false,
                Starttime = 54
            });
            JungleCamps.Add(new JungleCamps
            {
                Position = new Vector3(3016, -4692, 384),
                StackPosition = new Vector3(3952, -6417, 384),
                WaitPosition = new Vector3(3146, -5071, 384),
                Team = Team.Radiant,
                Id = 4,
                Empty = false,
                Stacked = false,
                Starttime = 53
            });
            JungleCamps.Add(new JungleCamps
            {
                Position = new Vector3(4474, -3598, 384),
                StackPosition = new Vector3(2486, -4125, 384),
                WaitPosition = new Vector3(4121, -3902, 384),
                Team = Team.Radiant,
                Id = 5,
                Empty = false,
                Stacked = false,
                Starttime = 53
            });
            JungleCamps.Add(new JungleCamps
            {
                Position = new Vector3(-3617, 805, 384),
                StackPosition = new Vector3(-5268, 1400, 384),
                WaitPosition = new Vector3(-3984, 643, 384), //this one
                Team = Team.Radiant,
                Id = 6,
                Empty = false,
                Stacked = false,
                Starttime = 53
            });
            JungleCamps.Add(new JungleCamps
            {
                Position = new Vector3(-4446, 3541, 384),
                StackPosition = new Vector3(-3953, 4954, 384),
                WaitPosition = new Vector3(-4251, 3760, 384),
                Team = Team.Dire,
                Id = 7,
                Empty = false,
                Stacked = false,
                Starttime = 53
            });
            JungleCamps.Add(new JungleCamps
            {
                Position = new Vector3(-2981, 4591, 384),
                StackPosition = new Vector3(-3248, 5993, 384),
                WaitPosition = new Vector3(-3050, 4897, 384),
                Team = Team.Dire,
                Id = 8,
                Empty = false,
                Stacked = false,
                Starttime = 53
            });
            JungleCamps.Add(new JungleCamps
            {
                Position = new Vector3(-392, 3652, 384),
                StackPosition = new Vector3(-224, 5088, 384),
                WaitPosition = new Vector3(-503, 3955, 384),
                Team = Team.Dire,
                Id = 9,
                Empty = false,
                Stacked = false,
                Starttime = 55
            });
            JungleCamps.Add(new JungleCamps
            {
                Position = new Vector3(-1524, 2641, 256),
                StackPosition = new Vector3(-1266, 4273, 384),
                WaitPosition = new Vector3(-1465, 2908, 256),
                Team = Team.Dire,
                Id = 10,
                Empty = false,
                Stacked = false,
                Starttime = 53
            });
            JungleCamps.Add(new JungleCamps
            {
                Position = new Vector3(1098, 3338, 384),
                StackPosition = new Vector3(910, 5003, 384),
                WaitPosition = new Vector3(975, 3586, 384),
                Team = Team.Dire,
                Id = 11,
                Empty = false,
                Stacked = false,
                Starttime = 53
            });
            JungleCamps.Add(new JungleCamps
            {
                Position = new Vector3(4141, 554, 384),
                StackPosition = new Vector3(2987, -2, 384),
                WaitPosition = new Vector3(3876, 506, 384),
                Team = Team.Dire,
                Id = 12,
                Empty = false,
                Stacked = false,
                Starttime = 53
            });
            #endregion

            Menu.AddItem(new MenuItem("comboAbilities", "Combo Items:").SetValue(new AbilityToggler(combodict)));
            Menu.AddItem(new MenuItem("enabledAbilities", "Push Abilities:").SetValue(new AbilityToggler(dict)));
            Menu.AddItem(new MenuItem("Stack", "Stack").SetValue(new KeyBind('F', KeyBindType.Toggle)));
            Menu.AddItem(new MenuItem("LanePush", "Lane Push").SetValue(new KeyBind('G', KeyBindType.Toggle)));
            Menu.AddItem(new MenuItem("JungleFarm", "Jungle Farm").SetValue(new KeyBind('H', KeyBindType.Toggle)));
            Menu.AddItem(new MenuItem("RunAway", "Run Away").SetValue(new KeyBind('T', KeyBindType.Toggle)));
            Menu.AddItem(new MenuItem("ComboMode", "Combo!").SetValue(new KeyBind('D', KeyBindType.Press)));
            Menu.AddItem(new MenuItem("PoofSelected", "Poof All Meepos to Selected Meepo").SetValue(new KeyBind('R', KeyBindType.Press)));
            Menu.AddItem(new MenuItem("health", "Run away when below X% HP").SetValue(new Slider(30, 0, 100)));
            Menu.AddToMainMenu();

        }

        public static void Game_OnUpdate(EventArgs args)
        {
            if (!isloaded)
            {
                me = ObjectManager.LocalHero;
                if (!Game.IsInGame || me == null)
                {
                    return;
                }
                if (me.Team == Team.Dire)
                    foreach (var creepWave in CreepWaves)
                        creepWave.Coords.Reverse();
                isloaded = true;
            }

            if (me == null || !me.IsValid)
            {
                isloaded = false;
                me = ObjectManager.LocalHero;
                return;
            }


            if (me.ClassID != ClassID.CDOTA_Unit_Hero_Meepo || Game.IsPaused || Game.IsChatOpen)
            {
                return;
            }

            var stackKey = Menu.Item("Stack").GetValue<KeyBind>().Active;
            var lanePush = Menu.Item("LanePush").GetValue<KeyBind>().Active;
            var JungleFarm = Menu.Item("JungleFarm").GetValue<KeyBind>().Active;
            _menuValue = Menu.Item("enabledAbilities").GetValue<AbilityToggler>();
            _comboValue = Menu.Item("comboAbilities").GetValue<AbilityToggler>();
            Ww = me.Spellbook.Spell2;
            var wRadius = Ww.GetCastRange() - 30;
            var movementspeed = me.MovementSpeed;


            travelss = me.Inventory.Items.FirstOrDefault(item => item.Name.Contains("item_travel_boots"));
            hex = me.FindItem("item_sheepstick");
            ethereal = me.FindItem("item_ethereal_blade");
            dagger = me.FindItem("item_blink");
            abyssal = me.FindItem("item_abyssal_blade");
            meka = me.FindItem("item_mekansm");

            meepos = ObjectManager.GetEntities<Meepo>().Where(meepo => meepo.Team == me.Team && !meepo.Equals(me)).ToList();
            meeposs = ObjectManager.GetEntities<Hero>().Where(x => x.IsControllable && x.IsAlive && x.ClassID == ClassID.CDOTA_Unit_Hero_Meepo).ToList();

            var meeposCount = meepos.Count;

            var seconds = ((int)Game.GameTime) % 60;


            if (JungleCamps.FindAll(x => x.meepos != null).Count != meepos.Count || seconds == 1)
            {
                foreach (var camp in JungleCamps)
                {
                    camp.meepos = null;
                    camp.Stacking = false;
                    camp.Farming = false;
                    camp.State = 0;
                }
            }
            if (seconds == 0)
            {
                foreach (var camp in JungleCamps)
                {
                    camp.meepos = null;
                    camp.Stacking = false;
                    camp.Farming = false;
                    camp.Empty = false;
                    camp.State = 0;
                }
            }

            #region lanepush

            if (lanePush && Utils.SleepCheck("lanePush"))
            {
                try
                {
                    var creeps =
                        ObjectManager.GetEntities<Unit>()
                            .Where(
                                x =>
                                    x.IsAlive && x.IsVisible && x.Team != me.Team
                                    && (x.ClassID == ClassID.CDOTA_BaseNPC_Creep_Lane
                                    || x.ClassID == ClassID.CDOTA_BaseNPC_Creep
                                    || x.ClassID == ClassID.CDOTA_BaseNPC_Creep_Neutral
                                    || x.ClassID == ClassID.CDOTA_BaseNPC_Creep_Siege
                                     ))
                            .OrderByDescending(x => x.Distance2D(new Vector3(0, 0, 0))).ToList();

                    var creepdel = new List<Unit>();
                    foreach (var creepWave in CreepWaves)
                    {
                        creepdel.AddRange(creepWave.Creeps.Where(creep => creeps.All(x => x.Handle != creep.Handle)));
                        foreach (var creep in creepdel)
                            creepWave.Creeps.Remove(creep);
                    }

                    foreach (var creep in creeps)
                    {
                        float[] distance = { float.MaxValue };
                        var name = "";
                        foreach (var creepWave in CreepWaves)
                        {
                            foreach (var pos in creepWave.Coords.Where(pos => distance[0] > pos.Distance2D(creep)))
                            {
                                name = creepWave.Name;
                                distance[0] = pos.Distance2D(creep);
                            }
                        }
                        if (CreepWaves.Any(x => x.Name == name && !x.Creeps.Contains(creep)))
                            CreepWaves.First(x => x.Name == name && !x.Creeps.Contains(creep)).Creeps.Add(creep);
                    }

                    foreach (var creepWave in CreepWaves)
                    {
                        if (creepWave.Creeps.Count > 0)
                            creepWave.Position = new Vector3(
                                creepWave.Creeps.Average(x => x.Position.X),
                                creepWave.Creeps.Average(x => x.Position.Y),
                                creepWave.Creeps.Average(x => x.Position.Z));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error LanePush" + e);
                }

                if (meepos.Count > 0)
                {
                    try
                    {
                        foreach (var meepo in meepos)
                        {
                            if (meepo.HasModifier("modifier_fountain_aura_buff") && (meepo.Health < meepo.MaximumHealth))
                                return;
                            if (!CreepWaves.Any(x => x.meepo != null && x.meepo.Handle == meepo.Handle) &&
                                CreepWaves.Count(x => x.meepo == null) > 0)
                                CreepWaves.First(x => x.meepo == null).meepo = meepo;
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Error LanePush 4");
                    }
                    try
                    {
                        foreach (var creepWave in CreepWaves.Where(x => x.meepo != null))
                        {
                            if (GetClosestCreep(creepWave) != null)
                            {
                                if (creepWave.meepo.Distance2D(GetClosestWave(creepWave)) < 300
                                    || creepWave.meepo.Distance2D(creepWave.Position) < 1000)
                                    creepWave.meepo.Attack(GetClosestCreep(creepWave));
                                else
                                    creepWave.meepo.Move(creepWave.Position);
                            }
                            else
                            {
                                if (creepWave.meepo.Distance2D(GetClosestWave(creepWave)) > 100)
                                {
                                    creepWave.meepo.Move(GetClosestWave(creepWave));
                                    creepWave.Position = GetNextWave(creepWave);
                                }
                                else
                                {
                                    creepWave.meepo.Move(GetNextWave(creepWave));
                                    creepWave.Position = GetNextWave(creepWave);
                                }
                            }

                            if (creepWave.meepo.Modifiers.Any(
                                m => m.Name == "modifier_kill" && Math.Abs(m.Duration - m.ElapsedTime - 1) < 0) ||
                                meepos.All(x => x.Handle != creepWave.meepo.Handle))
                                creepWave.meepo = null;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error LanePush 5" + e);
                    }
                }
                Utils.Sleep(500, "lanePush");
            }
            #endregion

            #region Stack

            else if (stackKey && meepos.Count > 0 && Utils.SleepCheck("wait"))
            {
                foreach (var meepo in meepos)
                {
                    if (runAway)
                    {
                        var fountain = ObjectManager.GetEntities<Unit>()
                        .FirstOrDefault(x => x.Team == me.Team && x.ClassID == ClassID.CDOTA_Unit_Fountain);

                        if (meepo.Health < meepo.MaximumHealth && meepo.Distance2D(fountain) < 1000) return;

                    }


                    if (!Check(meepo))
                    {
                        JungleCamps.Find(x => x.Id == GetClosestCamp(meepo, true, false).Id).meepos = meepo;
                        JungleCamps.Find(x => x.Id == GetClosestCamp(meepo, true, false).Id).Stacking = true;
                    }
                    else
                    {
                        var illusionCamps = CheckMeepo(meepo);
                        switch (illusionCamps.State)
                        {
                            case 0:
                                if (meepo.Distance2D(illusionCamps.WaitPosition) < 5)
                                    illusionCamps.State = 1;
                                else
                                    meepo.Move(illusionCamps.WaitPosition);
                                Utils.Sleep(500, "wait");
                                break;
                            case 1:
                                creepscount = CreepCount(illusionCamps.meepos, 800);
                                if (creepscount == 0)
                                {
                                    JungleCamps.Find(x => x.Id == illusionCamps.Id).meepos = null;
                                    JungleCamps.Find(x => x.Id == illusionCamps.Id).Empty = true;
                                    JungleCamps.Find(x => x.Id == illusionCamps.Id).Stacking = false;
                                    JungleCamps.Find(x => x.Id == GetClosestCamp(meepo, true, false).Id).meepos =
                                        meepo;
                                    JungleCamps.Find(x => x.Id == GetClosestCamp(meepo, true, false).Id).Stacking =
                                        true;
                                }
                                else if (seconds >= illusionCamps.Starttime - 5)
                                {
                                    closestNeutral = GetNearestCreepToPull(illusionCamps.meepos, 800);
                                    stackPosition = illusionCamps.StackPosition;
                                    var moveTime = illusionCamps.Starttime -
                                                   (GetDistance2D(illusionCamps.meepos.Position,
                                                       closestNeutral.Position) +
                                                    (closestNeutral.IsRanged
                                                        ? closestNeutral.AttackRange
                                                        : closestNeutral.RingRadius)) / movementspeed;
                                    illusionCamps.AttackTime = (int)moveTime;
                                    illusionCamps.State = 2;
                                }
                                Utils.Sleep(500, "wait");
                                break;
                            case 2:
                                if (seconds >= illusionCamps.AttackTime)
                                {
                                    closestNeutral = GetNearestCreepToPull(illusionCamps.meepos, 1200);
                                    stackPosition = GetClosestCamp(illusionCamps.meepos, true, false).StackPosition;
                                    illusionCamps.meepos.Attack(closestNeutral);
                                    illusionCamps.State = 3;
                                    var tWait =
                                        (int)
                                            (((GetDistance2D(illusionCamps.meepos.Position, closestNeutral.Position)) /
                                              movementspeed) * 1000 + Game.Ping);
                                    Utils.Sleep(tWait, "" + illusionCamps.meepos.Handle);
                                }
                                break;
                            case 3:
                                if (Utils.SleepCheck("" + illusionCamps.meepos.Handle))
                                {
                                    var poof = meepo.Spellbook.SpellW;
                                    var poofDelay = 1.5;
                                    if (_menuValue.IsEnabled(poof.Name) && poof.CanBeCasted() &&
                                        Creepcountall(wRadius) > Creepcountall(600) / 2 && seconds >= illusionCamps.Starttime - poofDelay)
                                        poof.UseAbility(meepo);
                                    illusionCamps.meepos.Move(illusionCamps.StackPosition);
                                    illusionCamps.State = 4;
                                }
                                break;
                            case 4:
                                meepo.Move(illusionCamps.StackPosition);
                                Utils.Sleep(1000, "wait");
                                break;
                            default:
                                illusionCamps.State = 0;
                                break;

                        }
                    }
                }

            }
            #endregion Stack

            #region Farm

            else if (JungleFarm && meepos.Count > 0 && Utils.SleepCheck("farm"))
            {
                foreach (var meepo in meepos)
                {
                    if (!Check(meepo))
                    {
                        JungleCamps.Find(x => x.Id == GetClosestCamp(meepo, false, false).Id).meepos = meepo;
                        JungleCamps.Find(x => x.Id == GetClosestCamp(meepo, false, false).Id).Farming = true;
                    }
                    else
                    {
                        var illusionCamps = CheckMeepo(meepo);
                        if (meepo.HasModifier("modifier_fountain_aura_buff") && (meepo.Health < meepo.MaximumHealth))
                            return;
                        if (meepo.Distance2D(illusionCamps.Position) > 100)
                        {
                            meepo.Move(illusionCamps.Position);
                        }
                        else
                        {
                            var poof = meepo.Spellbook.SpellW;
                            if (poof.CanBeCasted() && CreepCount(meepo, 300) > 0 && _menuValue.IsEnabled(poof.Name) &&
                                Creepcountall(wRadius) >= Creepcountall(600) / 2)
                                poof.UseAbility(meepo);
                            meepo.Attack(GetNearestCreepToPull(illusionCamps.meepos, 500));
                        }

                       
                    }

                }
                Utils.Sleep(1000, "farm");
            }

            #endregion Farm
        }

        private static void Dodge(EventArgs args)
        {
            if (!Game.IsInGame || Game.IsPaused || Game.IsWatchingGame)
                return;
            if (me == null || me.ClassID != ClassID.CDOTA_Unit_Hero_Meepo) return;

            var fount = ObjectManager.GetEntities<Unit>().Where(x => x.Team == me.Team && x.ClassID == ClassID.CDOTA_Unit_Fountain).ToList();



            var e = ObjectManager.GetEntities<Hero>()
                         .Where(x => x.IsAlive && x.Team != me.Team && !x.IsIllusion)
                         .OrderBy(x => GetDistance2D(x.Position, meeposs.OrderBy(y => GetDistance2D(x.Position, y.Position)).FirstOrDefault().Position))
                         .FirstOrDefault();



            var f = ObjectManager.GetEntities<Hero>()
                        .Where(x => x.IsAlive && x.Team == me.Team && !x.IsIllusion && x.IsControllable && x.ClassID == ClassID.CDOTA_Unit_Hero_Meepo)
                        .OrderBy(x => GetDistance2D(x.Position, fount.OrderBy(y => GetDistance2D(x.Position, y.Position)).FirstOrDefault().Position))
                        .FirstOrDefault();

            var meeposCount = meeposs.Count();
            Ability[] q = new Ability[meeposCount];
            for (int i = 0; i < meeposCount; ++i) q[i] = meeposs[i].Spellbook.SpellQ;
            Ability[] w = new Ability[meeposCount];
            for (int i = 0; i < meeposCount; ++i) w[i] = meeposs[i].Spellbook.SpellW;
            if (runAway && me.IsAlive)
            {
                var baseDota =
                  ObjectManager.GetEntities<Unit>().Where(unit => unit.Name == "npc_dota_base" && unit.Team != me.Team).ToList();
                if (baseDota != null)
                {
                    for (int t = 0; t < baseDota.Count(); ++t)
                    {
                        for (int i = 0; i < meeposCount; ++i)
                        {
                            float angle = meeposs[i].FindAngleBetween(baseDota[t].Position, true);
                            Vector3 pos = new Vector3((float)(baseDota[t].Position.X - 710 * Math.Cos(angle)), (float)(baseDota[t].Position.Y - 710 * Math.Sin(angle)), 0);
                            if (meeposs[i].Distance2D(baseDota[t]) <= 700 && !meeposs[i].HasModifier("modifier_bloodseeker_rupture") && Utils.SleepCheck(meeposs[i].Handle + "MoveDodge"))
                            {
                                meeposs[i].Move(pos);
                                Utils.Sleep(120, meeposs[i].Handle + "MoveDodge");
                                //	Console.WriteLine("Name: " + baseDota[t].Name);
                                //	Console.WriteLine("Speed: " + baseDota[t].Speed);
                                //	Console.WriteLine("ClassID: " + baseDota[t].ClassID);
                                //	Console.WriteLine("Handle: " + baseDota[t].Handle);
                                //	Console.WriteLine("UnitState: " + baseDota[t].UnitState);
                            }
                        }
                    }
                }

                var thinker =
                   ObjectManager.GetEntities<Unit>().Where(unit => unit.Name == "npc_dota_thinker" && unit.Team != me.Team).ToList();
                if (thinker != null)
                {
                    for (int i = 0; i < thinker.Count(); ++i)
                    {
                        for (int j = 0; j < meeposCount; ++j)
                        {
                            float angle = meeposs[j].FindAngleBetween(thinker[i].Position, true);
                            Vector3 pos = new Vector3((float)(thinker[i].Position.X - 360 * Math.Cos(angle)), (float)(thinker[i].Position.Y - 360 * Math.Sin(angle)), 0);
                            if (meeposs[j].Distance2D(thinker[i]) <= 350 && !meeposs[j].HasModifier("modifier_bloodseeker_rupture"))
                            {

                                if (Utils.SleepCheck(meeposs[j].Handle + "MoveDodge"))
                                {
                                    meeposs[j].Move(pos);
                                    Utils.Sleep(350, meeposs[j].Handle + "MoveDodge");
                                }

                            }
                        }
                    }
                }
                foreach (var v in meeposs)
                {
                    if (Utils.SleepCheck(v.Handle + "_move") && v.Health <= v.MaximumHealth / 100 * Menu.Item("health").GetValue<Slider>().Value
                        && !v.HasModifier("modifier_bloodseeker_rupture")
                        && v.Distance2D(fount.First().Position) >= 1000
                        )
                    {
                        v.Move(fount.First().Position);
                        Utils.Sleep(300, v.Handle + "_move");
                    }
                    if (Menu.Item("Stack").GetValue<KeyBind>().Active || Menu.Item("LanePush").GetValue<KeyBind>().Active || Menu.Item("JungleFarm").GetValue<KeyBind>().Active || combo)
                    {
                        float angle = v.FindAngleBetween(fount.First().Position, true);
                        Vector3 pos = new Vector3((float)(fount.First().Position.X - 500 * Math.Cos(angle)), (float)(fount.First().Position.Y - 500 * Math.Sin(angle)), 0);

                        if (
                            v.Health >= v.MaximumHealth * 0.58
                            && v.Distance2D(fount.First()) <= 400
                            && me.Team == Team.Radiant
                            && Utils.SleepCheck(v.Handle + "RadMove")
                            )
                        {
                            v.Move(pos);
                            Utils.Sleep(400, v.Handle + "RadMove");
                        }
                        if (
                            v.Health >= v.MaximumHealth * 0.58
                            && v.Distance2D(fount.First()) <= 400
                            && me.Team == Team.Dire
                            && Utils.SleepCheck(v.Handle + "DireMove")
                            )
                        {
                            v.Move(pos);
                            Utils.Sleep(400, v.Handle + "DireMove");
                        }
                    }
                }

                for (int i = 0; i < meeposCount; ++i)
                {
                    travels = meeposs[i].FindItem("item_travel_boots") ?? meeposs[i].FindItem("item_travel_boots_2");
                    if (w[i] != null
                        && f != null
                        && w[i].CanBeCasted()
                        && !meeposs[i].IsChanneling()
                        && meeposs[i].Health <= meeposs[i].MaximumHealth
                        / 100 * Menu.Item("health").GetValue<Slider>().Value
                        && meeposs[i].Handle != f.Handle
                        && meeposs[i].Distance2D(f) >= 700
                        && e == null
                        && !meeposs[i].HasModifier("modifier_fountain_aura_buff")
                        && meeposs[i].Distance2D(fount.First().Position) >= 1500
                        && Utils.SleepCheck(meeposs[i].Handle + "poof"))
                    {
                        w[i].UseAbility(f);
                        Utils.Sleep(1000, meeposs[i].Handle + "poof");
                    }
                    else if (
                        travels != null
                        && travels.CanBeCasted()
                        && !meeposs[i].IsChanneling()
                        && meeposs[i].Health <= meeposs[i].MaximumHealth
                        / 100 * Menu.Item("health").GetValue<Slider>().Value
                       && (!w[i].CanBeCasted()
                       || meeposs[i].Position.Distance2D(f) >= 1000
                       || (w[i].CanBeCasted()
                       && f.Distance2D(fount.First()) >= 1500))
                       || (meeposs[i].IsSilenced()
                       || meeposs[i].MovementSpeed <= 280)
                       && meeposs[i].Distance2D(fount.First().Position) >= 1500
                       && e == null
                       && !meeposs[i].HasModifier("modifier_fountain_aura_buff")
                       && Utils.SleepCheck(meeposs[i].Handle + "travel"))
                    {
                        travels.UseAbility(fount.First().Position);
                        Utils.Sleep(1000, meeposs[i].Handle + "travel");
                    }

                    if (meeposs[i].HasModifier("modifier_bloodseeker_rupture"))
                    {

                        if (w[i] != null
                            && f != null
                            && w[i].CanBeCasted()
                            && !meeposs[i].IsChanneling()
                            && meeposs[i].Handle != f.Handle
                            && !meeposs[i].HasModifier("modifier_fountain_aura_buff")
                            && Utils.SleepCheck(meeposs[i].Handle + "poof"))
                        {
                            w[i].UseAbility(f);
                            Utils.Sleep(500, meeposs[i].Handle + "poof");
                        }
                        else if (travels != null && travels.CanBeCasted()
                                 && !w[i].CanBeCasted()
                                 && !meeposs[i].IsChanneling()
                                 && meeposs[i].Distance2D(fount.First().Position) >= 1200
                                 && !meeposs[i].HasModifier("modifier_fountain_aura_buff")
                                 && Utils.SleepCheck(meeposs[i].Handle + "travel"))
                        {
                            travels.UseAbility(fount.First().Position);
                            Utils.Sleep(1000, meeposs[i].Handle + "travel");
                        }
                    }
                    if (e != null
                        && q[i] != null
                        && !meeposs[i].IsChanneling()
                        && meeposs[i].Health <= meeposs[i].MaximumHealth
                        / 100 * Menu.Item("health").GetValue<Slider>().Value
                        && q[i].CanBeCasted()
                        && e.Modifiers.Any(y => y.Name != "modifier_meepo_earthbind")
                        && !e.IsMagicImmune()
                        && meeposs[i].Distance2D(e) <= q[i].CastRange - 50
                        && Utils.SleepCheck(meeposs[i].Handle + "NetCast"))
                    {
                        q[i].CastSkillShot(e);
                        Utils.Sleep(q[i].GetCastDelay(meeposs[i], e, true) + 500, meeposs[i].Handle + "NetCast");
                    }
                    else if (!q[i].CanBeCasted() && meeposs[i].Health <= meeposs[i].MaximumHealth / 100 * Menu.Item("health").GetValue<Slider>().Value)
                    {
                        for (var j = 0; j < meeposCount; ++j)
                        {
                            if (e != null
                                && q[j] != null
                                && meeposs[i].Handle != meeposs[j].Handle
                                && meeposs[j].Position.Distance2D(e) < q[i].CastRange
                                && e.Modifiers.Any(y => y.Name != "modifier_meepo_earthbind")
                                && meeposs[j].Position.Distance2D(meeposs[i]) < q[j].CastRange
                                && !e.IsMagicImmune()
                                && Utils.SleepCheck(meeposs[i].Handle + "NetCast"))
                            {
                                q[j].CastSkillShot(e);
                                Utils.Sleep(q[j].GetCastDelay(meeposs[j], e, true) + 1500, meeposs[i].Handle + "NetCast");
                                break;
                            }
                        }
                    }
                    if (e != null
                        && f != null
                        && w[i] != null
                        && w[i].CanBeCasted()
                        && !meeposs[i].IsChanneling()
                        && meeposs[i].Health <= meeposs[i].MaximumHealth
                        / 100 * Menu.Item("health").GetValue<Slider>().Value
                        && meeposs[i].Handle != f.Handle && meeposs[i].Distance2D(f) >= 700
                        && (meeposs[i].Distance2D(e) >= (e.AttackRange + 60)
                        || meeposs[i].MovementSpeed <= 290)
                        && (q == null || (!q[i].CanBeCasted()
                        || e.HasModifier("modifier_meepo_earthbind")
                        || !e.IsMagicImmune()) || meeposs[i].Distance2D(e) >= 1000)
                        && meeposs[i].Distance2D(fount.First().Position) >= 1100
                        && !meeposs[i].HasModifier("modifier_fountain_aura_buff")
                        && Utils.SleepCheck(meeposs[i].Handle + "poof"))
                    {
                        w[i].UseAbility(f);
                        Utils.Sleep(1000, meeposs[i].Handle + "poof");
                    }
                    else if (
                            e != null
                            && travels != null
                            && travels.CanBeCasted()
                            && !meeposs[i].IsChanneling()
                            && meeposs[i].Health <= meeposs[i].MaximumHealth
                            / 100 * Menu.Item("health").GetValue<Slider>().Value
                            && (!w[i].CanBeCasted()
                            || meeposs[i].Position.Distance2D(f) >= 1000
                            || (w[i].CanBeCasted()
                            && f.Distance2D(fount.First()) >= 2000))
                            && (meeposs[i].Distance2D(e) >= (e.AttackRange + 60)
                            || (meeposs[i].IsSilenced()
                            || meeposs[i].MovementSpeed <= 290))
                            && meeposs[i].Distance2D(fount.First().Position) >= 1100
                            && !meeposs[i].HasModifier("modifier_fountain_aura_buff")
                            && Utils.SleepCheck(meeposs[i].Handle + "travel"))
                    {
                        travels.UseAbility(fount.First().Position);
                        Utils.Sleep(1000, meeposs[i].Handle + "travel");
                    }
                }
            }

        }

        public static void Combo(EventArgs args)
        {
            if (!Game.IsInGame || Game.IsPaused || Game.IsWatchingGame)
                return;
            if (me == null || me.ClassID != ClassID.CDOTA_Unit_Hero_Meepo) return;

            if (combo)
            {

                var meeposCount = meeposs.Count;
                meNet = me.Spellbook.Spell1;
                Ability[] meepossNet = new Ability[meeposCount];
                for (int i = 0; i < meeposCount; ++i) meepossNet[i] = meeposs[i].Spellbook.SpellQ;
                Ability[] meepossPoof = new Ability[meeposCount];
                for (int i = 0; i < meeposCount; ++i) meepossPoof[i] = meeposs[i].Spellbook.SpellW;

                var e = ObjectManager.GetEntities<Hero>()
             .Where(x => x.IsAlive && x.Team != me.Team && !x.IsIllusion)
             .OrderBy(x => GetDistance2D(x.Position, meeposs.OrderBy(y => GetDistance2D(x.Position, y.Position)).FirstOrDefault().Position))
             .FirstOrDefault();

                for (int i = 0; i < meeposCount; ++i)
                {
                    target = me.ClosestToMouseTarget();
                    if (target == null) return;

                    var closestmeepo = GetCombo.GetClosestToTarget(meeposs, target);


                    if (meepossPoof[i] != null && meeposs[i].CanCast() && meepossPoof[i].CanBeCasted() && meeposs.Count(x => x.Distance2D(meeposs[i]) <= 1000) > 1 && target.HasModifier("modifier_meepo_earthbind") &&
                        closestmeepo.Distance2D(target) <= 300 && meeposs[i].Health >= meeposs[i].MaximumHealth * 0.7 &&
                        Utils.SleepCheck(meeposs[i].Handle + "poorf"))
                    {
                        meepossPoof[i].UseAbility(target.Position);
                        Utils.Sleep(250, meeposs[i].Handle + "poorf");
                    }

                    if (me.Distance2D(target) > 500)
                    {
                        if (Ww != null && me.CanCast() && !me.IsChanneling() && Ww.CanBeCasted() &&
                            meeposs.Count(x => x.Distance2D(meeposs[i]) <= 1000) > 1 &&
                            closestmeepo.Distance2D(target) <= 300 && Utils.SleepCheck(me.Handle + "poofMe"))
                        {
                            Ww.UseAbility(target.Position);
                            Utils.Sleep(250, me.Handle + "poofMe");
                        }
                    }

                    if (hex != null && hex.CanBeCasted() && me.CanCast() &&
                        !target.UnitState.HasFlag(UnitState.MagicImmune) && me.Distance2D(target) <= 800 &&
                        meeposs[i].Distance2D(target) <= 350 && _comboValue.IsEnabled(hex.Name) && Utils.SleepCheck("hex"))
                    {
                        hex.UseAbility(target);
                        Utils.Sleep(250, "hex");
                    }

                    if (meka != null && meka.CanBeCasted() && me.CanCast() && meeposs[i].Health <= meeposs[i].MaximumHealth
                        / 100 * Menu.Item("health").GetValue<Slider>().Value && me.Distance2D(meeposs[i]) < 900 && _comboValue.IsEnabled(meka.Name) && Utils.SleepCheck("mek"))
                    {
                        meka.UseAbility();
                        Utils.Sleep(250, "mek");
                    }

                    if (Utils.SleepCheck("Net") && !target.HasModifier("modifier_meepo_earthbind") &&
                        (((!dagger.CanBeCasted() || dagger == null) &&
                          meeposs[i].Distance2D(target) <= meepossNet[i].GetCastRange()) ||
                         (dagger.CanBeCasted() && meeposs[i].Distance2D(target) <= 350)))
                    {
                        if (meepossNet[i] != null &&
                            e != null &&
                            (meeposs[i].Health >=
                             meeposs[i].MaximumHealth / 100 * Menu.Item("health").GetValue<Slider>().Value || !runAway) &&
                            meepossNet[i].CanBeCasted() && !e.IsMagicImmune() && !meeposs[i].IsChanneling() &&
                            meeposs[i].Distance2D(target) <= meepossNet[i].GetCastRange() - 30 &&
                            Utils.SleepCheck(meeposs[i].Handle + "NetCast"))
                        {
                            meepossNet[i].CastSkillShot(e);
                            Utils.Sleep(meepossNet[i].GetCastDelay(meeposs[i], e, true) + 1500, meeposs[i].Handle + "NetCast");
                            Utils.Sleep(1500, "Net");
                        }
                    }

                    if (dagger != null && me.CanCast() && dagger.CanBeCasted() && me.Distance2D(target) >= 400 && me.Distance2D(target) <= 1100)
                    {

                        if (me.CanCast() && dagger.CanBeCasted() && me.Distance2D(target) >= 400 &&
                            me.Distance2D(target) <= 1100 &&
                            meeposs[i].Health >= meeposs[i].MaximumHealth / 100 * Menu.Item("health").GetValue<Slider>().Value &&
                            Utils.SleepCheck("dagger"))
                        {
                            dagger.UseAbility(target.Position);
                            Utils.Sleep(200, "dagger");
                        }

                        for (int a = 0; a < meeposCount; ++a)
                        {
                            if (meepossPoof[a] != null
                                && meeposs[a].CanCast() &&
                                meepossPoof[a].CanBeCasted() &&
                                meeposs.Count(x => x.Distance2D(meeposs[a]) <= 1000) > 1 &&
                                closestmeepo.Distance2D(target) <= 300 &&
                                meeposs[a].Health >=
                                meeposs[a].MaximumHealth / 100 * Menu.Item("health").GetValue<Slider>().Value &&
                                Utils.SleepCheck(meeposs[a].Handle + "poof"))
                            {
                                meepossPoof[a].UseAbility(target.Position);
                                Utils.Sleep(250, meeposs[a].Handle + "poof");
                            }
                        }
                    }

                    if (meeposs[i].Distance2D(target) <= 200 &&
                        (!meeposs[i].IsAttackImmune() || !target.IsAttackImmune()) &&
                        meeposs[i].NetworkActivity != NetworkActivity.Attack && meeposs[i].CanAttack() &&
                        meeposs[i].Health >= meeposs[i].MaximumHealth / 100 * Menu.Item("health").GetValue<Slider>().Value &&
                        !meeposs[i].IsChanneling() && Utils.SleepCheck(meeposs[i].Handle + "attacking"))
                    {
                        foreach (
                            var mapo in
                                ObjectManager.GetEntities<Meepo>()
                                    .Where(x => x.IsAlive && x.IsVisible && x.IsControllable))
                        {
                            Orbwalker orbwalker;
                            if (!orbwalkerDictionary.TryGetValue(mapo.Handle, out orbwalker))
                            {
                                orbwalker = new Orbwalker(mapo);
                                orbwalkerDictionary.Add(mapo.Handle, orbwalker);
                            }
                            orbwalker.OrbwalkOn(target);
                            Utils.Sleep(100, meeposs[i].Handle + "attacking");
                        }

                    }

                    else if ((((!meeposs[i].CanAttack() || meeposs[i].Distance2D(target) >= 0) &&
                               meeposs[i].NetworkActivity != NetworkActivity.Attack &&
                               meeposs[i].Distance2D(target) <= 1000) &&
                              ((dagger != null && dagger.CanBeCasted() && me.Distance2D(target) <= 300) ||
                               (!dagger.CanBeCasted())) || dagger == null) &&
                             meeposs[i].Health >=
                             meeposs[i].MaximumHealth / 100 * Menu.Item("health").GetValue<Slider>().Value &&
                             !meeposs[i].IsChanneling() && Utils.SleepCheck(meeposs[i].Handle + "moving"))
                    {
                        meeposs[i].Move(target.Predict(400));
                        Utils.Sleep(400, meeposs[i].Handle + "moving");
                    }

                }

                target = me.ClosestToMouseTarget();
                if (target == null) return;

                if (ethereal != null && ethereal.CanBeCasted() && me.CanCast() &&
                    !target.UnitState.HasFlag(UnitState.MagicImmune) && _comboValue.IsEnabled(ethereal.Name) && Utils.SleepCheck("ethereal"))
                {
                    ethereal.UseAbility(target);
                    Utils.Sleep(250, "ethereal");
                }

                if (abyssal != null && abyssal.CanBeCasted() && me.CanCast() && !target.IsHexed() && !target.IsStunned() && _comboValue.IsEnabled(abyssal.Name) && Utils.SleepCheck("abyssal"))
                {
                    abyssal.UseAbility(target);
                    Utils.Sleep(250, "abyssal");
                }
            }

        }


        public static void Game_OnWndProc(WndEventArgs args)
        {
            if (Game.IsChatOpen) return;
            if (Game.IsKeyDown(Menu.Item("Stack").GetValue<KeyBind>().Key))
            {
                Menu.Item("LanePush").SetValue(new KeyBind(Menu.Item("LanePush").GetValue<KeyBind>().Key, KeyBindType.Toggle));
                Menu.Item("JungleFarm").SetValue(new KeyBind(Menu.Item("JungleFarm").GetValue<KeyBind>().Key, KeyBindType.Toggle));
            }
            if (Game.IsKeyDown(Menu.Item("LanePush").GetValue<KeyBind>().Key))
            {
                Menu.Item("Stack").SetValue(new KeyBind(Menu.Item("Stack").GetValue<KeyBind>().Key, KeyBindType.Toggle));
                Menu.Item("JungleFarm").SetValue(new KeyBind(Menu.Item("JungleFarm").GetValue<KeyBind>().Key, KeyBindType.Toggle));
            }
            if (Game.IsKeyDown(Menu.Item("JungleFarm").GetValue<KeyBind>().Key))
            {
                Menu.Item("Stack").SetValue(new KeyBind(Menu.Item("Stack").GetValue<KeyBind>().Key, KeyBindType.Toggle));
                Menu.Item("LanePush").SetValue(new KeyBind(Menu.Item("LanePush").GetValue<KeyBind>().Key, KeyBindType.Toggle));
            }
            if (Menu.Item("ComboMode").GetValue<KeyBind>().Active)
            {
                Menu.Item("LanePush").SetValue(new KeyBind(Menu.Item("LanePush").GetValue<KeyBind>().Key, KeyBindType.Toggle));
                Menu.Item("JungleFarm").SetValue(new KeyBind(Menu.Item("JungleFarm").GetValue<KeyBind>().Key, KeyBindType.Toggle));
                Menu.Item("Stack").SetValue(new KeyBind(Menu.Item("Stack").GetValue<KeyBind>().Key, KeyBindType.Toggle));
            }

            if (Menu.Item("RunAway").GetValue<KeyBind>().Active)
            {
                runAway = true;
            }
            else
            {
                runAway = false;
            }

            if (Menu.Item("ComboMode").GetValue<KeyBind>().Active)
            {
                combo = true;
            }
            else
            {
                combo = false;
            }

            if (Menu.Item("PoofSelected").GetValue<KeyBind>().Active)
            {
                pooferino = true;
            }
            else
            {
                pooferino = false;
            }
        }

        private static JungleCamps GetClosestCamp(Meepo illusion, bool stack, bool any)
        {
            JungleCamps[] closest =
            {
                new JungleCamps {WaitPosition = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue), Id = 0}
            };
            var Camps =
                    JungleCamps.Where(
                        x =>
                            illusion.Distance2D(x.WaitPosition) < illusion.Distance2D(closest[0].WaitPosition) &&
                            !x.Farming &&
                            !x.Stacking && !x.Empty);
            if (stack)
            {
                Camps =
                JungleCamps.Where(
                    x =>
                        illusion.Distance2D(x.WaitPosition) < illusion.Distance2D(closest[0].WaitPosition) &&
                        !x.Farming &&
                        !x.Stacking && !x.Empty && x.Team == me.Team);
            }
            foreach (var x in Camps)
            {
                closest[0] = x;
            }
            return closest[0];
        }

        private static JungleCamps CheckMeepo(Meepo clone)
        {
            var a = new JungleCamps();
            return JungleCamps.Where(x => x.meepos != null).Aggregate(a, (current, x) => (x.meepos.Handle == clone.Handle ? x : current));
        }

        private static bool Check(Meepo clone)
        {
            return JungleCamps.Where(x => x.meepos != null).Aggregate(false, (current, x) => (x.meepos.Handle == clone.Handle || current));
        }

        private static int Creepcountall(float radius)
        {
            var a = 0;
            foreach (var meepo in meepos)
            {
                _neutrals = ObjectManager.GetEntities<Unit>()
                        .Where(x => x.Team == Team.Neutral && x.IsSpawned && x.IsVisible && meepo.Distance2D(x) <= radius)
                        .ToList();
                a = a + _neutrals.Count;
            }
            return a;
        }

        private static int CreepCount(Unit h, float radius)
        {
            try
            {
                return
                    ObjectManager.GetEntities<Unit>()
                        .Where(x => x.Team == Team.Neutral && x.IsSpawned && x.IsVisible && h.Distance2D(x) <= radius)
                        .ToList().Count;
            }
            catch (Exception)
            {
                //
            }
            return 0;
        }

        private static Vector3 GetClosestWave(CreepWaves creepWave)
        {
            var pos = new Vector3();
            try
            {
                float[] distance = { float.MaxValue };
                foreach (var position in creepWave.Coords)
                {
                    if (!(position.Distance2D(creepWave.Position) < distance[0])) continue;
                    distance[0] = position.Distance2D(creepWave.Position);
                    pos = position;
                }
                return pos;
            }
            catch (Exception)
            {
                Console.WriteLine("Error GetClosestWave");
            }
            return pos;
        }

        private static Vector3 GetNextWave(CreepWaves creepWave)
        {
            float[] distance = { float.MaxValue };
            var p = 0;
            var coords = creepWave.Coords.ToArray();
            for (var i = 0; i < coords.Length; i++)
            {
                if (coords[i].Distance2D(creepWave.meepo.Position) < distance[0])
                {
                    distance[0] = coords[i].Distance2D(creepWave.meepo.Position);
                    p = i + 1 < coords.Length ? i + 1 : i;
                }
            }
            return coords[p];
        }

        private static Unit GetClosestCreep(CreepWaves creepWave)
        {
            float[] distance = { float.MaxValue };
            Unit closest = null;
            try
            {
                var creeps = creepWave.Creeps;
                foreach (var creep in creeps.Where(creep => creep.IsValidTarget() && distance[0] > creepWave.meepo.Distance2D(creep.Position))
                    )
                {
                    distance[0] = creepWave.meepo.Distance2D(creep.Position);
                    closest = creep;
                }
                return closest ?? ObjectManager
                    .GetEntities<Unit>(
                    ).Where(
                        x => x.IsAlive && x.IsVisible && x.IsValidTarget() && x.Team != me.Team
                             && (x.ClassID == ClassID.CDOTA_BaseNPC_Barracks
                                 || x.ClassID == ClassID.CDOTA_BaseNPC_Tower
                                 || x.ClassID == ClassID.CDOTA_BaseNPC_Building
                                 ) && x.Distance2D(creepWave.Position) < 2000)
                    .OrderBy(x => x.Distance2D(creepWave.meepo))
                    .DefaultIfEmpty(null)
                    .FirstOrDefault();
            }
            catch (Exception)
            {
                // ignore errors
            }
            return closest;
        }

        private static Unit GetNearestCreepToPull(Meepo illusion, int dis)
        {
            var creeps =
                ObjectManager.GetEntities<Unit>().Where(x => x.IsAlive && x.IsSpawned && x.IsVisible && illusion.Distance2D(x) <= dis && x.Team != me.Team).ToList();
            Unit bestCreep = null;
            var bestDistance = float.MaxValue;
            foreach (var creep in creeps)
            {
                var distance = GetDistance2DFast(illusion, creep);
                if (bestCreep == null || distance < bestDistance)
                {
                    bestDistance = distance;
                    bestCreep = creep;
                }

            }
            return bestCreep;
        }

        private static float GetDistance2DFast(Entity e1, Entity e2)
        {
            return (float)(Math.Pow(e1.Position.X - e2.Position.X, 2) + Math.Pow(e1.Position.Y - e2.Position.Y, 2));
        }

        private static float GetDistance2D(Vector3 p1, Vector3 p2)
        {
            return (float)Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        /* public static void UnAggro(List<Unit> z, Hero v)
         {
             var projectiles = ObjectManager.TrackingProjectiles.Where(x => x.Target.Handle == v.Handle).ToList();
             if (v == null) return;
             if (projectiles == null) return;
             for (int i = 0; i < projectiles.Count(); ++i)
             {
                 var closestCreepUnAgr = GetCombo.GetClosestUnnagroCreep(z, v);
                 if (closestCreepUnAgr == null) return;
                 if (projectiles[i].Source.ClassID == ClassID.CDOTA_BaseNPC_Tower
                 || projectiles[i].Source.ClassID == ClassID.CDOTA_Unit_Fountain)
                 {
                     if (closestCreepUnAgr.Distance2D(v) <= 500 & Utils.SleepCheck("UnAgr"))
                     {
                         v.Attack(closestCreepUnAgr);
                         Utils.Sleep(500, "UnAgr");
                     }
                 }
             }
         }*/

    }
}