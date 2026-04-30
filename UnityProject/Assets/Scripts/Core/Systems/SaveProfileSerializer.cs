using System.Text.Json;
using Cosmostar.Core.Models;

namespace Cosmostar.Core.Systems
{
    public static class SaveProfileSerializer
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            IncludeFields = true,
            WriteIndented = true
        };

        public static string Serialize(SaveProfile profile)
        {
            return JsonSerializer.Serialize(profile, Options);
        }

        public static SaveProfile Deserialize(string json)
        {
            return JsonSerializer.Deserialize<SaveProfile>(json, Options) ?? new SaveProfile();
        }
    }
}

