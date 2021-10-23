using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace SQLite.Editor
{
    public class CodeGenerator : EditorWindow
    {
        static private string m_ConfigPath = "Assets/Application/hotfix/SQLite/Editor/";            // 配置表路径
        static private string m_ScriptOutput = "Assets/Application/hotfix/Config/";                 // 导出的配置表结构脚本目录
        static private string m_Info;

        [MenuItem("Tools/Config CodeGenerator")]
        private static void ShowWindow()
        {
            var window = GetWindow<CodeGenerator>();
            window.titleContent = new GUIContent("CodeGenerator");
            window.Show();
        }
    
        private void OnGUI()
        {
            m_ConfigPath = EditorGUILayout.TextField("配置文件目录", m_ConfigPath);

            if(GUILayout.Button("Generate..."))
            {
                Run();
            }
        }

        static private void Run()
        {
            // find all csv
            string[] files = Directory.GetFiles(m_ConfigPath, "*.csv", SearchOption.AllDirectories);
            foreach(var file in files)
            {
                ProcessFile(file);
            }
        }

        // supported value type: int, float, bool, string, array<int>, array<float>, array<string>
        static private void ProcessFile(string file)
        {
            try
            {
                string[] lines = File.ReadAllLines(file);
                foreach (var l in lines)
                {
                    Debug.Log(l);
                }
            }
            catch(System.Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
    }    
}