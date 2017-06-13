using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using PlaySharp.Toolkit.Helper.Annotations;

namespace KaelSharp
{
    internal class KaelSharp
    {
        public static void OnLoad(object sender, EventArgs args)
        {
            if (!Game.IsInGame || Game.IsPaused || Game.IsWatchingGame) return;
            Variables.Me = ObjectManager.LocalHero;

            if (Variables.Me != null && Variables.Me.ClassID != ClassID.CDOTA_Unit_Hero_Invoker) return;
            Orbwalking.Load();
            MenuLoader();
            Game.OnIngameUpdate += InGameUpdate;
        }

        public static void MenuLoader()
        {
            Variables.Menu = new Menu("Kael#", "kaelsharp", true, "npc_dota_hero_invoker");

            Variables.Menu.AddItem(new MenuItem("enabled", "Enabled")).SetValue(true);

            var hotkeys = new Menu("Hotkeys", "hotkeys");
            hotkeys.AddItem(new MenuItem("combo", "Combo")).SetValue(new KeyBind(32, KeyBindType.Press));
            hotkeys.AddItem(new MenuItem("prepare", "Prepare Combo")).SetValue(new KeyBind('G', KeyBindType.Press));
            hotkeys.AddItem(new MenuItem("flee", "Flee?")).SetValue(new KeyBind('F', KeyBindType.Press));
            Variables.Menu.AddSubMenu(hotkeys);

            var debug = new Menu("Developer", "debug");
            debug.AddItem(new MenuItem("debug1", "debug1")).SetValue(false);
            debug.AddItem(new MenuItem("debug2", "debug2")).SetValue(false);
            Variables.Menu.AddSubMenu(debug);

            var otherOptions = new Menu("Other Options", "otheroptions");
            otherOptions.AddItem(new MenuItem("orbwalk", "Enable Orbwalk?")).SetValue(true);
            otherOptions.AddItem(new MenuItem("distance", "Range to look for targets"))
                .SetValue(new Slider(1000, 0, 2000));
            otherOptions.AddItem(new MenuItem("cancer", "Spam Invoke?")).SetValue(false);
            otherOptions.AddItem(new MenuItem("sunstrike", "Sunstrike on disabled unit?")).SetValue(false);
            Variables.Menu.AddSubMenu(otherOptions);

            Variables.Menu.AddToMainMenu();
        }

        public static void MenuUpdate()
        {
            MenuVariables.Enabled = Variables.Menu.Item("enabled").GetValue<bool>();
            if (!MenuVariables.Enabled) return;
            MenuVariables.ComboKey = Variables.Menu.Item("combo").GetValue<KeyBind>().Key;
            MenuVariables.PrepareKey = Variables.Menu.Item("prepare").GetValue<KeyBind>().Key;
            MenuVariables.Flee = Variables.Menu.Item("flee").GetValue<KeyBind>().Key;
            MenuVariables.Orbwalk = Variables.Menu.Item("orbwalk").GetValue<bool>();
            MenuVariables.Distance = Variables.Menu.Item("distance").GetValue<Slider>().Value;
            MenuVariables.Cancer = Variables.Menu.Item("cancer").GetValue<bool>();
            MenuVariables.SunStrike = Variables.Menu.Item("sunstrike").GetValue<bool>();

            MenuVariables.Debug1 = Variables.Menu.Item("debug1").GetValue<bool>();
            MenuVariables.Debug2 = Variables.Menu.Item("debug2").GetValue<bool>();

        }

