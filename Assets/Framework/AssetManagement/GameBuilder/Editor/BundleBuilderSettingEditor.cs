using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Framework.Core;
using System.Linq;
using Framework.AssetManagement.Runtime;

namespace Framework.AssetManagement.AssetEditorWindow
{
    [CustomEditor(typeof(BundleBuilderSetting))]
    public class BundleBuilderSettingEditor : Editor
    {
        SerializedProperty      m_useLZ4CompressProp;
        SerializedProperty      m_rebuildBundlesProp;
        SerializedProperty      m_appendHashProp;
        SerializedProperty      m_disableWriteTypeTreeProp;
        SerializedProperty      m_bundleCollectorConfigNameProp;
        SerializedProperty      m_nameStyleProp;

        private void Awake()
        {
            m_useLZ4CompressProp            = serializedObject.FindProperty("useLZ4Compress");
            m_rebuildBundlesProp            = serializedObject.FindProperty("rebuildBundles");
            m_appendHashProp                = serializedObject.FindProperty("appendHash");
            m_disableWriteTypeTreeProp      = serializedObject.FindProperty("disableWriteTypeTree");
            m_bundleCollectorConfigNameProp = serializedObject.FindProperty("bundleCollectorConfigName");
            m_nameStyleProp                 = serializedObject.FindProperty("nameStyle");
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
                m_disableWriteTypeTreeProp.boolValue    = EditorGUILayout.Toggle(new GUIContent("DisableWriteTypeTree"), m_disableWriteTypeTreeProp.boolValue);
                // m_nameStyleProp.intValue                = (int)(EOutputNameStyle)EditorGUILayout.EnumPopup("资源包名称样式", (EOutputNameStyle)m_nameStyleProp.intValue);
            }
            GUILayout.EndVertical();
        }
    }
}
