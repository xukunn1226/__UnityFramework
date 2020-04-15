using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Framework.GameBuilder
{
    [CustomEditor(typeof(GameBuilderSetting))]
    public class GameBuilderSettingEditor : Editor
    {
        SerializedProperty m_bundleSettingProp;
        SerializedProperty m_playerSettingProp;

        Editor m_bundleSettingEditor;
        Editor m_playerSettingEditor;

        enum BuildMode
        {
            BundlesAndPlayer,
            Bundles,
            Player,
        }
        BuildMode m_mode;

        private void Awake()
        {
            m_bundleSettingProp = serializedObject.FindProperty("bundleSetting");
            m_playerSettingProp = serializedObject.FindProperty("playerSetting");
        }

        public override void OnInspectorGUI()
        {
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
            DrawBuildButton();

            serializedObject.ApplyModifiedProperties();
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
                //m_mode = (BuildMode)EditorGUILayout.EnumPopup(m_mode, EditorStyles.popup, GUILayout.Width(135));
                m_mode = BuildMode.BundlesAndPlayer;

                GUIStyle boldStyle = new GUIStyle("LargeButtonLeft");
                boldStyle.fontStyle = FontStyle.Bold;
                boldStyle.alignment = TextAnchor.MiddleCenter;

                bool disable = false;
                switch(m_mode)
                {
                    case BuildMode.Bundles:
                        disable = m_bundleSettingProp.objectReferenceValue == null;
                        break;
                    case BuildMode.Player:
                        disable = m_playerSettingProp.objectReferenceValue == null;
                        break;
                    case BuildMode.BundlesAndPlayer:
                        disable = m_bundleSettingProp.objectReferenceValue == null || m_playerSettingProp.objectReferenceValue == null;
                        break;
                }
                EditorGUI.BeginDisabledGroup(disable);
                if(GUILayout.Button("Build Bundles & Player", boldStyle))
                {
                    switch(m_mode)
                    {
                        case BuildMode.Bundles:
                            BundleBuilder.BuildAssetBundles(m_bundleSettingProp.objectReferenceValue as BundleBuilderSetting);
                            break;
                        case BuildMode.Player:
                            PlayerBuilder.BuildPlayer(m_playerSettingProp.objectReferenceValue as PlayerBuilderSetting);
                            break;
                        case BuildMode.BundlesAndPlayer:
                            GameBuilder.BuildGame((GameBuilderSetting)target);
                            break;
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}