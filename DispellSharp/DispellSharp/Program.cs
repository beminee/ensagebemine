using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Ensage.SDK.Abilities.Aggregation;
using Ensage.SDK.Abilities.Items;
using Ensage.SDK.Extensions;
using Ensage.SDK.Handlers;
using Ensage.SDK.Helpers;
using Ensage.SDK.Inventory.Metadata;
using Ensage.SDK.Menu;
using Ensage.SDK.Service.Metadata;
using Ensage.SDK.TargetSelector;
using log4net;
using PlaySharp.Toolkit.Helper.Annotations;
using PlaySharp.Toolkit.Logging;

namespace DispellSharp
{
    using System;
    using System.Linq;
    using Ensage.SDK.Service;
    using Ensage;
    using Ensage.Common;
    using Ensage.Common.Extensions;
    using Ensage.Common.Menu;

    [PublicAPI]
    [ExportPlugin("DispellSharp", StartupMode.Auto, "beminee", "2.0.0.0", "Global Dispell Assembly")]
    public class Program : Plugin
    {
        private readonly IServiceContext context;

        private Unit Owner;

        public TaskHandler Handler { get; private set; }

        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [ItemBinding]
        private item_manta Manta { get; set; }
        [ItemBinding]
        private item_nullifier Nullifier { get; set; }
        [ItemBinding]
        private item_lotus_orb LotusOrb { get; set; }

        [ImportingConstructor]
        public Program(
            [Import] IServiceContext context)
        {
            this.context = context;
            this.Owner = context.Owner;
        }

        public DispellSharpConfig Config { get; private set; }

        protected override void OnActivate()
        {
            this.context.Inventory.Attach(this);

            this.Config = new DispellSharpConfig();
            this.Handler = UpdateManager.Run(this.OnUpdate);
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            this.context.Inventory.Detach(this);

            this.Handler.Cancel();
            this.Config.Dispose();
            base.OnDeactivate();
        }

        private async Task OnUpdate(CancellationToken token)
        {
            if (Game.IsPaused || !this.Owner.IsAlive || this.Owner.IsMuted() || !this.Owner.CanUseItems())
            {
                await Task.Delay(250, token);
                return;
            }

            try
            {
                if (Manta != null && Manta.CanBeCasted && IsSilenced &&
                    this.Config.Silences.Value)
                {
                    Manta.UseAbility();
                    await Task.Delay(Manta.GetCastDelay(), token);
                }
                else if (LotusOrb != null && LotusOrb.CanBeCasted && IsSilenced &&
                         this.Config.Silences.Value)
                {
                    LotusOrb.UseAbility(this.Owner);
                    await Task.Delay(LotusOrb.GetCastDelay(), token);
                }

                if (Manta != null && Manta.CanBeCasted && this.Owner.HasModifier("modifier_item_dustofappearance") &&
                    this.Config.Dust.Value)
                {
                    Manta.UseAbility();
                    await Task.Delay(Manta.GetCastDelay(), token);
                }
                else if (LotusOrb != null && LotusOrb.CanBeCasted &&
                         this.Owner.HasModifier("modifier_item_dustofappearance") &&
                         this.Config.Dust.Value)
                {
                    LotusOrb.UseAbility(this.Owner);
                    await Task.Delay(LotusOrb.GetCastDelay(), token);
                }

                var enemiesAround = EntityManager<Hero>.Entities
                    .Where(x => x.IsValid && x.IsAlive && !x.IsAlly(this.Owner) &&
                                x.Distance2D(this.Owner) <= this.Nullifier?.CastRange).ToList();

                foreach (var enemy in enemiesAround)
                {
                    if (enemy != null && !enemy.IsMagicImmune() && Nullifier.CanBeCasted)
                    {
                        if (enemy.IsEthereal() && this.Config.EnemyCleanseToggler.Value.IsEnabled("item_ghost"))
                        {
                            this.Nullifier.UseAbility(enemy);
                            await Task.Delay(Nullifier.GetCastDelay(), token);
                        }
                        else if (enemy.HasModifier("modifier_item_aeon_disk_buff") &&
                                 this.Config.EnemyCleanseToggler.Value.IsEnabled("item_aeon_disk"))
                        {
                            this.Nullifier.UseAbility(enemy);
                            await Task.Delay(Nullifier.GetCastDelay(), token);
                        }
                        else if (enemy.HasModifier("modifier_omninight_guardian_angel") &&
                                 this.Config.EnemyCleanseToggler.Value.IsEnabled("omniknight_guardian_angel"))
                        {
                            this.Nullifier.UseAbility(enemy);
                            await Task.Delay(Nullifier.GetCastDelay(), token);
                        }
                        else if (enemy.HasModifier("modifier_eul_cyclone") &&
                                 this.Config.EnemyCleanseToggler.Value.IsEnabled("item_cyclone"))
                        {
                            this.Nullifier.UseAbility(enemy);
                            await Task.Delay(Nullifier.GetCastDelay(), token);
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // ignore
            }
            catch (Exception e)
            {
                Log.Debug($"Exception: {e}");
            }
        }

        private bool IsSilenced
        {
            get
            {
                return (this.Owner.IsSilenced() || 
                        this.Owner.HasModifier("modifier_silencer_last_word")) &&
                       !this.Owner.HasModifier("modifier_night_stalker_crippling_fear_aura");
            }
        }
    }

    public class DispellSharpConfig : IDisposable
    {
        private bool _disposed;

        public MenuFactory Menu { get; }

        public MenuItem<bool> Silences { get; set; }
        public MenuItem<bool> Dust { get; set; }

        public MenuItem<AbilityToggler> EnemyCleanseToggler { get; set; }

        public DispellSharpConfig()
        {
            var enemyCleanse = new Dictionary<string, bool>
            {
                {"item_ghost", true},
                {"omniknight_guardian_angel", true},
                {"item_cyclone", true},
                {"item_aeon_disk", true }
            };

            this.Menu = MenuFactory.Create("DispellSharp");
            this.Silences = this.Menu.Item("Dispell Silence with Manta", true);
            this.Silences.Item.Tooltip =
                "Setting this to false will disable dispelling silence effects with Manta.";
            this.Dust = this.Menu.Item("Dispell Dust with Manta", true);
            this.Dust.Item.Tooltip = "Setting this to false will disable dispelling dust effect with Manta.";
            this.EnemyCleanseToggler = this.Menu.Item("Enemy Cleanse Toggler", new AbilityToggler(enemyCleanse));
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this._disposed)
            {
                return;
            }

            if (disposing)
            {
                this.Menu.Dispose();
            }

            this._disposed = true;
        }
    }
}