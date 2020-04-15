using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using UITreeView;
using UnityEditor;
using UnityEngine.Assertions;
using System;
using System.Linq;

namespace Framework.AssetBrowser
{
    internal class ReferencedObjectTreeView : TreeViewWithTreeModel<ReferencedObjectTreeElement>
    {
        static private Texture2D[] m_Icons =
        {
            EditorGUIUtility.FindTexture("console.warnicon.sml"),
            EditorGUIUtility.FindTexture("console.erroricon.sml"),
        };

        enum MyColumns
        {
            Name,
            Value2,
            Value1,
            Icon2,
            Icon3,
        }

        enum SortOption
        {
            Name1,
            Name2,
            Name3,
            Value1,
            Value2,
        }

        // Sort options per column
        SortOption[] m_SortOptions =
        {
            SortOption.Name1,
            SortOption.Name2,
            SortOption.Name3,
            SortOption.Value1,
            SortOption.Value2,
        };

        private BundleInspectorTab m_Owner;

        public ReferencedObjectTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader, TreeModel<ReferencedObjectTreeElement> model, BundleInspectorTab owner)
       : base(state, multiColumnHeader, model)
        {
            m_Owner = owner;
            showBorder = true;
            showAlternatingRowBackgrounds = true;

            extraSpaceBeforeIconAndLabel = -10;
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

            var myTypes = rootItem.children.Cast<TreeViewItem<ReferencedObjectTreeElement>>();

            var orderedQuery = InitialOrder(myTypes, sortedColumns);

            for (int i = 1; i < sortedColumns.Length; i++)
            {
                SortOption sortOption = m_SortOptions[sortedColumns[i]];
                bool ascending = multiColumnHeader.IsSortedAscending(sortedColumns[i]);

                switch (sortOption)
                {
                    case SortOption.Name1:
                        orderedQuery = orderedQuery.ThenBy(l => l.data.referencedObjectInfo.name, ascending);
                        break;
                    case SortOption.Name2:
                        orderedQuery = orderedQuery.ThenBy(l => l.data.referencedObjectInfo.type.Name, ascending);
                        break;
                    case SortOption.Name3:
                        orderedQuery = orderedQuery.ThenBy(l => l.data.referencedObjectInfo.assetPath, ascending);
                        break;
                    case SortOption.Value1:
                        orderedQuery = orderedQuery.ThenBy(l => l.data.referencedObjectInfo.isBuiltIn, ascending);
                        break;
                    case SortOption.Value2:
                        orderedQuery = orderedQuery.ThenBy(l => l.data.referencedObjectInfo.isExternal, ascending);
                        break;
                }
            }

            rootItem.children = orderedQuery.Cast<TreeViewItem>().ToList();
        }

