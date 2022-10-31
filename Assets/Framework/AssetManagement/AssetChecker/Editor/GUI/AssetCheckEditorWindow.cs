using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using Sirenix.OdinInspector;

namespace Framework.AssetManagement.AssetChecker
{
    public class AssetCheckEditorWindow : OdinMenuEditorWindow
    {
        static private AssetCheckEditorWindow s_Instance;
        static public AssetCheckEditorWindow instance
        {
            get
            {
                if(s_Instance == null)
                {
                    s_Instance = GetWindow<AssetCheckEditorWindow>();
                }
                return s_Instance;
            }
        }

        [MenuItem("Tools/Assets Management/Asset Check Editor Window")]
        private static void OpenWindow()
        {
            var window = GetWindow<AssetCheckEditorWindow>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 600);
            window.titleContent = new GUIContent("资源配置/检查工具");
        }

        private AssetCheckerOverview m_AssetCheckerOverview;
        private ToolBarPanel m_ToolBarPanel;
        private PropertyTree m_ToolBarPropertyTree;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_ToolBarPanel = new ToolBarPanel();
            m_ToolBarPropertyTree = PropertyTree.Create(m_ToolBarPanel);

            m_AssetCheckerOverview = AssetCheckerOverview.GetOrCreate();
        }

        protected override void OnGUI()
        {
            m_ToolBarPropertyTree?.Draw(false);
            base.OnGUI();
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            OdinMenuTree tree = new OdinMenuTree(false);

            // MenuTree config
            var customMenuStyle = new OdinMenuStyle
            {
                BorderPadding = 0f,
                AlignTriangleLeft = true,
                TriangleSize = 16f,
                TrianglePadding = 0f,
                Offset = 20f,
                Height = 23,
                IconPadding = 0f,
                BorderAlpha = 0.323f
            };
            tree.DefaultMenuStyle = customMenuStyle;
            tree.Config.DrawSearchToolbar = true;

            // 
            tree.AddObjectAtPath("Checker Overview", null);
            tree.AddMenuItemAtPath("Checker Overview/11", null);

            return tree;
        }
    }

    public class ToolBarPanel
    {
        [HorizontalGroup("statusBar")]
        //[BoxGroup("1111")]
        public string text;

        [HorizontalGroup("statusBar")]
        //[BoxGroup("1111")]
        [OnInspectorGUI]
        void OnShow()
        {
            SirenixEditorGUI.BeginHorizontalToolbar(26);
            Rect rect = AssetCheckEditorWindow.instance.position;
            GUI.Button(new Rect(rect.width - 100, 1, 100, 60), new GUIContent("保存"));
            SirenixEditorGUI.EndHorizontalToolbar();
        }
    }
}