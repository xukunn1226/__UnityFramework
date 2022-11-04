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
    /// ·��������
    /// </summary>
    [Serializable]
    public class AssetFilter_Path : IAssetFilter
    {
        [ShowInInspector]
        [LabelText("ɸѡ·��")]
        public List<string> input = new List<string>();             // ��Ҫɸѡ�ĸ�Ŀ¼

        [ShowInInspector]
        [LabelText("·������")]
        public string       pattern;                                // ������ʽ

        static public AssetFilter_Path Create(AssetFilter_Path other)
        {
            AssetFilter_Path result = new AssetFilter_Path();
            result.input.AddRange(other.input);
            result.pattern = other.pattern;
            return result;
        }

        /// <summary>
        /// ɸѡ������������Ŀ¼�б�������Ŀ¼�б����Ҳ��Ŀ¼�б�
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
                result.Add(AssetCheckerUtility.TrimProjectFolder(input[0]));    // ʼ�հ���������ļ�Ŀ¼
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
        [LabelText("ɸѡ·��")]
        public List<string> input = new List<string>();                 // ��Ҫɸѡ�ĸ�Ŀ¼

        [ShowInInspector]
        [LabelText("�ļ�������")]
        public string       nameFilter;                                 // �ļ���������ʽ

        [ShowInInspector]
        [LabelText("����")]
        public UnityType    typeFilter = UnityType.Object;              // ���͹�����

        static public AssetFilter_Filename Create(AssetFilter_Filename other)
        {
            AssetFilter_Filename result = new AssetFilter_Filename();
            result.input.AddRange(other.input);
            result.nameFilter = other.nameFilter;
            result.typeFilter = other.typeFilter;
            return result;
        }

        /// <summary>
        /// ɸѡ�������������ļ��б�������Ŀ¼�б�������ļ��б�
        /// </summary>
        /// <returns>���ط���������ļ�·����ȥ���˹���Ŀ¼ǰ׺</returns>
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