using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ensage;

using global::HuskarSharp.Abilities;
using global::HuskarSharp.Utilities;
using HuskarSharp.Features;

namespace HuskarSharp
{
    public class Variables
    {


        public static bool ComboOn
        {
            get
            {
                return MenuManager.ComboOn;
            }
        }

        public static int ArmletThreshold
        {
            get
            {
                return MenuManager.HP;
            }
        }

        public static int LifeBreakThreshold
        {
            get
            {
                return MenuManager.LBThreshold;
            }
        }

        public static int SatanicThreshold
        {
            get
            {
                return MenuManager.SThreshold;
            }
        }




        public static Hero Hero { get; set; }

        public static List<Unit> Illusions { get; set; }

        public static BurningSpear BurningSpear;

        public static LifeBreak LifeBreak;

        public static Pike Pike;

        public static Satanic Satanic;

        public static Team EnemyTeam { get; set; }

        public static MenuManager MenuManager { get; set; }

        public static PowerTreadsSwitcher PowerTreadsSwitcher { get; set; }

        public static float TickCount
        {
            get
            {
                return Environment.TickCount & int.MaxValue;
            }
        }
    }
}
