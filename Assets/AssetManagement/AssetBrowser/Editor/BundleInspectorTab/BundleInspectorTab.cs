using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UITreeView;
using UnityEditor.IMGUI.Controls;
using NUnit.Framework;
using System.IO;

namespace AssetManagement.AssetBrowser
{
    public class BundleInspectorTab
    {
        private EditorWindow                m_Parent;
        private Rect                        m_Position;

        const float                         k_SplitterWidth = 3f;

        private bool                        m_ResizingVerticalSplitter;
        private Rect                        m_VerticalSplitterRect;
        private float                       m_VerticalSplitterPercent;

        private bool                        m_ResizingHorizontalSplitterLeft;
        private Rect                        m_HorizontalSplitterRectLeft;
        private float                       m_HorizontalSplitterPercentLeft;

        private bool                        m_ResizingHorizontalSplitterRight;
        private Rect                        m_HorizontalSplitterRectRight;
        private float                       m_HorizontalSplitterPercentRight;

        private const int                   k_SearchFieldHeight = 16;
        private SearchField                 m_SearchField;
        private BundleListTreeView          m_BundleListTreeView;
        private TreeViewState               m_BundleListTreeViewState;
        private MultiColumnHeaderState      m_BundleListMultiColumnHeaderState;

        private TreeViewState               m_BundleDetailTreeViewState;
        private BundleDetailTreeView        m_BundleDetailTreeView;

        private AssetListTreeView           m_AssetListTreeView;
        private TreeViewState               m_AssetListTreeViewState;
        private MultiColumnHeaderState      m_AssetListMultiColumnHeaderState;

        private ReferencedObjectTreeView    m_ReferencedObjectTreeView;
        private TreeViewState               m_ReferencedObjectTreeViewState;
        private MultiColumnHeaderState      m_ReferencedObjectMultiColumnHeaderState;

        private IssueCollectionTreeView     m_IssueCollectionTreeView;
        private TreeViewState               m_IssueCollectionTreeViewState;
        private MultiColumnHeaderState      m_IssueCollectionMultiColumnHeaderState;

        internal void OnEnable(Rect pos, EditorWindow parent)
        {
            m_Position = pos;
            m_Parent = parent;

            m_VerticalSplitterPercent = 0.7f;
            m_HorizontalSplitterPercentLeft = 0.3f;
            m_HorizontalSplitterPercentRight = 0.55f;

            Init();
        }

        internal void OnDisable()
        {
        }

        private void Init()
        {
            InitBundleListView();

            InitBundleDetailView();

            InitAssetListView();

            InitReferencedObjectView();

            InitIssueCollectionView();

            // create search field
            m_SearchField = new SearchField();
            m_SearchField.downOrUpArrowKeyPressed += m_BundleListTreeView.SetFocusAndEnsureSelectedItem;
        }

        internal void Reload()
        {
            string[] abNames = BundleFileInfo.PreParse();
            if (abNames.Length == 0)
                return;

            int startIndex = 0;
            List<BundleFileInfo> bundleList = new List<BundleFileInfo>();
            float startTime = Time.time;
            EditorApplication.update = delegate ()
            {
                bool isCancel = EditorUtility.DisplayCancelableProgressBar("资源分析中", abNames[startIndex], (float)startIndex / abNames.Length);

                BundleFileInfo.ParseBundle(ref bundleList, abNames[startIndex]);
                Debug.Log("Parsing AssetBundle: " + abNames[startIndex]);

                ++startIndex;
                if(isCancel || startIndex >= abNames.Length)
                {
                    EditorUtility.ClearProgressBar();
                    EditorApplication.update = null;
                    BundleFileInfo.PostParse();
                    Debug.LogFormat("!!!Parsing AssetBundles Finished，Count[{0}]   Time[{1}]", bundleList.Count, Time.time - startTime);

                    BundleListToTreeView(bundleList);

                    // 根据问题原因汇总遇到问题的资产
                    BundleFileInfo.CollectIssues(ref bundleList);
                    IssueCollectionToTreeView();
                }
            };
        }

