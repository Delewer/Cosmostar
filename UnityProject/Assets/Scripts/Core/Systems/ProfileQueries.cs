#nullable disable

using System.Collections.Generic;
using Cosmostar.Core.Models;

namespace Cosmostar.Core.Systems
{
    public static class ProfileQueries
    {
        public static void EnsureDefaultState(SaveProfile profile, VerticalSliceCatalog catalog)
        {
            if (profile.Modules.Count == 0)
            {
                for (var index = 0; index < catalog.Modules.Count; index++)
                {
                    var module = catalog.Modules[index];
                    profile.Modules.Add(new ModuleProgress
                    {
                        ModuleId = module.Id,
                        Level = index == 0 ? 1 : 0,
                        Unlocked = index == 0,
                        Equipped = index == 0
                    });
                }
            }

            if (profile.Missions.Count == 0)
            {
                for (var index = 0; index < catalog.Missions.Count; index++)
                {
                    profile.Missions.Add(new MissionProgress
                    {
                        MissionId = catalog.Missions[index].Id
                    });
                }
            }
        }

        public static ModuleProgress GetModuleProgress(SaveProfile profile, string moduleId)
        {
            for (var index = 0; index < profile.Modules.Count; index++)
            {
                if (profile.Modules[index].ModuleId == moduleId)
                {
                    return profile.Modules[index];
                }
            }

            return null;
        }

        public static MissionProgress GetMissionProgress(SaveProfile profile, string missionId)
        {
            for (var index = 0; index < profile.Missions.Count; index++)
            {
                if (profile.Missions[index].MissionId == missionId)
                {
                    return profile.Missions[index];
                }
            }

            return null;
        }

        public static List<string> GetEquippedModuleIds(SaveProfile profile)
        {
            var result = new List<string>();
            for (var index = 0; index < profile.Modules.Count; index++)
            {
                if (profile.Modules[index].Unlocked && profile.Modules[index].Equipped)
                {
                    result.Add(profile.Modules[index].ModuleId);
                }
            }

            return result;
        }

        public static int GetEquippedModuleCount(SaveProfile profile)
        {
            var count = 0;
            for (var index = 0; index < profile.Modules.Count; index++)
            {
                if (profile.Modules[index].Unlocked && profile.Modules[index].Equipped)
                {
                    count += 1;
                }
            }

            return count;
        }
    }
}
