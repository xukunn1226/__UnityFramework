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
        static private Texture2D[] m_Icons =
        {
            EditorGUIUtility.FindTexture("Prefab Icon"),
            EditorGUIUtility.FindTexture("Folder Icon"),
        };

        enum MyColumns
        {
            Name,
            PSCount,
            DCCount,
            TexMemorySize,
            TexMemorySizeOnAndroid,
            Fillrate,
            Command_Test,
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
            CenterRectUsingSingleLineHeight(ref cellRect);

            TreeViewItem<ParticleAssetTreeElement> element = (TreeViewItem<ParticleAssetTreeElement>)args.item;
            if (element == null || element.data == null || (element.data.assetProfilerData == null && element.data.directoryProfilerData == null))
                return;

            switch (column)
            {
                case MyColumns.Name:
                    Rect iconRect = args.rowRect;
                    iconRect.x += GetContentIndent(args.item);
                    iconRect.width = 16f;

                    if (element.data.assetProfilerData != null)
                        GUI.DrawTexture(iconRect, m_Icons[0]);                        
                    else
                        GUI.DrawTexture(iconRect, m_Icons[1]);

                    // 选中icon时也可选中item
                    if (Event.current.type == EventType.MouseDown && iconRect.Contains(Event.current.mousePosition))
                        SelectionClick(args.item, false);

                    // draw label
                    args.rowRect = cellRect;
                    base.RowGUI(args); 
                    break;
                case MyColumns.PSCount:
                    if(element.data.assetProfilerData != null)
                    {
                        GUIStyle style = new GUIStyle("Label");
                        style.alignment = TextAnchor.MiddleRight;
                        int count = element.data.assetProfilerData.profilerData.componentCount;
                        if(count < ParticleProfiler.kRecommendParticleCompCount)
                        {
                            style.normal.textColor = Color.black;
                        }
                        else
                        {
                            style.normal.textColor = Color.red;
                        }
                        GUI.Label(cellRect, count.ToString(), style);
                    }
                    break;
                case MyColumns.DCCount:
                    if(element.data.assetProfilerData != null)
                    {
                        GUIStyle style = new GUIStyle("Label");
                        style.alignment = TextAnchor.MiddleRight;
                        int count = element.data.assetProfilerData.profilerData.maxDrawCall;
                        if(count < ParticleProfiler.kRecommendDrawCallCount)
                        {
                            style.normal.textColor = Color.black;
                        }
                        else
                        {
                            style.normal.textColor = Color.red;
                        }
                        GUI.Label(cellRect, count.ToString(), style);
                    }
                    break;
                case MyColumns.TexMemorySize:
                    if(element.data.assetProfilerData != null)
                    {
                        GUIStyle style = new GUIStyle("Label");
                        style.alignment = TextAnchor.MiddleRight;
                        long size = element.data.assetProfilerData.profilerData.textureMemory;
                        if(size < ParticleProfiler.kRecommendedTextureMemorySize)
                        {
                            style.normal.textColor = Color.black;
                        }
                        else
                        {
                            style.normal.textColor = Color.red;
                        }
                        GUI.Label(cellRect, EditorUtility.FormatBytes(size), style);
                    }
                    break;
                case MyColumns.TexMemorySizeOnAndroid:
                    if(element.data.assetProfilerData != null)
                    {
                        GUIStyle style = new GUIStyle("Label");
                        style.alignment = TextAnchor.MiddleRight;
                        long size = element.data.assetProfilerData.profilerData.textureMemoryOnAndroid;
                        if(size < ParticleProfiler.kRecommendedTextureMemorySize)
                        {
                            style.normal.textColor = Color.black;
                        }
                        else
                        {
                            style.normal.textColor = Color.red;
                        }
                        GUI.Label(cellRect, EditorUtility.FormatBytes(size), style);
                    }
                    break;
                case MyColumns.Fillrate:
                    if(element.data.assetProfilerData != null)
                    {
                        GUIStyle style = new GUIStyle("Label");
                        style.alignment = TextAnchor.MiddleRight;
                        float rate = element.data.assetProfilerData.overdrawData.m_Fillrate;
                        if(rate < ParticleProfiler.kRecommendFillrate)
                        {
                            style.normal.textColor = Color.black;
                        }
                        else
                        {
                            style.normal.textColor = Color.red;
                        }
                        GUI.Label(cellRect, string.Format("{0:0.00}", rate), style);
                    }
                    break;
                case MyColumns.Command_Test:
                    IList<int> selectedIDs = GetSelection();
                    if(selectedIDs.Count > 0 && item.data.id == selectedIDs[0])
                    {
                        if(GUI.Button(cellRect, "Test"))
                        {
                            m_Owner.ExecuteTest(item.data);
                        }
                    }
                    break;
            }
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if (selectedIds.Count == 0)
            {
                m_Owner.OnPostAssetListSelection(null);
                return;
            }

            TreeViewItem<ParticleAssetTreeElement> item = (TreeViewItem<ParticleAssetTreeElement>)FindItem(selectedIds[0], rootItem);
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
                    width = 400,
                    minWidth = 80,
                    //maxWidth = 60,
                    autoResize = false,
                    allowToggleVisibility = true,
                    canSort = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("PS"),
                    headerTextAlignment = TextAlignment.Right,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 60,
                    minWidth = 60,
                    maxWidth = 120,
                    autoResize = false,
                    allowToggleVisibility = false,
                    canSort = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("DC"),
                    headerTextAlignment = TextAlignment.Right,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 60,
                    minWidth = 60,
                    maxWidth = 120,
                    autoResize = false,
                    allowToggleVisibility = false,
                    canSort = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Tex Memory Size"),
                    contextMenuText = "Info",
                    headerTextAlignment = TextAlignment.Right,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 100,
                    minWidth = 100,
                    maxWidth = 160,
                    autoResize = false,
                    allowToggleVisibility = true,
                    canSort = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Tex Memory Size On Android"),
                    contextMenuText = "Info",
                    headerTextAlignment = TextAlignment.Right,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 100,
                    minWidth = 100,
                    maxWidth = 180,
                    autoResize = false,
                    allowToggleVisibility = true,
                    canSort = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Fillrate"),
                    contextMenuText = "Info",
                    headerTextAlignment = TextAlignment.Right,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 60,
                    minWidth = 50,
                    maxWidth = 120,
                    autoResize = false,
                    allowToggleVisibility = true,
                    canSort = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Test"),
                    contextMenuText = "Info",
                    headerTextAlignment = TextAlignment.Right,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 60,
                    minWidth = 60,
                    maxWidth = 100,
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
        public AssetProfilerData assetProfilerData;
        
        public DirectoryProfilerData directoryProfilerData;

        public ParticleAssetTreeElement(string name, int depth, int id) : base(name, depth, id)
        { }
    }
}