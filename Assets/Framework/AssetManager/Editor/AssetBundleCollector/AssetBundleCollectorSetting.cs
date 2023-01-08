using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.AssetEditorWindow
{
    public class AssetBundleCollectorSetting : ScriptableObject
    {
        static public readonly string[] IgnoreFileExtensions = { "", ".so", ".dll", ".cs", ".js", ".boo", ".meta", ".cginc" };

        static public readonly string[] IgnoreDirectoryName = { "Temp", "Editor", "RawData", "Resources", "Examples" };

        public List<AssetBundleCollectorConfig> Configs = new List<AssetBundleCollectorConfig>();
    }
}