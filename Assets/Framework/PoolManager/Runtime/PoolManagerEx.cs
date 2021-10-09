using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;
using System;
using Object = UnityEngine.Object;

namespace Framework.Cache
{
    public sealed class PoolManagerEx : SingletonMono<PoolManagerEx>
    {
        private static Dictionary<Type, IPool>                  m_Pools             = new Dictionary<Type, IPool>();            // 非Mono对象池

        private static Dictionary<long, MonoPoolBase>           m_MonoPools         = new Dictionary<long, MonoPoolBase>();     // Mono对象池（同一个PrefabAsset支持由多个不同类型的Pool）
                                                                                                                                // key: instanceId | poolType.hashcode << 32

#if UNITY_EDITOR
        static public Dictionary<Type, IPool>                   Pools               { get { return m_Pools; } }
        static public Dictionary<long, MonoPoolBase>            MonoPools           { get { return m_MonoPools; } }
#endif

        protected override void OnDestroy()
        {
            Destroy();
            base.OnDestroy();
        }

        static public void Destroy()
        {
            // RemoveAllAssetLoaders();
            // RemoveAllPrefabedPools();
            // RemoveAllLRUPools();

            // // 基础数据最后清除
            // RemoveAllMonoPools();
            RemoveAllObjectPools();
        }

        #region //////////////////////管理非Mono对象接口
        /// <summary>
        /// 非Mono对象池注册接口
        /// </summary>
        /// <param name="type"></param>
        /// <param name="pool"></param>
        static public void AddObjectPool(Type type, IPool pool)
        {
            if(m_Pools.ContainsKey(type))
            {
                throw new Exception($"AddObjectPool: Type[{type}] has already registered.");
            }

            m_Pools.Add(type, pool);
        }

        /// <summary>
        /// 非Mono对象池注销接口
        /// </summary>
        /// <param name="type"></param>
        static public void RemoveObjectPool(Type type)
        {
            if(!m_Pools.ContainsKey(type))
            {
                throw new Exception($"RemoveObjectPool: Type[{type}] can't find");
            }

            m_Pools[type].Clear();
            m_Pools.Remove(type);
        }

        static public void RemoveAllObjectPools()
        {
            foreach (var pool in m_Pools)
            {
                pool.Value.Clear();
            }
            m_Pools.Clear();
        }
        #endregion






        #region ////////////////////// 管理Mono对象接口—— GetOrCreate, RemoveMonoPool
        static public PrefabObjectPoolEx GetOrCreatePool(GameObject prefabAsset)
        {
            return GetOrCreatePool<MonoPooledObject, PrefabObjectPoolEx>(prefabAsset);
        }

        /// <summary>
        /// 创建对象池，默认以PrefabObjectPool管理
        /// </summary>
        /// <typeparam name="TPooledObject">缓存对象脚本</typeparam>
        /// <param name="prefabAsset">缓存对象</param>
        /// <returns></returns>
        static public PrefabObjectPoolEx GetOrCreatePool<TPooledObject>(GameObject prefabAsset) where TPooledObject : MonoPooledObject
        {
            return GetOrCreatePool<TPooledObject, PrefabObjectPoolEx>(prefabAsset);
        }
        
        /// <summary>
        /// 创建对象池
        /// </summary>
        /// <typeparam name="TPooledObject">缓存对象脚本</typeparam>
        /// <typeparam name="TPool">缓存池类型</typeparam>
        /// <param name="prefabAsset">缓存对象</param>
        /// <returns></returns>
        static public TPool GetOrCreatePool<TPooledObject, TPool>(GameObject prefabAsset) where TPooledObject : MonoPooledObject where TPool : MonoPoolBase
        {
            TPooledObject comp = prefabAsset.GetComponent<TPooledObject>();
            if (comp == null)
            {
                comp = prefabAsset.AddComponent<TPooledObject>();
            }
            return (TPool)InternalGetOrCreatePool(comp, typeof(TPool));
        }

        /// <summary>
        /// 创建对象池
        /// </summary>
        /// <typeparam name="TPool">缓存池类型</typeparam>
        /// <param name="prefabAsset">缓存对象</param>
        /// <returns></returns>
        static public TPool GetOrCreatePool<TPool>(MonoPooledObject prefabAsset) where TPool : MonoPoolBase
        {
            return (TPool)InternalGetOrCreatePool(prefabAsset, typeof(TPool));
        }

        /// <summary>
        /// 创建对象池，默认以PrefabObjectPool管理
        /// </summary>
        /// <param name="prefabAsset">缓存对象</param>
        /// <returns></returns>
        static public PrefabObjectPoolEx GetOrCreatePool(MonoPooledObject prefabAsset)
        {
            return (PrefabObjectPoolEx)InternalGetOrCreatePool(prefabAsset, typeof(PrefabObjectPoolEx));
        }

