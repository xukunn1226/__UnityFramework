using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Runtime
{
    public class FPSCounter : MonoBehaviour
    {
        private const int   m_BufferSize            = 256;
        private const int   m_BufferMask            = m_BufferSize - 1;
        private float[]     m_Buffer                = new float[m_BufferSize];
        private int         m_Position;
        private float       m_TotalFPS;

        public int          FPS         { get; private set; }
        public int          HighestFPS  { get; private set; }
        public int          LowestFPS   { get; private set; }
        public bool         ShowHLFPS;

        void Awake()
        {
            m_TotalFPS = 0;
            HighestFPS = 0;
            LowestFPS = int.MaxValue;
        }

        void Update()
        {
            CalculateFPSProgressively();
        }

        private void CalculateFPSProgressively()
        {
            float oldFPS = m_Buffer[m_Position];
            float newFPS = 1.0f / Time.smoothDeltaTime;

            m_Buffer[m_Position] = newFPS;         // fps = frameCount / elapsedTime
            m_TotalFPS += (newFPS - oldFPS);
            FPS = (int)(m_TotalFPS / m_Buffer.Length);

            m_Position = (m_Position + 1) & m_BufferMask;

            if(ShowHLFPS)
            {
                HighestFPS = 0;
                LowestFPS = int.MaxValue;
                foreach(var fps in m_Buffer)
                {
                    UpdateHighestAndLowestFPS(fps);
                }
            }
        }

        private void UpdateHighestAndLowestFPS(float fps)
        {
            if(HighestFPS < fps)
                HighestFPS = (int)fps;
            if (LowestFPS >= fps)
                LowestFPS = (int)fps;
        }
    }
}