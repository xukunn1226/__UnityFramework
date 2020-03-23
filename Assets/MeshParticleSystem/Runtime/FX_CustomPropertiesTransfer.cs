﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshParticleSystem
{
    /// <summary>
    /// 向材质传输参数（float, vector4, color, uv）
    /// </summary>
    public class FX_CustomPropertiesTransfer : MonoBehaviour
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

            public void Init()
            {
                m_Delay = Delay;
                m_Duration = Duration;
            }

            public void Reset(MaterialPropertyBlock block)
            {
                m_Delay = Delay;
                m_Duration = Duration;

                // 恢复初始值
                block.SetColor(FX_Const.SerializedIDToPropID[Id], Mode == ControlMode.Constant ? Color : Gradient.Evaluate(0));
            }

            public void Update(MaterialPropertyBlock block)
            {
                if(m_Delay > 0)
                {
                    m_Delay -= Time.deltaTime;
                    return;
                }

                if (Mode == ControlMode.Constant)
                {
                    block.SetColor(FX_Const.SerializedIDToPropID[Id], Color);
                }
                else
                {
                    m_Duration -= Time.deltaTime;

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
            public bool             Loop;

            public ControlMode      Mode;
            public float            Value;
            public AnimationCurve   Curve;

            public float            m_Delay     { get; private set; }
            public float            m_Duration  { get; private set; }

            public void Init()
            {
                m_Delay = Delay;
                m_Duration = Duration;
            }

            public void Reset(MaterialPropertyBlock block)
            {
                m_Delay = Delay;
                m_Duration = Duration;

                // 恢复初始值
                block.SetFloat(FX_Const.SerializedIDToPropID[Id], Mode == ControlMode.Constant ? Value : Curve.Evaluate(0));
            }

            public void Update(MaterialPropertyBlock block)
            {
                if(m_Delay > 0)
                {
                    m_Delay -= Time.deltaTime;
                    return;
                }

                if (Mode == ControlMode.Constant)
                {
                    block.SetFloat(FX_Const.SerializedIDToPropID[Id], Value);
                }
                else
                {
                    m_Duration -= Time.deltaTime;

                    float percent = 1;
                    if (m_Duration >= 0 || Loop)
                    {
                        m_Duration = (m_Duration < 0 && Loop) ? m_Duration + Duration : m_Duration;
                        percent = Mathf.Clamp01(1 - (m_Duration / Duration));
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
            public bool             Loop;

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

            public float            m_Delay     { get; private set; }
            public float            m_Duration  { get; private set; }
            private Vector4         m_Value;

            public void Init()
            {
                m_Delay = Delay;
                m_Duration = Duration;
            }

            public void Reset(MaterialPropertyBlock block)
            {
                m_Delay = Delay;
                m_Duration = Duration;

                // 恢复初始值
                m_Value.x = ModeX == ControlMode.Constant ? ValueX : CurveX.Evaluate(0);
                m_Value.y = ModeY == ControlMode.Constant ? ValueY : CurveY.Evaluate(0);
                m_Value.z = ModeZ == ControlMode.Constant ? ValueZ : CurveZ.Evaluate(0);
                m_Value.w = ModeW == ControlMode.Constant ? ValueW : CurveW.Evaluate(0);
                block.SetVector(FX_Const.SerializedIDToPropID[Id], m_Value);
            }

            public void Update(MaterialPropertyBlock block)
            {
                if(m_Delay > 0)
                {
                    m_Delay -= Time.deltaTime;
                    return;
                }

                m_Duration -= Time.deltaTime;

                float percent = 1;
                if (m_Duration >= 0 || Loop)
                {
                    m_Duration = (m_Duration < 0 && Loop) ? m_Duration + Duration : m_Duration;
                    percent = Mathf.Clamp01(1 - (m_Duration / Duration));
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
            public float            Duration;
            public Vector2          Speed;
            public Vector2          StartOffset;
            public Vector2          TileScale       = Vector2.one;
            public bool             Loop;
            public bool             LoopReset;
            public AnimationCurve   Curve;

            public float            m_Delay         { get; private set; }
            public float            m_Duration      { get; private set; }
            public Vector2          m_TotalSpeed    { get; private set; }
            private static Vector4  k_MainTex_ST    = new Vector4(1, 1, 0, 0);

            public void Init()
            {
                m_Delay = Delay;
                m_Duration = Duration;
            }

            public void Reset(MaterialPropertyBlock block)
            {
                m_Delay = Delay;
                m_Duration = Duration;
                m_TotalSpeed = Vector2.zero;
                k_MainTex_ST = new Vector4(1, 1, 0, 0);
                block.SetVector(FX_Const.SerializedIDToPropID[0], k_MainTex_ST);
            }

            public void Update(MaterialPropertyBlock block)
            {
                if (m_Delay > 0)
                {
                    m_Delay -= Time.deltaTime;
                    return;
                }

                m_Duration -= Time.deltaTime;
                float percent = 1;
                if (m_Duration >= 0)
                {
                    percent = 1 - (m_Duration / Duration);
                    m_TotalSpeed += Curve.Evaluate(percent) * Speed * Time.deltaTime;
                }
                else
                {
                    if (Loop)
                    {
                        if (LoopReset)
                            m_TotalSpeed = Vector2.zero;
                        m_Duration += Duration;
                        percent = 1 - (m_Duration / Duration);
                        m_TotalSpeed += Curve.Evaluate(percent) * Speed * Time.deltaTime;
                    }
                }

                k_MainTex_ST.x = TileScale.x;
                k_MainTex_ST.y = TileScale.y;
                k_MainTex_ST.z = m_TotalSpeed.x + StartOffset.x;
                k_MainTex_ST.w = m_TotalSpeed.y + StartOffset.y;
                block.SetVector(FX_Const.SerializedIDToPropID[0], k_MainTex_ST);
            }
        }


        [SerializeField] private List<CustomProp_Color>         m_CustomPropColorList;
        [SerializeField] private List<CustomProp_Float>         m_CustomPropFloatList;
        [SerializeField] private List<CustomProp_Vector4>       m_CustomPropVector4List;
        [SerializeField] private CustomProp_UV                  m_CustomPropUV;
        //public bool                                             useMeshProjection;

        private void Awake()
        {
            if (k_MaterialPropertyBlock == null)
                k_MaterialPropertyBlock = new MaterialPropertyBlock();

            m_Renderer = GetComponent<Renderer>();

            if (m_CustomPropColorList != null)
            {
                foreach (CustomProp_Color propColor in m_CustomPropColorList)
                {
                    propColor.Init();
                }
            }

            if (m_CustomPropFloatList != null)
            {
                foreach (CustomProp_Float propFloat in m_CustomPropFloatList)
                {
                    propFloat.Init();
                }
            }

            if (m_CustomPropVector4List != null)
            {
                foreach (CustomProp_Vector4 propVector4 in m_CustomPropVector4List)
                {
                    propVector4.Init();
                }
            }

            if(m_CustomPropUV != null && m_CustomPropUV.Active)
            {
                m_CustomPropUV.Init();
            }
        }

        private void OnEnable()
        {
            if (m_CustomPropColorList != null)
            {
                foreach (CustomProp_Color propColor in m_CustomPropColorList)
                {
                    propColor.Reset(k_MaterialPropertyBlock);
                }
            }

            if (m_CustomPropFloatList != null)
            {
                foreach (CustomProp_Float propFloat in m_CustomPropFloatList)
                {
                    propFloat.Reset(k_MaterialPropertyBlock);
                }
            }

            if (m_CustomPropVector4List != null)
            {
                foreach (CustomProp_Vector4 propVector4 in m_CustomPropVector4List)
                {
                    propVector4.Reset(k_MaterialPropertyBlock);
                }
            }

            if(m_CustomPropUV != null && m_CustomPropUV.Active)
            {
                m_CustomPropUV.Reset(k_MaterialPropertyBlock);
            }
        }

        private void UpdateColor()
        {
            if (m_CustomPropColorList == null)
                return;

            foreach (CustomProp_Color propColor in m_CustomPropColorList)
            {
                propColor.Update(k_MaterialPropertyBlock);
            }
        }

        private void UpdateFloat()
        {
            if (m_CustomPropFloatList == null)
                return;

            foreach(CustomProp_Float propFloat in m_CustomPropFloatList)
            {
                propFloat.Update(k_MaterialPropertyBlock);
            }
        }

        private void UpdateVector4()
        {
            if (m_CustomPropVector4List == null)
                return;

            foreach (CustomProp_Vector4 propVector4 in m_CustomPropVector4List)
            {
                propVector4.Update(k_MaterialPropertyBlock);
            }
        }

        private void UpdateUV()
        {
            if (m_CustomPropUV == null || !m_CustomPropUV.Active)
                return;

            m_CustomPropUV.Update(k_MaterialPropertyBlock);
        }

        //private void UpdateMeshProjection()
        //{
        //    k_MaterialPropertyBlock.SetMatrix(FX_Const.SerializedIDToPropID[50],        // "_ClipToLocal"
        //        (GL.GetGPUProjectionMatrix(Camera.current.projectionMatrix, true) * Camera.current.worldToCameraMatrix * transform.localToWorldMatrix).inverse);
        //}

        //private void OnWillRenderObject()
        //{
        //    if (m_Renderer == null /*|| !useMeshProjection*/)
        //        return;

        //    k_MaterialPropertyBlock.Clear();

        //    UpdateColor();
        //    UpdateMeshProjection();

        //    m_Renderer.SetPropertyBlock(k_MaterialPropertyBlock);
        //}

        private void Update()
        {
            if (m_Renderer == null /*|| useMeshProjection*/)
                return;

            k_MaterialPropertyBlock.Clear();

            UpdateColor();
            UpdateFloat();
            UpdateVector4();
            UpdateUV();

            m_Renderer.SetPropertyBlock(k_MaterialPropertyBlock);
        }
    }
}