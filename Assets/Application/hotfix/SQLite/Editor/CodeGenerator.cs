using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System;
using System.Linq;

namespace SQLite.Editor
{
    public class CodeGenerator : EditorWindow
    {
        static private string   m_ConfigPath        = "Assets/Application/hotfix/SQLite/Editor/";               // 配置表路径
        static private string   m_ScriptOutput      = "Assets/Application/hotfix/Config/DesignConfig.cs";       // 导出的配置表结构脚本目录
        static private string   m_DatabaseOutput    = "Assets/Application/hotfix/SQLite/Editor/config.db";      // 导出的数据库
        static private string   m_Namespace         = "Application.Runtime";
        static private string   m_Info;             // debug info
        static private string   m_ScriptContent;    // DesignConfig's content
        static private string[] m_AllLines;         // 
        static private string[] m_ColumnLine;       // the first line
        static private string[] m_FlagLine;         // the third line
        static private string[] m_ValueTypeLine;    // the forth line

        [MenuItem("Tools/Config Generator")]
        private static void ShowWindow()
        {
            var window = GetWindow<CodeGenerator>();
            window.titleContent = new GUIContent("CodeGenerator");
            window.Show();
        }
    
        private void OnGUI()
        {
            EditorGUILayout.LabelField("配置文件目录", m_ConfigPath);
            EditorGUILayout.LabelField("数据库", m_DatabaseOutput);
            EditorGUILayout.LabelField("Namespace", m_Namespace);

            if(GUILayout.Button("Clear"))
            {
                Clear();
            }

            if(GUILayout.Button("Generate..."))
            {
                Run();
            }
        }

        static private void Clear()
        {
            // clear code & db
            if(File.Exists(m_ScriptOutput))
            {
                File.Delete(m_ScriptOutput);
            }
            Directory.CreateDirectory(m_ScriptOutput.Substring(0, m_ScriptOutput.LastIndexOf("/")));

            if(File.Exists(m_DatabaseOutput))
            {
                File.Delete(m_DatabaseOutput);
            }
            AssetDatabase.Refresh();
        }

        static private void Run()
        {
            Clear();            

            m_ScriptContent = "using System.Collections;\nusing System.Collections.Generic;\n\n";
            m_ScriptContent += "namespace " + m_Namespace + "\n{\n";
            // find all csv
            string[] files = Directory.GetFiles(m_ConfigPath, "*.csv", SearchOption.AllDirectories);
            foreach(var file in files)
            {
                Debug.Log($"Begin process file: {Path.GetFileName(file)}");
                if(!Prepare(file))
                {
                    Debug.LogError($"配置导出失败：格式出错   {file}");
                    break;
                }
                ParseObjectFromCsv(file);
            }
            Debug.Log($"配置脚本导出完成");

            m_ScriptContent += "}";

            // serialized script content
            using(FileStream fs = File.Create(m_ScriptOutput))
            {
                byte[] data = new UTF8Encoding(false).GetBytes(m_ScriptContent);
                fs.Write(data, 0, data.Length);
            }
            AssetDatabase.Refresh();
        }

        static private bool Prepare(string file)
        {
            m_AllLines = null;
            try
            {
                m_AllLines = File.ReadAllLines(file);
            }
            catch(Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }

            if(m_AllLines.Length < 4)
            {
                Debug.LogError($"配置起始关键行不足：{m_AllLines.Length} < 4   ");
                return false;
            }

            m_ColumnLine = m_AllLines[0].Split(',');
            m_FlagLine = m_AllLines[2].Split(',');
            m_ValueTypeLine = m_AllLines[3].Split(',');
            if(m_ColumnLine.Length != m_FlagLine.Length || m_ColumnLine.Length != m_ValueTypeLine.Length)
            {
                Debug.LogError($"关键行长度不一致： column length[{m_ColumnLine.Length}]     flag length[{m_FlagLine.Length}]   valueType length[{m_ValueTypeLine.Length}]");
                return false;
            }
            return true;
        }

        // csv format:
        // line1: column
        // line2: comment
        // line3: client or server or all
        // line4: value type (int, float, bool, string, List<int>, List<float>, List<string>)
        static private void ParseObjectFromCsv(string file)
        {
            string tableName = Path.GetFileNameWithoutExtension(file);
            m_ScriptContent += "    public class " + tableName + "\n    {\n";
            for(int i = 0; i < m_ColumnLine.Length; ++i)
            {
                if(m_FlagLine[i].ToLower().Contains("all") || m_FlagLine[i].ToLower().Contains("client"))
                {
                    m_ScriptContent += "        public " + m_ValueTypeLine[i] + " " + m_ColumnLine[i] + ";\n";
                }
            }
            m_ScriptContent += "    }\n\n";
        }
    }    
}