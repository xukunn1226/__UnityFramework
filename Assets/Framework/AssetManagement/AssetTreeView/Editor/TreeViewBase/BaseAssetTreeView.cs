using System.Collections.Generic;
using UnityEngine;
using UnityEditor.IMGUI.Controls;

namespace Framework.AssetManagement.AssetTreeView
{
    public abstract class BaseAssetTreeView<T> : TreeView where T : BaseTreeViewItem, new()
    {
        public List<TreeViewItem<T>> m_itemData;
        public List<TreeViewItem> m_itemView;
        public TreeViewItem<T> rootNode;

        public BaseAssetTreeView(TreeViewState treeViewState,
                            MultiColumnHeader multiColumnHeader)
                            : base(treeViewState, multiColumnHeader)
        {
            rowHeight = 20;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            m_itemData = new List<TreeViewItem<T>>();

            Reload();
        }

        /// <summary>
        /// 设置数据
        /// </summary>
        public abstract void GetData(List<T> datas);

        protected abstract void DrawRow(Rect cellRect, TreeViewItem<T> item, int column,ref RowGUIArgs args);
        
        protected override TreeViewItem BuildRoot()
        {
            T t =new T();
            var root = new TreeViewItem<T> (0,-1,"root",t);
            return root;
        }
        // 更新数据
        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            m_itemView = new List<TreeViewItem> ();

            for (int i = 0; i < m_itemData.Count; i++)
            {
                // Add node Reverso 
                AddItemReverso(m_itemData[i].data);
            }

            SetupParentsAndChildrenFromDepths(root,m_itemView);
            base.BuildRows(root);
            return m_itemView;
        }

        void AddItemReverso(T item)
        {
            if (item == null )
            {
                return;
            }
            
            if (!item.isRootNode)
            {
                TreeViewItem<T> tv = new TreeViewItem<T>(item.mId, item.depth, item.DisPlayName, item);
                bool nodeExpanded = IsExpanded(item.mId);
                
                //  !expanded
                if (item.HasChildrens() && !nodeExpanded)
                {
                    tv.children = CreateChildListForCollapsedParent();
                    m_itemView.Add(tv);
                    return;
                }
                //  expanded
                m_itemView.Add(tv);
                if (item.HasChildrens())
                {
                    for (int i = 0; i < item.Children.Count; i++)
                    {
                        AddItemReverso(item.Children[i] as T);
                    }
                }
            }
            else if(item.isRootNode)
            {
                if (item.HasChildrens())
                {
                    for (int i = 0; i < item.Children.Count; i++)
                    {
                        AddItemReverso(item.Children[i] as T);
                    }
                }
            }
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = (TreeViewItem<T>)args.item;
            for (int i = 0; i < args.GetNumVisibleColumns(); i++)
            {
                CellGUI(args.GetCellRect(i), item, args.GetColumn(i), ref args);
            }
        }

        /*
         *  绘制列数据
         */
        void CellGUI(Rect cellRect, TreeViewItem<T> item, int column, ref RowGUIArgs args)
        {
            CenterRectUsingSingleLineHeight(ref cellRect);
            DrawRow(cellRect,item,column,ref args);
        }

    }




}



