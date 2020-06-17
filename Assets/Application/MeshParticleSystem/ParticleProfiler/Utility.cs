using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;

namespace MeshParticleSystem.Profiler
{
    [InitializeOnLoad]
    static public class Utility
    {
        static public MethodInfo m_CalcParticleCount = null;
        static public MethodInfo m_GetRuntimeMemorySizeLong = null;                 // 运行时纹理内存大小
        static public MethodInfo m_GetTextureFormat = null;                         // 当前平台纹理格式

        static Utility()
        {
            Type t = typeof(AssetDatabase).Assembly.GetType("UnityEditor.TextureUtil");

            m_GetRuntimeMemorySizeLong = t.GetMethod("GetRuntimeMemorySizeLong", BindingFlags.Public | BindingFlags.Static);
            // if(m_GetRuntimeMemorySizeLong != null)
            //     Debug.Log("222222222222");

            m_GetTextureFormat = t.GetMethod("GetTextureFormat", BindingFlags.Public | BindingFlags.Static);
            // if(m_GetTextureFormat != null)
            //     Debug.Log("333333333");

            m_CalcParticleCount = typeof(ParticleSystem).GetMethod("CalculateEffectUIData", BindingFlags.NonPublic | BindingFlags.Instance);
            // if(m_CalcParticleCount != null)
            //     Debug.Log("555555555");
        }

        private static object InvokeInternalAPI(string type, string method, params object[] parameters)
        {
            var assembly = typeof(AssetDatabase).Assembly;
            var custom = assembly.GetType(type);
            var methodInfo = custom.GetMethod(method, BindingFlags.Public | BindingFlags.Static);
            return methodInfo != null ? methodInfo.Invoke(null, parameters) : 0;
        }

        static public long GetRuntimeMemorySizeLong(Texture texture)
        {
            return (long)(m_GetRuntimeMemorySizeLong?.Invoke(null, new object[] {texture}) ?? 0);
        }

        static public string GetTextureFormatString(Texture texture)
        {
            return m_GetTextureFormat?.Invoke(null, new object[] {texture}).ToString() ?? null;
        }
    }
}