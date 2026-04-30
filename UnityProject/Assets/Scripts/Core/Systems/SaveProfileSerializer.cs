#nullable disable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Cosmostar.Core.Models;

namespace Cosmostar.Core.Systems
{
    public static class SaveProfileSerializer
    {
        public static string Serialize(SaveProfile profile)
        {
            if (profile == null)
            {
                profile = new SaveProfile();
            }

            var builder = new StringBuilder(2048);
            builder.AppendLine("{");
            AppendNumber(builder, "Version", profile.Version, 1);
            AppendNumber(builder, "SoftCurrency", profile.SoftCurrency, 1);
            AppendNumber(builder, "ModuleShards", profile.ModuleShards, 1);
            AppendNumber(builder, "UnlockTrackXp", profile.UnlockTrackXp, 1);
            AppendNumber(builder, "CurrentStreak", profile.CurrentStreak, 1);
            AppendNumber(builder, "BestStreak", profile.BestStreak, 1);
            AppendBool(builder, "SeenFtue", profile.SeenFtue, 1);
            AppendString(builder, "EquippedShipId", profile.EquippedShipId, 1);
            AppendModules(builder, profile.Modules);
            AppendMissions(builder, profile.Missions);
            AppendStringList(builder, "UnlockedAbilityIds", profile.UnlockedAbilityIds, 1);
            AppendStringList(builder, "ClaimedUnlockTrackIds", profile.ClaimedUnlockTrackIds, 1, false);
            builder.AppendLine("}");
            return builder.ToString();
        }

        public static SaveProfile Deserialize(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new SaveProfile();
            }

            try
            {
                var parsed = new JsonReader(json).ParseObject();
                var profile = new SaveProfile
                {
                    Version = GetInt(parsed, "Version", 1),
                    SoftCurrency = GetInt(parsed, "SoftCurrency", 90),
                    ModuleShards = GetInt(parsed, "ModuleShards", 0),
                    UnlockTrackXp = GetInt(parsed, "UnlockTrackXp", 0),
                    CurrentStreak = GetInt(parsed, "CurrentStreak", 0),
                    BestStreak = GetInt(parsed, "BestStreak", 0),
                    SeenFtue = GetBool(parsed, "SeenFtue", false),
                    EquippedShipId = GetString(parsed, "EquippedShipId", "starling_mk1")
                };

                ReadModules(parsed, profile.Modules);
                ReadMissions(parsed, profile.Missions);
                ReadStringList(parsed, "UnlockedAbilityIds", profile.UnlockedAbilityIds);
                ReadStringList(parsed, "ClaimedUnlockTrackIds", profile.ClaimedUnlockTrackIds);
                return profile;
            }
            catch
            {
                return new SaveProfile();
            }
        }

        private static void AppendModules(StringBuilder builder, List<ModuleProgress> modules)
        {
            AppendIndent(builder, 1);
            builder.AppendLine("\"Modules\": [");
            for (var index = 0; index < modules.Count; index++)
            {
                var module = modules[index];
                AppendIndent(builder, 2);
                builder.Append("{");
                AppendInlineString(builder, "ModuleId", module.ModuleId);
                builder.Append(", ");
                AppendInlineNumber(builder, "Level", module.Level);
                builder.Append(", ");
                AppendInlineBool(builder, "Unlocked", module.Unlocked);
                builder.Append(", ");
                AppendInlineBool(builder, "Equipped", module.Equipped);
                builder.Append(index == modules.Count - 1 ? "}" : "},");
                builder.AppendLine();
            }

            AppendIndent(builder, 1);
            builder.AppendLine("],");
        }

        private static void AppendMissions(StringBuilder builder, List<MissionProgress> missions)
        {
            AppendIndent(builder, 1);
            builder.AppendLine("\"Missions\": [");
            for (var index = 0; index < missions.Count; index++)
            {
                var mission = missions[index];
                AppendIndent(builder, 2);
                builder.Append("{");
                AppendInlineString(builder, "MissionId", mission.MissionId);
                builder.Append(", ");
                AppendInlineNumber(builder, "StarsEarned", mission.StarsEarned);
                builder.Append(", ");
                AppendInlineNumber(builder, "Clears", mission.Clears);
                builder.Append(", ");
                AppendInlineBool(builder, "Completed", mission.Completed);
                builder.Append(", ");
                AppendInlineFloat(builder, "BestShieldRatio", mission.BestShieldRatio);
                builder.Append(", ");
                AppendInlineBool(builder, "NoReviveClear", mission.NoReviveClear);
                builder.Append(index == missions.Count - 1 ? "}" : "},");
                builder.AppendLine();
            }

            AppendIndent(builder, 1);
            builder.AppendLine("],");
        }

