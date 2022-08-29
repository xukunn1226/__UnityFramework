using Framework.AssetManagement.AssetTreeView;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor.IMGUI.Controls;

namespace Framework.AssetManagement.AssetProcess
{
    [Serializable]
    public class Category 
    {
        public string Name;
        public int categoryIndex;
        public List<Rule> Rules = new List<Rule>();
        public void AddRule()
        {
            Rule NewRule = new Rule();
            NewRule.Name = $"�¹���{Rules.Count}";
            NewRule.OnAddPaths("");
            NewRule.categoryIndex = categoryIndex;
            NewRule.ruleIndex = Rules.Count;
            Rules.Add(NewRule);
        }
        public void RemoveRule(int index)
        {
            if (Rules.Count > 0 && index < Rules.Count)
            {
                Rules.RemoveAt(index);
            }
        }
        public void OnCategoryProcessorsApplyAll()
        {
            foreach(var rule in Rules)
            {
                Debug.Log($"<color=red>Category</color> : {Name}");
                rule.OnRuleProcessorsApplyAll(this);
            }
        }
    }

    [Serializable]
    public class Rule 
    {
        public string Name;
        public int categoryIndex;
        public int ruleIndex;
        public List<string> Paths = new List<string>();
        public List<bool> includeSubFold = new List<bool>();
        public List<Processor> Processors = new List<Processor>();
        // method
        public void OnRuleProcessorsApplyAll(Category category)
        {
            foreach(var processor in Processors)
            {
                Debug.Log($"<color=Blue>Rule</color> : {Name} <color=green>Pro</color> : {processor.Name}");
                processor.data.OnProcessorApply(category);
            }
        }
        public void AddProcessor()
        {
            Processor NewProcessor = new Processor();
            NewProcessor.Name = $"�´�����{Processors.Count}";
            NewProcessor.AddFilterMode();
            NewProcessor.data.categoryIndex = categoryIndex;
            NewProcessor.data.ruleIndex = ruleIndex;
            NewProcessor.data.processorIndex = Processors.Count;
            Processors.Add(NewProcessor);
        }
        public void RemoveProcessor(int index)
        {
            if (Processors.Count > 0 && index < Processors.Count )
            {
                Processors.RemoveAt(index);
            }
        }
        public void OnAddPaths(string newPath)
        {
            Paths.Add(newPath);
            includeSubFold.Add(false);
        }
        public void OnRemovePathsAtLast()
        {
            if (Paths.Count > 0)
            {
                Paths.RemoveAt(Paths.Count - 1);
                includeSubFold.RemoveAt(Paths.Count - 1);
            }
        }
    }

    [Serializable]
    public class Processor 
    {
        public string Name;

        public List<FilterMode> filterModes = new List<FilterMode>();
        public ProcessorBaseData data;
        public Processor()
        {
            // defalut processor
            data = new PresetProcessor();
        }
        public void AddFilterMode()
        {
            FilterMode NewFilterMode = new FilterMode();
            NewFilterMode.filterMode = FilterModeEnum.NoConditon;
            NewFilterMode.applyStr = "";
            filterModes.Add(NewFilterMode); 
        }
        public void RemoveFilterModeAtLast()
        {
            if(filterModes.Count > 0)
                filterModes.RemoveAt(filterModes.Count-1);
        }
    }

    [Serializable]
    public class FilterMode
    {
        public FilterModeEnum filterMode;
        public string applyStr;

        public static string GetModeInfo(FilterModeEnum mode)
        {
            string res = "";
            switch (mode)
            {
                case FilterModeEnum.MustContain:
                    res = "���ֱ�������ַ���";
                    break;
                case FilterModeEnum.CanNotContain:
                    res = "���ֲ��ܰ���";
                    break;
                case FilterModeEnum.MustEndWith:
                    res = "�����Ը�ֵ��β";
                    break;
                case FilterModeEnum.NoConditon:
                    res = "�Ը�ֵ��ͷ";
                    break;
                case FilterModeEnum.BeginWith:
                    res = "��ɸѡ����";
                    break;
                case FilterModeEnum.AssetType:
                    res = "��Դ����";
                    break;
            }
            return res;
        }
        public static string[] GetAllEnumStr()
        {
            string[] names = Enum.GetNames(typeof(FilterModeEnum));
            string[] res = new string[names.Length];
            for(int i = 0; i< res.Length; i++)
            { 
                res[i] = GetModeInfo((FilterModeEnum)i);
            }
            return res;
        }

        public bool isMatch(string target)
        {
            if (filterMode == FilterModeEnum.NoConditon) return true;
            if (filterMode == FilterModeEnum.AssetType) return true;
            if (string.IsNullOrEmpty(applyStr)) return true;

            string pattern = GetRealFilterStringByModeAndString(applyStr, filterMode);
            if (string.IsNullOrEmpty(pattern)) return true;

            bool match = Regex.IsMatch(target, pattern);

            return match;
        }

        public bool isAssetFitlerMode()
        {
            return filterMode == FilterModeEnum.AssetType ? true : false;
        }

        public static string GetRealFilterStringByModeAndString(string patternValue, FilterModeEnum mode)
        {
            string res = "";
            switch (mode)
            {
                case FilterModeEnum.MustContain:
                    res = $".*({patternValue}).*";
                    break;
                case FilterModeEnum.CanNotContain:
                    res = $"^((?!{patternValue}).)*$";
                    break;
                case FilterModeEnum.MustEndWith:
                    res = $".*({patternValue})$";
                    break;

                case FilterModeEnum.BeginWith:
                    res = $"^({patternValue}).*$";
                    break;
                // case CommomnFilterMode.CanNotContainAndCanNotEndWith:
                //     res = $"^((?!{str1}).)*(?<!{str2})$";
                //     break;
                case FilterModeEnum.NoConditon:
                    res = "";
                    break;
                case FilterModeEnum.AssetType:
                    res = $"t:{patternValue}";
                    break;
            }
            return res;
        }


    }
    [Serializable]
    public enum FilterModeEnum
    {
        NoConditon = 0,
        CanNotContain = 1,
        MustContain = 2,
        MustEndWith = 3,
        AssetType = 4,
        BeginWith = 5,
    }

    // processor data
    [Serializable]
    public class ProcessorBaseData :IProcessorApply
    {
        public static string DisplayName = "������";
        public string ProcessorType;

        public int categoryIndex;
        public int ruleIndex;
        public int processorIndex;

        public virtual void OnProcessorApply(Category category)
        {
            Debug.Log("On processor base");
        }

        public virtual void ProceeosrDraw(Rect cellRect, TreeViewItem<ProcessorTreeViewItem> item, int column)
        {
            GUI.Label(cellRect, "no gui implement");
        }
    }

    public interface IProcessorApply
    {
        public void OnProcessorApply(Category category);
    }
}
