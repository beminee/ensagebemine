using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.Common.Objects;

using SharpDX;
using Ensage.Common.Extensions.SharpDX;
using Ensage.Common.Objects.UtilityObjects;

namespace KaelSharp
{
   internal class Variables
    {
        public enum Abilities
        {
            None,
            Emp, // mana_burned
            Tornado, // lift_duration -- modifier_invoker_tornado
            Alacrity,
            GhostWalk,
            DeafeningBlast,
            ChaosMeteor,
            ColdSnap,
            IceWall,
            ForgeSpirit,
            SunStrike
        }

        public static readonly string TornadoModif = "modifier_invoker_tornado";

        public static string[] AbilitiesList =
        {
            "None", "EMP", "Tornado", "Alacrity", "Ghost Walk", "Deafening Blast", "Chaos Meteor", "Cold Snap",
            "Ice Wall", "Forge Spirit", "Sun Strike"
        };

        public static readonly string[] CantAttackModifiers =
        {
            "modifier_obsidian_destroyer_astral_imprisonment_prison",
            "modifier_abaddon_borrowed_time",
            "modifier_brewmaster_primal_split",
            "modifier_phoenix_supernova_hiding",
            "modifier_juggernaut_omnislash_invulnerability",
            "modifier_naga_siren_song_of_the_siren",
            "modifier_puck_phase_shift",
            "modifier_shadow_demon_disruption",
            "modifier_winter_wyvern_winters_curse_aura",
            "modifier_winter_wyvern_winters_curse",
            "modifier_storm_spirit_ball_lightning"
        };

        public static readonly string[] DangerousRightClickers =
        {
            "npc_dota_hero_anti_mage",
            "npc_dota_hero_clinkz",
            "npc_dota_hero_drow_ranger",
            "npc_dota_hero_huskar",
            "npc_dota_hero_meepo",
            "npc_dota_hero_obsidian_destroyer",
            "npc_dota_hero_phantom_assassin",
            "npc_dota_hero_skeleton_king",
            "npc_dota_hero_slark",
            "npc_dota_hero_spectre",
            "npc_dota_hero_sven",
            "npc_dota_hero_templar_assassin",
            "npc_dota_hero_terrorblade",
            "npc_dota_hero_troll_warlord",

        };

        public static Menu Menu;
        public static Hero Me, Target;
        public static Ability Quas, Wex, Exort, D, F, Invoke;
        public static float EulmodifTime;
        public static bool HasAghs;
        public static Dictionary<float, Orbwalker> OrbWalkerDictionary = new Dictionary<float, Orbwalker>();


    }

    internal class MenuVariables
    {
        #region MenuVariables

        public static bool Enabled;
        public static bool Cancer;
        public static bool SunStrike;
        public static uint Flee;
        public static int Distance;
        public static bool Orbwalk;
        public static bool Debug1;
        public static bool Debug2;
        public static uint ComboKey;
        public static uint PrepareKey;
        #endregion MenuVariables
    }
}
