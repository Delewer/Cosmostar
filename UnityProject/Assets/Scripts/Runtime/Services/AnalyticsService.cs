using System;
using System.IO;
using Cosmostar.Core.Models;
using UnityEngine;

namespace Cosmostar.Runtime.Services
{
    public sealed class AnalyticsService
    {
        private readonly string _analyticsPath;

        public AnalyticsService()
        {
            _analyticsPath = Path.Combine(Application.persistentDataPath, "analytics.log");
        }

        public void Track(string eventName, string screen, string contextJson)
        {
            var analyticsEvent = new AnalyticsEvent
            {
                EventName = eventName,
                Screen = screen,
                TimestampUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ContextJson = contextJson
            };

            try
            {
                var line = JsonUtility.ToJson(analyticsEvent);
                File.AppendAllText(_analyticsPath, line + Environment.NewLine);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Analytics write skipped: " + exception.Message);
            }
        }
    }
}
