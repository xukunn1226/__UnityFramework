using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.MeshParticleSystem
{
    /// <summary>
    /// 向材质传输参数（float, vector4, color, uv）
    /// </summary>
    [ExecuteInEditMode]
    public class FX_CustomPropertiesTransfer : FX_Component
    {
        private static MaterialPropertyBlock    k_MaterialPropertyBlock;

        private Renderer                        m_Renderer;

        public enum ControlMode
        {
            Constant,
            Curve
        }

        [System.Serializable]
        public class CustomProp_Color
        {
            public int              Id;
            public float            Delay;
            public float            Duration;
            public bool             Loop;

            public ControlMode      Mode;
            public Color            Color;
            public Gradient         Gradient;

            public float            m_Delay         { get; private set; }
            public float            m_Duration      { get; private set; }

            //public void Awake()
            //{
            //    m_Delay = Delay;
            //    m_Duration = Duration;
            //}

            public void Init(MaterialPropertyBlock block)
            {
                m_Delay = Delay;
                m_Duration = Duration;

                // 恢复初始值
                block.SetColor(FX_Const.SerializedIDToPropID[Id], Mode == ControlMode.Constant ? Color : Gradient.Evaluate(0));
            }

            public void Update(MaterialPropertyBlock block, float deltaTime)
            {
                if(m_Delay > 0)
                {
                    m_Delay -= deltaTime;
                    return;
                }

                if (Mode == ControlMode.Constant)
                {
                    block.SetColor(FX_Const.SerializedIDToPropID[Id], Color);
                }
                else
                {
                    m_Duration -= deltaTime;

                    float percent = 1;
                    if (m_Duration >= 0 || Loop)
                    {
                        m_Duration = (m_Duration < 0 && Loop) ? m_Duration + Duration : m_Duration;
                        percent = Mathf.Clamp01(1 - (m_Duration / Duration));
                    }
                    block.SetColor(FX_Const.SerializedIDToPropID[Id], Gradient.Evaluate(percent));
                }
            }
        }

        [System.Serializable]
        public class CustomProp_Float
        {
            public int              Id;
            public float            Delay;
            public float            Duration;

            public ControlMode      Mode;
            public float            Value;
            public AnimationCurve   Curve;

            private float           m_Delay;
            private float           m_ElapsedTime;

            //public void Awake()
            //{
            //    m_Delay = Delay;
            //    m_ElapsedTime = 0;
            //}

            public void Init(MaterialPropertyBlock block)
            {
                m_Delay = Delay;
                m_ElapsedTime = 0;

                // 恢复初始值
                block.SetFloat(FX_Const.SerializedIDToPropID[Id], Mode == ControlMode.Constant ? Value : Curve.Evaluate(0));
            }

            public void Update(MaterialPropertyBlock block, float deltaTime)
            {
                if(m_Delay > 0)
                {
                    m_Delay -= deltaTime;
                    return;
                }

                if (Mode == ControlMode.Constant)
                {
                    block.SetFloat(FX_Const.SerializedIDToPropID[Id], Value);
                }
                else
                {
                    float percent = 1;
                    if(Duration > 0)
                    {
                        m_ElapsedTime += deltaTime;
                        percent = m_ElapsedTime / Duration;
                    }
                    block.SetFloat(FX_Const.SerializedIDToPropID[Id], Curve.Evaluate(percent));
                }
            }
        }

        [System.Serializable]
        public class CustomProp_Vector4
        {
            public int              Id;
            public float            Delay;
            public float            Duration;

            public ControlMode      ModeX;
            public float            ValueX;
            public AnimationCurve   CurveX;

            public ControlMode      ModeY;
            public float            ValueY;
            public AnimationCurve   CurveY;

            public ControlMode      ModeZ;
            public float            ValueZ;
            public AnimationCurve   CurveZ;

            public ControlMode      ModeW;
            public float            ValueW;
            public AnimationCurve   CurveW;

            private float           m_Delay;
            private float           m_ElapsedTime;
            private Vector4         m_Value;

            //public void Awake()
            //{
            //    m_Delay = Delay;
            //    m_ElapsedTime = 0;
            //}

            public void Init(MaterialPropertyBlock block)
            {
                m_Delay = Delay;
                m_ElapsedTime = 0;

                // 恢复初始值
                m_Value.x = ModeX == ControlMode.Constant ? ValueX : CurveX.Evaluate(0);
                m_Value.y = ModeY == ControlMode.Constant ? ValueY : CurveY.Evaluate(0);
                m_Value.z = ModeZ == ControlMode.Constant ? ValueZ : CurveZ.Evaluate(0);
                m_Value.w = ModeW == ControlMode.Constant ? ValueW : CurveW.Evaluate(0);
                block.SetVector(FX_Const.SerializedIDToPropID[Id], m_Value);
            }

            public void Update(MaterialPropertyBlock block, float deltaTime)
            {
                if(m_Delay > 0)
                {
                    m_Delay -= deltaTime;
                    return;
                }

                float percent = 1;
                if (Duration > 0)
                {
                    m_ElapsedTime += deltaTime;
                    percent = m_ElapsedTime / Duration;
                }

                m_Value.x = ModeX == ControlMode.Constant ? ValueX : CurveX.Evaluate(percent);
                m_Value.y = ModeY == ControlMode.Constant ? ValueY : CurveY.Evaluate(percent);
                m_Value.z = ModeZ == ControlMode.Constant ? ValueZ : CurveZ.Evaluate(percent);
                m_Value.w = ModeW == ControlMode.Constant ? ValueW : CurveW.Evaluate(percent);
                
                block.SetVector(FX_Const.SerializedIDToPropID[Id], m_Value);
            }
        }

        [System.Serializable]
        public class CustomProp_UV
        {
            public bool             Active;

            public float            Delay;

            public Vector2          Tiling = new Vector2(1, 1);

            public Vector2          Offset;

            public Vector2          Speed;

            public Vector2          m_AccumulatedSpeed;

            private float           m_Delay;

            private static Vector4  k_MainTex_ST = new Vector4(1, 1, 0, 0);

            //public void Awake()
            //{
            //    m_Delay = Delay;
            //    m_AccumulatedSpeed = new Vector2(0, 0);
            //}

            public void Init(MaterialPropertyBlock block)
            {
                m_Delay = Delay;
                m_AccumulatedSpeed = new Vector2(0, 0);

                k_MainTex_ST = new Vector4(1, 1, 0, 0);
                block.SetVector(FX_Const.SerializedIDToPropID[0], k_MainTex_ST);
            }

            public void Update(MaterialPropertyBlock block, float deltaTime)
            {
                if(m_Delay > 0)
                {
                    m_Delay -= deltaTime;
                    return;
                }

                m_AccumulatedSpeed += Speed * deltaTime;

                k_MainTex_ST.x = Tiling.x;
                k_MainTex_ST.y = Tiling.y;
                k_MainTex_ST.z = Offset.x + m_AccumulatedSpeed.x;
                k_MainTex_ST.w = Offset.y + m_AccumulatedSpeed.y;

                block.SetVector(FX_Const.SerializedIDToPropID[0], k_MainTex_ST);
            }
        }

        [System.Serializable]
        public class CustomProp_TextureSheet
        {
            public bool             Active;

#if UNITY_2019_1_OR_NEWER
            [Min(0)]
#endif
            public float            Duration;

#if UNITY_2019_1_OR_NEWER
            [Min(1)]
#endif
            public int              TileX = 1;

#if UNITY_2019_1_OR_NEWER
            [Min(1)]
#endif
            public int              TileY = 1;

#if UNITY_2019_1_OR_NEWER
            [Min(0)]
#endif
            public int              StartFrame;

#if UNITY_2019_1_OR_NEWER
            [Min(0)]
#endif
            public int              FrameCount;

            public AnimationCurve   Curve;

            private int             m_CurFrame;
            private float           m_ElapsedTime;

            private static Vector4  k_MainTex_ST = new Vector4(1, 1, 0, 0);

            public void Init(MaterialPropertyBlock block)
            {
                m_CurFrame = StartFrame;
                m_ElapsedTime = 0;
            }

            private void UpdateFrame(float deltaTime)
            {
                if (Duration > 0 && FrameCount > 0)
                {
                    m_ElapsedTime += deltaTime;

                    m_CurFrame = StartFrame + (int)((Curve.Evaluate(m_ElapsedTime / Duration) - 0.01f) * FrameCount);
                }
                else
                {
                    m_CurFrame = StartFrame;
                }
            }

            private void PlayFrame()
            {
                float InvTilesX = 1.0f / TileX;
                float InvTilesY = 1.0f / TileY;

                float OffsetX = m_CurFrame % TileX;
                float OffsetY = TileY - m_CurFrame / TileX - 1;

                k_MainTex_ST.x = InvTilesX;
                k_MainTex_ST.y = InvTilesY;
                k_MainTex_ST.z = InvTilesX * OffsetX;
                k_MainTex_ST.w = InvTilesY * OffsetY;
            }

            public void Update(MaterialPropertyBlock block, float deltaTime)
            {
                UpdateFrame(deltaTime);

                PlayFrame();

                block.SetVector(FX_Const.SerializedIDToPropID[0], k_MainTex_ST);
            }
        }

        [SerializeField] private List<CustomProp_Color>     m_CustomPropColorList = null;
        [SerializeField] private List<CustomProp_Float>     m_CustomPropFloatList = null;
        [SerializeField] private List<CustomProp_Vector4>   m_CustomPropVector4List = null;
        [SerializeField] private CustomProp_UV              m_CustomPropUV = null;
        [SerializeField] private CustomProp_TextureSheet    m_CustomPropTextureSheet = null;

        private void Awake()
        {
            m_Renderer = GetComponent<Renderer>();
        }

        protected override void Init()
        {
            base.Init();

            if (k_MaterialPropertyBlock == null)
                k_MaterialPropertyBlock = new MaterialPropertyBlock();

            if (m_CustomPropColorList != null)
            {
                foreach (CustomProp_Color propColor in m_CustomPropColorList)
                {
                    propColor.Init(k_MaterialPropertyBlock);
                }
            }

            if (m_CustomPropFloatList != null)
            {
                foreach (CustomProp_Float propFloat in m_CustomPropFloatList)
                {
                    propFloat.Init(k_MaterialPropertyBlock);
                }
            }

            if (m_CustomPropVector4List != null)
            {
                foreach (CustomProp_Vector4 propVector4 in m_CustomPropVector4List)
                {
                    propVector4.Init(k_MaterialPropertyBlock);
                }
            }

            if (m_CustomPropUV != null && m_CustomPropUV.Active)
            {
                m_CustomPropUV.Init(k_MaterialPropertyBlock);
            }

            if (m_CustomPropUV != null && m_CustomPropTextureSheet.Active)
            {
                m_CustomPropTextureSheet.Init(k_MaterialPropertyBlock);
            }
        }

        private void UpdateColor()
        {
            if (m_CustomPropColorList == null)
                return;

            foreach (CustomProp_Color propColor in m_CustomPropColorList)
            {
                propColor.Update(k_MaterialPropertyBlock, deltaTime);
            }
        }

        private void UpdateFloat()
        {
            if (m_CustomPropFloatList == null)
                return;

            foreach(CustomProp_Float propFloat in m_CustomPropFloatList)
            {
                propFloat.Update(k_MaterialPropertyBlock, deltaTime);
            }
        }

        private void UpdateVector4()
        {
            if (m_CustomPropVector4List == null)
                return;

            foreach (CustomProp_Vector4 propVector4 in m_CustomPropVector4List)
            {
                propVector4.Update(k_MaterialPropertyBlock, deltaTime);
            }
        }

        private void UpdateUV()
        {
            if (m_CustomPropUV == null || !m_CustomPropUV.Active)
                return;

            m_CustomPropUV.Update(k_MaterialPropertyBlock, deltaTime);
        }

        private void UpdateAtlas()
        {
            if (m_CustomPropTextureSheet == null || !m_CustomPropTextureSheet.Active)
                return;

            m_CustomPropTextureSheet.Update(k_MaterialPropertyBlock, deltaTime);
        }

        private void Update()
        {
            if (m_Renderer == null)
                return;

            k_MaterialPropertyBlock.Clear();

            UpdateColor();
            UpdateFloat();
            UpdateVector4();
            UpdateUV();
            UpdateAtlas();

            m_Renderer.SetPropertyBlock(k_MaterialPropertyBlock);
        }
    }
}