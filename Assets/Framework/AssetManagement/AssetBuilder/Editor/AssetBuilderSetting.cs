using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using Framework.Core;


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

        private void OnEnable()
        {
            m_WhiteListOfPathProp = serializedObject.FindProperty("WhiteListOfPath");
            m_BlackListOfPathProp = serializedObject.FindProperty("BlackListOfFolder");
            m_ExtensionProp = serializedObject.FindProperty("Extension");
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