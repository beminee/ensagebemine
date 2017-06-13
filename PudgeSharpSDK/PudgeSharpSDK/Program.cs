using Ensage.SDK.Orbwalker.Modes;

namespace PudgeSharpSDK
{
    using System;
    using Ensage;

    internal class Program
    {
        public static void Main()
        {
            Game.OnIngameUpdate += OnLoad;
        }

        private static void OnLoad(EventArgs args)
        {
            if (ObjectManager.LocalHero == null || ObjectManager.LocalHero.ClassId != ClassId.CDOTA_Unit_Hero_Pudge)
            {
                return;
            }

            Game.OnIngameUpdate -= OnLoad;
        }
    }
}
