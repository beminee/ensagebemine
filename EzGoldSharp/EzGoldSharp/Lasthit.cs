using Ensage.Common.Objects.UtilityObjects;
using EzGoldSharp.UnitManager;
using EzGoldSharp.MenuLoader;

namespace EzGoldSharp
{
    
    using Ensage;
    using Ensage.Common;
    using Ensage.Common.Extensions;
    using SharpDX;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class Lasthit
    {
        #region PreDICKtionStuff

        public static int Attack(Entity unit)
        {
            var count = 0;
            try
            {
                var creeps =
                    ObjectManager.GetEntitiesFast<Unit>()
                        .Where(
                            x =>
                                x.Distance2D(unit) <= x.AttackRange + 100 && x.IsAttacking() && x.IsAlive &&
                                x.Handle != unit.Handle && x.Team != unit.Team)
                        .ToList();
                count += (from creep in creeps
                          let angle = creep.Rotation < 0 ? Math.Abs(creep.Rotation) : 360 - creep.Rotation
                          where Math.Abs(angle - creep.FindAngleForTurnTime(unit.Position)) <= 3
                          select creep).Count();
            }
            catch (Exception e)
            {
                Console.WriteLine(e + "Attack count failed");
            }
            return count;
        }

        public static void Attack_Calc()
        {
            if (!ObjectManager.GetEntitiesFast<Unit>().Any(x => x.Distance2D(Variables.Me) <= 2000 && x.IsAlive && x.Health > 0))
                return;
            var creeps =
                ObjectManager.GetEntitiesFast<Unit>()
                    .Where(x => x.IsValid && x.Distance2D(Variables.Me) <= 2000 && x.IsAlive && x.Health > 0)
                    .ToList();
            foreach (var creep in creeps)
            {
                if (!Variables.CreepsDic.Any(x => x.AHealth(creep)))
                {
                    Variables.CreepsDic.Add(new UnitDictionary { Unit = creep, KillableCreeps = new List<KillableCreeps>() });
                }
            }
            Clear();
            Utils.Sleep(100, "Wait");

            if (Utils.SleepCheck("Wait")) return;
            {
                foreach (var creep in Variables.CreepsDic)
                {
                    if (Variables.CreeptargetH != null && creep.Unit != null)
                    { 
                        if (creep.Unit.IsValid && !creep.Unit.IsRanged && creep.Unit.Team == Variables.CreeptargetH.GetEnemyTeam())
                        {
                            if (MinionAaData.StartedAttack(creep.Unit))
                            {
                                Variables.minionAttackPointList.Add(creep.Unit.Handle);

                                var creepAttackPoint = MinionAaData.GetAttackPoint(creep.Unit) * 1000;
                                var creepAttackBackswing = MinionAaData.GetAttackBackswing(creep.Unit) * 1000;

                                if (creep.Unit.IsValid)
                                    DelayAction.Add(creepAttackPoint, () =>
                                    {
                                        Variables.minionAttackPointList.Remove(creep.Unit.Handle);
                                        Variables.minionAttackBackswingList.Add(creep.Unit.Handle);
                                    });

                                if (creep.Unit.IsValid)
                                    DelayAction.Add(creepAttackPoint + creepAttackBackswing, () =>
                                    {
                                        Variables.minionAttackBackswingList.Remove(creep.Unit.Handle);
                                    });
                            }
                        }
                        else if (creep.Unit.IsValid && creep.Unit.IsRanged && creep.Unit.Team == Variables.CreeptargetH.GetEnemyTeam())
                        {
                            if (MinionAaData.StartedAttack(creep.Unit))
                            {
                                Variables.minionAttackPointList.Add(creep.Unit.Handle);

                                var creepAttackPoint = MinionAaData.GetAttackPoint(creep.Unit) * 1000;
                                var creepAttackBackswing = MinionAaData.GetAttackBackswing(creep.Unit) * 1000;
                                if (creep.Unit.IsValid)
                                    DelayAction.Add(creepAttackPoint, () =>
                                    {
                                        Variables.minionAttackPointList.Remove(creep.Unit.Handle);
                                        Variables.minionAttackBackswingList.Add(creep.Unit.Handle);
                                    });
                                if (creep.Unit.IsValid)
                                    DelayAction.Add(creepAttackPoint + creepAttackBackswing, () =>
                                    {
                                        Variables.minionAttackBackswingList.Remove(creep.Unit.Handle);
                                    });
                            }
                        }
                    }
                    //Console.WriteLine("Unit : {0}", creep.Unit.Handle);
                    /*foreach (var ht in creep.KillableCreeps)
                         {
                             //Console.WriteLine("Health - time : {0} - {1} - {2}", ht.Health, ht.Time, ht.ACreeps);
                         }*/
                }
            }
        }

        public static double Healthpredict(Unit unit, double time)
        {
            if (Variables.CreepsDic.Count != 0 && MenuVariables.Test)
            {
                if (Variables.CreepsDic.All(x => x.Unit.Handle != unit.Handle)) return 0;
                try
                {

                    var timetoCheck = Environment.TickCount + time;

                    const int rangedprojSpeed = 900;

                    //var hta = Variables.CreepsDic.First(x => x.Unit.Handle == unit.Handle).KillableCreeps.ToList();

                    var allyMeleeCreeps = ObjectManager.GetEntitiesFast<Creep>().Where(creep => creep.IsAlive && creep.IsValid && creep.Team == Variables.Me.Team && creep.IsMelee).ToList();
                    var enemyMeleeCreeps = ObjectManager.GetEntitiesFast<Creep>().Where(creep => creep.IsAlive && creep.IsValid && creep.Team == Variables.Me.GetEnemyTeam() && creep.IsMelee).ToList();

                    var allyRangedCreeps = ObjectManager.GetEntitiesFast<Creep>().Where(creep => creep.IsAlive && creep.IsValid && creep.Team == Variables.Me.Team && creep.IsRanged).ToList();
                    var enemyRangedCreeps = ObjectManager.GetEntitiesFast<Creep>().Where(creep => creep.IsAlive && creep.IsValid && creep.Team == Variables.Me.GetEnemyTeam() && creep.IsRanged).ToList();

                    var allyTowers = Buildings.AllyTowers;
                    var enemyTowers = Buildings.EnemyTowers;

                    if (unit.Team == Variables.Me.GetEnemyTeam()) //Enemy Creep
                    {
                        var rangedDamage = 0f;
                        var meleeDamage = 0f;
                        var towerDamage = 0f;

                        foreach (var allyCreep in allyRangedCreeps)
                        {
                            var projDamage = 0f;

                            Ray FrontPos = new Ray(allyCreep.NetworkPosition, allyCreep.Vector3FromPolarAngle());

                            BoundingSphere unitPos = new BoundingSphere(unit.NetworkPosition, 25);

                            if (FrontPos.Intersects(unitPos) && Math.Max(0, allyCreep.Distance2D(unit)) < (allyCreep.AttackRange + allyCreep.HullRadius / 2) && MinionAaData.StartedAttack(allyCreep))
                            {

                                var arrivalTime = Environment.TickCount + 1000 * Math.Max(0, allyCreep.Distance2D(unit)) / rangedprojSpeed + 1000 * MinionAaData.GetAttackPoint(allyCreep);

                                if (arrivalTime < timetoCheck)
                                {
                                    projDamage = GetPhysDamage(allyCreep, unit);
                                }
                            }

                            rangedDamage += projDamage;
                        }

                        foreach (var allyCreep in allyMeleeCreeps)
                        {
                            var hitDamage = 0f;

                            Ray FrontPos = new Ray(allyCreep.NetworkPosition, allyCreep.Vector3FromPolarAngle());

                            BoundingSphere unitPos = new BoundingSphere(unit.NetworkPosition, 25);

                            if (FrontPos.Intersects(ref unitPos) && Math.Max(0, allyCreep.Distance2D(unit)) < allyCreep.GetAttackRange() && MinionAaData.StartedAttack(allyCreep))
                            {
                                var arrivalTime = Environment.TickCount + MinionAaData.GetAttackPoint(allyCreep) * 1000;

                                if (arrivalTime < timetoCheck)
                                {
                                    hitDamage = GetPhysDamage(allyCreep, unit);
                                }
                            }

                            meleeDamage += hitDamage;
                        }

                        if (allyTowers != null) // Ally Towers
                        {
                            foreach (var allytower in allyTowers)
                            {
                                var tprjDamage = 0f;

                                if (allytower.IsAttacking()) continue;

                                var arrivalTime = (Environment.TickCount + TowerData.GetAttackSpeed(allytower)) + allytower.Distance2D(Variables.CreeptargetH) * 1000;

                                if (arrivalTime < timetoCheck)
                                {
                                    tprjDamage = GetPhysDamage(allytower, unit);
                                }
                                towerDamage += tprjDamage;
                            }
                        }
                        else
                        {
                            towerDamage = 0f;
                        }

                        return Math.Max(0, unit.Health - (rangedDamage + meleeDamage + towerDamage));
                    }

                    if (unit.Team == Variables.Me.Team) //Ally Creep
                    {
                        var rangedDamage = 0f;
                        var meleeDamage = 0f;
                        var towerDamage = 0f;

                        foreach (var enemyCreep in enemyRangedCreeps)
                        {
                            var projDamage = 0f;

                            Ray FrontPos = new Ray(enemyCreep.NetworkPosition, enemyCreep.Vector3FromPolarAngle());

                            BoundingSphere unitPos = new BoundingSphere(unit.NetworkPosition, 25);

                            if (FrontPos.Intersects(ref unitPos) && Math.Max(0, enemyCreep.Distance2D(unit)) < (enemyCreep.AttackRange + enemyCreep.HullRadius / 2) && MinionAaData.StartedAttack(enemyCreep))
                            {

                                var arrivalTime = Environment.TickCount + 1000 * Math.Max(0, enemyCreep.Distance2D(unit)) / rangedprojSpeed + 1000 * MinionAaData.GetAttackPoint(enemyCreep);

                                if (arrivalTime < timetoCheck)
                                {
                                    projDamage = GetPhysDamage(enemyCreep, unit);
                                }
                            }

                            rangedDamage += projDamage;
                        }

                        foreach (var enemyCreep in enemyMeleeCreeps)
                        {
                            var hitDamage = 0f;

                            Ray FrontPos = new Ray(enemyCreep.NetworkPosition, enemyCreep.Vector3FromPolarAngle());

                            BoundingSphere unitPos = new BoundingSphere(unit.NetworkPosition, 25);

                            if (FrontPos.Intersects(ref unitPos) && Math.Max(0, enemyCreep.Distance2D(unit)) < enemyCreep.GetAttackRange() && MinionAaData.StartedAttack(enemyCreep))
                            {
                                var arrivalTime = Environment.TickCount + MinionAaData.GetAttackPoint(enemyCreep) * 1000;

                                if (arrivalTime < timetoCheck)
                                {
                                    hitDamage = GetPhysDamage(enemyCreep, unit);
                                }
                            }

                            meleeDamage += hitDamage;
                        }

                        if (enemyTowers != null) // Enemy Towers
                        {
                            foreach (var enemytower in enemyTowers)
                            {
                                var tprjDamage = 0f;

                                if (enemytower.IsAttacking()) continue;

                                var arrivalTime = (Environment.TickCount + TowerData.GetAttackSpeed(enemytower)) + enemytower.Distance2D(Variables.CreeptargetH) * 1000;

                                if (arrivalTime < timetoCheck)
                                {
                                    tprjDamage = GetPhysDamage(enemytower, unit);
                                }
                                towerDamage += tprjDamage;
                            }
                        }
                        else
                        {
                            towerDamage = 0f;
                        }

                        return Math.Max(0, unit.Health - (rangedDamage + meleeDamage + towerDamage));
                    }

                }
                catch (Exception e)
                {
                   Console.WriteLine(e + "Health predict Error");
                }
            }
            return unit.Health;
        }

        private static void Clear()
        {
            if (!Utils.SleepCheck("Lasthit.Clear")) return;
            var creeps = ObjectManager.GetEntitiesFast<Unit>().Where(x => x.IsAlive).ToList();
            Variables.CreepsDic = (from creep in creeps
                                   where Variables.CreepsDic.Any(x => x.Unit.Handle == creep.Handle)
                                   select Variables.CreepsDic.Find(x => x.Unit.Handle == creep.Handle)).ToList();
            Utils.Sleep(10000, "Lasthit.Clear");
        }

        #endregion PreDICKtionStuff

        #region Main

        public static void Farm()
        {
            if (!Utils.SleepCheck("Lasthit.Cast")) return;
            if (!Variables.DisableAaKeyPressed)
            {
                Variables.Autoattack(0);
                Variables.DisableAaKeyPressed = true;
                Variables.AutoAttackTypeDef = false;
            }
            Variables.CreeptargetH = KillableCreep(Variables.Me, false, 99);

            if (Variables.CreeptargetH != null && Variables.CreeptargetH.IsValid && Variables.CreeptargetH.IsVisible && Variables.CreeptargetH.IsAlive)
            {
                if (MenuVariables.UseSpell && Utils.SleepCheck("Lasthit.Cooldown"))
                    SpellCast();
                Orbwalking.Orbwalk(Variables.CreeptargetH);
            }

           
            else if (MenuVariables.Harass && EnemyHeroes.Heroes != null)
            {
                var enemyHeroes = EnemyHeroes.UsableHeroes;

                foreach (var enemyHero in enemyHeroes)
                {
                    if (enemyHero.IsValid && Variables.Me.IsValid && Variables.Me.CanAttack() && Variables.Me.IsAlive && enemyHero.Distance2D(Variables.Me) <= MyHero.AttackRange())
                    {
                        Orbwalking.Orbwalk(enemyHero, 0, MenuVariables.Outrange, true);
                    }
                }
            } 
        }

        public static List<Unit> GetNearestCreep(Unit source, float range)
        {
            try
            {
                return ObjectManager.GetEntitiesFast<Unit>()
                        .Where(
                            x =>
                                (x.ClassId == ClassId.CDOTA_BaseNPC_Tower ||
                                 x.ClassId == ClassId.CDOTA_BaseNPC_Creep_Lane ||
                                 x.ClassId == ClassId.CDOTA_BaseNPC_Creep ||
                                 x.ClassId == ClassId.CDOTA_BaseNPC_Creep_Neutral ||
                                 x.ClassId == ClassId.CDOTA_BaseNPC_Creep_Siege ||
                                 x.ClassId == ClassId.CDOTA_BaseNPC_Additive ||
                                 x.ClassId == ClassId.CDOTA_BaseNPC_Barracks ||
                                 x.ClassId == ClassId.CDOTA_BaseNPC_Building ||
                                 x.ClassId == ClassId.CDOTA_BaseNPC_Creature) &&
                                 x.IsAlive && x.IsVisible && x.Distance2D(source) < range)
                                 .OrderBy(creep => creep.Health)
                                 .ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e + "GetNearestCreep Error");
            }
            return null;
        }