        /// <summary>
        /// 获取或创建对象池
        /// </summary>
        /// <param name="prefabAsset"></param>
        /// <param name="poolType"></param>
        /// <returns></returns>
        static private MonoPoolBase InternalGetOrCreatePool(MonoPooledObject prefabAsset, Type poolType)
        {
            if (prefabAsset == null || poolType == null)
                throw new ArgumentNullException("asset == null || poolType == null");

            MonoPoolBase pool = GetMonoPool(prefabAsset, poolType);
            if(pool != null)
            {
                return pool;
            }

            GameObject go = new GameObject();
            go.transform.parent = Instance?.transform;
#if UNITY_EDITOR
            go.name = string.Format($"[Pool] {prefabAsset.gameObject.name}_{poolType.Name}");
#endif
            MonoPoolBase newPool = go.AddComponent(poolType) as MonoPoolBase;
            newPool.PrefabAsset = prefabAsset;
            newPool.Group = go.transform;
            newPool.Init();
            AddMonoPool(newPool);

            return newPool;
        }





        static public TPool GetOrCreateEmptyPool<TPool>() where TPool : MonoPoolBase
        {
            return (TPool)InternalGetOrCreateEmptyPool(typeof(TPool));
        }

        static public PrefabObjectPoolEx GetOrCreateEmptyPool()
        {
            return (PrefabObjectPoolEx)InternalGetOrCreateEmptyPool(typeof(PrefabObjectPoolEx));
        }

        static private MonoPoolBase InternalGetOrCreateEmptyPool(Type poolType)
        {
            if(poolType == null)
                throw new ArgumentNullException("poolType == null");
            
            GameObject go = new GameObject();
            go.transform.parent = Instance?.transform;
#if UNITY_EDITOR
            go.name = string.Format($"[Pool] [Empty]_{poolType.Name}");
#endif
            MonoPoolBase newPool = go.AddComponent(poolType) as MonoPoolBase;
            return newPool;
        }

        static public MonoPoolBase RegisterMonoPool(MonoPoolBase newPool)
        {
            if(newPool == null || newPool.PrefabAsset == null)
                throw new System.ArgumentNullException("RegisterMonoPool:newPool == null || newPool.PrefabAsset == null");

            MonoPoolBase pool = GetMonoPool(newPool.PrefabAsset, newPool.GetType());
            if(pool != null)
            {
                Debug.LogWarning($"RegisterMonoPool: {newPool} has already exist in mono pools");
                return pool;
            }
            newPool.Init();
#if UNITY_EDITOR
            newPool.gameObject.name = string.Format($"[Pool] {newPool.PrefabAsset.gameObject.name}_{newPool.name}");
#endif            
            AddMonoPool(newPool);
            return newPool;
        }

        /// <summary>
        /// 添加对象池
        /// NOTE: 为了兼容把对象池制作为Prefab，故开放此接口，见PrefabObjectPool
        /// </summary>
        /// <param name="newPool"></param>
        static public void AddMonoPool(MonoPoolBase newPool)
        {
            if (newPool == null || newPool.PrefabAsset == null)
                throw new System.ArgumentNullException("newPool == null || newPool.PrefabAsset == null");

            if (GetMonoPool(newPool.PrefabAsset, newPool.GetType()) != null)
            {
                Debug.LogError($"AddMonoPool: {newPool} has already exist in mono pools");
                return;
            }
            m_MonoPools.Add(GetMonoPoolHashCode(newPool.PrefabAsset, newPool.GetType()), newPool);

            // 对象池统一挂载到PoolManager
            if (PoolManagerEx.Instance.gameObject != newPool.transform.root.gameObject)
            {
                newPool.transform.parent = PoolManagerEx.Instance.transform;
            }
        }

        /// <summary>
        /// 删除内建对象池PrefabObjectPool
        /// </summary>
        /// <param name="prefabAsset"></param>
        static public void RemoveMonoPool(GameObject prefabAsset)
        {
            RemoveMonoPool<PrefabObjectPoolEx>(prefabAsset);
        }

        /// <summary>
        /// 删除对象池
        /// </summary>
        /// <typeparam name="TPool"></typeparam>
        /// <param name="prefabAsset"></param>
        static public void RemoveMonoPool<TPool>(GameObject prefabAsset) where TPool : MonoPoolBase
        {
            if (prefabAsset == null)
            {
                Debug.LogError("RemoveMonoPool: prefabAsset == null");
                return;
            }

            MonoPooledObject comp = prefabAsset.GetComponent<MonoPooledObject>();
            if (comp == null)
            {
                Debug.LogError("Can't find any script derived from MonoPooledObjectBase");
                return;
            }

            RemoveMonoPool<TPool>(comp);
        }

        /// <summary>
        /// 删除对象池
        /// </summary>
        /// <typeparam name="TPool">对象池类型</typeparam>
        /// <param name="prefabAsset">缓存对象</param>
        static public void RemoveMonoPool<TPool>(MonoPooledObject prefabAsset) where TPool : MonoPoolBase
        {
            if (prefabAsset == null)
            {
                Debug.LogError($"RemoveMonoPool: prefabAsset == null  poolType[{typeof(TPool)}]");
                return;
            }

            InternalRemoveMonoPool(prefabAsset, typeof(TPool));
        }

