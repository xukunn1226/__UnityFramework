﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Core;

namespace AssetManagement.GameBuilder
{
    [CustomEditor(typeof(PlayerBuilderSetting))]
    public class PlayerBuilderSettingEditor : Editor
    {
        SerializedProperty              m_outputPathProp;
        SerializedProperty              m_projectNameProp;
        SerializedProperty              m_autoRunPlayerProp;
        SerializedProperty              m_developmentProp;
        SerializedProperty              m_connectWithProfilerProp;
        SerializedProperty              m_allowDebuggingProp;
        SerializedProperty              m_buildScriptsOnlyProp;
        SerializedProperty              m_compressWithLz4Prop;
        SerializedProperty              m_compressWithLz4HCProp;
        SerializedProperty              m_strictModeProp;

        SerializedProperty              m_bundleVersionProp;
        SerializedProperty              m_useIL2CPPProp;
        SerializedProperty              m_useMTRenderingProp;
        SerializedProperty              m_useAPKExpansionFilesProp;
        SerializedProperty              m_macroDefinesProp;

        SerializedProperty              m_bOverrideBuildScenesProp;
        SerializedProperty              m_overrideBuildScenesProp;

        private void Awake()
        {
            m_outputPathProp            = serializedObject.FindProperty("outputPath");
            m_projectNameProp           = serializedObject.FindProperty("projectName");
            m_autoRunPlayerProp         = serializedObject.FindProperty("autoRunPlayer");
            m_developmentProp           = serializedObject.FindProperty("development");
            m_connectWithProfilerProp   = serializedObject.FindProperty("connectWithProfiler");
            m_allowDebuggingProp        = serializedObject.FindProperty("allowDebugging");
            m_buildScriptsOnlyProp      = serializedObject.FindProperty("buildScriptsOnly");
            m_compressWithLz4Prop       = serializedObject.FindProperty("compressWithLz4");
            m_compressWithLz4HCProp     = serializedObject.FindProperty("compressWithLz4HC");
            m_strictModeProp            = serializedObject.FindProperty("strictMode");

            m_bundleVersionProp         = serializedObject.FindProperty("bundleVersion");
            m_useIL2CPPProp             = serializedObject.FindProperty("useIL2CPP");
            m_useMTRenderingProp        = serializedObject.FindProperty("useMTRendering");
            m_useAPKExpansionFilesProp  = serializedObject.FindProperty("useAPKExpansionFiles");
            m_macroDefinesProp          = serializedObject.FindProperty("macroDefines");

            m_bOverrideBuildScenesProp  = serializedObject.FindProperty("bOverrideBuildScenes");
            m_overrideBuildScenesProp   = serializedObject.FindProperty("overrideBuildScenes");
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            serializedObject.Update();

            // draw HEADER
            GUIStyle boldStyle = EditorStyles.boldLabel;
            boldStyle.alignment = TextAnchor.MiddleLeft;
            EditorGUILayout.LabelField("Player Builder Setting  (" + Utility.GetPlatformName() + ")", boldStyle);

            // draw BODY
            DrawPlayerBuilderSetting();

            serializedObject.ApplyModifiedProperties();

            if(EditorGUI.EndChangeCheck())
            {
                AssetDatabase.SaveAssets();
            }
        }

        private void DrawPlayerBuilderSetting()
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

                string projectName = EditorGUILayout.TextField("ProjectName", m_projectNameProp.stringValue);
                projectName = projectName.Trim(new char[] { '/', '\\', ' ', '\'', '.', '*'});
                m_projectNameProp.stringValue = projectName;

                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Label("Full Output:", EditorStyles.largeLabel);
                GUILayout.Button(((PlayerBuilderSetting)target).GetLocalPathName());
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
                DrawBuildOptions();

                EditorGUILayout.Space();
                DrawPlayerSettings();

                EditorGUILayout.Space();
                DrawBuildScenes();

                EditorGUILayout.Space();
                if (GUILayout.Button("Build Player", new GUIStyle("LargeButtonMid")))
                {
                    needBuild = true;
                }
            }
            GUILayout.EndVertical();

