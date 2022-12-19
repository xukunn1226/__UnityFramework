using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    static public partial class AssetManagerEx
    {
        static public AssetOperationHandle LoadAsset<TObject>(string assetPath) where TObject : UnityEngine.Object
        {
            return s_AssetSystem.LoadAsset<TObject>(assetPath);
        }

        static public AssetOperationHandle LoadAsset(string assetPath, System.Type type)
        {
            return s_AssetSystem.LoadAsset(assetPath, type);
        }

        static public AssetOperationHandle LoadAssetAsync<TObject>(string assetPath) where TObject : UnityEngine.Object
        {
            return s_AssetSystem.LoadAssetAsync<TObject>(assetPath);
        }

        static public AssetOperationHandle LoadAssetAsync(string assetPath, System.Type type)
        {
            return s_AssetSystem.LoadAssetAsync(assetPath, type);
        }
    }
}
