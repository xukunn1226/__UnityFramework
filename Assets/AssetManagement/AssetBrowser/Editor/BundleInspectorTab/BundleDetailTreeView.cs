using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UITreeView;
using System.IO;

namespace AssetManagement.AssetBrowser
{
    internal class BundleDetailTreeView : TreeViewWithTreeModel<BundleDetailTreeElement>
    {
        private BundleInspectorTab m_Owner;

        public BundleDetailTreeView(TreeViewState state, TreeModel<BundleDetailTreeElement> model, BundleInspectorTab owner)
            : base(state, model)
        {
            m_Owner = owner;
            showBorder = true;
            showAlternatingRowBackgrounds = false;

            Reload();
        }

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            return false;
        }

        protected override bool CanRename(TreeViewItem item)
        {
            return false;
        }

        protected override void RenameEnded(RenameEndedArgs args)
        { }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        protected override void KeyEvent()
        {
            // 回车展开当前选中的item
            if ((Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter) && Event.current.type == EventType.KeyDown)
            {
                IList<int> selectedIDs = GetSelection();
                if (selectedIDs.Count != 1)
                    return;

                bool isExpanded = IsExpanded(selectedIDs[0]);

                if (Event.current.alt)
                    SetExpandedRecursive(selectedIDs[0], !isExpanded);
                else
                    SetExpanded(selectedIDs[0], !isExpanded);
            }

            // alt + mouse down
            if(Event.current.alt && Event.current.type == EventType.MouseDown)
            {
                TreeViewItem<BundleDetailTreeElement> item = (TreeViewItem<BundleDetailTreeElement>)FindItem(GetSelection()[0], rootItem);
                if (item.depth == 2)    // set focus to bundle list when click dependent on item
                    m_Owner.SetFocusToBundleList(item.data.name);
            }
        }

        internal void UpdateData(BundleListTreeElement selectedViewItem)
        {
            List<BundleDetailTreeElement> data = new List<BundleDetailTreeElement>();
            data.Add(new BundleDetailTreeElement("Root", -1, 0));

            if (selectedViewItem == null || string.IsNullOrEmpty(selectedViewItem.bundleFileInfo.assetBundleName))
            {
                treeModel.SetData(data);
                Reload();
                return;
            }

            BundleDetailTreeElement treeElement = new BundleDetailTreeElement(selectedViewItem.bundleFileInfo.assetBundleName, 0, selectedViewItem.bundleFileInfo.hashName.GetHashCode());
            data.Add(treeElement);

            string dependOnHeader = "Dependent On:";
            BundleDetailTreeElement dependOnElement = new BundleDetailTreeElement(dependOnHeader, 1, dependOnHeader.GetHashCode());
            data.Add(dependOnElement);

            foreach (string dependency in selectedViewItem.bundleFileInfo.dependentOnBundleList)
            {
                BundleDetailTreeElement dependentElement = new BundleDetailTreeElement(dependency, 2, dependency.GetHashCode());
                data.Add(dependentElement);
            }

            treeModel.SetData(data);
            Reload();

            SetExpandedRecursive(selectedViewItem.bundleFileInfo.hashName.GetHashCode(), true);
        }
    }
}