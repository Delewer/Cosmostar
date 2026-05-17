using Cosmostar.Core.Models;

namespace Cosmostar.Runtime.Services
{
    public interface IAdsService
    {
        bool TryShowRewarded(RewardedPlacement placement, out string rewardMessage);
    }

    public sealed class MockAdsService : IAdsService
    {
        public bool TryShowRewarded(RewardedPlacement placement, out string rewardMessage)
        {
            switch (placement)
            {
                case RewardedPlacement.Revive:
                    rewardMessage = "Backup spark recharged.";
                    return true;
                case RewardedPlacement.UpgradeReroll:
                    rewardMessage = "Draft rerolled.";
                    return true;
                default:
                    rewardMessage = "Rewards doubled.";
                    return true;
            }
        }
    }
}
