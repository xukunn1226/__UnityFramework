using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections.LowLevel.Unsafe;
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


        //////////////////
        // 1、修改字符串内容会使得HashCode发生变化，而字符串作为字典的Key时是用其HashCode，需要特别注意
        unsafe static public string ToLower_NoAlloc(this string str)
        {
            fixed(char* c = str)
            {
                int length = str.Length;
                for(int i = 0; i < length; ++i)
                {
                    c[i] = char.ToLower(str[i]);
                }
            }
            return str;
        }

        unsafe static public int Split_NoAlloc(this string str, char separator, string[] results)
        {
            if(results.Length == 0)
                throw new System.ArgumentException("results buffer must large than zero");

            if(str.Length == 0)
            {
                results[0] = string.Empty;
                return 1;
            }

            int length = str.Length;
            int bufSize = results.Length;
            int count = 0;
            fixed(char* p = str)
            {
                int startIndex = 0;
                for(int i = 0; i < length; ++i)
                {
                    if (p[i] == separator && count < bufSize)
                    {
                        results[count++] = i - startIndex == 0 ? string.Empty : new string(p, startIndex, i - startIndex);
                        startIndex = i + 1;
                    }
                }

                if(count < bufSize)
                {
                    if (startIndex == length)
                    {
                        results[count++] = string.Empty;
                    }
                    else
                    {
                        results[count++] = new string(p, startIndex, length - startIndex);
                    }
                }
            }
            return count;
        }

        unsafe static public void SetLength(this string str, int length)
        {
            fixed(char* p = str)
            {
                int* ptr = (int*)p;
                ptr[-1] = length;
                p[length] = '\0';
            }
        }

        unsafe static public string Substring(this string str, int startIndex, int length = 0)
        {
            if(startIndex < 0)
                throw new System.ArgumentOutOfRangeException("startIndex < 0");

            if(length <= 0)
                length = str.Length - startIndex;

            if(startIndex + length > str.Length)
                throw new System.ArgumentOutOfRangeException($"{startIndex} + {length} > {str.Length}");

            fixed(char* p = str)
            {
                UnsafeUtility.MemMove(p, p + startIndex, sizeof(char) * length);
            }
            SetLength(str, length);

            return str;
        }
    }
}