using System.IO;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.AssetTreeView;

namespace Framework.AssetManagement.AssetProcess
{
    public class AssetProceesorWindow : EditorWindow
    {
        //
        public static List<Processor> processors = new List<Processor>();
        public static List<Rule> rules = new List<Rule>();
        public static List<Category> categories = new List<Category>();
        public static List<AssetFileTreeViewItem> assetFiles = new List<AssetFileTreeViewItem>();
        AssetTreeViewView<ProcessorTreeView, ProcessorTreeViewItem> m_ProccessorTreeViewView;
        AssetTreeViewView<RuleTreeView, RuleTreeViewItem> m_RuleTreeViewView;
        AssetTreeViewView<CategoryTreeView, CategoryTreeViewItem> m_CategoryTreeViewView;
        AssetTreeViewView<AssetFileTreeView, AssetFileTreeViewItem> m_AssetFileTreeViewView;
        //
        int selectedRuleIndex = -1;
        int selectedProcessorIndex = -1;
        int selectedCategoryIndex = -1;
        [MenuItem("Tools/Assets Management/Test AssetProcessorSettingWindow", false, 1)]
        public static void ShowWindow()
        {
            AssetProceesorWindow wnd = GetWindow<AssetProceesorWindow>();
            wnd.titleContent = new GUIContent("资产配置工具");
            wnd.Show();
            
        }
        Rect multiColumnTreeViewRect
        {
            get { return new Rect(20, 30, position.width - 40, position.height - 60); }
        }
        public void OnEnable()
        {
            m_ProccessorTreeViewView = new AssetTreeViewView<ProcessorTreeView,ProcessorTreeViewItem>(multiColumnTreeViewRect);
            m_RuleTreeViewView = new AssetTreeViewView<RuleTreeView, RuleTreeViewItem>(multiColumnTreeViewRect);
            m_CategoryTreeViewView = new AssetTreeViewView<CategoryTreeView, CategoryTreeViewItem>(multiColumnTreeViewRect);
            m_AssetFileTreeViewView = new AssetTreeViewView<AssetFileTreeView, AssetFileTreeViewItem>(multiColumnTreeViewRect);
            m_CategoryTreeViewView.AssetTreeView.OnRowSelected += OnSelectedCategory;
            m_RuleTreeViewView.AssetTreeView.OnRowSelected += OnSelectedRule;
            m_ProccessorTreeViewView.AssetTreeView.OnRowSelected += OnSelectedProccessor;

            SetTreeViewData();
        }
        void OnSelectedProccessor(int selectedRow)
        {
            selectedCategoryIndex = m_CategoryTreeViewView.AssetTreeView.selectedRowIndex;
            selectedRuleIndex = m_RuleTreeViewView.AssetTreeView.selectedRowIndex;
            selectedProcessorIndex = m_ProccessorTreeViewView.AssetTreeView.selectedRowIndex;

            List<string> paths = new List<string>();
            paths = m_RuleTreeViewView.AssetTreeView.GetCurrentPaths();
            List<FilterMode> modes = new List<FilterMode>();
            modes = m_ProccessorTreeViewView.AssetTreeView.GetCurrentModes();
            List<bool> includeSubFold = new List<bool>();
            includeSubFold = m_RuleTreeViewView.AssetTreeView.GetCurrentIncludeSubFold();
            List<string> matchFilePaths = new List<string>();

            matchFilePaths = AssetFileUtility.GetMatchFiles(paths, modes,includeSubFold);

            assetFiles.Clear();
            for(int i = 0; i < matchFilePaths.Count; i++)
            {
                string name = Path.GetFileName(matchFilePaths[i]); 
                AssetFileTreeViewItem vt = new AssetFileTreeViewItem(i,0,name,matchFilePaths[i]);
                vt.presetMatch = "To Check Match";
                assetFiles.Add(vt);
            }

            SetTreeViewData();
        }
        void OnSelectedCategory(int selectedRow)
        {
            rules = categories[selectedRow].Rules;
            if (rules.Count > 0)
                processors = rules[0].Processors;
            else
                processors = new List<Processor>();
            SetTreeViewData();
            m_RuleTreeViewView.AssetTreeView.SetItemSeleted(0);
            m_ProccessorTreeViewView.AssetTreeView.SetItemSeleted(0);
        }
        void OnSelectedRule(int seletedRow)
        {
            processors = rules[seletedRow].Processors;
            SetTreeViewData();
            m_ProccessorTreeViewView.AssetTreeView.SetItemSeleted(0);
        }
        void AddCategory()
        {
            Category category = new Category();
            category.Name = $"新规则类型{categories.Count}";
            categories.Add(category);

            SetTreeViewData();
        }
        void RemoveCategory()
        {
            if (categories.Count <=1)
            {
                categories.Clear();
                rules.Clear();
                processors.Clear();
                SetTreeViewData();
                return;
            }

            categories.RemoveAt(categories.Count - 1);
            if(m_CategoryTreeViewView.AssetTreeView.selectedRowIndex > categories.Count -1)
            {
                m_CategoryTreeViewView.AssetTreeView.SetItemSeleted(0);
                m_RuleTreeViewView.AssetTreeView.SetItemSeleted(0);
                m_ProccessorTreeViewView.AssetTreeView.SetItemSeleted(0);
                rules = categories[0].Rules;
                if (rules.Count > 0) { 
                    processors = rules[0].Processors;
                }
                else{
                    processors = new List<Processor>();
                }
            }
            SetTreeViewData();
        }

