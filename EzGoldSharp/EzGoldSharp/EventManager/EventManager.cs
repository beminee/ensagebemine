using EzGoldSharp.UnitManager;

namespace EzGoldSharp.EventManager
{
    using Ensage;
    using Ensage.Common;
    using System;
    using System.Collections.Generic;

    internal class EventManager
    {

        public static void OnLoad(object sender, EventArgs e)
        {
            if (Variables.Loaded || ObjectManager.LocalHero == null)
            {
                return;
            }
            Variables.Me = ObjectManager.LocalHero;
            Variables.Loaded = true;
            EnemyHeroes.Heroes = new List<Hero>();
            EnemyHeroes.Illusions = new List<Hero>();
            AllyHeroes.Heroes = new List<Hero>();
            EnemyHeroes.UsableHeroes = new Hero[] { };
            AllyHeroes.UsableHeroes = new Hero[] { };
            AllyHeroes.AbilityDictionary = new Dictionary<float, List<Ability>>();
            EnemyHeroes.AbilityDictionary = new Dictionary<float, List<Ability>>();
            AllyHeroes.ItemDictionary = new Dictionary<float, List<Item>>();
            EnemyHeroes.ItemDictionary = new Dictionary<float, List<Item>>();

            MenuLoader.MenuLoader.Load();

            Game.OnUpdate += EzGoldSharp.Game_OnUpdate;
            Game.OnUpdate += Updater.Update;

            Orbwalking.Load();

            Game.PrintMessage("EzGoldSharp Loaded");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("EzGoldSharp Loaded");
            Console.ResetColor();
        }

        public static void OnClose(object sender, EventArgs e)
        {
            Game.OnUpdate -= EzGoldSharp.Game_OnUpdate;
            Game.OnUpdate -= Updater.Update;

            Variables.Loaded = false;
            MenuLoader.MenuLoader.UnLoad();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("EzGoldSharp Unloaded");
            Console.ResetColor();
        }
    }
}
