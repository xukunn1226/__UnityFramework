using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    /// <summary>
    /// 负责管理所有Mono及非Mono对象池
    /// </summary>
    public class PoolManager : MonoBehaviour
    {
        public delegate UnityEngine.Object InstantiateDelegate(UnityEngine.Object original);
        public delegate void DestroyDelegate(GameObject obj);

        public static InstantiateDelegate               InstantiateDelegates;
        public static DestroyDelegate                   DestroyDelegates;


        private static PoolManager                      instance;

        private static Dictionary<long, MonoPoolBase>   m_MonoPools     = new Dictionary<long, MonoPoolBase>();         // key: instanceId | type.hashcode << 32

        private static Dictionary<Type, IPool>          m_Pools         = new Dictionary<Type, IPool>();

        private void Awake()
        {
            instance = this;

            transform.parent = null;
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            InitMonoPools();
        }

        private void OnDestroy()
        {
            Dictionary<long, MonoPoolBase>.Enumerator e = m_MonoPools.GetEnumerator();
            while (e.MoveNext())
            {
                Destroy(e.Current.Value.gameObject);
            }
            e.Dispose();
            m_MonoPools.Clear();
        }

        private void InitMonoPools()
        {
            MonoPoolBase[] pools = GetComponents<MonoPoolBase>();
            foreach(MonoPoolBase pool in pools)
            {
                MonoPoolBase newPool = InitPool(pool);
                if (newPool == null)
                    continue;

                if(newPool != pool)
                {
                    pool.enabled = false;       // 已存在对应Pool
                }
            }
        }

        private MonoPoolBase InitPool(MonoPoolBase pool)
        {
            if(pool == null || pool.PrefabAsset == null)
            {
                Debug.LogWarning("pool == null || pool.PrefabAsset == null, return null");
                return null;
            }

            MonoPoolBase newPool = GetMonoPool(pool.PrefabAsset, pool.GetType());
            if (newPool != null)
            {
                Debug.LogWarning($"PrefabAsset[{pool.PrefabAsset.gameObject.name}] managed with [{pool.GetType().Name}] has already exist, plz check it");
                return newPool;
            }

            GameObject go = new GameObject();
            go.transform.parent = transform;
#if UNITY_EDITOR
            go.name = "[Pool]" + pool.PrefabAsset.gameObject.name;
#endif
            pool.Group = go.transform;
            m_MonoPools.Add(GenerateKey(pool.PrefabAsset, pool.GetType()), pool);

            return pool;
        }






        public static PrefabObjectPool GetOrCreatePool(MonoPooledObjectBase asset)
        {
            return GetOrCreatePool<PrefabObjectPool>(asset);
        }

        public static T GetOrCreatePool<T>(MonoPooledObjectBase asset) where T : MonoPoolBase
        {
            if (asset == null)
                return null;

            MonoPoolBase pool = GetMonoPool(asset, typeof(T));
            if (pool != null)
            {
                //Debug.LogWarning($"PrefabAsset[{asset.gameObject.name}] managed with [{typeof(T).Name}] has already exist, plz check it");
                return (T)pool;
            }

            GameObject go = new GameObject();
            go.transform.parent = instance?.transform;
#if UNITY_EDITOR
            go.name = "[Pool]" + asset.gameObject.name;
#endif
            T newPool = go.AddComponent<T>();
            newPool.PrefabAsset = asset;
            newPool.Group = go.transform;
            m_MonoPools.Add(GenerateKey(asset, typeof(T)), newPool);
            
            return newPool;
        }

        public static void RemovePool(MonoPoolBase pool)
        {
            if (pool == null || pool.PrefabAsset == null)
                return;

            long key = GenerateKey(pool.PrefabAsset, pool.GetType());
            if (!m_MonoPools.ContainsKey(key))
            {
                Debug.LogWarning($"Try to remove an inexistent pool, [{pool.PrefabAsset.gameObject.name}]  [{pool.GetType().Name}]");
                return;
            }

            Destroy(pool.Group.gameObject);
            m_MonoPools.Remove(key);
        }

        public static void RemovePool<T>(MonoPooledObjectBase asset) where T : MonoPoolBase
        {
            if (asset == null)
                return;

            long key = GenerateKey(asset, typeof(T));
            MonoPoolBase pool;
            if(!m_MonoPools.TryGetValue(key, out pool))
            {
                Debug.LogWarning($"Try to remove not exist pool, [{asset.gameObject.name}]  [{typeof(T).Name}]");
                return;
            }

            Destroy(pool.Group.gameObject);
            m_MonoPools.Remove(key);
        }

        // 仅返回第一个符合查找条件的数据
        public static MonoPoolBase GetPool(MonoPooledObjectBase asset)
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

        private static MonoPoolBase GetMonoPool(MonoPooledObjectBase asset, Type poolType)
        {
            if (asset == null || poolType == null)
                return null;

            MonoPoolBase pool;
            m_MonoPools.TryGetValue(GenerateKey(asset, poolType), out pool);
            return pool;
        }

        private static long GenerateKey(MonoPooledObjectBase asset, Type poolType)
        {
            long key1 = asset.gameObject.GetInstanceID();
            long key2 = (long)poolType.GetHashCode() << 32;
            long key = key1 | key2;

            //Debug.Log($"InstanceID: {key1}  Type: {poolType.GetHashCode()}  Type<<32: {key2}    key: {key}");

            return key;
        }


        /// <summary>
        /// 非Mono对象池注册接口
        /// </summary>
        /// <param name="type"></param>
        /// <param name="pool"></param>
        public static void RegisterObjectPool(Type type, IPool pool)
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
        public static void UnregisterObjectPool(Type type)
        {
            if(!m_Pools.ContainsKey(type))
            {
                Debug.LogError($"UnregisterObjectPool: Type[{type}] can't find");
                return;
            }

            m_Pools[type].Clear();
            m_Pools.Remove(type);
        }

        /// <summary>
        /// 由对象池管理的UnityEngine.Object的实例化接口
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        new public static UnityEngine.Object Instantiate(UnityEngine.Object original)
        {
            if(InstantiateDelegates != null)
            {
                return InstantiateDelegates(original);
            }
            else
            {
                return UnityEngine.Object.Instantiate(original);
            }
        }

        /// <summary>
        /// 由对象池管理的UnityEngine.Object销毁接口
        /// </summary>
        /// <param name="obj"></param>
        public static void Destroy(GameObject obj)
        {
            if(DestroyDelegates != null)
            {
                DestroyDelegates(obj);
            }
            else
            {
                UnityEngine.Object.Destroy(obj);
            }
        }
    }
}