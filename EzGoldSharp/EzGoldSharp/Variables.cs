using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common.Extensions;
using EzGoldSharp.UnitManager;
using MenuVariables = EzGoldSharp.MenuLoader.MenuVariables;

namespace EzGoldSharp
{
    internal class Variables
    {
        #region Variables
        public static int AutoAttackMode;
        public static bool AutoAttackTypeDef;
        public static List<Unit> Creeps;
        public static List<UnitDictionary> CreepsDic = new List<UnitDictionary>();
        public static List<uint> minionAttackPointList = new List<uint>();
        public static List<uint> minionAttackBackswingList = new List<uint>();
        public static Unit CreeptargetH;
        public static bool DisableAaKeyPressed;
        public static double HeroAPoint;
        public static bool Loaded;
        public static Hero Me;
        public static Ability Q, W, E, R;
        public static int Seconds;
        public static List<SleepDictionary> SleepDic = new List<SleepDictionary>();
        #endregion Variables

        public static void Autoattack(int a)
        {
            Game.ExecuteCommand("dota_player_units_auto_attack_mode " + a);
        }

        public static float GetOutRange(Unit unit)
        {
            if (unit.Handle == Me.Handle)
            {
                return MyHero.AttackRange() + MenuVariables.Outrange;
            }
            else
            {
                if (unit.IsRanged)
                    return 500 + MenuVariables.Outrange;
                else
                    return 200 + MenuVariables.Outrange;
            }
        }

        public static int GetAbilityDelay(Unit target, Ability ability)
        {
            return (int)((ability.FindCastPoint() + Me.GetTurnTime(target)) * 1000.0 + Game.Ping);
        }

        public static void UpdateCreeps()
        {
            try
            {
                Creeps = ObjectManager.GetEntitiesParallel<Unit>()
                    .Where(
                        x =>
                            (x.IsValid && x.ClassID == ClassID.CDOTA_BaseNPC_Tower ||
                             x.ClassID == ClassID.CDOTA_BaseNPC_Creep_Lane
                             || x.ClassID == ClassID.CDOTA_BaseNPC_Creep
                             || x.ClassID == ClassID.CDOTA_BaseNPC_Creep_Neutral
                             || x.ClassID == ClassID.CDOTA_BaseNPC_Creep_Siege
                             || x.ClassID == ClassID.CDOTA_BaseNPC_Additive
                             || x.ClassID == ClassID.CDOTA_BaseNPC_Barracks
                             || x.ClassID == ClassID.CDOTA_BaseNPC_Building
                             || x.ClassID == ClassID.CDOTA_BaseNPC_Creature) && x.IsAlive && x.IsVisible
                            && x.Distance2D(Me) < MenuVariables.Outrange + MyHero.AttackRange()).ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e + "Update Creeps Error");
            }
        }

    }

    public class KillableCreeps
    {
        #region Properties

        public int ACreeps { get; set; }
        public float Health { get; set; }
        public float Time { get; set; }

        #endregion Properties
    }

    public class UnitDictionary
    {
        #region Properties

        public List<KillableCreeps> KillableCreeps { get; set; }
        public Unit Unit { get; set; }

        #endregion Properties

        #region Methods

        public bool AHealth(Entity unit)
        {
            if (unit.Handle != Unit.Handle) return false;
            if (KillableCreeps.Any(x => x.Health - unit.Health < 10)) return true;
            KillableCreeps.Add(new KillableCreeps { Health = unit.Health, Time = Game.GameTime, ACreeps = Lasthit.Attack(unit) });
            return true;
        }

        #endregion Methods
    }

    public class SleepDictionary
    {
        #region Properties

        public float Period { get; set; }
        public string Text { get; set; }

        public long Time { get; set; }

        #endregion Properties
    }

    public class MinionAaData
    {
        #region Methods
        public static float GetAttackSpeed(Unit creep)
        {
            var attackSpeed = Math.Min(creep.AttacksPerSecond * 1 / 0.01, 600);

            return (float)attackSpeed;
        }

        public static float GetAttackPoint(Unit creep)
        {
            var animationPoint = 0f;

            var attackSpeed = GetAttackSpeed(creep);

            animationPoint = creep.IsRanged ? 0.5f : 0.467f;

            return animationPoint / (1 + (attackSpeed - 100) / 100);
        }

        public static float GetAttackRate(Unit creep)
        {
            var attackSpeed = GetAttackSpeed(creep);

            return 1 / (1 + (attackSpeed - 100) / 100);
        }

        public static float GetAttackBackswing(Unit creep)
        {
            var attackRate = GetAttackRate(creep);

            var attackPoint = GetAttackPoint(creep);

            return attackRate - attackPoint;
        }

        public static bool StartedAttack(Unit unit)
        {
            return unit.IsAttacking() && !Variables.minionAttackPointList.Contains(unit.Handle) && !Variables.minionAttackBackswingList.Contains(unit.Handle);
        }
        #endregion Methods
    }

    public class TowerData
    {
        public static float GetAttackSpeed(Unit tower)
        {
            var attackSpeed = tower.AttackSpeedValue;
            return attackSpeed;
        }
    }
}
