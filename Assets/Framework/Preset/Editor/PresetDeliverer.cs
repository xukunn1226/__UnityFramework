using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Presets;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

/*
* 常用四则表达式
* 包含xx：.*(xx).*
* 不包含xx：^((?!xx).)*$
* 以xx结尾：.*(xx)$         1、.*(face.*_N)$     2、.*(_D)$
* 不包含xx不以yy结尾：^((?!xx).)*(?<!yy)$
*/
namespace Framework.Core.Editor
{
    [CreateAssetMenu(menuName = "Create PresetDeliverer", fileName = "PresetDeliverer")]
    public class PresetDeliverer : ScriptableObject
    {
        [System.Serializable]
        public class ImportOption
        {
            public string filter;
            public bool enabled = true;
            public Preset preset;
        }
        public List<ImportOption> Presets = new List<ImportOption>();

        public void Apply(string directoryPath)
        {
            List<string> assetPaths = GetAllAssetPaths(directoryPath, 0);
            foreach(var p in Presets)
            {
                if(p.preset == null || !p.preset.IsValid())
                {
                    Debug.LogWarning($"Preset为空或无效，请检查");
                    continue;
                }

                List<string> filteredAssets = FilterAssets(assetPaths, p.filter);
                foreach(var assetPath in filteredAssets)
                {
                    AssetImporter importer = AssetImporter.GetAtPath(assetPath);
                    if(!p.preset.CanBeAppliedTo(importer))
                    {
                        Debug.LogWarning($"Preset[{p.preset.name}]不能应用于资源[{assetPath}]，请检查过滤器或资源命名");
                        continue;
                    }

                    // Debug.Log("begin apply to...");
                    bool success = p.preset.ApplyTo(importer);
                    if(success)
                    {
                        AssetDatabase.ImportAsset(assetPath);
                    }
                    // Debug.Log($"end apply.  {success}  {assetPath}");
                }
            }
        }

        static private List<string> FilterAssets(List<string> source, string filter)
        {            
            if(string.IsNullOrEmpty(filter))
            {
                return source;
            }

            Regex reg = new Regex(filter, RegexOptions.IgnoreCase);
            List<string> assets = new List<string>();
            foreach(var s in source)
            {
                string filenameWithoutExtension = Path.GetFileNameWithoutExtension(s);
                if(reg.IsMatch(filenameWithoutExtension))
                {
                    assets.Add(s);
                }
            }
            return assets;
        }

        // 筛选所有符合条件的资源列表（不统计meta、PresetDeliverer）
        static private List<string> GetAllAssetPaths(string folder, int depth, bool includeSubFolder = true)
        {
            List<string> assets = new List<string>();
            
            DirectoryInfo di = new DirectoryInfo(folder);
            FileInfo[] fis = di.GetFiles();
            foreach(var fi in fis)
            {
                string assetPath = FormatAssetPath(fi.FullName);

                // 忽略meta
                if(Path.GetExtension(assetPath) == ".meta")
                    continue;

                PresetDeliverer deliverer = AssetDatabase.LoadAssetAtPath<PresetDeliverer>(assetPath);
                if(depth != 0 && deliverer != null)
                {
                    // 不统计有PresetDeliverer的文件夹
                    return new List<string>();
                }

                if(deliverer == null)
                    assets.Add(assetPath);
            }

            if(includeSubFolder)
            {
                DirectoryInfo[] subDirectories = di.GetDirectories();
                foreach (var subDir in subDirectories)
                {
                    assets.AddRange(GetAllAssetPaths(subDir.FullName, depth + 1, includeSubFolder));
                }
            }
            
            return assets;
        }

        static private string FormatAssetPath(string assetPath)
        {
            string prefix = UnityEngine.Application.dataPath.Substring(0, UnityEngine.Application.dataPath.LastIndexOf("/"));
            return assetPath.Replace("\\", "/").Substring(prefix.Length + 1);
        }
    }

    [CustomEditor(typeof(PresetDeliverer))]
    public class PresetDelivererEditor : UnityEditor.Editor
    {
        private SerializedProperty m_Presets;

        public void OnEnable()
        {
            m_Presets = serializedObject.FindProperty("Presets");
        }

        public void OnDisable()
        {

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_Presets);
            if(EditorGUI.EndChangeCheck())
            {
                UnityEditor.EditorUtility.SetDirty(target);
            }

            if(GUILayout.Button("Apply"))
            {
                string filePath = AssetDatabase.GetAssetPath(target);
                ((PresetDeliverer)target).Apply(filePath.Substring(0, filePath.LastIndexOf("/")));
                AssetDatabase.SaveAssetIfDirty(target);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}