        private static void ReadModules(Dictionary<string, object> parsed, List<ModuleProgress> modules)
        {
            modules.Clear();
            var list = GetList(parsed, "Modules");
            if (list == null)
            {
                return;
            }

            for (var index = 0; index < list.Count; index++)
            {
                var entry = list[index] as Dictionary<string, object>;
                if (entry == null)
                {
                    continue;
                }

                modules.Add(new ModuleProgress
                {
                    ModuleId = GetString(entry, "ModuleId", string.Empty),
                    Level = GetInt(entry, "Level", 0),
                    Unlocked = GetBool(entry, "Unlocked", false),
                    Equipped = GetBool(entry, "Equipped", false)
                });
            }
        }

        private static void ReadMissions(Dictionary<string, object> parsed, List<MissionProgress> missions)
        {
            missions.Clear();
            var list = GetList(parsed, "Missions");
            if (list == null)
            {
                return;
            }

            for (var index = 0; index < list.Count; index++)
            {
                var entry = list[index] as Dictionary<string, object>;
                if (entry == null)
                {
                    continue;
                }

                missions.Add(new MissionProgress
                {
                    MissionId = GetString(entry, "MissionId", string.Empty),
                    StarsEarned = GetInt(entry, "StarsEarned", 0),
                    Clears = GetInt(entry, "Clears", 0),
                    Completed = GetBool(entry, "Completed", false),
                    BestShieldRatio = GetFloat(entry, "BestShieldRatio", 0f),
                    NoReviveClear = GetBool(entry, "NoReviveClear", false)
                });
            }
        }

        private static void AppendStringList(StringBuilder builder, string name, List<string> values, int indent, bool trailingComma = true)
        {
            AppendIndent(builder, indent);
            builder.Append("\"");
            builder.Append(name);
            builder.Append("\": [");
            for (var index = 0; index < values.Count; index++)
            {
                if (index > 0)
                {
                    builder.Append(", ");
                }

                AppendQuoted(builder, values[index]);
            }

            builder.Append(trailingComma ? "]," : "]");
            builder.AppendLine();
        }

        private static void ReadStringList(Dictionary<string, object> parsed, string name, List<string> values)
        {
            values.Clear();
            var list = GetList(parsed, name);
            if (list == null)
            {
                return;
            }

            for (var index = 0; index < list.Count; index++)
            {
                var value = list[index] as string;
                if (value != null)
                {
                    values.Add(value);
                }
            }
        }

        private static void AppendString(StringBuilder builder, string name, string value, int indent)
        {
            AppendIndent(builder, indent);
            AppendInlineString(builder, name, value);
            builder.AppendLine(",");
        }

        private static void AppendNumber(StringBuilder builder, string name, int value, int indent)
        {
            AppendIndent(builder, indent);
            AppendInlineNumber(builder, name, value);
            builder.AppendLine(",");
        }

        private static void AppendBool(StringBuilder builder, string name, bool value, int indent)
        {
            AppendIndent(builder, indent);
            AppendInlineBool(builder, name, value);
            builder.AppendLine(",");
        }

        private static void AppendInlineString(StringBuilder builder, string name, string value)
        {
            builder.Append("\"");
            builder.Append(name);
            builder.Append("\": ");
            AppendQuoted(builder, value);
        }

        private static void AppendInlineNumber(StringBuilder builder, string name, int value)
        {
            builder.Append("\"");
            builder.Append(name);
            builder.Append("\": ");
            builder.Append(value.ToString(CultureInfo.InvariantCulture));
        }

        private static void AppendInlineFloat(StringBuilder builder, string name, float value)
        {
            builder.Append("\"");
            builder.Append(name);
            builder.Append("\": ");
            builder.Append(value.ToString("0.########", CultureInfo.InvariantCulture));
        }

        private static void AppendInlineBool(StringBuilder builder, string name, bool value)
        {
            builder.Append("\"");
            builder.Append(name);
            builder.Append("\": ");
            builder.Append(value ? "true" : "false");
        }

        private static void AppendQuoted(StringBuilder builder, string value)
        {
            builder.Append("\"");
            if (!string.IsNullOrEmpty(value))
            {
                for (var index = 0; index < value.Length; index++)
                {
                    var current = value[index];
                    switch (current)
                    {
                        case '\\':
                            builder.Append("\\\\");
                            break;
                        case '"':
                            builder.Append("\\\"");
                            break;
                        case '\n':
                            builder.Append("\\n");
                            break;
                        case '\r':
                            builder.Append("\\r");
                            break;
                        case '\t':
                            builder.Append("\\t");
                            break;
                        default:
                            builder.Append(current);
                            break;
                    }
                }
            }

            builder.Append("\"");
        }

        private static void AppendIndent(StringBuilder builder, int count)
        {
            for (var index = 0; index < count; index++)
            {
                builder.Append("  ");
            }
        }

        private static string GetString(Dictionary<string, object> values, string name, string fallback)
        {
            object value;
            return values.TryGetValue(name, out value) && value is string ? (string)value : fallback;
        }

        private static int GetInt(Dictionary<string, object> values, string name, int fallback)
        {
            object value;
            if (!values.TryGetValue(name, out value))
            {
                return fallback;
            }

            if (value is int)
            {
                return (int)value;
            }

            if (value is double)
            {
                return (int)(double)value;
            }

            return fallback;
        }

