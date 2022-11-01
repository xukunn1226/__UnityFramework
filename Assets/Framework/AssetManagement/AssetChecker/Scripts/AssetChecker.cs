using Newtonsoft.Json;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Sirenix.Utilities.Editor;

namespace Framework.AssetManagement.AssetChecker
{
    public class AssetChecker
    {
        [Serializable]
        public class AssetFilterInfo
        {
            public bool enabled;
            public IAssetFilter filter;

            static public AssetFilterInfo Create<T>() where T : IAssetFilter, new()
            {
                AssetFilterInfo info = new AssetFilterInfo();
                info.enabled = true;
                info.filter = new T();
                return info;
            }
        }
        [LabelText("描述")]
        public string               Desc;
        [HideInInspector]
        public AssetFilterInfo      PathFilter;
        [HideInInspector]
        public AssetFilterInfo      FilenameFilter;
        [HideInInspector]
        public IAssetProcessor      Processor;

        private PropertyTree        m_PathFilterPropertyTree;
        private PropertyTree        m_FilenameFilterPropertyTree;
        private PropertyTree        m_ProcessorPropertyTree;
        private bool                m_Visible;

        [OnInspectorGUI]
        private void OnDrawFilters()
        {
            EditorGUILayout.Space();

            // draw Path Filter
            if(m_PathFilterPropertyTree == null)
            {
                m_PathFilterPropertyTree = PropertyTree.Create(PathFilter.filter);
            }
            SirenixEditorGUI.BeginToggleGroup(PathFilter, ref PathFilter.enabled, ref m_Visible, "【路径过滤器】");
            m_PathFilterPropertyTree.Draw(false);
            SirenixEditorGUI.EndToggleGroup();

            EditorGUILayout.Space();
            
            // draw Filename Filter
            if(m_FilenameFilterPropertyTree == null)
            {
                m_FilenameFilterPropertyTree = PropertyTree.Create(FilenameFilter.filter);
            }
            SirenixEditorGUI.BeginToggleGroup(FilenameFilter, ref FilenameFilter.enabled, ref m_Visible, "【文件名过滤器】");
            m_FilenameFilterPropertyTree.Draw(false);
            SirenixEditorGUI.EndToggleGroup();
        }

        [OnInspectorGUI]
        [PropertyOrder(11)]
        private void OnDrawProcessor()
        {
            if(m_ProcessorPropertyTree == null && Processor != null)
            {
                m_ProcessorPropertyTree = PropertyTree.Create(Processor);
            }

            if (Processor != null)
            {
                m_ProcessorPropertyTree?.Draw(false);
            }
        }

        [BoxGroup("【处理器】")]
        [HorizontalGroup("【处理器】/LayerHor", 0.3f)]
        [GUIColor(0, 1, 0)]
        [Button("添加处理器")]
        private void AddProcessor()
        {
            Processor = Activator.CreateInstance(Type.GetType(m_SelectedComponentName)) as IAssetProcessor;
            m_ProcessorPropertyTree = null;
        }

        [BoxGroup("【处理器】")]
        [HorizontalGroup("【处理器】/LayerHor", 0.6f)]
        [SerializeField]
        [ValueDropdown("ComponentNameList")]
        [LabelText("选择处理器")]
        [PropertyOrder(10)]
        private string m_SelectedComponentName = ComponentNameList[0];

        private static string[] ComponentNameList
        {
            get
            {
                var list = new List<string>();
                foreach (var type in TypeCache.GetTypesDerivedFrom<IAssetProcessor>())
                {
                    list.Add(type.FullName);
                }
                return list.ToArray();
            }
        }

        private static string[] DisplayNameList
        {
            get
            {
                var list = new List<string>();
                string[] fullName = ComponentNameList;
                foreach(var name in fullName)
                {
                    int index = name.LastIndexOf(".");
                    if(index != -1)
                    {
                        list.Add(name.Substring(index));
                    }
                    else
                    {
                        list.Add(name);
                    }
                }
                return list.ToArray();
            }
        }

        public AssetChecker()
        {
            PathFilter = AssetFilterInfo.Create<AssetFilter_Path>();
            FilenameFilter = AssetFilterInfo.Create<AssetFilter_Filename>();
        }

        /// <summary>
        /// 执行过滤器
        /// </summary>
        /// <returns>返回执行过滤器后的结果信息</returns>
        public List<string> DoFilter()
        {
            List<string> paths = new List<string>();
            if (PathFilter != null && PathFilter.enabled)
            {
                paths = PathFilter.filter.DoFilter();
            }

            if (FilenameFilter != null && FilenameFilter.enabled)
            {
                AssetFilter_Filename filter_Filename = (AssetFilter_Filename)FilenameFilter.filter;
                if (filter_Filename.input != null && filter_Filename.input.Count == 0)
                {
                    // 优先使用filter设置的input参数
                    filter_Filename.input = paths;
                }
                paths = filter_Filename.DoFilter();
            }
            return paths;
        }

        /// <summary>
        /// 执行处理器
        /// </summary>
        /// <returns>返回执行处理器后的信息</returns>
        /// <exception cref="Exception"></exception>
        public List<string> DoProcessor()
        {
            if (Processor == null)
                throw new Exception($"EMPTY Asset Processor");

            List<string> paths = DoFilter();

            List<string> results = new List<string>();
            foreach (var path in paths)
            {
                string result = Processor.DoProcess(path);
                if (!string.IsNullOrEmpty(result))
                {
                    results.Add(result);
                }
            }
            return results;
        }

        static public string Serialize(AssetChecker checker)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.All;
            return JsonConvert.SerializeObject(checker, Formatting.Indented, settings);
        }

        static public AssetChecker Deserialize(string json)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.All;
            return JsonConvert.DeserializeObject<AssetChecker>(json, settings);
        }
    }

    public class AssetCheckerProcessor : OdinAttributeProcessor<AssetChecker>
    {
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {

        }
    }
}