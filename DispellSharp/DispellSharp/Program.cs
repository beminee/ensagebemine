namespace DispellSharp
{
    using System;
    using System.Linq;

    using Ensage;
    using Ensage.Common;
    using Ensage.Common.Extensions;
    using Ensage.Common.Menu;

    internal class Program
    {
        #region Static Fields

        private static readonly string[] EnemyCleans =
            {
                "modifier_ghost_state", "modifier_item_ethereal_blade_ethereal",
                "modifier_omninight_guardian_angel"
            };

        private static readonly string[] OwnCleans =
            {
                "modifier_item_dustofappearance",
                "modifier_orchid_malevolence_debuff",
                "modifier_bloodthorn_debuff",
                "modifier_skywrath_mage_ancient_seal"
            };

        private static MenuItem EnabledItem;

        private static bool loaded;

        private static Menu Menu;

        #endregion

        #region Public Methods and Operators

        public static void Game_OnUpdate(EventArgs args)
        {
            if (!loaded || !Game.IsInGame || Game.IsPaused || Game.IsWatchingGame || !EnabledItem.GetValue<bool>()) return;

            var me = ObjectManager.LocalHero;
            if (me == null) return;

            var manta = me.FindItem("item_manta");

            var diff = me.Inventory.Items.FirstOrDefault(item => item.Name.Contains("item_diffusal_blade"));

            if (diff != null && Utils.SleepCheck("diff") && !me.IsChanneling() && me.CanAttack()
                && diff.CurrentCharges > 0 && diff.Cooldown <= 0)
            {
                var enemies =
                    ObjectManager.GetEntitiesParallel<Hero>()
                        .Where(
                            y =>
                                y.Team != me.Team && y.IsAlive && y.IsVisible && !y.IsIllusion
                                && me.Distance2D(y) < 500 && y.HasModifiers(EnemyCleans, false))
                        .ToList();

                foreach (var enemy in enemies)
                {
                    diff.UseAbility(enemy);
                    Utils.Sleep(200, "diff");
                    return;
                }
            }

            if (me.HasModifiers(OwnCleans, false))
            {
                if (manta != null && Utils.SleepCheck("manta") && manta.Cooldown <= 0 && !me.IsChanneling() )
                {
                    manta.UseAbility();
                    Utils.Sleep(200, "manta");
                }
            }
        }

        #endregion

        #region Methods

        private static void Events_OnLoad(object sender, EventArgs e)
        {
            if (loaded) return;
            loaded = true;

            Menu = new Menu("DispellSharp", "dispellsharp", true, "item_manta", true);
            var options = new Menu("Options", "opt");
            EnabledItem = new MenuItem("enable", "Active?").SetValue(true);
            options.AddItem(EnabledItem);
            Menu.AddSubMenu(options);
            Menu.AddToMainMenu();
        }

        private static void Main()
        {
            Events.OnLoad += Events_OnLoad;
            Game.OnUpdate += Game_OnUpdate;
            Console.WriteLine("DispellSharp Loaded");
        }

        #endregion
    }
}