        private static float GetFloat(Dictionary<string, object> values, string name, float fallback)
        {
            object value;
            if (!values.TryGetValue(name, out value))
            {
                return fallback;
            }

            if (value is float)
            {
                return (float)value;
            }

            if (value is double)
            {
                return (float)(double)value;
            }

            return fallback;
        }

        private static bool GetBool(Dictionary<string, object> values, string name, bool fallback)
        {
            object value;
            return values.TryGetValue(name, out value) && value is bool ? (bool)value : fallback;
        }

        private static List<object> GetList(Dictionary<string, object> values, string name)
        {
            object value;
            return values.TryGetValue(name, out value) ? value as List<object> : null;
        }

        private sealed class JsonReader
        {
            private readonly string _json;
            private int _index;

            public JsonReader(string json)
            {
                _json = json;
            }

            public Dictionary<string, object> ParseObject()
            {
                SkipWhitespace();
                Expect('{');
                var result = new Dictionary<string, object>();
                SkipWhitespace();
                if (TryConsume('}'))
                {
                    return result;
                }

                while (_index < _json.Length)
                {
                    var key = ParseString();
                    SkipWhitespace();
                    Expect(':');
                    result[key] = ParseValue();
                    SkipWhitespace();
                    if (TryConsume('}'))
                    {
                        return result;
                    }

                    Expect(',');
                    SkipWhitespace();
                }

                throw new FormatException("Object was not closed.");
            }

            private List<object> ParseArray()
            {
                Expect('[');
                var result = new List<object>();
                SkipWhitespace();
                if (TryConsume(']'))
                {
                    return result;
                }

                while (_index < _json.Length)
                {
                    result.Add(ParseValue());
                    SkipWhitespace();
                    if (TryConsume(']'))
                    {
                        return result;
                    }

                    Expect(',');
                    SkipWhitespace();
                }

                throw new FormatException("Array was not closed.");
            }

            private object ParseValue()
            {
                SkipWhitespace();
                if (_index >= _json.Length)
                {
                    throw new FormatException("Unexpected end of JSON.");
                }

                var current = _json[_index];
                if (current == '"')
                {
                    return ParseString();
                }

                if (current == '{')
                {
                    return ParseObject();
                }

                if (current == '[')
                {
                    return ParseArray();
                }

                if (Matches("true"))
                {
                    _index += 4;
                    return true;
                }

                if (Matches("false"))
                {
                    _index += 5;
                    return false;
                }

                if (Matches("null"))
                {
                    _index += 4;
                    return null;
                }

                return ParseNumber();
            }

            private string ParseString()
            {
                Expect('"');
                var builder = new StringBuilder();
                while (_index < _json.Length)
                {
                    var current = _json[_index++];
                    if (current == '"')
                    {
                        return builder.ToString();
                    }

                    if (current != '\\')
                    {
                        builder.Append(current);
                        continue;
                    }

                    if (_index >= _json.Length)
                    {
                        throw new FormatException("Invalid string escape.");
                    }

                    var escaped = _json[_index++];
                    switch (escaped)
                    {
                        case '"':
                        case '\\':
                        case '/':
                            builder.Append(escaped);
                            break;
                        case 'b':
                            builder.Append('\b');
                            break;
                        case 'f':
                            builder.Append('\f');
                            break;
                        case 'n':
                            builder.Append('\n');
                            break;
                        case 'r':
                            builder.Append('\r');
                            break;
                        case 't':
                            builder.Append('\t');
                            break;
                        default:
                            throw new FormatException("Unsupported string escape.");
                    }
                }

                throw new FormatException("String was not closed.");
            }

            private double ParseNumber()
            {
                var start = _index;
                while (_index < _json.Length)
                {
                    var current = _json[_index];
                    if ((current >= '0' && current <= '9') || current == '-' || current == '+' || current == '.' || current == 'e' || current == 'E')
                    {
                        _index++;
                        continue;
                    }

                    break;
                }

                if (start == _index)
                {
                    throw new FormatException("Expected number.");
                }

                return double.Parse(_json.Substring(start, _index - start), CultureInfo.InvariantCulture);
            }

            private bool Matches(string token)
            {
                return _index + token.Length <= _json.Length && string.CompareOrdinal(_json, _index, token, 0, token.Length) == 0;
            }

            private bool TryConsume(char expected)
            {
                SkipWhitespace();
                if (_index < _json.Length && _json[_index] == expected)
                {
                    _index++;
                    return true;
                }

                return false;
            }

            private void Expect(char expected)
            {
                SkipWhitespace();
                if (_index >= _json.Length || _json[_index] != expected)
                {
                    throw new FormatException("Expected '" + expected + "'.");
                }

                _index++;
            }

            private void SkipWhitespace()
            {
                while (_index < _json.Length && char.IsWhiteSpace(_json[_index]))
                {
                    _index++;
                }
            }
        }
    }
}
