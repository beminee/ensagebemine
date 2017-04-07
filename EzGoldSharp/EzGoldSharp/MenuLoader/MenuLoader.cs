using Ensage.Common.Menu;

namespace EzGoldSharp.MenuLoader
{
   internal class MenuLoader
   {
       public static Menu Menu;

       public static void Load()
       {
           Menu = Menu = new Menu("EzGoldSharp", "ezgoldsharp", true, "alchemist_goblins_greed", true);
           MenuManager.Load();
       }

       public static void Update()
       {
           MenuManager.Update();
       }

       public static void UnLoad()
       {
           if (Menu == null)
           {
               return;
           }

           Menu.RemoveFromMainMenu();
           Menu = null;
       }

    }
}
