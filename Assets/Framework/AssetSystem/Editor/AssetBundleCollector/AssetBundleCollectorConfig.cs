using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Framework.AssetManagement.AssetBundleCollector
{
    [Serializable]
    public class AssetBundleCollectorConfig
    {
        public string ConfigName;
        public string ConfigDesc;
        public List<AssetBundleCollectorGroup> Groups = new List<AssetBundleCollectorGroup>();
    }
}