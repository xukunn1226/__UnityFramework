using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;


namespace Framework.AssetManagement.AssetBuilder
{
    public class AssetBuilderSetting : ScriptableObject
    {
#pragma warning disable CS0414
        static private string s_SavedPath = "Assets/Framework/AssetManagement/AssetBuilder";
#pragma warning restore CS0414

        public string[]         WhiteListOfPath         = new string[] { "Assets/Res/" };       // 路径白名单

        public string[]         BlackListOfFolder       = new string[] { "Resources", "Scripts", "RawData", "Editor", "StreamingAssets", "Examples", "Temp" };   // 文件夹黑名单

        public string[]         Extension               = new string[] { ".meta", ".cs"};

        public enum PackType
        {
            Pack_ByFolder,          // 文件夹内资源打成bundle，仅作用于当前文件夹，不包括子文件夹
            Pack_ByAllFolder,       // 文件夹及所有子文件夹的资源打成bundle
            Pack_ByFile,            // 每个文件打成单独bundle，仅作用于当前文件夹，不包括子文件夹
            Pack_BySize,            // 收集一组固定大小的资源，打成bundle，仅作用于当前文件夹，不包括子文件夹
        }
        public string[]         PackByTopFolder_Paths   = new string[] { };
        public string[]         PackByFile_Paths        = new string[] { };
        public string[]         PackBySize_Paths        = new string[] { };

        public string[]         ExtraPackPaths          = new string[] { };

        public string WhichPackage(string assetPath)
        {
            assetPath = assetPath.Substring(0, assetPath.LastIndexOf("/")).ToLower();
            if(ExtraPackPaths.Count(path => (path.ToLower().StartsWith(assetPath))) > 0)
            {
                return "extra";
            }
            return "base";
        }

        // 根据路径判断打包策略
        public PackType GetPackType(string assetPath, out string packPath)
        {
            string directory = assetPath.Substring(0, assetPath.LastIndexOf("/")) + "/";

            foreach(var path in PackByFile_Paths)
            {
                if(string.Compare(path.TrimEnd(new char[] { '/' }) + "/", directory, true) == 0)
                {
                    packPath = path;
                    return PackType.Pack_ByFile;
                }
            }

            foreach(var path in PackBySize_Paths)
            {
                if (string.Compare(path.TrimEnd(new char[] { '/' }) + "/", directory, true) == 0)
                {
                    packPath = path;
                    return PackType.Pack_BySize;
                }
            }

            foreach(var path in PackByTopFolder_Paths)
            {
                if(directory.StartsWith(path.TrimEnd(new char[] { '/' }) + "/", true, System.Globalization.CultureInfo.CurrentCulture))
                {
                    packPath = path;
                    return PackType.Pack_ByAllFolder;
                }
            }

            //if (PackByFile_Paths.Count(s => string.Compare(s.TrimEnd(new char[] { '/' }) + "/", directory, true) == 0) != 0)
            //    return PackType.Pack_ByFile;

            //if (PackBySize_Paths.Count(s => string.Compare(s.TrimEnd(new char[] { '/' }) + "/", directory, true) == 0) != 0)
            //    return PackType.Pack_BySize;

            //if (PackByTopFolder_Paths.Count(s => directory.StartsWith(s.TrimEnd(new char[] { '/' }) + "/", true, System.Globalization.CultureInfo.CurrentCulture)) != 0)
            //    return PackType.Pack_ByTopFolder;

            packPath = directory;
            return PackType.Pack_ByFolder;
        }

        /// <summary>
        /// 检查路径的有效性
        /// 不能同时被PackByFile_Paths、PackBySize_Paths使用
        /// </summary>
        /// <returns></returns>
        public string CheckNewPathsValid_PackByFile(string newPath)
        {
            string info = CheckPathsValid(newPath);
            if (!string.IsNullOrEmpty(info))
                return info;

            newPath = newPath.TrimEnd(new char[] { '/' }) + "/";

            // 判断是否被其他pack paths包含
            foreach(var checkingPath in PackBySize_Paths)
            {
                string path = checkingPath.TrimEnd(new char[] { '/' }) + "/";
                if (string.Compare(newPath, path, true) == 0)
                    return string.Format($"{newPath} has already exists in PackBySize_Paths({path})");
            }
            foreach (var checkingPath in PackByTopFolder_Paths)
            {
                string path = checkingPath.TrimEnd(new char[] { '/' }) + "/";
                if (path.StartsWith(newPath, true, System.Globalization.CultureInfo.CurrentCulture))
                    return string.Format($"{newPath} has already exists in PackByTopFolder_Paths({path})");
            }
            return null;
        }

