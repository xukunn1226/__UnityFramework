using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Framework.AssetManagement.Runtime;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using Sirenix.OdinInspector;
using UnityEditor.UIElements;

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

            // if (GUILayout.Button(new GUIContent(""), GUILayoutOptions.Width(120)))
            // {
            //     
            // }
            EditorGUILayout.EndHorizontal();
        }
    }

    public class AssetViewPanel
    {
        [HideInInspector]
        public DebugProviderInfo providerInfo;

        public AssetViewPanel(DebugProviderInfo providerInfo)
        {
            this.providerInfo = providerInfo;
        }

        [OnInspectorGUI]
        void OnGUI()
        {
            EditorGUILayout.LabelField("AssetPath", providerInfo.AssetPath);
            EditorGUILayout.LabelField("SpawnScene", providerInfo.SpawnScene);
            EditorGUILayout.LabelField("SpawnTime", providerInfo.SpawnTime);
            EditorGUILayout.LabelField("LoadingTime", $"{providerInfo.LoadingTime}ms");
            EditorGUILayout.LabelField("RefCount", providerInfo.RefCount.ToString());
            EditorGUILayout.LabelField("Status", providerInfo.Status);

            EditorGUILayout.BeginFoldoutHeaderGroup(true, "Depend Bundles");
            for (int i = 0; providerInfo.DependBundleInfos != null && i < providerInfo.DependBundleInfos.Count; ++i)
            {
                EditorGUI.indentLevel++;
                DebugBundleInfo bundleInfo = providerInfo.DependBundleInfos[i];
                EditorGUILayout.LabelField("BundleName", bundleInfo.BundleName);
                EditorGUILayout.LabelField("RefCount", bundleInfo.RefCount.ToString());
                EditorGUILayout.LabelField("Status", bundleInfo.Status);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.BeginFoldoutHeaderGroup(true, "Stack Traces");
            for (int i = 0; providerInfo.StackTraces != null && i < providerInfo.StackTraces.Count; ++i)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.TextField("Stack Trace", providerInfo.StackTraces[i], GUILayout.MinHeight(120));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}