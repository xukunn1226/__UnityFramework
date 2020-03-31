using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using UITreeView;
using UnityEditor;
using UnityEngine.Assertions;
using System;
using System.Linq;

namespace AssetManagement.AssetBrowser
{
    internal class AssetListTreeView : TreeViewWithTreeModel<AssetListTreeElement>
    {
        static private Texture2D[] m_Icons =
        {
            EditorGUIUtility.FindTexture("console.warnicon.sml"),
            EditorGUIUtility.FindTexture("console.erroricon.sml"),
            EditorGUIUtility.FindTexture("d_Refresh"),
        };

        enum MyColumns
        {
            Name,           // display name
            Value,          // size
            Icon1,          // hint of missing reference
            Icon2,          // hint of builtin or external resource
        }

        enum SortOption
        {
            Name,
            Value1,
            Value2,
            Value3,
        }

        // Sort options per column
        SortOption[] m_SortOptions =
        {
            SortOption.Name,
            SortOption.Value1,
            SortOption.Value2,
            SortOption.Value3,
        };

        private BundleInspectorTab m_Owner;

        public AssetListTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader, TreeModel<AssetListTreeElement> model, BundleInspectorTab owner)
       : base(state, multiColumnHeader, model)
        {
            m_Owner = owner;
            showBorder = true;
            showAlternatingRowBackgrounds = true;

            extraSpaceBeforeIconAndLabel = 14;
            columnIndexForTreeFoldouts = 0;
            customFoldoutYOffset = (rowHeight - EditorGUIUtility.singleLineHeight) * 0.5f;
            multiColumnHeader.sortingChanged += OnSortingChanged;

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

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            var rows = base.BuildRows(root);
            SortIfNeeded(root, rows);
            return rows;
        }

        void OnSortingChanged(MultiColumnHeader multiColumnHeader)
        {
            SortIfNeeded(rootItem, GetRows());
        }

        void SortIfNeeded(TreeViewItem root, IList<TreeViewItem> rows)
        {
            if (rows.Count <= 1)
                return;

            if (multiColumnHeader.sortedColumnIndex == -1)
            {
                return; // No column to sort for (just use the order the data are in)
            }

            // Sort the roots of the existing tree items
            SortByMultipleColumns();
            TreeToList(root, rows);
            Repaint();
        }

        public static void TreeToList(TreeViewItem root, IList<TreeViewItem> result)
        {
            if (root == null)
                throw new NullReferenceException("root");
            if (result == null)
                throw new NullReferenceException("result");

            result.Clear();

            if (root.children == null)
                return;

            Stack<TreeViewItem> stack = new Stack<TreeViewItem>();
            for (int i = root.children.Count - 1; i >= 0; i--)
                stack.Push(root.children[i]);

            while (stack.Count > 0)
            {
                TreeViewItem current = stack.Pop();
                result.Add(current);

                if (current.hasChildren && current.children[0] != null)
                {
                    for (int i = current.children.Count - 1; i >= 0; i--)
                    {
                        stack.Push(current.children[i]);
                    }
                }
            }
        }

        void SortByMultipleColumns()
        {
            var sortedColumns = multiColumnHeader.state.sortedColumns;

            if (sortedColumns.Length == 0)
                return;

            var myTypes = rootItem.children.Cast<TreeViewItem<AssetListTreeElement>>();

            var orderedQuery = InitialOrder(myTypes, sortedColumns);

            for (int i = 1; i < sortedColumns.Length; i++)
            {
                SortOption sortOption = m_SortOptions[sortedColumns[i]];
                bool ascending = multiColumnHeader.IsSortedAscending(sortedColumns[i]);

                switch (sortOption)
                {
                    case SortOption.Value2:
                        orderedQuery = orderedQuery.ThenBy(l => l.data.assetInfo.missingReferenceError, ascending);
                        break;
                    case SortOption.Value3:
                        orderedQuery = orderedQuery.ThenBy(l => l.data.assetInfo.isValid, ascending);
                        break;
                }
            }

            rootItem.children = orderedQuery.Cast<TreeViewItem>().ToList();
        }

