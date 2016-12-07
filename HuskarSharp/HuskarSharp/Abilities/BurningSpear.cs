using HuskarSharp.Utilities;
using Ensage;
using Ensage.Common.Extensions;

namespace HuskarSharp.Abilities
{
    public class BurningSpear
    {
        private Ability ability;

        public BurningSpear(Ability ability)
        {
            this.ability = ability;
        }

        public bool CanbeCastF()
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