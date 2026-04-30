using UnityEngine;

public class SynergyEffectApplier : MonoBehaviour
{
    [SerializeField] private SynergyResolver synergyResolver;
    [SerializeField] private PlayerController player;

    private void OnEnable()
    {
        if (synergyResolver != null)
        {
            synergyResolver.OnSynergyUnlocked += HandleSynergyUnlocked;
        }
    }

    private void OnDisable()
    {
        if (synergyResolver != null)
        {
            synergyResolver.OnSynergyUnlocked -= HandleSynergyUnlocked;
        }
    }

    private void HandleSynergyUnlocked(string synergyKey)
    {
        if (player == null || string.IsNullOrWhiteSpace(synergyKey)) return;

        switch (synergyKey.Trim().ToLowerInvariant())
        {
            case "solar_flare":
                player.ApplySynergyBonus(fireRateMultiplier: 1.12f, damageMultiplier: 1.18f, critChanceBonus: 0.05f);
                break;
            default:
                Debug.Log($"No gameplay effect configured for synergy '{synergyKey}'.");
                break;
        }
    }
}
