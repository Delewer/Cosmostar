using System.Collections.Generic;
using UnityEngine;

public class ExperiencePickupPool : MonoBehaviour
{
    [SerializeField] private ExperiencePickup prefab;
    [SerializeField] private int initialSize = 64;

    private readonly Queue<ExperiencePickup> pool = new();

    private void Awake()
    {
        for (int i = 0; i < initialSize; i++)
        {
            ExperiencePickup pickup = CreateInstance();
            Return(pickup);
        }
    }

    public ExperiencePickup Get()
    {
        ExperiencePickup pickup = pool.Count > 0 ? pool.Dequeue() : CreateInstance();
        pickup.gameObject.SetActive(true);
        return pickup;
    }

    public void Return(ExperiencePickup pickup)
    {
        if (pickup == null) return;
        pickup.gameObject.SetActive(false);
        pool.Enqueue(pickup);
    }

    private ExperiencePickup CreateInstance()
    {
        ExperiencePickup pickup = Instantiate(prefab, transform);
        pickup.OnExpired -= Return;
        pickup.OnExpired += Return;
        return pickup;
    }
}
