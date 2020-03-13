using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    /// <summary>
    /// 具有生命周期Prefab的缓存池
    /// </summary>
    public class LivingPrefabObjectPool : MonoPoolBase
    {
        public int LimitAmount = 20;
        public float Speed = 1;

        private List<LivingMonoPooledObject> m_DeactiveObjects = new List<LivingMonoPooledObject>();

        public int countAll { get; private set; }

        public int countActive { get { return countAll - countInactive; } }

        public int countInactive { get { return m_DeactiveObjects.Count; } }

        private void OnDestroy()
        {
            Clear();
        }

        public override IPooledObject Get()
        {
            if (PrefabAsset == null)
            {
                Debug.LogError("LivingPrefabObjectPool::Get() return null, because of PrefabAsset == null");
                return null;
            }

            LivingMonoPooledObject obj;
            if (m_DeactiveObjects.Count > 0)
            {
                obj = m_DeactiveObjects[m_DeactiveObjects.Count - 1];
                m_DeactiveObjects.RemoveAt(m_DeactiveObjects.Count - 1);
            }
            else
            {
                obj = (LivingMonoPooledObject)PoolManager.Instantiate(PrefabAsset);
                obj.Pool = this;
                ++countAll;
            }

            if (obj != null)
            {                
                obj.transform.parent = Group;       // 每次取出时默认放置Group下，因为parent可能会被改变
                obj.OnGet();

                if(countActive > LimitAmount)
                {
                    obj.SetSpeed(Speed);
                }
            }

            return obj;
        }

        public override void Return(IPooledObject item)
        {
            if (item == null)
                return;

            LivingMonoPooledObject monoObj = (LivingMonoPooledObject)item;
            m_DeactiveObjects.Add(monoObj);
            monoObj.transform.parent = Group;           // 回收时放置Group下

            monoObj.OnRelease();
        }

        public override void Clear()
        {
            int count = m_DeactiveObjects.Count;
            for (int i = 0; i < count; ++i)
            {
                MonoPooledObjectBase inst = m_DeactiveObjects[i];
                if (inst != null)
                {
                    PoolManager.Destroy(inst.gameObject);
                }
            }
            m_DeactiveObjects.Clear();
        }

        public override void Trim()
        {
        }
    }
}