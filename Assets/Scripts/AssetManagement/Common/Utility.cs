using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AssetManagement
{
    static public class Utility
    {
        public static string GetPlatformName()
        {
#if UNITY_EDITOR
            return GetPlatformForAssetBundles(EditorUserBuildSettings.activeBuildTarget);
#else
            return GetPlatformForAssetBundles(Application.platform);
#endif
        }

#if UNITY_EDITOR
        private static string GetPlatformForAssetBundles(BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.Android:
                    return "Android";
                case BuildTarget.iOS:
                    return "iOS";
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return "Windows";
                case BuildTarget.StandaloneOSX:
                    return "OSX";
                default:
                    return null;
            }
        }
#endif

        private static string GetPlatformForAssetBundles(RuntimePlatform platform)
        {
            switch (platform)
            {
                case RuntimePlatform.Android:
                    return "Android";
                case RuntimePlatform.IPhonePlayer:
                    return "iOS";
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    return "Windows";
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.OSXEditor:
                    return "OSX";
                case RuntimePlatform.LinuxPlayer:
                case RuntimePlatform.LinuxEditor:
                    return "Linux";
                default:
                    return null;
            }
        }

        /// <summary>
        /// 把全路径转为相对工程的路径
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns>e.g. "Assets/Res/tex.png"</returns>
        static public string GetRelativeProjectPath(string fullPath)
        {
            string projectFolder = Application.dataPath.Replace("Assets", "");
            if(fullPath.StartsWith(projectFolder, System.StringComparison.OrdinalIgnoreCase))
            {
                return fullPath.Substring(projectFolder.Length);
            }
            return fullPath;
        }

#if UNITY_EDITOR
        static public T GetOrCreateEditorConfigObject<T>(string directoryPath) where T : UnityEngine.ScriptableObject
        {
            //name of config data object
            string stringName = "com.myproject." + typeof(T).Name;
            
            //path to Config Object and asset name
            string stringPath = string.Format("{0}{1}{2}{3}", directoryPath.TrimEnd(new char[] { '/' }), "/", typeof(T).Name, ".asset");

            //used to hold config data
            T data = null;

            //if a config data object exists with the same name, return its config data
            if (EditorBuildSettings.TryGetConfigObject<T>(stringName, out data))
                return data;

            //If the asset file already exists, store existing config data
            if (System.IO.File.Exists(stringPath))
                data = AssetDatabase.LoadAssetAtPath<T>(stringPath);

            //if no previous config data exists
            if (data == null)
            {
                //initialise config data object
                data = ScriptableObject.CreateInstance<T>();
                //create new asset from data at specified path
                //asset MUST be saved with the AssetDatabase before adding to EditorBuildSettings
                AssetDatabase.CreateAsset(data, stringPath);
            }

            //add the new or loaded config object to EditorBuildSettings
            EditorBuildSettings.AddConfigObject(stringName, data, true);

            return data;
        }
#endif
    }
}