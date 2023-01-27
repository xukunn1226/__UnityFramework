using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        private List<AssetBundleCollectorGroup> m_PendingRemovedGroup = new List<AssetBundleCollectorGroup>();
        private List<AssetBundleCollector> m_PendingRemovedCollector = new List<AssetBundleCollector>();

        [MenuItem("Tools/Assets Management/资源包收集工具")]
        private static void OpenWindow()
        {
            var window = GetWindow<AssetBundleCollectorWindow>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 600);
            window.titleContent = new GUIContent("资源包收集工具");
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
            tree.DefaultMenuStyle = OdinMenuStyle.TreeViewStyle;
            tree.Config.DrawSearchToolbar = true;

            for (int i = 0; i < AssetBundleCollectorSettingData.Instance.Configs.Count; ++i)
            {
                ConfigPanel config = new ConfigPanel(AssetBundleCollectorSettingData.Instance.Configs[i]);
                var customMenuItem = new CustomMenuItem(tree, config, i);
                tree.AddMenuItemAtPath("", customMenuItem);
            }

            return tree;
        }

        private class CustomMenuItem : OdinMenuItem
        {
            private readonly ConfigPanel instance;

            public CustomMenuItem(OdinMenuTree tree, ConfigPanel instance, int index) : base(tree, string.Format($"{index}.{instance.config.ConfigName}"), instance)
            {
                this.instance = instance;
            }

            public override string SmartName
            {
                get
                {
                    if (string.IsNullOrEmpty(instance.config.ConfigDesc))
                        return string.Format($"{instance.config.ConfigName}");
                    return string.Format($"{instance.config.ConfigName}({instance.config.ConfigDesc})");
                }
            }
        }

        protected override void OnBeginDrawEditors()
        {
            base.OnBeginDrawEditors();
            OdinMenuTreeSelection selected = this.MenuTree.Selection; //獲取目前的選擇
            SirenixEditorGUI.BeginHorizontalToolbar(); //在這範圍內的選項會顯示在水平位置
            {
                GUILayout.FlexibleSpace();
                if (SirenixEditorGUI.ToolbarButton("Delete current")) //如果點擊Delete current，此UnitData就會被刪除
                {
                    //UnitData unitData = selected.SelectedValue as UnitData;
                    //string path = AssetDatabase.GetAssetPath(unitData);
                    //AssetDatabase.DeleteAsset(path);
                    //AssetDatabase.SaveAssets();
                }
            }
            SirenixEditorGUI.EndHorizontalToolbar();
        }

        //protected override void OnEndDrawEditors()
        //{
        //    SirenixEditorGUI.BeginIndentedHorizontal();
        //    if(GUILayout.Button("Add Group"))
        //    {

        //    }
        //    SirenixEditorGUI.EndIndentedHorizontal();
        //    base.OnEndDrawEditors();
        //}

        class ConfigPanel
        {
            [HideInInspector]
            public AssetBundleCollectorConfig config { get; private set; }
            //private PropertyTree m_PropertyTree;

            public ConfigPanel(AssetBundleCollectorConfig config)
            {
                this.config = config;
            }

            [OnInspectorGUI]
            void OnGUI()
            {
                //if(m_PropertyTree == null)
                //{
                //    m_PropertyTree = PropertyTree.Create(config);
                //}
                //m_PropertyTree.Draw(false);

                EditorGUI.BeginChangeCheck();
                
                config.ConfigName = EditorGUILayout.TextField(new GUIContent("Config Name", ""), config.ConfigName);
                config.ConfigDesc = EditorGUILayout.TextField(new GUIContent("Config Desc", ""), config.ConfigDesc);

                // draw AssetBundleCollectorGroup
                EditorGUI.indentLevel++;
                AssetBundleCollectorWindow.instance.m_PendingRemovedGroup.Clear();
                for (int i = 0; i < config.Groups.Count; ++i)
                {
                    DrawGroup(config.Groups[i]);
                }
                foreach(var removedGroup in AssetBundleCollectorWindow.instance.m_PendingRemovedGroup)
                {
                    config.RemoveGroup(removedGroup);
                }
                EditorGUI.indentLevel--;

                if (GUILayout.Button("Add Group"))
                {
                    config.AddGroup("Default GroupName", "");
                }

                if (EditorGUI.EndChangeCheck())
                {
                    AssetBundleCollectorSettingData.SetDirty();
                }
            }

            void DrawGroup(AssetBundleCollectorGroup group)
            {
                EditorGUILayout.BeginHorizontal();
                if(GUILayout.Button("[-]", GUILayout.Width(30)))
                {
                    AssetBundleCollectorWindow.instance.m_PendingRemovedGroup.Add(group);
                }

                EditorGUILayout.BeginVertical(EditorStyles.textArea);
                group.GroupName = EditorGUILayout.TextField(new GUIContent("Group Name", ""), group.GroupName);
                group.GroupDesc = EditorGUILayout.TextField(new GUIContent("Group Desc", ""), group.GroupDesc);
                

                // draw collector
                EditorGUI.indentLevel++;
                {
                    AssetBundleCollectorWindow.instance.m_PendingRemovedCollector.Clear();
                    foreach (var collector in group.Collectors)
                    {
                        DrawCollector(group, collector);
                    }
                    foreach(var removedCollector in AssetBundleCollectorWindow.instance.m_PendingRemovedCollector)
                    {
                        group.RemoveCollector(removedCollector);
                    }
                }
                if(GUILayout.Button("Add Collector"))
                {
                    group.AddCollector();
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
            }

            void DrawCollector(AssetBundleCollectorGroup group, AssetBundleCollector collector)
            {
                EditorGUILayout.BeginHorizontal();
                if(GUILayout.Button("[-]", GUILayout.Width(30)))
                {
                    AssetBundleCollectorWindow.instance.m_PendingRemovedCollector.Add(collector);
                }

                EditorGUILayout.BeginVertical();
                {
                    UnityEngine.Object oldObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(collector.CollectPath);
                    UnityEngine.Object newObj = EditorGUILayout.ObjectField(new GUIContent("Collector", ""), oldObj, typeof(UnityEngine.Object), false);
                    if(oldObj != newObj)
                    {
                        collector.CollectGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(newObj));
                    }

                    EditorGUILayout.BeginHorizontal();
                    {
                        // Collector Type
                        int selectedIndex = EditorGUILayout.Popup((int)collector.CollectorType, new string[] { "MainCollector", "StaticCollector", "DependCollector" });
                        collector.CollectorType = (ECollectorType)selectedIndex;

                        // Pack Rule
                        List<RuleDisplayName> packRuleDisplayNames = AssetBundleCollectorSettingData.GetPackRuleNames();                        
                        int index = packRuleDisplayNames.FindIndex(item => { return item.ClassName == collector.PackRuleName; });
                        selectedIndex = EditorGUILayout.Popup(Mathf.Max(0, index), packRuleDisplayNames.Select(item => item.DisplayName).ToArray());
                        collector.PackRuleName = packRuleDisplayNames[selectedIndex].ClassName;

                        // Filter Rule
                        List<RuleDisplayName> filterRuleDisplayNames = AssetBundleCollectorSettingData.GetFilterRuleNames();
                        index = filterRuleDisplayNames.FindIndex(item => { return item.ClassName == collector.FilterRuleName; });
                        selectedIndex = EditorGUILayout.Popup(Mathf.Max(0, index), filterRuleDisplayNames.Select(item => item.DisplayName).ToArray());
                        collector.FilterRuleName = filterRuleDisplayNames[selectedIndex].ClassName;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
            }
        }

        class ToolBarPanel
        {
            [OnInspectorGUI]
            void OnShow()
            {
                EditorGUILayout.BeginHorizontal();
                string text = AssetBundleCollectorSettingData.isDirty ? "保存*" : "保存";
                if (GUILayout.Button(new GUIContent(text), GUILayoutOptions.Width(120)))
                {
                    AssetBundleCollectorSettingData.SaveFile();
                    instance.ForceMenuTreeRebuild();
                }
                if (GUILayout.Button(new GUIContent("Add Config"), GUILayoutOptions.Width(150)))
                {
                    AssetBundleCollectorSettingData.AddConfig("Default ConfigName", "");
                    instance.ForceMenuTreeRebuild();
                }
                if (GUILayout.Button(new GUIContent("Remove Config"), GUILayoutOptions.Width(150)))
                {
                    if (instance.MenuTree.Selection.SelectedValue != null)
                    {
                        AssetBundleCollectorSettingData.RemoveConfig(instance.MenuTree.Selection.SelectedValue as AssetBundleCollectorConfig);
                        instance.ForceMenuTreeRebuild();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }    
}