using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.AssetBundleCollector
{
    public interface IPackRule
    {
        string GetBundleName(PackRuleData data);
    }

    public struct PackRuleData
    {
        public string AssetPath;
        public string CollectPath;
        public string GroupName;

        public PackRuleData(string assetPath)
        {
            AssetPath = assetPath;
            CollectPath = string.Empty;
            GroupName = string.Empty;
        }

        public PackRuleData(string assetPath, string collectPath, string groupName)
        {
            AssetPath = assetPath;
            CollectPath = collectPath;
            GroupName = groupName;
        }
    }
}