        private static void InGameUpdate(EventArgs args)
        {
            Variables.Quas = Variables.Me.Spellbook.SpellQ;
            Variables.Wex = Variables.Me.Spellbook.SpellW;
            Variables.Exort = Variables.Me.Spellbook.SpellE;
            Variables.Invoke = Variables.Me.Spellbook.SpellR;
            Variables.D = Variables.Me.Spellbook.SpellD;
            Variables.F = Variables.Me.Spellbook.SpellF;

            MenuUpdate();

            if (Variables.Quas.Level == 0 && Variables.Wex.Level == 0 && Variables.Exort.Level == 0 && Utils.SleepCheck("KaelSharp.OrbWarning"))
            {
                Game.PrintMessage(">>> You at least need to put 1 level on any orb <<<");
                Utils.Sleep(15000, "KaelSharp.OrbWarning");
                return;
            }

            Variables.Target = Variables.Me.ClosestToMouseTarget(); //Todo: Add Custom Target Selector
            Variables.HasAghs = Variables.Me.AghanimState();

            if (MenuVariables.Enabled)
            {
                if (Game.IsKeyDown(MenuVariables.PrepareKey))
                {
                    Prepare();
                    Functions.UnitAttack();

                    if (MenuVariables.Orbwalk && Variables.Target != null && Variables.Target.HasModifiers(Variables.CantAttackModifiers, false))
                    {
                        Orbwalking.Orbwalk(Variables.Target, 0, 0, true);
                    }
                }

                else if (Game.IsKeyDown(MenuVariables.ComboKey))
                {
                    Combo();
                    Functions.UnitAttack();

                    if (MenuVariables.Orbwalk && Variables.Target != null && Variables.Target.HasModifiers(Variables.CantAttackModifiers, false))
                    {
                        Orbwalking.Orbwalk(Variables.Target, 0, 0, true);
                    }
                }

                if (Game.IsKeyDown(MenuVariables.Flee))
                {
                    Flee();
                }

                if (MenuVariables.Cancer)
                {
                    Cancer();
                    if (Game.IsChatOpen) return;
                    if (Game.IsKeyDown(MenuVariables.PrepareKey) || Game.IsKeyDown(MenuVariables.ComboKey))
                    {
                        Variables.Menu.Item("cancer").SetValue(false);
                    }
                }

                if (MenuVariables.SunStrike)
                {
                    SunStrike();
                }

                if (MenuVariables.Debug1) // TODO: Improve
                {
                    foreach (
                        var x in
                        ObjectManager.GetEntitiesParallel<Unit>()
                            .Where(x => x.IsControllableByPlayer(ObjectManager.LocalPlayer)))
                    {
                        Game.PrintMessage(x.Name + "/" + x.ClassID + "/" + x.Handle);
                    }
                }

                if (MenuVariables.Debug2)
                {
                    if (Variables.Target == null) return;
                    foreach (var targetModif in Variables.Target.Modifiers)
                    {
                        Game.PrintMessage(targetModif.Name + "/" + targetModif.RemainingTime);
                    }

                    if (Variables.Target == null || !Variables.Target.HasModifier(Variables.TornadoModif)) return;
                    if (Variables.Target.HasModifier(Variables.TornadoModif))
                    {
                        var remainingTime = Variables.Target.FindModifier(Variables.TornadoModif).RemainingTime;
                        Console.WriteLine(remainingTime);
                    }
                }
            }
        }

        public static void Init()
        {
            Events.OnLoad += OnLoad;
        }

