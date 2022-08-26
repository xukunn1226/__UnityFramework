using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor.IMGUI.Controls;
using System;

namespace Framework.AssetManagement.AssetTreeView
{

    /// <summary>
    /// warp 类 包装了接口
    /// </summary>
    /// <typeparam name="T"> 继承自BaseAssetTreeView的自定TreeView类型 </typeparam>
    /// <typeparam name="TD"> 继承自BaseTreeViewItem的自定TreeViewItem类型, 是数据项</typeparam>
    public class AssetTreeViewView<T, TD> where T : BaseAssetTreeView<TD>  where TD : BaseTreeViewItem,new() 
    {
        TreeViewState m_treeViewState;
        T m_assetTreeView;
        Rect multiColumnTreeViewRect;
        /// <summary>
        /// 得到TreeView
        /// </summary>
        public T AssetTreeView
        {
            get { return m_assetTreeView; }
        }

        /// <summary>
        /// 构造 并初始化 treeView
        /// </summary>
        /// <param name="multiColumnTreeViewRect">Rect 确定TreeView的表头的布局</param>
        public AssetTreeViewView(Rect multiColumnTreeViewRect)
        {
            m_treeViewState = new TreeViewState();
            this.multiColumnTreeViewRect = multiColumnTreeViewRect;
            buildTreeView();
        }

        /// <summary>
        /// 初始化TreeView
        /// </summary>
        void buildTreeView()
        {
            TD tD = new TD();
            var headState =tD.GetMutiColumnHeader(multiColumnTreeViewRect.width);
    
            var multiColumnHeader = new MultiColumnHeader(headState);

            m_assetTreeView = (T)Activator.CreateInstance(typeof(T),m_treeViewState, multiColumnHeader);
            m_assetTreeView.multiColumnHeader.ResizeToFit();
        }

        /// <summary>
        /// 绘制TreeView
        /// </summary>
        /// <param name="pos">Rect 确定TreeView的布局位置，跟GUI类的Rect一样</param>
        public void DrawGUI(Rect pos)
        {
            m_assetTreeView.OnGUI(pos);
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="datas">传入List，数据类型是自定义的继承自BaseTreeViewItem的类型</param>
        public void UpdateData(List<TD> datas)
        {
            m_assetTreeView.GetData(datas);

        }

    }
}
