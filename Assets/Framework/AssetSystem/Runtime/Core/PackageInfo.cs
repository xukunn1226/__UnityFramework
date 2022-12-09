using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    internal class PackageInfo
    {
        public string   BundleName;
        public string   FileHash;
        public string   FileCRC;
        public long     FileSize;
        public bool     IsRawFile;
    }
}