        public static void Combo()
        {
            if (Variables.Target == null) return;

            var emp = Variables.Me.FindSpell(Functions.EnumToString(Variables.Abilities.Emp));
            var tornado = Variables.Me.FindSpell(Functions.EnumToString(Variables.Abilities.Tornado));
            var alacrity = Variables.Me.FindSpell(Functions.EnumToString(Variables.Abilities.Alacrity));
            var ghostwalk = Variables.Me.FindSpell(Functions.EnumToString(Variables.Abilities.GhostWalk));
            var deafeningblast = Variables.Me.FindSpell(Functions.EnumToString(Variables.Abilities.DeafeningBlast));
            var chaosmeteor = Variables.Me.FindSpell(Functions.EnumToString(Variables.Abilities.ChaosMeteor));
            var coldsnap = Variables.Me.FindSpell(Functions.EnumToString(Variables.Abilities.ColdSnap));
            var icewall = Variables.Me.FindSpell(Functions.EnumToString(Variables.Abilities.IceWall));
            var forgespirit = Variables.Me.FindSpell(Functions.EnumToString(Variables.Abilities.ForgeSpirit));
            var sunstrike = Variables.Me.FindSpell(Functions.EnumToString(Variables.Abilities.SunStrike));
            var eLevel = Variables.Me.Spellbook.SpellE.Level;
            var wLevel = Variables.Me.Spellbook.SpellW.Level;
            var qLevel = Variables.Me.Spellbook.SpellQ.Level;
            var tornadoLiftTime = Functions.TornadoLiftTime();
            var empTime = 2.9f;

            if (Variables.Target.HasModifiers(Variables.CantAttackModifiers, false)) return;

            if (Variables.HasAghs)
            {
                if (Functions.HasInvokerSpell(Variables.Abilities.Tornado) && tornado.CanBeCasted() &&
                    Functions.HasInvokerSpell(Variables.Abilities.Emp) && emp.CanBeCasted() && Utils.SleepCheck("KaelSharp.Tornado"))
                {
                    tornado.CastSkillShot(Variables.Target, Variables.Me.NetworkPosition);
                    Utils.Sleep(100 ,"KaelSharp.Tornado");
                }

                else if (Functions.OnTornadoHit() && emp.CanBeCasted())
                {
                    if (Math.Abs(Variables.Target.FindModifier(Variables.TornadoModif).RemainingTime) > 0)
                    {
                        Functions.ChainTornado(Variables.Abilities.Emp, tornadoLiftTime);
                    }
                }

                else
                {
                    if (!Functions.HasInvokerSpell(Variables.Abilities.Emp) && emp.CanBeCasted() &&
                        Variables.Invoke.CanBeCasted())
                    {
                        Functions.PrepareSpell(Variables.Abilities.Emp);
                    }

                    else if (Functions.HasInvokerSpell(Variables.Abilities.Emp) && emp.CanBeCasted())
                    {
                        emp.CastSkillShot(Variables.Target, Variables.Me.NetworkPosition);
                    }
                    
                    else if (!Functions.HasInvokerSpell(Variables.Abilities.Tornado) && Variables.Invoke.CanBeCasted() &&
                             tornado.AbilityState == AbilityState.Ready)
                    {
                        Functions.PrepareSpell(Variables.Abilities.Tornado);
                    }

                    else if (Functions.HasInvokerSpell(Variables.Abilities.Tornado) && tornado.CanBeCasted() &&
                             Utils.SleepCheck("KaelSharp.Tornado"))
                    {
                        tornado.CastSkillShot(Variables.Target, Variables.Me.NetworkPosition);
                        Utils.Sleep(100, "KaelSharp.Tornado");
                    }

                    else if (!Functions.HasInvokerSpell(Variables.Abilities.ChaosMeteor) &&
                             chaosmeteor.AbilityState == AbilityState.Ready && Variables.Invoke.CanBeCasted())
                    {
                        Functions.PrepareSpell(Variables.Abilities.ChaosMeteor);
                    }

                    else if (Functions.HasInvokerSpell(Variables.Abilities.ChaosMeteor) && chaosmeteor.CanBeCasted()
                             && Utils.SleepCheck("KaelSharp.Meteor"))
                    {
                        chaosmeteor.CastSkillShot(Variables.Target, Variables.Me.NetworkPosition);
                        Utils.Sleep(100, "KaelSharp.Meteor");
                    }

                    else if (!Functions.HasInvokerSpell(Variables.Abilities.DeafeningBlast) &&
                             deafeningblast.AbilityState == AbilityState.Ready && Variables.Invoke.CanBeCasted())
                    {
                        Functions.PrepareSpell(Variables.Abilities.DeafeningBlast);
                    }
                    
                    else if (Functions.HasInvokerSpell(Variables.Abilities.DeafeningBlast) && deafeningblast.CanBeCasted() &&
                             Utils.SleepCheck("KaelSharp.Blast"))
                    {
                        deafeningblast.CastSkillShot(Variables.Target, Variables.Me.NetworkPosition);
                        Utils.Sleep(100, "KaelSharp.Blast");
                    }

                    else if (Variables.Target.Distance2D(Variables.Me) > 350)
                    {
                        if (!Functions.HasInvokerSpell(Variables.Abilities.ColdSnap) &&
                            coldsnap.AbilityState == AbilityState.Ready && Variables.Invoke.CanBeCasted())
                        {
                            Functions.PrepareSpell(Variables.Abilities.ColdSnap);
                        }

                        else if (Functions.HasInvokerSpell(Variables.Abilities.ColdSnap) && coldsnap.CanBeCasted() &&
                                 Utils.SleepCheck("KaelSharp.ColdSnap"))
                        {
                            coldsnap.UseAbility(Variables.Target);
                            Utils.Sleep(100, "KaelSharp.ColdSnap");
                        }

                        else if (!Functions.HasInvokerSpell(Variables.Abilities.ForgeSpirit) &&
                                 forgespirit.AbilityState == AbilityState.Ready && Variables.Invoke.CanBeCasted())
                        {
                            Functions.PrepareSpell(Variables.Abilities.ForgeSpirit);
                        }

                        else if (Functions.HasInvokerSpell(Variables.Abilities.ForgeSpirit) && forgespirit.CanBeCasted() &&
                                 Utils.SleepCheck("KaelSharp.Spirit"))
                        {
                            forgespirit.UseAbility();
                            Utils.Sleep(100, "KaelSharp.Spirit");
                        }

                        else if (!Functions.HasInvokerSpell(Variables.Abilities.Alacrity) &&
                                 alacrity.AbilityState == AbilityState.Ready && Variables.Invoke.CanBeCasted())
                        {
                            Functions.PrepareSpell(Variables.Abilities.Alacrity);
                        }

                        else if (Functions.HasInvokerSpell(Variables.Abilities.Alacrity) && alacrity.CanBeCasted() &&
                                 Utils.SleepCheck("KaelSharp.Alacrity"))
                        {
                            alacrity.UseAbility(Variables.Me);
                            Utils.Sleep(100, "KaelSharp.Alacrity");
                        }
                    }

                    else if (Variables.Target.Distance2D(Variables.Me) <= 350)
                    {
                        if (!Functions.HasInvokerSpell(Variables.Abilities.IceWall) &&
                            icewall.AbilityState == AbilityState.Ready && Variables.Invoke.CanBeCasted())
                        {
                            Functions.PrepareSpell(Variables.Abilities.IceWall);
                        }

                        else if (Functions.HasInvokerSpell(Variables.Abilities.IceWall) && icewall.CanBeCasted() &&
                                 Utils.SleepCheck("KaelSharp.IceWall"))
                        {
                            if (Prediction.InFront(Variables.Me, 200).Distance2D(Variables.Target) < 105)
                            {
                                icewall.UseAbility();
                                Utils.Sleep(100, "KaelSharp.IceWall");
                            }
                        }

                        else if (!Functions.HasInvokerSpell(Variables.Abilities.SunStrike) &&
                                 sunstrike.AbilityState == AbilityState.Ready && Variables.Invoke.CanBeCasted())
                        {
                            Functions.PrepareSpell(Variables.Abilities.SunStrike);
                        }

                        else if (Functions.HasInvokerSpell(Variables.Abilities.SunStrike) && sunstrike.CanBeCasted() &&
                                 Utils.SleepCheck("KaelSharp.Sunstrike"))
                        {
                            sunstrike.CastSkillShot(Variables.Target, Variables.Me.NetworkPosition);
                            Utils.Sleep(100, "KaelSharp.Sunstrike");
                        }
                    }
                }
            }

            else
            {
                if (wLevel < eLevel)
                {
                    if (Functions.HasInvokerSpell(Variables.Abilities.Tornado) && tornado.CanBeCasted() &&
                    Functions.HasInvokerSpell(Variables.Abilities.ChaosMeteor) && chaosmeteor.CanBeCasted() && Utils.SleepCheck("KaelSharp.Tornado"))
                    {
                        tornado.CastSkillShot(Variables.Target, Variables.Me.NetworkPosition);
                        Utils.Sleep(100, "KaelSharp.Tornado");
                    }

                    else if (Functions.OnTornadoHit() && chaosmeteor.CanBeCasted())
                    {
                        if (Math.Abs(Variables.Target.FindModifier(Variables.TornadoModif).RemainingTime) > 0)
                        {
                            Functions.ChainTornado(Variables.Abilities.ChaosMeteor, tornadoLiftTime);
                        }
                    }

                    else
                    {
                        if (!Functions.HasInvokerSpell(Variables.Abilities.ChaosMeteor) && Variables.Invoke.CanBeCasted() &&
                             chaosmeteor.AbilityState == AbilityState.Ready)
                        {
                            Functions.PrepareSpell(Variables.Abilities.ChaosMeteor);
                        }

                        else if (Functions.HasInvokerSpell(Variables.Abilities.ChaosMeteor) && chaosmeteor.CanBeCasted() &&
                                 Utils.SleepCheck("KaelSharp.Meteor"))
                        {
                            tornado.CastSkillShot(Variables.Target, Variables.Me.NetworkPosition);
                            Utils.Sleep(100, "KaelSharp.Meteor");
                        }

                        else if (!Functions.HasInvokerSpell(Variables.Abilities.Tornado) && Variables.Invoke.CanBeCasted() &&
                             tornado.AbilityState == AbilityState.Ready)
                        {
                            Functions.PrepareSpell(Variables.Abilities.Tornado);
                        }

                        else if (Functions.HasInvokerSpell(Variables.Abilities.Tornado) && tornado.CanBeCasted() &&
                                 Utils.SleepCheck("KaelSharp.Tornado"))
                        {
                            tornado.CastSkillShot(Variables.Target, Variables.Me.NetworkPosition);
                            Utils.Sleep(100, "KaelSharp.Tornado");
                        }

                        else if (!Functions.HasInvokerSpell(Variables.Abilities.DeafeningBlast) && Variables.Invoke.CanBeCasted() &&
                             deafeningblast.AbilityState == AbilityState.Ready)
                        {
                            Functions.PrepareSpell(Variables.Abilities.DeafeningBlast);
                        }

                        else if (Functions.HasInvokerSpell(Variables.Abilities.DeafeningBlast) && deafeningblast.CanBeCasted() &&
                                 Utils.SleepCheck("KaelSharp.Blast"))
                        {
                            deafeningblast.CastSkillShot(Variables.Target, Variables.Me.NetworkPosition);
                            Utils.Sleep(100, "KaelSharp.Blast");
                        }

                        else if (Variables.Target.Distance2D(Variables.Me) > 350)
                        {
                            if (!Functions.HasInvokerSpell(Variables.Abilities.ColdSnap) &&
                                coldsnap.AbilityState == AbilityState.Ready && Variables.Invoke.CanBeCasted())
                            {
                                Functions.PrepareSpell(Variables.Abilities.ColdSnap);
                            }

                            else if (Functions.HasInvokerSpell(Variables.Abilities.ColdSnap) && coldsnap.CanBeCasted() &&
                                     Utils.SleepCheck("KaelSharp.ColdSnap"))
                            {
                                coldsnap.UseAbility(Variables.Target);
                                Utils.Sleep(100, "KaelSharp.ColdSnap");
                            }

                            else if (!Functions.HasInvokerSpell(Variables.Abilities.ForgeSpirit) &&
                                     forgespirit.AbilityState == AbilityState.Ready && Variables.Invoke.CanBeCasted())
                            {
                                Functions.PrepareSpell(Variables.Abilities.ForgeSpirit);
                            }

                            else if (Functions.HasInvokerSpell(Variables.Abilities.ForgeSpirit) && forgespirit.CanBeCasted() &&
                                     Utils.SleepCheck("KaelSharp.Spirit"))
                            {
                                forgespirit.UseAbility();
                                Utils.Sleep(100, "KaelSharp.Spirit");
                            }

                            else if (!Functions.HasInvokerSpell(Variables.Abilities.Alacrity) &&
                                     alacrity.AbilityState == AbilityState.Ready && Variables.Invoke.CanBeCasted())
                            {
                                Functions.PrepareSpell(Variables.Abilities.Alacrity);
                            }

                            else if (Functions.HasInvokerSpell(Variables.Abilities.Alacrity) && alacrity.CanBeCasted() &&
                                     Utils.SleepCheck("KaelSharp.Alacrity"))
                            {
                                alacrity.UseAbility(Variables.Me);
                                Utils.Sleep(100, "KaelSharp.Alacrity");
                            }
                        }

                        else if (Variables.Target.Distance2D(Variables.Me) <= 350)
                        {
                            if (!Functions.HasInvokerSpell(Variables.Abilities.IceWall) &&
                                icewall.AbilityState == AbilityState.Ready && Variables.Invoke.CanBeCasted())
                            {
                                Functions.PrepareSpell(Variables.Abilities.IceWall);
                            }

                            else if (Functions.HasInvokerSpell(Variables.Abilities.IceWall) && icewall.CanBeCasted() &&
                                     Utils.SleepCheck("KaelSharp.IceWall"))
                            {
                                if (Prediction.InFront(Variables.Me, 200).Distance2D(Variables.Target) < 105)
                                {
                                    icewall.UseAbility();
                                    Utils.Sleep(100, "KaelSharp.IceWall");
                                }
                            }

                            else if (!Functions.HasInvokerSpell(Variables.Abilities.SunStrike) &&
                                     sunstrike.AbilityState == AbilityState.Ready && Variables.Invoke.CanBeCasted())
                            {
                                Functions.PrepareSpell(Variables.Abilities.SunStrike);
                            }

                            else if (Functions.HasInvokerSpell(Variables.Abilities.SunStrike) && sunstrike.CanBeCasted() &&
                                     Utils.SleepCheck("KaelSharp.Sunstrike"))
                            {
                                sunstrike.CastSkillShot(Variables.Target, Variables.Me.NetworkPosition);
                                Utils.Sleep(100, "KaelSharp.Sunstrike");
                            }
                        }
                    }
                }
                else if (wLevel >= eLevel)
                {
                    if (Functions.HasInvokerSpell(Variables.Abilities.Tornado) && tornado.CanBeCasted() &&
                    Functions.HasInvokerSpell(Variables.Abilities.Emp) && emp.CanBeCasted() && Utils.SleepCheck("KaelSharp.Tornado"))
                    {
                        tornado.CastSkillShot(Variables.Target, Variables.Me.NetworkPosition);
                        Utils.Sleep(100, "KaelSharp.Tornado");
                    }

                    else if (Functions.OnTornadoHit() && emp.CanBeCasted())
                    {
                        if (Math.Abs(Variables.Target.FindModifier(Variables.TornadoModif).RemainingTime) > 0)
                        {
                            Functions.ChainTornado(Variables.Abilities.Emp, tornadoLiftTime);
                        }
                    }

                    else
                    {
                        if (!Functions.HasInvokerSpell(Variables.Abilities.Emp) && emp.CanBeCasted() &&
                        Variables.Invoke.CanBeCasted())
                        {
                            Functions.PrepareSpell(Variables.Abilities.Emp);
                        }

                        else if (Functions.HasInvokerSpell(Variables.Abilities.Emp) && emp.CanBeCasted())
                        {
                            emp.CastSkillShot(Variables.Target, Variables.Me.NetworkPosition);
                        }

                        else if (!Functions.HasInvokerSpell(Variables.Abilities.Tornado) && Variables.Invoke.CanBeCasted() &&
                                 tornado.AbilityState == AbilityState.Ready)
                        {
                            Functions.PrepareSpell(Variables.Abilities.Tornado);
                        }

                        else if (Functions.HasInvokerSpell(Variables.Abilities.Tornado) && tornado.CanBeCasted() &&
                                 Utils.SleepCheck("KaelSharp.Tornado"))
                        {
                            tornado.CastSkillShot(Variables.Target, Variables.Me.NetworkPosition);
                            Utils.Sleep(100, "KaelSharp.Tornado");
                        }

                        if (Variables.Target.Distance2D(Variables.Me) <= 350)
                        {
                            if (!Functions.HasInvokerSpell(Variables.Abilities.IceWall) &&
                                icewall.AbilityState == AbilityState.Ready && Variables.Invoke.CanBeCasted())
                            {
                                Functions.PrepareSpell(Variables.Abilities.IceWall);
                            }

                            else if (Functions.HasInvokerSpell(Variables.Abilities.IceWall) && icewall.CanBeCasted() &&
                                     Utils.SleepCheck("KaelSharp.IceWall"))
                            {
                                if (Prediction.InFront(Variables.Me, 200).Distance2D(Variables.Target) < 105)
                                {
                                    icewall.UseAbility();
                                    Utils.Sleep(100, "KaelSharp.IceWall");
                                }
                            }

                            else if (!Functions.HasInvokerSpell(Variables.Abilities.SunStrike) &&
                                     sunstrike.AbilityState == AbilityState.Ready && Variables.Invoke.CanBeCasted())
                            {
                                Functions.PrepareSpell(Variables.Abilities.SunStrike);
                            }

                            else if (Functions.HasInvokerSpell(Variables.Abilities.SunStrike) && sunstrike.CanBeCasted() &&
                                     Utils.SleepCheck("KaelSharp.Sunstrike"))
                            {
                                sunstrike.CastSkillShot(Variables.Target, Variables.Me.NetworkPosition);
                                Utils.Sleep(100, "KaelSharp.Sunstrike");
                            }
                        }

                        else if (Variables.Target.Distance2D(Variables.Me) > 350)
                        {
                            if (!Functions.HasInvokerSpell(Variables.Abilities.ColdSnap) &&
                                coldsnap.AbilityState == AbilityState.Ready && Variables.Invoke.CanBeCasted())
                            {
                                Functions.PrepareSpell(Variables.Abilities.ColdSnap);
                            }

                            else if (Functions.HasInvokerSpell(Variables.Abilities.ColdSnap) && coldsnap.CanBeCasted() &&
                                     Utils.SleepCheck("KaelSharp.ColdSnap"))
                            {
                                coldsnap.UseAbility(Variables.Target);
                                Utils.Sleep(100, "KaelSharp.ColdSnap");
                            }

                            else if (!Functions.HasInvokerSpell(Variables.Abilities.ForgeSpirit) &&
                                     forgespirit.AbilityState == AbilityState.Ready && Variables.Invoke.CanBeCasted())
                            {
                                Functions.PrepareSpell(Variables.Abilities.ForgeSpirit);
                            }

                            else if (Functions.HasInvokerSpell(Variables.Abilities.ForgeSpirit) && forgespirit.CanBeCasted() &&
                                     Utils.SleepCheck("KaelSharp.Spirit"))
                            {
                                forgespirit.UseAbility();
                                Utils.Sleep(100, "KaelSharp.Spirit");
                            }

                            else if (!Functions.HasInvokerSpell(Variables.Abilities.Alacrity) &&
                                     alacrity.AbilityState == AbilityState.Ready && Variables.Invoke.CanBeCasted())
                            {
                                Functions.PrepareSpell(Variables.Abilities.Alacrity);
                            }

                            else if (Functions.HasInvokerSpell(Variables.Abilities.Alacrity) && alacrity.CanBeCasted() &&
                                     Utils.SleepCheck("KaelSharp.Alacrity"))
                            {
                                alacrity.UseAbility(Variables.Me);
                                Utils.Sleep(100, "KaelSharp.Alacrity");
                            }
                        }
                    }

                }
            }

            if (chaosmeteor.AbilityState != AbilityState.Ready && emp.AbilityState != AbilityState.Ready &&
                deafeningblast.AbilityState != AbilityState.Ready && tornado.AbilityState != AbilityState.Ready &&
                sunstrike.AbilityState != AbilityState.Ready)
            {
                var refresher = Variables.Me.FindItem("item_refresher");
                if (refresher.CanBeCasted() && Utils.SleepCheck("KaelSharp.Refresh"))
                {
                    refresher.UseAbility();
                    Utils.Sleep(100, "KaelSharp.Refresh");
                }
            }

        }

