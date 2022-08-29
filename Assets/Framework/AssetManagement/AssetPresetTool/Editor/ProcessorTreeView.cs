using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.AssetTreeView;
using UnityEditor.IMGUI.Controls;
using UnityEditor;
using UnityEditor.Presets;

namespace Framework.AssetManagement.AssetProcess
{
    public class ProcessorTreeViewItem : BaseTreeViewItem
    {
        public List<Category> categories = new List<Category>();
        public Processor processor;
        public bool FitlerIsFolded = false;
        public ProcessorTreeViewItem() { }
        public ProcessorTreeViewItem(int id, int depth, string name) : base(id, depth, name)
        {
        }

        public override MultiColumnHeaderState GetMutiColumnHeader(float width)
        {
            var colums = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("处理器名字"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 40,
                    minWidth = 30,
                    autoResize = true,
                    canSort = true,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("处理器筛选设置"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 90,
                    minWidth = 90,
                    autoResize = true,
                    canSort = true,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("处理器设置"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 70,
                    minWidth = 60,
                    autoResize = true,
                    canSort = true,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("执行"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 40,
                    minWidth = 30,
                    autoResize = true,
                    canSort = true,
                    allowToggleVisibility = false
                },

            };
            var state = new MultiColumnHeaderState(colums);
            return state;
        }
    }
    public class ProcessorTreeView : BaseAssetTreeView<ProcessorTreeViewItem>
    {
        public int selectedRowIndex;
        public string fitlerStr = "";
        public Action<int> OnRowSelected;
        public List<Type> ProceeosrTypes = new List<Type>();
        public ProcessorTreeView(TreeViewState treeViewState, MultiColumnHeader multiColumnHeader) : base(treeViewState, multiColumnHeader)
        {
            ProceeosrTypes.Clear();
            var res = from type in Assembly.GetExecutingAssembly().GetTypes() where type.IsSubclassOf(typeof(ProcessorBaseData)) select type;
            foreach (var type in res)
            {
                Debug.Log($"types: {type.Name}");
                ProceeosrTypes.Add(type);
            }

        }
        public List<FilterMode> GetCurrentModes()
        {
            List<FilterMode> modes = new List<FilterMode>();
            var searchList = GetMatchSearchList();
            int itemIndex = 0;
            for (int i = 0; i < searchList.Count; i++)
            {
                if (searchList[i] == selectedRowIndex)
                {
                    itemIndex = i;
                    break;
                }
            }
            if (m_itemData.Count > 0)
            {
                modes = m_itemData[itemIndex].data.processor.filterModes;
            }
            return modes;
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            var searchList = GetMatchSearchList();
            int firstRow = selectedIds[0];

            selectedRowIndex = firstRow;
            OnRowSelected?.Invoke(selectedRowIndex);
            base.SelectionChanged(selectedIds);
        }
        public void SetItemSeleted(int index)
        {
            var searchList = GetMatchSearchList();
            int realIndex = index;
            SetSelection(new List<int> { realIndex });
            selectedRowIndex = realIndex;
            OnRowSelected?.Invoke(selectedRowIndex);
        }
        public override void GetData(List<ProcessorTreeViewItem> datas)
        {
            m_itemData = new List<TreeViewItem<ProcessorTreeViewItem>>();
            foreach (var item in datas)
            {
                if (itemNameFitler(item.DisPlayName))
                {
                    TreeViewItem<ProcessorTreeViewItem> tv = new TreeViewItem<ProcessorTreeViewItem>(item.mId, item.depth, item.DisPlayName, item);
                    m_itemData.Add(tv);
                }
            }
            Reload();
        }
        bool itemNameFitler(string name)
        {
            if (string.IsNullOrEmpty(name))
                return true;
            if (name.Contains(fitlerStr))
            {
                return true;
            }
            return false;
        }

        public List<int> GetMatchSearchList()
        {
            List<int> res = new List<int>();
            for (int i = 0; i < m_itemData.Count; i++)
            {
                if (itemNameFitler(m_itemData[i].displayName))
                {
                    res.Add(i);
                }
            }
            return res;
        }

        protected override float GetCustomRowHeight(int row, TreeViewItem item)
        {
            int count = m_itemData[row].data.processor.filterModes.Count;
            if (m_itemData[row].data.FitlerIsFolded)
            {
                count = 0;
            }
            float height = count * 24 + 50;
            height = Math.Max(80, height);
            return height;
        }
        public void OnAddFitleMode(int itemIndex)
        {
            m_itemData[itemIndex].data.processor.AddFilterMode();
            Reload();
        }
        public void OnRemoveFitleMode(int itemIndex)
        {
            m_itemData[itemIndex].data.processor.RemoveFilterModeAtLast();
            Reload();
        }

        protected override void DrawRow(Rect cellRect, TreeViewItem<ProcessorTreeViewItem> item, int column, ref RowGUIArgs args)
        {
            switch (column)
            {
                case 0:
                    cellRect.y -= GetCustomRowHeight(args.row, item) / 2 - 20;
                    string name = item.data.processor.Name;
                    cellRect.x = cellRect.x + 10;
                    cellRect.width /= 2;
                    cellRect.width += 40;
                    item.data.processor.Name = GUI.TextField(cellRect, name);
                    cellRect.y += 20;
                    EditorGUI.LabelField(cellRect, "处理器类型选择");
                    cellRect.y += 20;
                    var typeNames = from type in ProceeosrTypes select (string)type.GetField("DisplayName").GetValue(null);
                    int currentTypeIndex = ProceeosrTypes.FindIndex((type) =>type == item.data.processor.data.GetType());
                    int newTypeIndex = EditorGUI.Popup(cellRect, currentTypeIndex, typeNames.ToArray());
                    if(newTypeIndex != currentTypeIndex)
                    {
                        var newType = ProceeosrTypes[newTypeIndex];
                        item.data.processor.data = (ProcessorBaseData)Activator.CreateInstance(newType);
                    }
                    break;
                case 1:
                    cellRect.y -= GetCustomRowHeight(args.row, item) / 2 - 20;
                    //
                    var listRect = cellRect;
                    GUI.Label(listRect, "处理器筛选设置");
                    listRect.width = 20;
                    listRect.x = cellRect.x + cellRect.width - 90;
                    if (GUI.Button(listRect, "+"))
                    {
                        OnAddFitleMode(args.row);
                    }
                    listRect.x += 20;
                    if (GUI.Button(listRect, "-"))
                    {
                        OnRemoveFitleMode(args.row);
                    }
                    listRect.x += 20;
                    listRect.width = 50;
                    bool isFolded = item.data.FitlerIsFolded;
                    if (GUI.Button(listRect, isFolded ? "展开" : "折叠"))
                    {
                        item.data.FitlerIsFolded = !item.data.FitlerIsFolded;
                        isFolded = item.data.FitlerIsFolded;
                        Reload();
                    }
                    cellRect.y += 15;
                    //
                    string[] modeOptions = FilterMode.GetAllEnumStr();
                    int fitlers = item.data.processor.filterModes.Count;
                    cellRect.y += 15;
                    if (!isFolded)
                    {
                        for (int i = 0; i < fitlers; i++)
                        {
                            var filerRect = cellRect;
                            filerRect.width /= 4;
                            EditorGUI.LabelField(filerRect, $"条件{i}");
                            filerRect.width = cellRect.width / 3 + 20;
                            filerRect.x = cellRect.x + cellRect.width - 2 * filerRect.width;

                            int currentIndex = (int)item.data.processor.filterModes[i].filterMode;
                            int afterPopIndex = EditorGUI.Popup(filerRect, currentIndex, modeOptions);
                            item.data.processor.filterModes[i].filterMode = (FilterModeEnum)afterPopIndex;
                            filerRect.x += filerRect.width;
                            item.data.processor.filterModes[i].applyStr = EditorGUI.TextField(filerRect, item.data.processor.filterModes[i].applyStr);
                            //
                            cellRect.y += 24;
                        }
                    }

                    break;
                case 2:
                    cellRect.y -= GetCustomRowHeight(args.row, item) / 2 - 10;
                    GUI.Label(cellRect, "处理器属性");
                    cellRect.y += 15;
                    cellRect.y += 15;
                    // 处理器绘制
                    item.data.processor.data.ProceeosrDraw(cellRect, item, column);

                    break;
                case 3:
                    cellRect.y -= GetCustomRowHeight(args.row, item) / 2 - 20;
                    if (GUI.Button(cellRect, "执行"))
                    {
                        int categoryIndex = item.data.processor.data.categoryIndex;
                        item.data.processor.data.OnProcessorApply(item.data.categories[categoryIndex]);
                    }
                    break;
            }
        }
        
    }
}
