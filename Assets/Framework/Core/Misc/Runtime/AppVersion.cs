using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Framework.Core
{
    // [CreateAssetMenu(menuName = "Create AppVersion.asset")]
    public class AppVersion : ScriptableObject, IComparable<AppVersion>
    {
        public int  MainVersion;
        public int  MinorVersion;
        public int  Revision;
        public int  BuildNumber;

        protected AppVersion()
        {}

        public void Set(int mainVersion, int minorVersion, int revision)
        {
            MainVersion = mainVersion;
            MinorVersion = minorVersion;
            Revision = revision;
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

        new public string ToString()
        {
            return string.Format($"{MainVersion}.{MinorVersion}.{Revision}");
        }

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
    }
}