        IOrderedEnumerable<TreeViewItem<ReferencedObjectTreeElement>> InitialOrder(IEnumerable<TreeViewItem<ReferencedObjectTreeElement>> myTypes, int[] history)
        {
            SortOption sortOption = m_SortOptions[history[0]];
            bool ascending = multiColumnHeader.IsSortedAscending(history[0]);
            switch (sortOption)
            {
                case SortOption.Name1:
                    return myTypes.Order(l => l.data.referencedObjectInfo.name, ascending);
                case SortOption.Name2:
                    return myTypes.Order(l => l.data.referencedObjectInfo.type.Name, ascending);
                case SortOption.Name3:
                    return myTypes.Order(l => l.data.referencedObjectInfo.assetPath, ascending);
                case SortOption.Value1:
                    return myTypes.Order(l => l.data.referencedObjectInfo.isBuiltIn, ascending);
                case SortOption.Value2:
                    return myTypes.Order(l => l.data.referencedObjectInfo.isExternal, ascending);
                default:
                    Assert.IsTrue(false, "Unhandled enum");
                    break;
            }

            // default
            return myTypes.Order(l => l.data.referencedObjectInfo.name, ascending);
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = (TreeViewItem<ReferencedObjectTreeElement>)args.item;

            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), item, (MyColumns)args.GetColumn(i), ref args);
            }
        }
        
        void CellGUI(Rect cellRect, TreeViewItem<ReferencedObjectTreeElement> item, MyColumns column, ref RowGUIArgs args)
        {
            CenterRectUsingSingleLineHeight(ref cellRect);

            TreeViewItem<ReferencedObjectTreeElement> element = (TreeViewItem<ReferencedObjectTreeElement>)args.item;
            switch (column)
            {
                case MyColumns.Name:
                    // draw label
                    args.rowRect = cellRect;
                    base.RowGUI(args);
                    break;
                case MyColumns.Value2:       // type
                    GUI.Label(cellRect, new GUIContent(element.data.referencedObjectInfo.type.Name));
                    break;
                case MyColumns.Value1:       // asset path
                    GUI.Label(cellRect, new GUIContent(element.data.referencedObjectInfo.assetPath, element.data.referencedObjectInfo.assetPath));
                    break;
                case MyColumns.Icon2:       // isBuiltIn
                    if (element.data.referencedObjectInfo.isBuiltIn)
                        GUI.DrawTexture(cellRect, m_Icons[1], ScaleMode.ScaleToFit);
                    break;
                case MyColumns.Icon3:       // isExternal
                    if (element.data.referencedObjectInfo.isExternal)
                        GUI.DrawTexture(cellRect, m_Icons[1], ScaleMode.ScaleToFit);
                    break;
            }
        }

        internal void UpdateData(AssetListTreeElement selectedViewItem)
        {
            List<ReferencedObjectTreeElement> data = new List<ReferencedObjectTreeElement>();
            data.Add(new ReferencedObjectTreeElement("Root", -1, -1));

            if (selectedViewItem != null && selectedViewItem.assetInfo != null && selectedViewItem.assetInfo.dependentOn.Count > 0)
            {
                for (int index = 0; index < selectedViewItem.assetInfo.dependentOn.Count; ++index)
                {
                    ReferencedObjectInfo roi = selectedViewItem.assetInfo.dependentOn[index];
                    ReferencedObjectTreeElement treeElement = new ReferencedObjectTreeElement(roi.name, 0, index);
                    treeElement.referencedObjectInfo = roi;
                    //Debug.Log($"        {roi.name}  {roi.type.Name}  {roi.assetPath}  {roi.isBuiltIn}    {roi.isExternal}");
                    data.Add(treeElement);
                }
            }

            treeModel.SetData(data);
            Reload();

            // 更新数据后选择最近选择
            SetFocusAndSelectLast(false);
        }

        protected override void KeyEvent()
        {
            if (Event.current.keyCode == KeyCode.LeftArrow && Event.current.type == EventType.KeyDown)
            {
                m_Owner.SetFocusToAssetList();
            }
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if (selectedIds == null || selectedIds.Count == 0)
                return;

            TreeViewItem<ReferencedObjectTreeElement> item = (TreeViewItem<ReferencedObjectTreeElement>)FindItem(selectedIds[0], rootItem);
            if (item != null && item.data != null)
            {
                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(item.data.referencedObjectInfo.assetPath);
                if (obj != null)
                    Selection.activeObject = obj;
            }
        }

        internal void SetFocusAndSelectLast(bool bFocus = true)
        {
            if (bFocus)
                SetFocus();

            IList<int> selectedIDs = GetSelection();
            // 优先选中最近选择且存在的数据
            SetSelection((selectedIDs.Count > 0 && FindItem(selectedIDs[0], rootItem) != null) ? selectedIDs : new int[] { 0 });
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
                    width = 160,
                    minWidth = 50,
                    //maxWidth = 60,
                    autoResize = false,
                    allowToggleVisibility = true,
                    canSort = true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Type"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 100,
                    minWidth = 50,
                    maxWidth = 120,
                    autoResize = false,
                    allowToggleVisibility = false,
                    canSort = true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Asset Path"),
                    contextMenuText = "Info",
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 200,
                    minWidth = 50,
                    //maxWidth = 250,
                    autoResize = false,
                    allowToggleVisibility = true,
                    canSort = true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent(EditorGUIUtility.FindTexture("console.infoicon"), "isBuiltIn"),
                    headerTextAlignment = TextAlignment.Right,
                    sortedAscending = false,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 25,
                    minWidth = 25,
                    maxWidth = 25,
                    autoResize = false,
                    allowToggleVisibility = false,
                    canSort = true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent(EditorGUIUtility.FindTexture("console.infoicon"), "isExternal"),
                    headerTextAlignment = TextAlignment.Right,
                    sortedAscending = false,
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

    internal class ReferencedObjectMultiColumnHeader : MultiColumnHeader
    {
        Mode m_Mode;

        public enum Mode
        {
            LargeHeader,
            DefaultHeader,
            MinimumHeaderWithoutSorting
        }

        public ReferencedObjectMultiColumnHeader(MultiColumnHeaderState state)
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