        public static void LastHit()
        {
            if (!Utils.SleepCheck("Lasthit.Cast")) return;
            if (!Variables.DisableAaKeyPressed)
            {
                Variables.Autoattack(0);
                Variables.DisableAaKeyPressed = true;
                Variables.AutoAttackTypeDef = false;
            }
            Variables.CreeptargetH = KillableCreep(Variables.Me, false, 2);
            if (MenuVariables.UseSpell && Utils.SleepCheck("Lasthit.Cooldown"))
                SpellCast();

            if (Variables.CreeptargetH != null && Variables.CreeptargetH.IsValid &&
                Variables.CreeptargetH.IsVisible && Variables.CreeptargetH.IsAlive)
            {
                var attackPoint = Variables.Me.IsRanged == false
                    ? 0 
                    : MyHero.GetMyAttackPoint(Variables.Me);
                var time = Variables.Me.IsRanged == false
                    ? Variables.HeroAPoint / 1000 + Variables.Me.GetTurnTime(Variables.CreeptargetH.Position)
                    : Variables.HeroAPoint / 1000 + Variables.Me.GetTurnTime(Variables.CreeptargetH.Position) +
                      Variables.Me.Distance2D(Variables.CreeptargetH) / MyHero.GetProjectileSpeed(Variables.Me);
                var getPred = Healthpredict(Variables.CreeptargetH, time + attackPoint);
                var getDamage = Math.Ceiling(GetDamageOnUnit(Variables.Me, Variables.CreeptargetH, 0));
                //Game.PrintMessage(getDamage.ToString());
                //Game.PrintMessage(getPred.ToString());
                //Console.WriteLine(attackBackswing);
                if (Variables.CreeptargetH.Distance2D(Variables.Me) <= MyHero.AttackRange())
                {
                    if (getPred > 0 && getPred <= getDamage || Variables.CreeptargetH.Health < getDamage ||
                        Variables.CreeptargetH.Health < getDamage && Variables.CreeptargetH.Team == Variables.Me.Team &&
                        (MenuVariables.Denie || MenuVariables.Aoc))
                    {
                        if (!Variables.Me.IsAttacking())
                            Variables.Me.Attack(Variables.CreeptargetH);
                    }
                    else if (getPred <= 0 ||Variables.CreeptargetH.Health < getDamage * 2 && Variables.CreeptargetH.Health >= getDamage &&
                             Variables.CreeptargetH.Team != Variables.Me.Team && Utils.SleepCheck("Lasthit.Stop"))
                    {
                        Variables.Me.Hold(); // Is stop better ?
                        Variables.Me.Attack(Variables.CreeptargetH);
                        Utils.Sleep((float)Variables.HeroAPoint / 2 + Game.Ping, "Lasthit.Stop");
                    }
                }

                else if (Variables.Me.Distance2D(Variables.CreeptargetH) >= MyHero.AttackRange() && Utils.SleepCheck("Lasthit.Walk"))
                {
                    Variables.Me.Move(Variables.CreeptargetH.Position);
                    Utils.Sleep(100 + Game.Ping, "Lasthit.Walk");
                }
            }
        }