        private void BundleListToTreeView(List<BundleFileInfo> bundleList)
        {
            // push bundle data to tree view
            List<BundleListTreeElement> treeElements = new List<BundleListTreeElement>();
            treeElements.Add(new BundleListTreeElement("Root", -1, 0));

            foreach (var bundle in bundleList)
            {
                BundleListTreeElement treeElement = new BundleListTreeElement(bundle.name, bundle.depth, bundle.hashName.GetHashCode());
                treeElement.bundleFileInfo = bundle;
                //Debug.Log($"{bundle.depth}  {bundle.name}   {bundle.hashName}");
                treeElements.Add(treeElement);
            }
            m_BundleListTreeView = new BundleListTreeView(m_BundleListTreeViewState, new BundleListMultiColumnHeader(m_BundleListMultiColumnHeaderState), new TreeModel<BundleListTreeElement>(treeElements), this);
            m_SearchField.downOrUpArrowKeyPressed += m_BundleListTreeView.SetFocusAndEnsureSelectedItem;

            // 触发之前选中的item，以便正确刷新detail view
            if (m_BundleListTreeViewState.selectedIDs != null)
                m_BundleListTreeView.SetSelection(m_BundleListTreeViewState.selectedIDs, TreeViewSelectionOptions.FireSelectionChanged);
        }

        private void IssueCollectionToTreeView()
        {
            List<IssueCollectionTreeElement> treeElements = new List<IssueCollectionTreeElement>();
            treeElements.Add(new IssueCollectionTreeElement("Root", -1, 0));

            if(IssueCollection.collections.Count > 0)
            {
                int startIndex = 1;
                Dictionary<int, IssueCollection>.Enumerator e = IssueCollection.collections.GetEnumerator();
                while(e.MoveNext())
                {
                    IssueCollectionTreeElement treeElement = new IssueCollectionTreeElement(e.Current.Value.name, 0, startIndex * 10000);
                    treeElement.issueInfo = e.Current.Value;
                    treeElements.Add(treeElement);

                    int subIndex = 1;
                    foreach(var assetFileInfo in e.Current.Value.assetCollection)
                    {
                        IssueCollectionTreeElement item = new IssueCollectionTreeElement(assetFileInfo.name, 1, startIndex * 10000 + subIndex++);
                        item.issueInfo = e.Current.Value;
                        treeElements.Add(item);
                    }

                    ++startIndex;
                }
                e.Dispose();
            }

            m_IssueCollectionTreeView = new IssueCollectionTreeView(m_IssueCollectionTreeViewState, new IssueCollectionMultiColumnHeader(m_IssueCollectionMultiColumnHeaderState), new TreeModel<IssueCollectionTreeElement>(treeElements), this);
        }

        private void InitBundleListView()
        {
            // create view state for bundle list tree view
            if (m_BundleListTreeViewState == null)
            {
                m_BundleListTreeViewState = new TreeViewState();
            }

            // create header state for bundle list tree view
            bool firstInit = m_BundleListMultiColumnHeaderState == null;
            var headerState = BundleListTreeView.CreateDefaultMultiColumnHeaderState();
            if (MultiColumnHeaderState.CanOverwriteSerializedFields(m_BundleListMultiColumnHeaderState, headerState))
                MultiColumnHeaderState.OverwriteSerializedFields(m_BundleListMultiColumnHeaderState, headerState);
            m_BundleListMultiColumnHeaderState = headerState;
            BundleListMultiColumnHeader multiColumnHeader = new BundleListMultiColumnHeader(headerState);
            if (firstInit)
                multiColumnHeader.ResizeToFit();

            m_BundleListTreeView = new BundleListTreeView(m_BundleListTreeViewState, multiColumnHeader, GetBundleListData(), this);
        }

        private void InitBundleDetailView()
        {
            // create bundle detail tree view
            if (m_BundleDetailTreeViewState == null)
            {
                m_BundleDetailTreeViewState = new TreeViewState();
            }
            m_BundleDetailTreeView = new BundleDetailTreeView(m_BundleDetailTreeViewState, GetBundleDetailData(), this);
        }

