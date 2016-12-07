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
                            "modifier_venomancer_venomous_gale", "modifier_item_urn_damage"
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
