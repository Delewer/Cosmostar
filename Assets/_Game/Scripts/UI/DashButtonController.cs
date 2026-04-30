using UnityEngine;
using UnityEngine.EventSystems;

public class DashButtonController : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private PlayerDash playerDash;
    [SerializeField] private Vector2 defaultDashDirection = Vector2.up;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (playerDash == null) return;
        playerDash.TryDash(defaultDashDirection);
    }
}
