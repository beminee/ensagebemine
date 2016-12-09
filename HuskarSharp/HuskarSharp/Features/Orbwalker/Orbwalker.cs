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
        ///     The counter 10 sleeper.
        /// </summary>
        private readonly Sleeper counter10Sleeper = new Sleeper();

        /// <summary>
        ///     The counter 10 sleeper 2.
        /// </summary>
        private readonly Sleeper counter10Sleeper2 = new Sleeper();

        /// <summary>
        ///     The counter sleeper.
        /// </summary>
        private readonly Sleeper counterSleeper = new Sleeper();

        public static Hero Hero { get; set; }

        /// <summary>
        ///     The hero.
        /// </summary>
        private readonly bool hero;

        /// <summary>
        ///     The move sleeper.
        /// </summary>
        private readonly Sleeper moveSleeper;

        /// <summary>
        ///     The set user delay manually.
        /// </summary>
        private readonly bool setUserDelayManually;

        /// <summary>
        ///     The attacks per second.
        /// </summary>
        private float attacksPerSecond;

        /// <summary>
        ///     The counter 10 result.
        /// </summary>
        private float counter10Result;

        /// <summary>
        ///     The counter 10 running.
        /// </summary>
        private bool counter10Running;

        /// <summary>
        ///     The counter 10 start.
        /// </summary>
        private float counter10Start;

        /// <summary>
        ///     The counter 10 started.
        /// </summary>
        private bool counter10Started;

        /// <summary>
        ///     The counter start.
        /// </summary>
        private float counterStart;

        /// <summary>
        ///     The current 10 count.
        /// </summary>
        private float current10Count;

        /// <summary>
        ///     The current count.
        /// </summary>
        private float currentCount;

        /// <summary>
        ///     The custom move position.
        /// </summary>
        private bool customMovePosition;

        /// <summary>
        ///     The enable debug.
        /// </summary>
        private bool enableDebug = true;

        /// <summary>
        ///     The last move position.
        /// </summary>
        private Vector3 lastMovePosition;

        /// <summary>
        ///     The moving when ready.
        /// </summary>
        private bool movingWhenReady;

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
            if (unit == null)
            {
                return;
            }

            this.hero = unit.Equals(ObjectManager.LocalHero);
            this.attackSleeper = new Sleeper();
            this.moveSleeper = new Sleeper();
            this.attackSleeper2 = new Sleeper();
            this.attacker = new Attacker(unit);
        }

        public Orbwalker(Unit unit, bool setUserDelayManually)
            : base(unit)
        {
            if (unit == null)
            {
                return;
            }

            this.setUserDelayManually = setUserDelayManually;
            this.hero = unit.Equals(ObjectManager.LocalHero);
            this.attackSleeper = new Sleeper();
            this.moveSleeper = new Sleeper();
            this.attackSleeper2 = new Sleeper();
            this.attacker = new Attacker(unit);
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the user delay.
        /// </summary>
        public float UserDelay { get; set; }

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
        ///     The attack end.
        /// </summary>
        new public void AttackEnd()
        {
            if (this.counter10Running)
            {
                this.current10Count += 1;
            }
        }

        /// <summary>
        ///     The attack start.
        /// </summary>
        new public void AttackStart()
        {
            DelayAction.Add(
                (int)(UnitDatabase.GetAttackRate(this.Unit) * 1000),
                () =>
                {
                    this.currentCount +=
                        (float)((Game.RawGameTime - this.counterStart) / UnitDatabase.GetAttackRate(this.Unit));
                });
            if (!this.counter10Sleeper2.Sleeping)
            {
                this.counter10Started = true;
            }

            if (this.movingWhenReady)
            {
                return;
            }

            if (this.moveSleeper.Sleeping || this.attackSleeper.Sleeping
                || (this.hero && (!Utils.SleepCheck("Orbwalk.Move") || !Utils.SleepCheck("Orbwalk.Attack"))))
            {
                this.movingWhenReady = true;
            }
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
                    (float)(Game.Ping / 1000 + this.Unit.GetTurnTime(target.Position) * this.Unit.MovementSpeed));
                distance = pos.Distance2D(target) - this.Unit.Distance2D(target);
            }

            var isValid = target != null && target.IsValid && target.IsAlive && target.IsVisible;
            var isAttackable = target != null && target.IsValid && !target.IsInvul() && !target.IsAttackImmune()
                               && !target.HasModifiers(
                                   new[] { "modifier_ghost_state", "modifier_item_ethereal_blade_slow" },
                                   false)
                               && target.Distance2D(this.Unit)
                               <= this.Unit.GetAttackRange() + this.Unit.HullRadius + 50 + targetHull + bonusRange
                               + Math.Max(distance, 0);
            if ((isValid && isAttackable)
                || (!isAttackable && target != null && target.IsValid && this.Unit.IsAttacking()
                    && this.Unit.GetTurnTime(target.Position) < 0.1))
            {
                var canAttack = !this.IsAttackOnCoolDown(target, bonusWindupMs) && this.Unit.CanAttack();
                if (canAttack && !this.attackSleeper.Sleeping && (!this.hero || Utils.SleepCheck("Orbwalk.Attack")))
                {
                    this.attacker.Attack(target, attackmodifiers);
                    this.AttackOrder();
                    this.attackSleeper.Sleep(
                        (float)
                        (UnitDatabase.GetAttackPoint(this.Unit) * 1000 + this.Unit.GetTurnTime(target) * 1000
                         + Game.Ping + 100));
                    this.moveSleeper.Sleep(
                        (float)
                        (UnitDatabase.GetAttackPoint(this.Unit) * 1000 + this.Unit.GetTurnTime(target) * 1000 + 50));
                    if (!this.hero)
                    {
                        return;
                    }

                    Utils.Sleep(
                        UnitDatabase.GetAttackPoint(this.Unit) * 1000 + this.Unit.GetTurnTime(target) * 1000 + Game.Ping
                        + 100,
                        "Orbwalk.Attack");
                    Utils.Sleep(
                        UnitDatabase.GetAttackPoint(this.Unit) * 1000 + this.Unit.GetTurnTime(target) * 1000 + 50,
                        "Orbwalk.Move");
                    return;
                }

                if (canAttack && !this.attackSleeper2.Sleeping)
                {
                    this.attacker.Attack(target, attackmodifiers);
                    this.AttackOrder();
                    this.attackSleeper2.Sleep(100);
                    return;
                }
            }

            var userdelay = this.setUserDelayManually ? this.UserDelay : UserDelay;
            var canCancel = (this.CanCancelAttack(userdelay) && this.IsAttackOnCoolDown(target, bonusWindupMs))
                            || ((!isValid || !isAttackable)
                                && (!this.Unit.IsAttacking() || this.CanCancelAttack(userdelay)));
            if (!canCancel || this.moveSleeper.Sleeping || this.attackSleeper.Sleeping
                || (this.hero && (!Utils.SleepCheck("Orbwalk.Move") || !Utils.SleepCheck("Orbwalk.Attack"))))
            {
                return;
            }

            if (followTarget && target != null)
            {
                var pos = target.NetworkActivity == NetworkActivity.Move
                              ? target.Predict(Game.Ping + 100)
                              : target.Position;
                this.Unit.Move(pos);
                this.customMovePosition = true;
                this.lastMovePosition = pos;
            }
            else
            {
                this.Unit.Move(movePosition);
                if (movePosition != Game.MousePosition)
                {
                    this.customMovePosition = true;
                    this.lastMovePosition = movePosition;
                }
                else
                {
                    this.customMovePosition = false;
                }
            }

            this.moveSleeper.Sleep(100);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     The move when ready.
        /// </summary>
        /// <param name="args">
        ///     The args.
        /// </param>
        private void MoveWhenReady(EventArgs args)
        {
            var userdelay = this.setUserDelayManually ? this.UserDelay : UserDelay;
            if (this.CanCancelAttack(userdelay))
            {
                this.Unit.Move(this.customMovePosition ? this.lastMovePosition : Game.MousePosition);
                this.movingWhenReady = false;
            }
        }

        #endregion
    }
}
