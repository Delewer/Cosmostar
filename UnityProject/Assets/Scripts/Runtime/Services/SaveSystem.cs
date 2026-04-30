using System.IO;
using Cosmostar.Core.Models;
using UnityEngine;

namespace Cosmostar.Runtime.Services
{
    public sealed class SaveSystem
    {
        private readonly string _savePath;

        public SaveSystem()
        {
            _savePath = Path.Combine(Application.persistentDataPath, "cosmostar_profile.json");
        }

        public SaveProfile Load()
        {
            if (!File.Exists(_savePath))
            {
                return new SaveProfile();
            }

            var json = File.ReadAllText(_savePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new SaveProfile();
            }

            return JsonUtility.FromJson<SaveProfile>(json) ?? new SaveProfile();
        }

        public void Save(SaveProfile profile)
        {
            var json = JsonUtility.ToJson(profile, true);
            File.WriteAllText(_savePath, json);
        }
    }
}

