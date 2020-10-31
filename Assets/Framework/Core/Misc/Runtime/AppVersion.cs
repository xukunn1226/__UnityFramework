using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Core
{
    public class AppVersion : ScriptableObject
    {
        public int MainVersion;
        public int SecondaryVersion;
        public int MinorVersion;
        public int BuildNumber;
    }
}