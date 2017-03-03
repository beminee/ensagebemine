using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EzGoldSharp.UnitManager;

namespace EzGoldSharp.EventManager
{
    using Ensage;
    using Ensage.Common;
    using Ensage.Common.Extensions;
    using System;
    using System.Linq;

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

           AllyHeroes.EnemyHeroes.Update();
           AllyHeroes.Update();

           Variables.Q = Variables.Me.Spellbook.SpellQ;
           Variables.W = Variables.Me.Spellbook.SpellW;
           Variables.E = Variables.Me.Spellbook.SpellE;
           Variables.R = Variables.Me.Spellbook.SpellR;

           double apoint = Variables.Me.ClassID == ClassID.CDOTA_Unit_Hero_ArcWarden
                ? 0.3
                : UnitDatabase.Units.Find(x => x.UnitName == Variables.Me.Name).AttackPoint;
           Variables.HeroAPoint = apoint / (1 + Variables.Me.AttacksPerSecond * Variables.Me.BaseAttackTime / 100) * 1000;

           MenuLoader.MenuLoader.Update();

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
