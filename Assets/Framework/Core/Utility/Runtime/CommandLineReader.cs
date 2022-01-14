using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Framework.Core
{
    /// <summary>
    /// 命令行工具类
    /// 格式同Unity：-COMMAND PARAMETER -COMMAND PARAMETER
    /// "C:\Program Files\Unity\Editor\Unity.exe" -quit -batchmode -projectPath "C:\Users\UserName\Documents\MyProject" -executeMethod MyEditorScript.PerformBuild -UseAPKExpansionFiles
    /// </summary>
    public class CommandLineReader
    {
        static private Dictionary<string, string> s_CommandArgsDict;

        static public string[] GetCommandLineArgs()
        {
            return System.Environment.GetCommandLineArgs();
        }

        static private void ParseCommandLine()
        {
            ParseCommandLine(GetCommandLineArgs());
        }

        static public void ParseCommandLine(string[] args)
        {
            s_CommandArgsDict = new Dictionary<string, string>();

            int cmdIndex = 0;
            while (cmdIndex < args.Length)
            {
                string cmd = null;

                // find the "command" start with "-"
                if (args[cmdIndex].StartsWith("-"))
                {
                    cmd = args[cmdIndex].TrimStart('-');
                    string parameter = string.Empty;

                    // check next "args" whether or not is the "parameter"
                    int nextCmdIndex = System.Math.Min(cmdIndex + 1, args.Length - 1);
                    if (!args[nextCmdIndex].StartsWith("-"))
                    {
                        parameter = args[nextCmdIndex];
                    }
                    AddCommand(cmd, parameter);
                }

                ++cmdIndex;
            }
        }

        static private void AddCommand(string command, string parameter)
        {
            if(s_CommandArgsDict.ContainsKey(command.ToLower()))
            {
                Debug.LogWarning($"AddCommand: {command} has already exist, plz check it");
                return;
            }
            s_CommandArgsDict.Add(command.ToLower(), parameter.ToLower());
        }

        /// <summary>
        /// 是否有cmd，如果有相应的param，则返回para，没有则不变
        /// </summary>
        /// <param name="command"></param>
        /// <param name="para"></param>
        /// <returns>true: 有此命令；false，反之没有</returns>
        static public bool GetCommand(string command, ref string para)
        {
            if(s_CommandArgsDict == null)
            {
                ParseCommandLine();
            }

            string value;
            if(!s_CommandArgsDict.TryGetValue(command.ToLower(), out value))
            {
                return false;
            }

            if(!string.IsNullOrEmpty(value))
            {
                para = value;
            }
            return true;
        }

        static public bool GetCommand(string command, ref bool para)
        {
            if (s_CommandArgsDict == null)
            {
                ParseCommandLine();
            }

            string value;
            if (!s_CommandArgsDict.TryGetValue(command.ToLower(), out value))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(value))
            {
                if(value.ToLower() == "true")
                {
                    para = true;
                }
                else if(value.ToLower() == "false" )
                {
                    para = false;
                }
            }
            return true;
        }

        static public bool GetCommand(string command, ref int para)
        {
            if (s_CommandArgsDict == null)
            {
                ParseCommandLine();
            }

            string value;
            if (!s_CommandArgsDict.TryGetValue(command.ToLower(), out value))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(value))
            {
                para = int.Parse(value, System.Globalization.NumberStyles.Integer);
            }
            return true;
        }
    }
}