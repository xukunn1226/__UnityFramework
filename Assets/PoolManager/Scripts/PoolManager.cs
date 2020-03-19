using System;
using System.Collections.Generic;
using UnityEngine;

namespace CacheMech
{
    /// <summary>
    /// 负责管理所有Mono及非Mono对象池
    /// </summary>
    public partial class PoolManager : MonoBehaviour
    {
        //public delegate UnityEngine.Object InstantiateDelegate(UnityEngine.Object original);
        //public delegate void DestroyDelegate(GameObject obj);

        //public static InstantiateDelegate               InstantiatePrefabDelegate;
        //public static DestroyDelegate                   DestroyPrefabDelegate;


        private static PoolManager                      m_kInstance;
        static public PoolManager                       Instance
        {
            get
            {
                if(m_kInstance == null)
                {
                    GameObject go = new GameObject("[PoolManager]");
                    m_kInstance = go.AddComponent<PoolManager>();
                    DontDestroyOnLoad(go);
                }
                return m_kInstance;
            }
        }

        private static Dictionary<long, MonoPoolBase>           m_MonoPools         = new Dictionary<long, MonoPoolBase>();         // key: instanceId | poolType.hashcode << 32
                                                                                                                                    // 同一个PrefabAsset支持由多个不同类型Pool

        private static Dictionary<Type, IPool>                  m_Pools             = new Dictionary<Type, IPool>();

        static private Dictionary<string, IAssetLoaderProxy>    m_kAssetLoaderDict  = new Dictionary<string, IAssetLoaderProxy>();  // <key, value>: <assetPath, assetLoader>

