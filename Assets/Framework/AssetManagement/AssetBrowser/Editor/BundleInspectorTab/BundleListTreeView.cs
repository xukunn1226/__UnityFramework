using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UITreeView;
using UnityEngine.Assertions;
using System.Linq;

namespace Framework.AssetManagement.AssetBrowser
{
    internal class BundleListTreeView : TreeViewWithTreeModel<BundleListTreeElement>
    {
        static private Texture2D[] m_Icons =
        {
            EditorGUIUtility.FindTexture("d_winbtn_mac_inact"),
            EditorGUIUtility.FindTexture("d_winbtn_mac_max"),
            EditorGUIUtility.FindTexture("TestPassed"),
            EditorGUIUtility.FindTexture("console.warnicon.sml"),
            EditorGUIUtility.FindTexture("console.erroricon.sml"),
            EditorGUIUtility.FindTexture("BuildSettings.Editor.Small"),
        };

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

        private BundleInspectorTab m_Owner;

        public BundleListTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader, TreeModel<BundleListTreeElement> model, BundleInspectorTab owner)
            : base(state, multiColumnHeader, model)
        {
            m_Owner = owner;
            showBorder = true;
            showAlternatingRowBackgrounds = true;

            extraSpaceBeforeIconAndLabel = 18;
            columnIndexForTreeFoldouts = 0;
            customFoldoutYOffset = (rowHeight - EditorGUIUtility.singleLineHeight) * 0.5f;
            //multiColumnHeader.sortingChanged += OnSortingChanged;  // 不支持排序

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

        //protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        //{
        //    var rows = base.BuildRows(root);
        //    SortIfNeeded(root, rows);
        //    return rows;
        //}

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

            var myTypes = rootItem.children.Cast<TreeViewItem<BundleListTreeElement>>();

            var orderedQuery = InitialOrder(myTypes, sortedColumns);

            for (int i = 1; i < sortedColumns.Length; i++)
            {
                SortOption sortOption = m_SortOptions[sortedColumns[i]];
                bool ascending = multiColumnHeader.IsSortedAscending(sortedColumns[i]);

                switch (sortOption)
                {
                    case SortOption.Name:
                        orderedQuery = orderedQuery.ThenBy(l => l.data.name, ascending);
                        break;
                    case SortOption.Value:
                        orderedQuery = orderedQuery.ThenBy(l => l.data.bundleFileInfo.dependentOnBundleList.Length, ascending);
                        break;
                }
            }

            rootItem.children = orderedQuery.Cast<TreeViewItem>().ToList();
        }

