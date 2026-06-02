#nullable enable

using System;
using UnityEngine;
using NeonSkySurvivors.Core.Models;

namespace NeonSkySurvivors.Runtime.App
{
    /// <summary>
    /// Generates procedural 32×32 (or 24×24 for icons) white-alpha Texture2D sprites at runtime.
    /// Sprites are white so SpriteRenderer.color tints them to the correct neon hue at render time.
    /// All sprites are cached statically after first creation.
    /// </summary>
    internal static class NeonSpriteFactory
    {
        private const int S = 32;
        private const int IS = 24;

        private static Sprite? _blank;
        private static Sprite? _playerBody;
        private static Sprite? _playerNose;
        private static Sprite? _playerWing;
        private static Sprite? _boss;
        private static Sprite? _projectile;
        private static Sprite? _xpShard;
        private static Sprite? _mine;
        private static Sprite? _orbitBlade;
        private static readonly Sprite?[] EnemyCache = new Sprite?[7];
        private static readonly Sprite?[] IconCache = new Sprite?[6];
        private static readonly Sprite?[] UpgradeCache = new Sprite?[5];

        public static Sprite Blank      => _blank      ??= Make(S,  (u, v) => 1f);
        public static Sprite PlayerBody => _playerBody ??= Make(S,  PlayerBodyFn);
        public static Sprite PlayerNose => _playerNose ??= Make(S,  PlayerNoseFn);
        public static Sprite PlayerWing => _playerWing ??= Make(S,  PlayerWingFn);
        public static Sprite Boss       => _boss       ??= Make(S,  BossFn);
        public static Sprite Projectile => _projectile ??= Make(S,  (u, v) => Edge(0.85f - Mathf.Sqrt(u * u * 0.3f + v * v)));
        public static Sprite XpShard   => _xpShard    ??= Make(S,  XpShardFn);
        public static Sprite Mine      => _mine       ??= Make(S,  MineFn);
        public static Sprite OrbitBlade => _orbitBlade ??= Make(S,  (u, v) => Edge(0.92f - Mathf.Sqrt(u * u * 0.1f + v * v)));

        public static Sprite GetEnemy(NeonEnemyBehaviorType t)
        {
            var i = (int)t;
            if ((uint)i >= (uint)EnemyCache.Length) return Blank;
            return EnemyCache[i] ??= Make(S, EnemyFnFor(t));
        }

        public static Sprite GetIcon(NeonEquipmentSlot s)
        {
            var i = (int)s;
            if ((uint)i >= (uint)IconCache.Length) return Blank;
            return IconCache[i] ??= Make(IS, IconFnFor(s));
        }

        public static Sprite GetUpgradeIcon(NeonUpgradeCategory c)
        {
            var i = (int)c;
            if ((uint)i >= (uint)UpgradeCache.Length) return Blank;
            return UpgradeCache[i] ??= Make(IS, UpgradeFnFor(c));
        }

        // ── Shape functions ──────────────────────────────────────────────────────

        private static float PlayerBodyFn(float u, float v)
        {
            // Fuselage: tall narrow ellipse
            var fuselage = Edge(0.88f - Mathf.Sqrt(u * u * 2.8f + v * v * 0.72f));
            // Cockpit indent at top center
            var cockpit = 1f - Edge(0.26f - Mathf.Sqrt(u * u * 4f + (v - 0.68f) * (v - 0.68f) * 5f));
            return fuselage * cockpit;
        }

        private static float PlayerNoseFn(float u, float v)
        {
            // Pointed chevron aiming up
            return Edge(0.82f - Mathf.Abs(u) * 1.9f - Mathf.Abs(v - 0.08f));
        }

        private static float PlayerWingFn(float u, float v)
        {
            // Swept delta wing: wide at base, tapers at tip
            return Edge(0.88f - Mathf.Abs(v + 0.38f - u * 0.5f) * 1.5f - Mathf.Abs(u - 0.2f) * 0.7f);
        }

        private static float BossFn(float u, float v)
        {
            // 8-pointed star
            var r = Mathf.Sqrt(u * u + v * v);
            var theta = Mathf.Atan2(v, u);
            var arm = 0.42f + 0.46f * Mathf.Abs(Mathf.Cos(theta * 4f));
            return Edge(arm - r);
        }

        private static float XpShardFn(float u, float v)
        {
            // 4-pointed star
            var r = Mathf.Sqrt(u * u + v * v);
            var theta = Mathf.Atan2(v, u);
            var arm = 0.18f + 0.74f * Mathf.Abs(Mathf.Cos(theta * 2f));
            return Edge(arm - r);
        }

        private static float MineFn(float u, float v)
        {
            // Circle with 6 spikes
            var r = Mathf.Sqrt(u * u + v * v);
            var theta = Mathf.Atan2(v, u);
            var spike = 0.48f + 0.35f * Mathf.Abs(Mathf.Cos(theta * 3f));
            return Edge(spike - r);
        }

