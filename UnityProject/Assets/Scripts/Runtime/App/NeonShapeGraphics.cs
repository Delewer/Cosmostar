using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NeonSkySurvivors.Runtime.App
{
    /// <summary>
    /// Shared mesh builder for the neon UI's angular shapes. uGUI Image cannot reproduce
    /// the design's CSS clip-path corners, so panels/buttons/slots are drawn as custom
    /// vector meshes: a filled interior plus a crisp inset border ring of any thickness.
    /// </summary>
    public static class NeonShapeMesh
    {
        private static readonly List<Vector2> Inner = new List<Vector2>(16);

        public static void Build(VertexHelper vh, IList<Vector2> outer, float borderThickness, Color fill, Color border)
        {
            vh.Clear();
            var count = outer.Count;
            if (count < 3) return;

            var hasBorder = borderThickness > 0.01f && border.a > 0.001f;

            Inner.Clear();
            if (hasBorder)
            {
                for (var i = 0; i < count; i++)
                {
                    var prev = outer[(i - 1 + count) % count];
                    var cur = outer[i];
                    var next = outer[(i + 1) % count];
                    var nIn = InwardNormal(prev, cur);
                    var nOut = InwardNormal(cur, next);
                    var bis = (nIn + nOut);
                    if (bis.sqrMagnitude < 1e-6f) bis = nIn;
                    bis.Normalize();
                    Inner.Add(cur + bis * borderThickness);
                }
            }
            else
            {
                for (var i = 0; i < count; i++) Inner.Add(outer[i]);
            }

            // Fill: fan from interior centroid.
            if (fill.a > 0.001f)
            {
                var centroid = Vector2.zero;
                for (var i = 0; i < count; i++) centroid += Inner[i];
                centroid /= count;

                var baseIndex = vh.currentVertCount;
                AddVert(vh, centroid, fill);
                for (var i = 0; i < count; i++) AddVert(vh, Inner[i], fill);
                for (var i = 0; i < count; i++)
                {
                    var a = baseIndex + 1 + i;
                    var b = baseIndex + 1 + (i + 1) % count;
                    vh.AddTriangle(baseIndex, a, b);
                }
            }

            // Border ring: quad per edge between outer and inner.
            if (hasBorder)
            {
                for (var i = 0; i < count; i++)
                {
                    var j = (i + 1) % count;
                    var o0 = vh.currentVertCount;
                    AddVert(vh, outer[i], border);
                    AddVert(vh, outer[j], border);
                    AddVert(vh, Inner[j], border);
                    AddVert(vh, Inner[i], border);
                    vh.AddTriangle(o0, o0 + 1, o0 + 2);
                    vh.AddTriangle(o0, o0 + 2, o0 + 3);
                }
            }
        }

        private static Vector2 InwardNormal(Vector2 a, Vector2 b)
        {
            var d = (b - a).normalized;
            return new Vector2(d.y, -d.x); // -90° rotation → interior side for CW polygon (y-up)
        }

        private static void AddVert(VertexHelper vh, Vector2 pos, Color color)
        {
            vh.AddVert(new Vector3(pos.x, pos.y), color, new Vector2(0.5f, 0.5f));
        }
    }

    /// <summary>
    /// Angular panel/button/chip with fixed-size corner cuts (top-right + bottom-left by
    /// default, matching .ns-cut / .ns-btn). Fill color is the Graphic color; border is
    /// drawn separately so the thin neon stroke reads at any size.
    /// </summary>
    public sealed class NeonCutRect : MaskableGraphic
    {
        public float CutSize = 12f;
        public bool CutTL = false;
        public bool CutTR = true;
        public bool CutBR = false;
        public bool CutBL = true;
        public float BorderThickness = 1.5f;
        public Color BorderColor = Color.white;

        private static readonly List<Vector2> Pts = new List<Vector2>(8);

        public void Refresh() => SetVerticesDirty();

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            var r = GetPixelAdjustedRect();
            var c = Mathf.Min(CutSize, Mathf.Min(r.width, r.height) * 0.5f);
            float xMin = r.xMin, xMax = r.xMax, yMin = r.yMin, yMax = r.yMax;

            Pts.Clear();
            // Clockwise from top-left.
            if (CutTL) { Pts.Add(new Vector2(xMin, yMax - c)); Pts.Add(new Vector2(xMin + c, yMax)); }
            else Pts.Add(new Vector2(xMin, yMax));

            if (CutTR) { Pts.Add(new Vector2(xMax - c, yMax)); Pts.Add(new Vector2(xMax, yMax - c)); }
            else Pts.Add(new Vector2(xMax, yMax));

            if (CutBR) { Pts.Add(new Vector2(xMax, yMin + c)); Pts.Add(new Vector2(xMax - c, yMin)); }
            else Pts.Add(new Vector2(xMax, yMin));

            if (CutBL) { Pts.Add(new Vector2(xMin + c, yMin)); Pts.Add(new Vector2(xMin, yMin + c)); }
            else Pts.Add(new Vector2(xMin, yMin));

            NeonShapeMesh.Build(vh, Pts, BorderThickness, color, BorderColor);
        }
    }

    /// <summary>
    /// Proportional polygon (points in normalized 0..1 space, CSS y-down). Used for the
    /// hexagonal equipment slots (.ns-slot) and any other shape that scales with its box.
    /// </summary>
    public sealed class NeonPolyGraphic : MaskableGraphic
    {
        // Default: vertical hexagon (50% 0,100% 25%,100% 75%,50% 100%,0 75%,0 25%).
        public Vector2[] NormalizedPoints =
        {
            new Vector2(0.5f, 0f), new Vector2(1f, 0.25f), new Vector2(1f, 0.75f),
            new Vector2(0.5f, 1f), new Vector2(0f, 0.75f), new Vector2(0f, 0.25f),
        };
        public float BorderThickness = 1.5f;
        public Color BorderColor = Color.white;

        private static readonly List<Vector2> Pts = new List<Vector2>(8);

        public void Refresh() => SetVerticesDirty();

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            var r = GetPixelAdjustedRect();
            Pts.Clear();
            foreach (var p in NormalizedPoints)
            {
                Pts.Add(new Vector2(r.xMin + p.x * r.width, r.yMax - p.y * r.height));
            }
            NeonShapeMesh.Build(vh, Pts, BorderThickness, color, BorderColor);
        }
    }
}
