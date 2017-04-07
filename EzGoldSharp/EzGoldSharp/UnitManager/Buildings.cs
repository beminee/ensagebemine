using Ensage.Common.Extensions;

namespace EzGoldSharp.UnitManager
{
    using Ensage;
    using System.Collections.Generic;
    using System.Linq;

    internal class Buildings
    {
        #region Fields

        public static List<Unit> AllyTowers;

        public static List<Unit> EnemyTowers;

        public static Unit AllyFountain;

        public static Unit EnemyFountain;

        #endregion Fields

        #region Methods

        public static List<Unit> GetAllyBuildings()
        {
            AllyTowers = ObjectManager.GetEntitiesParallel<Unit>().Where(x => x.IsValid && x.IsAlive && x is Tower && ReferenceEquals(((Tower)x).AttackTarget, (Unit)Variables.CreeptargetH) && x.Team == Variables.Me.Team && x.MinimumDamage == 100 && x.Distance2D(Variables.CreeptargetH) <= x.AttackRange).ToList();
            return AllyTowers;
        }

        public static List<Unit> GetEnemyBuildings()
        {
            EnemyTowers = ObjectManager.GetEntitiesParallel<Unit>().Where(x => x.IsValid && x.IsAlive && x is Tower && ReferenceEquals(((Tower) x).AttackTarget, (Unit) Variables.CreeptargetH) && x.Team != Variables.Me.Team && x.MinimumDamage == 100 && x.Distance2D(Variables.CreeptargetH) <= x.AttackRange).ToList();

            return EnemyTowers;
        }

        #endregion Methods
    }
}
