using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;


namespace Framework.AssetManagement.AssetBuilder
{
    public class AssetBuilderSetting : ScriptableObject
    {
#pragma warning disable CS0414
        static private string s_SavedPath = "Assets/Framework/AssetManagement/AssetBuilder";
#pragma warning restore CS0414

        public string[]         WhiteListOfPath         = new string[] { "Assets/Res/" };       // 路径白名单（ab name生成规则）

        public string[]         BlackListOfFolder       = new string[] { "Resources", "Scenes", "Scripts", "RawData", "Editor", "StreamingAssets", "Examples", "Temp" };   // 文件夹黑名单

        public string[]         Extension               = new string[] { ".meta", ".cs"};
        public string[]         BundleNameWithParent    = new string[] { };

        public enum PackType
        {
            Pack_ByFolder,          // 文件夹内资源打成bundle，不包括子文件夹
            Pack_ByTopFolder,       // 文件夹及所有子文件夹的资源打成bundle
            Pack_ByFile,            // 每个文件打成单独bundle
            Pack_BySize,            // 收集一组固定大小的资源，打成bundle
        }
        public string[]         PackByFolder_Paths      = new string[] { };
        public string[]         PackByTopFolder_Paths   = new string[] { };
        public string[]         PackByFile_Paths        = new string[] { };
        public string[]         PackBySize_Paths        = new string[] { };

        public string CheckTopFolderPathsValid()
        {
            return null;
        }

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
        SerializedProperty m_BundleNameWithParentProp;
        SerializedProperty m_PackByFolder_PathsProp;
        SerializedProperty m_PackByTopFolder_PathsProp;
        SerializedProperty m_PackByFile_PathsProp;
        SerializedProperty m_PackBySize_PathsProp;

        private void OnEnable()
        {
            m_WhiteListOfPathProp = serializedObject.FindProperty("WhiteListOfPath");
            m_BlackListOfPathProp = serializedObject.FindProperty("BlackListOfFolder");
            m_ExtensionProp = serializedObject.FindProperty("Extension");
            m_BundleNameWithParentProp = serializedObject.FindProperty("BundleNameWithParent");
            m_PackByFolder_PathsProp = serializedObject.FindProperty("PackByFolder_Paths");
            m_PackByTopFolder_PathsProp = serializedObject.FindProperty("PackByTopFolder_Paths");
            m_PackByFile_PathsProp = serializedObject.FindProperty("PackByFile_Paths");
            m_PackBySize_PathsProp = serializedObject.FindProperty("PackBySize_Paths");
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            serializedObject.Update();

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
                EditorGUILayout.PropertyField(m_BundleNameWithParentProp, new GUIContent("以下文件夹将保持与父文件夹一致的BundleName"), true);
                --EditorGUI.indentLevel;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(m_PackByTopFolder_PathsProp, new GUIContent("Pack By Top Folder: 以下文件夹及其子文件夹内所有资源将打成一个bundle"), true);
                --EditorGUI.indentLevel;
                EditorGUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();

            if(EditorGUI.EndChangeCheck())
            {
                AssetDatabase.SaveAssets();
            }
        }
    }
#endif
}