        private static double GetDamageOnUnit(Unit unit, Unit minion, double bonusdamage)
        {
            double modif = 1;
            double magicdamage = 0;
            double physDamage = unit.MinimumDamage + unit.BonusDamage;
            if (unit.Handle == Variables.Me.Handle)
            {
                var quellingBlade = unit.FindItem("item_quelling_blade");
                var ironTalon = unit.FindItem("item_iron_talon");
                if (ironTalon != null || quellingBlade != null && minion.Team != unit.Team)
                {
                    if (unit.IsRanged)
                    {
                        physDamage = unit.MinimumDamage + unit.BonusDamage + 7;
                    }
                    else
                    {
                        physDamage = unit.MinimumDamage + unit.BonusDamage + 24;
                    }
                }
                switch (unit.ClassId)
                {
                    case ClassId.CDOTA_Unit_Hero_AntiMage:
                        var manabreakDamage = Variables.Q.GetAbilityData("mana_per_hit");
                        if (minion.MaximumMana > 0 && minion.Mana > 0 && Variables.Q.Level > 0 && minion.Team != unit.Team)
                            bonusdamage += manabreakDamage * 0.6;
                        break;

                    case ClassId.CDOTA_Unit_Hero_Viper:
                        if (minion.Team != unit.Team)
                        {
                            var nethertoxindmg = Variables.W.GetAbilityData("bonus_damage") / (Variables.W.GetAbilityData("non_hero_damage_pct") / 100);
                            var percent = Math.Floor((double)minion.Health / minion.MaximumHealth * 100);
                            if (percent > 80 && percent <= 100)
                                bonusdamage += nethertoxindmg;
                            else if (percent > 60 && percent <= 80)
                                bonusdamage += nethertoxindmg * 2;
                            else if (percent > 40 && percent <= 60)
                                bonusdamage += nethertoxindmg * 4;
                            else if (percent > 20 && percent <= 40)
                                bonusdamage += nethertoxindmg * 8;
                            else if (percent > 0 && percent <= 20)
                                bonusdamage += nethertoxindmg * 16;
                        }
                        break;

                    case ClassId.CDOTA_Unit_Hero_Ursa:
                        var furymodif = 0;
                        var furyDamage = Variables.E.GetAbilityData("damage_per_stack");
                        var furyTalent = Variables.Me.Spellbook.Spells.First(x => x.Name == "special_bonus_unique_ursa").Level <= 0 ? 0 : 10;
                        if (unit.Modifiers.Any(x => x.Name == "modifier_ursa_fury_swipes_damage_increase"))
                            furymodif =
                                minion.Modifiers.Find(x => x.Name == "modifier_ursa_fury_swipes").StackCount;
                        if (minion.Team != unit.Team)
                        {
                            if (furymodif > 0)
                                bonusdamage += furymodif * (furyDamage + furyTalent);
                            else
                                bonusdamage += furyDamage + furyTalent;
                        }
                        break;

                    case ClassId.CDOTA_Unit_Hero_BountyHunter:
                        var jinadaDamage = Variables.W.GetAbilityData("crit_multiplier");
                        if (Variables.W.AbilityState == AbilityState.Ready)
                            bonusdamage += (physDamage * jinadaDamage / 100) - physDamage;
                        break;

                    case ClassId.CDOTA_Unit_Hero_Weaver:
                        if (Variables.E.Level > 0 && Variables.E.AbilityState == AbilityState.Ready)
                            bonusdamage += physDamage;
                        break;

                    case ClassId.CDOTA_Unit_Hero_Kunkka:
                        var tidebringerDamage = Variables.W.GetAbilityData("damage_bonus");
                        if (Variables.W.AbilityState == AbilityState.Ready &&
                            Variables.W.IsAutoCastEnabled)
                            bonusdamage += tidebringerDamage;
                        break;

                    case ClassId.CDOTA_Unit_Hero_Life_Stealer:
                        var feastDamage = Variables.E.GetAbilityData("hp_leech_percent");  
                            bonusdamage += minion.Health * feastDamage;
                        break;

                }
                if (unit.Modifiers.Any(x => x.Name == "modifier_storm_spirit_overload"))
                {
                    var overloadDamage = Variables.E.GetAbilityData("AbilityDamage");
                    magicdamage += overloadDamage;
                }
                if (unit.Modifiers.Any(x => x.Name == "modifier_chilling_touch"))
                {
                    var chillingtouchDamage = Variables.E.GetAbilityData("bonus_damage");
                    var chillingtouchTalent = Variables.Me.Spellbook.Spells.First(x => x.Name == "special_bonus_unique_ancient_apparition_2").Level <= 0 ? 0 : 80;
                    magicdamage += chillingtouchDamage + chillingtouchTalent;
                }
                if (unit.ClassId == ClassId.CDOTA_Unit_Hero_Pudge && Variables.W.Level > 0 &&
                    MenuVariables.UseSpell && Variables.E.CanHit(minion))
                {
                    var rotDamage = Variables.E.GetAbilityData("rot_damage") / 2;
                    var rotTalent = Variables.Me.Spellbook.Spells.First(x => x.Name == "special_bonus_unique_pudge_2").Level <= 0 ? 0 : 120;
                    magicdamage += rotDamage + rotTalent;
                }
            }
            if (unit.Modifiers.Any(x => x.Name == "modifier_bloodseeker_bloodrage"))
            {
                modif = modif *
                        (ObjectManager.GetEntitiesFast<Hero>()
                            .First(x => x.ClassId == ClassId.CDOTA_Unit_Hero_Bloodseeker)
                            .Spellbook.Spell1.Level - 1) * 0.05 + 1.25;
            }

            var damageMp = 1 - 0.06 * minion.Armor / (1 + 0.06 * Math.Abs(minion.Armor));
            magicdamage = magicdamage * (1 - minion.MagicDamageResist);

            var realDamage = ((bonusdamage + physDamage) * damageMp + magicdamage) * modif;
            if (minion.ClassId == ClassId.CDOTA_BaseNPC_Creep_Siege ||
                minion.ClassId == ClassId.CDOTA_BaseNPC_Tower)
            {
                realDamage = realDamage / 2;
            }
            if (realDamage > minion.MaximumHealth)
                realDamage = minion.Health + 10;

            return realDamage;
        }

