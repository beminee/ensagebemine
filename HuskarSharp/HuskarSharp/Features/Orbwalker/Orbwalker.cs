using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuskarSharp.Features.Orbwalk
{
    public class Orbwalker : AttackAnimationTracker
    {
        #region Fields

        /// <summary>
        ///     The attacker.
        /// </summary>
        private readonly Attacker attacker;

        /// <summary>
        ///     The attack sleeper.
        /// </summary>
        private readonly Sleeper attackSleeper;

        /// <summary>
        ///     The attack sleeper 2.
        /// </summary>
        private readonly Sleeper attackSleeper2;

        /// <summary>
        ///     The move sleeper.
        /// </summary>
        private readonly Sleeper moveSleeper;

        public static Hero Hero { get; set; }
        /// <summary>
        ///     The hero.
        /// </summary>
        private bool hero;

        private float angle;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="AttackAnimationTracker" /> class.
        /// </summary>
        /// <param name="unit">
        ///     The unit.
        /// </param>
        public Orbwalker(Unit unit)
            : base(unit)
        {
            this.hero = unit.Equals(ObjectManager.LocalHero);
            this.attackSleeper = new Sleeper();
            this.moveSleeper = new Sleeper();
            this.attackSleeper2 = new Sleeper();
            this.attacker = new Attacker(unit);
        }

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
        public void Attack(Unit target, bool useModifier)
        {
            this.attacker.Attack(target, useModifier);
        }

        /// <summary>
        ///     The orbwalk.
        /// </summary>
        /// <param name="target">
        ///     The target.
        /// </param>
        /// <param name="bonusWindupMs">
        ///     The bonus windup ms.
        /// </param>
        /// <param name="bonusRange">
        ///     The bonus range.
        /// </param>
        /// <param name="attackmodifiers">
        ///     The attackmodifiers.
        /// </param>
        /// <param name="followTarget">
        ///     The follow target.
        /// </param>
        public void OrbwalkOn(
            Unit target,
            float bonusWindupMs = 0,
            float bonusRange = 0,
            bool attackmodifiers = true,
            bool followTarget = false)
        {
            this.OrbwalkOn(target, Game.MousePosition, bonusWindupMs, bonusRange, attackmodifiers, followTarget);
        }

        /// <summary>
        ///     The orbwalk.
        /// </summary>
        /// <param name="target">
        ///     The target.
        /// </param>
        /// <param name="movePosition">
        ///     The move Position.
        /// </param>
        /// <param name="bonusWindupMs">
        ///     The bonus windup ms.
        /// </param>
        /// <param name="bonusRange">
        ///     The bonus range.
        /// </param>
        /// <param name="attackmodifiers">
        ///     The attackmodifiers.
        /// </param>
        /// <param name="followTarget">
        ///     The follow target.
        /// </param>
        public void OrbwalkOn(
            Unit target,
            Vector3 movePosition,
            float bonusWindupMs = 0,
            float bonusRange = 0,
            bool attackmodifiers = true,
            bool followTarget = false)
        {
            if (this.Unit == null || !this.Unit.IsValid)
            {
                return;
            }

            var targetHull = 0f;
            if (target != null)
            {
                targetHull = target.HullRadius;
            }

            float distance = 0;
            if (target != null)
            {
                var pos = Prediction.InFront(
                    this.Unit,
                    (float)((Game.Ping / 1000) + (this.Unit.GetTurnTime(target.Position) * this.Unit.MovementSpeed)));
                distance = pos.Distance2D(target) - this.Unit.Distance2D(target);
            }

            var isValid = target != null && target.IsValid && target.IsAlive && target.IsVisible && !target.IsInvul()
                          && !target.HasModifiers(
                              new[] { "modifier_ghost_state", "modifier_item_ethereal_blade_slow" },
                              false)
                          && target.Distance2D(this.Unit)
                          <= this.Unit.GetAttackRange() + this.Unit.HullRadius + 50 + targetHull + bonusRange
                          + Math.Max(distance, 0);
            if (isValid || (target != null && this.Unit.IsAttacking() && this.Unit.GetTurnTime(target.Position) < 0.1))
            {
                var canAttack = this.CanAttack(target, bonusWindupMs) && !target.IsAttackImmune() && !target.IsInvul()
                                && this.Unit.CanAttack();
                if (canAttack && !this.attackSleeper.Sleeping && (!this.hero || Utils.SleepCheck("Orbwalk.Attack")))
                {
                    this.attacker.Attack(target, attackmodifiers);
                    this.attackSleeper.Sleep(
                        (float)
                        ((UnitDatabase.GetAttackPoint(this.Unit) * 1000) + (this.Unit.GetTurnTime(target) * 1000)
                         + Game.Ping + 100));
                    this.moveSleeper.Sleep(
                        (float)
                        ((UnitDatabase.GetAttackPoint(this.Unit) * 1000) + (this.Unit.GetTurnTime(target) * 1000) + 50));
                    if (!this.hero)
                    {
                        return;
                    }

                    Utils.Sleep(
                        (UnitDatabase.GetAttackPoint(this.Unit) * 1000) + (this.Unit.GetTurnTime(target) * 1000)
                        + Game.Ping + 100,
                        "Orbwalk.Attack");
                    Utils.Sleep(
                        (UnitDatabase.GetAttackPoint(this.Unit) * 1000) + (this.Unit.GetTurnTime(target) * 1000) + 50,
                        "Orbwalk.Move");
                    return;
                }

                if (canAttack && !this.attackSleeper2.Sleeping)
                {
                    this.attacker.Attack(target, attackmodifiers);
                    this.attackSleeper2.Sleep(100);
                    return;
                }
            }

            var canCancel = (this.CanCancelAttack() && !this.CanAttack(target, bonusWindupMs))
                            || (!isValid && !this.Unit.IsAttacking() && this.CanCancelAttack());
            if (!canCancel || this.moveSleeper.Sleeping || this.attackSleeper.Sleeping
                || (this.hero && (!Utils.SleepCheck("Orbwalk.Move") || !Utils.SleepCheck("Orbwalk.Attack"))))
            {
                return;
            }

            var angle =
                    Math.Abs(target.FindAngleR() -
                             Utils.DegreeToRadian(target.FindAngleForTurnTime(Hero.NetworkPosition)));

            if (followTarget && Unit.Distance2D(target) >= 400 && Utils.SleepCheck("xd"))
            {
                this.Unit.Move(Prediction.InFront(this.Unit, 100));
            }
                
                
            else if (Unit.Distance2D(target) <= 400 || angle <= 0.03)
            {
                this.Unit.Move(Behind(this.Unit, 400));
                Utils.Sleep(100, "xd");
            }

            this.moveSleeper.Sleep(100);
        }

        public static Vector3 Behind(Unit unit, float distance)
        {
            var v = unit.Position + unit.Vector3FromPolarAngle() * distance;
            return new Vector3(-v.X, -v.Y, 0);
        }
        #endregion
    }
}