using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AssetManagement.AssetBuilder
{
    public class AssetBuilderSetting : ScriptableObject
    {
#pragma warning disable CS0414
        static private string m_kSavedPath = "Assets/AssetManagement/AssetBuilder";
#pragma warning restore CS0414

        public bool             ForceDisplayDialogWhenAssetNameNotMetSpec;                  // 资源名称不符合规范是否强制提示
        
        public string[]         WhiteListOfPath         = new string[] { "Assets/Res/" };   // 路径白名单（ab name生成规则）

        public string[]         BlackListOfFolder       = new string[] { "Resources", "Scenes", "Scripts", "RawData", "Editor", "StreamingAssets", "Examples", "Temp" };   // 文件夹黑名单

#if UNITY_EDITOR
        [MenuItem("Assets Management/Create AssetBuilder Setting", false, 1)]
        static private void CreateSetting()
        {
            AssetBuilderSetting asset = GetDefault();
            if (asset != null)
                Selection.activeObject = asset;
        }

        static public AssetBuilderSetting GetDefault()
        {
            return AssetManagement.Utility.GetOrCreateEditorConfigObject<AssetBuilderSetting>(m_kSavedPath);
        }
#endif
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(AssetBuilderSetting))]
    public class AssetBuilderSettingEditor : Editor
    {
        SerializedProperty m_ForceDisplayDialogWhenAssetNameNotMetSpecProp;
        SerializedProperty m_WhiteListOfPathProp;
        SerializedProperty m_BlackListOfPathProp;

        private void OnEnable()
        {
            m_ForceDisplayDialogWhenAssetNameNotMetSpecProp = serializedObject.FindProperty("ForceDisplayDialogWhenAssetNameNotMetSpec");
            m_WhiteListOfPathProp = serializedObject.FindProperty("WhiteListOfPath");
            m_BlackListOfPathProp = serializedObject.FindProperty("BlackListOfFolder");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUIStyle newStyle = EditorStyles.boldLabel;
            newStyle.alignment = TextAnchor.MiddleLeft;
            EditorGUILayout.LabelField("Asset Builder Setting", newStyle);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            {
                m_ForceDisplayDialogWhenAssetNameNotMetSpecProp.boolValue = EditorGUILayout.ToggleLeft(new GUIContent("Force Display Dialog", ""), m_ForceDisplayDialogWhenAssetNameNotMetSpecProp.boolValue);

                EditorGUILayout.HelpBox("当资源命名不规则时强制对话框提示", MessageType.Info);

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

                EditorGUILayout.HelpBox("在路径白名单之下，且不在文件夹黑名单之内的资源将自动设置AssetBundle Name", MessageType.Info);
            }
            GUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}