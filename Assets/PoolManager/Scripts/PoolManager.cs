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

        private MonoPoolBase GetOrCreatePool(MonoPoolBase pool)
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
            GameObject go = new GameObject();
            go.transform.parent = transform;
#if UNITY_EDITOR
            go.name = asset.gameObject.name;
#endif

            PrefabObjectPool pool = go.AddComponent<PrefabObjectPool>();            // 默认使用PrefabObjectPool
            pool.PrefabAsset = asset;

            MonoPoolBase newPool = GetOrCreatePool(pool);
            if ( newPool != pool)
            { // 已存在相同PrefabAsset对应的Pool
                Destroy(go);
            }

            return newPool;
        }
    }
}