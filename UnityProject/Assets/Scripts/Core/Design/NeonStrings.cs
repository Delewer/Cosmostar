using System.Collections.Generic;

namespace NeonSkySurvivors.Core.Design
{
    /// <summary>
    /// Lightweight localization string table.
    /// Keys follow the pattern "screen.element[.modifier]".
    /// Call NeonStrings.Get(key) everywhere UI copy is needed; swap the table via Load()
    /// to support additional locales without changing call sites.
    /// </summary>
    public static class NeonStrings
    {
        private static Dictionary<string, string> _table = BuildDefault();

        /// <summary>Returns the localized string for <paramref name="key"/>, or <paramref name="key"/> itself if not found.</summary>
        public static string Get(string key) =>
            _table.TryGetValue(key, out var v) ? v : key;

        /// <summary>Replace the active string table (call once on app start or locale change).</summary>
        public static void Load(Dictionary<string, string> table) => _table = table;

        /// <summary>Revert to the built-in English strings.</summary>
        public static void ResetToDefault() => _table = BuildDefault();

        private static Dictionary<string, string> BuildDefault() => new Dictionary<string, string>
        {
            // HUD
            { "hud.wave.format",         "WAVE {0} · LV {1}" },
            { "hud.sector.prefix",       "S" },
            { "hud.kills.label",         "KILLS" },

            // Main menu
            { "menu.title",              "NEON SKY SURVIVORS" },
            { "menu.subtitle",           "SURVIVE THE NEON SKIES" },
            { "menu.play",               "PLAY" },
            { "menu.settings",           "SETTINGS" },
            { "menu.daily_ops",          "— DAILY OPS —" },
            { "menu.mission.claimed",    "✓ CLAIMED" },
            { "menu.mission.complete",   "COMPLETE" },

            // Garage
            { "garage.title",            "NEON SKY SURVIVORS — GARAGE" },
            { "garage.start_run",        "START RUN" },
            { "garage.missions",         "Missions" },
            { "garage.achievements",     "Achievements" },
            { "garage.back",             "< Menu" },
            { "garage.sector.base",      "SECTOR 1 — base difficulty" },
            { "garage.sector.unlock_hint", "win a run to unlock Sector 2" },

            // Settings
            { "settings.title",          "SETTINGS" },
            { "settings.music",          "Music" },
            { "settings.sfx",            "SFX" },
            { "settings.vibration",      "Vibration" },
            { "settings.dash_mode",      "Dash" },
            { "settings.reduced_motion", "Reduced Motion" },
            { "settings.button",         "BUTTON" },
            { "settings.double_tap",     "DOUBLE-TAP" },
            { "settings.on",             "ON" },
            { "settings.off",            "OFF" },
            { "settings.back",           "Back" },

            // Missions panel
            { "missions.title",          "DAILY MISSIONS" },
            { "missions.claimed",        "✓ Claimed" },
            { "missions.complete",       "— COMPLETE!" },
            { "missions.weekly_prefix",  "⚡ WEEKLY: " },
            { "missions.back",           "Back" },

            // Pilot rank card
            { "pilot.rank_header",       "PILOT RANK" },
            { "pilot.perks_prefix",      "PERKS: " },
            { "pilot.perks_none",        "none yet" },
            { "pilot.next_prefix",       "NEXT: " },
            { "pilot.max_level",         "MAX LEVEL" },
            { "pilot.extra_reroll",      "{0} Reroll/run" },
            { "pilot.extra_rerolls",     "{0} Rerolls/run" },
            { "pilot.extra_banish",      "{0} Banish/run" },
            { "pilot.extra_banishes",    "{0} Banishes/run" },

            // Achievements panel
            { "achievements.title",      "ACHIEVEMENTS" },
            { "achievements.codex",      "PILOT CODEX" },
            { "achievements.unlocked",   "UNLOCKED" },
            { "achievements.locked",     "Locked" },
            { "achievements.count",      "Achievements: {0} / {1}" },
            { "achievements.back",       "Back" },

            // Results screen
            { "results.victory",         "VICTORY" },
            { "results.defeated",        "DEFEATED" },
            { "results.time_label",      "TIME SURVIVED" },
            { "results.kills_label",     "KILLS" },
            { "results.bosses_label",    "BOSSES" },
            { "results.coins_label",     "COINS" },
            { "results.total_label",     "TOTAL" },
            { "results.best_label",      "BEST" },
            { "results.runs_label",      "RUNS" },
            { "results.salvage_prefix",  "SALVAGE: " },
            { "results.continue",        "CONTINUE" },

            // Pause menu
            { "pause.title",             "PAUSED" },
            { "pause.resume",            "RESUME" },
            { "pause.restart",           "RESTART" },
            { "pause.quit",              "QUIT TO GARAGE" },

            // Upgrade draft
            { "draft.level_up",          "LEVEL UP" },
            { "draft.reroll_prefix",     "REROLL·" },
            { "draft.banish",            "✕" },

            // Run messages
            { "run.intro.format",        "Survive 10 minutes. Bosses at 3:00, 6:00, 7:30, 8:45, 10:00." },
            { "run.sector_intro.format", "SECTOR {0} — Survive 10 minutes. Bosses at 3:00, 6:00, 7:30, 8:45, 10:00." },
            { "run.weapon_evolved",      "WEAPON EVOLVED!" },
            { "run.special_ready",       "SPECIAL READY" },

            // General
            { "general.back",            "Back" },
            { "general.claim",           "Claim!" },
            { "general.toggle",          "Toggle" },
        };
    }
}
