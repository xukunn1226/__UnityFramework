using System;

namespace Framework.NetWork.Log
{
    /// <summary>
    /// console trace listener
    /// </summary>
    public sealed class ConsoleListener : ITraceListener
    {
        /// <summary>
        /// debug
        /// </summary>
        /// <param name="message"></param>
        public void Debug(string message)
        {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE
            UnityEngine.Debug.Log(string.Concat("Debug: ", message, Environment.NewLine));
#else
            Console.WriteLine(string.Concat("Debug: ", message, Environment.NewLine));
#endif
        }
        /// <summary>
        /// error
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public void Error(string message)
        {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE
            UnityEngine.Debug.LogError(string.Concat("Error: ", message, Environment.NewLine));
#else
            Console.WriteLine(string.Concat("Error: ", message, Environment.NewLine));
#endif
        }
        /// <summary>
        /// warning
        /// </summary>
        /// <param name="message"></param>
        public void Warning(string message)
        {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE
            UnityEngine.Debug.LogWarning(string.Concat("Warning: ", message, Environment.NewLine));
#else
            Console.WriteLine(string.Concat("Warning: ", message, Environment.NewLine));
#endif
        }
    }
}