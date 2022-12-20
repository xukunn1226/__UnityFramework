using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Diagnostics;

namespace Framework.AssetManagement.Runtime
{
    static public partial class AssetManagerEx
    {
        [Conditional("DEBUG")]
        static private void DebugCheckInitialize()
        {
            if (s_OperationStatus == EOperationStatus.None)
                throw new System.Exception("AssetManager initialize not completed !");
            else if (s_OperationStatus == EOperationStatus.Failed)
                throw new System.Exception($"AssetManager initialize failed ! {s_Error}");
        }

        static public AssetOperationHandle LoadAsset<TObject>(string assetPath) where TObject : UnityEngine.Object
        {
            DebugCheckInitialize();
            return s_AssetSystem.LoadAsset<TObject>(assetPath);
        }

        static public AssetOperationHandle LoadAsset(string assetPath, System.Type type)
        {
            DebugCheckInitialize();
            return s_AssetSystem.LoadAsset(assetPath, type);
        }

        static public AssetOperationHandle LoadAssetAsync<TObject>(string assetPath) where TObject : UnityEngine.Object
        {
            DebugCheckInitialize();
            return s_AssetSystem.LoadAssetAsync<TObject>(assetPath);
        }

        static public AssetOperationHandle LoadAssetAsync(string assetPath, System.Type type)
        {
            DebugCheckInitialize();
            return s_AssetSystem.LoadAssetAsync(assetPath, type);
        }

        /// <summary>
		/// 异步加载场景
		/// </summary>
		/// <param name="location">场景的定位地址</param>
		/// <param name="sceneMode">场景加载模式</param>
		/// <param name="activateOnLoad">加载完毕时是否主动激活</param>
		/// <param name="priority">优先级</param>
		public static SceneOperationHandle LoadSceneAsync(string location, LoadSceneMode sceneMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
        {
            //DebugCheckDefaultPackageValid();
            //return _defaultPackage.LoadSceneAsync(location, sceneMode, activateOnLoad, priority);
            return null;
        }

        static public void UnloadUnusedAssets()
        {
            DebugCheckInitialize();
            s_AssetSystem.UnloadUnusedAssets();
        }

        static public void ForceUnloadAllAssets()
        {
            DebugCheckInitialize();
            s_AssetSystem.ForceUnloadAllAssets();
        }
    }
}
