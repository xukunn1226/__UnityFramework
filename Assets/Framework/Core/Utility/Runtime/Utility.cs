using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework.Core
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
                    return "android";
                case BuildTarget.iOS:
                    return "ios";
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return "windows";
                case BuildTarget.StandaloneOSX:
                    return "osx";
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
                    return "android";
                case RuntimePlatform.IPhonePlayer:
                    return "ios";
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    return "windows";
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.OSXEditor:
                    return "osx";
                case RuntimePlatform.LinuxPlayer:
                case RuntimePlatform.LinuxEditor:
                    return "linux";
                default:
                    return null;
            }
        }

        /// <summary>
        /// 把全路径转为相对工程的路径
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns>e.g. "Assets/Res/tex.png"</returns>
        static public string GetProjectPath(string fullPath)
        {
            string projectFolder = Application.dataPath.Replace("Assets", "");
            fullPath = fullPath.Replace("\\", "/");
            if(fullPath.StartsWith(projectFolder, System.StringComparison.OrdinalIgnoreCase))
            {
                return fullPath.Substring(projectFolder.Length);
            }
            return fullPath;
        }

#if UNITY_ANDROID
        static public void RestartApp()
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                AndroidJavaObject pm = currentActivity.Call<AndroidJavaObject>("getPackageManager");
                AndroidJavaObject intent = pm.Call<AndroidJavaObject>("getLaunchIntentForPackage", Application.identifier);
                intent.Call<AndroidJavaObject>("setFlags", 0x20000000);//Intent.FLAG_ACTIVITY_SINGLE_TOP

                AndroidJavaClass pendingIntent = new AndroidJavaClass("android.app.PendingIntent");
                AndroidJavaObject contentIntent = pendingIntent.CallStatic<AndroidJavaObject>("getActivity", currentActivity, 0, intent, 0x8000000); //PendingIntent.FLAG_UPDATE_CURRENT = 134217728 [0x8000000]
                AndroidJavaObject alarmManager = currentActivity.Call<AndroidJavaObject>("getSystemService", "alarm");
                AndroidJavaClass system = new AndroidJavaClass("java.lang.System");
                long currentTime = system.CallStatic<long>("currentTimeMillis");
                alarmManager.Call("set", 1, currentTime + 1000, contentIntent); // android.app.AlarmManager.RTC = 1 [0x1]

                Debug.LogError("alarm_manager set time " + currentTime + 1000);
                currentActivity.Call("finish");

                AndroidJavaClass process = new AndroidJavaClass("android.os.Process");
                int pid = process.CallStatic<int>("myPid");
                process.CallStatic("killProcess", pid);
            }
        }
#endif        
    }
}