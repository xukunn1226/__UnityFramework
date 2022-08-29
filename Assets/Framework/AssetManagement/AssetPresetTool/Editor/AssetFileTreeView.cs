using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using UnityEditor.IMGUI.Controls;
using Framework.AssetManagement.AssetTreeView;

namespace Framework.AssetManagement.AssetProcess
{
    public class AssetFileTreeViewItem : BaseTreeViewItem
    {
        public string path;
        public string presetMatch;
        public AssetFileTreeViewItem():base()
        {
            path = "";
        }
        public AssetFileTreeViewItem(int id, int depth, string name, string path)
            : base(id, depth, name)
        {
            this.path = path;
        }
        public override MultiColumnHeaderState GetMutiColumnHeader(float width)
        {
            var colums = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Name"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    canSort = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 100,
                    minWidth = 60,
                    autoResize = true,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Path"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    canSort = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 150,
                    minWidth = 60,
                    autoResize = true,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("presetMatch"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    canSort = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 50,
                    minWidth = 50,
                    autoResize = true,
                    allowToggleVisibility = false
                },
            };
            var state = new MultiColumnHeaderState(colums);
            return state;
        }
    }
    public class AssetFileTreeView : BaseAssetTreeView<AssetFileTreeViewItem>
    {
        public AssetFileTreeView(TreeViewState treeViewState, MultiColumnHeader multiColumnHeader) 
            : base(treeViewState, multiColumnHeader)
        {
            columnIndexForTreeFoldouts = 2;
            multiColumnHeader.sortingChanged += OnSorting; 
        }
        //protected override float GetCustomRowHeight(int row, TreeViewItem item)
        //{
        //    return 50;
        //}

        protected override void DrawRow(Rect cellRect, TreeViewItem<AssetFileTreeViewItem> item, int column, ref RowGUIArgs args)
        {
            //根据当前表格项的所在的列数进行switch 
            switch (column)
            {
                case 0:
                    string name = item.data.DisPlayName;
                    GUI.Label(cellRect, name);
                    break;
                case 1:
                    string path = item.data.path;
                    GUI.Label(cellRect, path);
                    break;
                case 2:
                    string match = item.data.presetMatch;
                    GUIStyle style = new GUIStyle(TreeView.DefaultStyles.label);
                    if(match =="Match")
                    {
                        style.normal.textColor = Color.green;
                    }
                    else if(match == "Dont Match")
                    {
                        style.normal.textColor = Color.red;
                    }
                    else 
                    {
                        style.normal.textColor = Color.gray;
                    }
                    GUI.Label(cellRect, match,style);
                    break;
            }
        }
        public override void GetData(List<AssetFileTreeViewItem> datas)
        {
            m_itemData = new List<TreeViewItem<AssetFileTreeViewItem>>();
            foreach(AssetFileTreeViewItem item in datas)
            {
                TreeViewItem<AssetFileTreeViewItem> vt = new TreeViewItem<AssetFileTreeViewItem>(item.mId, item.depth, item.DisPlayName, item);
                m_itemData.Add(vt);
            }
            Reload();

        }

        void OnSorting(MultiColumnHeader sortingHeader)
        {
            Debug.Log("sort");
            int headIndex = sortingHeader.sortedColumnIndex;
            m_itemData.Sort((a, b) =>
            {
                string compareA = a.data.DisPlayName;
                string compareB = b.data.DisPlayName;
                if(headIndex == 1)
                {
                    compareA = a.data.path;
                    compareB = b.data.path;
                }
                if (string.Compare(compareA, compareB) > 0)
                {
                    return 1;
                }
                else if (string.Compare(compareA, compareB) < 0)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            });
            Reload();
        }

        protected override void DoubleClickedItem(int id)
        {
            foreach(var item in m_itemData)
            {
                if(item.id == id)
                {
                    Debug.Log($"item: {item.displayName}");
                    Object obj = UnityEditor.AssetDatabase.LoadAssetAtPath<Object>(item.data.path);
                    UnityEditor.EditorGUIUtility.PingObject(obj);
                    UnityEditor.Selection.activeObject = obj;
                }
            }
        }

    }

}