            if(needBuild)
            {
                PlayerBuilder.BuildPlayer(target as PlayerBuilderSetting);
            }
        }

        private void DrawBuildOptions()
        {
            EditorGUILayout.LabelField("BuildOptions", EditorStyles.largeLabel);
            GUILayout.BeginVertical(new GUIStyle("HelpBox"));
            {
                m_autoRunPlayerProp.boolValue = EditorGUILayout.Toggle(new GUIContent("AutoRunPlayer"), m_autoRunPlayerProp.boolValue);
                m_developmentProp.boolValue = EditorGUILayout.Toggle(new GUIContent("Development"), m_developmentProp.boolValue);
                if (!m_developmentProp.boolValue)
                    EditorGUI.BeginDisabledGroup(true);
                m_connectWithProfilerProp.boolValue = EditorGUILayout.Toggle(new GUIContent("ConnectWithProfiler"), m_connectWithProfilerProp.boolValue);
                m_allowDebuggingProp.boolValue = EditorGUILayout.Toggle(new GUIContent("AllowDebugging"), m_allowDebuggingProp.boolValue);
                m_buildScriptsOnlyProp.boolValue = EditorGUILayout.Toggle(new GUIContent("BuildScriptsOnly"), m_buildScriptsOnlyProp.boolValue);
                if (!m_developmentProp.boolValue)
                    EditorGUI.EndDisabledGroup();

                if (m_compressWithLz4HCProp.boolValue)
                    EditorGUI.BeginDisabledGroup(true);
                m_compressWithLz4Prop.boolValue = EditorGUILayout.Toggle(new GUIContent("CompressWithLz4"), m_compressWithLz4Prop.boolValue);
                if (m_compressWithLz4HCProp.boolValue)
                    EditorGUI.EndDisabledGroup();
                m_compressWithLz4HCProp.boolValue = EditorGUILayout.Toggle(new GUIContent("CompressWithLz4HC"), m_compressWithLz4HCProp.boolValue);
                m_strictModeProp.boolValue = EditorGUILayout.Toggle(new GUIContent("StrictMode"), m_strictModeProp.boolValue);
            }
            GUILayout.EndVertical();
        }

        private void DrawBuildScenes()
        {
            EditorGUILayout.LabelField("Scenes In Build", EditorStyles.largeLabel);

            m_bOverrideBuildScenesProp.boolValue = EditorGUILayout.Toggle(new GUIContent("Override Build Scenes"), m_bOverrideBuildScenesProp.boolValue, EditorStyles.toggle);

            EditorGUI.BeginDisabledGroup(!m_bOverrideBuildScenesProp.boolValue);
            EditorGUILayout.PropertyField(m_overrideBuildScenesProp);
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(m_bOverrideBuildScenesProp.boolValue);
            GUILayout.BeginVertical(new GUIStyle("HelpBox"));
            {
                for (int i = 0; i < EditorBuildSettings.scenes.Length; ++i)
                {
                    EditorBuildSettingsScene scene = EditorBuildSettings.scenes[i];

                    EditorGUI.BeginChangeCheck();

                    bool bExist = AssetDatabase.LoadAssetAtPath<SceneAsset>(AssetDatabase.GUIDToAssetPath(scene.guid.ToString())) != null;
                    EditorGUI.BeginDisabledGroup(!bExist);
                    EditorGUILayout.ToggleLeft(new GUIContent(scene.path), scene.enabled);
                    EditorGUI.EndDisabledGroup();

                    if(EditorGUI.EndChangeCheck())
                    {
                        List<EditorBuildSettingsScene> newBuildSettingsScenes = CopyEditorBuildSettingsScene();
                        newBuildSettingsScenes[i].enabled = !newBuildSettingsScenes[i].enabled;
                        EditorBuildSettings.scenes = newBuildSettingsScenes.ToArray();
                        break;
                    }
                }
            }
            GUILayout.EndVertical();
            EditorGUI.EndDisabledGroup();
        }

        static private List<EditorBuildSettingsScene> CopyEditorBuildSettingsScene()
        {
            List<EditorBuildSettingsScene> newScenes = new List<EditorBuildSettingsScene>();
            foreach(var scene in EditorBuildSettings.scenes)
            {
                newScenes.Add(new EditorBuildSettingsScene(scene.guid, scene.enabled));
            }
            return newScenes;
        }

        private void DrawPlayerSettings()
        {
            EditorGUILayout.LabelField("PlayerSettings", EditorStyles.largeLabel);
            GUILayout.BeginVertical(new GUIStyle("HelpBox"));
            {
                EditorGUILayout.LabelField("App Identifier", PlayerSettings.applicationIdentifier, GUILayout.Width(400));

                m_bundleVersionProp.stringValue = EditorGUILayout.TextField("Bundle Version", m_bundleVersionProp.stringValue);

                m_useIL2CPPProp.boolValue = EditorGUILayout.Toggle("UseIL2CPP", m_useIL2CPPProp.boolValue);

                EditorGUI.BeginDisabledGroup(!m_useIL2CPPProp.boolValue);
                ((PlayerBuilderSetting)target).il2CppCompilerConfiguration = (Il2CppCompilerConfiguration)EditorGUILayout.EnumPopup("CompilerConfiguration", ((PlayerBuilderSetting)target).il2CppCompilerConfiguration);
                EditorGUI.EndDisabledGroup();

                m_useMTRenderingProp.boolValue = EditorGUILayout.Toggle("UseMTRendering", m_useMTRenderingProp.boolValue);

                m_useAPKExpansionFilesProp.boolValue = EditorGUILayout.Toggle("UseAPKExpansionFiles", m_useAPKExpansionFilesProp.boolValue);

                EditorGUI.BeginChangeCheck();
                m_macroDefinesProp.stringValue = EditorGUILayout.DelayedTextField(new GUIContent("Macro Defines(;)"), m_macroDefinesProp.stringValue);
                if(EditorGUI.EndChangeCheck())
                {
                    m_macroDefinesProp.stringValue = m_macroDefinesProp.stringValue.TrimEnd(new char[] { ';' });
                }
            }
            GUILayout.EndVertical();
        }
    }
}