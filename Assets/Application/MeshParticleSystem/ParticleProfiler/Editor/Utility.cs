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
        static public MethodInfo m_GetRuntimeMemorySizeLong = null;                 // 运行时纹理内存大小
        static public MethodInfo m_GetTextureFormat = null;                         // 当前平台纹理格式

        static Utility()
        {
            Type t = typeof(AssetDatabase).Assembly.GetType("UnityEditor.TextureUtil");

            m_GetRuntimeMemorySizeLong = t.GetMethod("GetRuntimeMemorySizeLong", BindingFlags.Public | BindingFlags.Static);

            m_GetTextureFormat = t.GetMethod("GetTextureFormat", BindingFlags.Public | BindingFlags.Static);
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

        static public long GetRuntimeMemorySizeLongOnAndroid(Texture texture)
        {
            string path = AssetDatabase.GetAssetPath(texture);
            TextureImporter ti = TextureImporter.GetAtPath(path) as TextureImporter;
            if(ti == null)
                return 0;

            long size = texture.width * texture.height * (ti.DoesSourceTextureHaveAlpha() ? 8 : 4) / 8;
            if(ti.mipmapEnabled)
                size = size * 13 / 10;

            return size;
        }

        static public long GetRuntimeMemorySizeLongOnIPhone(Texture texture)
        {
            string path = AssetDatabase.GetAssetPath(texture);
            TextureImporter ti = TextureImporter.GetAtPath(path) as TextureImporter;
            if(ti == null)
                return 0;

            float size = texture.width * texture.height * (3.56f) / 8;
            if(ti.mipmapEnabled)
                size = size * 1.3f;

            return (long)size;
        }
    }
}