        public static void Prepare()
        {
            var firstInvoke = Variables.Me.AghanimState()
                ? Variables.Abilities.Emp
                : (Variables.Wex.Level >= Variables.Exort.Level
                    ? Variables.Abilities.Emp
                    : Variables.Abilities.ChaosMeteor);
            const Variables.Abilities secondInvoke = Variables.Abilities.Tornado;

            var ability1 = Functions.GetSequence(firstInvoke);
            var ability2 = Functions.GetSequence(secondInvoke);

            if (ability1 == null || ability2 == null) return;

            if (Variables.Invoke.CanBeCasted())
            {
                var isOnF1 = new bool();
                var isOnF2 = new bool();

                if (Functions.HasInvokerSpell(ability2, out isOnF2) && !Functions.HasInvokerSpell(ability1, out isOnF1))
                {
                    if (isOnF2)
                    {
                        foreach (var ability in ability2)
                        {
                            ability.UseAbility();
                        }
                        Functions.Invoke();
                    }
                    else
                    {
                        foreach (var ability in ability1)
                        {
                            ability.UseAbility();
                        }
                        Functions.Invoke();
                    }
                }

                else if (!Functions.HasInvokerSpell(ability2, out isOnF2))
                {
                    if (Functions.HasInvokerSpell(ability1, out isOnF1))
                    {
                        if (isOnF1)
                        {
                            foreach (var ability in ability1)
                            {
                                ability.UseAbility();
                            }
                            Functions.Invoke();
                        }
                        else
                        {
                            foreach (var ability in ability2)
                            {
                                ability.UseAbility();
                            }
                            Functions.Invoke();
                        }
                    }
                    else
                    {
                        foreach (var ability in ability1)
                        {
                            ability.UseAbility();
                        }
                        Functions.Invoke();
                    }
                }
            }
        }

