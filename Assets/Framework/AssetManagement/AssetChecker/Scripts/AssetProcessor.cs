using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;

namespace Framework.AssetManagement.AssetChecker
{
    public interface IAssetProcessor
    {
        string DoProcess(string assetPath);
    }

    public class AssetProcessor_Mesh : IAssetProcessor
    {
        public int threshold { get; set; }

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