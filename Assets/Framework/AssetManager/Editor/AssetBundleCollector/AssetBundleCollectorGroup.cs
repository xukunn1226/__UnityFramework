using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Framework.AssetManagement.AssetEditorWindow
{
    [Serializable]
    public class AssetBundleCollectorGroup
    {
        public string GroupName;
        public string GroupDesc;
        public List<AssetBundleCollector> Collectors = new List<AssetBundleCollector>();
    }
}