        private void InitAssetListView()
        {
            // create view state for asset list tree view
            if (m_AssetListTreeViewState == null)
            {
                m_AssetListTreeViewState = new TreeViewState();
            }

            // create header state for asset list tree view
            bool firstInit = m_AssetListMultiColumnHeaderState == null;
            var headerState = AssetListTreeView.CreateDefaultMultiColumnHeaderState();
            if (MultiColumnHeaderState.CanOverwriteSerializedFields(m_AssetListMultiColumnHeaderState, headerState))
                MultiColumnHeaderState.OverwriteSerializedFields(m_AssetListMultiColumnHeaderState, headerState);
            m_AssetListMultiColumnHeaderState = headerState;
            AssetListMultiColumnHeader multiColumnHeader = new AssetListMultiColumnHeader(headerState);
            if (firstInit)
                multiColumnHeader.ResizeToFit();

            m_AssetListTreeView = new AssetListTreeView(m_AssetListTreeViewState, multiColumnHeader, GetAssetListData(), this);
        }
        
        private void InitReferencedObjectView()
        {
            // create view state for asset list tree view
            if (m_ReferencedObjectTreeViewState == null)
            {
                m_ReferencedObjectTreeViewState = new TreeViewState();
            }

            // create header state for asset list tree view
            bool firstInit = m_ReferencedObjectMultiColumnHeaderState == null;
            var headerState = ReferencedObjectTreeView.CreateDefaultMultiColumnHeaderState();
            if (MultiColumnHeaderState.CanOverwriteSerializedFields(m_ReferencedObjectMultiColumnHeaderState, headerState))
                MultiColumnHeaderState.OverwriteSerializedFields(m_ReferencedObjectMultiColumnHeaderState, headerState);
            m_ReferencedObjectMultiColumnHeaderState = headerState;
            ReferencedObjectMultiColumnHeader multiColumnHeader = new ReferencedObjectMultiColumnHeader(headerState);
            if (firstInit)
                multiColumnHeader.ResizeToFit();

            m_ReferencedObjectTreeView = new ReferencedObjectTreeView(m_ReferencedObjectTreeViewState, multiColumnHeader, GetReferencedObjectData(), this);
        }
        
        private void InitIssueCollectionView()
        {
            // create view state for asset list tree view
            if (m_IssueCollectionTreeViewState == null)
            {
                m_IssueCollectionTreeViewState = new TreeViewState();
            }

            // create header state for asset list tree view
            bool firstInit = m_IssueCollectionMultiColumnHeaderState == null;
            var headerState = IssueCollectionTreeView.CreateDefaultMultiColumnHeaderState();
            if (MultiColumnHeaderState.CanOverwriteSerializedFields(m_IssueCollectionMultiColumnHeaderState, headerState))
                MultiColumnHeaderState.OverwriteSerializedFields(m_IssueCollectionMultiColumnHeaderState, headerState);
            m_IssueCollectionMultiColumnHeaderState = headerState;
            IssueCollectionMultiColumnHeader multiColumnHeader = new IssueCollectionMultiColumnHeader(headerState);
            if (firstInit)
                multiColumnHeader.ResizeToFit();

            m_IssueCollectionTreeView = new IssueCollectionTreeView(m_IssueCollectionTreeViewState, multiColumnHeader, GetIssueCollectionData(), this);
        }

        private TreeModel<AssetListTreeElement> GetAssetListData()
        {
            List<AssetListTreeElement> treeElements = new List<AssetListTreeElement>();
            treeElements.Add(new AssetListTreeElement("Root", -1, 0));

            return new TreeModel<AssetListTreeElement>(treeElements);
        }

        private TreeModel<BundleDetailTreeElement> GetBundleDetailData()
        {
            List<BundleDetailTreeElement> treeElements = new List<BundleDetailTreeElement>();
            treeElements.Add(new BundleDetailTreeElement("Root", -1, 0));

            return new TreeModel<BundleDetailTreeElement>(treeElements);
        }

