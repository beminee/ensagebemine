using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common;

namespace HuskarSharp
{
    public class Bootstrap
    {
        private readonly HuskarSharp HuskarSharp;

        public Bootstrap()
        {
            this.HuskarSharp = new HuskarSharp();
        }

        public void SubscribeEvents()
        {
            Events.OnLoad += this.Events_Onload;
            Events.OnUpdate += this.Events_Update;
            Events.OnClose += this.Events_OnClose;
            Game.OnUpdate += this.Game_OnUpdate;
            //Game.OnWndProc += this.Game_OnWndProc;
            Drawing.OnDraw += this.Drawing_OnDraw;
            Player.OnExecuteOrder += this.Player_OnExecuteOrder;
        }

        private void Events_Update(EventArgs args)
        {
            this.HuskarSharp.Event_OnUpdate(args);
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            this.HuskarSharp.OnDraw();
        }

        private void Events_Onload(object sender, EventArgs e)
        {
            this.HuskarSharp.OnLoad();
        }

        private void Events_OnClose(object sender, EventArgs e)
        {
            this.HuskarSharp.OnClose();
        }

        private void Game_OnUpdate(EventArgs args)
        {
            this.HuskarSharp.OnUpdate_Combo();
            this.HuskarSharp.OnUpdate_AutoArmlet();
            this.HuskarSharp.OnUpdate_autoLifebreak();
        }

        private void Player_OnExecuteOrder(Player sender, ExecuteOrderEventArgs args)
        {
            if (sender.Equals(ObjectManager.LocalPlayer))
            {
                this.HuskarSharp.Player_OnExecuteOrder(sender, args);
            }
        }

    }
}