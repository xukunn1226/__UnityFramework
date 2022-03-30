using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

namespace Framework.AssetManagement.AssetBuilder
{
    public class AssetImporterSetting : ScriptableObject
    {
        static private string s_SavedPath = "Assets/Framework/AssetManagement/AssetBuilder";

        [System.Serializable]
        public class Rule
        {            
            public string                   Name;                                   // 规则名称
            public List<string>             PathList        = new List<string>();   // 路径列表（规则应用于此）
            public List<IAssetProcessor>    Processor;                              // 资源处理器
            public Rule()
            {
                Name = "Test";
                PathList.Add("");
            }
        }

        [System.Serializable]
        public class Category
        {
            public string                   Group;                                  // 规则类型（仅作分类用）
            public List<Rule>               Rules           = new List<Rule>();

            public Category()
            {
                Group = "Test";
                Rules.Add(new Rule());
            }
        }

        public List<Category>               CategoryList    = new List<Category>();

        [MenuItem("Tools/Assets Management/Open AssetImporter Setting", false, 1)]
        static private void GetOrCreateAssetImporterSetting()
        {
            AssetImporterSetting asset = GetDefault();
            if (asset != null)
                Selection.activeObject = asset;
        }

        static public AssetImporterSetting GetDefault()
        {
            return Core.Editor.EditorUtility.GetOrCreateEditorConfigObject<AssetImporterSetting>(s_SavedPath);
        }

        public int AddCategory()
        {
            CategoryList.Add(new Category());
            return CategoryList.Count - 1;
        }

        public void RemoveCategory(int index)
        {
            if(index < 0 || index >= CategoryList.Count)
                throw new System.ArgumentOutOfRangeException($"{index} out of range");

            CategoryList.RemoveAt(index);
        }

        public string Execute(Rule rule)
        {
            string err = null;
            List<string> guids = FindAllAssetPaths(rule.PathList);
            foreach(var process in rule.Processor)
            {
                foreach(var guid in guids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    if(process.IsMatch(Path.GetFileNameWithoutExtension(assetPath), Path.GetExtension(assetPath)))
                    {
                        err += process.Execute(assetPath);
                    }
                }
            }
            return err;
        }

        public string Execute(Category category)
        {
            string err = null;
            foreach(var rule in category.Rules)
            {
                err += Execute(rule);
            }
            return err;
        }

        public string ExecuteAll()
        {
            string err = null;
            foreach(var category in CategoryList)
            {
                err += Execute(category);
            }
            return err;
        }

        public List<string> FindAllAssetPaths(List<string> pathList)
        {
            List<string> assetPaths = new List<string>();
            foreach(var path in pathList)
            {
                assetPaths.AddRange(AssetDatabase.FindAssets("*.*", new string[] { path }));
            }
            return assetPaths;
        }

        static private string FormatAssetPath(string assetPath)
        {
            string prefix = UnityEngine.Application.dataPath.Substring(0, UnityEngine.Application.dataPath.LastIndexOf("/"));
            return assetPath.Replace("\\", "/").Substring(prefix.Length + 1);
        }
    }
}