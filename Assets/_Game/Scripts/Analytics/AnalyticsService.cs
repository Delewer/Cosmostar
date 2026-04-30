using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class AnalyticsService
{
    public void Track(string eventName, Dictionary<string, object> payload = null)
    {
        Debug.Log($"[Analytics] {eventName} | {SerializePayload(payload)}");
    }

    private string SerializePayload(Dictionary<string, object> payload)
    {
        if (payload == null || payload.Count == 0) return "{}";

        StringBuilder builder = new("{");
        bool first = true;

        foreach (var pair in payload)
        {
            if (!first) builder.Append(", ");
            first = false;
            builder.Append(pair.Key).Append(":").Append(pair.Value);
        }

        builder.Append("}");
        return builder.ToString();
    }
}
