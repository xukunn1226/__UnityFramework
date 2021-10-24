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
        static private string   m_ScriptFilePath    = "Assets/Application/hotfix/Config/DesignConfig.cs";       // 导出的配置表结构脚本目录
        static private string   m_DatabaseFilePath  = "Assets/Application/hotfix/SQLite/Editor/config.db";      // 导出的数据库
        static private string   m_Namespace         = "Application.Runtime";
        static private string   m_ScriptContent;    // DesignConfig's content
        static private string[] m_AllLines;         // 
        static private string[] m_ColumnLine;       // the first line
        static private string[] m_FlagLine;         // the third line
        static private string[] m_ValueTypeLine;    // the forth line
        static private SqlData  m_Sql;

        [MenuItem("Tools/Config Generator")]
        private static void ShowWindow()
        {
            var window = GetWindow<CodeGenerator>();
            window.titleContent = new GUIContent("CodeGenerator");
            window.Show();
        }

        private void OnDisable()
        {
            m_Sql?.Close();
        }
    
        private void OnGUI()
        {
            EditorGUILayout.LabelField("配置文件目录", m_ConfigPath);
            EditorGUILayout.LabelField("数据库", m_DatabaseFilePath);
            EditorGUILayout.LabelField("Namespace", m_Namespace);

            if(GUILayout.Button("Clear"))
            {
                Clear();                
                AssetDatabase.Refresh();
            }

            if(GUILayout.Button("Generate..."))
            {
                Run();
            }
        }

        static private void Clear()
        {
            // clear code & db
            if(File.Exists(m_ScriptFilePath))
            {
                File.Delete(m_ScriptFilePath);
            }
            Directory.CreateDirectory(m_ScriptFilePath.Substring(0, m_ScriptFilePath.LastIndexOf("/")));

            if(File.Exists(m_DatabaseFilePath))
            {
                File.Delete(m_DatabaseFilePath);
            }
        }

        static private void Run()
        {
            Clear();

            DoCodeGenerated();

            DoDBGenerated();
            
            AssetDatabase.Refresh();
        }

        static private void DoCodeGenerated()
        {
            m_ScriptContent = "using System.Collections;\nusing System.Collections.Generic;\n\n";
            m_ScriptContent += "namespace " + m_Namespace + "\n{\n";
            // find all csv
            string[] files = Directory.GetFiles(m_ConfigPath, "*.csv", SearchOption.AllDirectories);
            foreach(var file in files)
            {
                Debug.Log($"创建表结构对象: {Path.GetFileName(file)}");
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
            using(FileStream fs = File.Create(m_ScriptFilePath))
            {
                byte[] data = new UTF8Encoding(false).GetBytes(m_ScriptContent);
                fs.Write(data, 0, data.Length);
            }
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
            if(m_ColumnLine.Length != m_ValueTypeLine.Length || m_ColumnLine.Length != m_FlagLine.Length)
            {
                Debug.LogError($"关键行长度不一致： column length[{m_ColumnLine.Length}]   flag length[{m_FlagLine.Length}]  valueType length[{m_ValueTypeLine.Length}]");
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
                m_ScriptContent += "        public " + m_ValueTypeLine[i] + " " + m_ColumnLine[i] + ";\n";                
            }
            m_ScriptContent += "    }\n\n";
        }

        static private bool NeedImport(string[] flags, int index)
        {
            if(index < 0 || index >= flags.Length)
                throw new System.ArgumentOutOfRangeException("index");
            return flags[index].ToLower().Contains("all") || flags[index].ToLower().Contains("client");
        }

        static private void DoDBGenerated()
        {
            m_Sql = new SqlData(m_DatabaseFilePath);

            // find all csv
            string[] files = Directory.GetFiles(m_ConfigPath, "*.csv", SearchOption.AllDirectories);
            foreach(var file in files)
            {
                Debug.Log($"创建表数据: {Path.GetFileName(file)}");
                if(!Prepare(file))
                {
                    Debug.LogError($"配置导出失败：格式出错   {file}");
                    break;
                }
                CreateTableFromCsv(file);
            }
            Debug.Log($"数据库导出完成");

            m_Sql.Close();
        }

        static private void CreateTableFromCsv(string file)
        {
            string tableName = Path.GetFileNameWithoutExtension(file);
            m_Sql.CreateTable(tableName, m_ColumnLine, ConvertCustomizedValueTypesToSql(m_ValueTypeLine));

            for(int i = 4; i < m_AllLines.Length; ++i)
            {
                string[] values = m_AllLines[i].Split(',');
                foreach(var v in values)
                    Debug.Log($"------ {v}");
                m_Sql.InsertValues(tableName, values);
            }
        }

        // 自定义的类型标签转化为SQLite的类型标签
        static private string ConvertCustomizedValueTypeToSql(string valueType)
        {
            switch(valueType.ToLower())
            {
                case "int":
                    return "INTEGER";
                case "string":
                    return "TEXT";
                case "float":
                    return "REAL";
                case "bool":
                    return "BOOLEAN";
            }
            return "TEXT";
        }

        static private string[] ConvertCustomizedValueTypesToSql(string[] valueTypes)
        {
            string[] ret = new string[valueTypes.Length];
            for(int i = 0; i < valueTypes.Length; ++i)
            {
                ret[i] = ConvertCustomizedValueTypeToSql(valueTypes[i]);
            }
            return ret;
        }
    }    
}