using System;
using System.Collections.Generic;

namespace Framework.NetWork.Log
{
    /// <summary>
    /// trace
    /// </summary>
    static public class Trace
    {
        static private readonly List<ITraceListener> m_List = new List<ITraceListener>();

        /// <summary>
        /// enable console trace listener
        /// </summary>
        static public void EnableConsole()
        {
            m_List.Add(new ConsoleListener());
        }
        /// <summary>
        /// enable diagnostic
        /// </summary>
        static public void EnableDiagnostic()
        {
            m_List.Add(new DiagnosticListener());
        }

        /// <summary>
        /// add listener
        /// </summary>
        /// <param name="listener"></param>
        /// <exception cref="ArgumentNullException">listener is null</exception>
        static public void AddListener(ITraceListener listener)
        {
            if (listener == null) throw new ArgumentNullException("listener");
            m_List.Add(listener);
        }

        /// <summary>
        /// debug
        /// </summary>
        /// <param name="message"></param>
        /// <exception cref="ArgumentNullException">message is null</exception>
        static public void Debug(string message)
        {
            if (message == null) throw new ArgumentNullException("message");
            m_List.ForEach(c => c.Debug(message));
        }
        /// <summary>
        /// info
        /// </summary>
        /// <param name="message"></param>
        /// <exception cref="ArgumentNullException">message is null</exception>
        static public void Warning(string message)
        {
            if (message == null) throw new ArgumentNullException("message");
            m_List.ForEach(c => c.Warning(message));
        }
        /// <summary>
        /// error
        /// </summary>
        /// <param name="message"></param>
        /// <exception cref="ArgumentNullException">message is null</exception>
        static public void Error(string message)
        {
            if (message == null) throw new ArgumentNullException("message");
            m_List.ForEach(c => c.Error(message));
        }
    }
}