using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Framework.AssetManagement.Runtime;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using Sirenix.OdinInspector;
using Framework.AssetManagement.AssetChecker;
using Unity.Plastic.Antlr3.Runtime.Tree;

namespace Framework.AssetManagement.AssetBundleDebugger
{
    public class AssetBundleDebuggerWindow : OdinMenuEditorWindow
    {
        private int             m_CachedCount = -1;
        private DebugReport     m_DebugReport;
        private ToolBarPanel    m_ToolBarPanel;
        private PropertyTree    m_ToolBarPropertyTree;

        [MenuItem("Tools/Assets Management/资源包调试工具")]
        private static void OpenWindow()
        {
            var window = GetWindow<AssetBundleDebuggerWindow>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 600);
            window.titleContent = new GUIContent("资源包调试工具");
        }
        
        static private AssetBundleDebuggerWindow s_Instance;
        static public AssetBundleDebuggerWindow instance
        {
            get
            {
                if(s_Instance == null)
                {
                    s_Instance = GetWindow<AssetBundleDebuggerWindow>();
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
            if (EditorApplication.isPlaying)
            {
                m_DebugReport = AssetManagerEx.GetDebugReport();
                //int count = m_DebugReport.DebugProviderInfos.Count;
                //if(count != m_CachedCount)
                //{
                //    ForceMenuTreeRebuild();
                //    m_CachedCount = count;
                //}
            }
            else
            {
                m_DebugReport = new DebugReport();
                m_DebugReport.DebugProviderInfos.Add(new DebugProviderInfo() { AssetPath = "范例.png" });
            }

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

            for (int i = 0; i < m_DebugReport.DebugProviderInfos.Count; ++i)
            {
                var viewData = new AssetViewPanel(m_DebugReport.DebugProviderInfos[i]);
                var menuItem = new CustomCheckerMenuItem(tree, viewData);
                tree.AddMenuItemAtPath("", menuItem);
            }

            return tree;
        }

        public void ForceTreeRebuild()
        {
            ForceMenuTreeRebuild();
        }

        private class CustomCheckerMenuItem : OdinMenuItem
        {
            private readonly DebugProviderInfo instance;

            public CustomCheckerMenuItem(OdinMenuTree tree, AssetViewPanel viewData) : base(tree, viewData.providerInfo.AssetPath, viewData)
            {
                this.instance = viewData.providerInfo;
            }

            protected override void OnDrawMenuItem(Rect rect, Rect labelRect)
            {
                //labelRect.x -= 8;
                //if (GUI.Button(labelRect.AlignMiddle(18).AlignRight(50), new GUIContent("ɾ��")))
                //{
                //}
            }

            public override string SmartName { get { return instance != null ? instance.AssetPath : "NULL"; } }
        }
    }

    public class ToolBarPanel
    {
        [OnInspectorGUI]
        void OnShow()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("刷新"), GUILayoutOptions.Width(120)))
            {
                AssetBundleDebuggerWindow.instance.ForceTreeRebuild();
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    public class AssetViewPanel
    {
        public DebugProviderInfo providerInfo;

        public AssetViewPanel(DebugProviderInfo providerInfo)
        {
            this.providerInfo = providerInfo;
        }

        //[ShowInInspector]
        //[TableList(AlwaysExpanded = true, HideToolbar = true, IsReadOnly = true), HideLabel]
        //public List<AssetViewItem> assetViews
        //{
        //    get
        //    {
        //        if(m_DebugReport == null)
        //        {
        //            return new List<AssetViewItem>() { new AssetViewItem() { index = 1, assetPath = "����" } };
        //        }
        //        else
        //        {
        //            List<AssetViewItem> list = new List<AssetViewItem>();
        //            for(int i = 0; i < m_DebugReport.DebugProviderInfos.Count; ++i)
        //            {
        //                AssetViewItem item = new AssetViewItem();
        //                item.index = i;
        //                item.assetPath = m_DebugReport.DebugProviderInfos[i].AssetPath;
        //                item.refCount = m_DebugReport.DebugProviderInfos[i].RefCount;
        //                item.status = m_DebugReport.DebugProviderInfos[i].Status;
        //                item.spawnScene = m_DebugReport.DebugProviderInfos[i].SpawnScene;
        //                item.spawnTime = m_DebugReport.DebugProviderInfos[i].SpawnTime;
        //                item.loadingTime = m_DebugReport.DebugProviderInfos[i].LoadingTime;
        //                list.Add(item);
        //            }
        //            return list;
        //        }                
        //    }
        //}
    }
}