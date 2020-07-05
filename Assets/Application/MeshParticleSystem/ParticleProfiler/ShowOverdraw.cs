using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace MeshParticleSystem.Profiler
{
    public class ShowOverdraw : MonoBehaviour
    {
        static public float kRecommendFillrate = 3;

        private Camera m_CachedCamera;
        public Camera cachedCamera
        {
            get
            {
                if (m_CachedCamera == null)
                {
                    m_CachedCamera = Camera.main;
                }
                return m_CachedCamera;
            }
        }

        private const int m_rtWidth = 512;
        private const int m_rtHeight = 512;
        public RenderTexture m_RT;
        private Texture2D m_TexCopyFromRT;

        private int     m_pixTotalThisFrame;
        private int     m_pixActualDrawThisFrame;
        
        [System.Serializable]
        public class OverdrawData
        {
            public int      m_FrameCount;
            public int      m_pixTotal;
            public int      m_pixActualTotal;
            public float    m_Fillrate;

            public float GetAveragePixDraw()
            {
                if (m_FrameCount == 0)
                    return 0;
                return 1.0f * m_pixTotal / m_FrameCount;
            }

            public float GetAverageActualPixDraw()
            {
                if (m_FrameCount == 0)
                    return 0;
                return 1.0f * m_pixActualTotal / m_FrameCount;
            }

            public float GetAverageFillrate()
            {
                if (m_FrameCount == 0)
                    return 0;
                return GetAverageActualPixDraw() / GetAveragePixDraw();
            }
        }        
        public OverdrawData m_Data = new OverdrawData();

        void OnEnable()
        {
            if (cachedCamera != null)
            {
                cachedCamera.SetReplacementShader(Shader.Find("ParticleProfiler/OverDraw"), "");
            }

            m_RT = RenderTexture.GetTemporary(new RenderTextureDescriptor(m_rtWidth, m_rtHeight, RenderTextureFormat.ARGB32));
            m_TexCopyFromRT = new Texture2D(m_RT.width, m_RT.height, TextureFormat.ARGB32, false);

            m_Data.m_pixTotal = 0;
            m_Data.m_pixActualTotal = 0;
            m_Data.m_FrameCount = 0;

            Application.targetFrameRate = 30;           // 固定帧率测试
        }

        void OnDisable()
        {
            Texture2D.DestroyImmediate(m_TexCopyFromRT);
            RenderTexture.ReleaseTemporary(m_RT);
            m_RT = null;

            if (cachedCamera != null)
            {
                cachedCamera.ResetReplacementShader();
            }
        }

        void Update()
        {
            FetchOverdrawData();
        }

        void FetchOverdrawData()
        {
            if (cachedCamera == null)
                return;

            // set rt to camera and render
            cachedCamera.targetTexture = m_RT;
            cachedCamera.Render();
            cachedCamera.targetTexture = null;      // disable targetTexture will render to screen    

            RenderTexture prevRT = RenderTexture.active;
            {
                RenderTexture.active = m_RT;
                m_TexCopyFromRT.ReadPixels(new Rect(0, 0, m_RT.width, m_RT.height), 0, 0);          // read pixels from screen into the saved texture data

                GetOverDrawDataThisFrame(m_TexCopyFromRT, out m_pixTotalThisFrame, out m_pixActualDrawThisFrame);

                m_Data.m_pixTotal += m_pixTotalThisFrame;
                m_Data.m_pixActualTotal += m_pixActualDrawThisFrame;
                m_Data.m_FrameCount += 1;
                m_Data.m_Fillrate = m_Data.GetAverageFillrate();
            }
            RenderTexture.active = prevRT;
        }

        public void GetOverDrawDataThisFrame(Texture2D texture, out int pixTotal, out int pixActualDraw)
        {
            var texw = texture.width;
            var texh = texture.height;

            var pixels = texture.GetPixels();

            int index = 0;

            pixTotal = 0;
            pixActualDraw = 0;

            for (var y = 0; y < texh; y++)
            {
                for (var x = 0; x < texw; x++)
                {
                    float r = pixels[index].r;
                    float g = pixels[index].g;
                    float b = pixels[index].b;

                    bool isEmptyPix = IsEmptyPix(r, g, b);
                    if (!isEmptyPix)
                    {

                        pixTotal++;
                    }

                    int drawThisPixTimes = DrawPixTimes(r, g, b);
                    pixActualDraw += drawThisPixTimes;

                    index++;
                }
            }
        }

        //计算单像素的绘制次数
        private int DrawPixTimes(float r, float g, float b)
        {
            //在OverDraw.Shader中像素每渲染一次，g值就会叠加0.04
            return Convert.ToInt32(g / 0.04);
        }

        private bool IsEmptyPix(float r, float g, float b)
        {
            return r == 0 && g == 0 && b == 0;
        }
    }
}
