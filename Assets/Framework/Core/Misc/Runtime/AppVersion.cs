using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework.Core
{
    // [CreateAssetMenu(menuName = "Create AppVersion.asset")]
    /// <summary>
    /// Format: X.Y.Z.W
    /// X: 重大版本时变动，两位数
    /// Y: 商店需要版本号增加，每次提交+1，两位数
    /// Z: 同Y，检测到SDK或C#改动则+1，两位数
    /// W: 热更新版本号（不改变app binary基础上的补丁包序号），每次+1，大于两位数。若X.Y.Z有变动则W清零
    /// </summary>
    public class AppVersion : ScriptableObject, IComparable<AppVersion>
    {
        public int  MainVersion;        // X
        public int  MinorVersion;       // Y
        public int  Revision;           // Z
        public int  BuildNumber;        // 每次打包自增，对应android.bundleVersionCode & ios.buildNumber

        [NonSerialized]
        public int  HotfixNumber;       // W——补丁包序列号

        static public AppVersion Load()
        {
            return Resources.Load<AppVersion>("AppVersion");
        }

        static public void Unload(AppVersion version)
        {
            Resources.UnloadAsset(version);
        }

        public void Set(string version)
        {
            string[] str = version.Split(new char[] { '.' });
            if (str.Length != 3 && str.Length != 4)
                throw new ArgumentException($"version format is not standard.   {version}");

            if (str.Length > 0)
            {
                try
                {
                    int n = int.Parse(str[0], System.Globalization.NumberStyles.Integer);
                    MainVersion = n;
                }
                catch (Exception e)
                {
                    Debug.LogError($"AppVersion construct: {e.Message}");
                }
            }

            if (str.Length > 1)
            {
                try
                {
                    int n = int.Parse(str[1], System.Globalization.NumberStyles.Integer);
                    MinorVersion = n;
                }
                catch (Exception e)
                {
                    Debug.LogError($"AppVersion construct: {e.Message}");
                }
            }

            if (str.Length > 2)
            {
                try
                {
                    int n = int.Parse(str[2], System.Globalization.NumberStyles.Integer);
                    Revision = n;
                }
                catch (Exception e)
                {
                    Debug.LogError($"AppVersion construct: {e.Message}");
                }
            }

            if (str.Length > 3)
            {
                try
                {
                    int n = int.Parse(str[3], System.Globalization.NumberStyles.Integer);
                    HotfixNumber = n;
                }
                catch (Exception e)
                {
                    Debug.LogError($"AppVersion construct: {e.Message}");
                }
            }
        }

#if UNITY_EDITOR
        static public AppVersion EditorLoad()
        {
            return AssetDatabase.LoadAssetAtPath<AppVersion>("Assets/Resources/AppVersion.asset");
        }

        public void Set(int mainVersion, int minorVersion, int revision)
        {
            MainVersion     = mainVersion;
            MinorVersion    = minorVersion;
            Revision        = revision;
            HotfixNumber    = 0;
            ++BuildNumber;
        }

        public void Grow()
        {
            ++Revision;
            if(Revision > 99)
            {
                ++MinorVersion;
                Revision = 0;
            }
            if(MinorVersion > 99)
            {
                ++MainVersion;
                MinorVersion = 0;
            }

            ++BuildNumber;
        }
#endif

        new public string ToString()
        {
            if(HotfixNumber == 0)
                return string.Format($"{MainVersion}.{MinorVersion}.{Revision}");
            else
                return string.Format($"{MainVersion}.{MinorVersion}.{Revision}.{HotfixNumber}");
        }

        /// <summary>
        /// 仅使用前三位X.Y.Z做比较，通常用于兼容包逻辑
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(AppVersion other)
        {
            if(this.MainVersion < other.MainVersion)
                return -1;
            if(this.MainVersion > other.MainVersion)
                return 1;

            if(this.MinorVersion < other.MinorVersion)
                return -1;
            if(this.MinorVersion > other.MinorVersion)
                return 1;

            if(this.Revision < other.Revision)
                return -1;
            if(this.Revision > other.Revision)
                return 1;

            return 0;
        }

        public int CompareTo(string other)
        {
            AppVersion version = ScriptableObject.CreateInstance<AppVersion>();
            version.Set(other);
            return this.CompareTo(version);
        }

        static public int CompareTo(AppVersion lhs, AppVersion rhs)
        {
            return lhs.CompareTo(rhs);
        }
    }
}