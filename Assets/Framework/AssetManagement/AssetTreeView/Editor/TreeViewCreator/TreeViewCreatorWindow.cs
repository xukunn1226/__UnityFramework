using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Framework.AssetManagement.AssetTreeView
{
    public class TemplateFileParam
    {
        public string treeViewName;
        public string namespaceName;
        public string OutputFilePath;
        public List<string> treeViewColumnName = new List<string>();
        public Dictionary<string, string> treeViewItemDataMap = new Dictionary<string, string>();

    }

    public class TreeViewCreatorWindow : EditorWindow
    {
        // Start is called before the first frame update
        TemplateFileParser Parser = new TemplateFileParser();

        [MenuItem("Assets/TreeViewCreator/ExprotTreeView", false, 114514)]
        static void ShowTreeViewCreator()
        {
            TreeViewCreatorWindow wnd = GetWindow<TreeViewCreatorWindow>();
            wnd.titleContent = new GUIContent("TreeViewCreator");
            wnd.Show();
        }

        public static string TreeViewNameStr;
        public static string TreeViewColumnsStr;
        public static string TreeViewItemDataStr;
        public static string NameSpaceStr;


        private void OnGUI()
        {
            string path = GetSelectedPathOrFallback();
            GUILayout.BeginVertical();

            GUILayout.Label("TreeViewName");
            TreeViewNameStr = GUILayout.TextField(TreeViewNameStr);

            GUILayout.Label("nameSpace");
            NameSpaceStr = GUILayout.TextField(NameSpaceStr);

            GUILayout.Label("TreeViewColumns");
            TreeViewColumnsStr = GUILayout.TextField(TreeViewColumnsStr);

            GUILayout.Label("TreeViewItemDataMap");
            TreeViewItemDataStr = GUILayout.TextField(TreeViewItemDataStr);

            if (GUILayout.Button("生成treeView"))
            {
                TemplateFileParam param = new TemplateFileParam();
                param.treeViewName = TreeViewNameStr;
                param.namespaceName = NameSpaceStr;
                string[] columnNameSplits = TreeViewColumnsStr.Split(" ");
                param.treeViewColumnName = new List<string>(columnNameSplits);
                param.OutputFilePath = path;
                Parser.DoGenCode(param);
            }

            if (GUILayout.Button("Test"))
            {
                Debug.Log(path);
            }
            
            GUILayout.EndVertical();

        }
        public static string GetSelectedPathOrFallback()
        {
            string path = "Assets";
		
            foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if ( !string.IsNullOrEmpty(path) && File.Exists(path) ) 
                {
                    path = Path.GetDirectoryName(path);
                    break;
                }
            }
            return path;
        }
    }
}