        public string CheckNewPathsValid_PackBySize(string newPath)
        {
            string info = CheckPathsValid(newPath);
            if (!string.IsNullOrEmpty(info))
                return info;

            newPath = newPath.TrimEnd(new char[] { '/' }) + "/";

            // 判断是否被其他pack paths包含
            foreach (var checkingPath in PackByFile_Paths)
            {
                string path = checkingPath.TrimEnd(new char[] { '/' }) + "/";
                if (string.Compare(newPath, path, true) == 0)
                    return string.Format($"{newPath} has already exists in PackByFile_Paths({path})");
            }
            foreach (var checkingPath in PackByTopFolder_Paths)
            {
                string path = checkingPath.TrimEnd(new char[] { '/' }) + "/";
                if (path.StartsWith(newPath, true, System.Globalization.CultureInfo.CurrentCulture))
                    return string.Format($"{newPath} has already exists in PackByTopFolder_Paths({path})");
            }
            return null;
        }

        public string CheckNewPathsValid_PackByTopFolder(string newPath)
        {
            string info = CheckPathsValid(newPath);
            if (!string.IsNullOrEmpty(info))
                return info;

            newPath = newPath.TrimEnd(new char[] { '/' }) + "/";

            // newPath是否被TopFolderPaths的其他路径包含
            foreach(var checkingPath in PackByTopFolder_Paths)
            {
                string path = checkingPath.TrimEnd(new char[] { '/' }) + "/";
                if (string.Compare(newPath, path, true) == 0)
                    continue;   // 相同路径则忽略

                if (path.StartsWith(newPath, true, System.Globalization.CultureInfo.CurrentCulture))
                    return string.Format($"{newPath} has contains in PackByTopFolder_Paths({path})");
            }

            // 判断是否被其他pack paths包含
            foreach (var checkingPath in PackByFile_Paths)
            {
                string path = checkingPath.TrimEnd(new char[] { '/' }) + "/";
                if (string.Compare(newPath, path, true) == 0)
                    return string.Format($"{newPath} has already exists in PackByFile_Paths({path})");
            }
            foreach (var checkingPath in PackBySize_Paths)
            {
                string path = checkingPath.TrimEnd(new char[] { '/' }) + "/";
                if (string.Compare(newPath, path, true) == 0)
                    return string.Format($"{newPath} has already exists in PackBySize_Paths({path})");
            }
            return null;
        }

        public string CheckPathsValid(string newPath)
        {
            if (!AssetDatabase.IsValidFolder(newPath))
                return string.Format($"{newPath} is not valid folder");

            if (!AssetBuilderUtil.IsPassByWhiteList(newPath))
                return string.Format($"{newPath} is not pass by white list");

            if (AssetBuilderUtil.IsBlockedByBlackList(newPath))
                return string.Format($"{newPath} is blocked by black list");
            return null;
        }

        //public string[] UpdatePackByFolderPaths()
        //{
        //    List<string> paths = new List<string>();
        //    foreach (var folder in WhiteListOfPath)
        //    {
        //        string[] directories = Directory.GetDirectories(folder, "*.*", SearchOption.AllDirectories);
        //        IEnumerable<string> ret = directories.Select(p => (p.Replace("\\", "/")));
        //        foreach(var path in ret)
        //        {
        //            if (!string.IsNullOrEmpty(CheckPathsValid(path)))
        //                continue;

        //            if (PackByFile_Paths.Count(s => string.Compare(s.TrimEnd(new char[] { '/' }), path, true) == 0) != 0)
        //                continue;

        //            if (PackBySize_Paths.Count(s => string.Compare(s.TrimEnd(new char[] { '/' }), path, true) == 0) != 0)
        //                continue;

        //            if (PackByTopFolder_Paths.Count(s => (path + "/").StartsWith(s.TrimEnd(new char[] { '/' }) + "/", true, System.Globalization.CultureInfo.CurrentCulture)) != 0)
        //                continue;

        //            paths.Add(path);
        //        }
        //    }
        //    PackByFolder_Paths = paths.ToArray();
        //    return PackByFolder_Paths;
        //}

#if UNITY_EDITOR
        [MenuItem("Tools/Assets Management/Create AssetBuilder Setting", false, 1)]
        static private void CreateSetting()
        {
            AssetBuilderSetting asset = GetDefault();
            if (asset != null)
                Selection.activeObject = asset;
        }

