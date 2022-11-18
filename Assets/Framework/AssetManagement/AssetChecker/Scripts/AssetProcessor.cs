using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using Sirenix.OdinInspector;

namespace Framework.AssetManagement.AssetChecker
{
    public interface IAssetProcessor
    {
        string DoProcess(string assetPath);
    }

    public class AssetProcessor_Mesh : IAssetProcessor
    {
        [ShowInInspector]
        [LabelText("顶点数阈值")]
        public int threshold;

        public string DoProcess(string assetPath)
        {
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
            if (mesh == null)
                return string.Format($"{assetPath}: 非Mesh资源");
                
            if (mesh.vertexCount < threshold)
                return null;
            return string.Format($"{assetPath}: 模型顶点数量大于预设值: {mesh.vertexCount} > {threshold}");
        }
    }

    public class AssetProcessor_Bone : IAssetProcessor
    {
        [ShowInInspector]
        [LabelText("顶点数阈值")]
        public int threshold;

        public string DoProcess(string assetPath)
        {
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
            if (mesh == null)
                return string.Format($"{assetPath}: 非Mesh资源");

            if (mesh.vertexCount < threshold)
                return null;
           
            return string.Format($"{assetPath}: 模型顶点数量大于预设值: {mesh.vertexCount} > {threshold}");
        }
    }


    public class AssetProcessor_Texture : IAssetProcessor
    {
        [ShowInInspector]
        [LabelText("贴图长度")]
        public int texWidth;
        [LabelText("贴图宽度")]
        public int texHight;
        
        public string DoProcess(string assetPath)
        {
            Texture2D Tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);

            if (Tex == null)
                return string.Format($"{assetPath}: 非贴图资源");

            if (Tex.width <= texWidth && Tex.height <= texHight)
                return null;

            if (Tex.width > texWidth && Tex.height > texHight)
                return string.Format($"{assetPath}:贴图长宽都超出规范:{Tex.width} * {Tex.height} > {texWidth} * {texHight}");
            else if(Tex.width > texWidth)
                return string.Format($"{assetPath}: 贴图宽度超出规范: {Tex.width} > {texWidth}");
            else
                return string.Format($"{assetPath}: 贴图长度超出规范: {Tex.height} > {texHight} ");
        }
    }
    
}