        private TreeModel<BundleListTreeElement> GetBundleListData()
        {
            List<BundleListTreeElement> treeElements = new List<BundleListTreeElement>();
            treeElements.Add(new BundleListTreeElement("Root", -1, 0));

            return new TreeModel<BundleListTreeElement>(treeElements);
        }

        private TreeModel<ReferencedObjectTreeElement> GetReferencedObjectData()
        {
            List<ReferencedObjectTreeElement> treeElements = new List<ReferencedObjectTreeElement>();
            treeElements.Add(new ReferencedObjectTreeElement("Root", -1, 0));

            return new TreeModel<ReferencedObjectTreeElement>(treeElements);
        }

        private TreeModel<IssueCollectionTreeElement> GetIssueCollectionData()
        {
            List<IssueCollectionTreeElement> treeElements = new List<IssueCollectionTreeElement>();
            treeElements.Add(new IssueCollectionTreeElement("Root", -1, 0));

            return new TreeModel<IssueCollectionTreeElement>(treeElements);
        }

        internal void OnGUI(Rect pos)
        {
            m_Position = pos;
            
            HandleResize();
            UpdateSplitterRect();

            Rect scope1 = new Rect(m_Position.x, 
                                   m_Position.y, 
                                   m_HorizontalSplitterRectLeft.x - m_Position.x, 
                                   k_SearchFieldHeight);
            if (m_BundleListTreeView != null)
            {
                m_BundleListTreeView.searchString = m_SearchField.OnToolbarGUI(scope1, m_BundleListTreeView.searchString);
            }

            Rect scope2 = new Rect(m_Position.x, 
                                   m_Position.y + k_SearchFieldHeight,
                                   m_HorizontalSplitterRectLeft.x - m_Position.x, 
                                   m_VerticalSplitterRect.y - m_Position.y - k_SearchFieldHeight);
            m_BundleListTreeView.OnGUI(scope2);

            Rect scope3 = new Rect(m_VerticalSplitterRect.x,
                                   m_VerticalSplitterRect.y + k_SplitterWidth,
                                   m_HorizontalSplitterRectLeft.x - m_Position.x,
                                   m_Position.height - (m_VerticalSplitterRect.y - m_Position.y + k_SplitterWidth));
            m_BundleDetailTreeView.OnGUI(scope3);

            Rect scope4 = new Rect(m_HorizontalSplitterRectLeft.x + k_SplitterWidth,
                                   m_HorizontalSplitterRectLeft.y,
                                   m_HorizontalSplitterRectRight.x - m_HorizontalSplitterRectLeft.x - k_SplitterWidth,
                                   m_HorizontalSplitterRectRight.height);
            m_AssetListTreeView.OnGUI(scope4);

            Rect scope5 = new Rect(m_HorizontalSplitterRectRight.x + k_SplitterWidth,
                                   m_HorizontalSplitterRectRight.y,
                                   m_Position.width - (m_HorizontalSplitterRectRight.x + k_SplitterWidth - m_Position.x),
                                   m_HorizontalSplitterRectRight.height);
            m_ReferencedObjectTreeView.OnGUI(scope5);

            Rect scope6 = new Rect(m_HorizontalSplitterRectLeft.x + k_SplitterWidth,
                                   m_VerticalSplitterRect.y + k_SplitterWidth,
                                   m_Position.width - (m_HorizontalSplitterRectLeft.x - m_Position.x + k_SplitterWidth),
                                   m_Position.height - (m_VerticalSplitterRect.y - m_Position.y + k_SplitterWidth));
            m_IssueCollectionTreeView.OnGUI(scope6);
        }

        private void UpdateSplitterRect()
        {
            m_VerticalSplitterRect = new Rect(
                m_Position.x,
                m_Position.y + (int)(m_Position.height * m_VerticalSplitterPercent),
                m_Position.width,
                k_SplitterWidth);

            m_HorizontalSplitterRectLeft = new Rect(
                m_Position.x + (int)(m_Position.width * m_HorizontalSplitterPercentLeft),
                m_Position.y,
                k_SplitterWidth,
                m_Position.height);

            m_HorizontalSplitterRectRight = new Rect(
                m_Position.x + (int)(m_Position.width * m_HorizontalSplitterPercentRight),
                m_Position.y,
                k_SplitterWidth,
                m_VerticalSplitterRect.y - m_Position.y);
        }

