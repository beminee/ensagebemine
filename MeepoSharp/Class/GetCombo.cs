using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common.Extensions;
using Ensage.Heroes;

namespace MeepoSharp.Class
{
    public class GetCombo
    {
        public static Hero GetClosestToTarget(List<Hero> units, Hero target)
        {
            Hero closestHero = null;
            foreach (var v in units.Where(v => closestHero == null || closestHero.Distance2D(target) > v.Distance2D(target)))
            {
                closestHero = v;
            }
            return closestHero;
        }

       /* public static Unit GetClosestUnnagroCreep(List<Unit> units, Hero me)
        {
            Unit closestUnit = null;
            foreach (var v in units.Where(v => closestUnit == null || closestUnit.Distance2D(me) > v.Distance2D(me)))
            {
                closestUnit = v;
            }
            return closestUnit;
        }*/
    }
}
