using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;
using Framework.AssetManagement.Runtime;
using System;
using Object = UnityEngine.Object;

namespace Framework.Cache
{
    public sealed class PoolManager : SingletonMono<PoolManager>
    {
        private static Dictionary<Type, IPool>                  m_Pools             = new Dictionary<Type, IPool>();                // 非Mono对象池

        private static Dictionary<long, MonoPoolBase>           m_MonoPools         = new Dictionary<long, MonoPoolBase>();         // Mono对象池（同一个PrefabAsset支持由多个不同类型的Pool）
                                                                                                                                    // key: instanceId | poolType.hashcode << 32

        public class PrefabedPoolInfo
        {
            public MonoPoolBase         m_Pool;
            public int                  m_RefCount;
            internal BuiltinAssetLoader m_Loader;
        }
        static private Dictionary<string, PrefabedPoolInfo>     m_PrefabedPoolDict  = new Dictionary<string, PrefabedPoolInfo>();   // Prefab对象池集合
                                                                                                                                    // <key, value> : <对象池Prefab资源路径, PrefabedPoolInfo>

#if UNITY_EDITOR
        static public Dictionary<Type, IPool>                   Pools                   { get { return m_Pools; } }
        static public Dictionary<long, MonoPoolBase>            MonoPools               { get { return m_MonoPools; } }
        static public Dictionary<string, PrefabedPoolInfo>      PrefabedPools           { get { return m_PrefabedPoolDict; } }
        static private HashSet<MonoPooledObject>                m_DynamicAddedScripts   = new HashSet<MonoPooledObject>();
#endif

        protected override void OnDestroy()
        {
#if UNITY_EDITOR
            ProcessDynamicAddedScript();      // 动态加载的脚本退出play mode时需要删除
#endif                
            Destroy();
            base.OnDestroy();
        }

        static public void Destroy()
        {
            // RemoveAllAssetLoaders();
            RemoveAllPrefabedPools();
            // RemoveAllLRUPools();

            // // 基础数据最后清除
            RemoveAllMonoPools();
            RemoveAllObjectPools();
        }

