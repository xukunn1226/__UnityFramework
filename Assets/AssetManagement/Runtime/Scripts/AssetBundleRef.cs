using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetManagement.Runtime
{
    /// <summary>
    /// asset bundle reference info
    /// </summary>
    public class AssetBundleRef
    {
        static private ResLoaderPool<AssetBundleRef> m_Pool;

        private int             m_Refs;

        public AssetBundle      AssetBundle         { get; set; }

        public string           AssetBundleName     { get; set; }

        public AssetBundleRef()
        {
            m_Refs = 1;
        }

        internal int IncreaseRefs()
        {
            ++m_Refs;
            return m_Refs;
        }

        internal int DecreaseRefs()
        {
            --m_Refs;
            return m_Refs;
        }

        private void Reset()
        {
            m_Refs = 1;
            AssetBundle = null;
            AssetBundleName = null;
        }

        static public LinkedListNode<AssetBundleRef> Get(string assetBundleName, AssetBundle assetBundle)
        {
            if (m_Pool == null)
            {
                m_Pool = new ResLoaderPool<AssetBundleRef>(AssetManager.preAllocateAssetBundlePoolSize);
            }

            LinkedListNode<AssetBundleRef> abRef = m_Pool.Get();
            abRef.Value.AssetBundleName = assetBundleName;
            abRef.Value.AssetBundle = assetBundle;

            return abRef;
        }

        static public void Release(LinkedListNode<AssetBundleRef> abRef)
        {
            if (m_Pool == null || abRef == null)
                return;

            abRef.Value.AssetBundle.Unload(true);
            abRef.Value.Reset();
            m_Pool.Release(abRef);
        }
    }
}