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
    public class AutoSatanic
    {
        private Hero me
        {
            get
            {
                return Variables.Hero;
            }
        }

        public Satanic Satanic
        {
            get
            {
                return Variables.Satanic;
            }
        }



        public void Execute()
        {

            if (!Satanic.CanbeCastS()) return;
            if (me.CanCast() && me.CanAttack() && Variables.ComboOn && Variables.SatanicThreshold <= me.Health)
            {
                    if (Utils.SleepCheck("satanic_HS"))
                    {
                        Satanic.UseOn();
                        Utils.Sleep(200, "satanic_HS");
                    }
            }
        }


    }
}
