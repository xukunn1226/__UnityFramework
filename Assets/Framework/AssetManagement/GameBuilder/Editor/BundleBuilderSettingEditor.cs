﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Core;

namespace Framework.GameBuilder
{
    [CustomEditor(typeof(BundleBuilderSetting))]
    public class BundleBuilderSettingEditor : Editor
    {
        SerializedProperty      m_outputPathProp;
        SerializedProperty      m_useLZ4CompressProp;
        SerializedProperty      m_rebuildBundlesProp;
        SerializedProperty      m_appendHashProp;
        SerializedProperty      m_strictModeProp;
        
        private void Awake()
        {
            m_outputPathProp        = serializedObject.FindProperty("outputPath");
            m_useLZ4CompressProp    = serializedObject.FindProperty("useLZ4Compress");
            m_rebuildBundlesProp    = serializedObject.FindProperty("rebuildBundles");
            m_appendHashProp        = serializedObject.FindProperty("appendHash");
            m_strictModeProp        = serializedObject.FindProperty("strictMode");
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
            bool needBuild = false;
            GUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Output:", EditorStyles.largeLabel);
                if (GUILayout.Button(m_outputPathProp.stringValue))
                {
                    string newPath = EditorUtility.OpenFolderPanel("", m_outputPathProp.stringValue, "");
                    if (!string.IsNullOrEmpty(newPath))
                    {
                        m_outputPathProp.stringValue = Utility.GetRelativeProjectPath(newPath);
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Label("Actual Output:", EditorStyles.largeLabel);
                GUILayout.Button(m_outputPathProp.stringValue + "/" + Utility.GetPlatformName());
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
                DrawBuildAssetBundleOptions();

                EditorGUILayout.Space();
                if (GUILayout.Button("Build Bundles", new GUIStyle("LargeButtonMid")))
                {
                    needBuild = true;
                }
            }
            GUILayout.EndVertical();

            if (needBuild)
            {
                BundleBuilder.BuildAssetBundles(target as BundleBuilderSetting);
            }
        }

        private void DrawBuildAssetBundleOptions()
        {
            EditorGUILayout.LabelField("Build AssetBundle Options", EditorStyles.largeLabel);
            GUILayout.BeginVertical(new GUIStyle("HelpBox"));
            {
                m_useLZ4CompressProp.boolValue  = EditorGUILayout.Toggle(new GUIContent("UseLZ4Compress"), m_useLZ4CompressProp.boolValue);
                m_rebuildBundlesProp.boolValue  = EditorGUILayout.Toggle(new GUIContent("RebuildBundels"), m_rebuildBundlesProp.boolValue);
                m_appendHashProp.boolValue      = EditorGUILayout.Toggle(new GUIContent("AppendHash"), m_appendHashProp.boolValue);
                m_strictModeProp.boolValue      = EditorGUILayout.Toggle(new GUIContent("StrictMode"), m_strictModeProp.boolValue);
            }
            GUILayout.EndVertical();
        }
    }
}
