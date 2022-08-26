using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.IMGUI.Controls;

namespace Framework.AssetManagement.AssetTreeView
{

    public abstract  class BaseTreeViewItem
    {
        public int mId;
        public int depth;
        public string DisPlayName;
        public bool enabled = true;
        public bool isRootNode = false;
        
		public BaseTreeViewItem Parent;
		public List<BaseTreeViewItem> Children;

        public void SetChildrens(List<BaseTreeViewItem> children)
        {
            children = children;
        }

        public bool HasChildrens()
        {
            if (Children == null)
            {
                return false;
            }

            if (Children.Count == 0)
            {
                return false;
            }

            if (Children.Count == 1 && Children[0] == null)
            {
                return false;
            }
            
            return true;
        }
        public BaseTreeViewItem()
        {
            mId = -1;
            depth = 0;
            DisPlayName = "root";
        }

        public BaseTreeViewItem(int id, int depth, string name,bool enabled = true)
        {
            mId = id;
            this.depth = depth;
            DisPlayName = name;
            this.enabled = enabled;
        }
        public abstract MultiColumnHeaderState GetMutiColumnHeader(float width);
    }

    public class TreeViewItem<T> : TreeViewItem where T : BaseTreeViewItem
    {
        public T data { get; set; }
        public TreeViewItem(int id, int depth, string displayName, T data,bool enabled = true )
            : base(id, depth, displayName)
        {
            this.data = data;
        }
    }

}
