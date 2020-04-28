using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.Runtime;
using Framework.Core;
using Framework.Cache;

namespace Tests
{
    /// <summary>
    /// 测试两种对象池的使用方法
    /// 1、代码创建对象池，使用接口SpawnFromPool、DestroyPool
    /// 2、把对象池制作成prefab，使用接口GetOrCreatePoolInst、RemoveMonoPoolInst
    /// </summary>
    public class TestPrefabSoftObjectUsePool : MonoBehaviour
    {
        public LoaderType type;

        string info;

        [SoftObject]
        [Tooltip("缓存对象")]
        public SoftObject m_PooledObject;           // for case1

        [SoftObject]
        [Tooltip("对象池Prefab")]
        public SoftObject m_PoolPrefab;             // for case2
        
        private Stack<TestPooledObject> m_Stack = new Stack<TestPooledObject>();

        private void Awake()
        {
            AssetManager.Init(type);
        }

        void OnDestroy()
        {
            AssetManager.Uninit();
        }

        private void OnGUI()
        {
            if (GUI.Button(new Rect(100, 100, 180, 80), "Load"))
            {
                StartTask_Case2();
            }

            if (GUI.Button(new Rect(100, 200, 180, 80), "Return To Pool"))
            {
                ReturnToPool_Case2();
            }

            if (GUI.Button(new Rect(100, 300, 180, 80), "Unload"))
            {
                EndTask_Case2();
            }

            if (!string.IsNullOrEmpty(info))
            {
                GUI.Label(new Rect(100, 600, 500, 100), info);
            }
        }

        void StartTask_Case1()
        {
            if (m_PooledObject == null || string.IsNullOrEmpty(m_PooledObject.assetPath))
                return;

            TestPooledObject obj = (TestPooledObject)m_PooledObject.SpawnFromPool<TestPooledObject, PrefabObjectPool>();
            obj.transform.position = Random.insideUnitSphere * 3;

            m_Stack.Push(obj);

            info = obj.gameObject != null ? "sucess to load: " : "fail to load: ";
            info += m_PooledObject.assetPath;
        }

        void ReturnToPool_Case1()
        {
            if (m_Stack.Count > 0)
            {
                TestPooledObject item = m_Stack.Pop();
                item.ReturnToPool();
            }
        }

        void EndTask_Case1()
        {
            if (m_PooledObject != null)
            {
                m_PooledObject.DestroyPool<PrefabObjectPool>();
            }
            info = null;
        }









        void StartTask_Case2()
        {
            if (m_PoolPrefab == null || string.IsNullOrEmpty(m_PoolPrefab.assetPath))
                return;

            TestPooledObject obj = (TestPooledObject)m_PoolPrefab.SpawnFromPrefabedPool();
            obj.transform.position = Random.insideUnitSphere * 3;

            m_Stack.Push(obj);

            info = obj.gameObject != null ? "sucess to load: " : "fail to load: ";
            info += m_PooledObject.assetPath;
        }

        void ReturnToPool_Case2()
        {
            if (m_Stack.Count > 0)
            {
                TestPooledObject item = m_Stack.Pop();
                item.ReturnToPool();
            }
        }

        void EndTask_Case2()
        {
            if (m_PoolPrefab == null || string.IsNullOrEmpty(m_PoolPrefab.assetPath))
                return;

            m_PoolPrefab.DestroyPrefabedPool();
            info = null;
        }
    }
}