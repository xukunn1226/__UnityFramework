using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Build.Pipeline;
using Framework.Core;

namespace Framework.AssetManagement.Runtime
{
    static internal class AssetBundleManager
    {
        static private string                               m_StreamingAssetPath;
        static private string                               m_PersistentDataPath;
        static private Dictionary<string, AssetBundleRef>   m_DictAssetBundleRefs       = new Dictionary<string, AssetBundleRef>();        // 已加载完成的assetbundle
        static private bool                                 m_Init;
        
        static private bool bInit
        {
            get
            {
                return m_Init;
            }
        }

        static internal void Init()
        {
            m_Init = true;
            m_StreamingAssetPath = string.Format("{0}/{1}/", UnityEngine.Application.streamingAssetsPath, Utility.GetPlatformName());
            m_PersistentDataPath = string.Format("{0}/{1}/", UnityEngine.Application.persistentDataPath, Utility.GetPlatformName());
            Debug.LogFormat($"AssetBundleManager: init rootPath      {m_StreamingAssetPath}");
        }

        /// <summary>
        /// 释放所有bundle，务必在应用层释放后再调用
        /// 暂时屏蔽，否则退出时会报错
        /// </summary>
        static internal void Uninit()
        {
            // unload already loaded asset bundle
            foreach (KeyValuePair<string, AssetBundleRef> kvp in m_DictAssetBundleRefs)
            {
                AssetBundleRef.Release(kvp.Value);
            }
            m_DictAssetBundleRefs.Clear();
        }
        
        static internal string[] GetAllDependencies(string InAssetBundleName)
        {
            if (!bInit)
            {
                Debug.LogError("AssetBundleManager::GetAllDependencies -- Please init AssetBundleManager...");
                return null;
            }
            return AssetManager.GetBundleDetail(InAssetBundleName).dependencies;
        }

        static internal AssetBundleRef LoadAssetBundleFromFile(string InAssetBundleName)
        {
            if (!bInit)
            {
                Debug.LogError("AssetBundleManager::LoadAssetBundle -- Please init AssetBundleManager...");
                return null;
            }

            AssetBundleRef ABRef;
            m_DictAssetBundleRefs.TryGetValue(InAssetBundleName, out ABRef);

            if (ABRef != null)
            {
                ABRef.IncreaseRefs();
            }
            else
            {
                CustomManifest.BundleDetail bundleDetail = AssetManager.GetBundleDetail(InAssetBundleName);
                AssetBundle ab = AssetBundle.LoadFromFile(GetRootPath(bundleDetail));
                if(ab != null)
                {
                    ABRef = AssetBundleRef.Get(InAssetBundleName, ab);          // reference count equal to 1
                    m_DictAssetBundleRefs.Add(InAssetBundleName, ABRef);
                }
                else
                {
                    Debug.LogErrorFormat($"AssetBundleManager::Failed to LoadFromFile {GetRootPath(bundleDetail)}");
                }
            }

            return ABRef;
        }

        static internal void Unload(string InAssetBundleName)
        {
            if (!bInit)
            {
                Debug.LogError("AssetBundleManager::Unload -- Please init AssetBundleManager...");
                return;
            }

            AssetBundleRef ABRef;
            if (m_DictAssetBundleRefs.TryGetValue(InAssetBundleName, out ABRef))
            {
                int Refs = ABRef.DecreaseRefs();
                if (Refs <= 0)
                {
                    AssetBundleRef.Release(ABRef);
                    m_DictAssetBundleRefs.Remove(InAssetBundleName);
                }
            }
            else
            {
                Debug.LogErrorFormat("AssetBundleManager::Unload -- can't find asset bundle [{0}]", InAssetBundleName);
            }
        }
        
        static private string GetRootPath(CustomManifest.BundleDetail bundleDetail)
        {
            return bundleDetail.isStreamingAsset ? m_StreamingAssetPath + bundleDetail.bundleName : m_PersistentDataPath + bundleDetail.bundleName;
        }    
    }
}