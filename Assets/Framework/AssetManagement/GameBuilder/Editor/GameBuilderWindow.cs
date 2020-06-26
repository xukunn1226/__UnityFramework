using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Framework.AssetManagement.GameBuilder
{
    public class GameBuilderWindow : EditorWindow
    {
        private GameBuilderSettingCollection target;

        private List<string>    toolbarLabels = new List<string>();
        private int             selectedSettingIndex;
        //private string          selectedKeyName;
        private Editor          selectedEditor;

        private Vector2         scrollPosition;
        private string          info;


        [MenuItem("Assets Management/GameBuilder Window")]
        static void Init()
        {
            GameBuilderWindow window = GetWindow<GameBuilderWindow>();
            window.Show();
            window.titleContent = new GUIContent("GameBuilder Window", EditorGUIUtility.FindTexture("SettingsIcon"));
            window.position = new Rect(600, 120, 700, 800);
        }

        private void OnEnable()
        {
            target = GameBuilderSettingCollection.GetDefault();

            UpdateToolbar(PlayerPrefs.GetInt("GameBuilderWindow.SelectedIndex"));
        }

        private void UpdateToolbar(int pendingIndex)
        {
            // refresh toolbar
            toolbarLabels.Clear();
            foreach(var kvp in target.setting)
            {
                toolbarLabels.Add(kvp.displayName);
            }

            // update the selected GameBuilderSetting asset
            selectedSettingIndex = pendingIndex;
            selectedSettingIndex = toolbarLabels.Count == 0 ? -1 : Mathf.Clamp(selectedSettingIndex, 0, toolbarLabels.Count - 1);
            GameBuilderSetting selectedSetting = selectedSettingIndex != -1 ? target.GetData(toolbarLabels[selectedSettingIndex]) : null;
            //if(selectedSetting != null)
            //{
            //    Selection.activeObject = selectedSetting;
            //}

            // create a new Editor
            selectedEditor = selectedSetting != null ? Editor.CreateEditor(selectedSetting) : null;

            //selectedKeyName = selectedSettingIndex != -1 ? toolbarLabels[selectedSettingIndex] : null;

            info = string.Empty;

            PlayerPrefs.SetInt("GameBuilderWindow.SelectedIndex", selectedSettingIndex);
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }

        private void OnGUI()
        {
            if (target == null)
                return;

            GUILayout.Space(5);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(5);

                if(GUILayout.Button(new GUIContent("Add", ""), new GUIStyle("LargeButton"), GUILayout.Width(100)))
                {
                    target.Add(GameBuilderUtil.s_DefaultSettingPath);
                    UpdateToolbar(toolbarLabels.Count);
                }

                if(GUILayout.Button(new GUIContent("Remove", ""), new GUIStyle("LargeButton"), GUILayout.Width(100)))
                {
                    if (selectedSettingIndex >= 0)
                    {
                        if (EditorUtility.DisplayDialogComplex("", "Are you sure DELETE the GameBuilder Setting?", "OK", "Cancel", "") == 0)
                        {
                            target.Remove(toolbarLabels[selectedSettingIndex]);
                            UpdateToolbar(selectedSettingIndex);
                        }
                    }
                }

                GUILayout.Space(5);

                EditorGUI.BeginChangeCheck();
                selectedSettingIndex = GUILayout.Toolbar(selectedSettingIndex, toolbarLabels.ToArray(), "LargeButton", GUILayout.Width(position.width - 225));
                if (EditorGUI.EndChangeCheck())
                {
                    UpdateToolbar(selectedSettingIndex);
                }
            }

            GUILayout.Space(5);
            DrawSelectedSetting(selectedEditor);
        }

        private void DrawSelectedSetting(Editor editor)
        {
            if (!editor)
                return;

            EditorGUILayout.BeginHorizontal();
            GUIStyle newStyle = new GUIStyle("TextArea");
            newStyle.fixedHeight = 24;
            newStyle.fontSize = 14;
            EditorGUI.BeginChangeCheck();
            string newName = EditorGUILayout.TextField("Setting Name", toolbarLabels[selectedSettingIndex], newStyle, GUILayout.Width(260));
            if(EditorGUI.EndChangeCheck())
            {
                if(target.Rename(toolbarLabels[selectedSettingIndex], newName))
                {
                    UpdateToolbar(selectedSettingIndex);
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.BeginArea(new Rect(0, 65, position.width, position.height - 70), "", EditorStyles.textArea);

            using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition))
            {
                scrollPosition = scrollView.scrollPosition;

                editor.OnInspectorGUI();
            }

            GUILayout.EndArea();
        }
    }
}