        IOrderedEnumerable<TreeViewItem<BundleListTreeElement>> InitialOrder(IEnumerable<TreeViewItem<BundleListTreeElement>> myTypes, int[] history)
        {
            SortOption sortOption = m_SortOptions[history[0]];
            bool ascending = multiColumnHeader.IsSortedAscending(history[0]);
            switch (sortOption)
            {
                case SortOption.Name:
                    return myTypes.Order(l => l.data.name, ascending);
                case SortOption.Value:
                    return myTypes.Order(l => l.data.bundleFileInfo.dependentOnBundleList.Length, ascending);
                default:
                    Assert.IsTrue(false, "Unhandled enum");
                    break;
            }

            // default
            return myTypes.Order(l => l.data.name, ascending);
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = (TreeViewItem<BundleListTreeElement>)args.item;

            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), item, (MyColumns)args.GetColumn(i), ref args);
            }
        }

        void CellGUI(Rect cellRect, TreeViewItem<BundleListTreeElement> item, MyColumns column, ref RowGUIArgs args)
        {
            CenterRectUsingSingleLineHeight(ref cellRect);

            TreeViewItem<BundleListTreeElement> element = (TreeViewItem<BundleListTreeElement>)args.item;
            if (element == null || element.data == null || element.data.bundleFileInfo == null)
                return;

            switch (column)
            {
                case MyColumns.Name:
                    Rect iconRect = args.rowRect;
                    iconRect.x += GetContentIndent(args.item);
                    iconRect.width = 16f;

                    if(element.data.bundleFileInfo.includeScene)
                    {
                        GUI.DrawTexture(iconRect, m_Icons[5]);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(element.data.bundleFileInfo.assetBundleName))
                            GUI.DrawTexture(iconRect, m_Icons[0]);                        
                        else
                            GUI.DrawTexture(iconRect, m_Icons[1]);
                    }

                    // 选中icon时也可选中item
                    if (Event.current.type == EventType.MouseDown && iconRect.Contains(Event.current.mousePosition))
                        SelectionClick(args.item, false);

                    // draw label
                    args.rowRect = cellRect;
                    base.RowGUI(args); 
                    break;
                case MyColumns.Value1:
                    if(element.data.bundleFileInfo.size > 0)
                        GUI.Label(cellRect, new GUIContent(EditorUtility.FormatBytes(element.data.bundleFileInfo.size)));
                    break;
                case MyColumns.Icon:
                    if (!element.data.bundleFileInfo.isBundle)
                        break;      // 不是bundle则忽略

                    if (!element.data.bundleFileInfo.isValid)
                        GUI.DrawTexture(cellRect, m_Icons[4], ScaleMode.ScaleToFit);
                    else if (element.data.bundleFileInfo.hasMissingReference)
                        GUI.DrawTexture(cellRect, m_Icons[3], ScaleMode.ScaleToFit);
                    else
                        GUI.DrawTexture(cellRect, m_Icons[2], ScaleMode.ScaleToFit);
                    break;
                case MyColumns.Value2:
                    if (item.data.bundleFileInfo.dependentOnBundleList != null)
                    {
                        string info = string.Format("[" + item.data.bundleFileInfo.dependentOnBundleList.Length + "]");
                        if (item.data.bundleFileInfo.dependentOnBundleList.Length > 4)
                        {
                            GUIStyle style = new GUIStyle("BoldLabel");
                            style.normal.textColor = Color.red;
                            GUI.Label(cellRect, info, style);
                        }
                        else
                        {
                            GUI.Label(cellRect, info);
                        }
                    }
                    break;
                case MyColumns.Button:
                    IList<int> selectedIDs = GetSelection();
                    if(selectedIDs.Count > 0 && item.data.id == selectedIDs[0])
                    {
                        if(GUI.Button(cellRect, "Fix"))
                        {
                            if (item.data.bundleFileInfo.includedAssetFileList == null)
                                return;
                            foreach(var assetFileInfo in item.data.bundleFileInfo.includedAssetFileList)
                            {
                                AssetBrowserUtil.FixRedundantMeshOfParticleSystemRender(assetFileInfo.assetPath);
                            }
                            AssetDatabase.SaveAssets();
                        }
                    }
                    break;
            }
        }

        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState()
        {
            var columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Bundle"),
                    contextMenuText = "Bundle",
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

            Assert.AreEqual(columns.Length, Enum.GetValues(typeof(MyColumns)).Length, "Number of columns should match number of enum values: You probably forgot to update one of them.");

            return new MultiColumnHeaderState(columns);
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if (selectedIds.Count == 0)
            {
                m_Owner.OnPostBundleListSelection(null);
                return;
            }

            TreeViewItem<BundleListTreeElement> item = (TreeViewItem<BundleListTreeElement>)FindItem(selectedIds[0], rootItem);
            if (item != null && item.data != null)
            {
                m_Owner.OnPostBundleListSelection(item.data);
            }
            else
            {
                m_Owner.OnPostBundleListSelection(null);
            }
        }

        //protected override void ContextClickedItem(int id)
        //{
        //    TreeViewItem<BundleListTreeElement> item = (TreeViewItem<BundleListTreeElement>)FindItem(id, rootItem);
        //    if (item == null || item.data == null)
        //        return;

        //    GenericMenu menu = new GenericMenu();
        //    if(string.IsNullOrEmpty(item.data.bundleFileInfo.assetBundleName))
        //        menu.AddDisabledItem(new GUIContent("Refresh"));
        //    else
        //        menu.AddItem(new GUIContent("Refresh"), false, ReloadBundleData, item.data);
        //    menu.ShowAsContext();
        //}

        //private void ReloadBundleData(object userData)
        //{
        //    BundleListTreeElement element = (BundleListTreeElement)userData;
        //    if (element == null)
        //        return;

        //    Debug.Log("Refresh: " + element.bundleFileInfo.assetBundleName);
        //}
        
        internal void SetFocusAndSelect(string name)
        {
            SetFocus();

            if (string.IsNullOrEmpty(name))
            {
                IList<int> selectedIDs = GetSelection();
                SetSelection(selectedIDs.Count > 0 ? selectedIDs : new int[] { 0 }, TreeViewSelectionOptions.FireSelectionChanged);
            }
            else
            {
                SetSelection(new int[] { name.GetHashCode() }, TreeViewSelectionOptions.FireSelectionChanged);
            }
        }

        protected override void KeyEvent()
        {
            // 回车展开当前选中的item
            if((Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter) && Event.current.type == EventType.KeyDown)
            {
                IList<int> selectedIDs = GetSelection();
                if (selectedIDs.Count == 0)
                    return;

                bool isExpanded = IsExpanded(selectedIDs[0]);

                if (Event.current.alt)
                    SetExpandedRecursive(selectedIDs[0], !isExpanded);
                else
                    SetExpanded(selectedIDs[0], !isExpanded);
            }

            // 切换至AssetList View
            if (Event.current.keyCode == KeyCode.RightArrow && Event.current.type == EventType.KeyDown)
            {
                m_Owner.SetFocusToAssetList();
            }
        }
    }

    internal class BundleListMultiColumnHeader : MultiColumnHeader
    {
        Mode m_Mode;

        public enum Mode
        {
            LargeHeader,
            DefaultHeader,
            MinimumHeaderWithoutSorting
        }

        public BundleListMultiColumnHeader(MultiColumnHeaderState state)
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

    static class MyExtensionMethods
    {
        public static IOrderedEnumerable<T> Order<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector, bool ascending)
        {
            if (ascending)
            {
                return source.OrderBy(selector);
            }
            else
            {
                return source.OrderByDescending(selector);
            }
        }

        public static IOrderedEnumerable<T> ThenBy<T, TKey>(this IOrderedEnumerable<T> source, Func<T, TKey> selector, bool ascending)
        {
            if (ascending)
            {
                return source.ThenBy(selector);
            }
            else
            {
                return source.ThenByDescending(selector);
            }
        }
    }
}