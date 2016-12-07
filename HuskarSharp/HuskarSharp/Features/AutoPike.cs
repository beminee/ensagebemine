using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HuskarSharp.Abilities;

namespace HuskarSharp.Features
{
    public class AutoPike
    {
        private Hero me
        {
            get
            {
                return Variables.Hero;
            }
        }

        private Pike pike
        {
            get
            {
                return Variables.Pike;
            }
        }

        private Hero pikeTarget;



        public void Execute()
        {

            if (!pike.CanbeCastP()) return;
            if (me.CanCast())
            {
                FindPikeTarget();
                if (pikeTarget == null) return;
                if (this.pikeTarget.Distance2D(me) <= 400)
                    if (Utils.SleepCheck("pike"))
                    {
                        Console.WriteLine("1");
                        pike.UseOn(this.pikeTarget);
                        Utils.Sleep(300, "pike");
                    }
            }
        }

        private void FindPikeTarget()
        {
            var target = ObjectManager.GetEntities<Hero>().Where(x =>
                                       x.Team != Variables.Hero.Team && x.IsAlive
                                        && x.Distance2D(me) <= 900)
                                       .OrderByDescending(x => x.Distance2D(me)).FirstOrDefault();
            if (target == null) return;
            this.pikeTarget = target;
        }


    }
}