        private void HandleResize()
        {
            // handle vertical splitter rect
            EditorGUIUtility.AddCursorRect(m_VerticalSplitterRect, MouseCursor.ResizeVertical);

            if (Event.current.type == EventType.MouseDown && m_VerticalSplitterRect.Contains(Event.current.mousePosition))
                m_ResizingVerticalSplitter = true;

            if (Event.current.type == EventType.MouseUp)
                m_ResizingVerticalSplitter = false;

            if (m_ResizingVerticalSplitter)
            {
                m_VerticalSplitterPercent = Mathf.Clamp((Event.current.mousePosition.y - m_Position.y) / m_Position.height, 0.2f, 0.8f);
            }

            // handle horizontal left splitter rect
            EditorGUIUtility.AddCursorRect(m_HorizontalSplitterRectLeft, MouseCursor.ResizeHorizontal);

            if (Event.current.type == EventType.MouseDown && m_HorizontalSplitterRectLeft.Contains(Event.current.mousePosition))
                m_ResizingHorizontalSplitterLeft = true;

            if (Event.current.type == EventType.MouseUp)
                m_ResizingHorizontalSplitterLeft = false;

            if (m_ResizingHorizontalSplitterLeft)
            {
                m_HorizontalSplitterPercentLeft = Mathf.Clamp((Event.current.mousePosition.x - m_Position.x) / m_Position.width, 0.2f, Mathf.Min(m_HorizontalSplitterPercentRight - 0.1f, 0.6f));
            }
            
            // handle horizontal right splitter rect
            EditorGUIUtility.AddCursorRect(m_HorizontalSplitterRectRight, MouseCursor.ResizeHorizontal);

            if (Event.current.type == EventType.MouseDown && m_HorizontalSplitterRectRight.Contains(Event.current.mousePosition))
                m_ResizingHorizontalSplitterRight = true;

            if (Event.current.type == EventType.MouseUp)
                m_ResizingHorizontalSplitterRight = false;

            if (m_ResizingHorizontalSplitterRight)
            {
                m_HorizontalSplitterPercentRight = Mathf.Clamp((Event.current.mousePosition.x - m_Position.x) / m_Position.width, m_HorizontalSplitterPercentLeft + 0.1f, 0.9f);
            }

            if (m_ResizingVerticalSplitter || m_ResizingHorizontalSplitterLeft || m_ResizingHorizontalSplitterRight)
                m_Parent.Repaint();
        }

        // BundleList选中后处理
        internal void OnPostBundleListSelection(BundleListTreeElement bundleTreeElement)
        {
            // update bundle detail view
            m_BundleDetailTreeView.UpdateData(bundleTreeElement);

            // selecte the appropriate folder
            if (bundleTreeElement != null)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(bundleTreeElement.bundleFileInfo.guid);
                if (AssetDatabase.IsValidFolder(assetPath))
                {
                    Object obj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                    if (obj != null)
                        Selection.activeObject = obj;
                }
            }

            // update asset list view
            m_AssetListTreeView.UpdateData(bundleTreeElement);
        }

        // AssetList选中后处理
        internal void OnPostAssetListSelection(AssetListTreeElement assetTreeElement)
        {
            if (assetTreeElement != null)
            {
                Object obj = AssetDatabase.LoadAssetAtPath<Object>(assetTreeElement.assetInfo.assetPath);
                if (obj != null)
                    Selection.activeObject = obj;
            }

            m_ReferencedObjectTreeView.UpdateData(assetTreeElement);
        }

        internal void SetFocusToBundleList(string name)
        {
            m_BundleListTreeView.SetFocusAndSelect(name);
        }

        internal void SetFocusToAssetList()
        {
            m_AssetListTreeView.SetFocusAndSelectLast();
        }

        internal void SetFocusToReferencedObjectList()
        {
            m_ReferencedObjectTreeView.SetFocusAndSelectLast();
        }
    }
}