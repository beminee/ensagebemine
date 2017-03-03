using System;
using EzGoldSharp.EventManager;
using EzGoldSharp.UnitManager;

namespace EzGoldSharp
{
    using Ensage;
    using Ensage.Common;
    using Ensage.Common.Extensions;
    using SharpDX;
    using System;
    using System.Linq;
    internal class EzGoldSharp
    {
        public static void Game_OnUpdate(EventArgs args)
        {
            if (!Updater.CanUpdate()) return;

            Variables.Seconds = (int)Game.GameTime % 60;


            if (MenuLoader.MenuVariables.LastHitEnable)
            {
                if (MenuLoader.MenuVariables.Test)
                    Lasthit.Attack_Calc();

                if (Game.IsKeyDown(MenuLoader.MenuVariables.LastHitKey))
                {
                    Lasthit.LastHit();
                }
                else if (Game.IsKeyDown(MenuLoader.MenuVariables.FarmKey))
                {
                    Lasthit.Farm();
                }
                else
                {
                    if (!Variables.AutoAttackTypeDef)
                    {
                        Variables.Me.Hold();
                        Variables.Autoattack(MenuLoader.MenuVariables.AutoAttackMode);
                        Variables.DisableAaKeyPressed = false;
                        Variables.AutoAttackTypeDef = true;
                    }
                    Variables.CreeptargetH = null;
                }
            }
        }


        public static void Init()
        {
            Events.OnLoad += EventManager.EventManager.OnLoad;
            Events.OnClose += EventManager.EventManager.OnClose;
        }
    }
}
