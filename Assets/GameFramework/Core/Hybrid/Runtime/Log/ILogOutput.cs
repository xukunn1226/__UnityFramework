using System.Collections;
using System;
using UnityEngine;

namespace Core
{
    public interface ILogOutput : IDisposable
    {
        void Output(string logString, string stackTrace, LogType type);

        void Flush();
    }
}