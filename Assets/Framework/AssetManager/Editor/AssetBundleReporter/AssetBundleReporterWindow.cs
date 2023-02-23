using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Framework.AssetManagement.Runtime;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using Sirenix.OdinInspector;

namespace Framework.AssetManagement.AssetEditorWindow
{
    public class AssetBundleReporterWindow : OdinEditorWindow
    {
        [MenuItem("Tools/Assets Management/资源包查看工具")]
        public static void OpenWindow()
        {
            if(instance == null)
            {
                var window = GetWindow<AssetBundleReporterWindow>();
                window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 600);
                window.titleContent = new GUIContent("资源包查看工具");
            }
            else            
            {
                instance.Show(true);
                instance.Focus();
            }
        }

        static private AssetBundleReporterWindow s_Instance;
        static public AssetBundleReporterWindow instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = GetWindow<AssetBundleReporterWindow>();
                }
                return s_Instance;
            }
        }

        enum ViewMode
        {
            Summary,
            Asset,
            Bundle,
        }
        private ViewMode m_ViewMode = ViewMode.Summary;

        private ReportBuild m_Reporter;
        private SummaryViewer m_SummaryViewer;
        
        protected override void OnBeginDrawEditors()
        {            
            base.OnBeginDrawEditors();
            SirenixEditorGUI.BeginHorizontalToolbar();
            {
                m_ViewMode = (ViewMode)EditorGUILayout.EnumPopup(m_ViewMode, GUILayout.Width(150));

                GUILayout.FlexibleSpace();
                Color clr = GUI.color;
                GUI.color = Color.green;
                if(SirenixEditorGUI.ToolbarButton("导入"))
                {
                    ImportReporter(EditorUtility.OpenFilePanel("导入报告", "", "json"));                    
                }
                GUI.color = clr;
            }
            SirenixEditorGUI.EndHorizontalToolbar();
        }

        protected override void DrawEditors()
        {
            if(m_Reporter != null)
            {
                switch(m_ViewMode)
                {
                    case ViewMode.Summary:
                        m_SummaryViewer?.Draw();
                        break;
                }
            }
        }

        private void ImportReporter(string path)
        {
            if(!string.IsNullOrEmpty(path))
            {
                var jsonData = System.IO.File.ReadAllText(path);
                m_Reporter = ReportBuild.Deserialize(jsonData);

                m_SummaryViewer = new SummaryViewer(m_Reporter);
            }
        }
    }
}