using System;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Framework.Core
{
    /// <summary>
    /// Log, LogWarning, LogError, LogAssertion, LogException
    /// Release版本：保留LogWarning、LogError、LogException, 其余strip
    /// Debug版本：提供两种方式开关Log. 1、ENABLE_LOG ———— strip，更彻底；2、EnableLog ———— not strip，仍有参数传递的GC
    /// </summary>
    public class GameDebug : MonoBehaviour
    {
        static public bool EnableLog = true;            // 仅控制DevLog, Log

        private static List<BaseLogOutput> m_Outputs = new List<BaseLogOutput>();

        void OnEnable()
        {            
            Application.logMessageReceivedThreaded += HandleLog;
            Debug.developerConsoleVisible = false;
        }

        void OnDisable()
        {
            Flush();
            Application.logMessageReceivedThreaded -= HandleLog;
        }

        private static void HandleLog(string logString, string stackTrace, LogType type)
        {
            foreach(BaseLogOutput device in m_Outputs)
            {
                device.Output(logString, stackTrace, type);
            }
        }

        public static void Flush()
        {
            foreach(BaseLogOutput device in m_Outputs)
            {
                device.Flush();
            }
        }

        public void RegisterOutputDevice(BaseLogOutput device)
        {
            if(device != null && !m_Outputs.Contains(device))
            {
                m_Outputs.Add(device);
            }
        }

        public void UnregisterOutputDevice(BaseLogOutput device)
        {
            if (device == null)
                return;

            m_Outputs.Remove(device);
        }
        
        [Conditional("UNITY_EDITOR")]
        public static void DevLog(object message)
        {
            if(!GameDebug.EnableLog)
                return;
            Debug.Log(message);
        }

        [Conditional("UNITY_EDITOR")]
        public static void DevLog(object message, Object context)
        {
            if(!GameDebug.EnableLog)
                return;
            Debug.Log(message, context);
        }

        [Conditional("ENABLE_LOG")]
        public static void Log(object message)
        {
            if(!GameDebug.EnableLog)
                return;
            Debug.Log(message);
        }

        [Conditional("ENABLE_LOG")]
        public static void Log(object message, Object context)
        {
            if(!GameDebug.EnableLog)
                return;
            Debug.Log(message, context);
        }

        public static void LogWarning(object message)
        {
            Debug.LogWarning(message);
        }

        public static void LogWarning(object message, Object context)
        {
            Debug.LogWarning(message, context);
        }

        public static void LogError(object message)
        {
            Debug.LogError(message);
        }

        public static void LogError(object message, Object context)
        {
            Debug.LogError(message, context);
        }

        [Conditional("UNITY_ASSERTIONS")]
        public static void Assert(bool condition)
        {
            Debug.Assert(condition);
        }

        [Conditional("UNITY_ASSERTIONS")]
        public static void Assert(bool condition, object message)
        {
            Debug.Assert(condition, message);
        }

        [Conditional("UNITY_ASSERTIONS")]
        public static void Assert(bool condition, Object context)
        {
            Debug.Assert(condition, context);
        }

        [Conditional("UNITY_ASSERTIONS")]
        public static void Assert(bool condition, object message, Object context)
        {
            Debug.Assert(condition, message, context);
        }

        public static void LogException(Exception exception)
        {
            Debug.LogException(exception);
        }

        public static void LogException(Exception exception, Object context)
        {
            Debug.LogException(exception, context);
        }
    }
}