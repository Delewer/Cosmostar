using UnityEngine;

namespace Cosmostar.Runtime.App
{
    public sealed class BootSceneSetDressing : MonoBehaviour
    {
        private Transform _hero;
        private Transform _droneA;
        private Transform _droneB;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureSetDressing()
        {
            if (FindObjectOfType<BootSceneSetDressing>() != null)
            {
                return;
            }

            var root = new GameObject("BootSceneSetDressing");
            root.AddComponent<BootSceneSetDressing>();
        }

        private void Awake()
        {
            CreateBackdrop();
            CreateShips();
        }

        private void Update()
        {
            var t = Time.time;
            if (_hero != null)
            {
                _hero.position = new Vector3(Mathf.Sin(t * 0.9f) * 1.5f, Mathf.Cos(t * 0.9f) * 1.1f, 0f);
                _hero.rotation = Quaternion.Euler(0f, 0f, -t * 70f);
            }

            if (_droneA != null)
            {
                _droneA.position = new Vector3(Mathf.Cos(t * 1.2f) * 2.6f, Mathf.Sin(t * 1.2f) * 1.7f, 0f);
            }

            if (_droneB != null)
            {
                _droneB.position = new Vector3(Mathf.Cos(t * 1.2f + Mathf.PI) * 2.6f, Mathf.Sin(t * 1.2f + Mathf.PI) * 1.7f, 0f);
            }
        }

        private static void CreateBackdrop()
        {
            CreateSprite("ArenaField", MakeSolidSprite(new Color(0.04f, 0.05f, 0.1f)), Vector3.zero, new Vector3(18f, 18f, 1f), -1);
            CreateSprite("LaneHorizontal", MakeSolidSprite(new Color(0.09f, 0.16f, 0.24f, 0.75f)), Vector3.zero, new Vector3(12f, 0.25f, 1f), 0);
            CreateSprite("LaneVertical", MakeSolidSprite(new Color(0.11f, 0.09f, 0.22f, 0.75f)), Vector3.zero, new Vector3(0.25f, 10f, 1f), 0);
        }

        private void CreateShips()
        {
            _hero = CreateSprite(
                "HeroShipProxy",
                MakeSolidSprite(new Color(0.2f, 0.95f, 1f)),
                new Vector3(0f, 1.5f, 0f),
                new Vector3(0.65f, 0.95f, 1f),
                3);

            _droneA = CreateSprite(
                "DroneA",
                MakeSolidSprite(new Color(1f, 0.3f, 0.75f)),
                new Vector3(2.2f, 0f, 0f),
                new Vector3(0.45f, 0.45f, 1f),
                2);

            _droneB = CreateSprite(
                "DroneB",
                MakeSolidSprite(new Color(0.95f, 0.45f, 0.2f)),
                new Vector3(-2.2f, 0f, 0f),
                new Vector3(0.45f, 0.45f, 1f),
                2);
        }

        private static Transform CreateSprite(string name, Sprite sprite, Vector3 position, Vector3 scale, int order)
        {
            var go = new GameObject(name);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = order;
            go.transform.position = position;
            go.transform.localScale = scale;
            return go.transform;
        }

        private static Sprite MakeSolidSprite(Color color)
        {
            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            tex.SetPixel(0, 0, color);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        }
    }
}
