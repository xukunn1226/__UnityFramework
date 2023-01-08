using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Framework.AssetManagement.AssetEditorWindow
{
    public class CollectAll : IFilterRule
    {
        public bool IsCollectAsset(FilterRuleData data)
        {
            return true;
        }
    }

    public class CollectScene : IFilterRule
    {
        public bool IsCollectAsset(FilterRuleData data)
        {
            return Path.GetExtension(data.AssetPath) == ".unity";
        }
    }

    public class CollectPrefab : IFilterRule
    {
        public bool IsCollectAsset(FilterRuleData data)
        {
            return Path.GetExtension(data.AssetPath) == ".prefab";
        }
    }

    public class CollectSprite : IFilterRule
    {
        public bool IsCollectAsset(FilterRuleData data)
        {
            var mainAssetType = AssetDatabase.GetMainAssetTypeAtPath(data.AssetPath);
            if (mainAssetType == typeof(Texture2D))
            {
                var texImporter = AssetImporter.GetAtPath(data.AssetPath) as TextureImporter;
                if (texImporter != null && texImporter.textureType == TextureImporterType.Sprite)
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }
        }
    }
}