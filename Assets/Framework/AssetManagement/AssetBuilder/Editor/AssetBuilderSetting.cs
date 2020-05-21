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
        static private string s_SavedPath = "Assets/AssetManagement/AssetBuilder";
#pragma warning restore CS0414

        public bool             ForceDisplayDialogWhenAssetNameNotMet;                          // 资源名称不符合规范是否强制提示
        
        public string[]         WhiteListOfPath         = new string[] { "Assets/Res/" };       // 路径白名单（ab name生成规则）

        public string[]         BlackListOfFolder       = new string[] { "Resources", "Scenes", "Scripts", "RawData", "Editor", "StreamingAssets", "Examples", "Temp" };   // 文件夹黑名单

        public string[]         BundleNameWithParent    = new string[] { };

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
            return Core.Editor.EditorUtility.GetOrCreateEditorConfigObject<AssetBuilderSetting>(s_SavedPath);
        }
#endif
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(AssetBuilderSetting))]
    public class AssetBuilderSettingEditor : Editor
    {
        SerializedProperty m_ForceDisplayDialogWhenAssetNameNotMetProp;
        SerializedProperty m_WhiteListOfPathProp;
        SerializedProperty m_BlackListOfPathProp;
        SerializedProperty m_BundleNameWithParentProp;

        private void OnEnable()
        {
            m_ForceDisplayDialogWhenAssetNameNotMetProp = serializedObject.FindProperty("ForceDisplayDialogWhenAssetNameNotMet");
            m_WhiteListOfPathProp = serializedObject.FindProperty("WhiteListOfPath");
            m_BlackListOfPathProp = serializedObject.FindProperty("BlackListOfFolder");
            m_BundleNameWithParentProp = serializedObject.FindProperty("BundleNameWithParent");
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
                m_ForceDisplayDialogWhenAssetNameNotMetProp.boolValue = EditorGUILayout.ToggleLeft(new GUIContent("Force Display Dialog", ""), m_ForceDisplayDialogWhenAssetNameNotMetProp.boolValue);

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

                EditorGUILayout.Separator();
                EditorGUILayout.Separator();

                EditorGUILayout.BeginHorizontal();
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(m_BundleNameWithParentProp, new GUIContent("以下文件夹将保持与父文件夹一致的BundleName"), true);
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