        void SetTreeViewData()
        {
            List<ProcessorTreeViewItem> data = new List<ProcessorTreeViewItem>();
            for(int i = 0; i < processors.Count; i++)
            {
                ProcessorTreeViewItem vt = new ProcessorTreeViewItem(i,0,processors[i].Name);
                vt.categories = categories;
                vt.processor = processors[i];
                data.Add(vt);
            }

            List<RuleTreeViewItem> ruleData = new List<RuleTreeViewItem>();
            for(int i = 0; i < rules.Count; i++)
            {
                RuleTreeViewItem vt = new RuleTreeViewItem(i,0,rules[i].Name);
                vt.categories = categories;
                vt.rule = rules[i];
                ruleData.Add(vt);
            }

            List<CategoryTreeViewItem> categoryData = new List<CategoryTreeViewItem>();
            for(int i = 0; i < categories.Count; i++)
            {
                CategoryTreeViewItem vt = new CategoryTreeViewItem(i,0,categories[i].Name);
                vt.category = categories[i];
                categoryData.Add(vt);
            }

            m_CategoryTreeViewView.UpdateData(categoryData);
            m_RuleTreeViewView.UpdateData(ruleData);
            m_ProccessorTreeViewView.UpdateData(data);
            m_AssetFileTreeViewView.UpdateData(assetFiles);
        }

        void SaveConfig()
        {
            RefreshDatasIndexs();
            jsonSeriTest.JsonSerialize(categories);
        }
        void LoadConfig()
        {
            categories = jsonSeriTest.JsonDeserializeTest();
            // start up
            if(categories.Count > 0)
            {
                rules = categories[0].Rules;
            }
            if(rules.Count > 0)
            {
                processors = rules[0].Processors;
            }
            SetTreeViewData();
            m_CategoryTreeViewView.AssetTreeView.SetItemSeleted(0);
            m_RuleTreeViewView.AssetTreeView.SetItemSeleted(0);
            m_ProccessorTreeViewView.AssetTreeView.SetItemSeleted(0);
        }
        void updateFileData()
        {
            if (selectedCategoryIndex != m_CategoryTreeViewView.AssetTreeView.selectedRowIndex ||
                selectedRuleIndex != m_RuleTreeViewView.AssetTreeView.selectedRowIndex ||
                selectedProcessorIndex != m_ProccessorTreeViewView.AssetTreeView.selectedRowIndex
            ){
                assetFiles.Clear();
                m_AssetFileTreeViewView.UpdateData(assetFiles);
            }
        }
        void RefreshDatasIndexs()
        {
            for(int c = 0; c < categories.Count; c++)
            {
                var category = categories[c];
                category.categoryIndex = c;
                for(int r = 0; r <category.Rules.Count; r++)
                {
                    var rule = category.Rules[r];
                    rule.categoryIndex = c;
                    rule.ruleIndex = r;
                    for(int p = 0 ; p < rule.Processors.Count; p++)
                    {
                        var processor = rule.Processors[p]; 
                        processor.data.categoryIndex = c;
                        processor.data.ruleIndex = r;
                        processor.data.processorIndex = p;
                    }
                }
            }
        }

