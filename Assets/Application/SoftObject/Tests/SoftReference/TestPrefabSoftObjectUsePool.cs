using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.Runtime;
using Framework.Core;
using Framework.Cache;

namespace Application.Runtime.Tests
{
    /// <summary>
    /// 测试两种对象池的使用方法
    /// 1、代码创建对象池，使用接口SpawnFromPool、DestroyPool
    /// 2、把对象池制作成prefab，使用接口GetOrCreatePoolInst、RemoveMonoPoolInst
    /// </summary>
    public class TestPrefabSoftObjectUsePool : MonoBehaviour
    {
        string info;

        [SoftObject]
        [Tooltip("缓存对象")]
        public SoftObject m_PooledObject;
       
        private Stack<TestPooledObject> m_Stack = new Stack<TestPooledObject>();

        private void OnGUI()
        {
            if (GUI.Button(new Rect(100, 100, 180, 80), "Load"))
            {
                StartTask_Case1();
            }

            if (GUI.Button(new Rect(100, 200, 180, 80), "Return To Pool"))
            {
                ReturnToPool_Case1();
            }

            if (GUI.Button(new Rect(100, 300, 180, 80), "Unload"))
            {
                EndTask_Case1();
            }

            if (!string.IsNullOrEmpty(info))
            {
                GUI.Label(new Rect(100, 600, 500, 100), info);
            }
        }

        void StartTask_Case1()
        {
            if (m_PooledObject == null || string.IsNullOrEmpty(m_PooledObject.assetName))
                return;

            TestPooledObject obj = (TestPooledObject)m_PooledObject.SpawnFromPool<TestPooledObject, PrefabObjectPool>();
            obj.transform.position = Random.insideUnitSphere * 3;

            m_Stack.Push(obj);

            info = obj.gameObject != null ? "sucess to load: " : "fail to load: ";
            info += m_PooledObject.assetName;
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
                m_PooledObject.DestroyPool();
            }
            info = null;
        }
    }
}