using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Framework.AssetManagement.AssetEditorWindow;
using Application.Runtime;
using System.IO;
using System.Text;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using Framework.AssetManagement.Runtime;
using UnityEditor.Build;

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
        static private int[]        m_KeyIndices;       // 关键字所在列的索引
        static private SqlData      m_Sql;
        static private string       m_Info;
        static private bool         m_SkipTheFile;

        private class BuildProcessor : IPreprocessBuildWithReport
        {
            public int callbackOrder { get { return 50; } }

            public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
            {
                OnPostprocessBundleBuild();
            }
        }

        static ConfigBuilder()
        {
            CtrlGEditor.OnPreprocessQuickLaunch += DoRun;
        }

        static private void OnPostprocessBundleBuild()
        {
            DoRun();

            // copy db to streamingAssets
            string dstPath = string.Format($"{UnityEngine.Application.streamingAssetsPath}/{AssetManagerSettings.StreamingAssetsBuildinFolder}/{System.IO.Path.GetFileName(ConfigBuilderSetting.DatabaseFilePath)}");
            FileUtil.DeleteFileOrDirectory(dstPath);
            FileUtil.CopyFileOrDirectory(ConfigBuilderSetting.DatabaseFilePath, dstPath);
        }
        
        [MenuItem("Tools/Config Build %h")]
        static private void Run()
        {
            DoRun();            
            
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
            AssetDatabase.Refresh();
        }

        static public void DoRun()
        {
            m_Info = null;

            if(!DoDBGenerated())
                return;

            if(!GenerateDesignConfigScript())
                return;

            if(!GenerateConfigManagerScript())
                return;
        }

        static private bool GenerateDesignConfigScript()
        {
            m_ScriptContent = "using System.Collections;\nusing System.Collections.Generic;\n\n";
            m_ScriptContent += "namespace " + ConfigBuilderSetting.Namespace + "\n{\n";
            // find all csv
            string[] files = Directory.GetFiles(ConfigBuilderSetting.ConfigPath, "*.csv", SearchOption.AllDirectories);
            foreach(var file in files)
            {
                string info = string.Format($"创建表结构对象: {Path.GetFileName(file)}");
                Debug.Log(info);
                m_Info += info + "\n";

                if(!Prepare(file))
                {
                    info = string.Format($"DesignConfig.cs导出失败：格式出错   {file}");
                    Debug.LogError(info);
                    m_Info += info + "\n";
                    return false;
                }

                if(m_SkipTheFile)
                    continue;
                    
                if(!ParseObjectFromCsv(file, out info))
                {
                    Debug.LogError(info);
                    m_Info += info + "\n";
                    return false;
                }
            }
            Debug.Log($"配置脚本DesignConfig.cs导出完成");

            m_ScriptContent += "}";

            // 脚本代码成功生成再删除老脚本
            try
            {
                if(File.Exists(ConfigBuilderSetting.DesignConfigScriptFilePath))
                {
                    File.Delete(ConfigBuilderSetting.DesignConfigScriptFilePath);
                }
                Directory.CreateDirectory(ConfigBuilderSetting.DesignConfigScriptFilePath.Substring(0, ConfigBuilderSetting.DesignConfigScriptFilePath.LastIndexOf("/")));
            }
            catch(Exception e)
            {
                Debug.LogError(e.Message);
                m_Info += e.Message + "\n";
                return false;
            }

            // serialized script content
            using(FileStream fs = File.Create(ConfigBuilderSetting.DesignConfigScriptFilePath))
            {
                byte[] data = new UTF8Encoding(false).GetBytes(m_ScriptContent);
                fs.Write(data, 0, data.Length);
            }
            return true;
        }

        static private bool Prepare(string file)
        {
            m_AllLines = null;
            try
            {
                using (FileStream fs = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        string line;
                        List<string> allLine = new List<string>();
                        while ((line = sr.ReadLine()) != null)
                        {
                            allLine.Add(line.Replace("\\n", "\n"));
                        }
                        m_AllLines = allLine.ToArray();
                    }
                }
                // m_AllLines = File.ReadAllLines(file);
            }
            catch(Exception e)
            {
                Debug.LogError(e.Message);
                m_Info += e.Message + "\n";
                return false;
            }

            if(m_AllLines.Length < 4)
            {
                string info = string.Format($"配置起始关键行不足：{m_AllLines.Length} < 4   ");
                Debug.LogError(info);
                m_Info += info + "\n";
                return false;
            }
            // Trim value type column \" 
            string tempstr = "";
            for (int i = 0; i < 4; i++)
            {
                tempstr = new string(m_AllLines[i].Replace("\"", "").Trim());
                m_AllLines[i] = tempstr;
            }

            m_ColumnLine = m_AllLines[1].Split(',');
            // Trim Key 行 空格
            for (int i = 0; i < m_ColumnLine.Length; i++)
            {
                m_ColumnLine[i] = m_ColumnLine[i].Trim(' ');
            }

            m_FlagLine = m_AllLines[2].Split(',');
            m_ValueTypeLine = m_AllLines[3].Split(',');
            if(m_ColumnLine.Length != m_ValueTypeLine.Length || m_ColumnLine.Length != m_FlagLine.Length)
            {
                string info = string.Format($"关键行长度不一致： column length[{m_ColumnLine.Length}]   flag length[{m_FlagLine.Length}]  valueType length[{m_ValueTypeLine.Length}]");
                Debug.LogError(info);
                m_Info += info + "\n";
                return false;
            }

            // find the "key"
            m_SkipTheFile = m_FlagLine.Count((obj) => (obj.ToLower().StartsWith("client") || obj.ToLower().StartsWith("all"))) == 0;
            int count = m_FlagLine.Count((obj) => (obj.ToLower().StartsWith("key")));
            if(count == 0)
            {
                string info = string.Format("配置缺少key");
                Debug.LogError(info);
                m_Info += info + "\n";
                return false;
            }
            m_KeyIndices = new int[count];

            List<string> columnList = new List<string>();
            List<string> valueTypeList = new List<string>();
            int index = 0;
            for(int i = 0; i < m_FlagLine.Length; ++i)
            {
                try
                {
                    if(ColumnNeedImport(m_FlagLine, i))
                    {
                        columnList.Add(m_ColumnLine[i]);
                        valueTypeList.Add(m_ValueTypeLine[i]);

                        if (m_FlagLine[i].ToLower().StartsWith("key"))
                        {
                            m_KeyIndices[index++] = columnList.Count - 1;
                        }
                    }
                }
                catch(System.Exception e)
                {
                    Debug.LogError(e.Message);
                    m_Info += e.Message + "\n";
                    return false;
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
        static private bool ParseObjectFromCsv(string file, out string error)
        {
            error = null;
            string tableName = GetFinalTableName(Path.GetFileNameWithoutExtension(file));
            m_ScriptContent += "    public class " + tableName + "\n    {\n";
            for(int i = 0; i < m_ColumnLine.Length; ++i)
            {
                string typeString = ConvertValueTypeToText(m_ValueTypeLine[i]);
                if(string.IsNullOrEmpty(typeString))
                {
                    error = string.Format($"非法的值类型:  {m_ValueTypeLine[i]}  检查第{i + 1}列");
                    return false;
                }
                
                m_ScriptContent += "        public " + typeString + " " + m_ColumnLine[i];
                m_ScriptContent += AppendInitIfNeed(m_ValueTypeLine[i]);
                m_ScriptContent += ";\n";
            }
            m_ScriptContent += "    }\n\n";
            return true;
        }

        static private string AppendInitIfNeed(string valueType)
        {
            valueType = valueType.ToLower();
            if(valueType == "array<string>")
            {
                return " = new List<string>()";
            }
            else if(valueType == "array<int>")
            {
                return " = new List<int>()";
            }
            else if(valueType == "array<float>")
            {
                return " = new List<float>()";
            }
            else if(valueType == "dict<int:int>")
            {
                return " = new Dictionary<int, int>()";
            }
            else if(valueType == "dict<int:float>")
            {
                return " = new Dictionary<int, float>()";
            }
            else if(valueType == "dict<int:string>")
            {
                return " = new Dictionary<int, string>()";
            }
            else if(valueType == "dict<string:int>")
            {
                return " = new Dictionary<string, int>()";
            }
            else if(valueType == "dict<string:float>")
            {
                return " = new Dictionary<string, float>()";
            }
            else if(valueType == "dict<string:string>")
            {
                return " = new Dictionary<string, string>()";
            }
            return "";
        }

        static private string ConvertValueTypeToText(string valueType)
        {
            string vt = valueType.ToLower();
            switch(vt)
            {
                case "int":
                    return "int";
                case "float":
                    return "float";
                case "string":
                    return "string";
                case "bool":
                    return "bool";
                case "array<string>":
                    return "List<string>";
                case "array<int>":
                    return "List<int>";
                case "array<float>":
                    return "List<float>";
                case "dict<int:int>":
                    return "Dictionary<int, int>";
                case "dict<int:string>":
                    return "Dictionary<int, string>";
                case "dict<int:float>":
                    return "Dictionary<int, float>";
                case "dict<string:int>":
                    return "Dictionary<string, int>";
                case "dict<string:float>":
                    return "Dictionary<string, float>";
                case "dict<string:string>":
                    return "Dictionary<string, string>";
            }

            if(valueType.StartsWith("reference@"))
            {
                return GetFinalTableName(valueType.Substring(valueType.IndexOf("@") + 1));
            }            
            return null;
        }

        static private bool ColumnNeedImport(string[] flags, int index)
        {
            if(index < 0 || index >= flags.Length)
                throw new System.ArgumentOutOfRangeException($"index: {index} out of range [{flags.Length}]");
            string flag = flags[index].ToLower();
            return flag.Contains("key") || flag.Contains("all") || flag.Contains("client");
        }

        // static private bool FileNeedImport(string file)
        // {
        //     int count = m_FlagLine.Count((obj) => (obj.ToLower().StartsWith("key")));
        // }

        static private bool DoDBGenerated()
        {
            try
            {
                if(File.Exists(ConfigBuilderSetting.DatabaseFilePath))
                {
                    File.Delete(ConfigBuilderSetting.DatabaseFilePath);
                }
                Directory.CreateDirectory(ConfigBuilderSetting.DatabaseFilePath.Substring(0, ConfigBuilderSetting.DatabaseFilePath.LastIndexOf("/")));
            }
            catch(Exception e)
            {
                Debug.LogError(e.Message);
                m_Info += e.Message + "\n";
                return false;
            }

            m_Sql = new SqlData(ConfigBuilderSetting.DatabaseFilePath);

            // find all csv
            string[] files = Directory.GetFiles(ConfigBuilderSetting.ConfigPath, "*.csv", SearchOption.AllDirectories);
            foreach(var file in files)
            {
                string info = string.Format($"创建表数据: {Path.GetFileName(file)}");
                Debug.Log(info);
                m_Info += info + "\n";

                if(!Prepare(file))
                {
                    info = string.Format($"数据库导出失败：格式出错   {file}");
                    Debug.LogError(info);
                    m_Info += info + "\n";
                    m_Sql.Close();
                    return false;
                }

                if(m_SkipTheFile)
                    continue;

                string error = CreateTableFromCsv(file);
                if(!string.IsNullOrEmpty(error))
                {
                    Debug.LogError(error);
                    m_Info += error + "\n";
                    m_Sql.Close();
                    return false;
                }
            }
            Debug.Log($"数据库导出完成: {ConfigBuilderSetting.DatabaseFilePath}");

            m_Sql.Close();
            m_Sql = null;
            return true;
        }

        static private string CreateTableFromCsv(string file)
        {
            string tableName = GetFinalTableName(Path.GetFileNameWithoutExtension(file));
            m_Sql.CreateTable(tableName, m_ColumnLine, ConvertCustomizedValueTypesToSql(m_ValueTypeLine));

            m_Sql.BeginTransaction();
            for(int i = 4; i < m_AllLines.Length; ++i)
            {
                string[] values = Regex.Split(m_AllLines[i], ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

                if(values.Length != m_FlagLine.Length)
                {
                    return string.Format($"检查是否有配置非法字符\",\"：第{i + 1}行");
                }

                // 过滤服务器字段
                List<string> valList = new List<string>();
                for(int j = 0; j < values.Length; ++j)
                {
                    if(ColumnNeedImport(m_FlagLine, j))
                        valList.Add(values[j]);
                }
                try
                {
                    // m_Sql.InsertValues(tableName, ConvertToSqlContents(valList.ToArray(), m_ValueTypeLine));
                    m_Sql.InsertValuesWithTransaction(tableName, ConvertToSqlContents(valList.ToArray(), m_ValueTypeLine));
                }
                catch(Exception e)
                {
                    Debug.LogError(e.Message);
                    return e.Message;
                }
            }
            m_Sql.EndTransaction();
            return null;
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
            if(valueType.ToLower().StartsWith("reference@"))
            { // 引用其他表数据，记录ID
                return "INTEGER";
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
            string tempstr;
            switch (valueType.ToLower())
            {
                case "int":
                    if(string.IsNullOrEmpty(content))
                        return "0";

                    tempstr = new string(content.Replace("\"", "").Trim());
                    content = tempstr;
                    return string.Format($"{content}");         // 不带''可以检查数据正确性
                case "string":
                    if(string.IsNullOrEmpty(content))
                        return string.Format($"''");
                    return string.Format("'{0}'",PostprocessContent(content));
                case "float":
                    if(string.IsNullOrEmpty(content))
                        return "0";
                    tempstr = new string(content.Replace("\"", "").Trim());
                    content = tempstr;
                    return string.Format($"{content}");         // 同int
                case "bool":
                    if(content.ToLower() == "yes")
                        return "true";
                    return "false";
            }

            tempstr = new string(content.Replace("\"", "").Trim());
            content = tempstr;
            if (string.IsNullOrEmpty(content))
                return string.Format($"''");
            return string.Format($"'{content}'");
        }

        static private string PostprocessContent(string content)
        {
            content = content.Replace("'", "''");
            return content;
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

        static private bool GenerateConfigManagerScript()
        {
            string content = null;
            string[] lines = null;
            string[] files = null;
            try
            {
                lines = File.ReadAllLines(ConfigBuilderSetting.ConfigManagerTemplateFilePath);
                files = Directory.GetFiles(ConfigBuilderSetting.ConfigPath, "*.csv", SearchOption.AllDirectories);
            }
            catch(Exception e)
            {
                Debug.LogError(e.Message);
                m_Info += e.Message + "\n";
                return false;
            }

            if(!ParseConfigManager(ref content, lines, files))
            {
                string error = string.Format($"ConfigManager导出失败");
                Debug.LogError(error);
                m_Info += error + "\n";
                return false;
            }

            content += "\n";

            // 脚本代码成功生成再删除老脚本
            try
            {
                if(File.Exists(ConfigBuilderSetting.ConfigManagerScriptFilePath))
                {
                    File.Delete(ConfigBuilderSetting.ConfigManagerScriptFilePath);
                }
                Directory.CreateDirectory(ConfigBuilderSetting.ConfigManagerScriptFilePath.Substring(0, ConfigBuilderSetting.ConfigManagerScriptFilePath.LastIndexOf("/")));
            }
            catch(Exception e)
            {
                Debug.LogError(e.Message);
                m_Info += e.Message + "\n";
                return false;
            }

            using(FileStream fs = File.Create(ConfigBuilderSetting.ConfigManagerScriptFilePath))
            {
                byte[] data = new UTF8Encoding(false).GetBytes(content);
                fs.Write(data, 0, data.Length);
            }
            string info = string.Format($"ConfigManager导出完成");
            m_Info += info + "\n";
            return true;
        }

        static private bool ParseConfigManager(ref string content, string[] lines, string[] files)
        {
            for(int i = 0; i < lines.Length; ++i)
            {
                string flag = ExtractFlag(lines[i]);
                if(string.IsNullOrEmpty(flag))
                { // 无标签
                    content += lines[i] + "\n";
                }
                else
                {
                    if(string.Compare(flag, "#ITERATOR_BEGIN#", true) == 0)
                    {
                        int lastIndex = FindFlag("#ITERATOR_END#", lines, i + 1);
                        if(lastIndex == -1)
                        {
                            Debug.LogError($"can't find the flag \"#ITERATOR_END#\"");
                            return false;
                        }

                        // 剔除首尾标签行，选取中间数据
                        string[] subLines = lines.Where((lines, index) => index > i && index < lastIndex).ToArray();
                        foreach(var file in files)
                        {
                            string info = string.Format($"----ConfigManager.{Path.GetFileName(file)}");
                            Debug.Log(info);
                            m_Info += info + "\n";
                            if (!Prepare(file))
                            {
                                info = string.Format($"ConfigManager导出失败：格式出错   {file}");
                                Debug.LogError(info);
                                m_Info += info + "\n";
                                return false;
                            }

                            if(m_SkipTheFile)
                                continue;

                            string tableName = GetFinalTableName(Path.GetFileNameWithoutExtension(file));
                            if(!ParseConfigObjectToManager(ref content, tableName, subLines))
                                return false;
                            content += "\n";
                        }

                        i = lastIndex;  // skip to last index
                    }
                }
            }
            return true;
        }

        static private bool ParseConfigObjectToManager(ref string content, string tableName, string[] lines)
        {
            for(int i = 0; i < lines.Length; ++i)
            {
                string flag = ExtractFlag(lines[i]);
                if(string.IsNullOrEmpty(flag))
                { // 无标签
                    content += lines[i] + "\n";
                }
                else
                {
                    if(string.Compare(flag, "#READER_VARIANT_BEGIN#", true) == 0)
                    { // 遍历非数组变量
                        int lastIndex = FindFlag("#READER_VARIANT_END#", lines, i + 1);
                        if(lastIndex == -1)
                        {
                            Debug.LogError($"can't find the flag \"#READER_VARIANT_END#\"");
                            return false;
                        }
                        string[] subLines = lines.Where((lines, index) => index > i && index < lastIndex).ToArray();
                        Debug.Assert(subLines.Length == 1);

                        for(int j = 0; j < m_ColumnLine.Length; ++j)
                        {
                            if(m_ValueTypeLine[j].StartsWith("array<", StringComparison.OrdinalIgnoreCase)
                            || m_ValueTypeLine[j].StartsWith("reference@", StringComparison.OrdinalIgnoreCase)
                            || m_ValueTypeLine[j].StartsWith("dict<", StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }

                            content += subLines[0].Replace("#VARIANT#", m_ColumnLine[j]).Replace("#READER_FUNCTION#", GetSQLFunctionNameByValueType(m_ValueTypeLine[j])) + "\n";
                        }

                        i = lastIndex;
                    }
                    else if(string.Compare(flag, "#READER_LIST_BEGIN#", true) == 0)
                    { // 遍历数组变量
                        int lastIndex = FindFlag("#READER_LIST_END#", lines, i + 1);
                        if(lastIndex == -1)
                        {
                            Debug.LogError($"can't find the flag \"#READER_LIST_END#\"");
                            return false;
                        }
                        string[] subLines = lines.Where((lines, index) => index > i && index < lastIndex).ToArray();
                        Debug.Assert(subLines.Length == 1);

                        for(int j = 0; j < m_ColumnLine.Length; ++j)
                        {
                            if(!m_ValueTypeLine[j].StartsWith("array<", StringComparison.OrdinalIgnoreCase)
                            && !m_ValueTypeLine[j].StartsWith("dict<", StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }

                            content += subLines[0].Replace("#VARIANT#", m_ColumnLine[j]) + "\n";
                        }

                        i= lastIndex;
                    }
                    else if(string.Compare(flag, "#READER_REFERENCE_BEGIN#", true) == 0)
                    { // 解析对其他表的引用
                        int lastIndex = FindFlag("#READER_REFERENCE_END#", lines, i + 1);
                        if(lastIndex == -1)
                        {
                            Debug.LogError($"can't find the flag \"#READER_REFERENCE_END#\"");
                            return false;
                        }
                        string[] subLines = lines.Where((lines, index) => index > i && index < lastIndex).ToArray();
                        Debug.Assert(subLines.Length == 1);

                        for(int j = 0; j < m_ColumnLine.Length; ++j)
                        {
                            if(!m_ValueTypeLine[j].StartsWith("reference@"))
                            {
                                continue;
                            }

                            string referencedTableName = m_ValueTypeLine[j].Substring(m_ValueTypeLine[j].IndexOf("@") + 1);
                            content += subLines[0].Replace("#VARIANT#", m_ColumnLine[j]).Replace("#TABLENAME#", GetFinalTableName(referencedTableName)) + "\n";
                        }

                        i = lastIndex;
                    }
                    else if(string.Compare(flag, "#FINDER_1KEY_BEGIN#", true) == 0)
                    {
                        int lastIndex = FindFlag("#FINDER_1KEY_END#", lines, i + 1);
                        if(lastIndex == -1)
                        {
                            Debug.LogError($"can't find the flag \"#FINDER_1KEY_END#\"");
                            return false;
                        }
                        // 处理单key
                        if(m_KeyIndices.Length != 1)
                        {
                            i = lastIndex;
                            continue;
                        }

                        string[] subLines = lines.Where((lines, index) => index > i && index < lastIndex).ToArray();

                        foreach(var line in subLines)
                        {
                            string label = ExtractFlag(line);
                            if(string.IsNullOrEmpty(label))
                            { // 无标签
                                content += line + "\n";
                            }
                            else
                            {
                                string text = line;
                                if (text.IndexOf("#TABLENAME#") != -1)
                                {
                                    text = text.Replace("#TABLENAME#", tableName);
                                }
                                if (text.IndexOf("#KEY1_VALUETYPE#") != -1)
                                {
                                    text = text.Replace("#KEY1_VALUETYPE#", m_ValueTypeLine[0]);       // key默认放第一个
                                }
                                if (text.IndexOf("#KEY1_NAME#") != -1)
                                {
                                    text = text.Replace("#KEY1_NAME#", m_ColumnLine[m_KeyIndices[0]]);
                                }
                                content += text + "\n";
                            }
                        }

                        i = lastIndex;
                    }
                    else if(string.Compare(flag, "#FINDER_2KEY_BEGIN#", true) == 0)
                    {
                        int lastIndex = FindFlag("#FINDER_2KEY_END#", lines, i + 1);
                        if(lastIndex == -1)
                        {
                            Debug.LogError($"can't find the flag \"#FINDER_2KEY_END#\"");
                            return false;
                        }
                        // 处理双key
                        if(m_KeyIndices.Length != 2)
                        {
                            i = lastIndex;
                            continue;
                        }

                        string[] subLines = lines.Where((lines, index) => index > i && index < lastIndex).ToArray();

                        foreach(var line in subLines)
                        {
                            string label = ExtractFlag(line);
                            if(string.IsNullOrEmpty(label))
                            { // 无标签
                                content += line + "\n";
                            }
                            else
                            {
                                string text = line;
                                if (text.IndexOf("#TABLENAME#") != -1)
                                {
                                    text = text.Replace("#TABLENAME#", tableName);
                                }
                                if (text.IndexOf("#KEY1_VALUETYPE#") != -1)
                                {
                                    text = text.Replace("#KEY1_VALUETYPE#", m_ValueTypeLine[0]);       // key默认放第一个
                                }
                                if (text.IndexOf("#KEY2_VALUETYPE#") != -1)
                                {
                                    text = text.Replace("#KEY2_VALUETYPE#", m_ValueTypeLine[1]);
                                }
                                content += text + "\n";
                            }
                        }

                        i = lastIndex;
                    }
                    else if(string.Compare(flag, "#FINDER_3KEY_BEGIN#", true) == 0)
                    {
                        int lastIndex = FindFlag("#FINDER_3KEY_END#", lines, i + 1);
                        if(lastIndex == -1)
                        {
                            Debug.LogError($"can't find the flag \"#FINDER_3KEY_END#\"");
                            return false;
                        }
                        // 处理三key
                        if(m_KeyIndices.Length != 3)
                        {
                            i = lastIndex;
                            continue;
                        }

                        string[] subLines = lines.Where((lines, index) => index > i && index < lastIndex).ToArray();

                        foreach(var line in subLines)
                        {
                            string label = ExtractFlag(line);
                            if(string.IsNullOrEmpty(label))
                            { // 无标签
                                content += line + "\n";
                            }
                            else
                            {
                                string text = line;
                                if (text.IndexOf("#TABLENAME#") != -1)
                                {
                                    text = text.Replace("#TABLENAME#", tableName);
                                }
                                if (text.IndexOf("#KEY1_VALUETYPE#") != -1)
                                {
                                    text = text.Replace("#KEY1_VALUETYPE#", m_ValueTypeLine[0]);       // key默认放第一个
                                }
                                if (text.IndexOf("#KEY2_VALUETYPE#") != -1)
                                {
                                    text = text.Replace("#KEY2_VALUETYPE#", m_ValueTypeLine[1]);
                                }
                                if (text.IndexOf("#KEY3_VALUETYPE#") != -1)
                                {
                                    text = text.Replace("#KEY3_VALUETYPE#", m_ValueTypeLine[2]);
                                }
                                content += text + "\n";
                            }
                        }

                        i = lastIndex;
                    }
                    else if(string.Compare(flag, "#FINDER_4KEY_BEGIN#", true) == 0)
                    {
                        int lastIndex = FindFlag("#FINDER_4KEY_END#", lines, i + 1);
                        if(lastIndex == -1)
                        {
                            Debug.LogError($"can't find the flag \"#FINDER_4KEY_END#\"");
                            return false;
                        }
                        // 处理四key
                        if(m_KeyIndices.Length != 4)
                        {
                            i = lastIndex;
                            continue;
                        }

                        string[] subLines = lines.Where((lines, index) => index > i && index < lastIndex).ToArray();

                        foreach(var line in subLines)
                        {
                            string label = ExtractFlag(line);
                            if(string.IsNullOrEmpty(label))
                            { // 无标签
                                content += line + "\n";
                            }
                            else
                            {
                                string text = line;
                                if (text.IndexOf("#TABLENAME#") != -1)
                                {
                                    text = text.Replace("#TABLENAME#", tableName);
                                }
                                if (text.IndexOf("#KEY1_VALUETYPE#") != -1)
                                {
                                    text = text.Replace("#KEY1_VALUETYPE#", m_ValueTypeLine[0]);       // key默认放第一个
                                }
                                if (text.IndexOf("#KEY2_VALUETYPE#") != -1)
                                {
                                    text = text.Replace("#KEY2_VALUETYPE#", m_ValueTypeLine[1]);
                                }
                                if (text.IndexOf("#KEY3_VALUETYPE#") != -1)
                                {
                                    text = text.Replace("#KEY3_VALUETYPE#", m_ValueTypeLine[2]);
                                }
                                if (text.IndexOf("#KEY4_VALUETYPE#") != -1)
                                {
                                    text = text.Replace("#KEY4_VALUETYPE#", m_ValueTypeLine[3]);
                                }
                                content += text + "\n";
                            }
                        }

                        i = lastIndex;
                    }
                    else if(string.Compare(flag, "#SELECT_FIND_1KEY_BEGIN#", true) == 0)
                    {
                        int lastIndex = FindFlag("#SELECT_FIND_1KEY_END#", lines, i + 1);
                        if(lastIndex == -1)
                        {
                            Debug.LogError($"can't find the flag \"#SELECT_FIND_1KEY_END#\"");
                            return false;
                        }
                        // 处理单key
                        if(m_KeyIndices.Length != 1)
                        {
                            i = lastIndex;
                            continue;
                        }

                        string[] subLines = lines.Where((lines, index) => index > i && index < lastIndex).ToArray();
                        Debug.Assert(subLines.Length == 1);

                        content += subLines[0].Replace("#TABLENAME#", tableName) + "\n";

                        i = lastIndex;
                    }
                    else if(string.Compare(flag, "#SELECT_FIND_2KEY_BEGIN#", true) == 0)
                    {
                        int lastIndex = FindFlag("#SELECT_FIND_2KEY_END#", lines, i + 1);
                        if(lastIndex == -1)
                        {
                            Debug.LogError($"can't find the flag \"#SELECT_FIND_2KEY_END#\"");
                            return false;
                        }
                        // 处理双key
                        if(m_KeyIndices.Length != 2)
                        {
                            i = lastIndex;
                            continue;
                        }

                        string[] subLines = lines.Where((lines, index) => index > i && index < lastIndex).ToArray();
                        Debug.Assert(subLines.Length == 1);

                        content += subLines[0].Replace("#TABLENAME#", tableName) + "\n";

                        i = lastIndex;
                    }
                    else if(string.Compare(flag, "#SELECT_FIND_3KEY_BEGIN#", true) == 0)
                    {
                        int lastIndex = FindFlag("#SELECT_FIND_3KEY_END#", lines, i + 1);
                        if(lastIndex == -1)
                        {
                            Debug.LogError($"can't find the flag \"#SELECT_FIND_3KEY_END#\"");
                            return false;
                        }
                        // 处理三key
                        if(m_KeyIndices.Length != 3)
                        {
                            i = lastIndex;
                            continue;
                        }

                        string[] subLines = lines.Where((lines, index) => index > i && index < lastIndex).ToArray();
                        Debug.Assert(subLines.Length == 1);

                        content += subLines[0].Replace("#TABLENAME#", tableName) + "\n";

                        i = lastIndex;
                    }
                    else if(string.Compare(flag, "#SELECT_FIND_4KEY_BEGIN#", true) == 0)
                    {
                        int lastIndex = FindFlag("#SELECT_FIND_4KEY_END#", lines, i + 1);
                        if(lastIndex == -1)
                        {
                            Debug.LogError($"can't find the flag \"#SELECT_FIND_4KEY_END#\"");
                            return false;
                        }
                        // 处理四key
                        if(m_KeyIndices.Length != 4)
                        {
                            i = lastIndex;
                            continue;
                        }

                        string[] subLines = lines.Where((lines, index) => index > i && index < lastIndex).ToArray();
                        Debug.Assert(subLines.Length == 1);

                        content += subLines[0].Replace("#TABLENAME#", tableName) + "\n";

                        i = lastIndex;
                    }
                    else if(string.Compare(flag, "#SELECT_ONE_KEY_BEGIN#", true) == 0)
                    {
                        int lastIndex = FindFlag("#SELECT_ONE_KEY_END#", lines, i + 1);
                        if(lastIndex == -1)
                        {
                            Debug.LogError($"can't find the flag \"#SELECT_ONE_KEY_END#\"");
                            return false;
                        }
                        // 处理单key
                        if(m_KeyIndices.Length != 1)
                        {
                            i = lastIndex;
                            continue;
                        }

                        string[] subLines = lines.Where((lines, index) => index > i && index < lastIndex).ToArray();
                        Debug.Assert(subLines.Length == 1);

                        string text = subLines[0];
                        text = text.Replace("#TABLENAME#", tableName);
                        text = text.Replace("#KEY1_VALUETYPE#", m_ValueTypeLine[m_KeyIndices[0]]);
                        content += text + "\n";

                        i = lastIndex;
                    }
                    else if(string.Compare(flag, "#SELECT_TWO_KEY_BEGIN#", true) == 0)
                    {
                        int lastIndex = FindFlag("#SELECT_TWO_KEY_END#", lines, i + 1);
                        if(lastIndex == -1)
                        {
                            Debug.LogError($"can't find the flag \"#SELECT_TWO_KEY_END#\"");
                            return false;
                        }
                        // 处理双key
                        if(m_KeyIndices.Length != 2)
                        {
                            i = lastIndex;
                            continue;
                        }

                        string[] subLines = lines.Where((lines, index) => index > i && index < lastIndex).ToArray();
                        Debug.Assert(subLines.Length == 1);

                        string text = subLines[0];
                        text = text.Replace("#TABLENAME#", tableName);
                        text = text.Replace("#KEY1_VALUETYPE#", m_ValueTypeLine[m_KeyIndices[0]]);
                        text = text.Replace("#KEY2_VALUETYPE#", m_ValueTypeLine[m_KeyIndices[1]]);
                        content += text + "\n";

                        i = lastIndex;
                    }
                    else if(string.Compare(flag, "#SELECT_THREE_KEY_BEGIN#", true) == 0)
                    {
                        int lastIndex = FindFlag("#SELECT_THREE_KEY_END#", lines, i + 1);
                        if(lastIndex == -1)
                        {
                            Debug.LogError($"can't find the flag \"#SELECT_THREE_KEY_END#\"");
                            return false;
                        }
                        // 处理三key
                        if(m_KeyIndices.Length != 3)
                        {
                            i = lastIndex;
                            continue;
                        }

                        string[] subLines = lines.Where((lines, index) => index > i && index < lastIndex).ToArray();
                        Debug.Assert(subLines.Length == 1);

                        string text = subLines[0];
                        text = text.Replace("#TABLENAME#", tableName);
                        text = text.Replace("#KEY1_VALUETYPE#", m_ValueTypeLine[m_KeyIndices[0]]);
                        text = text.Replace("#KEY2_VALUETYPE#", m_ValueTypeLine[m_KeyIndices[1]]);
                        text = text.Replace("#KEY3_VALUETYPE#", m_ValueTypeLine[m_KeyIndices[2]]);
                        content += text + "\n";

                        i = lastIndex;
                    }
                    else if(string.Compare(flag, "#SELECT_FOUR_KEY_BEGIN#", true) == 0)
                    {
                        int lastIndex = FindFlag("#SELECT_FOUR_KEY_END#", lines, i + 1);
                        if(lastIndex == -1)
                        {
                            Debug.LogError($"can't find the flag \"#SELECT_FOUR_KEY_END#\"");
                            return false;
                        }
                        // 处理四key
                        if(m_KeyIndices.Length != 4)
                        {
                            i = lastIndex;
                            continue;
                        }

                        string[] subLines = lines.Where((lines, index) => index > i && index < lastIndex).ToArray();
                        Debug.Assert(subLines.Length == 1);

                        string text = subLines[0];
                        text = text.Replace("#TABLENAME#", tableName);
                        text = text.Replace("#KEY1_VALUETYPE#", m_ValueTypeLine[m_KeyIndices[0]]);
                        text = text.Replace("#KEY2_VALUETYPE#", m_ValueTypeLine[m_KeyIndices[1]]);
                        text = text.Replace("#KEY3_VALUETYPE#", m_ValueTypeLine[m_KeyIndices[2]]);
                        text = text.Replace("#KEY4_VALUETYPE#", m_ValueTypeLine[m_KeyIndices[3]]);
                        content += text + "\n";

                        i = lastIndex;
                    }
                    else if(string.Compare(flag, "#SELECT_DICT_ONE_KEY_BEGIN#", true) == 0)
                    {
                        int lastIndex = FindFlag("#SELECT_DICT_ONE_KEY_END#", lines, i + 1);
                        if(lastIndex == -1)
                        {
                            Debug.LogError($"can't find the flag \"#SELECT_DICT_ONE_KEY_END#\"");
                            return false;
                        }
                        // 处理单key
                        if(m_KeyIndices.Length != 1)
                        {
                            i = lastIndex;
                            continue;
                        }

                        string[] subLines = lines.Where((lines, index) => index > i && index < lastIndex).ToArray();
                        Debug.Assert(subLines.Length == 1);

                        string text = subLines[0];
                        text = text.Replace("#TABLENAME#", tableName);
                        text = text.Replace("#KEY1_VALUETYPE#", m_ValueTypeLine[m_KeyIndices[0]]);
                        content += text + "\n";

                        i = lastIndex;
                    }
                    else if(string.Compare(flag, "#SELECT_DICT_TWO_KEY_BEGIN#", true) == 0)
                    {
                        int lastIndex = FindFlag("#SELECT_DICT_TWO_KEY_END#", lines, i + 1);
                        if(lastIndex == -1)
                        {
                            Debug.LogError($"can't find the flag \"#SELECT_DICT_TWO_KEY_END#\"");
                            return false;
                        }
                        // 处理双key
                        if(m_KeyIndices.Length != 2)
                        {
                            i = lastIndex;
                            continue;
                        }

                        string[] subLines = lines.Where((lines, index) => index > i && index < lastIndex).ToArray();
                        Debug.Assert(subLines.Length == 1);

                        string text = subLines[0];
                        text = text.Replace("#TABLENAME#", tableName);
                        text = text.Replace("#KEY1_VALUETYPE#", m_ValueTypeLine[m_KeyIndices[0]]);
                        text = text.Replace("#KEY2_VALUETYPE#", m_ValueTypeLine[m_KeyIndices[1]]);
                        content += text + "\n";

                        i = lastIndex;
                    }
                    else if(string.Compare(flag, "#SELECT_DICT_THREE_KEY_BEGIN#", true) == 0)
                    {
                        int lastIndex = FindFlag("#SELECT_DICT_THREE_KEY_END#", lines, i + 1);
                        if(lastIndex == -1)
                        {
                            Debug.LogError($"can't find the flag \"#SELECT_DICT_THREE_KEY_END#\"");
                            return false;
                        }
                        // 处理三key
                        if(m_KeyIndices.Length != 3)
                        {
                            i = lastIndex;
                            continue;
                        }

                        string[] subLines = lines.Where((lines, index) => index > i && index < lastIndex).ToArray();
                        Debug.Assert(subLines.Length == 1);

                        string text = subLines[0];
                        text = text.Replace("#TABLENAME#", tableName);
                        text = text.Replace("#KEY1_VALUETYPE#", m_ValueTypeLine[m_KeyIndices[0]]);
                        text = text.Replace("#KEY2_VALUETYPE#", m_ValueTypeLine[m_KeyIndices[1]]);
                        text = text.Replace("#KEY3_VALUETYPE#", m_ValueTypeLine[m_KeyIndices[2]]);
                        content += text + "\n";

                        i = lastIndex;
                    }
                    else if(string.Compare(flag, "#SELECT_DICT_FOUR_KEY_BEGIN#", true) == 0)
                    {
                        int lastIndex = FindFlag("#SELECT_DICT_FOUR_KEY_END#", lines, i + 1);
                        if(lastIndex == -1)
                        {
                            Debug.LogError($"can't find the flag \"#SELECT_DICT_FOUR_KEY_END#\"");
                            return false;
                        }
                        // 处理四key
                        if(m_KeyIndices.Length != 4)
                        {
                            i = lastIndex;
                            continue;
                        }

                        string[] subLines = lines.Where((lines, index) => index > i && index < lastIndex).ToArray();
                        Debug.Assert(subLines.Length == 1);

                        string text = subLines[0];
                        text = text.Replace("#TABLENAME#", tableName);
                        text = text.Replace("#KEY1_VALUETYPE#", m_ValueTypeLine[m_KeyIndices[0]]);
                        text = text.Replace("#KEY2_VALUETYPE#", m_ValueTypeLine[m_KeyIndices[1]]);
                        text = text.Replace("#KEY3_VALUETYPE#", m_ValueTypeLine[m_KeyIndices[2]]);
                        text = text.Replace("#KEY4_VALUETYPE#", m_ValueTypeLine[m_KeyIndices[3]]);
                        content += text + "\n";

                        i = lastIndex;
                    }
                    else if(string.Compare(flag, "#SELECT_READER_ONE_KEY_BEGIN#", true) == 0)
                    {
                        int lastIndex = FindFlag("#SELECT_READER_ONE_KEY_END#", lines, i + 1);
                        if(lastIndex == -1)
                        {
                            Debug.LogError($"can't find the flag \"#SELECT_READER_ONE_KEY_END#\"");
                            return false;
                        }
                        // 处理单key
                        if(m_KeyIndices.Length != 1)
                        {
                            i = lastIndex;
                            continue;
                        }

                        string[] subLines = lines.Where((lines, index) => index > i && index < lastIndex).ToArray();
                        Debug.Assert(subLines.Length == 1);

                        string text = subLines[0];
                        text = text.Replace("#KEY1_NAME#", m_ColumnLine[m_KeyIndices[0]]);
                        content += text + "\n";

                        i = lastIndex;
                    }
                    else if(string.Compare(flag, "#SELECT_READER_TWO_KEY_BEGIN#", true) == 0)
                    {
                        int lastIndex = FindFlag("#SELECT_READER_TWO_KEY_END#", lines, i + 1);
                        if(lastIndex == -1)
                        {
                            Debug.LogError($"can't find the flag \"#SELECT_READER_TWO_KEY_END#\"");
                            return false;
                        }
                        // 处理双key
                        if(m_KeyIndices.Length != 2)
                        {
                            i = lastIndex;
                            continue;
                        }

                        string[] subLines = lines.Where((lines, index) => index > i && index < lastIndex).ToArray();
                        Debug.Assert(subLines.Length == 1);

                        string text = subLines[0];
                        text = text.Replace("#KEY1_NAME#", m_ColumnLine[m_KeyIndices[0]]);
                        text = text.Replace("#KEY2_NAME#", m_ColumnLine[m_KeyIndices[1]]);
                        content += text + "\n";

                        i = lastIndex;
                    }
                    else if(string.Compare(flag, "#SELECT_READER_THREE_KEY_BEGIN#", true) == 0)
                    {
                        int lastIndex = FindFlag("#SELECT_READER_THREE_KEY_END#", lines, i + 1);
                        if(lastIndex == -1)
                        {
                            Debug.LogError($"can't find the flag \"#SELECT_READER_THREE_KEY_END#\"");
                            return false;
                        }
                        // 处理三key
                        if(m_KeyIndices.Length != 3)
                        {
                            i = lastIndex;
                            continue;
                        }

                        string[] subLines = lines.Where((lines, index) => index > i && index < lastIndex).ToArray();
                        Debug.Assert(subLines.Length == 1);

                        string text = subLines[0];
                        text = text.Replace("#KEY1_NAME#", m_ColumnLine[m_KeyIndices[0]]);
                        text = text.Replace("#KEY2_NAME#", m_ColumnLine[m_KeyIndices[1]]);
                        text = text.Replace("#KEY3_NAME#", m_ColumnLine[m_KeyIndices[2]]);
                        content += text + "\n";

                        i = lastIndex;
                    }
                    else if(string.Compare(flag, "#SELECT_READER_FOUR_KEY_BEGIN#", true) == 0)
                    {
                        int lastIndex = FindFlag("#SELECT_READER_FOUR_KEY_END#", lines, i + 1);
                        if(lastIndex == -1)
                        {
                            Debug.LogError($"can't find the flag \"#SELECT_READER_FOUR_KEY_END#\"");
                            return false;
                        }
                        // 处理四key
                        if(m_KeyIndices.Length != 4)
                        {
                            i = lastIndex;
                            continue;
                        }

                        string[] subLines = lines.Where((lines, index) => index > i && index < lastIndex).ToArray();
                        Debug.Assert(subLines.Length == 1);

                        string text = subLines[0];
                        text = text.Replace("#KEY1_NAME#", m_ColumnLine[m_KeyIndices[0]]);
                        text = text.Replace("#KEY2_NAME#", m_ColumnLine[m_KeyIndices[1]]);
                        text = text.Replace("#KEY3_NAME#", m_ColumnLine[m_KeyIndices[2]]);
                        text = text.Replace("#KEY4_NAME#", m_ColumnLine[m_KeyIndices[3]]);
                        content += text + "\n";

                        i = lastIndex;
                    }
                    else if(string.Compare(flag, "#REMOVE_ONE_KEY_BEGIN#", true) == 0)
                    {
                        int lastIndex = FindFlag("#REMOVE_ONE_KEY_END#", lines, i + 1);
                        if(lastIndex == -1)
                        {
                            Debug.LogError($"can't find the flag \"#REMOVE_ONE_KEY_END#\"");
                            return false;
                        }
                        // 处理单key
                        if(m_KeyIndices.Length != 1)
                        {
                            i = lastIndex;
                            continue;
                        }

                        string[] subLines = lines.Where((lines, index) => index > i && index < lastIndex).ToArray();
                        Debug.Assert(subLines.Length == 1);

                        content += subLines[0].Replace("#TABLENAME#", tableName) + "\n";

                        i = lastIndex;
                    }
                    else if(string.Compare(flag, "#REMOVE_TWO_KEY_BEGIN#", true) == 0)
                    {
                        int lastIndex = FindFlag("#REMOVE_TWO_KEY_END#", lines, i + 1);
                        if(lastIndex == -1)
                        {
                            Debug.LogError($"can't find the flag \"#REMOVE_TWO_KEY_END#\"");
                            return false;
                        }
                        // 处理双key
                        if(m_KeyIndices.Length != 2)
                        {
                            i = lastIndex;
                            continue;
                        }

                        string[] subLines = lines.Where((lines, index) => index > i && index < lastIndex).ToArray();
                        Debug.Assert(subLines.Length == 1);

                        content += subLines[0].Replace("#TABLENAME#", tableName) + "\n";

                        i = lastIndex;
                    }
                    else if(string.Compare(flag, "#REMOVE_THREE_KEY_BEGIN#", true) == 0)
                    {
                        int lastIndex = FindFlag("#REMOVE_THREE_KEY_END#", lines, i + 1);
                        if(lastIndex == -1)
                        {
                            Debug.LogError($"can't find the flag \"#REMOVE_THREE_KEY_END#\"");
                            return false;
                        }
                        // 处理三key
                        if(m_KeyIndices.Length != 3)
                        {
                            i = lastIndex;
                            continue;
                        }

                        string[] subLines = lines.Where((lines, index) => index > i && index < lastIndex).ToArray();
                        Debug.Assert(subLines.Length == 1);

                        content += subLines[0].Replace("#TABLENAME#", tableName) + "\n";

                        i = lastIndex;
                    }
                    else if(string.Compare(flag, "#REMOVE_FOUR_KEY_BEGIN#", true) == 0)
                    {
                        int lastIndex = FindFlag("#REMOVE_FOUR_KEY_END#", lines, i + 1);
                        if(lastIndex == -1)
                        {
                            Debug.LogError($"can't find the flag \"#REMOVE_FOUR_KEY_END#\"");
                            return false;
                        }
                        // 处理四key
                        if(m_KeyIndices.Length != 4)
                        {
                            i = lastIndex;
                            continue;
                        }

                        string[] subLines = lines.Where((lines, index) => index > i && index < lastIndex).ToArray();
                        Debug.Assert(subLines.Length == 1);

                        content += subLines[0].Replace("#TABLENAME#", tableName) + "\n";

                        i = lastIndex;
                    }
                    else if(string.Compare(flag, "#REMOVE_ONE_KEY_IMPLEMENTION_BEGIN#", true) == 0)
                    {
                        int lastIndex = FindFlag("#REMOVE_ONE_KEY_IMPLEMENTION_END#", lines, i + 1);
                        if(lastIndex == -1)
                        {
                            Debug.LogError($"can't find the flag \"#REMOVE_ONE_KEY_IMPLEMENTION_END#\"");
                            return false;
                        }
                        // 处理单key
                        if(m_KeyIndices.Length != 1)
                        {
                            i = lastIndex;
                            continue;
                        }

                        string[] subLines = lines.Where((lines, index) => index > i && index < lastIndex).ToArray();

                        foreach(var line in subLines)
                        {
                            string label = ExtractFlag(line);
                            if(string.IsNullOrEmpty(label))
                            { // 无标签
                                content += line + "\n";
                            }
                            else
                            {
                                string text = line;
                                if (text.IndexOf("#TABLENAME#") != -1)
                                {
                                    text = text.Replace("#TABLENAME#", tableName);
                                }
                                if (text.IndexOf("#KEY1_VALUETYPE#") != -1)
                                {
                                    text = text.Replace("#KEY1_VALUETYPE#", m_ValueTypeLine[0]);       // key默认放第一个
                                }
                                if (text.IndexOf("#KEY1_NAME#") != -1)
                                {
                                    text = text.Replace("#KEY1_NAME#", m_ColumnLine[m_KeyIndices[0]]);
                                }
                                content += text + "\n";
                            }
                        }

                        i = lastIndex;
                    }
                    else if(string.Compare(flag, "#REMOVE_TWO_KEY_IMPLEMENTION_BEGIN#", true) == 0)
                    {
                        int lastIndex = FindFlag("#REMOVE_TWO_KEY_IMPLEMENTION_END#", lines, i + 1);
                        if(lastIndex == -1)
                        {
                            Debug.LogError($"can't find the flag \"#REMOVE_TWO_KEY_IMPLEMENTION_END#\"");
                            return false;
                        }
                        // 处理双key
                        if(m_KeyIndices.Length != 2)
                        {
                            i = lastIndex;
                            continue;
                        }

                        string[] subLines = lines.Where((lines, index) => index > i && index < lastIndex).ToArray();

                        foreach(var line in subLines)
                        {
                            string label = ExtractFlag(line);
                            if(string.IsNullOrEmpty(label))
                            { // 无标签
                                content += line + "\n";
                            }
                            else
                            {
                                string text = line;
                                if (text.IndexOf("#TABLENAME#") != -1)
                                {
                                    text = text.Replace("#TABLENAME#", tableName);
                                }
                                if (text.IndexOf("#KEY1_VALUETYPE#") != -1)
                                {
                                    text = text.Replace("#KEY1_VALUETYPE#", m_ValueTypeLine[0]);       // key默认放第一个
                                }
                                if (text.IndexOf("#KEY2_VALUETYPE#") != -1)
                                {
                                    text = text.Replace("#KEY2_VALUETYPE#", m_ValueTypeLine[1]);
                                }
                                content += text + "\n";
                            }
                        }

                        i = lastIndex;
                    }        
                    else if(string.Compare(flag, "#REMOVE_THREE_KEY_IMPLEMENTION_BEGIN#", true) == 0)
                    {
                        int lastIndex = FindFlag("#REMOVE_THREE_KEY_IMPLEMENTION_END#", lines, i + 1);
                        if(lastIndex == -1)
                        {
                            Debug.LogError($"can't find the flag \"#REMOVE_THREE_KEY_IMPLEMENTION_END#\"");
                            return false;
                        }
                        // 处理三key
                        if(m_KeyIndices.Length != 3)
                        {
                            i = lastIndex;
                            continue;
                        }

                        string[] subLines = lines.Where((lines, index) => index > i && index < lastIndex).ToArray();

                        foreach(var line in subLines)
                        {
                            string label = ExtractFlag(line);
                            if(string.IsNullOrEmpty(label))
                            { // 无标签
                                content += line + "\n";
                            }
                            else
                            {
                                string text = line;
                                if (text.IndexOf("#TABLENAME#") != -1)
                                {
                                    text = text.Replace("#TABLENAME#", tableName);
                                }
                                if (text.IndexOf("#KEY1_VALUETYPE#") != -1)
                                {
                                    text = text.Replace("#KEY1_VALUETYPE#", m_ValueTypeLine[0]);       // key默认放第一个
                                }
                                if (text.IndexOf("#KEY2_VALUETYPE#") != -1)
                                {
                                    text = text.Replace("#KEY2_VALUETYPE#", m_ValueTypeLine[1]);
                                }
                                if (text.IndexOf("#KEY3_VALUETYPE#") != -1)
                                {
                                    text = text.Replace("#KEY3_VALUETYPE#", m_ValueTypeLine[2]);
                                }
                                content += text + "\n";
                            }
                        }

                        i = lastIndex;
                    }
                    else if(string.Compare(flag, "#REMOVE_FOUR_KEY_IMPLEMENTION_BEGIN#", true) == 0)
                    {
                        int lastIndex = FindFlag("#REMOVE_FOUR_KEY_IMPLEMENTION_END#", lines, i + 1);
                        if(lastIndex == -1)
                        {
                            Debug.LogError($"can't find the flag \"#REMOVE_FOUR_KEY_IMPLEMENTION_END#\"");
                            return false;
                        }
                        // 处理四key
                        if(m_KeyIndices.Length != 4)
                        {
                            i = lastIndex;
                            continue;
                        }

                        string[] subLines = lines.Where((lines, index) => index > i && index < lastIndex).ToArray();

                        foreach(var line in subLines)
                        {
                            string label = ExtractFlag(line);
                            if(string.IsNullOrEmpty(label))
                            { // 无标签
                                content += line + "\n";
                            }
                            else
                            {
                                string text = line;
                                if (text.IndexOf("#TABLENAME#") != -1)
                                {
                                    text = text.Replace("#TABLENAME#", tableName);
                                }
                                if (text.IndexOf("#KEY1_VALUETYPE#") != -1)
                                {
                                    text = text.Replace("#KEY1_VALUETYPE#", m_ValueTypeLine[0]);       // key默认放第一个
                                }
                                if (text.IndexOf("#KEY2_VALUETYPE#") != -1)
                                {
                                    text = text.Replace("#KEY2_VALUETYPE#", m_ValueTypeLine[1]);
                                }
                                if (text.IndexOf("#KEY3_VALUETYPE#") != -1)
                                {
                                    text = text.Replace("#KEY3_VALUETYPE#", m_ValueTypeLine[2]);
                                }
                                if (text.IndexOf("#KEY4_VALUETYPE#") != -1)
                                {
                                    text = text.Replace("#KEY4_VALUETYPE#", m_ValueTypeLine[3]);
                                }
                                content += text + "\n";
                            }
                        }

                        i = lastIndex;
                    }            
                    else
                    {
                        string text = lines[i];
                        if(text.IndexOf("#TABLENAME#") != -1)
                        {
                            text = text.Replace("#TABLENAME#", tableName);
                        }
                        if(text.IndexOf("#KEY1_VALUETYPE#") != -1)
                        {
                            text = text.Replace("#KEY1_VALUETYPE#", m_ValueTypeLine[0]);       // key默认放第一个
                        }
                        if(text.IndexOf("#KEY1_NAME#") != -1)
                        {
                            text = text.Replace("#KEY1_NAME#", m_ColumnLine[m_KeyIndices[0]]);
                        }
                        content += text + "\n";
                    }
                }
            }
            return true;
        }

        static private string GetFinalTableName(string tableName)
        {
            return string.Format($"{tableName}{ConfigBuilderSetting.Suffix}");
        }

        static private string GetSQLFunctionNameByValueType(string valueType)
        {
            switch(valueType)
            {
                case "int":
                    return "GetInt32";
                case "string":
                    return "GetString";
                case "float":
                    return "GetFloat";
                case "bool":
                    return "GetBoolean";
            }
            return null;
        }
        
        static private string ExtractFlag(string src)
        {
            int firstIndex = src.IndexOf("#");
            int secondIndex = src.IndexOf("#", Mathf.Max(0, firstIndex + 1));
            if(firstIndex == -1 || secondIndex == -1)
                return null;
            return src.Substring(firstIndex, secondIndex - firstIndex + 1);
        }

        static private int FindFlag(string flag, string[] lines, int startIndex)
        {
            // 考虑嵌套因素，倒序查找
            for(int i = lines.Length - 1; i >= startIndex; --i)
            {
                if(string.Compare(flag, ExtractFlag(lines[i])) == 0)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}