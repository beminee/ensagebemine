using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage;
using HuskarSharp.Features;
using HuskarSharp.Utilities;
using HuskarSharp.Abilities;
using HuskarSharp.Features.Orbwalk;

namespace HuskarSharp
{
    public class HuskarSharp
    {
        public HuskarSharp()
        {
            this.combo = new Combo();
        }

        private static Hero Me
        {
            get
            {
                return Variables.Hero;
            }
        }

        private static List<Unit> Illusions
        {
            get
            {
                return Variables.Illusions;
            }
        }

        private bool pause;

        private Combo combo;

        private AutoArmlet autoArmlet;

        private AutoLifeBreak autoLifebreak;

        private AutoPike autoPike;

        public TargetFind targetFind;

        private DrawText drawText;

        public Hero Target 
        {
            get
            {
                return this.targetFind.Target;
            }
        }

        public void OnDraw()
        {
            if (this.pause || Variables.Hero == null || !Variables.Hero.IsValid || !Variables.Hero.IsAlive)
            {
                return;
            }
            drawText.DrawTextCombo(Variables.ComboOn);
            if (Variables.ComboOn)
            {
                this.targetFind.DrawTarget();
            }
            combo.DrawTarget(Target);
        }

        public void OnLoad()
        {
            Variables.Hero = ObjectManager.LocalHero;
            Orbwalker.Hero = ObjectManager.LocalHero;
            pause = Variables.Hero.Name != "npc_dota_hero_huskar";
            if (this.pause) return;
            Variables.Hero = ObjectManager.LocalHero;
            Variables.MenuManager = new MenuManager(Me.Name);
            Variables.MenuManager.Menu.AddToMainMenu();
            Variables.EnemyTeam = Me.GetEnemyTeam();
            Variables.LifeBreak = new LifeBreak(Me.Spellbook.SpellR);
            Variables.Pike = new Pike(Me.FindItem("item_hurricane_pike"));
            Variables.Illusions = ObjectManager.GetEntities<Unit>().Where(unit => unit.Name == "npc_dota_hero_huskar").ToList();
            this.targetFind = new TargetFind();
            this.combo = new Combo();
            this.drawText = new DrawText();
            this.autoArmlet = new AutoArmlet();
            this.autoLifebreak = new AutoLifeBreak();
            this.autoPike = new AutoPike();


            Game.PrintMessage(
                "HuskarSharp" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version + " is loaded!",
                MessageType.LogMessage);
        }

        public void OnUpdate_Combo()
        {
            if (this.pause || Variables.Hero == null || !Variables.Hero.IsValid || !Variables.Hero.IsAlive)
            {
                return;
            }
            if (!Variables.ComboOn) return;
        }

        public void OnUpdate_AutoArmlet()
        {
            if (this.pause || Variables.Hero == null || !Variables.Hero.IsValid || !Variables.Hero.IsAlive)
            {
                return;
            }
            autoArmlet.Execute();
        }

        public void OnUpdate_autoLifebreak()
        {
            if (this.pause || Variables.Hero == null || !Variables.Hero.IsValid || !Variables.Hero.IsAlive)
            {
                return;
            }
            autoLifebreak.Execute();
        }


        public void OnUpdate_AutoPike()
        {
            if (this.pause || Variables.Hero == null || !Variables.Hero.IsValid || !Variables.Hero.IsAlive)
            {
                return;
            }
            autoPike.Execute();
        }

        public void Player_OnExecuteOrder(Player sender, ExecuteOrderEventArgs args)
        {
            if (this.pause || Variables.Hero == null || !Variables.Hero.IsValid || !Variables.Hero.IsAlive)
            {
                return;
            }
            autoArmlet.PlayerExecution_Armlet(args);
            if (Target == null) return;
            if (args.Order == Order.AttackTarget)
            {
                this.targetFind.UnlockTarget();
                this.targetFind.Find();
            }
            else
            {
                this.targetFind.UnlockTarget();
            }
        }




        public void OnClose()
        {
            this.pause = true;
            if (Variables.MenuManager != null)
            {
                Variables.MenuManager.Menu.RemoveFromMainMenu();
            }
            Variables.PowerTreadsSwitcher = null;
        }

        public void Event_OnUpdate(EventArgs e)
        {
            if (this.pause || Variables.Hero == null || !Variables.Hero.IsValid || !Variables.Hero.IsAlive)
            {
                return;
            }
            if (!Variables.ComboOn) return;
            Variables.Illusions = ObjectManager.GetEntities<Unit>().Where(unit => unit.Name == "npc_dota_hero_huskar" && unit.IsIllusion).ToList();

            if (Illusions == null) return;
            this.targetFind.Find();
            if (Target == null) return;
            this.targetFind.LockTarget();
            combo.SetTarget(Target);
            combo.Events_OnUpdate();


        }

    }
}