        private void Awake()
        {
            if(GameObject.FindObjectsOfType<PoolManager>().Length > 1)
            {
                Debug.LogErrorFormat("PoolManager has already exist [{0}], kill it", name);
                DestroyImmediate(gameObject);
                return;
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
            RemoveAllMonoPools();
            UnregisterAllObjectPools();
            m_kInstance = null;
        }



        #region //////////////////////管理Mono对象接口—— GetOrCreate, RemoveMonoPool
        static public TPool GetOrCreatePool<TPooledObject, TPool, TLoaderType>(string assetPath) where TPooledObject : MonoPooledObjectBase where TPool : MonoPoolBase where TLoaderType : IAssetLoaderProxy
        {
            IAssetLoaderProxy loader;
            if (m_kAssetLoaderDict.TryGetValue(assetPath, out loader))
            {
                return GetOrCreatePool<TPooledObject, TPool>(loader.asset as GameObject);
            }

            loader = (IAssetLoaderProxy)Activator.CreateInstance(typeof(TLoaderType));
            loader.Load(assetPath);
            m_kAssetLoaderDict.Add(assetPath, loader);

            return GetOrCreatePool<TPooledObject, TPool>(loader.asset as GameObject);
        }

        static public PrefabObjectPool GetOrCreatePool<TPooledObject, TLoaderType>(string assetPath) where TPooledObject : MonoPooledObjectBase where TLoaderType : IAssetLoaderProxy
        {
            return GetOrCreatePool<TPooledObject, PrefabObjectPool, TLoaderType>(assetPath);
        }

        static public PrefabObjectPool GetOrCreatePool<TPooledObject>(GameObject asset) where TPooledObject : MonoPooledObjectBase
        {
            return GetOrCreatePool<TPooledObject, PrefabObjectPool>(asset);
        }

        static public TPool GetOrCreatePool<TPooledObject, TPool>(GameObject asset) where TPooledObject : MonoPooledObjectBase where TPool : MonoPoolBase
        {
            bool scriptNewAdded = false;
            TPooledObject comp = asset.GetComponent<TPooledObject>();
            if (comp == null)
            {
                scriptNewAdded = true;
                comp = asset.AddComponent<TPooledObject>();
            }
            return (TPool)GetOrCreatePoolInternal(comp, typeof(TPool), scriptNewAdded);
        }

        static public TPool GetOrCreatePool<TPool>(MonoPooledObjectBase asset) where TPool : MonoPoolBase
        {
            return (TPool)GetOrCreatePoolInternal(asset, typeof(TPool), false);
        }

        static public PrefabObjectPool GetOrCreatePool(MonoPooledObjectBase asset)
        {
            return (PrefabObjectPool)GetOrCreatePoolInternal(asset, typeof(PrefabObjectPool), false);
        }

        static private MonoPoolBase GetOrCreatePoolInternal(MonoPooledObjectBase asset, Type poolType, bool scriptNewAdded = true)
        {
            if (asset == null || poolType == null)
                return null;

            MonoPoolBase pool = GetMonoPool(asset, poolType);

            return pool != null ? pool : CreatePoolInternal(asset, poolType, scriptNewAdded);
        }

        static private MonoPoolBase CreatePoolInternal(MonoPooledObjectBase asset, Type poolType, bool scriptNewAdded = true)
        {
            GameObject go = new GameObject();
            go.transform.parent = Instance?.transform;
#if UNITY_EDITOR
            go.name = "[Pool]" + asset.gameObject.name;
#endif
            MonoPoolBase newPool = go.AddComponent(poolType) as MonoPoolBase;
            newPool.PrefabAsset = asset;
            newPool.ScriptNewAdded = scriptNewAdded;
            newPool.Group = go.transform;
            RegisterMonoPool(newPool);

            return newPool;
        }

        /// <summary>
        /// register new pool
        /// </summary>
        /// <param name="newPool"></param>
        static public void RegisterMonoPool(MonoPoolBase newPool)
        {
            if (newPool == null || newPool.PrefabAsset == null)
                throw new System.ArgumentNullException("newPool == null || newPool.PrefabAsset == null");

            if(GetMonoPool(newPool.PrefabAsset, newPool.GetType()) == null)
            {
                m_MonoPools.Add(GenerateKey(newPool.PrefabAsset, newPool.GetType()), newPool);
            }
        }

        static public void RemoveMonoPool<TPool>(string assetPath) where TPool : MonoPoolBase
        {
            IAssetLoaderProxy loader;
            if (!m_kAssetLoaderDict.TryGetValue(assetPath, out loader))
            {
                return;
            }

            RemoveMonoPool<TPool>(loader.asset as GameObject);
            m_kAssetLoaderDict.Remove(assetPath);
            loader.Unload();
        }

        static public void RemoveMonoPool(string assetPath)
        {
            RemoveMonoPool<PrefabObjectPool>(assetPath);
        }

        static public void RemoveMonoPool(MonoPoolBase pool)
        {
            if (pool == null)
                return;

            RemoveMonoPoolInternal(pool.PrefabAsset, pool.GetType());
        }

        static public void RemoveMonoPool<TPool>(MonoPooledObjectBase asset) where TPool : MonoPoolBase
        {
            if (asset == null)
                return;

            RemoveMonoPoolInternal(asset, typeof(TPool));
        }

        static public void RemoveMonoPool(GameObject prefabAsset)
        {
            RemoveMonoPool<PrefabObjectPool>(prefabAsset);
        }

        static public void RemoveMonoPool<TPool>(GameObject prefabAsset) where TPool : MonoPoolBase
        {
            if (prefabAsset == null)
                return;

            MonoPooledObjectBase comp = prefabAsset.GetComponent<MonoPooledObjectBase>();
            if(comp == null)
            {
                Debug.LogWarning("Can't find any script derived from MonoPooledObjectBase");
                return;
            }

            RemoveMonoPool<TPool>(comp);
        }

        static private void RemoveMonoPoolInternal(MonoPooledObjectBase asset, Type poolType)
        {
            if (asset == null || poolType == null)
                return;

            long key = GenerateKey(asset, poolType);
            MonoPoolBase pool;
            if (!m_MonoPools.TryGetValue(key, out pool))
            {
                // 为了安全可能重复销毁Pool，暂时屏蔽此Log
                //Debug.LogWarning($"Try to remove not exist pool, [{asset.gameObject.name}]  [{poolType.Name}]");
                return;
            }

            pool.Clear();
            UnityEngine.Object.Destroy(pool.gameObject);        // 删除dummy gameobject
            m_MonoPools.Remove(key);
        }

        static private void RemoveAllMonoPools()
        {
            Dictionary<long, MonoPoolBase>.Enumerator e = m_MonoPools.GetEnumerator();
            while (e.MoveNext())
            {
                e.Current.Value.Clear();
                UnityEngine.Object.Destroy(e.Current.Value.gameObject);
            }
            e.Dispose();

            m_MonoPools.Clear();
        }

        // 有必要提供此API吗？
        // 仅返回第一个符合查找条件的数据
        static private MonoPoolBase GetMonoPool(MonoPooledObjectBase asset)
        {
            if (asset == null)
                return null;

            MonoPoolBase pool = null;
            int instanceId = asset.gameObject.GetInstanceID();
            Dictionary<long, MonoPoolBase>.Enumerator e = m_MonoPools.GetEnumerator();
            while(e.MoveNext())
            {
                int key = (int)(e.Current.Key & 0xFFFFFFFF);
                if(key == instanceId)
                {
                    pool = e.Current.Value;
                    break;
                }
            }
            e.Dispose();

            return pool;
        }

        static private MonoPoolBase GetMonoPool(MonoPooledObjectBase asset, Type poolType)
        {
            if (asset == null || poolType == null)
                throw new System.ArgumentNullException("GetMonoPool: asset == null || poolType == null");

            MonoPoolBase pool;
            m_MonoPools.TryGetValue(GenerateKey(asset, poolType), out pool);
            return pool;
        }

        static private long GenerateKey(MonoPooledObjectBase asset, Type poolType)
        {
            if (asset == null || poolType == null)
                throw new System.ArgumentNullException("GenerateKey: asset == null || poolType == null");

            long key1 = asset.gameObject.GetInstanceID();
            long key2 = (long)poolType.GetHashCode() << 32;
            long key = key1 | key2;

            //Debug.Log($"InstanceID: {key1}  Type: {poolType.GetHashCode()}  Type<<32: {key2}    key: {key}");

            return key;
        }
        #endregion
        
        #region //////////////////////管理非Mono对象接口——register, unregister, trim

        static public void TrimAllObjectPools()
        {
            foreach(var pool in m_Pools)
            {
                pool.Value.Trim();
            }
        }

        static public void UnregisterAllObjectPools()
        {
            foreach(var pool in m_Pools)
            {
                pool.Value.Clear();
            }
            m_Pools.Clear();
        }

        /// <summary>
        /// 非Mono对象池注册接口
        /// </summary>
        /// <param name="type"></param>
        /// <param name="pool"></param>
        static public void RegisterObjectPool(Type type, IPool pool)
        {
            if(m_Pools.ContainsKey(type))
            {
                Debug.LogError($"RegisterObjectPool: Type[{type}] has already registered.");
                return;
            }

            m_Pools.Add(type, pool);
        }

        /// <summary>
        /// 非Mono对象池注销接口
        /// </summary>
        /// <param name="type"></param>
        static public void UnregisterObjectPool(Type type)
        {
            if(!m_Pools.ContainsKey(type))
            {
                Debug.LogError($"UnregisterObjectPool: Type[{type}] can't find");
                return;
            }

            m_Pools[type].Clear();
            m_Pools.Remove(type);
        }
        #endregion

        /// <summary>
        /// 由对象池管理的UnityEngine.Object的实例化接口
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        new static public UnityEngine.Object Instantiate(UnityEngine.Object original)
        {
            //if(InstantiatePrefabDelegate != null)
            //{
            //    return InstantiatePrefabDelegate(original);
            //}
            //else
            {
                return UnityEngine.Object.Instantiate(original);
            }
        }

        /// <summary>
        /// 由对象池管理的UnityEngine.Object销毁接口
        /// </summary>
        /// <param name="obj"></param>
        static public void Destroy(GameObject obj)
        {
            //if(DestroyPrefabDelegate != null)
            //{
            //    DestroyPrefabDelegate(obj);
            //}
            //else
            {
                UnityEngine.Object.Destroy(obj);
            }
        }
    }
}