using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using Sirenix.OdinInspector;

namespace Framework.AssetManagement.AssetEditorWindow
{
    public class AssetBundleCollectorWindow : OdinMenuEditorWindow
    {
        private ToolBarPanel m_ToolBarPanel;
        private PropertyTree m_ToolBarPropertyTree;

        [MenuItem("Tools/Assets Management/资源包收集工具")]
        private static void OpenWindow()
        {
            var window = GetWindow<AssetBundleCollectorWindow>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 600);
            window.titleContent = new GUIContent("资源包调试工具");
        }

        static private AssetBundleCollectorWindow s_Instance;
        static public AssetBundleCollectorWindow instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = GetWindow<AssetBundleCollectorWindow>();
                }
                return s_Instance;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            m_ToolBarPanel = new ToolBarPanel();
            m_ToolBarPropertyTree = PropertyTree.Create(m_ToolBarPanel);

        }

        protected override void OnGUI()
        {
            m_ToolBarPropertyTree?.Draw(false);

            base.OnGUI();
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree(false);

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

            return tree;
        }

        public class ToolBarPanel
        {
            [OnInspectorGUI]
            void OnShow()
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent("保存"), GUILayoutOptions.Width(120)))
                {
                }
                //if (GUILayout.Button(new GUIContent("执行所有检测并导出"), GUILayoutOptions.Width(150)))
                //{
                //}
                EditorGUILayout.EndHorizontal();
            }
        }
    }    
}