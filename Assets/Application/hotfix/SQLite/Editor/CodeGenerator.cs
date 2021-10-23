using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

namespace SQLite.Editor
{
    public class CodeGenerator : EditorWindow
    {
        static private string m_ConfigPath = "Assets/Application/hotfix/SQLite/Editor/";                // 配置表路径
        static private string m_ScriptOutput = "Assets/Application/hotfix/Config/DesignConfig.cs";      // 导出的配置表结构脚本目录
        static private string m_DatabaseOutput = "Assets/Application/hotfix/SQLite/Editor/config.db";   // 导出的数据库
        static private string m_Namespace = "Application.Runtime";
        static private string m_Info;
        static private string m_ScriptContent;

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
            // clear
            if(File.Exists(m_ScriptOutput))
            {
                File.Delete(m_ScriptOutput);
            }
            Directory.CreateDirectory(m_ScriptOutput.Substring(0, m_ScriptOutput.LastIndexOf("/")));

            m_ScriptContent = string.Empty;
            m_ScriptContent = "using System.Collections;\nusing System.Collections.Generic;\n\n";
            m_ScriptContent += "namespace " + m_Namespace + "\n{\n";

            // find all csv
            string[] files = Directory.GetFiles(m_ConfigPath, "*.csv", SearchOption.AllDirectories);
            foreach(var file in files)
            {
                ProcessFile(file);
            }

            m_ScriptContent += "}";

            // serialized script content
            using(FileStream fs = File.Create(m_ScriptOutput))
            {
                byte[] info = new UTF8Encoding(false).GetBytes(m_ScriptContent);
                fs.Write(info, 0, info.Length);
            }
            AssetDatabase.Refresh();
        }

        // csv format:
        // line1: column
        // line2: comment
        // line3: client or server or all
        // line4: value type (int, float, bool, string, List<int>, List<float>, List<string>)
        static private void ProcessFile(string file)
        {
            string[] lines = null;
            try
            {
                lines = File.ReadAllLines(file);
                foreach (var l in lines)
                {
                    Debug.Log(l);
                }
            }
            catch(System.Exception e)
            {
                Debug.LogError(e.Message);
            }

            string tableName = Path.GetFileNameWithoutExtension(file);
            m_ScriptContent += "public class " + tableName + "\n{\n";
            string[] columns = lines[0].Split(',');
            string[] flags = lines[2].Split(',');
            string[] valueTypes = lines[3].Split(',');
            for(int i = 0; i < columns.Length; ++i)
            {
                m_ScriptContent += "\tpublic " + valueTypes[i] + " " + columns[i] + ";\n";
            }
            m_ScriptContent += "}\n";
        }
    }    
}