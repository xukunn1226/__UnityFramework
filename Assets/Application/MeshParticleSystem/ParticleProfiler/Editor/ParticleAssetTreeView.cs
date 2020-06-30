using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MeshParticleSystem.UITreeView;
using UnityEditor.IMGUI.Controls;

namespace MeshParticleSystem.Profiler
{
    internal class ParticleAssetTreeView : TreeViewWithTreeModel<ParticleAssetTreeElement>
    {
        enum MyColumns
        {
            Name,
            Value1,
            Value2,
            Icon,
            Button
        }

        public enum SortOption
        {
            Name,
            Value,
        }

        // Sort options per column
        SortOption[] m_SortOptions =
        {
            SortOption.Name,
            SortOption.Value,
            SortOption.Value,
        };

        private BatchProfilingWindow m_Owner;

        public ParticleAssetTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader, TreeModel<ParticleAssetTreeElement> model, BatchProfilingWindow owner)
            : base(state, multiColumnHeader, model)
        {
            m_Owner = owner;
            showBorder = true;
            showAlternatingRowBackgrounds = true;

            extraSpaceBeforeIconAndLabel = 18;
            columnIndexForTreeFoldouts = 0;
            customFoldoutYOffset = (rowHeight - EditorGUIUtility.singleLineHeight) * 0.5f;

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

        
        protected override void RowGUI(RowGUIArgs args)
        {
            var item = (TreeViewItem<ParticleAssetTreeElement>)args.item;

            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), item, (MyColumns)args.GetColumn(i), ref args);
            }
        }

        void CellGUI(Rect cellRect, TreeViewItem<ParticleAssetTreeElement> item, MyColumns column, ref RowGUIArgs args)
        {

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
                // TreeViewItem<ParticleAssetTreeElement> item = (TreeViewItem<ParticleAssetTreeElement>)FindItem(GetSelection()[0], rootItem);
                // if (item.depth == 2)    // set focus to bundle list when click dependent on item
                //     m_Owner.SetFocusToBundleList(item.data.name);
            }
        }

        internal void UpdateData(ParticleAssetTreeElement selectedViewItem)
        {
            // List<ParticleAssetTreeElement> data = new List<ParticleAssetTreeElement>();
            // data.Add(new ParticleAssetTreeElement("Root", -1, 0));

            // if (selectedViewItem == null || string.IsNullOrEmpty(selectedViewItem.bundleFileInfo.assetBundleName))
            // {
            //     treeModel.SetData(data);
            //     Reload();
            //     return;
            // }

            // ParticleAssetTreeElement treeElement = new ParticleAssetTreeElement(selectedViewItem.bundleFileInfo.assetBundleName, 0, selectedViewItem.bundleFileInfo.hashName.GetHashCode());
            // data.Add(treeElement);

            // string dependOnHeader = "Dependent On:";
            // ParticleAssetTreeElement dependOnElement = new ParticleAssetTreeElement(dependOnHeader, 1, dependOnHeader.GetHashCode());
            // data.Add(dependOnElement);

            // foreach (string dependency in selectedViewItem.bundleFileInfo.dependentOnBundleList)
            // {
            //     ParticleAssetTreeElement dependentElement = new ParticleAssetTreeElement(dependency, 2, dependency.GetHashCode());
            //     data.Add(dependentElement);
            // }

            // treeModel.SetData(data);
            // Reload();

            // SetExpandedRecursive(selectedViewItem.bundleFileInfo.hashName.GetHashCode(), true);
        }
        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState()
        {
            var columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Particle Asset"),
                    contextMenuText = "Particle Asset",
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 220,
                    minWidth = 80,
                    //maxWidth = 60,
                    autoResize = false,
                    allowToggleVisibility = true,
                    canSort = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Size"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 60,
                    minWidth = 30,
                    maxWidth = 100,
                    autoResize = false,
                    allowToggleVisibility = false,
                    canSort = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Count", "Dependencies Count"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 50,
                    minWidth = 30,
                    maxWidth = 70,
                    autoResize = false,
                    allowToggleVisibility = false,
                    canSort = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent(EditorGUIUtility.FindTexture("console.infoicon"), "Warning, Error or Info"),
                    contextMenuText = "Info",
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 25,
                    minWidth = 25,
                    maxWidth = 25,
                    autoResize = false,
                    allowToggleVisibility = true,
                    canSort = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent(EditorGUIUtility.FindTexture("console.infoicon"), "Fix Unused Mesh of ParticleSystem"),
                    contextMenuText = "Info",
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 50,
                    minWidth = 50,
                    maxWidth = 50,
                    autoResize = false,
                    allowToggleVisibility = true,
                    canSort = false
                },
            };

            UnityEngine.Assertions.Assert.AreEqual(columns.Length, Enum.GetValues(typeof(MyColumns)).Length, "Number of columns should match number of enum values: You probably forgot to update one of them.");

            return new MultiColumnHeaderState(columns);
        }
    }

    internal class ParticleAssetMultiColumnHeader : MultiColumnHeader
    {
        Mode m_Mode;

        public enum Mode
        {
            LargeHeader,
            DefaultHeader,
            MinimumHeaderWithoutSorting
        }

        public ParticleAssetMultiColumnHeader(MultiColumnHeaderState state)
            : base(state)
        {
            mode = Mode.MinimumHeaderWithoutSorting;
        }

        public Mode mode
        {
            get
            {
                return m_Mode;
            }
            set
            {
                m_Mode = value;
                switch (m_Mode)
                {
                    case Mode.LargeHeader:
                        canSort = true;
                        height = 37f;
                        break;
                    case Mode.DefaultHeader:
                        canSort = true;
                        height = DefaultGUI.defaultHeight;
                        break;
                    case Mode.MinimumHeaderWithoutSorting:
                        canSort = false;
                        height = DefaultGUI.minimumHeight;
                        break;
                }
            }
        }
    }

    public class ParticleAssetTreeElement : TreeElement
    {
        public string assetPath;
        public ParticleAssetTreeElement(string name, int depth, int id) : base(name, depth, id)
        { }
    }
}