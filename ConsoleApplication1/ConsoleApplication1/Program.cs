using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            var heroName = "npc_dota_hero_sniper";
            var hero = heroName.Substring(14).Replace("_", " ");
            hero = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(hero);
            Console.WriteLine(hero);
            Console.ReadLine();
        }
    }
}
