using EzGoldSharp.MenuLoader;
using EzGoldSharp.UnitManager;
using SharpDX;

namespace EzGoldSharp.EventManager
{
    using Ensage;
    using Ensage.Common;
    using System;

   internal class Updater
    {
       public static bool CanUpdate()
       {
           if (!Game.IsPaused && Game.IsInGame && Variables.Me != null && Variables.Me.IsValid && !Game.IsChatOpen)
               return true;
           else
           {
               return false;
           }
       }

       public static void Update(EventArgs args)
       {
           if (!CanUpdate()) return;

           EnemyHeroes.Update();
           AllyHeroes.Update();

           Variables.Q = Variables.Me.Spellbook.SpellQ;
           Variables.W = Variables.Me.Spellbook.SpellW;
           Variables.E = Variables.Me.Spellbook.SpellE;
           Variables.R = Variables.Me.Spellbook.SpellR;

           double apoint = Variables.Me.ClassId == ClassId.CDOTA_Unit_Hero_ArcWarden
                ? 0.3
                : UnitDatabase.Units.Find(x => x.UnitName == Variables.Me.Name).AttackPoint;
           Variables.HeroAPoint = apoint / (1 + Variables.Me.AttacksPerSecond * Variables.Me.BaseAttackTime / 100) * 1000;

           MenuLoader.MenuLoader.Update();

            if (MenuVariables.ShowAttackRange)
            {
                if (Variables.AttackRange == null)
                {
                    if (Variables.Me.IsAlive)
                    {
                        Variables.AttackRange = Variables.Me.AddParticleEffect(@"particles\ui_mouseactions\drag_selected_ring.vpcf");
                        Variables.AttackRange.SetControlPoint(1, new Vector3(255, 80, 50));
                        Variables.AttackRange.SetControlPoint(3, new Vector3(20, 0, 0));
                        Variables.AttackRange.SetControlPoint(2, new Vector3(Variables.LastRange, 255, 0));
                    }
                }
                else
                {
                    if (!Variables.Me.IsAlive)
                    {
                        Variables.AttackRange.Dispose();
                        Variables.AttackRange = null;
                    }
                    else if (Variables.LastRange != MyHero.AttackRange())
                    {
                        Variables.AttackRange.Dispose();
                        Variables.LastRange = MyHero.AttackRange();
                        Variables.AttackRange = Variables.Me.AddParticleEffect(@"particles\ui_mouseactions\drag_selected_ring.vpcf");
                        Variables.AttackRange.SetControlPoint(1, new Vector3(255, 80, 50));
                        Variables.AttackRange.SetControlPoint(3, new Vector3(15, 0, 0));
                        Variables.AttackRange.SetControlPoint(2, new Vector3(Variables.LastRange, 255, 0));
                    }
                }
            }
            else if (!MenuVariables.ShowAttackRange)
            {
                if (Variables.AttackRange != null) Variables.AttackRange.Dispose();
                Variables.AttackRange = null;
            }


            if (Variables.AutoAttackMode != MenuLoader.MenuVariables.AutoAttackMode)
           {
               switch (MenuLoader.MenuVariables.AutoAttackMode)
               {
                   case 0:
                       Variables.AutoAttackMode = 0;
                       Variables.Autoattack(Variables.AutoAttackMode);
                       break;

                   case 1:
                       Variables.AutoAttackMode = 1;
                       Variables.Autoattack(Variables.AutoAttackMode);
                       break;

                   case 2:
                       Variables.AutoAttackMode = 2;
                       Variables.Autoattack(Variables.AutoAttackMode);
                       break;
               }
           }
       }
        

    }
}