        public static void Flee()
        {
            var onF = new bool();

            if (!Functions.HasInvokerSpell(Variables.Abilities.GhostWalk, out onF) && Variables.Invoke.CanBeCasted() &&
                Utils.SleepCheck("KaelSharp.GhostWalk") && Utils.SleepCheck("KaelSharp.OrbSleep"))
            {

                var seq = Functions.GetSequence(Variables.Abilities.GhostWalk);

                foreach (var orb in seq)
                {
                    orb.UseAbility();
                    Utils.Sleep(100, "KaelSharp.OrbSleep");
                }
                Functions.Invoke();
            }

            else
            {
                var ghostWalk = Variables.Me.FindSpell("invoker_ghost_walk");

                if (ghostWalk.CanBeCasted() && Utils.SleepCheck("KaelSharp.GhostWalk"))
                {
                    Variables.Wex.UseAbility();
                    Variables.Wex.UseAbility();
                    Variables.Wex.UseAbility();

                    ghostWalk.UseAbility();
                    Utils.Sleep(100, "KaelSharp.GhostWalk");
                }
                if (Variables.Me.HasModifier("modifier_invoker_ghost_walk_self") || Utils.SleepCheck("KaelSharp.GWalk")) return;
                Variables.Me.Move(Game.MousePosition);
                Utils.Sleep(20, "KaelSharp.GWalk");
            }
        }