        /// <summary>
        /// 删除对象池
        /// </summary>
        /// <param name="pool"></param>
        static public void RemoveMonoPool(MonoPoolBase pool)
        {
            if (pool == null || pool.PrefabAsset == null)
            {
                Debug.LogError("RemoveMonoPool: pool == null || pool.PrefabAsset == null");
                return;
            }

            InternalRemoveMonoPool(pool.PrefabAsset, pool.GetType());
        }

        /// <summary>
        /// 删除对象池
        /// </summary>
        /// <param name="prefabAsset">缓存对象</param>
        /// <param name="poolType">对象池类型</param>
        static private void InternalRemoveMonoPool(MonoPooledObject prefabAsset, Type poolType)
        {
            if (prefabAsset == null || poolType == null)
                throw new System.ArgumentNullException("InternalRemoveMonoPool: prefabAsset == null || poolType == null");

            long key = GetMonoPoolHashCode(prefabAsset, poolType);
            MonoPoolBase pool;
            if (!m_MonoPools.TryGetValue(key, out pool))
            {
                Debug.LogError($"Try to remove not exist pool, [{prefabAsset.gameObject.name}]  [{poolType.Name}]");
                return;
            }

            pool.Clear();
            Object.Destroy(pool.gameObject);        // 删除pool gameobject
            m_MonoPools.Remove(key);
        }

        static public void RemoveAllMonoPools()
        {
            Dictionary<long, MonoPoolBase>.Enumerator e = m_MonoPools.GetEnumerator();
            while (e.MoveNext())
            {
                e.Current.Value.Clear();
                Object.Destroy(e.Current.Value.gameObject);
            }
            e.Dispose();

            m_MonoPools.Clear();
        }

        /// <summary>
        /// 查找所有管理此对象的对象池
        /// </summary>
        /// <param name="prefabAsset"></param>
        /// <returns></returns>
        static public MonoPoolBase[] GetMonoPools(MonoPooledObject prefabAsset)
        {
            if (prefabAsset == null)
                throw new ArgumentNullException("GetMonoPools::prefabAsset == null");

            int instanceId = prefabAsset.gameObject.GetInstanceID();
            int count = 0;

            // 统计缓存了此对象(prefabAsset)的对象池数量
            Dictionary<long, MonoPoolBase>.Enumerator e = m_MonoPools.GetEnumerator();
            while(e.MoveNext())
            {
                int key = (int)(e.Current.Key & 0xFFFFFFFF);
                if (key == instanceId)
                {
                    ++count;
                }
            }
            e.Dispose();
            
            // collect pools
            MonoPoolBase[] pools = new MonoPoolBase[count];
            int index = 0;
            Dictionary<long, MonoPoolBase>.Enumerator e1 = m_MonoPools.GetEnumerator();
            while (e1.MoveNext())
            {
                int key = (int)(e1.Current.Key & 0xFFFFFFFF);
                if(key == instanceId)
                {
                    pools[index++] = e1.Current.Value;
                }
            }

            return pools;
        }

        // 根据缓存的资源对象及对象池类型获取对象池实例
        static private MonoPoolBase GetMonoPool(MonoPooledObject prefabAsset, Type poolType)
        {
            if (prefabAsset == null || poolType == null)
                throw new System.ArgumentNullException("GetMonoPool: asset == null || poolType == null");

            MonoPoolBase pool;
            m_MonoPools.TryGetValue(GetMonoPoolHashCode(prefabAsset, poolType), out pool);
            return pool;
        }
        
        /// <summary>
        /// 根据缓存资源和缓存池类型生成hash code
        /// </summary>
        /// <param name="prefabAsset">缓存对象</param>
        /// <param name="poolType">缓存池类型</param>
        /// <returns></returns>
        static private long GetMonoPoolHashCode(MonoPooledObject prefabAsset, Type poolType)
        {
            if (prefabAsset == null || poolType == null)
                throw new System.ArgumentNullException("GenerateKey: asset == null || poolType == null");

            long key1 = prefabAsset.gameObject.GetInstanceID();
            long key2 = (long)poolType.GetHashCode() << 32;
            long key = key1 | key2;

            //Debug.Log($"InstanceID: {key1}  Type: {poolType.GetHashCode()}  Type<<32: {key2}    key: {key}");

            return key;
        }
        #endregion











        /// <summary>
        /// 对象实例化接口，避免直接使用UnityEngine.Object.Instantiate
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        new static public Object Instantiate(Object original)
        {
            return Object.Instantiate(original);
        }

        /// <summary>
        /// 对象销毁接口，避免直接使用UnityEngine.Object.Destroy
        /// </summary>
        /// <param name="obj"></param>
        static public void Destroy(GameObject obj)
        {
            Object.Destroy(obj);
        }
    }
}