        private void OnGUI()
        {
            updateFileData();
            int borderSize = 2;
            float height = 0;
            GUIStyle style = new GUIStyle( GUI.skin.button);
            style.fontSize = 22;
            height += 10;
            
            if(GUI.Button(new Rect(10, height, position.width / 2 - 20, 25), "读取配置"))
            {
                LoadConfig();   
            }
            if(GUI.Button(new Rect(position.width / 2, height, position.width / 2 - 120, 25), "保存配置")){
                SaveConfig();
            }
            if(GUI.Button(new Rect(position.width - 110, height, 90, 25), "Refresh"))
            {
                OnSelectedProccessor(0);
                RefreshDatasIndexs();
            }
            height += 30;

            float upHeight = 430;
            // category scroll
            float categoryWidth = position.width / 5;
            float categoryX = 10;
            float categoryH = position.height - height - 40;
            string categoryFitler = m_CategoryTreeViewView.AssetTreeView.fitlerStr; 
            string categotyNewFitlerStr = GUI.TextField(new Rect(categoryX, height, categoryWidth-70, 20),categoryFitler);
            if(categoryFitler != categotyNewFitlerStr)
            {
                m_CategoryTreeViewView.AssetTreeView.fitlerStr = categotyNewFitlerStr;
                rules = new List<Rule>();
                processors = new List<Processor>();
                assetFiles.Clear();
                var matchIndexs = m_CategoryTreeViewView.AssetTreeView.GetMatchSearchList();
                if(matchIndexs.Count > 0)
                {
                    OnSelectedCategory(matchIndexs[0]);
                }
            }
            if(GUI.Button(new Rect(categoryX + categoryWidth-70 + 5, height, 30, 20), "+", style))
            {
                AddCategory();
            }
            if(GUI.Button(new Rect(categoryX + categoryWidth-70 + 35, height, 30, 20), "-", style))
            {
                RemoveCategory();
            }
            m_CategoryTreeViewView.DrawGUI(new Rect(categoryX, height + 25, categoryWidth, categoryH));
            // rule scroll
            float ruleWidth = position.width / 3;
            float ruleX = categoryX + categoryWidth + 10;
            float ruleH = upHeight - 25;
            m_RuleTreeViewView.DrawGUI(new Rect(ruleX, height, ruleWidth, ruleH));
            string ruleFitlerStr = m_RuleTreeViewView.AssetTreeView.fitlerStr; 
            string ruleNewFitlerStr =GUI.TextField(new Rect(ruleX, height + ruleH + 5, ruleWidth-70, 20),ruleFitlerStr);
            //SetTreeViewData();
            if (ruleFitlerStr != ruleNewFitlerStr)
            {
                m_RuleTreeViewView.AssetTreeView.fitlerStr = ruleNewFitlerStr;
                processors = new List<Processor>();
                assetFiles.Clear();
                var matchIndexs = m_RuleTreeViewView.AssetTreeView.GetMatchSearchList();
                if(matchIndexs.Count > 0)
                {
                    OnSelectedRule(matchIndexs[0]);
                }
            }
            if(GUI.Button(new Rect(ruleX + ruleWidth-70 + 5, height + ruleH + 5, 30, 20), "+", style))
            {
                if (categories.Count > 0)
                {
                    categories[m_CategoryTreeViewView.AssetTreeView.selectedRowIndex].AddRule();
                    rules = categories[m_CategoryTreeViewView.AssetTreeView.selectedRowIndex].Rules;

                    m_RuleTreeViewView.AssetTreeView.SetItemSeleted(0);
                    SetTreeViewData();
                }
            }
            if (GUI.Button(new Rect(ruleX + ruleWidth - 70 + 35, height + ruleH + 5, 30, 20), "-", style))
            {
                if (categories.Count > 0)
                {
                    if (m_RuleTreeViewView.AssetTreeView.selectedRowIndex == rules.Count - 1)
                    {
                        m_RuleTreeViewView.AssetTreeView.SetItemSeleted(0);
                    }
                    categories[m_CategoryTreeViewView.AssetTreeView.selectedRowIndex].RemoveRule(rules.Count - 1);
                    rules = categories[m_CategoryTreeViewView.AssetTreeView.selectedRowIndex].Rules;

                    if (rules.Count > 0)
                    {
                        processors = rules[m_RuleTreeViewView.AssetTreeView.selectedRowIndex].Processors;
                    }
                    else
                    {
                        processors = new List<Processor>();
                    }
                    SetTreeViewData();
                }
            }

            // processor scroll
            float processorWidth = position.width - ruleWidth - categoryWidth - 40;
            float processorX =ruleX + ruleWidth + 10;   
            float precessorH = upHeight - 25;
            m_ProccessorTreeViewView.DrawGUI(new Rect(processorX, height, processorWidth, precessorH));
            string processorFitlerStr = m_ProccessorTreeViewView.AssetTreeView.fitlerStr;
            string processorFitlerNewStr =GUI.TextField(new Rect(processorX, height +precessorH + 5, processorWidth-70, 20),processorFitlerStr);
            if(processorFitlerStr != processorFitlerNewStr)
            {
                m_ProccessorTreeViewView.AssetTreeView.fitlerStr = processorFitlerNewStr;
                assetFiles.Clear();
                var matchIndexs = m_ProccessorTreeViewView.AssetTreeView.GetMatchSearchList();
                if(matchIndexs.Count > 0)
                {
                    OnSelectedProccessor(matchIndexs[0]);
                }
            }
            if(GUI.Button(new Rect(processorX + processorWidth-70 + 5, height +precessorH + 5, 30, 20), "+", style))
            {
                if (categories[m_CategoryTreeViewView.AssetTreeView.selectedRowIndex].Rules.Count > 0)
                {
                    categories[m_CategoryTreeViewView.AssetTreeView.selectedRowIndex].Rules[m_RuleTreeViewView.AssetTreeView.selectedRowIndex].AddProcessor();
                    processors = categories[m_CategoryTreeViewView.AssetTreeView.selectedRowIndex].Rules[m_RuleTreeViewView.AssetTreeView.selectedRowIndex].Processors;

                    m_ProccessorTreeViewView.AssetTreeView.SetItemSeleted(0);
                    SetTreeViewData();
                }
            }
            if(GUI.Button(new Rect(processorX + processorWidth-70 + 35, height +precessorH + 5, 30, 20), "-", style))
            {
                if (categories[m_CategoryTreeViewView.AssetTreeView.selectedRowIndex].Rules.Count > 0)
                {
                    categories[m_CategoryTreeViewView.AssetTreeView.selectedRowIndex].Rules[m_RuleTreeViewView.AssetTreeView.selectedRowIndex].RemoveProcessor(processors.Count - 1);
                    processors = categories[m_CategoryTreeViewView.AssetTreeView.selectedRowIndex].Rules[m_RuleTreeViewView.AssetTreeView.selectedRowIndex].Processors;
                    SetTreeViewData();
                }
            }

            height += upHeight + 5;
            // AssetFile scroll
            float assetFileWidth = position.width - categoryWidth -30;
            float assetFileX =ruleX;   
            GUI.Label(new Rect(assetFileX, height, assetFileWidth, 30),"当前选中的处理器所匹配的资源");
            height += 30;

            float assetFileH =position.height - height - 10;
            m_AssetFileTreeViewView.DrawGUI(new Rect(assetFileX, height, assetFileWidth, assetFileH));

        }
    }
}