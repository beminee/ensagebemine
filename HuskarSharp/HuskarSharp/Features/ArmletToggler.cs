using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common.Extensions;
using Ensage.Common.Objects;
using Ensage.Common.Objects.UtilityObjects;

namespace HuskarSharp.Features
{
    public class ArmletToggler
    {
        public ArmletToggler(Item armlet)
        {
            this.Armlet = armlet;
            this.Sleeper = new Sleeper();
        }

        public Item Armlet { get; set; }

        public bool CanToggle
        {

            get
            {
                var position = Variables.Hero.IsMoving && Math.Abs(Variables.Hero.RotationDifference) < 60
                               ? Variables.Hero.InFront(100)
                               : Variables.Hero.NetworkPosition;

                var heroProjectiles =
                ObjectManager.TrackingProjectiles.Where(
                    x => x?.Target is Hero && x.Source is Unit && x.Target.Equals(Variables.Hero)).ToList();

                var noProjectiles =
                heroProjectiles.All(
                    x =>
                        x.Position.Distance2D(position) / x.Speed > 0.30
                        || x.Position.Distance2D(position) / x.Speed < Game.Ping / 1000);

                var nearEnemies =
                ObjectManager.GetEntitiesParallel<Unit>()
                    .Where(
                        x =>
                            x.IsValid && x.IsAlive && x.IsSpawned && x.AttackCapability != AttackCapability.None
                            && x.Team == Variables.EnemyTeam && x.Distance2D(Variables.Hero) < x.GetAttackRange() + 200);

                var noAutoAttacks = nearEnemies.All(x => x.FindRelativeAngle(Variables.Hero.Position) > 0.5 || !x.IsAttacking());

                if (!noProjectiles && !noAutoAttacks) return false;

                return this.Armlet.IsValid;
            }
        }

        public Sleeper Sleeper { get; private set; }

        public void TurnOn()
        {
            if (!Variables.Hero.CanUseItems())
            {
                return;
            }
            if (!Variables.Hero.HasModifier("modifier_item_armlet_unholy_strength"))
            {
                this.Armlet.ToggleAbility();
            }
            else if (Variables.Hero.Health < Variables.ArmletThreshold)
            {
                this.Armlet.ToggleAbility();
                this.Armlet.ToggleAbility();
            }
        }

        public void TurnOff()
        {
            if (!Variables.Hero.CanUseItems())
            {
                return;
            }
            if (Variables.Hero.HasModifier("modifier_item_armlet_unholy_strength"))
            {
                this.Armlet.ToggleAbility();
            }
        }

        public void Toggle()
        {
            if (!Variables.Hero.CanUseItems())
            {
                return;
            }

            if (
                !Heroes.GetByTeam(Variables.EnemyTeam)
                     .Any(
                         x =>
                         x.IsValid && x.IsAlive && x.IsVisible
                         && x.Distance2D(Variables.Hero) < 800)
                && !Variables.Hero.HasModifiers(
                    new[]
                        {
                            "modifier_axe_battle_hunger", "modifier_queenofpain_shadow_strike",
                            "modifier_phoenix_fire_spirit_burn", "modifier_venomancer_poison_nova",
                            "modifier_venomancer_venomous_gale", "modifier_item_urn_damage",
							"modifier_necrolyte_heartstopper_aura_effect", "modifier_pudge_rot",
							"modifier_death_prophet_spirit_siphon_slow", "modifier_disruptor_static_storm",
							"modifier_ice_blast"
                        },
                    false))
            {
                return;
            }


            if (Variables.Hero.HasModifier("modifier_item_armlet_unholy_strength") || this.Armlet.IsToggled)
            {
                this.Armlet.ToggleAbility();
                this.Armlet.ToggleAbility();
            }
            else
            {
                this.Armlet.ToggleAbility();
            }
        }
    }
}
