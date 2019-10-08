using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public class PoolManager : MonoBehaviour
    {
        private Dictionary<int, MonoPoolBase>           m_dictInstanceIdToPool = new Dictionary<int, MonoPoolBase>();

        private void Awake()
        {
            transform.parent = null;
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            InitMonoPools();
        }

        private void InitMonoPools()
        {
            MonoPoolBase[] pools = GetComponents<MonoPoolBase>();
            foreach(MonoPoolBase pool in pools)
            {
                MonoPoolBase newPool = GetOrCreatePool(pool);
                if (newPool == null)
                    continue;

                if(newPool != pool)
                {
                    Destroy(pool);
                }
                else
                {
                    GameObject go = new GameObject();
                    go.transform.parent = transform;
#if UNITY_EDITOR
                    go.name = newPool.PrefabAsset.gameObject.name;
#endif

                }
            }
        }

        public MonoPoolBase GetOrCreatePool(MonoPoolBase pool)
        {
            if (pool == null)
                return null;

            if(pool.PrefabAsset == null)
            {
                Debug.LogWarning("pool.PrefabAsset == null, return null");
                return null;
            }

            MonoPoolBase value;
            if(m_dictInstanceIdToPool.TryGetValue(pool.PrefabAsset.gameObject.GetInstanceID(), out value))
            {
                Debug.LogWarning($"PrefabAsset[{pool.PrefabAsset}] has already exist, plz check it");
                return value;
            }

            pool.transform.parent = transform;
            m_dictInstanceIdToPool.Add(pool.PrefabAsset.gameObject.GetInstanceID(), pool);

            return pool;
        }
                
        public MonoPoolBase GetOrCreatePool(MonoPooledObjectBase asset)
        {
            MonoPoolBase pool = GetPool(asset);
            if (pool != null)
                return pool;

            // create new
            GameObject go = new GameObject();
            go.transform.parent = transform;
#if UNITY_EDITOR
            go.name = "[Pool]" + asset.gameObject.name;
#endif

            pool = go.AddComponent<PrefabObjectPool>();            // 默认使用PrefabObjectPool
            pool.PrefabAsset = asset;
            pool.Pivot = go.transform;

            pool = GetOrCreatePool(pool);
            return pool;
        }

        private MonoPoolBase GetPool(MonoPooledObjectBase asset)
        {
            if (asset == null)
                return null;

            MonoPoolBase pool;
            m_dictInstanceIdToPool.TryGetValue(asset.gameObject.GetInstanceID(), out pool);
            return pool;
        }
    }
}