        static public AssetBuilderSetting GetDefault()
        {
            return Core.Editor.EditorUtility.GetOrCreateEditorConfigObject<AssetBuilderSetting>(s_SavedPath);
        }
#endif
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(AssetBuilderSetting))]
    public class AssetBuilderSettingEditor : Editor
    {
        SerializedProperty m_WhiteListOfPathProp;
        SerializedProperty m_BlackListOfPathProp;
        SerializedProperty m_ExtensionProp;
        //SerializedProperty m_PackByFolder_PathsProp;
        SerializedProperty m_PackByTopFolder_PathsProp;
        SerializedProperty m_PackByFile_PathsProp;
        SerializedProperty m_PackBySize_PathsProp;
        SerializedProperty m_ExtraPackPathsProp;

        private void OnEnable()
        {
            m_WhiteListOfPathProp = serializedObject.FindProperty("WhiteListOfPath");
            m_BlackListOfPathProp = serializedObject.FindProperty("BlackListOfFolder");
            m_ExtensionProp = serializedObject.FindProperty("Extension");
            //m_PackByFolder_PathsProp = serializedObject.FindProperty("PackByFolder_Paths");
            m_PackByTopFolder_PathsProp = serializedObject.FindProperty("PackByTopFolder_Paths");
            m_PackByFile_PathsProp = serializedObject.FindProperty("PackByFile_Paths");
            m_PackBySize_PathsProp = serializedObject.FindProperty("PackBySize_Paths");
            m_ExtraPackPathsProp = serializedObject.FindProperty("ExtraPackPaths");
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            serializedObject.Update();

            AssetBuilderSetting setting = AssetBuilderSetting.GetDefault();
            string error = null;

            GUIStyle newStyle = EditorStyles.boldLabel;
            newStyle.alignment = TextAnchor.MiddleLeft;
            EditorGUILayout.LabelField("Asset Builder Setting", newStyle);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.Space();
                EditorGUILayout.Separator();
                EditorGUILayout.Separator();

                EditorGUILayout.BeginHorizontal();
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(m_WhiteListOfPathProp, new GUIContent("路径白名单"), true);
                --EditorGUI.indentLevel;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(m_BlackListOfPathProp, new GUIContent("文件夹黑名单"), true);
                --EditorGUI.indentLevel;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(m_ExtensionProp, new GUIContent("无效扩展名"), true);
                --EditorGUI.indentLevel;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.HelpBox("在路径白名单之下，且不在文件夹黑名单之内的资源将自动设置AssetBundle Name", MessageType.Info);

                EditorGUILayout.Separator();
                EditorGUILayout.Separator();



                EditorGUILayout.BeginHorizontal();
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(m_PackByTopFolder_PathsProp, new GUIContent("Pack By Top Folder: 以下文件夹及其子文件夹内所有资源将打成一个bundle"), true);
                for(int i = 0; i < m_PackByTopFolder_PathsProp.arraySize; ++i)
                {
                    string err = setting.CheckNewPathsValid_PackByTopFolder(m_PackByTopFolder_PathsProp.GetArrayElementAtIndex(i).stringValue);
                    if(!string.IsNullOrEmpty(err))
                    {
                        error = err;
                        break;
                    }
                }
                --EditorGUI.indentLevel;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(m_PackByFile_PathsProp, new GUIContent("Pack By File: 文件夹中每个资源单独打一个bundle"), true);
                for (int i = 0; i < m_PackByFile_PathsProp.arraySize; ++i)
                {
                    string err = setting.CheckNewPathsValid_PackByFile(m_PackByFile_PathsProp.GetArrayElementAtIndex(i).stringValue);
                    if (!string.IsNullOrEmpty(err))
                    {
                        error = err;
                        break;
                    }
                }
                --EditorGUI.indentLevel;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(m_PackBySize_PathsProp, new GUIContent("Pack By Size: 文件夹中的资源按固定大小自动打bundle"), true);
                for (int i = 0; i < m_PackBySize_PathsProp.arraySize; ++i)
                {
                    string err = setting.CheckNewPathsValid_PackBySize(m_PackBySize_PathsProp.GetArrayElementAtIndex(i).stringValue);
                    if (!string.IsNullOrEmpty(err))
                    {
                        error = err;
                        break;
                    }
                }
                --EditorGUI.indentLevel;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(m_ExtraPackPathsProp, new GUIContent("Extra pack paths: 二次下载资源路径"), true);
                
                --EditorGUI.indentLevel;
                EditorGUILayout.EndHorizontal();


                if (!string.IsNullOrEmpty(error))
                {
                    Color clr = GUI.color;
                    GUI.color = Color.red;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent(EditorGUIUtility.FindTexture("console.infoicon")), EditorStyles.label, GUILayout.Width(20));
                    EditorGUILayout.LabelField(error);
                    EditorGUILayout.EndHorizontal();
                    GUI.color = clr;
                }

                //EditorGUI.BeginDisabledGroup(true);
                //EditorGUILayout.BeginHorizontal();
                //++EditorGUI.indentLevel;
                //EditorGUILayout.PropertyField(m_PackByFolder_PathsProp, new GUIContent("Pack By Folder: 文件夹中的所有资源打一个bundle"), true);
                //--EditorGUI.indentLevel;
                //EditorGUILayout.EndHorizontal();
                //EditorGUI.EndDisabledGroup();                
            }
            GUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();

            if(EditorGUI.EndChangeCheck())
            {
                //setting.UpdatePackByFolderPaths();
                AssetDatabase.SaveAssets();
            }
        }
    }
#endif
}