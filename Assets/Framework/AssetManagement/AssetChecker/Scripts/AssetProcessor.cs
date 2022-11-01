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
                throw new System.Exception($"can't load mesh object from {assetPath}");

            if (mesh.vertexCount < threshold)
                return null;
            return string.Format($"模型顶点数量大于预设值: {mesh.vertexCount} > {threshold}");
        }
    }
}