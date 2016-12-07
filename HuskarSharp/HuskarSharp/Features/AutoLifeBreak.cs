using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ensage.Common.Menu;
using HuskarSharp.Abilities;
using HuskarSharp.Features.Orbwalk;
using HuskarSharp.Utilities;

namespace HuskarSharp.Features
{
    public class AutoLifeBreak
    {
        private Hero me
        {
            get
            {
                return Variables.Hero;
            }
        }

        public LifeBreak Lifebreak
        {
            get
            {
                return Variables.LifeBreak;
            }
        }

        public Hero lifebreakTarget;



        public void Execute()
        {

            if (!Lifebreak.CanbeCastL()) return;
            if (me.CanCast() && Variables.ComboOn)
            {
                FindLifeBreakTarget();
                if (lifebreakTarget == null) return;
                if (this.lifebreakTarget.Distance2D(me) <= 550)
                if (Utils.SleepCheck("lifebreak"))
                {
                    Lifebreak.UseOn(this.lifebreakTarget);
                    Utils.Sleep(200, "lifebreak");
                }
            }
        }

        public void FindLifeBreakTarget()
        {
            var target = ObjectManager.GetEntities<Hero>().Where(x =>
                                       x.Team != Variables.Hero.Team && x.IsAlive && !x.IsIllusion
                                        && x.Distance2D(me) <= 500)
                                       .OrderByDescending(x => x.Distance2D(me)).FirstOrDefault();
            if (target == null) return;
            this.lifebreakTarget = target;
        }


    }
}
