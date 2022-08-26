using System.Collections;
using System.Collections.Generic;
using Framework.AssetManagement.AssetTreeView;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

/// <summary>
/// Auto Gen From TreeView Creator
/// </summary>

namespace Framework.AssetManagement.AssetPackageEditor.Editor
{
    public class AssetPackageFileShowTreeViewItem : BaseTreeViewItem
    {
        public string path;
        public AssetPackageFileShowTreeViewItem()
        {
        }
        public AssetPackageFileShowTreeViewItem(int id, int depth, string name,string path):base(id,depth,name)
        {
            this.path = path;
        }
        public override MultiColumnHeaderState GetMutiColumnHeader(float width)
        {
            var colums = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("path"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 70,
                    minWidth = 60,
                    autoResize = true,
                    allowToggleVisibility = false
                },
                
            };
            var state = new MultiColumnHeaderState(colums);
            return state;
        }
    }
    
    public class AssetPackageFileShowTreeView : BaseAssetTreeView<AssetPackageFileShowTreeViewItem>
    {
        public AssetPackageFileShowTreeView(TreeViewState treeViewState, MultiColumnHeader multiColumnHeader) : base(treeViewState, multiColumnHeader)
        {
    
        }
    
        public override void GetData(List<AssetPackageFileShowTreeViewItem> datas)
        {
            m_itemData = new List<TreeViewItem<AssetPackageFileShowTreeViewItem>>();
            foreach(var item in datas)
            {
                TreeViewItem<AssetPackageFileShowTreeViewItem> tv = new TreeViewItem<AssetPackageFileShowTreeViewItem>(item.mId, item.depth, item.DisPlayName, item);
                m_itemData.Add(tv);
            }
            Reload();
        }
    
        protected override void DrawRow(Rect cellRect, TreeViewItem<AssetPackageFileShowTreeViewItem> item, int column, ref RowGUIArgs args)
        {
            switch (column)
            {
                case 0:
                    string path = item.data.path;
                    GUI.Label(cellRect, path );
                    break;
            }
        }
    }
}
