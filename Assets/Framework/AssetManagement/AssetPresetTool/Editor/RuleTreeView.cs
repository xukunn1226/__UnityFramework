using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.AssetTreeView;
using UnityEditor.IMGUI.Controls;
using UnityEditor;

namespace Framework.AssetManagement.AssetProcess
{
    public class RuleTreeViewItem : BaseTreeViewItem
    {
        public List<Category> categories = new List<Category>();
        public Rule rule;
        public bool PathIsFolded = false;
        public RuleTreeViewItem() { }
        public RuleTreeViewItem(int id, int depth, string name):base(id,depth,name){ }
        public override MultiColumnHeaderState GetMutiColumnHeader(float width)
        {
            var colums = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("规则名称"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 50,
                    minWidth = 30,
                    autoResize = true,
                    canSort = true,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("规则应用路径"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 200,
                    minWidth = 200,
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
                    width = 30,
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
    public class RuleTreeView : BaseAssetTreeView<RuleTreeViewItem>
    {
        public int selectedRowIndex;
        public string fitlerStr="";
        public Action<int> OnRowSelected;
        public Action<int> OnRowMatched;
        public RuleTreeView(TreeViewState treeViewState, MultiColumnHeader multiColumnHeader) : base(treeViewState, multiColumnHeader)
        {
        }
        public List<bool> GetCurrentIncludeSubFold()
        {
            List<bool> include = new List<bool>();
            var searchList = GetMatchSearchList();
            int itemIndex = 0;
            for(int i = 0;i< searchList.Count;i++ )
            {
                if(searchList[i] == selectedRowIndex)
                {
                    itemIndex = i;
                    break;
                }
            }
            if (m_itemData.Count > 0)
            {
                include =new List<bool>( m_itemData[itemIndex].data.rule.includeSubFold );
            }
            return include;
        }
        public List<string> GetCurrentPaths()
        {
            List<string> paths = new List<string>();
            var searchList = GetMatchSearchList();
            int itemIndex = 0;
            for(int i = 0;i< searchList.Count;i++ )
            {
                if(searchList[i] == selectedRowIndex)
                {
                    itemIndex = i;
                    break;
                }
            }
            if (m_itemData.Count > 0)
            {
                paths =new List<string>( m_itemData[itemIndex].data.rule.Paths );
            }
            return paths;
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
        public override void GetData(List<RuleTreeViewItem> datas)
        {
            m_itemData = new List<TreeViewItem<RuleTreeViewItem>>();
            foreach (var item in datas)
            {
                if (itemNameFitler(item.DisPlayName))
                {
                    TreeViewItem<RuleTreeViewItem> tv = new TreeViewItem<RuleTreeViewItem>(item.mId, item.depth, item.DisPlayName, item);
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
            int count = m_itemData[row].data.rule.Paths.Count;
            if(m_itemData[row].data.PathIsFolded)
            {
                count = 0;
            }
            float height = count * 24 + 50; 
            return height;
        }
        private void OnAddItem(int itemIndex)
        {
            m_itemData[itemIndex].data.rule.OnAddPaths("");
            Reload();
        }
        private void OnRemoveItem(int itemIndex)
        {
            m_itemData[itemIndex].data.rule.OnRemovePathsAtLast();
            Reload();
        }
        protected override void DrawRow(Rect cellRect, TreeViewItem<RuleTreeViewItem> item, int column, ref RowGUIArgs args)
        {
            switch (column)
            {
                case 0:
                    cellRect.y -= GetCustomRowHeight(args.row, item) / 2 - 20;
                    string name = item.data.rule.Name;
                    cellRect.x = cellRect.x + 5;
                    cellRect.width /= 2;
                    cellRect.width += 30;
                    item.data.rule.Name = GUI.TextField(cellRect, name);
                    break;
                case 1:
                    cellRect.y -= GetCustomRowHeight(args.row, item) / 2 - 20;
                    //
                    var listRect = cellRect;
                    GUI.Label(listRect, "路径筛选设置");
                    listRect.width = 20;
                    listRect.x = cellRect.x + cellRect.width - 90;
                    if (GUI.Button(listRect, "+"))
                    {
                        OnAddItem(args.row);
                    }
                    listRect.x += 20;
                    if (GUI.Button(listRect, "-"))
                    {
                        OnRemoveItem(args.row);
                    }
                    listRect.x += 20;
                    listRect.width = 50;
                    bool isFolded = item.data.PathIsFolded;
                    if (GUI.Button(listRect, isFolded ? "展开" : "折叠")) {
                        item.data.PathIsFolded = !item.data.PathIsFolded;
                        isFolded = item.data.PathIsFolded;
                        Reload();
                    }
                    cellRect.y += 15;
                    //
                    int pathCount = item.data.rule.Paths.Count;
                    cellRect.y += 15;
                    //
                    if (!isFolded)
                    {
                        for (int i = 0; i < pathCount; i++)
                        {
                            var filerRect = cellRect;
                            var LabelRect = cellRect;
                            string theControlName = $"folderPath_{RulePathIndex}";
                            string preControlName = $"prefolderPath_{RulePathIndex}";

                            GUI.SetNextControlName(preControlName);
                            EditorGUI.LabelField(LabelRect, $"路径{i}");

                            filerRect.x = cellRect.x + cellRect.width / 2 - 120;
                            filerRect.width = cellRect.width / 2 + 10;
                            RulePathIndex++;

                            GUI.SetNextControlName(theControlName);
                            item.data.rule.Paths[i] = EditorGUI.TextField(filerRect, item.data.rule.Paths[i]);

                            filerRect.x = cellRect.x + cellRect.width - 100;
                            filerRect.width = 100;
                            item.data.rule.includeSubFold[i] = EditorGUI.ToggleLeft(filerRect, "包含子文件夹", item.data.rule.includeSubFold[i]);

                            if (GUI.GetNameOfFocusedControl() == theControlName)
                            {
                                string currentPath = item.data.rule.Paths[i];
                                currentPath = string.IsNullOrEmpty(currentPath) ? UnityEngine.Application.dataPath : currentPath;
                                string newPath = EditorUtility.OpenFolderPanel("选择路径", currentPath, "");
                                if (newPath.Contains(UnityEngine.Application.dataPath))
                                {
                                    newPath = newPath.Remove(0, UnityEngine.Application.dataPath.Length);
                                    newPath = "Assets" + newPath;
                                }
                                else
                                {
                                    newPath = item.data.rule.Paths[i];
                                }
                                GUI.FocusControl(preControlName);
                                newPath = newPath == null ? "" : newPath;
                                item.data.rule.Paths[i] = newPath;
                            }
                            //
                            cellRect.y += 24;
                        }
                    }
                    break;
                case 2:
                    cellRect.y -= GetCustomRowHeight(args.row, item) / 2 - 20;
                    if (GUI.Button(cellRect, "执行"))
                    {
                        int categoryIndex = item.data.rule.categoryIndex;
                        item.data.rule.OnRuleProcessorsApplyAll(item.data.categories[categoryIndex]);
                    }
                    break;

              }
        }
        static int RulePathIndex = 0;
        //static bool hasOpenFolderPanel = false;
    }
    
}
