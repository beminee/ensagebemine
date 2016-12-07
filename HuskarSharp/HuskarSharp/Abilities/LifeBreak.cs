using Ensage;
using Ensage.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuskarSharp.Abilities
{
    public class LifeBreak
    {
        private Ability ability;

        public LifeBreak(Ability ability)
        {
            this.ability = ability;
        }

        public bool CanbeCastL()
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