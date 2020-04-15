using System;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Framework.Core
{
    public static class GameDebug
    {
        private static List<ILogOutput> m_Outputs = new List<ILogOutput>();

        public static void Init()
        {
            Application.logMessageReceivedThreaded += HandleLog;
        }
        public static void Shutdown()
        {
            Application.logMessageReceivedThreaded -= HandleLog;
            foreach(ILogOutput device in m_Outputs)
            {
                device.Dispose();
            }
        }

        private static void HandleLog(string logString, string stackTrace, LogType type)
        {
            foreach(ILogOutput device in m_Outputs)
            {
                device.Output(logString, stackTrace, type);
            }
        }

        public static void Flush()
        {
            foreach(ILogOutput device in m_Outputs)
            {
                device.Flush();
            }
        }

        public static void RegisterOutputDevice(ILogOutput device)
        {
            if(device != null && !m_Outputs.Contains(device))
            {
                m_Outputs.Add(device);
            }
        }

        public static void UnregisterOutputDevice(ILogOutput device)
        {
            if (device == null)
                return;

            device.Dispose();
            m_Outputs.Remove(device);
        }

        public static void Assert(bool condition, string format, params object[] args)
        {
            Debug.AssertFormat(condition, format, args);
        }

        [Conditional("UNITY_EDITOR")]
        public static void DevLog(string format, params object[] args)
        {
            Debug.LogFormat(format, args);
        }

        public static void Log(string format, params object[] args)
        {
            Debug.LogFormat(format, args);
        }

        public static void LogWarning(string format, params object[] args)
        {
            Debug.LogWarningFormat(format, args);
        }

        public static void LogError(string format, params object[] args)
        {
            Debug.LogErrorFormat(format, args);
        }

        public static void LogException(Exception exception)
        {
            Debug.LogException(exception);
        }
    }
}