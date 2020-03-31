using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AssetManagement.AssetBrowser
{
    public class AssetBrowserMain : EditorWindow
    {
        [MenuItem("Assets Management/Asset Browser", priority = 2050)]
        static public void Init()
        {
            AssetBrowserMain window = GetWindow<AssetBrowserMain>();
            window.titleContent = new GUIContent("Asset Browser");
            window.position = new Rect(350, 260, 1200, 600);
            window.Show();
        }

        private Texture2D   m_RefreshTexture;
        const float         k_ToolbarPadding    = 15;
        const float         k_MenubarPadding    = 32;
        const float         k_StatusbarPadding  = 20;

        enum Mode
        {
            AssetInspect,
            SceneInspect,
        }
        private Mode m_Mode;

        internal string             status { get; set; }

        BundleInspectorTab          m_AssetInspectorTab;

        private void OnEnable()
        {
            m_RefreshTexture = EditorGUIUtility.FindTexture("Refresh");

            m_AssetInspectorTab = new BundleInspectorTab();
            m_AssetInspectorTab.OnEnable(GetSubWindowArea(), this);

            status = "This is status bar";
        }

        private void OnDisable()
        {
            if (m_AssetInspectorTab != null)
                m_AssetInspectorTab.OnDisable();
            m_AssetInspectorTab = null;

            m_RefreshTexture = null;
        }

        private void OnGUI()
        {
            ModeToggle();

            DrawStatusBar();

            DrawSubWindow(GetSubWindowArea());
        }

        void ModeToggle()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(k_ToolbarPadding);
                switch (m_Mode)
                {
                    case Mode.AssetInspect:
                        if(GUILayout.Button(m_RefreshTexture, GUILayout.Width(m_RefreshTexture.width * 2)))
                        {
                            m_AssetInspectorTab.Reload();
                        }
                        break;
                    case Mode.SceneInspect:
                        if(GUILayout.Button(m_RefreshTexture, GUILayout.Width(m_RefreshTexture.width * 2)))
                        {

                        }
                        break;
                }
                float toolbarWidth = position.width - k_ToolbarPadding * 3 - m_RefreshTexture.width * 2;
                string[] labels = new string[2] { "Asset", "Scene" };
                m_Mode = (Mode)GUILayout.Toolbar((int)m_Mode, labels, "LargeButton", GUILayout.Width(toolbarWidth));
            }            
        }

        void DrawStatusBar()
        {
            Rect rc = new Rect(0, position.height - k_StatusbarPadding, position.width, k_StatusbarPadding);
            GUILayout.BeginArea(rc);

            GUILayout.Label(status, "LargeLabel");

            GUILayout.EndArea();
        }

        void DrawSubWindow(Rect rect)
        {
            switch(m_Mode)
            {
                case Mode.AssetInspect:
                    m_AssetInspectorTab.OnGUI(rect);
                    break;
            }
        }

        private Rect GetSubWindowArea()
        {
            return new Rect(0, k_MenubarPadding, position.width, position.height - k_MenubarPadding - k_StatusbarPadding);
        }
    }
}