        private static Unit GetLowestHpCreep(Unit source, float range)
        {
            try
            {
                var lowestHp =
                    ObjectManager.GetEntitiesFast<Unit>()
                        .Where(
                            x =>
                                (x.ClassId == ClassId.CDOTA_BaseNPC_Tower ||
                                 x.ClassId == ClassId.CDOTA_BaseNPC_Creep_Lane ||
                                 x.ClassId == ClassId.CDOTA_BaseNPC_Creep ||
                                 x.ClassId == ClassId.CDOTA_BaseNPC_Creep_Neutral ||
                                 x.ClassId == ClassId.CDOTA_BaseNPC_Creep_Siege ||
                                 x.ClassId == ClassId.CDOTA_BaseNPC_Additive ||
                                 x.ClassId == ClassId.CDOTA_BaseNPC_Barracks ||
                                 x.ClassId == ClassId.CDOTA_BaseNPC_Building ||
                                 x.ClassId == ClassId.CDOTA_BaseNPC_Creature) &&
                                 x.IsAlive && x.IsVisible && x.Team != source.Team &&
                                 x.Distance2D(source) < range)
                        .OrderBy(creep => creep.Health)
                        .DefaultIfEmpty(null)
                        .FirstOrDefault();
                return lowestHp;
            }
            catch (Exception e)
            {
                Console.WriteLine(e + "GetLowestHpCreep Error");
            }
            return null;
        }

