using System;
using System.Collections.Generic;
using Framework.AssetManagement.AssetTreeView;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framework.AssetManagement.AssetPackageEditor.Editor
{
    public enum pathType
    {
        file = 0,
        dir = 1,
    }

    public class AssetPackageEditorTreeViewItem : BaseTreeViewItem
    {
        public string Path;
        public pathType PathType;
        public string packageID;
        public AssetPackageBuildBundleType buildBundleType;
        public AssetPackageEditorTreeViewItem()
        {
        }

        public AssetPackageEditorTreeViewItem(int id, int depth, string name, string path) : base(id,
            depth, name)
        {
            this.Path = path;
        }

        public override MultiColumnHeaderState GetMutiColumnHeader(float width)
        {
            var colums = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent(EditorGUIUtility.IconContent("d_Favorite")),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 30,
                    minWidth = 30,
                    maxWidth = 30,
                    autoResize = true,
                    allowToggleVisibility = false,
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("FolderName"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 70,
                    minWidth = 60,
                    autoResize = true,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Package ID"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 70,
                    minWidth = 60,
                    autoResize = true,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("BundleBuid Setting"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 70,
                    minWidth = 60,
                    autoResize = true,
                    allowToggleVisibility = false
                },

            };
            var state = new MultiColumnHeaderState(colums);
            return state;
        }
    }

    public class AssetPackageEditorTreeView : BaseAssetTreeView<AssetPackageEditorTreeViewItem>
    {
        private List<AssetPackageEditorTreeViewItem> selectedItems;
        public AssetPackageEditorTreeView(TreeViewState treeViewState, MultiColumnHeader multiColumnHeader) : base(
            treeViewState, multiColumnHeader)
        {
            //getNewSelectionOverride = GetNewSelectionFunction;
            foldoutOverride = DrawFoldout;
        }

        private List<AssetPackageEditorTreeViewItem> allItems;
        private bool DrawFoldout(Rect foldoutRect, bool expandedState, GUIStyle foldoutStyle)
        {
            return expandedState;
        }

        public void SetAllItemsMap(List<AssetPackageEditorTreeViewItem> allitems)
        {
            this.allItems = allitems;
        }

        public void HandleEachItem(Action<AssetPackageEditorTreeViewItem> itemTodo)
        {
            foreach (var item in allItems)
            {
                string path = item.Path;
                itemTodo?.Invoke(item);
            }
        }

        public void UpatePackageID(string targetPkgID, string newID)
        {
            if (m_itemData.Count > 0)
            {
                UpatePackageIDReverso(m_itemData[0].data,targetPkgID,newID);
            }
        }

        void UpatePackageIDReverso(AssetPackageEditorTreeViewItem root,string targetPkgID, string newID)
        {
            foreach (var child in root.Children)
            {
                var childItem = child as AssetPackageEditorTreeViewItem;
                if (childItem != null)
                {
                    if (childItem.packageID == targetPkgID)
                    {
                        childItem.packageID = newID;
                    }

                    UpatePackageIDReverso(childItem, targetPkgID, newID);
                }
            }
        }
        
        public override void GetData(List<AssetPackageEditorTreeViewItem> datas)
        {
            m_itemData = new List<TreeViewItem<AssetPackageEditorTreeViewItem>>();
            foreach (var item in datas)
            {
                TreeViewItem<AssetPackageEditorTreeViewItem> tv =
                    new TreeViewItem<AssetPackageEditorTreeViewItem>(item.mId, item.depth, item.DisPlayName, item);
                m_itemData.Add(tv);
            }

            Reload();
        }

        protected override void DoubleClickedItem(int id)
        {
            var data = GetItemFromID(id);
            string itemPath= data.Path.Replace(UnityEngine.Application.dataPath, "");
            itemPath = "Assets" + itemPath;
            var obj = AssetDatabase.LoadAssetAtPath<Object>(itemPath);
            Selection.activeObject = obj; 
            EditorGUIUtility.PingObject(obj);
        }

        protected override float GetCustomRowHeight(int row, TreeViewItem item)
        {
            return 1.5f * EditorGUIUtility.singleLineHeight;
        }


        public AssetPackageEditorTreeViewItem GetItemFromID(int id)
        {
            var item = allItems.Find((item => item.mId == id));
            return item;
        }


        protected override void DrawRow(Rect cellRect, TreeViewItem<AssetPackageEditorTreeViewItem> item, int column,
            ref RowGUIArgs args)
        {
            switch (column)
            {
                case 0:
                    Rect labelRect = cellRect;
                    var LabelContent = EditorGUIUtility.IconContent("Prefab Icon");
                    GUI.Label(labelRect,LabelContent);
                    break;
                case 1:
                    Rect NameRect = cellRect;
                    NameRect.x +=GetContentIndent(item);
                    NameRect.x -= 10;
                    if (item.hasChildren)
                    {
                        bool flag = EditorGUI.Foldout(NameRect, IsExpanded(item.id), GUIContent.none);
                        SetExpanded(item.id, flag);
                    }
                    NameRect.x += EditorGUIUtility.GetIconSize().x;
                    var iconContent = EditorGUIUtility.IconContent("Folder Icon");
                    GUI.Label(NameRect, iconContent);
                    NameRect.x += EditorGUIUtility.GetIconSize().x;
                    string name = item.data.DisPlayName;
                    GUI.Label(NameRect, name);
                    break;
                case 2:
                    Rect packageIDRect = cellRect;

                    GUIStyle error = new GUIStyle(GUI.skin.label);
                    if (item.data.packageID == PackageIDDefines.Instance.packageError)
                    {
                        error.normal.textColor = Color.red;
                    }

                    Rect prerixRect = EditorGUI.PrefixLabel(packageIDRect, new GUIContent("分包ID"),error);
                    
                    prerixRect.x -= 90;
                    prerixRect.width += 80;
                    
                    List<string> packageOptions =new List<string>(PackageIDDefines.Instance.GetPackageList());
                    
                    if (item.data.buildBundleType == AssetPackageBuildBundleType.ByFollowParent)
                    {
                        GUI.enabled = false;
                    }
                    //
                    int prePackageIDIndex = 0;
                    int currentPackageIDIndex = 0;
                    if (item.data.packageID == PackageIDDefines.Instance.packageError)
                    {
                        packageOptions.Insert(0, PackageIDDefines.Instance.packageError);
                        
                        prePackageIDIndex = PackageIDDefines.Instance.GetPackageIDIndexFromPackageID(item.data.packageID);
                        prePackageIDIndex += 1;
                        currentPackageIDIndex = EditorGUI.Popup(prerixRect,prePackageIDIndex, packageOptions.ToArray());
                        item.data.packageID = PackageIDDefines.Instance.GetPackageIDByIndex(currentPackageIDIndex-1);
                    }
                    else
                    {
                        prePackageIDIndex = PackageIDDefines.Instance.GetPackageIDIndexFromPackageID(item.data.packageID); 
                        currentPackageIDIndex = EditorGUI.Popup(prerixRect,prePackageIDIndex, packageOptions.ToArray());
                        item.data.packageID = PackageIDDefines.Instance.GetPackageIDByIndex(currentPackageIDIndex);
                    }
                    GUI.enabled = true;
                    //
                    if (prePackageIDIndex != currentPackageIDIndex &&
                        item.data.buildBundleType == AssetPackageBuildBundleType.ByAllInOne
                        )
                    {
                        UpdateSubItemSettingFromParent(item.data, true);
                    }
                    break;
                case 3:
                    string[] popupListLabel =
                    {
                        "按当前文件夹", 
                        "按包含子文件夹", 
                        "按各文件",
                    };
                    Rect buildSettingRect = cellRect;
                    EditorGUI.LabelField(buildSettingRect, "打包设置");
                    buildSettingRect.x += 60;
                    buildSettingRect.width -= 70;

                    AssetPackageBuildBundleType preType = item.data.buildBundleType;
                    if (item.data.buildBundleType == AssetPackageBuildBundleType.ByFollowParent)
                    {
                        GUI.enabled = false;
                        // default: just show
                        EditorGUI.Popup(buildSettingRect,0, popupListLabel);
                        GUI.enabled = true;
                    }
                    else
                    {
                        item.data.buildBundleType =(AssetPackageBuildBundleType) EditorGUI.Popup(buildSettingRect,(int)item.data.buildBundleType, popupListLabel);
                    }
                    AssetPackageBuildBundleType currentType = item.data.buildBundleType;
                    //
                    if (preType != AssetPackageBuildBundleType.ByAllInOne &&
                        currentType == AssetPackageBuildBundleType.ByAllInOne)
                    {
                        UpdateSubItemSettingFromParent(item.data, true);
                    }else if (preType == AssetPackageBuildBundleType.ByAllInOne &&
                              currentType != AssetPackageBuildBundleType.ByAllInOne)
                    {
                        UpdateSubItemSettingFromParent(item.data, false);
                    }
                    break;
            }
        }

        public bool CheckIsSomeItemHavePackageNotFound()
        {
            if (m_itemData.Count > 0)
            {
                return NotFoundCheckReverso(m_itemData[0].data);
            }
            else
            {
                return false;
            }
        }

        public bool NotFoundCheckReverso(AssetPackageEditorTreeViewItem item)
        {
            if (item.packageID == PackageIDDefines.Instance.packageError)
            {
                return true;
            }

            foreach (var child in item.Children)
            {
                var childItem = child as AssetPackageEditorTreeViewItem;
                if (childItem != null)
                {
                    if (NotFoundCheckReverso(childItem))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        void UpdateSubItemSettingFromParent(AssetPackageEditorTreeViewItem item,bool isSetIncludeSubDir)
        {
            foreach (var child in item.Children)
            {
                var childItem = child as AssetPackageEditorTreeViewItem;
                if (childItem != null)
                {
                    if (isSetIncludeSubDir)
                    {
                        childItem.buildBundleType = AssetPackageBuildBundleType.ByFollowParent;
                        childItem.packageID = item.packageID;
                    }
                    else
                    {
                        childItem.buildBundleType = AssetPackageBuildBundleType.ByDirectionary;
                    }

                    UpdateSubItemSettingFromParent(childItem, isSetIncludeSubDir);
                }
            }
        }
    }
}