        static public void Trim()
        {
            foreach(var pool in m_Pools)
            {
                pool.Value.Trim();
            }
            foreach(var pool in m_MonoPools)
            {
                pool.Value.Trim();
            }
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


#if UNITY_EDITOR
        static private void TrackDynamicAddedScript(MonoPooledObject comp)
        {
            m_DynamicAddedScripts.Add(comp);
        }

        static private void ProcessDynamicAddedScript()
        {
            foreach(var obj in m_DynamicAddedScripts)
            {
                UnityEngine.Object.DestroyImmediate(obj, true);
            }
        }
#endif



        #region ////////////////////// 管理Mono对象接口—— GetOrCreate, RemoveMonoPool
        static public PrefabObjectPool GetOrCreatePool(GameObject prefabAsset)
        {
            return GetOrCreatePool<MonoPooledObject, PrefabObjectPool>(prefabAsset);
        }

        /// <summary>
        /// 创建对象池，默认以PrefabObjectPool管理
        /// </summary>
        /// <typeparam name="TPooledObject">缓存对象脚本</typeparam>
        /// <param name="prefabAsset">缓存对象</param>
        /// <returns></returns>
        static public PrefabObjectPool GetOrCreatePool<TPooledObject>(GameObject prefabAsset) where TPooledObject : MonoPooledObject
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
        static public TPool GetOrCreatePool<TPooledObject, TPool>(GameObject prefabAsset) where TPooledObject : MonoPooledObject where TPool : MonoPoolBase
        {
            TPooledObject comp = prefabAsset.GetComponent<TPooledObject>();
            if (comp == null)
            {
                comp = prefabAsset.AddComponent<TPooledObject>();
#if UNITY_EDITOR
                TrackDynamicAddedScript(comp);      // 记录动态加载的脚本，退出play mode时需要删除
#endif                
            }
            return (TPool)InternalGetOrCreatePool(comp, typeof(TPool));
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

            MonoPoolBase pool = FindMonoPool(prefabAsset, poolType);
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
            newPool.Init();
            AddMonoPool(newPool);

            return newPool;
        }





        static public TPool BeginCreateEmptyPool<TPool>() where TPool : MonoPoolBase
        {
            return (TPool)InternalBeginCreateEmptyPool(typeof(TPool));
        }

        static public PrefabObjectPool BeginCreateEmptyPool()
        {
            return (PrefabObjectPool)InternalBeginCreateEmptyPool(typeof(PrefabObjectPool));
        }

        static private MonoPoolBase InternalBeginCreateEmptyPool(Type poolType)
        {
            if(poolType == null)
                throw new ArgumentNullException("poolType == null");
            
            GameObject go = new GameObject();
            go.transform.parent = Instance?.transform;
#if UNITY_EDITOR
            go.name = GetMonoPoolName(poolType);
#endif
            MonoPoolBase newPool = go.AddComponent(poolType) as MonoPoolBase;
            return newPool;
        }

        static public MonoPoolBase EndCreateEmptyPool(MonoPoolBase newPool, GameObject prefabAsset)
        {
            return EndCreateEmptyPool<MonoPooledObject>(newPool, prefabAsset);
        }

        static public MonoPoolBase EndCreateEmptyPool<TPooledObject>(MonoPoolBase newPool, GameObject prefabAsset) where TPooledObject : MonoPooledObject
        {
            TPooledObject comp = prefabAsset.GetComponent<TPooledObject>();
            if(comp == null)
            {
                comp = prefabAsset.gameObject.AddComponent<TPooledObject>();
#if UNITY_EDITOR
                TrackDynamicAddedScript(comp);      // 记录动态加载的脚本，退出play mode时需要删除
#endif                                
            }

            // 调用EndCreateEmptyPool之前不可赋值PrefabAsset
            if(newPool.PrefabAsset != null)
                throw new System.ArgumentException("EndCreateEmptyPool: newPool.PrefabAsset != null");
            newPool.PrefabAsset = comp;
            return InternalEndCreateEmptyPool(newPool);
        }

        static public MonoPoolBase EndCreateEmptyPool(MonoPoolBase newPool, MonoPooledObject prefabAsset)
        {
            newPool.PrefabAsset = prefabAsset;
            return InternalEndCreateEmptyPool(newPool);
        }

        static public MonoPoolBase InternalEndCreateEmptyPool(MonoPoolBase newPool)
        {
            if(newPool == null || newPool.PrefabAsset == null)
                throw new System.ArgumentNullException("EndCreateEmptyPool:newPool == null || newPool.PrefabAsset == null");

            MonoPoolBase pool = FindMonoPool(newPool.PrefabAsset, newPool.GetType());
            if(pool != null)
            {
                Debug.LogWarning($"RegisterMonoPool: {newPool} has already exist in mono pools");
                return pool;
            }

            newPool.Init();
#if UNITY_EDITOR
            newPool.gameObject.name = GetMonoPoolName(newPool);
#endif            
            AddMonoPool(newPool);
            return newPool;
        }

        static public string GetMonoPoolName(MonoPoolBase pool)
        {
            string prefabAssetName = pool.PrefabAsset != null ? pool.PrefabAsset.gameObject.name : "Empty";
            string poolTypeName = pool.name;
            return string.Format($"[Pool] {prefabAssetName}_{poolTypeName}");
        }

        static public string GetMonoPoolName(Type poolType)
        {
            return string.Format($"[Pool] \"Empty\"_{poolType.Name}");
        }

        /// <summary>
        /// 添加对象池
        /// NOTE: 为了兼容把对象池制作为Prefab，故开放此接口，见PrefabObjectPool
        /// </summary>
        /// <param name="newPool"></param>
        static private void AddMonoPool(MonoPoolBase newPool)
        {
            if (newPool == null || newPool.PrefabAsset == null)
                throw new System.ArgumentNullException("newPool == null || newPool.PrefabAsset == null");

            if (FindMonoPool(newPool.PrefabAsset, newPool.GetType()) != null)
            {
                Debug.LogError($"AddMonoPool: {newPool} has already exist in mono pools");
                return;
            }
            m_MonoPools.Add(GetMonoPoolHashCode(newPool.PrefabAsset, newPool.GetType()), newPool);

            // 对象池统一挂载到PoolManager
            if (PoolManager.Instance.gameObject != newPool.transform.root.gameObject)
            {
                newPool.transform.parent = PoolManager.Instance.transform;
            }
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
        static public MonoPoolBase[] FindMonoPools(MonoPooledObject prefabAsset)
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
        static public MonoPoolBase FindMonoPool(MonoPooledObject prefabAsset, Type poolType)
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






        #region ////////////////////// Prefabed Pool管理接口
        internal class BuiltinAssetLoader
        {
            private AssetOperationHandle m_Handle;

            public GameObject asset
            {
                get
                {
                    return m_Handle?.assetObject as GameObject;
                }
            }

            public GameObject Load(string assetPath)
            {
                m_Handle = AssetManagerEx.LoadAsset<GameObject>(assetPath);
                return m_Handle.assetObject as GameObject;
            }

            public void Unload()
            {
                m_Handle?.Release();
            }
        }

        /// <summary>
        /// 基于资源地址获取/创建对象池，目前仅支持prefab asset
        /// WARNING: 不同于其他GetOrCreatePool接口，每次获取使得对象池引用计数自增
        /// 最佳实践是不同应用环境只使用一次，把pool保存下来反复使用
        /// </summary>
        /// <param name="assetPath">对象池Prefab资源地址</param>
        /// <returns></returns>
        static public PrefabObjectPool GetOrCreatePool(string assetPath)
        {            
            return (PrefabObjectPool)GetOrCreatePool<MonoPooledObject, PrefabObjectPool>(assetPath);
        }

        static public PrefabObjectPool GetOrCreatePool<TPooledObject>(string assetPath) where TPooledObject : MonoPooledObject
        {
            return (PrefabObjectPool)GetOrCreatePool<TPooledObject, PrefabObjectPool>(assetPath);
        }

        static public MonoPoolBase GetOrCreatePool<TPooledObject, TPool>(string assetPath) where TPooledObject : MonoPooledObject where TPool : MonoPoolBase
        {
            PrefabedPoolInfo info;
            if (m_PrefabedPoolDict.TryGetValue(assetPath, out info))
            {
                info.m_RefCount += 1;
                return info.m_Pool;
            }

            BuiltinAssetLoader loader = new BuiltinAssetLoader();
            loader.Load(assetPath);     // 仅加载了prefab资源，未实例化
            if (loader.asset == null)
                throw new ArgumentNullException("loader.asset", $"failed to load asset {assetPath}");

            TPooledObject comp = loader.asset.GetComponent<TPooledObject>();
            if(comp == null)
            {
                comp = loader.asset.AddComponent<TPooledObject>();
                #if UNITY_EDITOR
                TrackDynamicAddedScript(comp);
                #endif
            }

            info = new PrefabedPoolInfo();
            info.m_Pool = GetOrCreatePool<TPooledObject, TPool>(loader.asset);
            info.m_RefCount = 1;
            info.m_Loader = loader;
            m_PrefabedPoolDict.Add(assetPath, info);
            return (TPool)info.m_Pool;
        }

        static public MonoPoolBase EndCreateEmptyPool(MonoPoolBase newPool, string assetPath)
        {
            return EndCreateEmptyPool<MonoPooledObject>(newPool, assetPath);
        }
        
        static public MonoPoolBase EndCreateEmptyPool<TPooledObject>(MonoPoolBase newPool, string assetPath) where TPooledObject : MonoPooledObject
        {
            PrefabedPoolInfo info;
            if (m_PrefabedPoolDict.TryGetValue(assetPath, out info))
            {
                UnityEngine.Object.Destroy(newPool.gameObject);     // ugly code
                info.m_RefCount += 1;
                return info.m_Pool;
            }

            BuiltinAssetLoader loader = new BuiltinAssetLoader();
            loader.Load(assetPath);     // 仅加载了prefab资源，未实例化
            if (loader.asset == null)
                throw new ArgumentNullException("loader.asset", $"failed to load asset {assetPath}");

            TPooledObject comp = loader.asset.GetComponent<TPooledObject>();
            if(comp == null)
            {
                comp = loader.asset.AddComponent<TPooledObject>();
                #if UNITY_EDITOR
                TrackDynamicAddedScript(comp);
                #endif
            }

            newPool.PrefabAsset = comp;
            newPool.Init();
            #if UNITY_EDITOR
            newPool.gameObject.name = GetMonoPoolName(newPool);
            #endif            
            AddMonoPool(newPool);

            info = new PrefabedPoolInfo();
            info.m_Pool = newPool;
            info.m_RefCount = 1;
            info.m_Loader = loader;
            m_PrefabedPoolDict.Add(assetPath, info);
            return info.m_Pool;
        }






        /// <summary>
        /// 释放对象池Prefab，与GetOrCreatePool对应
        /// </summary>
        /// <param name="assetPath"></param>
        static public void RemoveMonoPool(string assetPath)
        {
            PrefabedPoolInfo info;
            if (!m_PrefabedPoolDict.TryGetValue(assetPath, out info))
            {
                Debug.LogError($"RemoveMonoPool: {assetPath} not exist");
                return;
            }

            info.m_RefCount -= 1;
            if (info.m_RefCount <= 0)
            {
                m_PrefabedPoolDict.Remove(assetPath);
                RemoveMonoPool(info.m_Pool);
                info.m_Loader.Unload();
            }
        }
        
        // 见RemoveMonoPrefabedPool
        static public void RemoveAllPrefabedPools()
        {
            foreach (var item in m_PrefabedPoolDict)
            {
                PrefabedPoolInfo info = item.Value;

                if (info.m_RefCount > 0)
                {
                    Debug.LogWarning($"RemoveAllPrefabedPools: refCount:{info.m_RefCount} greater than 1. AssetPath: {item.Key}");
                }

                RemoveMonoPool(info.m_Pool);
                info.m_Loader.Unload();
            }
            m_PrefabedPoolDict.Clear();
        }
        #endregion







        /// <summary>
        /// 对象实例化接口，避免直接使用UnityEngine.Object.Instantiate
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        static public MonoPooledObject Instantiate(MonoPooledObject original, MonoPoolBase pool)
        {
            MonoPooledObject obj = Object.Instantiate(original);
            obj.Pool = pool;
            return obj;
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