        private static Func<float, float, float> EnemyFnFor(NeonEnemyBehaviorType t)
        {
            switch (t)
            {
                case NeonEnemyBehaviorType.Chaser:
                    // Red diamond
                    return (u, v) => Edge(0.85f - Mathf.Abs(u) - Mathf.Abs(v));
                case NeonEnemyBehaviorType.FastChaser:
                    // Thin forward-pointing diamond
                    return (u, v) => Edge(0.82f - Mathf.Abs(u * 1.6f) - Mathf.Abs(v));
                case NeonEnemyBehaviorType.Shooter:
                    // Hexagon
                    return (u, v) =>
                    {
                        var h = Mathf.Max(Mathf.Abs(v), Mathf.Max(
                            Mathf.Abs(0.866f * u + 0.5f * v),
                            Mathf.Abs(0.866f * u - 0.5f * v)));
                        return Edge(0.78f - h);
                    };
                case NeonEnemyBehaviorType.Tank:
                    // Wide rounded rectangle
                    return (u, v) =>
                    {
                        var rx = Mathf.Max(0f, Mathf.Abs(u) - 0.62f);
                        var ry = Mathf.Max(0f, Mathf.Abs(v) - 0.4f);
                        return Edge(0.28f - Mathf.Sqrt(rx * rx + ry * ry));
                    };
                case NeonEnemyBehaviorType.MineCarrier:
                    // Rounded body + small circle on top (mine)
                    return (u, v) =>
                    {
                        var body = Edge(0.7f - Mathf.Sqrt(u * u + (v + 0.12f) * (v + 0.12f)));
                        var load = Edge(0.28f - Mathf.Sqrt(u * u * 1.4f + (v - 0.62f) * (v - 0.62f) * 1.4f));
                        return Mathf.Max(body, load);
                    };
                case NeonEnemyBehaviorType.Splitter:
                    // Circle with vertical gap (split)
                    return (u, v) =>
                    {
                        var ring = Edge(0.78f - Mathf.Sqrt(u * u + v * v));
                        var gap = 1f - Edge(0.07f - Mathf.Abs(u));
                        return ring * gap;
                    };
                default:
                    return BossFn;
            }
        }

        private static Func<float, float, float> IconFnFor(NeonEquipmentSlot s)
        {
            switch (s)
            {
                case NeonEquipmentSlot.Weapon:
                    // Gun barrel + grip
                    return (u, v) =>
                    {
                        var barrel = Edge(Mathf.Min(0.22f - Mathf.Abs(v) * 2.8f, Mathf.Min(u + 0.82f, -u + 1.05f)));
                        var grip   = Edge(Mathf.Min(0.25f - Mathf.Abs(u + 0.52f), 0.42f - Mathf.Abs(v + 0.52f)));
                        return Mathf.Max(barrel, grip);
                    };
                case NeonEquipmentSlot.Wings:
                    // Swept delta wing
                    return (u, v) => Edge(0.82f - Mathf.Abs(v - u * 0.4f) * 2.1f - Mathf.Abs(u) * 0.22f);
                case NeonEquipmentSlot.Engine:
                    // Flame / thrust cone
                    return (u, v) => Edge(0.86f - Mathf.Abs(u) * 1.55f - Mathf.Abs(v - 0.22f) * 0.72f);
                case NeonEquipmentSlot.Hull:
                    // Shield (wide top, pointed bottom)
                    return (u, v) =>
                    {
                        var rx = Mathf.Max(0f, Mathf.Abs(u) - 0.52f);
                        var ry = Mathf.Max(0f, Mathf.Abs(v + 0.12f) - 0.62f);
                        var shell = Edge(0.28f - Mathf.Sqrt(rx * rx + ry * ry));
                        var tip   = Edge(0.84f - Mathf.Abs(u) * 1.5f - (v + 0.7f) * 1.5f);
                        return v < -0.12f ? Mathf.Max(shell, tip) : shell;
                    };
                case NeonEquipmentSlot.Core:
                    // Lightning bolt
                    return (u, v) => Edge(0.82f - Mathf.Abs(u - v * 0.35f) * 3.2f - Mathf.Abs(v) * 0.18f);
                default: // Radar
                    // Concentric arcs + center dot
                    return (u, v) =>
                    {
                        var r = Mathf.Sqrt(u * u + v * v);
                        var outer  = Edge(0.08f - Mathf.Abs(r - 0.78f));
                        var mid    = Edge(0.07f - Mathf.Abs(r - 0.52f));
                        var center = Edge(0.18f - r);
                        return Mathf.Max(outer, Mathf.Max(mid, center));
                    };
            }
        }

        private static Func<float, float, float> UpgradeFnFor(NeonUpgradeCategory c)
        {
            switch (c)
            {
                case NeonUpgradeCategory.Weapon:   return IconFnFor(NeonEquipmentSlot.Weapon);
                case NeonUpgradeCategory.Trail:    return IconFnFor(NeonEquipmentSlot.Engine);
                case NeonUpgradeCategory.Defense:  return IconFnFor(NeonEquipmentSlot.Hull);
                case NeonUpgradeCategory.Special:  return IconFnFor(NeonEquipmentSlot.Core);
                default: // Passive
                    // Circle with plus sign (boost symbol)
                    return (u, v) =>
                    {
                        var ring = Edge(0.1f - Mathf.Abs(Mathf.Sqrt(u * u + v * v) - 0.7f));
                        var plusH = Edge(0.16f - Mathf.Abs(v) * 3.5f - Mathf.Abs(u) * 0.3f);
                        var plusV = Edge(0.16f - Mathf.Abs(u) * 3.5f - Mathf.Abs(v) * 0.3f);
                        return Mathf.Max(ring, Mathf.Max(plusH, plusV));
                    };
            }
        }

        // ── Renderer ─────────────────────────────────────────────────────────────

        private static Sprite Make(int size, Func<float, float, float> fn)
        {
            var pixels = new Color[size * size];
            var half = size * 0.5f;
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var u = (x + 0.5f - half) / half;
                    var v = (y + 0.5f - half) / half;
                    pixels[y * size + x] = new Color(1f, 1f, 1f, Mathf.Clamp01(fn(u, v)));
                }
            }
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        // d > 0 = inside, d < 0 = outside; returns 0→1 over ~1-2 pixels
        private static float Edge(float d) => Mathf.Clamp01(d * 7f + 0.5f);
    }
}
