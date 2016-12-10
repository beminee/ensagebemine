using Ensage;
using Ensage.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuskarSharp.Abilities
{
    public class Satanic
    {
        private Ability ability;

        public Satanic(Ability ability)
        {
            this.ability = ability;
        }

        public bool CanbeCastS()
        {
            return this.ability.CanBeCasted();
        }

        public void UseOn()
        {
            this.ability.UseAbility();
        }
    }
}