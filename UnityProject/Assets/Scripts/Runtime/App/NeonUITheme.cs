using UnityEngine;

namespace NeonSkySurvivors.Runtime.App
{
    /// <summary>
    /// Central design tokens for the Neon Sky Survivors synthwave UI — palette, fonts,
    /// and shared metrics. Mirrors the locked values in the design's neon.css so the
    /// procedural Unity UI matches the HTML mockups.
    /// </summary>
    public static class NeonUITheme
    {
        // ── Background (dark digital sky, never pure black) ──
        public static readonly Color Bg      = Hex(0x04050F);
        public static readonly Color Bg1     = Hex(0x080A18);
        public static readonly Color Bg2     = Hex(0x0D1024);
        public static readonly Color Bg3     = Hex(0x141833);
        public static readonly Color Line    = Hex(0x1C2142);
        public static readonly Color Line2   = Hex(0x2A3160);

        // ── Neon accents ──
        public static readonly Color Cyan     = Hex(0x00E5FF);
        public static readonly Color CyanSoft = Hex(0x6AF3FF);
        public static readonly Color Teal     = Hex(0x2BFFB0);
        public static readonly Color Purple   = Hex(0xB14DFF);
        public static readonly Color Magenta  = Hex(0xFF3DE0);
        public static readonly Color Red      = Hex(0xFF3B4E);
        public static readonly Color Orange   = Hex(0xFF7A2B);
        public static readonly Color Pink     = Hex(0xFF4D9D);
        public static readonly Color Amber    = Hex(0xFFB02B);
        public static readonly Color Steel    = Hex(0x5B8DEF);
        public static readonly Color Toxic    = Hex(0x7CFF2B);
        public static readonly Color Violet   = Hex(0x8A4DFF);

        // ── Rarity ──
        public static readonly Color Common    = Hex(0x8A93A6);
        public static readonly Color Uncommon  = Hex(0x3DFF7A);
        public static readonly Color Rare      = Hex(0x2A7BFF);
        public static readonly Color Epic      = Hex(0xB14DFF);
        public static readonly Color Legendary = Hex(0xFFC23D);
        public static readonly Color Mythic    = Hex(0xFF3B4E);

        // ── Text ──
        public static readonly Color Text      = Hex(0xEAF4FF);
        public static readonly Color TextDim   = Hex(0x9AA6C4);
        public static readonly Color TextMute  = Hex(0x5C6685);
        public static readonly Color TextFaint = Hex(0x39406A);

        // bright accent text used for headings on color
        public static readonly Color TextCyan    = Hex(0xEAFDFF);
        public static readonly Color TextMagenta = Hex(0xFFEAFB);
        public static readonly Color TextRed     = Hex(0xFFE0E3);

        // ── Fonts (loaded from Resources/Fonts) ──
        private static Font? _display;   // Orbitron — logo / titles
        private static Font? _ui;        // Rajdhani SemiBold — body UI
        private static Font? _uiBold;    // Rajdhani Bold — headings / numbers

        public static Font Display => _display ??= Load("Fonts/Orbitron");
        public static Font Ui      => _ui ??= Load("Fonts/Rajdhani-SemiBold");
        public static Font UiBold  => _uiBold ??= Load("Fonts/Rajdhani-Bold");

        private static Font Load(string path)
        {
            var font = Resources.Load<Font>(path);
            if (font == null)
            {
                // Fallback keeps the UI legible even if the asset failed to import.
                font = Font.CreateDynamicFontFromOSFont("Arial", 16);
            }
            return font;
        }

        public static Color Rarity(NeonEquipmentRarity rarity)
        {
            switch (rarity)
            {
                case NeonEquipmentRarity.Uncommon:  return Uncommon;
                case NeonEquipmentRarity.Rare:      return Rare;
                case NeonEquipmentRarity.Epic:      return Epic;
                case NeonEquipmentRarity.Legendary: return Legendary;
                case NeonEquipmentRarity.Mythic:    return Mythic;
                default:                            return Common;
            }
        }

        /// <summary>color-mix(in srgb, color pct%, bg) — matches the CSS tint helper.</summary>
        public static Color Mix(Color color, float pct, Color with)
        {
            var t = Mathf.Clamp01(pct);
            return new Color(
                Mathf.Lerp(with.r, color.r, t),
                Mathf.Lerp(with.g, color.g, t),
                Mathf.Lerp(with.b, color.b, t),
                Mathf.Lerp(with.a, color.a, t));
        }

        public static Color Alpha(Color color, float a)
        {
            color.a = a;
            return color;
        }

        private static Color Hex(int rgb)
        {
            return new Color(
                ((rgb >> 16) & 0xFF) / 255f,
                ((rgb >> 8) & 0xFF) / 255f,
                (rgb & 0xFF) / 255f,
                1f);
        }
    }
}
