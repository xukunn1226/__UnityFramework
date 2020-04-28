using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framework.Cache
{
    /// <summary>
    /// 负责管理所有Mono及非Mono对象池
    /// </summary>
    public sealed class PoolManager : MonoBehaviour
    {
        private static PoolManager                      m_kInstance;
        static public PoolManager                       Instance
        {
            get
            {
                if(m_kInstance == null)
                {
                    GameObject go = new GameObject();
                    m_kInstance = go.AddComponent<PoolManager>();
                }
                return m_kInstance;
            }
        }

        private static Dictionary<long, MonoPoolBase>           m_MonoPools         = new Dictionary<long, MonoPoolBase>();         // key: instanceId | poolType.hashcode << 32
                                                                                                                                    // 同一个PrefabAsset支持由多个不同类型Pool

        private static Dictionary<Type, IPool>                  m_Pools             = new Dictionary<Type, IPool>();

        static private Dictionary<string, IAssetLoader>         m_AssetLoaderDict   = new Dictionary<string, IAssetLoader>();       // <key, value>: <assetPath, assetLoader>

        class PrefabPoolInfo
        {
            public MonoPoolBase     m_Pool;
            public int              m_RefCount;
            public IAssetLoader     m_Loader;
        }
        static private Dictionary<string, PrefabPoolInfo>       k_PrefabPoolDict    = new Dictionary<string, PrefabPoolInfo>();       // [assetPath, MonoPoolBase]   instantiated prefab pool
        static private Dictionary<string, int>                  k_PoolRefCountDict  = new Dictionary<string, int>();                // [assetPath, refCount]

#if UNITY_EDITOR
        static public Dictionary<Type, IPool>                   Pools               { get { return m_Pools; } }

        static public Dictionary<long, MonoPoolBase>            MonoPools           { get { return m_MonoPools; } }

        static public Dictionary<string, IAssetLoader>          AssetLoaders        { get { return m_AssetLoaderDict; } }
#endif

        private void Awake()
        {
            if(GameObject.FindObjectsOfType<PoolManager>().Length > 1)
            {
                Debug.LogErrorFormat("PoolManager has already exist [{0}], kill it", name);
                DestroyImmediate(gameObject);
                throw new Exception("PoolManager has already exist...");
            }

            m_kInstance = this;
            transform.parent = null;
            transform.gameObject.name = "[PoolManager]";
            DontDestroyOnLoad(gameObject);

            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        private void OnDestroy()
        {
            Clear();
            m_kInstance = null;
        }

        static public void Clear()
        {
            RemoveAllAssetLoaders();
            RemoveAllMonoPools();
            RemoveAllObjectPools();
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

        static public void RemoveAllObjectPools()
        {
            foreach (var pool in m_Pools)
            {
                pool.Value.Clear();
            }
            m_Pools.Clear();
        }

        static public void RemoveAllAssetLoaders()
        {
            foreach (var item in m_AssetLoaderDict)
            {
                if (item.Value.asset == null)
                    continue;

                MonoPooledObjectBase comp = (item.Value.asset).GetComponent<MonoPooledObjectBase>();
                MonoPoolBase[] pools = GetMonoPools(comp);      // 获取管理此对象的所有对象池
                foreach (var pool in pools)
                {
                    RemoveMonoPool(pool);
                }
                item.Value.Unload();
            }
            m_AssetLoaderDict.Clear();
        }

        static public void TrimAllObjectPools()
        {
            foreach (var pool in m_Pools)
            {
                pool.Value.Trim();
            }

            foreach (var pool in m_MonoPools)
            {
                pool.Value.Trim();
            }
        }

        #region //////////////////////管理Mono对象接口—— GetOrCreate, RemoveMonoPool
        /// <summary>
        /// 获取或创建对象池Prefab
        /// WARNING: 不同于其他GetOrCreatePool接口，每次获取使得对象池引用计数自增
        /// 最佳实践是不同应用环境只使用一次，把MonoPoolBase保存下来反复使用
        /// </summary>
        /// <param name="assetPath">对象池Prefab资源地址</param>
        /// <returns></returns>
        static public MonoPoolBase GetOrCreatePrefabedPool<TLoaderType>(string assetPath) where TLoaderType : IAssetLoader
        {
            PrefabPoolInfo info;
            if (k_PrefabPoolDict.TryGetValue(assetPath, out info))
            {
                info.m_RefCount += 1;
                return info.m_Pool;
            }

            IAssetLoader loader = (IAssetLoader)Activator.CreateInstance(typeof(TLoaderType));
            loader.Load(assetPath);     // 仅加载了prefab资源，未实例化
            if (loader.asset == null)
                throw new ArgumentNullException("loader.asset", $"failed to load asset {assetPath}");

            GameObject poolInst = Object.Instantiate<GameObject>(loader.asset);
            if (poolInst == null)
                throw new System.ArgumentNullException("poolInst", $"failed to inst prefab pool: {assetPath}");

            MonoPoolBase pool = poolInst.GetComponent<MonoPoolBase>();
            if (pool == null)
                throw new System.ArgumentNullException("MonoPoolBase", "not found MonoPoolBase");

            info = new PrefabPoolInfo();
            info.m_Pool = pool;
            info.m_RefCount = 1;
            info.m_Loader = loader;
            k_PrefabPoolDict.Add(assetPath, info);
            return pool;
        }

        static public PrefabObjectPool GetOrCreatePool<TPooledObject, TLoaderType>(string assetPath) where TPooledObject : MonoPooledObjectBase where TLoaderType : IAssetLoader
        {
            return GetOrCreatePool<TPooledObject, PrefabObjectPool, TLoaderType>(assetPath);
        }

        /// <summary>
        /// 根据资源路径创建对象池
        /// </summary>
        /// <typeparam name="TPooledObject">缓存对象脚本</typeparam>
        /// <typeparam name="TPool">缓存池类型</typeparam>
        /// <typeparam name="TLoaderType"></typeparam>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        static public TPool GetOrCreatePool<TPooledObject, TPool, TLoaderType>(string assetPath) where TPooledObject : MonoPooledObjectBase where TPool : MonoPoolBase where TLoaderType : IAssetLoader
        {
            IAssetLoader loader;
            if (!m_AssetLoaderDict.TryGetValue(assetPath, out loader))
            {
                loader = (IAssetLoader)Activator.CreateInstance(typeof(TLoaderType));
                loader.Load(assetPath);
                if(loader.asset == null)
                {
                    Debug.LogError($"Failed to GetOrCreatePool from {assetPath}");
                    return null;
                }
                m_AssetLoaderDict.Add(assetPath, loader);
            }

            return GetOrCreatePool<TPooledObject, TPool>(loader.asset);
        }

        /// <summary>
        /// 创建对象池，默认以PrefabObjectPool管理
        /// </summary>
        /// <typeparam name="TPooledObject">缓存对象脚本</typeparam>
        /// <param name="prefabAsset">缓存对象</param>
        /// <returns></returns>
        static public PrefabObjectPool GetOrCreatePool<TPooledObject>(GameObject prefabAsset) where TPooledObject : MonoPooledObjectBase
        {
            return GetOrCreatePool<TPooledObject, PrefabObjectPool>(prefabAsset);
        }

        /// <summary>
        /// 创建对象池
        /// </summary>
        /// <typeparam name="TPooledObject">缓存对象脚本</typeparam>
        /// <typeparam name="TPool">缓存池类型</typeparam>
        /// <param name="prefabAsset">缓存对象</param>
        /// <returns></returns>
        static public TPool GetOrCreatePool<TPooledObject, TPool>(GameObject prefabAsset) where TPooledObject : MonoPooledObjectBase where TPool : MonoPoolBase
        {
            bool scriptNewAdded = false;
            TPooledObject comp = prefabAsset.GetComponent<TPooledObject>();
            if (comp == null)
            {
                scriptNewAdded = true;
                comp = prefabAsset.AddComponent<TPooledObject>();
            }
            return (TPool)InternalGetOrCreatePool(comp, typeof(TPool), scriptNewAdded);
        }

        /// <summary>
        /// 创建对象池
        /// </summary>
        /// <typeparam name="TPool">缓存池类型</typeparam>
        /// <param name="prefabAsset">缓存对象</param>
        /// <returns></returns>
        static public TPool GetOrCreatePool<TPool>(MonoPooledObjectBase prefabAsset) where TPool : MonoPoolBase
        {
            return (TPool)InternalGetOrCreatePool(prefabAsset, typeof(TPool), false);
        }

        /// <summary>
        /// 创建对象池，默认以PrefabObjectPool管理
        /// </summary>
        /// <param name="prefabAsset">缓存对象</param>
        /// <returns></returns>
        static public PrefabObjectPool GetOrCreatePool(MonoPooledObjectBase prefabAsset)
        {
            return (PrefabObjectPool)InternalGetOrCreatePool(prefabAsset, typeof(PrefabObjectPool), false);
        }

        /// <summary>
        /// 获取或创建对象池
        /// </summary>
        /// <param name="prefabAsset"></param>
        /// <param name="poolType"></param>
        /// <param name="scriptDynamicAdded"></param>
        /// <returns></returns>
        static private MonoPoolBase InternalGetOrCreatePool(MonoPooledObjectBase prefabAsset, Type poolType, bool scriptDynamicAdded)
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
            go.name = "[Pool]" + prefabAsset.gameObject.name;
#endif
            MonoPoolBase newPool = go.AddComponent(poolType) as MonoPoolBase;
            newPool.PrefabAsset = prefabAsset;
            newPool.ScriptDynamicAdded = scriptDynamicAdded;
            newPool.Group = go.transform;
            AddMonoPool(newPool);

            return newPool;
        }

        /// <summary>
        /// 添加对象池
        /// </summary>
        /// <param name="newPool"></param>
        static public void AddMonoPool(MonoPoolBase newPool)
        {
            if (newPool == null || newPool.PrefabAsset == null)
                throw new System.ArgumentNullException("newPool == null || newPool.PrefabAsset == null");

            if (GetMonoPool(newPool.PrefabAsset, newPool.GetType()) != null)
            {
                throw new Exception($"RegisterMonoPool: {newPool} has already exist in mono pools");
                //Debug.LogWarning($"RegisterMonoPool: {newPool} has already exist in mono pools");
                //return;
            }
            m_MonoPools.Add(GetMonoPoolHashCode(newPool.PrefabAsset, newPool.GetType()), newPool);
        }



        /// <summary>
        /// 释放对象池Prefab，与GetOrCreatePrefabedPool对应
        /// </summary>
        /// <param name="assetPath"></param>
        static public void RemoveMonoPrefabedPool(string assetPath)
        {
            PrefabPoolInfo info;
            if (!k_PrefabPoolDict.TryGetValue(assetPath, out info))
            {
                Debug.LogError($"RemoveMonoPool: {assetPath} deduplication!");
                return;
            }

            info.m_RefCount -= 1;
            if (info.m_RefCount == 0)
            {
                info.m_Loader.Unload();
                Object.Destroy(info.m_Pool.gameObject);
                k_PrefabPoolDict.Remove(assetPath);
            }
        }

        /// <summary>
        /// 根据资源路径删除内建对象池PrefabObjectPool
        /// </summary>
        /// <param name="assetPath"></param>
        static public void RemoveMonoPool(string assetPath)
        {
            RemoveMonoPool<PrefabObjectPool>(assetPath);
        }

        /// <summary>
        /// 根据资源路径删除对象池
        /// </summary>
        /// <typeparam name="TPool"></typeparam>
        /// <param name="assetPath"></param>
        static public void RemoveMonoPool<TPool>(string assetPath) where TPool : MonoPoolBase
        {
            IAssetLoader loader;
            if (!m_AssetLoaderDict.TryGetValue(assetPath, out loader))
            {
                return;
            }

            RemoveMonoPool<TPool>(loader.asset);
            m_AssetLoaderDict.Remove(assetPath);
            loader.Unload();
        }


        /// <summary>
        /// 删除内建对象池PrefabObjectPool
        /// </summary>
        /// <param name="prefabAsset"></param>
        static public void RemoveMonoPool(GameObject prefabAsset)
        {
            RemoveMonoPool<PrefabObjectPool>(prefabAsset);
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
                // 为了健壮性可能重复删除Pool，故不抛出异常
                //throw new System.ArgumentNullException($"RemoveMonoPool: prefabAsset == null");
                return;
            }                

            MonoPooledObjectBase comp = prefabAsset.GetComponent<MonoPooledObjectBase>();
            if(comp == null)
            {
                Debug.LogWarning("Can't find any script derived from MonoPooledObjectBase");
                return;
            }

            RemoveMonoPool<TPool>(comp);
        }

        /// <summary>
        /// 删除对象池
        /// </summary>
        /// <param name="pool"></param>
        static public void RemoveMonoPool(MonoPoolBase pool)
        {
            if (pool == null || pool.PrefabAsset == null)
            {
                // 为了健壮性可能重复删除Pool，故不抛出异常
                //throw new System.ArgumentNullException($"RemoveMonoPool: pool == null || pool.PrefabAsset == null");
                return;
            }

            InternalRemoveMonoPool(pool.PrefabAsset, pool.GetType());
        }

        /// <summary>
        /// 删除对象池
        /// </summary>
        /// <typeparam name="TPool">对象池类型</typeparam>
        /// <param name="prefabAsset">缓存对象</param>
        static public void RemoveMonoPool<TPool>(MonoPooledObjectBase prefabAsset) where TPool : MonoPoolBase
        {
            if (prefabAsset == null)
            {
                // 为了健壮性可能重复删除Pool，故不抛出异常
                //throw new System.ArgumentNullException($"RemoveMonoPool: prefabAsset == null  poolType[{typeof(TPool)}]");
                return;
            }

            InternalRemoveMonoPool(prefabAsset, typeof(TPool));
        }

        /// <summary>
        /// 删除对象池
        /// </summary>
        /// <param name="prefabAsset">缓存对象</param>
        /// <param name="poolType">对象池类型</param>
        static private void InternalRemoveMonoPool(MonoPooledObjectBase prefabAsset, Type poolType)
        {
            if (prefabAsset == null || poolType == null)
                throw new System.ArgumentNullException("InternalRemoveMonoPool: prefabAsset == null || poolType == null");

            long key = GetMonoPoolHashCode(prefabAsset, poolType);
            MonoPoolBase pool;
            if (!m_MonoPools.TryGetValue(key, out pool))
            {
                // 为了安全可能重复销毁Pool，暂时屏蔽此Log
                //Debug.LogWarning($"Try to remove not exist pool, [{asset.gameObject.name}]  [{poolType.Name}]");
                return;
            }

            pool.Clear();
            Object.Destroy(pool.gameObject);        // 删除dummy gameobject
            m_MonoPools.Remove(key);
        }

        /// <summary>
        /// 查找所有管理此对象的对象池
        /// </summary>
        /// <param name="prefabAsset"></param>
        /// <returns></returns>
        static public MonoPoolBase[] GetMonoPools(MonoPooledObjectBase prefabAsset)
        {
            if (prefabAsset == null)
                return null;

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

        static private MonoPoolBase GetMonoPool(MonoPooledObjectBase asset, Type poolType)
        {
            if (asset == null || poolType == null)
                throw new System.ArgumentNullException("GetMonoPool: asset == null || poolType == null");

            MonoPoolBase pool;
            m_MonoPools.TryGetValue(GetMonoPoolHashCode(asset, poolType), out pool);
            return pool;
        }
        
        /// <summary>
        /// 根据缓存资源和缓存池类型生成hash code
        /// </summary>
        /// <param name="prefabAsset">缓存对象</param>
        /// <param name="poolType">缓存池类型</param>
        /// <returns></returns>
        static private long GetMonoPoolHashCode(MonoPooledObjectBase prefabAsset, Type poolType)
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
                throw new Exception($"RegisterObjectPool: Type[{type}] has already registered.");
                //Debug.LogError($"RegisterObjectPool: Type[{type}] has already registered.");
                //return;
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
                throw new Exception($"UnregisterObjectPool: Type[{type}] can't find");
                //Debug.LogError($"UnregisterObjectPool: Type[{type}] can't find");
                //return;
            }

            m_Pools[type].Clear();
            m_Pools.Remove(type);
        }
        #endregion

        /// <summary>
        /// 由对象池管理的对象实例化接口
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        new static public Object Instantiate(Object original)
        {
            return Object.Instantiate(original);
        }

        /// <summary>
        /// 由对象池管理的对象销毁接口
        /// </summary>
        /// <param name="obj"></param>
        static public void Destroy(GameObject obj)
        {
            Object.Destroy(obj);
        }
    }
}