        private static Unit GetMyLowestHpCreep(Unit source, float range)
        {
            try
            {
                return ObjectManager.GetEntitiesFast<Unit>()
                        .Where(
                            x =>
                                (x.ClassId == ClassId.CDOTA_BaseNPC_Tower ||
                                 x.ClassId == ClassId.CDOTA_BaseNPC_Creep_Lane ||
                                 x.ClassId == ClassId.CDOTA_BaseNPC_Creep ||
                                 x.ClassId == ClassId.CDOTA_BaseNPC_Creep_Neutral ||
                                 x.ClassId == ClassId.CDOTA_BaseNPC_Creep_Siege ||
                                 x.ClassId == ClassId.CDOTA_BaseNPC_Additive ||
                                 x.ClassId == ClassId.CDOTA_BaseNPC_Barracks ||
                                 x.ClassId == ClassId.CDOTA_BaseNPC_Building ||
                                 x.ClassId == ClassId.CDOTA_BaseNPC_Creature) &&
                                 x.IsAlive && x.IsVisible && x.Team == source.Team &&
                                 x.Distance2D(source) < range)
                        .OrderBy(creep => creep.Health)
                        .DefaultIfEmpty(null)
                        .FirstOrDefault();
            }
            catch (Exception e)
            {
                Console.WriteLine(e + "Error GetAllLowestHpCreep");
            }
            return null;
        }

        private static Unit KillableCreep(Unit unit, bool islaneclear, double x)
        {
            try
            {
                Unit minion, miniondenie;

                    minion = GetLowestHpCreep(unit, Variables.GetOutRange(unit));
                    miniondenie = GetMyLowestHpCreep(unit, Variables.GetOutRange(unit));

                if (minion == null && miniondenie == null) return null;
                if (minion != null && miniondenie != null)
                {
                var percent = minion.Health / minion.MaximumHealth * 100;
                if ((miniondenie.Health > GetDamageOnUnit(unit, miniondenie, 0) ||
                    minion.Health < GetDamageOnUnit(unit, minion, 0) + 30) &&
                    (percent < 90 || GetDamageOnUnit(unit, minion, 0) > minion.MaximumHealth) &&
                    minion.Health < GetDamageOnUnit(unit, minion, 0) * x && !MenuVariables.Support)
                {
                    if (unit.CanAttack())
                        return minion;
                }
                else if (islaneclear)
                {
                    return minion;
                }

                if (MenuVariables.Denie)
                {
                    if (miniondenie.Health <= GetDamageOnUnit(unit, miniondenie, 0) * x * 0.75 &&
                        miniondenie.Health <= miniondenie.MaximumHealth / 2 &&
                        miniondenie.Team == unit.Team)
                    {
                        if (unit.CanAttack())
                            return miniondenie;
                    }
                }

                if (MenuVariables.Aoc)
                {
                    if (miniondenie.Health <= miniondenie.MaximumHealth / 2 &&
                        miniondenie.Health > GetDamageOnUnit(unit, miniondenie, 0) * x * 0.75 &&
                        miniondenie.Team == unit.Team)
                        if (unit.CanAttack())
                            return miniondenie;
                }
                return null;
            }
            }
            catch (Exception e)
            {
                Console.WriteLine(e + "Error in Killable Creeps");
            }
            return null;
        }

        public static float GetPhysDamage(Unit source, Unit target)
        {
            var physDamage = source.MinimumDamage + source.BonusDamage;

            var damageMp = 1 - 0.06 * target.Armor / (1 + 0.06 * Math.Abs(target.Armor));

            return (float)(physDamage * damageMp);
        }

