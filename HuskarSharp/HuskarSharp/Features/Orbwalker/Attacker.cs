using Ensage;
using Ensage.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuskarSharp.Features.Orbwalk
{
    public class Attacker
    {
        #region Fields

        /// <summary>
        ///     The attack.
        /// </summary>
        private readonly Action<Unit> attack;

        /// <summary>
        ///     The use modifiers.
        /// </summary>
        private bool useModifier;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Attacker" /> class.
        /// </summary>
        /// <param name="unit">
        ///     The unit.
        /// </param>
        public Attacker(Unit unit)
        {
            this.Unit = unit;
            if (Unit.Name == "npc_dota_hero_huskar")
            {
                this.AttackModifier = unit.Spellbook.Spell2;
                this.attack = (target) =>
                {
                    if (!target.IsMagicImmune())
                    {
                        useModifier = true;
                    }
                    else
                    {
                        useModifier = false;
                    }
                    if (this.useModifier && this.Unit.CanCast() && this.AttackModifier.Level > 0)
                    {
                        this.AttackModifier.UseAbility(target);
                        return;
                    }
                        this.Unit.Attack(target);

                };
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the attack modifier.
        /// </summary>
        public Ability AttackModifier { get; set; }

        /// <summary>
        ///     Gets or sets the unit.
        /// </summary>
        public Unit Unit { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The attack.
        /// </summary>
        /// <param name="target">
        ///     The target.
        /// </param>
        /// <param name="useModifier">
        ///     The use modifier.
        /// </param>
        public void Attack(Unit target, bool useModifier = true)
        {
            this.useModifier = useModifier;
            this.attack.Invoke(target);
        }

        #endregion
    }
}
