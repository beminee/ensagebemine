using Ensage.Common.Extensions;

namespace PudgeSharpSDK
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel.Composition;
    using System.Linq;

    using Ensage;
    using Ensage.Common;
    using Ensage.Common.Enums;
    using Ensage.Common.Menu;
    using Ensage.SDK.Inventory;
    using Ensage.SDK.Extensions;
    using Ensage.SDK.Menu;
    using Ensage.SDK.Orbwalker;
    using Ensage.SDK.TargetSelector;
    using Ensage.SDK.Prediction;
    using Ensage.SDK.Prediction.Collision;
    using Ensage.SDK.Service;
    using Ensage.SDK.Service.Metadata;

    [ExportPlugin("PudgeSharpSDK", HeroId.npc_dota_hero_pudge)]
    public class PudgeSharpSDK : Plugin, IOrbwalkingMode
    {
        private readonly Lazy<IOrbwalkerManager> orbwalkerManager;

        private readonly Lazy<ITargetSelectorManager> targetManager;

        [ImportingConstructor]
        public PudgeSharpSDK([Import] Lazy<IOrbwalkerManager> orbManager, [Import] Lazy<ITargetSelectorManager> targetManager, [Import] IInventoryManager inventory, Lazy<IPrediction> prediction) // import IOrbwalker, ITargetSelectorManager and IInventoryManager
        {
            this.orbwalkerManager = orbManager;
            this.targetManager = targetManager;
            this.Inventory = inventory;
            this.Prediction = prediction;
        }


        public MyHeroConfig Config { get; private set; }

        private IOrbwalker Orbwalker => this.orbwalkerManager.Value.Active;

        private ITargetSelector TargetSelector => this.targetManager.Value.Active;

        private IInventoryManager Inventory { get; }

        private Lazy<IPrediction> Prediction { get; }

        private Ability Rot { get; set; }

        private Ability Ulti { get; set; }

        private Ability Hook { get; set; }

        private bool HasHookModif;

        public bool CanExecute => this.Config.Key;

        public void Execute()
        {
            var me = ObjectManager.LocalHero;
            var target = this.TargetSelector.GetTargets().FirstOrDefault(x => x.Distance2D(me) <= 2000);
            var targets = this.TargetSelector.GetTargets().AsParallel().Cast<Hero>();

            Hook = me.Spellbook.SpellQ;
            Rot = me.Spellbook.SpellW;
            Ulti = me.Spellbook.SpellR;
            if (!me.IsChanneling()) return;
            if (!me.IsSilenced())
            {
                if (me.IsChanneling() || me.HasModifier("modifier_pudge_dismember"))
                {
                    if (this.Config.AbilityValue.IsEnabled(this.Rot.Name) && this.Rot.CanBeCasted() && !this.Rot.IsToggled && Utils.SleepCheck("PudgeSharp.Rot"))
                    {
                        this.Rot.ToggleAbility();
                        Utils.Sleep(100, "PudgeSharp.Rot");
                    }
                }

                foreach (var hooktarget in targets)
                {
                    if (hooktarget == null || !this.Config.AbilityToggler.Value.IsEnabled(this.Hook.Name) ||
                        !this.Hook.CanBeCasted(hooktarget)) continue;
                    const float delay = 0.3f;
                    var speed = this.Hook.GetAbilitySpecialData("hook_speed");
                    var radius = this.Hook.GetAbilitySpecialData("hook_width");
                    var range = this.Hook.GetAbilitySpecialData("hook_distance");

                    if (hooktarget.Distance2D(me) > range)
                    {
                        continue;
                    }

                    var input =
                        new PredictionInput(
                            me,
                            hooktarget,
                            delay,
                            speed,
                            range,
                            radius,
                            PredictionSkillshotType.SkillshotLine,
                            true)
                        {
                            CollisionTypes =
                                CollisionTypes.AllyCreeps | CollisionTypes.EnemyCreeps | CollisionTypes.AllyHeroes |
                                CollisionTypes.EnemyHeroes
                        };

                    var output = this.Prediction.Value.GetPrediction(input);

                    if (output.HitChance < Config.HookTolerance.Item.GetValue<HitChance>())
                    {
                        me.Hold();
                        Utils.Sleep(400, "PudgeSharpSDK.Stopping");
                    }
                    else if (output.HitChance >= Config.HookTolerance.Item.GetValue<HitChance>() && Utils.SleepCheck("PudgeSharpSDK.Hook"))
                    {
                        this.Hook.UseAbility(output.CastPosition);
                        Utils.Sleep(500, "PudgeSharpSDK.Hook");
                    }
                }

                if (HasHookModif && this.Config.AbilityValue.IsEnabled(this.Rot.Name) && this.Rot.CanBeCasted() && !this.Rot.IsToggled && Utils.SleepCheck("PudgeSharp.Rot"))
                {
                    this.Rot.ToggleAbility();
                    Utils.Sleep(100, "PudgeSharp.Rot");
                }
                else if (target == null && !me.IsChanneling() && this.Rot.CanBeCasted() && this.Rot.IsToggled && Utils.SleepCheck("PudgeSharp.RotDisable"))
                {
                    this.Rot.ToggleAbility();
                    Utils.Sleep(100, "PudgeSharp.RotDisable");
                }
                if (!Config.AbilityValue.IsEnabled(this.Ulti.Name) && this.Ulti.CanBeCasted() && Utils.SleepCheck("PudgeSharpSDK.Ulti"))
                {
                    this.Ulti.UseAbility(target);
                    Utils.Sleep(500, "PudgeSharp.Ulti");
                }
                else if (this.Config.StopOnHook && target.Distance2D(me) < this.Ulti.CastRange)
                {
                    me.Hold();
                }
                return;
            }

            if (this.Orbwalker.OrbwalkTo(target))
            {
                //
            }

        }

        private void OnModifierAdded(Unit sender, ModifierChangedEventArgs args)
        {
            var target = TargetSelector.GetTargets().FirstOrDefault();
            if (target != null && sender == target && args.Modifier.Name == "modifier_pudge_meat_hook")
            {
                HasHookModif = true;
            }
        }

        private void OnModifierRemoved(Unit sender, ModifierChangedEventArgs args)
        {
            if (args.Modifier.Name == "modifier_pudge_meat_hook")
            {
                HasHookModif = false;
            }
        }

        protected override void OnActivate()
        {
            this.Config = new MyHeroConfig(); // create menus

            if (!this.Config.TogglerSet)
            {
                this.Config.AbilityValue = this.Config.AbilityToggler.Value;
                this.Config.TogglerSet = true;
            }

            this.orbwalkerManager.Value.RegisterMode(this);
            Unit.OnModifierRemoved += OnModifierRemoved;
            Unit.OnModifierAdded += OnModifierAdded;
        }

        protected override void OnDeactivate()
        {
            this.orbwalkerManager.Value.UnregisterMode(this);
            this.Config.Dispose();
            Unit.OnModifierRemoved -= OnModifierRemoved;
            Unit.OnModifierAdded -= OnModifierAdded;
        }

    }

    public class MyHeroConfig : IDisposable
    {
        public AbilityToggler ItemValue, AbilityValue;

        public bool TogglerSet = false;

        public MyHeroConfig()
        {

            var spellDict = new Dictionary<string, bool>
                           {
                               { "pudge_meat_hook", true },
                               { "pudge_rot", true },
                               { "pudge_dismember", true }
                           };

            this.Menu = MenuFactory.Create("PudgeSharpSDK");
            this.HookTolerance = this.Menu.Item("Hook tolerance?", new Slider(3, 3, 8));
            this.HookTolerance.Item.Tooltip = "Set accuracy of hook. Lower the value harder the hook => Higher chance to miss.";
            this.StopOnHook = this.Menu.Item("Stops after using hook so you don't run towards enemy", true);
            this.Key = this.Menu.Item("Combo Key", new KeyBind(32, KeyBindType.Press));
            this.AbilityToggler = this.Menu.Item("Ability Toggler", new AbilityToggler(spellDict));
        }

        public void Dispose()
        {
            this.Menu?.Dispose();
        }

        public MenuFactory Menu { get; }

        public MenuItem<Slider> HookTolerance { get; }

        public MenuItem<bool> StopOnHook { get; }

        public MenuItem<AbilityToggler> AbilityToggler { get; }

        public MenuItem<KeyBind> Key { get; }
    }
}