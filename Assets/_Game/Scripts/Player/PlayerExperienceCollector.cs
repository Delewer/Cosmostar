using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerExperienceCollector : MonoBehaviour
{
    [SerializeField] private ExperienceSystem experienceSystem;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<ExperiencePickup>(out var pickup)) return;

        experienceSystem?.AddExperience(pickup.Amount);
        pickup.Collect();
    }
}