        public static void Cancer()
        {
            Functions.CancerInvoke();
        }

        public static void DisableRightClicker()
        {
            // todo
        }

        public static void SunStrike()
        {
            var sunstrike = Variables.Me.FindSpell(Functions.EnumToString(Variables.Abilities.SunStrike));
            var sunstrikeDamage = sunstrike.GetAbilityData("damage");

            var disabledEnemy = ObjectManager.GetEntitiesParallel<Hero>().FirstOrDefault(x => x.IsValid && x.Team != Variables.Me.Team && !x.IsIllusion && x.IsAlive && Functions.IsOnTiming(sunstrike, x) && !x.HasModifier("modifier_invoker_cold_snap") && x.Health <= sunstrikeDamage);

            if (disabledEnemy != null)
            {
                var onF = new bool();
                if (!Functions.HasInvokerSpell(Variables.Abilities.SunStrike, out onF) && Variables.Invoke.CanBeCasted() &&
                Utils.SleepCheck("KaelSharp.SunStrike"))
                {

                    var seq = Functions.GetSequence(Variables.Abilities.SunStrike);

                    foreach (var orb in seq)
                    {
                        orb.UseAbility();
                    }

                    Functions.Invoke();
                }

                else
                {
                    if (sunstrike.AbilityState == AbilityState.Ready && sunstrike.CanBeCasted() && disabledEnemy.IsValid && Utils.SleepCheck("KaelSharp.SunStrike"))
                    {
                        sunstrike.UseAbility(disabledEnemy.NetworkPosition, false);
                        Utils.Sleep(500, "KaelSharp.SunStrike");
                    }
                }
            }

            
        }
    }
}
