using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.AssetBundleCollector
{
    public class AssetBundleCollectorSetting : ScriptableObject
    {
        public readonly string[] IgnoreFileExtensions = { "", ".so", ".dll", ".cs", ".js", ".boo", ".meta", ".cginc" };

        public List<AssetBundleCollectorConfig> Configs = new List<AssetBundleCollectorConfig>();
    }
}