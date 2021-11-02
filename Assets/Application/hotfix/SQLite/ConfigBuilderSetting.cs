using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Runtime
{
    public class ConfigBuilderSetting : MonoBehaviour
    {
        static public string    ConfigPath                      = "DesignConfig/";                                          // 配置表路径
        static public string    DesignConfigScriptFilePath      = "Assets/Application/hotfix/Config/DesignConfig.cs";       // 导出的配置表结构脚本目录
        static public string    ConfigManagerScriptFilePath     = "Assets/Application/hotfix/Config/ConfigManager.cs";
        static public string    ConfigManagerTemplateFilePath   = "Assets/Application/hotfix/Config/ConfigManager.cs.txt";
        static public string    DatabaseFilePath                = "Assets/Application/hotfix/SQLite/Editor/config.db";      // 导出的数据库
        static public string    Namespace                       = "Application.Runtime";
    }
}