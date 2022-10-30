using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
using System.IO;
using System;

namespace Framework.AssetManagement.AssetChecker
{
    public interface IAssetFilter
    {
        List<string>    input   { get; set; }
        string          pattern { get; set; }
        List<string> DoMatch();
    }

    /// <summary>
    /// 路径过滤器
    /// </summary>
    public class AssetChecker_PathFilter : IAssetFilter
    {
        public List<string> input   { get; set; }               // 需要筛选的根目录
        public string       pattern { get; set; }               // 正则表达式

        // 所有符合正则的路径，去除了工程目录前缀
        public List<string> DoMatch()
        {
            if (input == null || input.Count != 1)
                throw new System.ArgumentNullException(@"PathFilter: unsupport EMPTY directory or input directories count > 1");

            DirectoryInfo di = new DirectoryInfo(input[0]);
            DirectoryInfo[] dis = di.GetDirectories("*", SearchOption.AllDirectories);

            List<string> result = new List<string>();
            try
            {
                Regex regex = new Regex(pattern);

                foreach (var dir in dis)
                {
                    string path = dir.FullName.Replace(@"\", @"/");
                    if (regex.IsMatch(path))
                        result.Add(AssetCheckerUtility.TrimProjectFolder(path));
                }
            }
            catch(System.Exception e)
            {
                Debug.LogException(e);
            }
            return result;
        }
    }

    public class AssetChecker_FilenameFilter : IAssetFilter
    {
        public List<string> input   { get; set; }               // 需要筛选的根目录
        public string       pattern { get; set; }               // 正则表达式

        /// <summary>
        /// 
        /// </summary>
        /// <returns>返回符合正则的文件路径，去除了工程目录前缀</returns>
        public List<string> DoMatch()
        {
            List<string> result = new List<string>();

            try
            {
                Regex regex = new Regex(pattern);
                foreach (var dir in input)
                {
                    DirectoryInfo di = new DirectoryInfo(dir);
                    FileInfo[] fis = di.GetFiles();
                    foreach (var fi in fis)
                    {
                        if (regex.IsMatch(fi.Name))
                        {
                            string assetPath = fi.FullName.Replace(@"\", @"/");
                            result.Add(AssetCheckerUtility.TrimProjectFolder(assetPath));
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
    }

    public class AssetChecker_ExtensionFilter : IAssetFilter
    {
        public List<string> input   { get; set; }               // 需要筛选的文件路径
        public string       pattern { get; set; }               // 正则表达式

        public List<string> DoMatch()
        {
            List<string> result = new List<string>();
            foreach (var assetPath in input)
            {
                if(IsMatch(assetPath, pattern))
                {
                    result.Add(AssetCheckerUtility.TrimProjectFolder(assetPath));
                }
            }
            return result;
        }

        static private bool IsMatch(string assetPath, string type)
        {
            AssetCheckerUtility.UnityType unityType = AssetCheckerUtility.GetUnityType(type);
            Type t = AssetCheckerUtility.GetUnityObjectType(unityType);
            if(t == null)
                throw new System.Exception($"unsupported unity type: {type}");

            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(assetPath, t);
            if (obj == null)
                return false;

            if (unityType == AssetCheckerUtility.UnityType.Object)
            {
                return true;
            }

            if(unityType == AssetCheckerUtility.UnityType.Prefab)
            {
                AssetImporter importer = AssetImporter.GetAtPath(assetPath);
                return importer.GetType().Name == "PrefabImporter";
            }

            if(unityType == AssetCheckerUtility.UnityType.Model)
            {
                AssetImporter importer = AssetImporter.GetAtPath(assetPath);
                return importer.GetType().Name == "ModelImporter";
            }            

            return true;
        }
    }

    static public class AssetChecker_Test
    {
        [UnityEditor.MenuItem("Tools/AssetChecker_Test/Foo")]
        static private void Foo()
        {
            IAssetFilter filter = new AssetChecker_PathFilter();
            filter.input = new List<string> { "Assets/Resources" };
            filter.pattern = @"s/Wind";
            List<string> ret = filter.DoMatch();

            int count = 0;
            foreach(string s in ret)
            {
                Debug.Log(string.Format($"{count++}: {s}"));
            }
        }

        [UnityEditor.MenuItem("Tools/AssetChecker_Test/Foo2")]
        static private void Foo2()
        {
            IAssetFilter filter = new AssetChecker_FilenameFilter();
            filter.input = new List<string> { "Assets/Resources" };
            filter.pattern = @"a*(?!(meta)$)";
            List<string> ret = filter.DoMatch();

            int count = 0;
            foreach (string s in ret)
            {
                Debug.Log(string.Format($"{count++}: {s}"));
            }
        }

        [UnityEditor.MenuItem("Tools/AssetChecker_Test/PrintType")]
        static private void PrintType()
        {
            AssetImporter ai = AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]));
            Debug.Log(ai.GetType().Name);

            //AssetImporter.SourceAssetIdentifier identifier = new AssetImporter.SourceAssetIdentifier(Selection.activeObject);
            //Debug.Log(identifier.type);
        }

        [UnityEditor.MenuItem("Tools/AssetChecker_Test/TestSerialize")]
        static private void TestSerialize()
        {
            AssetProcessor_Mesh processor = new AssetProcessor_Mesh();
            processor.threshold = 312;

            BaseFuncComponentParam param = BaseFuncComponentParam<AssetProcessor_Mesh>.CreateParam(processor);

            AssetProcessor_Mesh p = (AssetProcessor_Mesh)BaseFuncComponentParam<IAssetProcessor>.CreateComponent(param);
            Debug.Log($"p: {p.threshold}");
        }
    }
}