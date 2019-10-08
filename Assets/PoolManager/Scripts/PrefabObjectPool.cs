﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public class PrefabObjectPool : MonoPoolBase
    {
        public GenericMonoPooledObject          PrefabAsset;

        [Range(0, 100)]
        public int                              PreAllocateAmount   = 1;                // 预实例化数量

        public bool                             LimitInstance;                          // 是否设定实例化上限值

        [Range(1, 1000)]
        public int                              LimitAmount         = 20;               // 最大实例化数量（激活与未激活）
        
        public bool                             CullDeactived;                          // 是否开启清理未激活实例功能

        public int                              CullAbove           = 10;               // 自动清理开启时至少保持deactive的数量

        
        private List<IPooledObject>             m_DeactiveObjects = new List<IPooledObject>();

        private int                             m_ActiveObjectsCount;

        public int                              totalCount
        {
            get
            {
                return m_DeactiveObjects.Count + m_ActiveObjectsCount;
            }
        }

        private void Awake()
        {
            Warmup();
        }

        public void Warmup()
        {
            if (PreAllocateAmount <= 0 || PrefabAsset == null)
                return;

            if(LimitInstance)
            {
                PreAllocateAmount = Mathf.Min(PreAllocateAmount, LimitAmount);
            }

            while(totalCount < PreAllocateAmount)
            {
                IPooledObject obj = Get();
                Return(obj);
            }
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
                }
            }

            if (obj != null)
            {
                obj.Pool = this;
                obj.OnGet();
                ++m_ActiveObjectsCount;
            }
            return obj;
        }

        public override void Return(IPooledObject item)
        {
            --m_ActiveObjectsCount;
            item.OnRelease();
            m_DeactiveObjects.Add(item);

            if(CullDeactived && m_DeactiveObjects.Count > CullAbove)
            {
                CullDeactivedObject();
            }
        }

        private void CullDeactivedObject()
        {
            while(m_DeactiveObjects.Count > CullAbove)
            {
                GenericMonoPooledObject inst = m_DeactiveObjects[m_DeactiveObjects.Count - 1] as GenericMonoPooledObject;
                m_DeactiveObjects.RemoveAt(m_DeactiveObjects.Count - 1);

                if(inst != null)
                    Destroy(inst);
            }
        }

        public void Clear()
        {
            int count = m_DeactiveObjects.Count;
            for(int i = 0; i < count; ++i)
            {
                GenericMonoPooledObject inst = m_DeactiveObjects[i] as GenericMonoPooledObject;
                if(inst != null)
                {
                    Destroy(inst.gameObject);
                }
            }
            m_DeactiveObjects.Clear();
        }

        public GenericMonoPooledObject GetObject()
        {
            return Get() as GenericMonoPooledObject;
        }

        public GenericMonoPooledObject GetObject(Vector3 pos = default(Vector3), Quaternion rot = default(Quaternion), Transform parent = null)
        {
            GenericMonoPooledObject inst = GetObject();
            if (inst != null)
            {
                inst.transform.position = pos;
                inst.transform.rotation = rot;
                inst.transform.parent = parent != null ? parent : transform;
            }
            return inst;
        }
    }
}