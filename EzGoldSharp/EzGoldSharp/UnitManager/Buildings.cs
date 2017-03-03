using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzGoldSharp.UnitManager
{
    using Ensage;
    using System.Collections.Generic;
    using System.Linq;

    internal class Buildings
    {
        #region Fields

        public static List<Unit> Towers;

        public static Unit AllyFountain;

        public static Unit EnemyFountain;

        #endregion Fields

        #region Methods

        public static void GetBuildings()
        {
            Towers = ObjectManager.GetEntities<Unit>().Where(x => x.IsAlive && (x.ClassID == ClassID.CDOTA_BaseNPC_Tower)).ToList();

            if (ObjectManager.GetEntities<Unit>().Any(x => x.ClassID == ClassID.CDOTA_Unit_Fountain && x.Team == Variables.Me.Team))
                AllyFountain = ObjectManager.GetEntities<Unit>().First(x => x.ClassID == ClassID.CDOTA_Unit_Fountain && x.Team == Variables.Me.Team);

            if (ObjectManager.GetEntities<Unit>().Any(x => x.ClassID == ClassID.CDOTA_Unit_Fountain && x.Team != Variables.Me.Team))
                EnemyFountain = ObjectManager.GetEntities<Unit>().First(x => x.ClassID == ClassID.CDOTA_Unit_Fountain && x.Team != Variables.Me.Team);
        }

        #endregion Methods
    }
}
