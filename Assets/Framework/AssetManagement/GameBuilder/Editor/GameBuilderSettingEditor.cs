using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Framework.AssetManagement.GameBuilder
{
    [CustomEditor(typeof(GameBuilderSetting))]
    public class GameBuilderSettingEditor : Editor
    {
        SerializedProperty m_bundleSettingProp;
        SerializedProperty m_playerSettingProp;
        SerializedProperty m_buildModeProp;

        Editor m_bundleSettingEditor;
        Editor m_playerSettingEditor;

        private void Awake()
        {
            m_bundleSettingProp = serializedObject.FindProperty("bundleSetting");
            m_playerSettingProp = serializedObject.FindProperty("playerSetting");
            m_buildModeProp = serializedObject.FindProperty("buildMode");
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

            EditorGUILayout.Separator();
            DrawBundleSettingEditor();

            EditorGUILayout.Separator();
            DrawPlayerSettingEditor();

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            DrawBuildButton();

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
    }
}