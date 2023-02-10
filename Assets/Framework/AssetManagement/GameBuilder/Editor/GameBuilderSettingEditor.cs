﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Framework.Core;

namespace Framework.AssetManagement.AssetEditorWindow
{
    [CustomEditor(typeof(GameBuilderSetting))]
    public class GameBuilderSettingEditor : Editor
    {
        SerializedProperty  m_bundleSettingProp;
        SerializedProperty  m_playerSettingProp;
        SerializedProperty  m_buildTargetProp;
        SerializedProperty  m_buildModeProp;
        SerializedProperty  m_backdoorProp;
        Backdoor            m_backdoor;

        Editor              m_bundleSettingEditor;
        Editor              m_playerSettingEditor;

        AppVersion          m_AppVersion;
        bool                m_isForceUpdate = true;
        int                 m_HotfixNumber = 1;

        private void Awake()
        {
            m_bundleSettingProp = serializedObject.FindProperty("bundleSetting");
            m_playerSettingProp = serializedObject.FindProperty("playerSetting");
            m_buildTargetProp   = serializedObject.FindProperty("buildTarget");
            m_buildModeProp     = serializedObject.FindProperty("buildMode");
            m_backdoorProp      = serializedObject.FindProperty("backdoor");
            m_backdoor          = ((JsonAsset)m_backdoorProp.objectReferenceValue)?.Require<Backdoor>(true);
            m_AppVersion        = AppVersion.EditorLoad();
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            serializedObject.Update();

            if (m_bundleSettingProp == null || m_playerSettingProp == null)
                return;

            m_bundleSettingProp.objectReferenceValue = EditorGUILayout.ObjectField( new GUIContent("Bundle Setting", ""), 
                                                                                    m_bundleSettingProp.objectReferenceValue, 
                                                                                    typeof(BundleBuilderSetting), 
                                                                                    false);

            m_playerSettingProp.objectReferenceValue = EditorGUILayout.ObjectField( new GUIContent("Player Setting", ""), 
                                                                                    m_playerSettingProp.objectReferenceValue, 
                                                                                    typeof(PlayerBuilderSetting), 
                                                                                    false);

            BuildTarget bt = (BuildTarget)m_buildTargetProp.enumValueFlag;
            bt = (BuildTarget)EditorGUILayout.EnumFlagsField("Build Target", bt);
            m_buildTargetProp.enumValueFlag = (int)bt;

            EditorGUILayout.Separator();
            DrawBundleSettingEditor();

            EditorGUILayout.Separator();
            DrawPlayerSettingEditor();

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            DrawBuildButton();

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("----------------------------------- Deploy -----------------------------------", EditorStyles.boldLabel);
            DrawDeploymentSetting();

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            DrawBackdoorSetting();
            
            serializedObject.ApplyModifiedProperties();

            if(EditorGUI.EndChangeCheck())
            {
                AssetDatabase.SaveAssets();
            }
        }

        private void DrawBundleSettingEditor()
        {
            // update bundle setting editor
            if (m_bundleSettingEditor != null && m_bundleSettingProp.objectReferenceValue == null)
            {
                DestroyImmediate(m_bundleSettingEditor);
                m_bundleSettingEditor = null;
            }
            else if (m_bundleSettingEditor == null && m_bundleSettingProp.objectReferenceValue != null)
            {
                m_bundleSettingEditor = Editor.CreateEditor(m_bundleSettingProp.objectReferenceValue);
            }
            else
            {
                if (m_bundleSettingEditor != null && m_bundleSettingProp.objectReferenceValue != null && m_bundleSettingEditor.target != m_bundleSettingProp.objectReferenceValue)
                {
                    DestroyImmediate(m_bundleSettingEditor);
                    m_bundleSettingEditor = Editor.CreateEditor(m_bundleSettingProp.objectReferenceValue);
                }
            }

            if (m_bundleSettingEditor != null)
            {
                m_bundleSettingEditor.OnInspectorGUI();
            }
            else
            {
                EditorGUILayout.HelpBox("Missing Bundle Builder Setting", MessageType.Error);
            }
        }

