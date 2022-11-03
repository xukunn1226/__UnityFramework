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

        [MenuItem("Tools/Assets Management/Asset Check Editor Window &x")]
        private static void OpenWindow()
        {
            var window = GetWindow<AssetCheckEditorWindow>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 600);
            window.titleContent = new GUIContent("资源配置/检查工具");
        }

        private AssetCheckerOverview m_AssetCheckerOverview;
        private ToolBarPanel m_ToolBarPanel;
        private PropertyTree m_ToolBarPropertyTree;

        private AssetChecker Template = new AssetChecker() { Desc = "new Checker" };
        private PropertyTree m_CheckerTemplatePropertyTree;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_ToolBarPanel = new ToolBarPanel();
            m_ToolBarPropertyTree = PropertyTree.Create(m_ToolBarPanel);

            m_AssetCheckerOverview = AssetCheckerOverview.GetOrCreate();
        }

        protected override void OnGUI()
        {
            //SirenixEditorGUI.BeginBox("Default Locator");
            m_ToolBarPropertyTree?.Draw(false);
            //SirenixEditorGUI.EndBox();

            base.OnGUI();
        }

        private class CustomCheckerMenuItem : OdinMenuItem
        {
            private readonly AssetChecker instance;

            public CustomCheckerMenuItem(OdinMenuTree tree, AssetChecker instance, int index) : base(tree, string.Format($"{index}.  {instance.Desc}"), instance)
            {
                this.instance = instance;
            }

            protected override void OnDrawMenuItem(Rect rect, Rect labelRect)
            {
                labelRect.x -= 8;
                if(GUI.Button(labelRect.AlignMiddle(18).AlignRight(50), new GUIContent("删除")))
                {
                    AssetCheckEditorWindow.instance.RemoveChecker(instance);
                }
            }

            public override string SmartName { get { return this.instance.Desc; } }
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

            tree.AddObjectAtPath("Checker Overview", this);
            for(int i = 0; i < m_AssetCheckerOverview.AllCheckers.Count; i++)
            {
                var item = m_AssetCheckerOverview.AllCheckers[i];
                var customMenuItem = new CustomCheckerMenuItem(tree, item, i);
                tree.AddMenuItemAtPath(string.Format($"Checker Overview"), customMenuItem);
            }

            return tree;
        }

        [OnInspectorGUI]
        private void OnShowTemplate()
        {
            if (m_CheckerTemplatePropertyTree == null)
            {
                m_CheckerTemplatePropertyTree = PropertyTree.Create(Template);
            }
            SirenixEditorGUI.BeginBox("Template");
            m_CheckerTemplatePropertyTree?.Draw(false);
            SirenixEditorGUI.EndBox();
        }

        [OnInspectorGUI]
        [GUIColor(1, 0, 0)]
        private void OnShowAdd()
        {
            if(GUILayout.Button("Add"))
            {
                AddChecker(AssetChecker.Create(Template));
            }
        }

        public void Save()
        {
            AssetCheckerOverview.Save(m_AssetCheckerOverview);
        }

        public void DoProcessorAndExportAll()
        {
            AssetCheckerOverview.DoProcessorAndExportAll(m_AssetCheckerOverview);
        }

        public void AddChecker(AssetChecker item)
        {
            m_AssetCheckerOverview?.Add(item);
            ForceMenuTreeRebuild();
        }

        public void RemoveChecker(AssetChecker item)
        {
            m_AssetCheckerOverview?.Remove(item);
            ForceMenuTreeRebuild();
        }
    }

    public class ToolBarPanel
    {
        [OnInspectorGUI]
        void OnShow()
        {
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button(new GUIContent("保存"), GUILayoutOptions.Width(120)))
            {
                AssetCheckEditorWindow.instance.Save();
            }
            if(GUILayout.Button(new GUIContent("执行所有检测并导出"), GUILayoutOptions.Width(150)))
            {
                AssetCheckEditorWindow.instance.DoProcessorAndExportAll();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}