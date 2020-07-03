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
            PSCount,
            TexMemorySize,
            DCCount,
            Fillrate,
            Command_Test,
            Command_Del,
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
            // CenterRectUsingSingleLineHeight(ref cellRect);

//             TreeViewItem<BundleListTreeElement> element = (TreeViewItem<BundleListTreeElement>)args.item;
//             if (element == null || element.data == null || element.data.bundleFileInfo == null)
//                 return;

//             switch (column)
//             {
//                 case MyColumns.Name:
//                     Rect iconRect = args.rowRect;
//                     iconRect.x += GetContentIndent(args.item);
//                     iconRect.width = 16f;

//                     if(element.data.bundleFileInfo.includeScene)
//                     {
//                         GUI.DrawTexture(iconRect, m_Icons[5]);
//                     }
//                     else
//                     {
//                         if (string.IsNullOrEmpty(element.data.bundleFileInfo.assetBundleName))
//                             GUI.DrawTexture(iconRect, m_Icons[0]);                        
//                         else
//                             GUI.DrawTexture(iconRect, m_Icons[1]);
//                     }

//                     // 选中icon时也可选中item
//                     if (Event.current.type == EventType.MouseDown && iconRect.Contains(Event.current.mousePosition))
//                         SelectionClick(args.item, false);

//                     // draw label
//                     args.rowRect = cellRect;
//                     base.RowGUI(args); 
//                     break;
//                 case MyColumns.Value1:
//                     if(element.data.bundleFileInfo.size > 0)
//                         GUI.Label(cellRect, new GUIContent(EditorUtility.FormatBytes(element.data.bundleFileInfo.size)));
//                     break;
//                 case MyColumns.Icon:
//                     if (!element.data.bundleFileInfo.isBundle)
//                         break;      // 不是bundle则忽略

//                     if (!element.data.bundleFileInfo.isValid)
//                         GUI.DrawTexture(cellRect, m_Icons[4], ScaleMode.ScaleToFit);
//                     else if (element.data.bundleFileInfo.hasMissingReference)
//                         GUI.DrawTexture(cellRect, m_Icons[3], ScaleMode.ScaleToFit);
//                     else
//                         GUI.DrawTexture(cellRect, m_Icons[2], ScaleMode.ScaleToFit);
//                     break;
//                 case MyColumns.Value2:
//                     if (item.data.bundleFileInfo.dependentOnBundleList != null)
//                     {
//                         string info = string.Format("[" + item.data.bundleFileInfo.dependentOnBundleList.Length + "]");
//                         if (item.data.bundleFileInfo.dependentOnBundleList.Length > 4)
//                         {
//                             GUIStyle style = new GUIStyle("BoldLabel");
//                             style.normal.textColor = Color.red;
//                             GUI.Label(cellRect, info, style);
//                         }
//                         else
//                         {
//                             GUI.Label(cellRect, info);
//                         }
//                     }
//                     break;
//                 case MyColumns.Button:
//                     IList<int> selectedIDs = GetSelection();
//                     if(selectedIDs.Count > 0 && item.data.id == selectedIDs[0])
//                     {
//                         if(GUI.Button(cellRect, "Fix"))
//                         {
//                             if (item.data.bundleFileInfo.includedAssetFileList == null)
//                                 return;
//                             foreach(var assetFileInfo in item.data.bundleFileInfo.includedAssetFileList)
//                             {
//                                 AssetBrowserUtil.FixRedundantMeshOfParticleSystemRender(assetFileInfo.assetPath);
//                             }
//                             AssetDatabase.SaveAssets();
//                         }
//                     }
//                     break;
//             }
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
                    headerContent = new GUIContent("PS Count"),
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
                    headerContent = new GUIContent("DC Count"),
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
                    headerContent = new GUIContent("Tex Memory Size"),
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
                    headerContent = new GUIContent("Fillrate"),
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
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Test"),
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
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Del"),
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
        public bool isFile;
        public AssetProfilerData assetProfilerData;
        
        public DirectoryProfilerData directoryProfilerData;

        public ParticleAssetTreeElement(string name, int depth, int id) : base(name, depth, id)
        { }
    }
}