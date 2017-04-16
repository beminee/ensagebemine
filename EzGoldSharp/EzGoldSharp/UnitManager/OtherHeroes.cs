using System;

namespace EzGoldSharp.UnitManager
{

    using Ensage;
    using Ensage.Common;
    using Ensage.Common.Extensions;
    using System.Collections.Generic;
    using System.Linq;

    class OtherHeroes
    {
    }

    internal class AllyHeroes
    {
        #region Fields

        public static Dictionary<float, List<Ability>> AbilityDictionary;

        public static List<Hero> Heroes;

        public static Dictionary<float, List<Item>> ItemDictionary;

        public static Hero[] UsableHeroes;

        #endregion Fields

        #region Methods

        public static void Update()
        {
            if (Utils.SleepCheck("AllyHeroes.Get"))
            {
                UpdateHeroes();
                Utils.Sleep(1000, "AllyHeroes.Get");
            }
            Heroes = Heroes.Where(x => x.IsValid).ToList();
            UsableHeroes = Heroes.Where(x => x.Health > 0 && x.IsAlive && x.IsVisible).ToArray();
            if (!Utils.SleepCheck("AllyHeroes.CheckValid")) return;
            Utils.Sleep(2000, "AllyHeroes.CheckValid");
            foreach (var hero in UsableHeroes)
            {
                var handle = hero.Handle;
                var items = hero.Inventory.Items.ToList();

                if (ItemDictionary.ContainsKey(handle))
                {
                    ItemDictionary[handle] =
                        items.Where(
                            x => x.AbilityType != AbilityType.Attribute && x.AbilityType != AbilityType.Hidden)
                            .ToList();
                    continue;
                }

                var itemlist =
                    items.Where(x => x.AbilityType != AbilityType.Attribute && x.AbilityType != AbilityType.Hidden)
                        .ToList();
                ItemDictionary.Add(handle, itemlist);
            }
        }

        public static void UpdateHeroes()
        {
            var list = ObjectManager.GetEntitiesParallel<Hero>().Where(x => x.Team == Variables.Me.Team && x.IsValid && !x.IsIllusion && x.IsVisible).ToList();
            var herolist = new List<Hero>(Heroes);
            foreach (var hero in list)
            {
                var handle = hero.Handle;
                var spells = hero.Spellbook.Spells.ToList();
                if (!herolist.Contains(hero))
                {
                    Heroes.Add(hero);
                }

                var abilitylist =
                    spells.Where(x => x.AbilityType != AbilityType.Attribute && x.AbilityType != AbilityType.Hidden)
                        .ToList();
                if (AbilityDictionary.ContainsKey(handle))
                {
                    AbilityDictionary[handle] = abilitylist;
                    continue;
                }

                AbilityDictionary.Add(handle, abilitylist);
            }
        }

        #endregion Methods
    }
        internal class EnemyHeroes
        {
            #region Fields

            public static Dictionary<float, List<Ability>> AbilityDictionary;

            public static List<Hero> Heroes;

            public static List<Hero> Illusions;
            public static Dictionary<float, List<Item>> ItemDictionary;
            public static Hero[] UsableHeroes;

            #endregion Fields

            #region Methods

            public static void Update()
            {
                if (Utils.SleepCheck("EnemyHeroes.Get"))
                {
                    UpdateHeroes();
                    Utils.Sleep(1000, "EnemyHeroes.Get");
                }
                if (Utils.SleepCheck("EnemyHeroes.GetIllu"))
                {
                    UpdateIllusions();
                    Utils.Sleep(100, "EnemyHeroes.GetIllu");
                }
                Illusions = Illusions.Where(x => x.IsValid).ToList();
                Heroes = Heroes.Where(x => x.IsValid).ToList();
                UsableHeroes = Heroes.Where(x => x.Health > 0 && x.IsAlive && x.IsVisible && x.Distance2D(Variables.Me) <= Variables.GetOutRange(Variables.Me)).ToArray();
                if (Utils.SleepCheck("EnemyHeroes.CheckValid") || UsableHeroes.Any(x => !ItemDictionary.ContainsKey(x.Handle)))
                {
                    Utils.Sleep(2000, "EnemyHeroes.CheckValid");
                    foreach (var hero in UsableHeroes)
                    {
                        var handle = hero.Handle;
                        var items = hero.Inventory.Items.ToList();
                        if (ItemDictionary.ContainsKey(handle))
                        {
                            ItemDictionary[handle] =
                                items.Where(
                                    x => x.AbilityType != AbilityType.Attribute && x.AbilityType != AbilityType.Hidden)
                                    .ToList();
                            continue;
                        }
                        var itemlist =
                            items.Where(x => x.AbilityType != AbilityType.Attribute && x.AbilityType != AbilityType.Hidden)
                                .ToList();
                        ItemDictionary.Add(handle, itemlist);
                    }
                }
            }

            private static void UpdateHeroes()
            {
                try
                {
                    var list = ObjectManager.GetEntitiesParallel<Hero>().Where(x => x.Team != Variables.Me.Team).ToList();
                    //if (list.Count < Heroes.Count) Heroes.Clear();
                    var heroeslist = new List<Hero>(Heroes);
                    foreach (var hero in list.Where(x => x.Team != Variables.Me.Team && x.IsValid && x.IsVisible && !x.IsIllusion))
                    {
                        var handle = hero.Handle;
                        var spells = hero.Spellbook.Spells.ToList();
                        if (!heroeslist.Contains(hero))
                        {
                            Heroes.Add(hero);
                        }
                        var abilitylist =
                            spells.Where(x => x.AbilityType != AbilityType.Attribute && x.AbilityType != AbilityType.Hidden)
                                .ToList();
                        if (AbilityDictionary.ContainsKey(handle))
                        {
                            AbilityDictionary[handle] = abilitylist;
                            continue;
                        }
                        AbilityDictionary.Add(handle, abilitylist);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e + "Error EnemyHeroes");
                }
            }

            private static void UpdateIllusions()
            {
                try
                {
                    var illusionslist = Illusions.Where(illusion => !illusion.IsAlive).ToList();
                    illusionslist.ForEach(x => Illusions.Remove(x));
                    illusionslist = new List<Hero>(Illusions);
                    var list = ObjectManager.GetEntitiesParallel<Hero>()
                        .Where(x => x.Team != Variables.Me.Team && x.IsValid && x.IsVisible && x.IsIllusion && x.IsAlive).ToList();
                    foreach (var illusion in list)
                    {
                        if (!illusionslist.Contains(illusion))
                        {
                            Illusions.Add(illusion);
                        }
                    }
                }
                catch (Exception e)
                {
                   Console.WriteLine(e + "Update Illusions Error");
                }
            }

            #endregion Methods
        }
    
}
