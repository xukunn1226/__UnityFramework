using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using Sirenix.OdinInspector;

namespace Framework.AssetManagement.AssetChecker
{
    public class AssetProcessor_AssetDependecy : IAssetProcessor
    {
        public string DoProcess(string assetPath)
        {
            WirteLogToDebug.WriteLineToLogFile($" ");
            WirteLogToDebug.WriteLineToResFile($" ");
            var dependcies = AssetDatabase.GetDependencies(assetPath, true);
            WirteLogToDebug.WriteLineToLogFile($"---<{assetPath}>, <Len:{dependcies.Length}>---");
            WirteLogToDebug.WriteLineToResFile($"---<{assetPath}>, <Len:{dependcies.Length}>---");
            string error = "";
            string fileName = Path.GetFileName(assetPath);
            foreach (var dependcy in dependcies)
            {
                var tempDependcy = dependcy.Replace('\\', '/').ToLower();
                if (tempDependcy.Contains("assets/res"))
                {
                    WirteLogToDebug.WriteLineToLogFile($"{dependcy}");

                    if (tempDependcy.Contains("/temp/"))
                    {
                        WirteLogToDebug.WriteLineToResFile($"error File: {dependcy}");
                        error += $"{fileName}引用了该有问题的资源{dependcy} \n";
                    }
                }
            }
            if(string.IsNullOrEmpty(error))
                return null;
            return error;
        }
    }

    public static class WirteLogToDebug
    {
        private static string folder = UnityEngine.Application.dataPath + "/../Temp/";
        static string logPath = folder + "AssetCheckerLog.txt";
        static string resPath = folder + "AssetCheckerRes.txt";
        public static void LogResFileReset()
        {
            System.IO.StreamWriter logStream = new System.IO.StreamWriter(logPath, false);
            logStream.Close();
            System.IO.StreamWriter resStream = new System.IO.StreamWriter(resPath, false);
            resStream.Close();
        }
        public static void WriteLineToLogFile(string line)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            System.IO.StreamWriter file = new System.IO.StreamWriter(logPath, true);
            file.WriteLine(line);
            file.Close();
        }
        public static void WriteLineToResFile(string line)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            System.IO.StreamWriter file = new System.IO.StreamWriter(resPath, true);
            file.WriteLine(line);
            file.Close();
        }
        
    }

}