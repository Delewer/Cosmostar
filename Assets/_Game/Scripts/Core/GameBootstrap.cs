using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private ServiceLocator serviceLocator;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (serviceLocator == null)
        {
            Debug.LogError("GameBootstrap is missing ServiceLocator reference.");
            return;
        }

        serviceLocator.InitializeServices();
    }
}
