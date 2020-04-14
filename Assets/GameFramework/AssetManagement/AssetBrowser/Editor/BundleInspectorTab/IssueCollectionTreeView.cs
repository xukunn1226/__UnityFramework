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
    internal class IssueCollectionTreeView : TreeViewWithTreeModel<IssueCollectionTreeElement>
    {
        static private Texture2D[] m_Icons =
        {
            EditorGUIUtility.FindTexture("console.warnicon.sml"),
            EditorGUIUtility.FindTexture("console.erroricon.sml"),
        };

        enum MyColumns
        {
            Name1,
            Name2,
            Name3,
            Name4,
            Name5,
            Name6,
        }

        enum SortOption
        {
            Name1,
            Name2,
            Name3,
            Name4,
            Name5,
            Name6,
        }

        // Sort options per column
        SortOption[] m_SortOptions =
        {
            SortOption.Name1,
            SortOption.Name2,
            SortOption.Name3,
            SortOption.Name4,
            SortOption.Name5,
            SortOption.Name6,
        };

        private BundleInspectorTab m_Owner;

        public IssueCollectionTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader, TreeModel<IssueCollectionTreeElement> model, BundleInspectorTab owner)
       : base(state, multiColumnHeader, model)
        {
            m_Owner = owner;
            showBorder = true;
            showAlternatingRowBackgrounds = true;

            extraSpaceBeforeIconAndLabel = 0;
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

            var myTypes = rootItem.children.Cast<TreeViewItem<IssueCollectionTreeElement>>();

            var orderedQuery = InitialOrder(myTypes, sortedColumns);

            for (int i = 1; i < sortedColumns.Length; i++)
            {
                SortOption sortOption = m_SortOptions[sortedColumns[i]];
                bool ascending = multiColumnHeader.IsSortedAscending(sortedColumns[i]);

                switch (sortOption)
                {
                    case SortOption.Name1:
                        orderedQuery = orderedQuery.ThenBy(l => l.data.issueInfo.name, ascending);
                        break;
                    case SortOption.Name2:
                        orderedQuery = orderedQuery.ThenBy(l => l.data.issueInfo.type.Name, ascending);
                        break;
                    case SortOption.Name3:
                        orderedQuery = orderedQuery.ThenBy(l => l.data.issueInfo.assetPath, ascending);
                        break;
                    case SortOption.Name4:
                        orderedQuery = orderedQuery.ThenBy(l => l.data.issueInfo.isBuiltIn, ascending);
                        break;
                    case SortOption.Name5:
                        orderedQuery = orderedQuery.ThenBy(l => l.data.issueInfo.isExternal, ascending);
                        break;
                }
            }

            rootItem.children = orderedQuery.Cast<TreeViewItem>().ToList();
        }

        IOrderedEnumerable<TreeViewItem<IssueCollectionTreeElement>> InitialOrder(IEnumerable<TreeViewItem<IssueCollectionTreeElement>> myTypes, int[] history)
        {
            SortOption sortOption = m_SortOptions[history[0]];
            bool ascending = multiColumnHeader.IsSortedAscending(history[0]);
            switch (sortOption)
            {
                case SortOption.Name1:
                    return myTypes.Order(l => l.data.issueInfo.name, ascending);
                case SortOption.Name2:
                    return myTypes.Order(l => l.data.issueInfo.type.Name, ascending);
                case SortOption.Name3:
                    return myTypes.Order(l => l.data.issueInfo.assetPath, ascending);
                case SortOption.Name4:
                    return myTypes.Order(l => l.data.issueInfo.isBuiltIn, ascending);
                case SortOption.Name5:
                    return myTypes.Order(l => l.data.issueInfo.isExternal, ascending);
                default:
                    //Assert.IsTrue(false, "Unhandled enum");
                    break;
            }

            // default
            return myTypes.Order(l => l.data.issueInfo.name, ascending);
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = (TreeViewItem<IssueCollectionTreeElement>)args.item;

            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), item, (MyColumns)args.GetColumn(i), ref args);
            }
        }
        
        void CellGUI(Rect cellRect, TreeViewItem<IssueCollectionTreeElement> item, MyColumns column, ref RowGUIArgs args)
        {
            GUIStyle labelStyleLeft = new GUIStyle("label");
            labelStyleLeft.alignment = TextAnchor.MiddleLeft;

            GUIStyle labelStyleCenter = new GUIStyle("label");
            labelStyleCenter.alignment = TextAnchor.MiddleCenter;

            GUIStyle labelStyleRight = new GUIStyle("label");
            labelStyleRight.alignment = TextAnchor.MiddleRight;

            CenterRectUsingSingleLineHeight(ref cellRect);

            TreeViewItem<IssueCollectionTreeElement> element = (TreeViewItem<IssueCollectionTreeElement>)args.item;
            //if (element == null || element.data == null || element.data.issueInfo == null)
            //    return;

            switch (column)
            {
                case MyColumns.Name1:
                    // draw label
                    args.rowRect = cellRect;
                    base.RowGUI(args);

                    if (item.depth == 0)
                    {
                        GUI.Label(cellRect, string.Format("[{0}]", element.data.issueInfo.assetCollection.Count), labelStyleRight);
                    }
                    break;
                case MyColumns.Name2:       // type
                    if (item.depth == 0)
                        GUI.Label(cellRect, new GUIContent(element.data.issueInfo.type.Name), labelStyleCenter);
                    break;
                case MyColumns.Name3:       // asset path
                    if (item.depth == 0)
                        GUI.Label(cellRect, new GUIContent(element.data.issueInfo.assetPath, element.data.issueInfo.assetPath), labelStyleLeft);
                    else
                    {
                        int remainder = item.id % 10000;
                        GUI.Label(cellRect, new GUIContent(element.data.issueInfo.assetCollection[remainder-1].assetPath), labelStyleLeft);
                    }
                    break;
                case MyColumns.Name4:       // isBuiltin
                    if(item.depth == 0)
                    {
                        if(item.data.issueInfo.isBuiltIn)
                            GUI.DrawTexture(cellRect, m_Icons[1], ScaleMode.ScaleToFit);
                    }
                    break;
                case MyColumns.Name5:       // isExternal
                    if(item.depth == 0)
                    {
                        if (item.data.issueInfo.isExternal)
                            GUI.DrawTexture(cellRect, m_Icons[1], ScaleMode.ScaleToFit);
                    }
                    break;
                case MyColumns.Name6:       // fix
                    if(item.depth == 0)
                    {
                        GUIStyle buttonStyle = new GUIStyle("ButtonMid");
                        if (item.data.issueInfo.hasFixed)
                            buttonStyle.normal.textColor = Color.green;
                        if (GUI.Button(cellRect, "Fix", buttonStyle))
                        {
                            Fix(item);
                        }
                    }
                    break;
            }
        }

        private void Fix(TreeViewItem<IssueCollectionTreeElement> item)
        {
            if (item == null || item.data == null || item.data.issueInfo == null)
                return;

            // fix external asset
            if(item.data.issueInfo.isExternal)
            {
                SelectObject(item.data.issueInfo.assetPath);
                EditorUtility.DisplayDialog("Info", "外部资产没有设置assetbundle name，请检查是否需要设置或使用了错误的资产", "OK");
                return;
            }

            // fix builtin shader
            if(item.data.issueInfo.isBuiltIn && item.data.issueInfo.type.Name == "Shader")
            {
                string newShaderPath = EditorUtility.OpenFilePanel("", "Assets/", "shader");
                if (string.IsNullOrEmpty(newShaderPath))
                    return;

                string path = AssetBrowserUtil.GetRelativePathToProjectFolder(newShaderPath);
                Shader newShader = AssetDatabase.LoadAssetAtPath<Shader>(path);
                if(newShader == null)
                {
                    return;
                }

                foreach (var fi in item.data.issueInfo.assetCollection)
                {
                    // issueInfo.name记录了内置shader name
                    AssetBrowserUtil.ReplaceShader(fi.assetPath, item.data.issueInfo.name, newShader.name);
                }
                item.data.issueInfo.hasFixed = true;
                Debug.Log("The fix of builtin shader is done.");
            }

            // fix builtin mesh
            if (item.data.issueInfo.isBuiltIn && item.data.issueInfo.type.Name == "Mesh")
            {
                string newMeshPath = EditorUtility.OpenFilePanel("", "Assets/", "asset");
                if (string.IsNullOrEmpty(newMeshPath))
                    return;

                string path = AssetBrowserUtil.GetRelativePathToProjectFolder(newMeshPath);
                Mesh newMesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if(newMesh == null)
                {
                    return;
                }

                foreach(var fi in item.data.issueInfo.assetCollection)
                {
                    AssetBrowserUtil.ReplaceMesh(fi.assetPath, item.data.issueInfo.name, newMesh);
                }
                item.data.issueInfo.hasFixed = true;
                Debug.Log("The fix of builtin mesh is done.");
            }

            // fix builtin texture
            if (item.data.issueInfo.isBuiltIn && item.data.issueInfo.type.Name == "Texture2D")
            {
                string newTexturePath = EditorUtility.OpenFilePanelWithFilters("", "Assets/", new string[] { "Image files", "png,tga,jpg,jpeg" });
                if (string.IsNullOrEmpty(newTexturePath))
                    return;
                               
                string path = AssetBrowserUtil.GetRelativePathToProjectFolder(newTexturePath);
                Texture2D newTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (newTexture == null)
                {
                    return;
                }

                foreach(var fi in item.data.issueInfo.assetCollection)
                {
                    AssetBrowserUtil.ReplaceTexture(fi.assetPath, item.data.issueInfo.name, newTexture);
                }
                item.data.issueInfo.hasFixed = true;
                Debug.Log("The fix of builtin texture is done.");
            }

            // fix builtin material
            if (item.data.issueInfo.isBuiltIn && item.data.issueInfo.type.Name == "Material")
            {
                string newMaterialPath = EditorUtility.OpenFilePanelWithFilters("", "Assets/", new string[] { "Material files", "mat" });
                if (string.IsNullOrEmpty(newMaterialPath))
                    return;

                string path = AssetBrowserUtil.GetRelativePathToProjectFolder(newMaterialPath);
                Material newMaterial = AssetDatabase.LoadAssetAtPath<Material>(path);
                if(newMaterial == null)
                {
                    return;
                }

                foreach(var fi in item.data.issueInfo.assetCollection)
                {
                    AssetBrowserUtil.ReplaceMaterial(fi.assetPath, item.data.issueInfo.name, newMaterial);
                }
                item.data.issueInfo.hasFixed = true;
                Debug.Log("The fix of builtin material is done.");
            }

            // fix builtin font
            if(item.data.issueInfo.isBuiltIn && item.data.issueInfo.type.Name == "Font")
            {
                string newFontPath = EditorUtility.OpenFilePanelWithFilters("", "Assets/", new string[] { "Font files", "fontsettings" });
                if (string.IsNullOrEmpty(newFontPath))
                    return;

                string path = AssetBrowserUtil.GetRelativePathToProjectFolder(newFontPath);
                Font newFont = AssetDatabase.LoadAssetAtPath<Font>(path);
                if (newFont == null)
                {
                    return;
                }

                foreach(var fi in item.data.issueInfo.assetCollection)
                {
                    AssetBrowserUtil.ReplaceFont(fi.assetPath, newFont);
                }
                item.data.issueInfo.hasFixed = true;
                Debug.Log("The fix of builtin font is done.");
            }

            AssetDatabase.SaveAssets();
        }

        private bool IsSceneAsset(UnityEngine.Object asset)
        {
            return asset.GetType().Name == "SceneAsset";
        }

        private void SelectObject(string assetPath)
        {
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (obj != null)
                Selection.activeObject = obj;
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if (selectedIds == null || selectedIds.Count == 0)
                return;

            TreeViewItem<IssueCollectionTreeElement> item = (TreeViewItem<IssueCollectionTreeElement>)FindItem(selectedIds[0], rootItem);
            if(item.data.depth == 1)
            {
                string path = item.data.issueInfo.assetCollection[item.id % 10000 - 1].assetPath;
                SelectObject(path);
            }
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
                    width = 240,
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
                    width = 350,
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
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("", "Fix"),
                    headerTextAlignment = TextAlignment.Right,
                    sortedAscending = false,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 80,
                    minWidth = 50,
                    maxWidth = 120,
                    autoResize = false,
                    allowToggleVisibility = false,
                    canSort = false
                },
            };

            Assert.AreEqual(columns.Length, Enum.GetValues(typeof(MyColumns)).Length, "Number of columns should match number of enum values: You probably forgot to update one of them.");

            return new MultiColumnHeaderState(columns);
        }
    }

    internal class IssueCollectionMultiColumnHeader : MultiColumnHeader
    {
        Mode m_Mode;

        public enum Mode
        {
            LargeHeader,
            DefaultHeader,
            MinimumHeaderWithoutSorting
        }

        public IssueCollectionMultiColumnHeader(MultiColumnHeaderState state)
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