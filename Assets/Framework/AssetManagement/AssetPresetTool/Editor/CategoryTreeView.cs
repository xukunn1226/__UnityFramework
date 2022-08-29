using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.AssetTreeView;
using UnityEditor.IMGUI.Controls;
using UnityEditor;

namespace Framework.AssetManagement.AssetProcess
{
    public class CategoryTreeViewItem : BaseTreeViewItem
    {
        public Category category;
        public CategoryTreeViewItem() { }
        public CategoryTreeViewItem(int id, int depth, string name):base(id,depth,name){ }
        public override MultiColumnHeaderState GetMutiColumnHeader(float width)
        {
            var colums = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("规则类型"),
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
    public class CategoryTreeView : BaseAssetTreeView<CategoryTreeViewItem>
    {
        public int selectedRowIndex;
        public string fitlerStr="";
        public Action<int> OnRowSelected;
        public Action<int> OnRowMatched;
        public CategoryTreeView(TreeViewState treeViewState, MultiColumnHeader multiColumnHeader) : base(treeViewState, multiColumnHeader)
        {
            columnIndexForTreeFoldouts = 0;
        }
        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
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
        }

        public override void GetData(List<CategoryTreeViewItem> datas)
        {
            m_itemData = new List<TreeViewItem<CategoryTreeViewItem>>();
            foreach(var item in datas)
            {
                if (itemNameFitler(item.DisPlayName))
                {
                    TreeViewItem<CategoryTreeViewItem> tv = new TreeViewItem<CategoryTreeViewItem>(item.mId, item.depth, item.DisPlayName, item);
                    m_itemData.Add(tv);
                }
            }
            Reload();
        }
        public List<int> GetMatchSearchList()
        {
            List<int> res = new List<int>();
            for(int i = 0;i<m_itemData.Count;i++)
            {
                if (itemNameFitler(m_itemData[i].displayName))
                {
                    res.Add(i);
                }
            }
            return res;
        }
        bool itemNameFitler(string name)
        {
            if (string.IsNullOrEmpty(name))
                return true;
            if(name.Contains(fitlerStr))
            {
                return true;
            }
            return false;
        }
        protected override float GetCustomRowHeight(int row, TreeViewItem item)
        {
            return 36;
        }

        protected override void DrawRow(Rect cellRect, TreeViewItem<CategoryTreeViewItem> item, int column, ref RowGUIArgs args)
        {
            switch (column)
            {
                case 0:
                    cellRect.y -= GetCustomRowHeight(args.row, item) /2  - 20;
                    string name = item.data.category.Name;
                    cellRect.x = cellRect.x + 10;
                    cellRect.width /= 2;
                    cellRect.width += 30;
                    GUI.Label(cellRect, "名称");
                    cellRect.x = cellRect.x + cellRect.width/2 - 20;
                    item.data.category.Name = GUI.TextField(cellRect, name);
                    break;
                case 1:
                    cellRect.y -= GetCustomRowHeight(args.row, item) /2  - 20;
                    cellRect.x += 40;
                    cellRect.width -= 40;
                    if(GUI.Button(cellRect, "执行"))
                    {
                        item.data.category.OnCategoryProcessorsApplyAll();
                    }
                    break;
            }

        }
    }
}