        private static void SpellCast()
        {
            try
            {
                Variables.UpdateCreeps();
                foreach (var creep in Variables.Creeps.Where(x => x.IsValid && x.Team != Variables.Me.Team)
                    .OrderByDescending(creep => creep.Health))
                {
                    double damage = 0;
                    switch (Variables.Me.ClassId)
                    {
                        case ClassId.CDOTA_Unit_Hero_Zuus:
                            if (Variables.Q.Level > 0 && Variables.Q.CanBeCasted() && Variables.Me.Distance2D(creep) > MyHero.AttackRange())
                            {
                                damage = Variables.Q.GetDamage(Variables.Q.Level - 1) * (1 - creep.MagicDamageResist);
                                //Console.WriteLine(damage);
                                if (damage > creep.Health && Variables.Me.CanCast())
                                {
                                    Variables.Q.UseAbility(creep);
                                    Utils.Sleep(Variables.GetAbilityDelay(creep, Variables.Q), "Lasthit.Cast");
                                }
                            }
                            break;

                        case ClassId.CDOTA_Unit_Hero_Bristleback:
                            if (Variables.W.Level > 0 && Variables.W.CanBeCasted() && Variables.Me.Distance2D(creep) > MyHero.AttackRange())
                            {
                                double quillSprayDmg = 0;
                                var radius = Variables.W.GetAbilityData("radius");
                                var quillspraybaseDamage = Variables.W.GetAbilityData("quill_base_damage");
                                var quillspraystackDamage = Variables.W.GetAbilityData("quill_stack_damage");
                                if (creep.Modifiers.Any(
                                        x =>
                                            x.Name == "modifier_bristleback_quill_spray_stack" ||
                                            x.Name == "modifier_bristleback_quill_spray"))
                                    quillSprayDmg =
                                        creep.Modifiers.Find(
                                            x =>
                                                x.Name == "modifier_bristleback_quill_spray_stack" ||
                                                x.Name == "modifier_bristleback_quill_spray").StackCount * quillspraystackDamage;

                                damage = (quillspraybaseDamage + quillSprayDmg) * (1 - 0.06 * creep.Armor / (1 + 0.06 * creep.Armor));
                                if (damage > creep.Health && Variables.Me.Distance2D(creep) < radius)
                                {
                                    Variables.W.UseAbility();
                                    Utils.Sleep(Variables.GetAbilityDelay(creep, Variables.W), "Lasthit.Cast");
                                    Utils.Sleep(Variables.W.GetCooldown(Variables.W.Level) * 1000 + 50 + Game.Ping, "Lasthit.Cooldown");
                                }
                            }
                            break;

                        case ClassId.CDOTA_Unit_Hero_PhantomAssassin:
                            if (Variables.Q.CanBeCasted() && Variables.Me.Distance2D(creep) > MyHero.AttackRange() + 100)
                            {
                                //var stifflingdaggercastRange = Variables.Q.GetAbilityData("AbilityCastRange");
                                var stifflingdaggerSpeed = Variables.Q.GetAbilityData("dagger_speed");
                                var stifflingdaggerbaseDamage = Variables.Q.GetAbilityData("base_damage");
                                var stifflingdaggerattackFactor = Variables.Q.GetAbilityData("attack_factor_tooltip") / 100;
                                var time = Variables.Me.Distance2D(creep) / stifflingdaggerSpeed;
                                if (time < creep.SecondsPerAttack * 1000)
                                    damage = stifflingdaggerbaseDamage + (stifflingdaggerattackFactor * (Variables.Me.MinimumDamage + Variables.Me.BonusDamage)) * (1 - 0.06 * creep.Armor / (1 + 0.06 * creep.Armor));
                                //Console.WriteLine("Dagger damage: " + damage);
                                if (damage > creep.Health && Variables.Me.CanCast() && Variables.Q.CanBeCasted())
                                {
                                    Variables.Q.UseAbility(creep);
                                    Utils.Sleep(Variables.GetAbilityDelay(creep, Variables.Q), "Lasthit.Cast");
                                    Utils.Sleep(6 * 1000 + Game.Ping, "Lasthit.Cooldown");
                                }
                            }
                            break;

                        case ClassId.CDOTA_Unit_Hero_Pudge:
                            if (Variables.W.Level > 0)
                            {
                                if (Variables.CreeptargetH != null && creep.Handle == Variables.CreeptargetH.Handle &&
                                    Variables.Me.Distance2D(creep) <= MyHero.AttackRange())
                                {
                                    damage = GetDamageOnUnit(Variables.Me, creep, 0);
                                    if (damage > creep.Health && !Variables.W.IsToggled && Variables.Me.IsAttacking())
                                    {
                                        Variables.W.ToggleAbility();
                                        Utils.Sleep(200 + Game.Ping, "Lasthit.Cooldown");
                                    }
                                }
                                if (Variables.W.IsToggled)
                                {
                                    Variables.W.ToggleAbility();
                                    Utils.Sleep((float)Variables.HeroAPoint + Game.Ping, "Lasthit.Cooldown");
                                }
                            }
                            break;

                        case ClassId.CDOTA_Unit_Hero_Skywrath_Mage:
                            if (Variables.Q.Level > 0)
                            {
                                if (Variables.Q.CanBeCasted() && Variables.Me.Distance2D(creep) > MyHero.AttackRange())
                                {
                                    var boltSpeed = Variables.Q.GetAbilityData("bolt_speed");
                                    var boltDamage = Variables.Q.GetAbilityData("bolt_damage") + (Variables.Q.GetAbilityData("int_multiplier") * Variables.Me.TotalIntelligence);
                                    var reachTime = Variables.Me.Distance2D(creep) / boltSpeed;

                                    if (reachTime <= creep.SecondsPerAttack * 1000 && boltDamage > creep.Health && Variables.Me.CanCast() && 
                                        Variables.Q.CanBeCasted())
                                    {
                                        Variables.Q.UseAbility(creep);
                                        Utils.Sleep(Variables.GetAbilityDelay(creep, Variables.Q), "Lasthit.Cast");
                                        Utils.Sleep(Variables.Q.GetCooldown(Variables.Q.Level - 1) * 1000 + 50 + Game.Ping, "Lasthit.Cooldown");
                                    }
                                }
                            }
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e + "Error casting spell");
            }
        }

        #endregion Main

        #region Draws

        public static void Drawhpbar()
        {
            try
            {
                var creeps =
                    ObjectManager.GetEntitiesFast<Unit>()
                        .Where(
                            x =>
                                (x.ClassId == ClassId.CDOTA_BaseNPC_Tower ||
                                 x.ClassId == ClassId.CDOTA_BaseNPC_Creep_Lane
                                 || x.ClassId == ClassId.CDOTA_BaseNPC_Creep
                                 || x.ClassId == ClassId.CDOTA_BaseNPC_Creep_Neutral
                                 || x.ClassId == ClassId.CDOTA_BaseNPC_Creep_Siege
                                 || x.ClassId == ClassId.CDOTA_BaseNPC_Additive
                                 || x.ClassId == ClassId.CDOTA_BaseNPC_Barracks
                                 || x.ClassId == ClassId.CDOTA_BaseNPC_Building
                                 || x.ClassId == ClassId.CDOTA_BaseNPC_Creature) && x.IsAlive && x.IsVisible
                                && x.Distance2D(Variables.Me) < MyHero.AttackRange() + MenuVariables.Outrange);
                foreach (var creep in creeps)
                {
                    if (MenuVariables.Support && Variables.Me.Team != creep.Team ||
                        !MenuVariables.Support && Variables.Me.Team == creep.Team)
                        continue;
                    var health = creep.Health;
                    var maxHealth = creep.MaximumHealth;
                    if (health == maxHealth) continue;
                    var damge = (float)GetDamageOnUnitHpBar(Variables.Me, creep, 0);
                    var hpperc = health / maxHealth;

                    var hbarpos = HUDInfo.GetHPbarPosition(creep);

                    Vector2 screenPos;
                    var enemyPos = creep.Position + new Vector3(0, 0, creep.HealthBarOffset);
                    if (!Drawing.WorldToScreen(enemyPos, out screenPos)) continue;

                    var start = screenPos;

                    hbarpos.X = start.X - HUDInfo.GetHPBarSizeX(creep) / 2;
                    hbarpos.Y = start.Y;
                    var hpvarx = hbarpos.X;
                    var a = (float)Math.Floor(damge * HUDInfo.GetHPBarSizeX(creep) / creep.MaximumHealth);
                    var position = hbarpos + new Vector2(hpvarx * hpperc + 10, -12);
                    if (creep.ClassId == ClassId.CDOTA_BaseNPC_Tower)
                    {
                        hbarpos.Y = start.Y - HUDInfo.GetHpBarSizeY(creep) * 6;
                        position = hbarpos + new Vector2(hpvarx * hpperc - 5, -1);
                    }
                    else if (creep.ClassId == ClassId.CDOTA_BaseNPC_Barracks)
                    {
                        hbarpos.X = start.X - HUDInfo.GetHPBarSizeX(creep);
                        hbarpos.Y = start.Y - HUDInfo.GetHpBarSizeY(creep) * 6;
                        position = hbarpos + new Vector2(hpvarx * hpperc + 10, -1);
                    }
                    else if (creep.ClassId == ClassId.CDOTA_BaseNPC_Building)
                    {
                        hbarpos.X = start.X - HUDInfo.GetHPBarSizeX(creep) / 2;
                        hbarpos.Y = start.Y - HUDInfo.GetHpBarSizeY(creep);
                        position = hbarpos + new Vector2(hpvarx * hpperc + 10, -1);
                    }

                    Drawing.DrawRect(
                        position,
                        new Vector2(a, HUDInfo.GetHpBarSizeY(creep) - 4),
                        creep.Health > damge
                            ? creep.Health > damge * 2 ? new Color(180, 205, 205, 40) : new Color(255, 0, 0, 60)
                            : new Color(127, 255, 0, 80));
                    Drawing.DrawRect(position, new Vector2(a, HUDInfo.GetHpBarSizeY(creep) - 4), Color.Black, true);


                   /* if (!MenuVariables.Test) continue;
                    var attackPoint = Variables.Me.IsRanged == false
                    ? 0
                    : MyHero.GetMyAttackPoint(Variables.Me);
                    var time = Variables.Me.IsRanged == false
                        ? Variables.HeroAPoint / 1000 + Variables.Me.GetTurnTime(Variables.CreeptargetH.Position)
                        : Variables.HeroAPoint / 1000 + Variables.Me.GetTurnTime(Variables.CreeptargetH.Position) +
                          Variables.Me.Distance2D(Variables.CreeptargetH) / MyHero.GetProjectileSpeed(Variables.Me);
                    var getPred = Healthpredict(Variables.CreeptargetH, time + attackPoint);
                   // var getDamage = Math.Ceiling(GetDamageOnUnit(Variables.Me, Variables.CreeptargetH, 0));
                    var b = (float)Math.Round(getPred * 1 * HUDInfo.GetHPBarSizeX(creep) / creep.MaximumHealth);
                    var position2 = position + new Vector2(a, 0);
                    Drawing.DrawRect(position2, new Vector2(b, HUDInfo.GetHpBarSizeY(creep) - 4), Color.YellowGreen);
                    Drawing.DrawRect(position2, new Vector2(b, HUDInfo.GetHpBarSizeY(creep) - 4), Color.Black, true);*/
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error drawing HP Bar: " + e);
            }
        }

        private static double GetDamageOnUnitHpBar(Unit unit, Unit minion, double bonusdamage)
        {
            double modif = 1;
            double magicdamage = 0;
            double physDamage = unit.MinimumDamage + unit.BonusDamage;
            if (unit.Handle == Variables.Me.Handle)
            {
                var quellingBlade = unit.FindItem("item_quelling_blade");
                var ironTalon = unit.FindItem("item_iron_talon");
                if (ironTalon != null || quellingBlade != null && minion.Team != unit.Team)
                {
                    if (unit.IsRanged)
                    {
                        physDamage = unit.MinimumDamage + unit.BonusDamage + 7;
                    }
                    else
                    {
                        physDamage = unit.MinimumDamage + unit.BonusDamage + 24;
                    }
                }
                switch (unit.ClassId)
                {
                    case ClassId.CDOTA_Unit_Hero_AntiMage:
                        var manabreakDamage = Variables.Q.GetAbilityData("mana_per_hit");
                        if (minion.MaximumMana > 0 && minion.Mana > 0 && Variables.Q.Level > 0 && minion.Team != unit.Team)
                            bonusdamage += manabreakDamage * 0.6;
                        break;

                    case ClassId.CDOTA_Unit_Hero_Viper:
                        if (minion.Team != unit.Team)
                        {
                            var nethertoxindmg = Variables.W.GetAbilityData("bonus_damage") / (Variables.W.GetAbilityData("non_hero_damage_pct") / 100);
                            var percent = Math.Floor((double)minion.Health / minion.MaximumHealth * 100);
                            if (percent > 80 && percent <= 100)
                                bonusdamage += nethertoxindmg;
                            else if (percent > 60 && percent <= 80)
                                bonusdamage += nethertoxindmg * 2;
                            else if (percent > 40 && percent <= 60)
                                bonusdamage += nethertoxindmg * 4;
                            else if (percent > 20 && percent <= 40)
                                bonusdamage += nethertoxindmg * 8;
                            else if (percent > 0 && percent <= 20)
                                bonusdamage += nethertoxindmg * 16;
                        }
                        break;

                    case ClassId.CDOTA_Unit_Hero_Ursa:
                        var furymodif = 0;
                        var furyDamage = Variables.E.GetAbilityData("damage_per_stack");
                        var furyTalent = Variables.Me.Spellbook.Spells.First(x => x.Name == "special_bonus_unique_ursa").Level <= 0 ? 0 : 10;
                        if (unit.Modifiers.Any(x => x.Name == "modifier_ursa_fury_swipes_damage_increase"))
                            furymodif =
                                minion.Modifiers.Find(x => x.Name == "modifier_ursa_fury_swipes").StackCount;
                        if (minion.Team != unit.Team)
                        {
                            if (furymodif > 0)
                                bonusdamage += furymodif * (furyDamage + furyTalent);
                            else
                                bonusdamage += furyDamage + furyTalent;
                        }
                        break;

                    case ClassId.CDOTA_Unit_Hero_BountyHunter:
                        var jinadaDamage = Variables.W.GetAbilityData("crit_multiplier");
                        if (Variables.W.AbilityState == AbilityState.Ready)
                            bonusdamage += (physDamage * jinadaDamage / 100) - physDamage;
                        break;

                    case ClassId.CDOTA_Unit_Hero_Weaver:
                        if (Variables.E.Level > 0 && Variables.E.AbilityState == AbilityState.Ready)
                            bonusdamage += physDamage;
                        break;

                    case ClassId.CDOTA_Unit_Hero_Kunkka:
                        var tidebringerDamage = Variables.W.GetAbilityData("damage_bonus");
                        if (Variables.W.AbilityState == AbilityState.Ready &&
                            Variables.W.IsAutoCastEnabled)
                            bonusdamage += tidebringerDamage;
                        break;

                    case ClassId.CDOTA_Unit_Hero_Life_Stealer:
                        var feastDamage = Variables.E.GetAbilityData("hp_leech_percent");
                        bonusdamage += minion.Health * feastDamage;
                        break;

                }
                if (unit.Modifiers.Any(x => x.Name == "modifier_storm_spirit_overload"))
                {
                    var overloadDamage = Variables.E.GetAbilityData("AbilityDamage");
                    magicdamage += overloadDamage;
                }
                if (unit.Modifiers.Any(x => x.Name == "modifier_chilling_touch"))
                {
                    var chillingtouchDamage = Variables.E.GetAbilityData("bonus_damage");
                    var chillingtouchTalent = Variables.Me.Spellbook.Spells.First(x => x.Name == "special_bonus_unique_ancient_apparition_2").Level <= 0 ? 0 : 80;
                    magicdamage += chillingtouchDamage + chillingtouchTalent;
                }
                if (unit.ClassId == ClassId.CDOTA_Unit_Hero_Pudge && Variables.W.Level > 0 &&
                    MenuVariables.UseSpell && Variables.E.CanHit(minion))
                {
                    var rotDamage = Variables.E.GetAbilityData("rot_damage") / 2;
                    var rotTalent = Variables.Me.Spellbook.Spells.First(x => x.Name == "special_bonus_unique_pudge_2").Level <= 0 ? 0 : 120;
                    magicdamage += rotDamage + rotTalent;
                }
            }
            if (unit.Modifiers.Any(x => x.Name == "modifier_bloodseeker_bloodrage"))
            {
                modif = modif *
                        (ObjectManager.GetEntitiesFast<Hero>()
                            .First(x => x.ClassId == ClassId.CDOTA_Unit_Hero_Bloodseeker)
                            .Spellbook.Spell1.Level - 1) * 0.05 + 1.25;
            }

            var damageMp = 1 - 0.06 * minion.Armor / (1 + 0.06 * Math.Abs(minion.Armor));
            magicdamage = magicdamage * (1 - minion.MagicDamageResist);

            var realDamage = ((bonusdamage + physDamage) * damageMp + magicdamage) * modif;
            if (minion.ClassId == ClassId.CDOTA_BaseNPC_Creep_Siege ||
                minion.ClassId == ClassId.CDOTA_BaseNPC_Tower)
            {
                realDamage = realDamage / 2;
            }
            if (realDamage > minion.MaximumHealth)
                realDamage = minion.Health + 10;

            return realDamage;
        }

        #endregion Draws
    }
}
