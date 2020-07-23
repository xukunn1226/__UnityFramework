using System.Collections;
using System;
using UnityEngine;

namespace Framework.Core
{
    [RequireComponent(typeof(GameDebug))]
    public abstract class BaseLogOutput : MonoBehaviour, IDisposable
    {
        protected GameDebug m_Debugger;

        protected virtual void OnEnable()
        {
            m_Debugger = GetComponent<GameDebug>();
            m_Debugger.RegisterOutputDevice(this);            
        }

        protected virtual void OnDisable()
        {
            Flush();
            m_Debugger.UnregisterOutputDevice(this);
        }

        protected virtual void OnDestroy()
        {
            Flush();
            Dispose();
        }

        public abstract void Output(string logString, string stackTrace, LogType type);
        public abstract void Flush();
        public abstract void Dispose();
    }
}