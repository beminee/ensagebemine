using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common.Menu;
using EzGoldSharp.EventManager;
using EzGoldSharp.UnitManager;
using SharpDX;

namespace EzGoldSharp
{
   internal class Draws
    {
        public static void OnLoad()
        {
            //
        }

        public static void Drawing(EventArgs args)
        {
            if (!Updater.CanUpdate()) return;

            #region HpBar

            if (!MenuLoader.MenuVariables.LastHitEnable) return;
            if (MenuLoader.MenuVariables.HpBar)
                Lasthit.Drawhpbar();

            #endregion HpBar
        }

    }
}
