using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Framework.Core;
using System.Linq;

namespace Framework.AssetManagement.AssetEditorWindow
{
    [CustomEditor(typeof(BundleBuilderSetting))]
    public class BundleBuilderSettingEditor : Editor
    {
        SerializedProperty      m_outputPathProp;
        SerializedProperty      m_useLZ4CompressProp;
        SerializedProperty      m_rebuildBundlesProp;
        SerializedProperty      m_appendHashProp;
        SerializedProperty      m_DisableWriteTypeTreeProp;
        SerializedProperty      m_DevelopmentBuildProp;
        SerializedProperty      m_OverrideResourcePathProp;
        SerializedProperty      m_bundleCollectorConfigNameProp;

        private void Awake()
        {
            m_outputPathProp            = serializedObject.FindProperty("outputPath");
            m_useLZ4CompressProp        = serializedObject.FindProperty("useLZ4Compress");
            m_rebuildBundlesProp        = serializedObject.FindProperty("rebuildBundles");
            m_appendHashProp            = serializedObject.FindProperty("appendHash");
            m_DisableWriteTypeTreeProp  = serializedObject.FindProperty("DisableWriteTypeTree");
            m_DevelopmentBuildProp      = serializedObject.FindProperty("DevelopmentBuild");
            m_OverrideResourcePathProp  = serializedObject.FindProperty("OverrideResourcePath");
            m_bundleCollectorConfigNameProp = serializedObject.FindProperty("bundleCollectorConfigName");
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            serializedObject.Update();
            
            // draw HEADER
            GUIStyle boldStyle = EditorStyles.boldLabel;
            boldStyle.alignment = TextAnchor.MiddleLeft;
            EditorGUILayout.LabelField("Bundle Builder Setting  (" + Utility.GetPlatformName() + ")", boldStyle);

            // draw BODY
            DrawBundleBuilderSetting();

            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck())
            {
                AssetDatabase.SaveAssets();
            }
        }

        private void DrawBundleBuilderSetting()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Output:", EditorStyles.largeLabel);
                if (GUILayout.Button(m_outputPathProp.stringValue))
                {
                    string newPath = EditorUtility.OpenFolderPanel("", m_outputPathProp.stringValue, "");
                    if (!string.IsNullOrEmpty(newPath))
                    {
                        m_outputPathProp.stringValue = Utility.GetProjectPath(newPath);
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Label("Actual Output:", EditorStyles.largeLabel);
                GUILayout.Button(m_outputPathProp.stringValue);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();

                // 绘制资源收集器配置
                DrawCollectorConfigs();

                EditorGUILayout.Space();
                DrawBuildAssetBundleOptions();
            }
            GUILayout.EndVertical();
        }

        private void DrawCollectorConfigs()
        {
            List<AssetBundleCollectorConfig> list = AssetBundleCollectorSettingData.Instance.Configs;
            int index = list.FindIndex(item => { return item.ConfigName == m_bundleCollectorConfigNameProp.stringValue; });
            string[] displayNames = list.Select(item => item.ConfigName).ToArray();
            index = EditorGUILayout.Popup("资源收集器", Mathf.Max(0, index), displayNames);
            m_bundleCollectorConfigNameProp.stringValue = list[index].ConfigName;
        }

        private void DrawBuildAssetBundleOptions()
        {
            EditorGUILayout.LabelField("Build AssetBundle Options", EditorStyles.largeLabel);
            GUILayout.BeginVertical(new GUIStyle("HelpBox"));
            {
                m_useLZ4CompressProp.boolValue          = EditorGUILayout.Toggle(new GUIContent("UseLZ4Compress"), m_useLZ4CompressProp.boolValue);
                m_rebuildBundlesProp.boolValue          = EditorGUILayout.Toggle(new GUIContent("RebuildBundles"), m_rebuildBundlesProp.boolValue);                
                m_appendHashProp.boolValue              = EditorGUILayout.Toggle(new GUIContent("AppendHash"), m_appendHashProp.boolValue);
                m_DisableWriteTypeTreeProp.boolValue    = EditorGUILayout.Toggle(new GUIContent("DisableWriteTypeTree"), m_DisableWriteTypeTreeProp.boolValue);
                m_DevelopmentBuildProp.boolValue        = EditorGUILayout.Toggle(new GUIContent("DevelopmentBuild"), m_DevelopmentBuildProp.boolValue);
                m_OverrideResourcePathProp.stringValue  = EditorGUILayout.TextField(new GUIContent("OverrideResourcePath", "非空表示仅此目录下的资源输出，留空表示所有设置了bundle name的资源都将输出"), m_OverrideResourcePathProp.stringValue);
            }
            GUILayout.EndVertical();
        }
    }
}