        IOrderedEnumerable<TreeViewItem<AssetListTreeElement>> InitialOrder(IEnumerable<TreeViewItem<AssetListTreeElement>> myTypes, int[] history)
        {
            SortOption sortOption = m_SortOptions[history[0]];
            bool ascending = multiColumnHeader.IsSortedAscending(history[0]);
            switch (sortOption)
            {
                case SortOption.Value2:
                    return myTypes.Order(l => l.data.assetInfo.missingReferenceError, ascending);
                case SortOption.Value3:
                    return myTypes.Order(l => l.data.assetInfo.isValid, ascending);
                default:
                    Assert.IsTrue(false, "Unhandled enum");
                    break;
            }

            // default
            return myTypes.Order(l => l.data.assetInfo.name, ascending);
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = (TreeViewItem<AssetListTreeElement>)args.item;

            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), item, (MyColumns)args.GetColumn(i), ref args);
            }
        }

        void CellGUI(Rect cellRect, TreeViewItem<AssetListTreeElement> item, MyColumns column, ref RowGUIArgs args)
        {
            CenterRectUsingSingleLineHeight(ref cellRect);

            TreeViewItem<AssetListTreeElement> element = (TreeViewItem<AssetListTreeElement>)args.item;
            switch (column)
            {
                case MyColumns.Name:
                    Rect iconRect = args.rowRect;
                    iconRect.width = 28f;

                    // 选中icon时也可选中item
                    if (Event.current.type == EventType.MouseDown && iconRect.Contains(Event.current.mousePosition))
                    {
                        SetFocus();
                        SelectionClick(args.item, false);
                    }

                    if(GUI.Button(iconRect, m_Icons[2]))
                    {
                        Refresh(element.data);
                    }

                    // draw label
                    args.rowRect = cellRect;
                    base.RowGUI(args);
                    break;
                case MyColumns.Value:
                    GUI.Label(cellRect, new GUIContent(EditorUtility.FormatBytes(element.data.assetInfo.size)));
                    break;
                case MyColumns.Icon1:       // missing reference
                    if (!string.IsNullOrEmpty(element.data.assetInfo.missingReferenceError))
                    {
                        GUI.BeginGroup(cellRect, new GUIContent(m_Icons[0], element.data.assetInfo.missingReferenceError));
                        GUI.EndGroup();
                    }
                    break;
                case MyColumns.Icon2:       // hint built-in or external referenced asset
                    if(!element.data.assetInfo.isValid)
                        GUI.DrawTexture(cellRect, m_Icons[1], ScaleMode.ScaleToFit);
                    break;
            }
        }

        private void Refresh(AssetListTreeElement element)
        {
            BundleFileInfo.ParseAsset(element.assetInfo.assetPath, ref element.assetInfo);
            SetFocusAndSelectLast();
        }

        // 由BundleList选中变化触发
        internal void UpdateData(BundleListTreeElement selectedViewItem)
        {
            List<AssetListTreeElement> data = new List<AssetListTreeElement>();
            data.Add(new AssetListTreeElement("Root", -1, -1));

            if (selectedViewItem != null && !string.IsNullOrEmpty(selectedViewItem.bundleFileInfo.assetBundleName) && selectedViewItem.bundleFileInfo.includedAssetFileList.Count > 0)
            {
                for (int index = 0; index < selectedViewItem.bundleFileInfo.includedAssetFileList.Count; ++index)
                {
                    AssetFileInfo afi = selectedViewItem.bundleFileInfo.includedAssetFileList[index];
                    AssetListTreeElement treeElement = new AssetListTreeElement(afi.name, 0, index);
                    treeElement.assetInfo = afi;
                    data.Add(treeElement);
                }
            }

            treeModel.SetData(data);
            Reload();

            // 更新数据后选择最近选择，触发刷新referencedObject list
            SetFocusAndSelectLast(false);
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if (selectedIds.Count == 0)
            {
                m_Owner.OnPostAssetListSelection(null);
                return;
            }

            TreeViewItem<AssetListTreeElement> item = (TreeViewItem<AssetListTreeElement>)FindItem(selectedIds[0], rootItem);
            if (item != null && item.data != null)
            {
                m_Owner.OnPostAssetListSelection(item.data);
            }
            else
            {
                m_Owner.OnPostAssetListSelection(null);
            }
        }

        protected override void KeyEvent()
        {
            // 切换至BundleList View
            if (Event.current.keyCode == KeyCode.LeftArrow && Event.current.type == EventType.KeyDown)
            {
                m_Owner.SetFocusToBundleList(null);
            }

            // 切换至ReferencedObject View
            if (Event.current.keyCode == KeyCode.RightArrow && Event.current.type == EventType.KeyDown)
            {
                m_Owner.SetFocusToReferencedObjectList();
            }
        }

        internal void SetFocusAndSelectLast(bool bFocus = true)
        {
            if(bFocus)
                SetFocus();

            IList<int> selectedIDs = GetSelection();
            // 优先选中最近选择且存在的数据
            SetSelection((selectedIDs.Count > 0 && FindItem(selectedIDs[0], rootItem) != null) ? selectedIDs : new int[] { 0 }, TreeViewSelectionOptions.FireSelectionChanged);
        }

        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState()
        {
            var columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Name"),
                    contextMenuText = "Name",
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 180,
                    minWidth = 50,
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
                    headerContent = new GUIContent(EditorGUIUtility.FindTexture("console.infoicon"), "Missing Reference"),
                    contextMenuText = "Info",
                    headerTextAlignment = TextAlignment.Right,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 25,
                    minWidth = 25,
                    maxWidth = 25,
                    autoResize = false,
                    allowToggleVisibility = true,
                    canSort = true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent(EditorGUIUtility.FindTexture("console.infoicon"), "Warning,Error or Info"),
                    headerTextAlignment = TextAlignment.Right,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 25,
                    minWidth = 25,
                    maxWidth = 25,
                    autoResize = false,
                    allowToggleVisibility = false,
                    canSort = true
                },
            };

            Assert.AreEqual(columns.Length, Enum.GetValues(typeof(MyColumns)).Length, "Number of columns should match number of enum values: You probably forgot to update one of them.");

            return new MultiColumnHeaderState(columns);
        }
    }

    internal class AssetListMultiColumnHeader : MultiColumnHeader
    {
        Mode m_Mode;

        public enum Mode
        {
            LargeHeader,
            DefaultHeader,
            MinimumHeaderWithoutSorting
        }

        public AssetListMultiColumnHeader(MultiColumnHeaderState state)
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
                        canSort = true;
                        height = DefaultGUI.minimumHeight;
                        break;
                }
            }
        }
    }
}