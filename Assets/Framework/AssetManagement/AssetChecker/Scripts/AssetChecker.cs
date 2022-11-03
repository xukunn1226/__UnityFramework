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
using System.Linq;
using UnityEditor.Build.Pipeline;
using System.Text;
using UnityEngine.Windows;
using System.IO;

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
        public string           Desc;
        [HideInInspector]
        public AssetFilterInfo  PathFilter;
        [HideInInspector]
        public AssetFilterInfo  FilenameFilter;
        [HideInInspector]
        public IAssetProcessor  Processor;

        public AssetChecker()
        {
            PathFilter = AssetFilterInfo.Create<AssetFilter_Path>();
            FilenameFilter = AssetFilterInfo.Create<AssetFilter_Filename>();
        }

        static public AssetChecker Create(AssetChecker other)
        {
            AssetChecker checker = new AssetChecker();
            checker.Desc = other.Desc;            
            checker.PathFilter.enabled = other.PathFilter.enabled;
            checker.PathFilter.filter = AssetFilter_Path.Create((AssetFilter_Path)other.PathFilter.filter);
            checker.FilenameFilter.enabled = other.FilenameFilter.enabled;
            checker.FilenameFilter.filter = AssetFilter_Filename.Create((AssetFilter_Filename)other.FilenameFilter.filter);
            return checker;
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
                List<string> prevInput = filter_Filename.input;
                if (filter_Filename.input != null && filter_Filename.input.Count == 0)
                {
                    // 优先使用filter设置的input参数
                    filter_Filename.input = paths;
                }
                paths = filter_Filename.DoFilter();
                filter_Filename.input = prevInput;
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

        public string Export()
        {
            if (m_Results.Count == 0)
                return null;

            var csv = new StringBuilder();
            foreach (var ret in m_Results)
            {
                csv.AppendLine(ret);
            }

            string directory = "Assets/Temp";
            if (!System.IO.Directory.Exists(directory))
                System.IO.Directory.CreateDirectory(directory);

            string error = null;
            string assetPath = string.Format($"{directory}/{Desc}_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm")}.csv");
            try
            {
                System.IO.File.WriteAllText(assetPath, csv.ToString(), Encoding.UTF8);
                AssetDatabase.ImportAsset(assetPath);
            }
            catch (Exception e)
            {
                error = string.Format($"{e.Message}  {assetPath}");
            }

            return error;
        }

        public void DoProcessorAndExport()
        {
            m_Results = DoProcessor();
            Export();
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



        #region ///////////////////////////// GUI

        private PropertyTree m_PathFilterPropertyTree;
        private PropertyTree m_FilenameFilterPropertyTree;
        private PropertyTree m_ProcessorPropertyTree;
        private bool m_Visible;        

        [OnInspectorGUI]
        [PropertyOrder(0)]
        private void OnDrawFilters()
        {
            EditorGUILayout.Space();

            // draw Path Filter
            if (m_PathFilterPropertyTree == null)
            {
                m_PathFilterPropertyTree = PropertyTree.Create(PathFilter.filter);
            }
            SirenixEditorGUI.BeginToggleGroup(PathFilter, ref PathFilter.enabled, ref m_Visible, "【路径过滤器】");
            m_PathFilterPropertyTree.Draw(false);
            SirenixEditorGUI.EndToggleGroup();

            EditorGUILayout.Space();

            // draw Filename Filter
            if (m_FilenameFilterPropertyTree == null)
            {
                m_FilenameFilterPropertyTree = PropertyTree.Create(FilenameFilter.filter);
            }
            SirenixEditorGUI.BeginToggleGroup(FilenameFilter, ref FilenameFilter.enabled, ref m_Visible, "【文件名过滤器】");
            m_FilenameFilterPropertyTree.Draw(false);
            SirenixEditorGUI.EndToggleGroup();
        }

        [OnInspectorGUI]
        [PropertyOrder(2)]
        private void OnDrawProcessor()
        {
            EditorGUILayout.Space();

            if (m_ProcessorPropertyTree == null && Processor != null)
            {
                m_ProcessorPropertyTree = PropertyTree.Create(Processor);
            }

            if (Processor != null)
            {
                m_ProcessorPropertyTree?.Draw(false);
            }
        }

        [PropertyOrder(1)]
        [BoxGroup("【处理器】")]
        [HorizontalGroup("【处理器】/LayerHor", 0.3f)]
        [GUIColor(0, 1, 0)]
        [Button("添加处理器")]
        private void AddProcessor()
        {
            Processor = Activator.CreateInstance(Type.GetType(m_SelectedComponentName)) as IAssetProcessor;
            m_ProcessorPropertyTree = null;
        }

        [PropertyOrder(1)]
        [BoxGroup("【处理器】")]
        [HorizontalGroup("【处理器】/LayerHor", 0.6f)]
        [SerializeField]
        [ValueDropdown("DisplayNameList")]
        [LabelText("选择处理器")]
        [JsonProperty]
        private string m_SelectedComponentName = "UnSelected Processor";

        static private IEnumerable DisplayNameList()
        {
            return ComponentNameList.Select(x => new ValueDropdownItem(GetDisplayName(x), x) );
        }
        
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

        static private string GetDisplayName(string str)
        {
            int idx = str.LastIndexOf(".");
            string name = str;

            if (idx != -1)
            {
                name = str.Substring(idx + 1);
            }
            return name;
        }

        [ShowInInspector]
        [PropertyOrder(5)]
        [LabelText("显示结果：")]
        [ListDrawerSettings(IsReadOnly = true, ShowIndexLabels = true, ShowItemCount = true, ShowPaging = false)]
        private List<string> m_Results = new List<string>();

        private string m_Info;


        [PropertyOrder(6)]
        [OnInspectorGUI]
        private void OnShowInfo()
        {
            EditorGUILayout.HelpBox(m_Info, MessageType.Info);
        }

        [PropertyOrder(7)]
        [Button("导出")]
        private void btnExport()
        {
            string error = Export();
            if (string.IsNullOrEmpty(error))
            {
                m_Info = $"资源检测结果导出完成：Assets/Temp/{Desc}_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm")}.csv";
                Debug.Log(m_Info);
            }                
            else
            {
                m_Info = error;
                Debug.LogError(error);
            }                
        }

        [Button("执行过滤器")]
        [PropertyOrder(3)]
        [HorizontalGroup("ButtonLayer")]
        private void btnDoFilter()
        {
            m_Results = DoFilter();
        }

        [Button("执行处理器")]
        [PropertyOrder(3)]
        [HorizontalGroup("ButtonLayer")]
        private void btnDoProcessor()
        {
            m_Results = DoProcessor();
        }
        #endregion
    }

    public class AssetCheckerProcessor : OdinAttributeProcessor<AssetChecker>
    {
        public override bool CanProcessSelfAttributes(InspectorProperty property)
        {
            //Debug.Log($"{property.Name}");
            return true;
        }

        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            //Debug.Log($"{member.Name}   {parentProperty.Name}   {parentProperty.ParentType}");
        }
    }
}