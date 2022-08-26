using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Framework.AssetManagement.AssetTreeView;
using System.Linq;
using Framework.AssetManagement.AssetBuilder;
using NUnit.Framework.Internal;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Framework.AssetManagement.AssetPackageEditor.Editor
{
    public class ResizeHandler
    {
        private bool isResize = false;
        private Action OnHandleTodo;
        private MouseCursor cursor;
        public float newPos;
        private Event currentEvent;
        

        public ResizeHandler(MouseCursor cursor,float pos,Action onHandleTodo = null)
        {
            newPos = pos;
            this.cursor = cursor;
            this.OnHandleTodo = onHandleTodo;
        }
        public void Draw(Rect rect,Event currentEvent)
        {
            this.currentEvent = currentEvent; 
            EditorGUIUtility.AddCursorRect(rect, cursor);
            if (currentEvent.type == EventType.MouseDown && rect.Contains(currentEvent.mousePosition))
            {
                isResize = true;
            } 
        }

        public void Update()
        {
            if (isResize && currentEvent != null)
            {
                if (currentEvent.type == EventType.MouseUp)
                {
                    isResize = false;
                }
                if (cursor == MouseCursor.ResizeHorizontal)
                {
                    newPos = currentEvent.mousePosition.x;
                }
                else if(cursor == MouseCursor.ResizeVertical)
                {
                    newPos = currentEvent.mousePosition.y;
                }
                OnHandleTodo?.Invoke();
            }
        }
        
    }
    public class AssetPackageEditorWindow : EditorWindow
    {
        // Start is called before the first frame update
        private static string settingSaveDir = "Assets/Framework/AssetManagement/AssetPackageEditor/Editor/Setting/";
        AssetTreeViewView<AssetPackageEditorTreeView, AssetPackageEditorTreeViewItem> m_AssetPackageEditorTreeView;
        AssetTreeViewView<AssetPackageFileShowTreeView, AssetPackageFileShowTreeViewItem> m_AssetPackageShowFileTreeView;

        public AssetPackageEditorSetting assetPackageEditorSetting;
        private PackageIDDefinesReorderableList packageIDDefinesReorderableList;
        
        private string settingName = "";
        [MenuItem("Tools/Assets Management/AssetPackageEditor", false, 1)]
        public static void ShowWindow()
        {
            AssetPackageEditorWindow wnd = GetWindow<AssetPackageEditorWindow>();
            wnd.titleContent = new GUIContent("分包编辑工具");
            wnd.Show();
        }

        Rect multiColumnTreeViewRect
        {
            get { return new Rect(20, 30, position.width - 40, position.height - 60); }
        }

        public void OnEnable()
        {
            m_AssetPackageEditorTreeView =
                new AssetTreeViewView<AssetPackageEditorTreeView, AssetPackageEditorTreeViewItem>(
                    multiColumnTreeViewRect);
            m_AssetPackageShowFileTreeView =
                new AssetTreeViewView<AssetPackageFileShowTreeView, AssetPackageFileShowTreeViewItem>(
                    multiColumnTreeViewRect
                );
            packageIDDefinesReorderableList = new PackageIDDefinesReorderableList();
            packageIDDefinesReorderableList.onItemAdded = OnPackageIDsAdded;
            packageIDDefinesReorderableList.onItemReplace = OnPackageIDsReplace;
            packageIDDefinesReorderableList.onItemDeleted = OnPackageIDsRemoved;
            InitHandler();

            string defaultGuid = EditorPrefs.GetString("DefaultAssetPackageEditorSettingGUID", "NAN");
            if (defaultGuid == "NAN")
            {
                assetPackageEditorSetting = null;
            }
            else
            {
                assetPackageEditorSetting = AssetDatabase.LoadAssetAtPath<AssetPackageEditorSetting>(AssetDatabase.GUIDToAssetPath(defaultGuid));
            }
            if (assetPackageEditorSetting != null)
            {
                LoadSettingConfig();
            }
        }

        private void OnDisable()
        {
            if (assetPackageEditorSetting != null)
            {
                string defaultGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(assetPackageEditorSetting));
                EditorPrefs.SetString("DefaultAssetPackageEditorSettingGUID", defaultGuid);
            }
            
        }

        public void OnPackageIDsReplace(string oldStr,string newStr)
        {
            m_AssetPackageEditorTreeView.AssetTreeView.UpatePackageID(oldStr,newStr);
        }
        public void OnPackageIDsAdded(string newStr)
        {
            Debug.Log($"add packageID: {newStr}");
            
        }
        public void OnPackageIDsRemoved(string oldStr)
        {
            m_AssetPackageEditorTreeView.AssetTreeView.UpatePackageID(oldStr,PackageIDDefines.Instance.packageError);
        }
        
        private float X1 = 250f;
        private float X2 = 600f;
        private float y1 = 8 * EditorGUIUtility.singleLineHeight;
        private MouseCursor currentCursor;
        private float posToHandle;
        private Dictionary<string,ResizeHandler> resizeHandlers = new Dictionary<string, ResizeHandler>();

        void updateHandle()
        {
            foreach (var resizeHandler in resizeHandlers)
            {
                resizeHandler.Value.Update();
            }

            X1 = resizeHandlers["x1"].newPos;

            y1 = resizeHandlers["y1"].newPos;

            X2 = resizeHandlers["x2"].newPos;
            Repaint();
        }

        void AddCursor(string des,MouseCursor cursor, float pos)
        {
            ResizeHandler handler = new ResizeHandler(cursor, pos);
            resizeHandlers.Add(des,handler);
        }

        void InitHandler()
        {
            AddCursor("y1", MouseCursor.ResizeVertical,y1);
            AddCursor("x1", MouseCursor.ResizeHorizontal,X1);
            AddCursor("x2", MouseCursor.ResizeHorizontal,X2);
        }

        void UpdateFileShowTreeView()
        {
            string path = GetPathFromEditorTreeViewSelection();
            var data= AssetPackageEditorUtility.GetFileShowDataFromPath(path);
            m_AssetPackageShowFileTreeView.UpdateData(data);
        }

        private void Update()
        {
            updateHandle();
            UpdateFileShowTreeView();
        }

        private void OnGUI()
        {
            //
            float height = 0.1f;
            float xpos = X1;
            float xpos2 = X2;
            float xBorder = 5f;
            float yBorder = 5f;
            //
            DrawSelectSettingArea(new Rect(3f, height, xpos, y1 - yBorder));
            resizeHandlers["y1"].Draw(new Rect(3f , y1, xpos,yBorder*2),Event.current);
            DrawPackageIDDefineArea(new Rect(3f, y1 , xpos, position.height - y1 - yBorder*2));
            // 
            resizeHandlers["x1"].Draw(new Rect( xpos , height, xBorder*2, position.height),Event.current);
            //
            DrawMainGUI(new Rect(xpos, height, xpos2 - xpos, position.height- yBorder*2));
            // 
            resizeHandlers["x2"].Draw(new Rect(xpos2, height, xBorder * 2, position.height),Event.current);
            //
            DrawShowFileArea(new Rect(xpos2, height, position.width - xpos2, position.height- yBorder*2));

        }

        

        void DrawShowFileArea(Rect rect)
        {
            float xPos = rect.x;
            float height = rect.y;
            float xborder = 8f;
            EditorGUI.LabelField(new Rect(xPos, height, rect.width - xborder, EditorGUIUtility.singleLineHeight),
                "分包路径文件展示");
            height += EditorGUIUtility.singleLineHeight;
            float treeViewHeight = rect.height - EditorGUIUtility.singleLineHeight;
            m_AssetPackageShowFileTreeView.DrawGUI(new Rect(xPos, height, rect.width - xborder, treeViewHeight));
        }

        string info = "";

        void DrawPackageIDDefineArea(Rect rect)
        {
            float height = rect.y;
            float xborder = 8f;
            float seperate = 60f;
            float xPos = rect.x;

            // tile
            EditorGUI.LabelField(new Rect(xPos, height, rect.width, EditorGUIUtility.singleLineHeight), 
                "PackageID设置");
            height += EditorGUIUtility.singleLineHeight;
            DrawUIBox(Color.black, GetDefaultBackgroundColor(),new Rect(xPos, height, rect.width, rect.height- EditorGUIUtility.singleLineHeight),0.9f);
            if (!packageIDDefinesReorderableList.CanSavePackageData())
            {
                GUI.enabled = false;
            }
            height += 5f;
            if (GUI.Button(new Rect(xPos + rect.width - 110, height, 100, EditorGUIUtility.singleLineHeight * 1.2f), "保存并更新"))
            {
                info = packageIDDefinesReorderableList.SavePacakgeIDData();   
            }
            GUI.enabled = true;
            
            height += 5f;
            height += EditorGUIUtility.singleLineHeight;
            packageIDDefinesReorderableList.DoList(new Rect(xPos, height, rect.width, rect.height - height - 70));
            
            height += 250;
            GUI.TextArea(new Rect(xPos + 1f, height, rect.width - 2f, rect.height -(height - rect.y)),
                info);
        }

        public static void DrawUIBox(Color borderColor, Color backgroundColor, Rect rect, float width = 2f)
        {
            Rect outter = new Rect(rect);
            Rect inner = new Rect(rect.x + width, rect.y + width, rect.width - width * 2, rect.height - width * 2);
            EditorGUI.DrawRect(outter, borderColor);
            EditorGUI.DrawRect(inner, backgroundColor);
        }
        private static Color GetDefaultBackgroundColor()
        {
            float kViewBackgroundIntensity =EditorGUIUtility.isProSkin ? 0.22f : 0.76f;
            return new Color(kViewBackgroundIntensity, kViewBackgroundIntensity, kViewBackgroundIntensity, 1f);
        }

        void DrawSelectSettingArea(Rect rect)
        {
            float height = rect.y;
            float xborder = 8f;
            float seperate = 60f;
            float xPos = rect.x;

            // tile
            EditorGUI.LabelField(new Rect(xPos, height, seperate, EditorGUIUtility.singleLineHeight),
                "配置设置");

            var refreshIcon = EditorGUIUtility.IconContent("Refresh");
            if (assetPackageEditorSetting == null)
            {
                GUI.enabled = false;
            }

            if (GUI.Button(new Rect(rect.x + seperate + 10f, height, 25f, EditorGUIUtility.singleLineHeight),
                    refreshIcon))
            {
                LoadSettingConfig();
            }

            GUI.enabled = true;

            height += EditorGUIUtility.singleLineHeight;
            DrawUIBox(Color.black, GetDefaultBackgroundColor(),
                new Rect(xPos, height, rect.width, rect.height - height), 0.9f);

            height += 5f;
            xPos += 4f;
            //
            // 
            EditorGUI.LabelField(new Rect(xPos, height, seperate, EditorGUIUtility.singleLineHeight),
                "配置选择");
            assetPackageEditorSetting = EditorGUI.ObjectField(
                new Rect(xPos + seperate, height, rect.width - seperate - xborder, EditorGUIUtility.singleLineHeight),
                assetPackageEditorSetting, typeof(AssetPackageEditorSetting)) as AssetPackageEditorSetting;
            height += EditorGUIUtility.singleLineHeight;
            //
            // changeName
            height += 5f;
            EditorGUI.LabelField(new Rect(xPos, height, seperate, EditorGUIUtility.singleLineHeight),
                "配置名字");
            if (assetPackageEditorSetting == null)
                settingName = "";
            settingName = EditorGUI.TextField(
                new Rect(xPos + seperate, height, rect.width - seperate - xborder, EditorGUIUtility.singleLineHeight),
                settingName);
            height += EditorGUIUtility.singleLineHeight;
            //
            // new Setting
            float seperate2 = rect.width / 2;
            if (GUI.Button(new Rect(xPos, height, rect.width - seperate2 - xborder, EditorGUIUtility.singleLineHeight),
                    "新建配置"))
            {
                CreateNewSettingFile();
            }

            // save Setting
            if (assetPackageEditorSetting == null)
            {
                GUI.enabled = false;
            }

            if (GUI.Button(
                    new Rect(xPos + seperate2, height, rect.width - seperate2 - xborder,
                        EditorGUIUtility.singleLineHeight),
                    "保存配置"))
            {
                SaveEditorSetting();
            }

            GUI.enabled = true;
            if (assetPackageEditorSetting == null)
            {
                GUI.enabled = false;
            }

            height += EditorGUIUtility.singleLineHeight;
            if (GUI.Button(new Rect(xPos, height, rect.width - xborder, EditorGUIUtility.singleLineHeight), "读取配置"))
            {
                LoadSettingConfig();
            }
            GUI.enabled = true;
            //
            bool haveNotFound = m_AssetPackageEditorTreeView.AssetTreeView.CheckIsSomeItemHavePackageNotFound();
            string Errorstr = "";
            if (haveNotFound)
            { 
                Errorstr = "  有分包的PakageID丢失";
            }
            GUIStyle error = new GUIStyle(GUI.skin.label);
            error.normal.textColor = Color.red;
            height += EditorGUIUtility.singleLineHeight;
            height += 5f;
            GUI.Label(new Rect(xPos, height, rect.width - xborder, EditorGUIUtility.singleLineHeight), Errorstr,error);
            //
            
        }

        void UpdateEditorSettingDataFromTreeView()
        {
            Dictionary<string, AssetPackageSettingItem> settingItemDict = new Dictionary<string, AssetPackageSettingItem>();
            for (int i = 0; i < assetPackageEditorSetting.editorAssetPackageSettingItems.Count; i++)
            {
                var item = assetPackageEditorSetting.editorAssetPackageSettingItems[i];
                settingItemDict.Add(item.path, item);
            }
            m_AssetPackageEditorTreeView.AssetTreeView.HandleEachItem((treeviewItem) =>
            {
                if(settingItemDict.TryGetValue(treeviewItem.Path,out var settingitem))
                {
                    settingitem.buildBundleType = treeviewItem.buildBundleType;
                    settingitem.packageID = treeviewItem.packageID;
                }
            });
        }

        void SaveEditorSetting()
        {
            UpdateEditorSettingDataFromTreeView();
            if (assetPackageEditorSetting != null)
            {
                string savePath = settingSaveDir + settingName+".asset";
                string realPath = UnityEngine.Application.dataPath + savePath.Replace("Assets", "");
                if (File.Exists(realPath))
                {
                    EditorUtility.SetDirty(assetPackageEditorSetting);
                    AssetDatabase.SaveAssets();
                }
                else
                {
                    string oldPath = AssetDatabase.GetAssetPath(assetPackageEditorSetting);
                    if (!string.IsNullOrEmpty(oldPath))
                    {
                        AssetDatabase.RenameAsset(oldPath, savePath);
                    }
                    else
                    {
                        AssetDatabase.CreateAsset(assetPackageEditorSetting, savePath);
                    }
                    AssetDatabase.SaveAssets();
                }
            }
        }

        void DrawMainGUI(Rect rect)
        {
            float xPos = rect.x;
            float height = rect.y;
            float xborder = 8f;
            EditorGUI.LabelField(new Rect(xPos + xborder, height, rect.width-2* xborder, EditorGUIUtility.singleLineHeight), "分包编辑");
            height += EditorGUIUtility.singleLineHeight;
            float treeViewHeight = rect.height - EditorGUIUtility.singleLineHeight;
            m_AssetPackageEditorTreeView.DrawGUI(new Rect(xPos+ xborder, height, rect.width-2* xborder, treeViewHeight));
            height += treeViewHeight;
        }

        void CreateNewSettingFile()
        {
            assetPackageEditorSetting = ScriptableObject.CreateInstance<AssetPackageEditorSetting>();
            var allItems = LoadPath();
            
            AssetPackageSettingItem[] itemArray = new AssetPackageSettingItem[allItems.Count];
            for(int z = 0;z<itemArray.Length;z++) itemArray[z] = new AssetPackageSettingItem();
            assetPackageEditorSetting.editorAssetPackageSettingItems =
                new List<AssetPackageSettingItem>(itemArray);
            
            
            for (int i = 0; i < allItems.Count; i++)
            {
                //assetPackageEditorSetting.editorAssetPackageSettingItems[i].treeViewItem = allItems[i];
                assetPackageEditorSetting.editorAssetPackageSettingItems[i].path = allItems[i].Path;
                allItems[i].buildBundleType =
                    assetPackageEditorSetting.editorAssetPackageSettingItems[i].buildBundleType;
                allItems[i].packageID =
                    assetPackageEditorSetting.editorAssetPackageSettingItems[i].packageID;
                
            }
            // initData
            settingName = "新建配置";
        }

        void LoadSettingConfig()
        {
            var allItems = LoadPath();
            Dictionary<string, AssetPackageSettingItem> settingItemDict = new Dictionary<string, AssetPackageSettingItem>();
            List<string> DeletedPaths = new List<string>(); 
            List<string> AddedPaths = new List<string>(); 
            for (int i = 0; i < assetPackageEditorSetting.editorAssetPackageSettingItems.Count; i++)
            {
                var item = assetPackageEditorSetting.editorAssetPackageSettingItems[i];
                settingItemDict.Add(item.path, item);
                DeletedPaths.Add(item.path);
            }

            // 新建路径 新增的Item
            // 删除路径 减少的Item
            // 改名的路径, 删除->重新新建

            for (int i = 0; i < allItems.Count; i++)
            {
                var item = allItems[i];
                string pathKey = item.Path;
                bool hasItem = settingItemDict.TryGetValue(pathKey,out var settingitem);
                if (hasItem)
                {
                    // 更新 treeview
                    allItems[i].buildBundleType = settingitem.buildBundleType;
                    allItems[i].packageID = settingitem.packageID;
                    DeletedPaths.Remove(pathKey);
                }
                // case 1, 路径改名
                // case 2, 新建路径
                else
                {
                    AddedPaths.Add(pathKey);
                    // default
                    // 更新 treeview
                    allItems[i].buildBundleType = AssetPackageBuildBundleType.ByDirectionary;
                    allItems[i].packageID = "base";
                }
            }
            
            //
            if (AddedPaths.Count > 0)
            {
                foreach (var addedPath in AddedPaths)
                {
                    var newSettingItem = new AssetPackageSettingItem();
                    newSettingItem.path = addedPath;
                    newSettingItem.buildBundleType = AssetPackageBuildBundleType.ByDirectionary;
                    newSettingItem.packageID = "base";
                    assetPackageEditorSetting.editorAssetPackageSettingItems.Add(newSettingItem);
                }
            }
            //
            if (DeletedPaths.Count > 0)
            {
                foreach (var deletedPath in DeletedPaths)
                {
                    assetPackageEditorSetting.editorAssetPackageSettingItems.RemoveAll(
                        item => item.path == deletedPath
                        );
                }
            }
            //
            
            settingName =Path.GetFileNameWithoutExtension( AssetDatabase.GetAssetPath(assetPackageEditorSetting));
        }

        string GetPathFromEditorTreeViewSelection()
        {
            var selectedID = m_AssetPackageEditorTreeView.AssetTreeView.GetSelection();
            if (selectedID.Count > 0)
            {
                var data = m_AssetPackageEditorTreeView.AssetTreeView.GetItemFromID(selectedID[0]);
                return data.Path;
            }
            else
            {
                return "";
            }
        }

        List<AssetPackageEditorTreeViewItem> LoadPath()
        {
            var treeviewData = AssetPackageEditorUtility.GetTreeViewItemFromBundleSettingsAndRawData(out List<AssetPackageEditorTreeViewItem> allItems);
            m_AssetPackageEditorTreeView.UpdateData(treeviewData);
            m_AssetPackageEditorTreeView.AssetTreeView.SetAllItemsMap(allItems);
            return allItems;
        }
    }

    
    public class PathNode
    {
        public string path;
        public int depth;
        public pathType PathType;

        public PathNode(string path, int depth, pathType type)
        {
            this.path = path;
            this.depth = depth;
            this.PathType = type;
        }
    }

    public static class AssetPackageEditorUtility
    {
        public static List<BaseTreeViewItem> GetPathNodesReverso(ref int id,string path, int depth,AssetPackageEditorTreeViewItem cuurentTopNode,ref List<AssetPackageEditorTreeViewItem> AllItems)
        {
            var directories = Directory.GetDirectories(path);
            var files = Directory.GetFiles(path);

            List<BaseTreeViewItem> childrenNodes = new List<BaseTreeViewItem>();
            // do work;
            foreach (var file in files)
            {
                if (Path.GetExtension(file) != ".meta")
                {
                    var node = new AssetPackageEditorTreeViewItem(id, depth, Path.GetFileName(file), file);
                    node.PathType = pathType.file;
                    node.Parent = cuurentTopNode;
                    node.enabled = false;
                    if (depth != 0)
                    {
                        //childrenNodes.Add(node);
                    }

                    //id++;
                    //AllItems.Add(node);
                }

            }
            // condition
            if (directories.Length == 0)
            {
                if(childrenNodes.Count == 0)
                    childrenNodes.Add(null);
                return childrenNodes;
            }
            else
            {
                foreach (var dir in directories)
                {
                    if (!AssetBuilderUtil.IsBlockedByBlackList(dir))
                    {
                        string newPath = dir.Replace('\\', '/').TrimEnd(new char[] { '/' });
                        var node = new AssetPackageEditorTreeViewItem(id, depth, Path.GetFileName(dir), newPath);
                        node.PathType = pathType.dir;
                        id++;
                        node.Children = GetPathNodesReverso(ref id, dir, depth + 1, node, ref AllItems);
                        node.Parent = cuurentTopNode;
                        childrenNodes.Add(node);
                        AllItems.Add(node);
                    }
                }
            }
            return childrenNodes;
        }

        public static AssetPackageEditorTreeViewItem GetSinglePathTreeViewItem(string path,ref int id,
            ref List<AssetPackageEditorTreeViewItem> allItems)
        {
            //string abspath = UnityEngine.Application.dataPath + Path.DirectorySeparatorChar + path;
            
            string pathName = Path.GetFileName(path);
            
            AssetPackageEditorTreeViewItem root = new AssetPackageEditorTreeViewItem(id,0,pathName,path);
            id++;
            allItems.Add(root);
            root.PathType = pathType.dir;
            root.buildBundleType = AssetPackageBuildBundleType.ByDirectionary;
            root.Parent = null;
            int depth = 1;
            root.Children = GetPathNodesReverso(ref id, path, depth, root, ref allItems);
            return root;
        }

        public static List<AssetPackageEditorTreeViewItem> GetTreeViewItemFromBundleSettingsAndRawData(
            out List<AssetPackageEditorTreeViewItem> AllItems)
        {
            List<AssetPackageEditorTreeViewItem> items = new List<AssetPackageEditorTreeViewItem>();
            AllItems = new List<AssetPackageEditorTreeViewItem>();
            AssetPackageEditorTreeViewItem root = new AssetPackageEditorTreeViewItem();
            root.Children = new List<BaseTreeViewItem>();
            int id = 0;
            string[] paths = AssetBuilderSetting.GetDefault().WhiteListOfPath;
            foreach (var assetPath in paths)
            {
                // datapath : ~/Assets
                string path = assetPath;
                var pathRoot = GetSinglePathTreeViewItem(path, ref id, ref AllItems);
                pathRoot.Parent = root;
                root.Children.Add(pathRoot);
            }

            root.Parent = null;
            root.isRootNode = true;

            items.Add(root);
            return items;
        }

        public static AssetPackageSettingItem  GetPathNodeSetting(string path)
        {
            // temp todo
            AssetPackageSettingItem temp = new AssetPackageSettingItem(path);
            //

            return temp;
        }

        public static List<AssetPackageFileShowTreeViewItem> GetFileShowDataFromPath(string path)
        {
            List<AssetPackageFileShowTreeViewItem> items = new List<AssetPackageFileShowTreeViewItem>();
            if (string.IsNullOrEmpty(path)) return items;
            
            var directories = Directory.GetDirectories(path);
            var files = Directory.GetFiles(path);

            int id = 0;
            foreach (var directory in directories)
            {
                AssetPackageFileShowTreeViewItem item =
                    new AssetPackageFileShowTreeViewItem(id, 0, Path.GetDirectoryName(directory),directory);
                
                id++;
                items.Add(item);
            }

            foreach (var file in files)
            {
                 AssetPackageFileShowTreeViewItem item =
                     new AssetPackageFileShowTreeViewItem(id, 0, Path.GetFileName(file),file);
                 if (! AssetBuilderSetting.GetDefault().Extension.Contains(Path.GetExtension(file)))
                 {
                     id++;
                     items.Add(item);
                 }
            }

            return items;
        }

        public static void Tempcode(string str)
        {
            Dictionary<char, int> res= new Dictionary<char, int>();
            for (int i = 0; i < str.Length; i++)
            {
                char currentChar = str[i];
                int numberIndexStart = i;
                int numberIndexEnd = i;
                if (currentChar> 0 && currentChar< 26)
                {
                    numberIndexStart = i + 1;
                    for (int a = i+1; a < str.Length; a++)
                    {
                        char NextChar = str[a];
                        if (NextChar > 0 && NextChar < 26)
                        {
                            numberIndexEnd = a - 1;
                        }
                    }

                    int intRes = GetIntFromStr(str, numberIndexStart, numberIndexEnd);
                    res.TryGetValue(currentChar,out i);
                    if (i == 0)
                    {
                        res.TryAdd(currentChar, intRes);
                    }
                    else
                    {
                        i += intRes;
                        res[currentChar] = i;
                    }
                }
            }
        }

        public static int GetIntFromStr(string str,int start, int end)
        {
            return 0;
        }
        
    }
}