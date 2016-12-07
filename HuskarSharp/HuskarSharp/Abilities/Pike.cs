using Ensage;
using Ensage.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuskarSharp.Abilities
{
    public class Pike
    {
        private Ability ability;

        public Pike(Ability ability)
        {
            this.ability = ability;
        }

        public bool CanbeCastP()
        {
            return this.ability.CanBeCasted();
        }

        public void UseOn(Hero target)
        {
            if (target == null) return;
            this.ability.UseAbility(target);
        }
    }
}