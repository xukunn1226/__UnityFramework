using System;

namespace Framework.NetWork.Log
{
    /// <summary>
    /// diagnostic listener
    /// </summary>
    public sealed class DiagnosticListener : ITraceListener
    {
        /// <summary>
        /// debug
        /// </summary>
        /// <param name="message"></param>
        public void Debug(string message)
        {
            System.Diagnostics.Trace.WriteLine(message);
        }
        /// <summary>
        /// error
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public void Error(string message)
        {
            System.Diagnostics.Trace.TraceError(message);
        }
        /// <summary>
        /// info
        /// </summary>
        /// <param name="message"></param>
        public void Warning(string message)
        {
            System.Diagnostics.Trace.TraceInformation(message);
        }
    }
}