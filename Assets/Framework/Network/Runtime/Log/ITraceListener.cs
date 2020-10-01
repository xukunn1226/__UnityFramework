using System;

namespace Framework.NetWork.Log
{
    /// <summary>
    /// trace listener interface.
    /// </summary>
    public interface ITraceListener
    {
        /// <summary>
        /// debug
        /// </summary>
        /// <param name="message"></param>
        void Debug(string message);
        /// <summary>
        /// error
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        void Error(string message);
        /// <summary>
        /// info
        /// </summary>
        /// <param name="message"></param>
        void Warning(string message);
    }
}