using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ensage.Common.Menu;

namespace EzGoldSharp.MenuLoader
{
    internal class MenuManager
    {

        private static readonly StringList Attack = new StringList(new[] { "Never", "Standart", "Always" }, 1);
        private static readonly Slider BonusRange = new Slider(100, 100, 500);

        public static void Load()
        {
            MenuLoader.Menu.AddItem(new MenuItem("enableLasthit", "Enabled").SetValue(true));
            MenuLoader.Menu.AddItem(new MenuItem("autoAttackMode", "Auto Attack Settings").SetValue(Attack));
            MenuLoader.Menu.AddItem(new MenuItem("sup", "Support").SetValue(false));
            MenuLoader.Menu.AddItem(new MenuItem("usespell", "Use spell for lasthitting").SetValue(true));
            //MenuLoader.Menu.AddItem(new MenuItem("harassheroes", "Harass enemy in farming mode").SetValue(true));
            MenuLoader.Menu.AddItem(new MenuItem("denied", "Try to deny creeps").SetValue(true));
            MenuLoader.Menu.AddItem(new MenuItem("AOC", "Attack own creeps").SetValue(false));
            MenuLoader.Menu.AddItem(new MenuItem("test", "Alpha Test Calculation").SetValue(true));
            MenuLoader.Menu.AddItem(new MenuItem("outrange", "Bonus range").SetValue(BonusRange));
            MenuLoader.Menu.AddToMainMenu();

            var subMenu = new Menu("Hotkeys", " hotkeys", false);
            subMenu.AddItem(new MenuItem("lasthit", "Lasthit mode").SetValue(new KeyBind('C', KeyBindType.Press)));
            subMenu.AddItem(new MenuItem("farmKey", "Farm mode").SetValue(new KeyBind('V', KeyBindType.Press)));
            MenuLoader.Menu.AddSubMenu(subMenu);
            
        }

        

        public static void Update()
        {
            MenuVariables.LastHitEnable = MenuLoader.Menu.Item("enableLasthit").GetValue<bool>();
            if (MenuVariables.LastHitEnable)
            {
                MenuVariables.Support = MenuLoader.Menu.Item("sup").GetValue<bool>();
                MenuVariables.UseSpell = MenuLoader.Menu.Item("usespell").GetValue<bool>();
                //MenuVariables.Harass = MenuLoader.Menu.Item("harassheroes").GetValue<bool>();
                MenuVariables.Denie = MenuLoader.Menu.Item("denied").GetValue<bool>();
                MenuVariables.Aoc = MenuLoader.Menu.Item("AOC").GetValue<bool>();
                MenuVariables.Test = MenuLoader.Menu.Item("test").GetValue<bool>();
            }

            MenuVariables.LastHitKey = MenuLoader.Menu.Item("lasthit").GetValue<KeyBind>().Key;
            MenuVariables.FarmKey = MenuLoader.Menu.Item("farmKey").GetValue<KeyBind>().Key;

            MenuVariables.AutoAttackMode = MenuLoader.Menu.Item("autoAttackMode").GetValue<StringList>().SelectedIndex;
            MenuVariables.Outrange = MenuLoader.Menu.Item("outrange").GetValue<Slider>().Value;


        }
    }

    internal class MenuVariables
    {
        #region MenuVariables
        public static bool Aoc;
        public static int AutoAttackMode;
        public static bool Denie;
        public static uint FarmKey;
        //public static bool Harass;
        public static bool LastHitEnable;
        public static uint LastHitKey;
        public static int Outrange;
        public static bool Support;
        public static bool Test;
        public static bool UseSpell;
        #endregion MenuVariables
    }
}
