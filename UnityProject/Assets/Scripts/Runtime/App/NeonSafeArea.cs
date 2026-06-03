using UnityEngine;

namespace NeonSkySurvivors.Runtime.App
{
    /// <summary>
    /// Resizes a full-stretch RectTransform to match the device safe area so HUD
    /// elements anchored to the screen edges are not clipped by notches, punch-holes,
    /// rounded corners, or gesture bars. Recomputes when the safe area or orientation
    /// changes (cheap early-out otherwise).
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class NeonSafeArea : MonoBehaviour
    {
        private RectTransform _rect = null!;
        private Rect _lastSafeArea = new Rect(0f, 0f, 0f, 0f);
        private Vector2Int _lastScreen = Vector2Int.zero;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            Apply();
        }

        private void Update()
        {
            // Early-out unless something actually changed (rotation, resize, cutout reveal).
            var safeArea = Screen.safeArea;
            var screen = new Vector2Int(Screen.width, Screen.height);
            if (safeArea == _lastSafeArea && screen == _lastScreen)
            {
                return;
            }

            Apply();
        }

        private void Apply()
        {
            var safeArea = Screen.safeArea;
            _lastSafeArea = safeArea;
            _lastScreen = new Vector2Int(Screen.width, Screen.height);

            var width = Screen.width;
            var height = Screen.height;
            if (width <= 0 || height <= 0)
            {
                return;
            }

            var anchorMin = safeArea.position;
            var anchorMax = safeArea.position + safeArea.size;
            anchorMin.x /= width;
            anchorMin.y /= height;
            anchorMax.x /= width;
            anchorMax.y /= height;

            // Guard against degenerate values before any real screen metrics exist.
            if (anchorMin.x < 0f || anchorMin.y < 0f || anchorMax.x > 1f || anchorMax.y > 1f)
            {
                return;
            }

            _rect.anchorMin = anchorMin;
            _rect.anchorMax = anchorMax;
            _rect.offsetMin = Vector2.zero;
            _rect.offsetMax = Vector2.zero;
        }
    }
}
