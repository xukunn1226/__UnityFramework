using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Core
{
    public class FPSCounter : SingletonMono<FPSCounter>
    {
        [Range(1, 2048)]
        public int          FrameRange              = 200;
        private int         m_CachedFrameRange;
        private const int   m_BufferSize            = 2048;
        private const int   m_BufferMask            = m_BufferSize - 1;
        private float[]     m_Buffer                = new float[m_BufferSize];
        private int         m_Position;
        private int         m_LastPosition;
        private int         m_FirstPosition;
        private float       m_TotalFPS;
        private int         m_ValidCount;

        public int          FPS         { get; private set; }
        public int          HighestFPS  { get; private set; }
        public int          LowestFPS   { get; private set; }
        public bool         ShowHLFPS;

        protected override void Awake()
        {
            base.Awake();
            SetFrameRange(FrameRange);
        }

        public void SetFrameRange(int newRange)
        {
            if(newRange != m_CachedFrameRange)
            {
                m_CachedFrameRange = Mathf.Clamp(newRange, 1, m_BufferSize);
                m_ValidCount = 0;
                m_TotalFPS = 0;
                HighestFPS = 0;
                LowestFPS = int.MaxValue;
            }
        }

        void Update()
        {
            UpdateFPSBuffer();
            CalculateFPSProgressively();

            // Debug.Log($"{FPS.ToString()}    {HighestFPS}    {LowestFPS}");
        }

        private void UpdateFPSBuffer()
        {
            m_Buffer[m_Position] = (float)(1.0f / Time.unscaledDeltaTime);
            m_LastPosition = m_Position;
            m_Position = (m_Position + 1) & m_BufferMask;
        }

        private void CalculateFPSProgressively()
        {
            m_FirstPosition = (m_LastPosition - m_CachedFrameRange + m_BufferSize) & m_BufferMask;

            if(m_ValidCount == m_CachedFrameRange)
                m_TotalFPS += (m_Buffer[m_LastPosition] - m_Buffer[m_FirstPosition]);
            else
                m_TotalFPS += m_Buffer[m_LastPosition];

            if(ShowHLFPS)
            {
                HighestFPS = 0;
                LowestFPS = int.MaxValue;
                if(m_LastPosition >= m_FirstPosition)
                {
                    for(int i = 0; i < m_LastPosition - m_FirstPosition + 1; ++i)
                    {
                        UpdateHighestAndLowestFPS(m_Buffer[m_FirstPosition + i]);
                    }
                }
                else
                {
                    for(int i = 0; i <= m_LastPosition; ++i)
                    {
                        UpdateHighestAndLowestFPS(m_Buffer[i]);
                    }
                    for(int i = m_FirstPosition; i < m_Buffer.Length; ++i)
                    {
                        UpdateHighestAndLowestFPS(m_Buffer[i]);
                    }
                }
            }

            m_ValidCount = Mathf.Min(++m_ValidCount, m_CachedFrameRange);
            FPS = (int)(m_TotalFPS / m_ValidCount);
        }

        private void UpdateHighestAndLowestFPS(float fps)
        {
            if(HighestFPS < fps)
                HighestFPS = (int)fps;
            if (LowestFPS >= fps)
                LowestFPS = (int)fps;
        }

        private void CalculateFPSDirectly()
        {
            m_TotalFPS = 0;
            m_FirstPosition = (m_LastPosition - m_CachedFrameRange + 1 + m_BufferSize) & m_BufferMask;

            if(m_LastPosition >= m_FirstPosition)
            {
                for(int i = 0; i < m_LastPosition - m_FirstPosition + 1; ++i)
                {
                    m_TotalFPS += m_Buffer[m_FirstPosition + i];
                }
                m_ValidCount = m_LastPosition - m_FirstPosition + 1;                
            }
            else
            {
                for(int i = 0; i <= m_LastPosition; ++i)
                {
                    m_TotalFPS += m_Buffer[i];
                }
                for(int i = m_FirstPosition; i < m_Buffer.Length; ++i)
                {
                    m_TotalFPS += m_Buffer[i];
                }
                m_ValidCount = (m_LastPosition + 1) + (m_Buffer.Length - m_FirstPosition);
            }

            FPS = (int)(m_TotalFPS / m_ValidCount);
        }

        public float GetLatestRangeFPS(int latestRange)
        {
            latestRange = Mathf.Clamp(latestRange, 1, m_BufferSize);
            if(latestRange == m_CachedFrameRange)
                return FPS;

            // cache
            int cachedRange = m_CachedFrameRange;
            float cachedFPS = m_TotalFPS;
            int cachedValidCount = m_ValidCount;

            m_CachedFrameRange = latestRange;
            CalculateFPSDirectly();

            // restore
            m_CachedFrameRange = cachedRange;
            m_TotalFPS = cachedFPS;
            m_ValidCount = cachedValidCount;

            return FPS;
        }
    }
}