using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
using System.IO;
using System;
using Sirenix.OdinInspector;

namespace Framework.AssetManagement.AssetChecker
{
    public interface IAssetFilter
    {
        List<string> DoFilter();
    }

    /// <summary>
    /// 路径过滤器
    /// </summary>
    [Serializable]
    public class AssetFilter_Path : IAssetFilter
    {
        [ShowInInspector]
        [LabelText("筛选路径")]
        public List<string> input = new List<string>();             // 需要筛选的根目录

        [ShowInInspector]
        [LabelText("路径正则")]
        public string       pattern;                                // 正则表达式

        static public AssetFilter_Path Create(AssetFilter_Path other)
        {
            AssetFilter_Path result = new AssetFilter_Path();
            result.input.AddRange(other.input);
            result.pattern = other.pattern;
            return result;
        }

        /// <summary>
        /// 筛选出符合条件的目录列表，输入是目录列表，输出也是目录列表
        /// </summary>
        /// <param name="input"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public List<string> DoFilter()
        {
            if (input == null || input.Count != 1)
                throw new System.ArgumentNullException(@"PathFilter: unsupport EMPTY directory or input directories count > 1");

            List<string> result = new List<string>();
            try
            {
                DirectoryInfo di = new DirectoryInfo(input[0]);
                DirectoryInfo[] dis = di.GetDirectories("*", SearchOption.AllDirectories);
                foreach (var dir in dis)
                {
                    string path = dir.FullName.Replace(@"\", @"/");
                    if(IsMatch(path, pattern))
                        result.Add(AssetCheckerUtility.TrimProjectFolder(path));
                }
                result.Add(AssetCheckerUtility.TrimProjectFolder(input[0]));    // 始终包括输入的文件目录
            }
            catch(System.Exception e)
            {
                Debug.LogException(e);
            }
            return result;
        }

        private bool IsMatch(string path, string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                return true;

            Regex regex = new Regex(pattern);
            return regex.IsMatch(path);                
        }
    }

    public class AssetFilter_Filename : IAssetFilter
    {
        public enum UnityType
        {
            Object,
            AnimationClip,
            AudioClip,
            ComputerShader,
            Font,
            GUISkin,
            Material,
            Mesh,
            Model,
            PhysicMaterial,
            Prefab,
            Shader,
            Sprite,
            Texture,
            VideoClip,
        }

        [ShowInInspector]
        [LabelText("筛选路径")]
        public List<string> input = new List<string>();                 // 需要筛选的根目录

        [ShowInInspector]
        [LabelText("文件名正则")]
        public string       nameFilter;                                 // 文件名正则表达式

        [ShowInInspector]
        [LabelText("类型")]
        public UnityType    typeFilter = UnityType.Object;              // 类型过滤器

        static public AssetFilter_Filename Create(AssetFilter_Filename other)
        {
            AssetFilter_Filename result = new AssetFilter_Filename();
            result.input.AddRange(other.input);
            result.nameFilter = other.nameFilter;
            result.typeFilter = other.typeFilter;
            return result;
        }

        /// <summary>
        /// 筛选出符合条件的文件列表，输入是目录列表，输出是文件列表
        /// </summary>
        /// <returns>返回符合正则的文件路径，去除了工程目录前缀</returns>
        public List<string> DoFilter()
        {
            List<string> result = new List<string>();

            try
            {
                foreach (var dir in input)
                {
                    DirectoryInfo di = new DirectoryInfo(dir);
                    FileInfo[] fis = di.GetFiles("*", SearchOption.TopDirectoryOnly);
                    foreach (var fi in fis)
                    {
                        if(IsMatchFilename(fi.Name, nameFilter))
                        {
                            string assetPath = AssetCheckerUtility.TrimProjectFolder(fi.FullName.Replace(@"\", @"/"));
                            if (IsMatchUnityType(assetPath, typeFilter))
                                result.Add(assetPath);
                        }
                    }
                }
            }
            catch(System.Exception e)
            {
                Debug.LogException(e);
            }

            return result;
        }

        private bool IsMatchFilename(string filename, string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                return true;

            Regex regex = new Regex(pattern);
            return regex.IsMatch(filename);
        }

        private Type GetUnityObjectType(UnityType type)
        {
            switch (type)
            {
                case UnityType.Object:
                    return typeof(UnityEngine.Object);
                case UnityType.AnimationClip:
                    return typeof(UnityEngine.AnimationClip);
                case UnityType.AudioClip:
                    return typeof(UnityEngine.AudioClip);
                case UnityType.ComputerShader:
                    return typeof(UnityEngine.ComputeShader);
                case UnityType.Font:
                    return typeof(UnityEngine.Font);
                case UnityType.GUISkin:
                    return typeof(UnityEngine.GUISkin);
                case UnityType.Material:
                    return typeof(UnityEngine.Material);
                case UnityType.Mesh:
                    return typeof(UnityEngine.Mesh);
                case UnityType.Model:
                    return typeof(UnityEngine.GameObject);
                case UnityType.PhysicMaterial:
                    return typeof(UnityEngine.PhysicMaterial);
                case UnityType.Prefab:
                    return typeof(UnityEngine.GameObject);
                case UnityType.Shader:
                    return typeof(UnityEngine.Shader);
                case UnityType.Sprite:
                    return typeof(UnityEngine.Sprite);
                case UnityType.Texture:
                    return typeof(UnityEngine.Texture);
                case UnityType.VideoClip:
                    return typeof(UnityEngine.Video.VideoClip);
            }
            return null;
        }

        private bool IsMatchUnityType(string assetPath, UnityType unityType)
        {
            Type t = GetUnityObjectType(unityType);
            if (t == null)
                throw new System.Exception($"unsupported unity type: {unityType}");

            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(assetPath, t);
            if (obj == null)
                return false;

            if (unityType == UnityType.Object)
            {
                return true;
            }

            if (unityType == UnityType.Prefab)
            {
                AssetImporter importer = AssetImporter.GetAtPath(assetPath);
                return importer.GetType().Name == "PrefabImporter";
            }

            if (unityType == UnityType.Model)
            {
                AssetImporter importer = AssetImporter.GetAtPath(assetPath);
                return importer.GetType().Name == "ModelImporter";
            }

            return true;
        }
    }
}