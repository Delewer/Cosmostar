using NeonSkySurvivors.Core.Models;
using UnityEngine;

namespace NeonSkySurvivors.Runtime.App
{
    /// <summary>
    /// Minimal local save for the MVP. Serializes the profile to PlayerPrefs as JSON.
    /// Mirrors the web prototype's localStorage persistence (coins, inventory, equipped
    /// loadout, run progress) so garage upgrades/merges survive between sessions.
    /// </summary>
    public static class NeonSaveService
    {
        private const string SaveKey = "neon_sky_survivors_profile_v1";

        public static NeonSaveProfile Load()
        {
            if (!PlayerPrefs.HasKey(SaveKey))
            {
                return null!;
            }

            var json = PlayerPrefs.GetString(SaveKey, string.Empty);
            if (string.IsNullOrWhiteSpace(json))
            {
                return null!;
            }

            var profile = JsonUtility.FromJson<NeonSaveProfile>(json);
            return profile ?? null!;
        }

        public static void Save(NeonSaveProfile profile)
        {
            if (profile == null)
            {
                return;
            }

            var json = JsonUtility.ToJson(profile);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }
    }
}
