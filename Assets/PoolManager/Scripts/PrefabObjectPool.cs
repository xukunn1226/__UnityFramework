﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public class PrefabObjectPool : MonoPoolBase
    {
        [Range(0, 100)]
        public int                              PreAllocateAmount   = 1;                // 预实例化数量

        public bool                             LimitInstance;                          // 是否设定实例化上限值

        [Range(1, 1000)]
        public int                              LimitAmount         = 20;               // 最大实例化数量（激活与未激活）
        
        public bool                             TrimDeactived;                          // 是否开启清理未激活实例功能

        [Range(1, 100)]
        public int                              TrimAbove           = 10;               // 自动清理开启时至少保持deactive的数量

        
        private List<IPooledObject>             m_DeactiveObjects   = new List<IPooledObject>();

        private int                             m_ActiveObjectsCount;

        public int                              totalCount
        {
            get
            {
                return m_DeactiveObjects.Count + m_ActiveObjectsCount;
            }
        }

        // 等待参数设置，故需在Start执行
        private void Start()
        {
            Warmup();
        }

        private void OnDestroy()
        {
            Clear();
        }

#if UNITY_EDITOR
        private void Update()
        {
            DisplayDebugInfo();
        }
#endif

        protected override void Warmup()
        {
            if (PreAllocateAmount <= 0 || PrefabAsset == null)
                return;

            if(LimitInstance)
            {
                PreAllocateAmount = Mathf.Min(PreAllocateAmount, LimitAmount);
            }

            while(totalCount < PreAllocateAmount)
            {
                IPooledObject obj = GetNew();
                Return(obj);
            }
        }

        private IPooledObject GetNew()
        {
            if (PrefabAsset == null)
            {
                Debug.LogError("PrefabObjectPool::GetNew() return null, because of PrefabAsset == null");
                return null;
            }

            IPooledObject obj = null;
            if (!LimitInstance || totalCount < LimitAmount)
            {
                obj = Instantiate(PrefabAsset);
                obj.Pool = this;
            }

            if (obj != null)
            {
                ++m_ActiveObjectsCount;

                MonoPooledObjectBase monoObj = (MonoPooledObjectBase)obj;
                monoObj.transform.parent = Group;       // 默认放置Group下

                obj.OnGet();
            }

            return obj;
        }

        public override IPooledObject Get()
        {
            if (PrefabAsset == null)
            {
                Debug.LogError("PrefabObjectPool::Get() return null, because of PrefabAsset == null");
                return null;
            }

            IPooledObject obj = null;
            if (m_DeactiveObjects.Count > 0)
            {
                obj = m_DeactiveObjects[m_DeactiveObjects.Count - 1];
                m_DeactiveObjects.RemoveAt(m_DeactiveObjects.Count - 1);
            }
            else
            {
                if (!LimitInstance || totalCount < LimitAmount)
                {
                    obj = Instantiate(PrefabAsset);
                    obj.Pool = this;
                }
            }

            if (obj != null)
            {
                ++m_ActiveObjectsCount;

                MonoPooledObjectBase monoObj = (MonoPooledObjectBase)obj;
                monoObj.transform.parent = Group;       // 默认放置Group下

                obj.OnGet();
            }

            return obj;
        }

        public override void Return(IPooledObject item)
        {
            if (item == null)
                return;

            --m_ActiveObjectsCount;
            m_DeactiveObjects.Add(item);

            MonoPooledObjectBase monoObj = (MonoPooledObjectBase)item;
            monoObj.transform.parent = Group;           // 回收时放置Group下

            item.OnRelease();

            if(TrimDeactived && m_DeactiveObjects.Count > TrimAbove)
            {
                TrimExcess();
            }
        }

        public override void TrimExcess()
        {
            while(m_DeactiveObjects.Count > TrimAbove)
            {
                MonoPooledObjectBase inst = m_DeactiveObjects[m_DeactiveObjects.Count - 1] as MonoPooledObjectBase;
                m_DeactiveObjects.RemoveAt(m_DeactiveObjects.Count - 1);

                if (inst != null)
                {
                    Destroy(inst);
                }
            }
        }

        protected override void Clear()
        {
            int count = m_DeactiveObjects.Count;
            for(int i = 0; i < count; ++i)
            {
                MonoPooledObjectBase inst = m_DeactiveObjects[i] as MonoPooledObjectBase;
                if(inst != null)
                {
                    Destroy(inst.gameObject);
                }
            }
            m_DeactiveObjects.Clear();
        }

        public MonoPooledObjectBase GetObject()
        {
            return Get() as MonoPooledObjectBase;
        }

        public MonoPooledObjectBase GetObject(Vector3 pos = default(Vector3), Quaternion rot = default(Quaternion), Transform parent = null)
        {
            MonoPooledObjectBase inst = GetObject();
            if (inst != null)
            {
                inst.transform.position = pos;
                inst.transform.rotation = rot;
                inst.transform.parent = parent != null ? parent : Group;
            }
            return inst;
        }

        private void DisplayDebugInfo()
        {
            gameObject.name = string.Format("[Pool]{0} ({1}/{2})", PrefabAsset.gameObject.name, m_DeactiveObjects.Count, totalCount);
        }
    }
}