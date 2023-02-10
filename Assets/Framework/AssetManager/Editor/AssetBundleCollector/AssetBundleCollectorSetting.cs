using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.AssetEditorWindow
{
    [CreateAssetMenu(fileName = "AssetBundleCollectorSetting", menuName = "AssetManager Settings/Create AssetBundleCollectorSetting")]
    public class AssetBundleCollectorSetting : ScriptableObject
    {
        public List<AssetBundleCollectorConfig> Configs = new List<AssetBundleCollectorConfig>();

        public AssetBundleCollectorConfig GetConfig(string configName)
        {
            int index = Configs.FindIndex(item => { return item.ConfigName == configName; });
            if (index == -1)
                throw new System.Exception($"Can't find the config name: {configName}");
            return Configs[index];
        }

        public void CheckConfigError(string configName)
        {
            AssetBundleCollectorConfig config = GetConfig(configName);
            config.CheckConfigError();
        }

        public string CheckValidation(string configName)
        {
            AssetBundleCollectorConfig config = GetConfig(configName);
            return config.CheckValidation();
        }

        public List<CollectAssetInfo> GetAllCollectAssets(string configName)
        {
            AssetBundleCollectorConfig config = GetConfig(configName);
            return config.GetAllCollectAssets();
        }
    }
}