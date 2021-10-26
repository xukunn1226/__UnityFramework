using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Framework.AssetManagement.GameBuilder;
using Application.Runtime;
using System.IO;
using System.Text;
using System;

namespace Application.Editor
{
    [InitializeOnLoad]
    static public class ConfigBuilder
    {
        static private string       m_ScriptContent;    // DesignConfig's content
        static private string[]     m_AllLines;         // 
        static private string[]     m_ColumnLine;       // the first line
        static private string[]     m_FlagLine;         // the third line
        static private string[]     m_ValueTypeLine;    // the forth line
        static private SqlData      m_Sql;

        static ConfigBuilder()
        {
            BundleBuilder.OnPostprocessBundleBuild += OnPostprocessBundleBuild;
        }

        static private void OnPostprocessBundleBuild()
        {
            Run();

            // copy db to streamingAssets
            string dstPath = string.Format($"{UnityEngine.Application.streamingAssetsPath}/{Framework.Core.Utility.GetPlatformName()}/{System.IO.Path.GetFileName(ConfigBuilderSetting.DatabaseFilePath)}");
            FileUtil.DeleteFileOrDirectory(dstPath);
            FileUtil.CopyFileOrDirectory(ConfigBuilderSetting.DatabaseFilePath, dstPath);
        }
        
        static private void Clear()
        {
            // clear code & db
            if(File.Exists(ConfigBuilderSetting.DesignConfigScriptFilePath))
            {
                File.Delete(ConfigBuilderSetting.DesignConfigScriptFilePath);
            }
            Directory.CreateDirectory(ConfigBuilderSetting.DesignConfigScriptFilePath.Substring(0, ConfigBuilderSetting.DesignConfigScriptFilePath.LastIndexOf("/")));

            if(File.Exists(ConfigBuilderSetting.ConfigManagerScriptFilePath))
            {
                File.Delete(ConfigBuilderSetting.ConfigManagerScriptFilePath);
            }
            Directory.CreateDirectory(ConfigBuilderSetting.ConfigManagerScriptFilePath.Substring(0, ConfigBuilderSetting.ConfigManagerScriptFilePath.LastIndexOf("/")));

            if(File.Exists(ConfigBuilderSetting.DatabaseFilePath))
            {
                File.Delete(ConfigBuilderSetting.DatabaseFilePath);
            }
        }

        [MenuItem("Tools/Config Build %h")]
        static private void Run()
        {
            Clear();

            GenerateDesignConfigScript();

            DoDBGenerated();

            GenerateConfigManagerScript();
            
            AssetDatabase.Refresh();
        }

        static private void GenerateDesignConfigScript()
        {
            m_ScriptContent = "using System.Collections;\nusing System.Collections.Generic;\n\n";
            m_ScriptContent += "namespace " + ConfigBuilderSetting.Namespace + "\n{\n";
            // find all csv
            string[] files = Directory.GetFiles(ConfigBuilderSetting.ConfigPath, "*.csv", SearchOption.AllDirectories);
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
            using(FileStream fs = File.Create(ConfigBuilderSetting.DesignConfigScriptFilePath))
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


            List<string> columnList = new List<string>();
            List<string> valueTypeList = new List<string>();
            for(int i = 0; i < m_FlagLine.Length; ++i)
            {
                if(NeedImport(m_FlagLine, i))
                {
                    columnList.Add(m_ColumnLine[i]);
                    valueTypeList.Add(m_ValueTypeLine[i]);
                }
            }
            m_ColumnLine = columnList.ToArray();
            m_ValueTypeLine = valueTypeList.ToArray();

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
                // if(NeedImport(m_FlagLine, i))
                    m_ScriptContent += "        public " + m_ValueTypeLine[i] + " " + m_ColumnLine[i];
                    m_ScriptContent += AppendNewIfNeed(m_ValueTypeLine[i]);
                    m_ScriptContent += ";\n";
            }
            m_ScriptContent += "    }\n\n";
        }

        static private string AppendNewIfNeed(string valueType)
        {
            valueType = valueType.ToLower();
            if(valueType == "list<string>")
            {
                return " = new List<string>()";
            }
            else if(valueType == "list<int>")
            {
                return " = new List<int>()";
            }
            else if(valueType == "list<float>")
            {
                return " = new List<float>()";
            }
            return "";
        }

        static private bool NeedImport(string[] flags, int index)
        {
            if(index < 0 || index >= flags.Length)
                throw new System.ArgumentOutOfRangeException("index");
            return flags[index].ToLower().Contains("all") || flags[index].ToLower().Contains("client");
        }

        static private void DoDBGenerated()
        {
            m_Sql = new SqlData(ConfigBuilderSetting.DatabaseFilePath);

            // find all csv
            string[] files = Directory.GetFiles(ConfigBuilderSetting.ConfigPath, "*.csv", SearchOption.AllDirectories);
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
            Debug.Log($"数据库导出完成: {ConfigBuilderSetting.DatabaseFilePath}");

            m_Sql.Close();
        }

        static private void CreateTableFromCsv(string file)
        {
            string tableName = Path.GetFileNameWithoutExtension(file);
            m_Sql.CreateTable(tableName, m_ColumnLine, ConvertCustomizedValueTypesToSql(m_ValueTypeLine));

            for(int i = 4; i < m_AllLines.Length; ++i)
            {
                string[] values = m_AllLines[i].Split(',');

                // 过滤服务器字段
                List<string> valList = new List<string>();
                for(int j = 0; j < values.Length; ++j)
                {
                    if(NeedImport(m_FlagLine, j))
                        valList.Add(values[j]);
                }
                m_Sql.InsertValues(tableName, ConvertToSqlContents(valList.ToArray(), m_ValueTypeLine));
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

        // csv数据格式转化为sql数据格式
        static private string ConvertToSqlContent(string content, string valueType)
        {
            switch(valueType.ToLower())
            {
                case "int":
                    if(string.IsNullOrEmpty(content))
                        return "0";
                    return string.Format($"{content}");         // 不带''可以检查数据正确性
                case "string":
                    if(string.IsNullOrEmpty(content))
                        return string.Format($"''");
                    return string.Format($"'{content}'");
                case "float":
                    if(string.IsNullOrEmpty(content))
                        return "0";
                    return string.Format($"{content}");         // 同int
                case "bool":
                    if(string.IsNullOrEmpty(content))
                        return "false";
                    return string.Format($"{content}");
            }

            if(string.IsNullOrEmpty(content))
                return string.Format($"''");
            return string.Format($"'{content}'");
        }

        static private string[] ConvertToSqlContents(string[] contents, string[] valueTypes)
        {
            string[] ret = new string[valueTypes.Length];
            for(int i = 0; i < valueTypes.Length; ++i)
            {
                ret[i] = ConvertToSqlContent(contents[i], valueTypes[i]);
            }
            return ret;
        }

        static private void GenerateConfigManagerScript()
        {
            
        }
    }
}