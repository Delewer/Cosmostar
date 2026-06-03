using System;
using System.IO;
using UnityEngine;

namespace NeonSkySurvivors.Runtime.App
{
    public class NeonCrashReporter : MonoBehaviour
    {
        private string _logPath;

        private void Awake()
        {
            _logPath = Path.Combine(Application.persistentDataPath, "crash_log.txt");
            Application.logMessageReceived += OnLogMessage;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= OnLogMessage;
        }

        private void OnLogMessage(string condition, string stackTrace, LogType type)
        {
            if (type != LogType.Exception && type != LogType.Error) return;

            try
            {
                var entry = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] [{type}]\n{condition}\n{stackTrace}\n---\n";
                File.AppendAllText(_logPath, entry);
            }
            catch
            {
                // never throw from the log callback
            }
        }

        public static void PurgeLogs()
        {
            var path = Path.Combine(Application.persistentDataPath, "crash_log.txt");
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