        private void DrawPlayerSettingEditor()
        {
            // update player setting editor
            if (m_playerSettingEditor != null && m_playerSettingProp.objectReferenceValue == null)
            {
                DestroyImmediate(m_playerSettingEditor);
                m_playerSettingEditor = null;
            }
            else if (m_playerSettingEditor == null && m_playerSettingProp.objectReferenceValue != null)
            {
                m_playerSettingEditor = Editor.CreateEditor(m_playerSettingProp.objectReferenceValue);
            }
            else
            {
                if (m_playerSettingEditor != null && m_playerSettingProp.objectReferenceValue != null && m_playerSettingEditor.target != m_playerSettingProp.objectReferenceValue)
                {
                    DestroyImmediate(m_playerSettingEditor);
                    m_playerSettingEditor = Editor.CreateEditor(m_playerSettingProp.objectReferenceValue);
                }
            }

            if (m_playerSettingEditor != null)
            {
                m_playerSettingEditor.OnInspectorGUI();
            }
            else
            {
                EditorGUILayout.HelpBox("Missing Player Builder Setting", MessageType.Error);
            }
        }

        private void DrawBuildButton()
        {
            EditorGUILayout.BeginHorizontal();
            {                
                m_buildModeProp.enumValueIndex = (int)(GameBuilderSetting.BuildMode)EditorGUILayout.EnumPopup("", (GameBuilderSetting.BuildMode)m_buildModeProp.enumValueIndex, GUILayout.Width(140));
                GameBuilderSetting.BuildMode buildMode = (GameBuilderSetting.BuildMode)m_buildModeProp.enumValueIndex;

                GUIStyle boldStyle = new GUIStyle("ButtonLeft");
                boldStyle.fontStyle = FontStyle.Bold;
                boldStyle.alignment = TextAnchor.MiddleCenter;

                bool disable = false;
                switch(buildMode)
                {
                    case GameBuilderSetting.BuildMode.Bundles:
                        disable = m_bundleSettingProp.objectReferenceValue == null;
                        break;
                    case GameBuilderSetting.BuildMode.Player:
                        disable = m_playerSettingProp.objectReferenceValue == null;
                        break;
                    case GameBuilderSetting.BuildMode.BundlesAndPlayer:
                        disable = m_bundleSettingProp.objectReferenceValue == null || m_playerSettingProp.objectReferenceValue == null;
                        break;
                }
                EditorGUI.BeginDisabledGroup(disable);
                if(GUILayout.Button("Build Game", boldStyle))
                {
                    GameBuilder.BuildGame((GameBuilderSetting)target);                    
                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawDeploymentSetting()
        {
            m_isForceUpdate = EditorGUILayout.Toggle("强更版本", m_isForceUpdate);

            EditorGUILayout.LabelField("AppVersion");
            GUILayout.BeginVertical(new GUIStyle("HelpBox"));
            EditorGUI.BeginDisabledGroup(true);
            int mainVersion = EditorGUILayout.IntField("MainVersion", m_AppVersion.MainVersion, GUILayout.Width(400));
            int minorVersion = EditorGUILayout.IntField("MinorVersion", m_AppVersion.MinorVersion, GUILayout.Width(400));
            int revision = EditorGUILayout.IntField("Revision", m_AppVersion.Revision, GUILayout.Width(400));
            EditorGUILayout.IntField("Build Number", m_AppVersion.BuildNumber, GUILayout.Width(400));
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(m_isForceUpdate);
                m_HotfixNumber = Mathf.Max(1, EditorGUILayout.IntField("HotfixNumber", m_HotfixNumber, GUILayout.Width(400)));
            EditorGUI.EndDisabledGroup();
            GUILayout.EndVertical();

            string deployVersion = string.Format($"{mainVersion}.{minorVersion}.{revision}");
            if(!m_isForceUpdate)
                deployVersion = string.Format($"{deployVersion}.{m_HotfixNumber}");

            string error = AppVersion.Check(deployVersion);
            if(!string.IsNullOrEmpty(error))
            {
                GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
                style.normal.textColor = Color.red;
                EditorGUILayout.LabelField(error, style);
            }

            string directory = string.IsNullOrEmpty(error) ? deployVersion : "X.X.X";
            string dstPath = string.Format($"{VersionDefines.DEPLOYMENT_ROOT_PATH}/{VersionDefines.DEPLOYMENT_BACKUP_FOLDER}/{Utility.GetPlatformName()}/{directory}");
            EditorGUILayout.LabelField("备份目录", dstPath);

            string patchPath = string.Format($"{VersionDefines.DEPLOYMENT_ROOT_PATH}/{VersionDefines.cdnPatchDataPath}/{Utility.GetPlatformName()}/{directory}");
            EditorGUILayout.LabelField("补丁目录", patchPath);

            EditorGUI.BeginDisabledGroup(!string.IsNullOrEmpty(error));
            if (GUILayout.Button("Deploy", EditorStyles.toolbarButton))
            {
                Deployment.Run(VersionDefines.DEPLOYMENT_ROOT_PATH, deployVersion);
            }
            EditorGUI.EndDisabledGroup();
        }

        private void DrawBackdoorSetting()
        {
            JsonAsset obj = (JsonAsset)EditorGUILayout.ObjectField(new GUIContent("Backdoor Setting", ""), 
                                                                              m_backdoorProp.objectReferenceValue, 
                                                                              typeof(JsonAsset), 
                                                                              false);
            if (obj != m_backdoorProp.objectReferenceValue)
            {
                m_backdoorProp.objectReferenceValue = null;
                m_backdoor = null;
                if(obj != null)
                {
                    m_backdoor = obj.Require<Backdoor>();
                    m_backdoorProp.objectReferenceValue = obj;
                }
            }

            if(m_backdoor == null)
                return;

            string assetPath = AssetDatabase.GetAssetPath(m_backdoorProp.objectReferenceValue);
            bool changed = false;

            string minVersion = EditorGUILayout.TextField("MinVersion", m_backdoor.MinVersion);
            if(minVersion != m_backdoor.MinVersion)
            {
                changed = true;
                m_backdoor.MinVersion = minVersion;
            }

            // 显示不可编辑属性
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("CurVersion", m_backdoor.CurVersion);

            // VersionHistory_Win64
            EditorGUILayout.LabelField(new GUIContent("VersionHistory_Win64"));
            GUILayout.BeginVertical(new GUIStyle("HelpBox"));
            foreach(var item in m_backdoor.VersionHistory_Win64)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(item.Key);
                EditorGUILayout.LabelField(item.Value);
                EditorGUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            // VersionHistory_Android
            EditorGUILayout.LabelField(new GUIContent("VersionHistory_Android"));
            GUILayout.BeginVertical(new GUIStyle("HelpBox"));
            foreach (var item in m_backdoor.VersionHistory_Android)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(item.Key);
                EditorGUILayout.LabelField(item.Value);
                EditorGUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            // VersionHistory_IOS
            EditorGUILayout.LabelField(new GUIContent("VersionHistory_IOS"));
            GUILayout.BeginVertical(new GUIStyle("HelpBox"));
            foreach (var item in m_backdoor.VersionHistory_IOS)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(item.Key);
                EditorGUILayout.LabelField(item.Value);
                EditorGUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            EditorGUI.EndDisabledGroup();
            
            if(changed)
            {
                Backdoor.Serialize(assetPath, m_backdoor);
                AssetDatabase.Refresh();
            }
        }
    }
}