using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Core
{
    public class MyManifest
    {
        public string m_Var1;
        public Dictionary<string, string> VersionHistory;         // [version, diffcollection json's hash]
    }
}