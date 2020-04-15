using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    static internal class AssetBundleManager
    {
        static private string                                               m_RootPath;
        static private AssetBundleManifest                                  m_AssetBundleManifest;
        static private Dictionary<string, AssetBundleRef>                   m_DictAssetBundleRefs       = new Dictionary<string, AssetBundleRef>();        // 已加载完成的assetbundle
        static private Dictionary<string, string[]>                         m_CachedDependencies        = new Dictionary<string, string[]>();
        
        static private bool bInit
        {
            get
            {
                return m_AssetBundleManifest != null;
            }
        }

        static internal void Init(string rootPath, string platformName)
        {
            m_RootPath = string.Format("{0}/{1}/", rootPath, platformName);
            Debug.LogFormat($"AssetBundleManager: init rootPath      {m_RootPath}");

            // init asset bundle manifest
            AssetBundle manifest = AssetBundle.LoadFromFile(GetRootPath(platformName));
            if (manifest != null)
            {
                m_AssetBundleManifest = manifest.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                manifest.Unload(false);
            }

            if (m_AssetBundleManifest == null)
                Debug.LogError("AssetBundleManager init failed becase of asset bundle manifest == null");
        }

        /// <summary>
        /// 释放所有bundle，务必在应用层释放后再调用
        /// 暂时屏蔽，否则退出时会报错
        /// </summary>
        static internal void Uninit()
        {
            //// unload already loaded asset bundle
            //foreach (KeyValuePair<string, AssetBundleRef> kvp in m_DictAssetBundleRefs)
            //{
            //    AssetBundleRef.Release(kvp.Value);
            //}
            //m_DictAssetBundleRefs.Clear();

            //// unload manifest
            //if (m_AssetBundleManifest != null)
            //{
            //    Resources.UnloadAsset(m_AssetBundleManifest);        // 卸载Asset-Object
            //    m_AssetBundleManifest = null;
            //}
        }
        
        static internal string[] GetAllDependencies(string InAssetBundleName)
        {
            if (!bInit)
            {
                Debug.LogError("AssetBundleManager::GetAllDependencies -- Please init AssetBundleManager...");
                return null;
            }

            string[] dependencies;
            if (!m_CachedDependencies.TryGetValue(InAssetBundleName, out dependencies))
            {
                dependencies = m_AssetBundleManifest.GetAllDependencies(InAssetBundleName);
                m_CachedDependencies.Add(InAssetBundleName, dependencies);
            }
            return dependencies;
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
                AssetBundle ab = AssetBundle.LoadFromFile(GetRootPath(InAssetBundleName));
                if(ab != null)
                {
                    ABRef = AssetBundleRef.Get(InAssetBundleName, ab);          // reference count equal to 1
                    m_DictAssetBundleRefs.Add(InAssetBundleName, ABRef);
                }
                else
                {
                    Debug.LogErrorFormat($"AssetBundleManager::Failed to LoadFromFile {GetRootPath(InAssetBundleName)}");
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
        
        static private string GetRootPath(string abName)
        {
            return m_RootPath + abName;
        }    
    }
}