using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;

namespace Framework.Cache
{
    // 以资源方式实例化的对象池不由PoolManager管理
    public class PrefabObjectPoolEx : MonoPoolBase
    {
        [Tooltip("预实例化数量")]
        [Range(0, 100)]
        public int                                          PreAllocateAmount   = 1;                // 预实例化数量，实例化后存储于m_UnusedObjects

        [Tooltip("是否设定实例化上限值")]
        public bool                                         LimitInstance;                          // 是否设定实例化上限值

        [Tooltip("最大实例化数量，需要开启LimitInstance")]
        [Range(1, 1000)]
        public int                                          LimitAmount         = 20;               // 最大实例化数量（需要开启LimitInstance）

        [Tooltip("是否开启清理未激活实例功能")]
        public bool                                         TrimUnused;                             // 是否开启清理未激活实例功能

        [Tooltip("至少保持deactive的数量，需要开启TrimUnused")]
        [Range(1, 100)]
        public int                                          TrimAbove           = 10;               // 自动清理开启时至少保持unused的数量

        protected BetterLinkedList<MonoPooledObject>        m_UsedObjects       = new BetterLinkedList<MonoPooledObject>();

        protected Stack<MonoPooledObject>                   m_UnusedObjects     = new Stack<MonoPooledObject>();

        public override int                                 countAll            { get { return countOfUsed + countOfUnused; } }

        public override int                                 countOfUsed         { get { return m_UsedObjects.Count; } }

        public override int                                 countOfUnused       { get { return m_UnusedObjects.Count; } }

        private void Awake()
        {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            // 当动态创建对象池时PrefabAsset可能尚未赋值
            if(PrefabAsset != null)
            {
                Init();
            }
        }

        // 动态创建对象池时，通过此接口主动设置缓存对象
        public override void Init()
        {            
            if(PrefabAsset == null)
                throw new System.ArgumentNullException("PrefabObjectPool.PrefabAsset");

            Warmup();
        }

#if UNITY_EDITOR
        private void Update()
        {
            DisplayDebugInfo();
        }
#endif

        /// <summary>
        /// 预实例化对象
        /// </summary>
        private void Warmup()
        {
            if(TrimUnused)
            {
                PreAllocateAmount = Mathf.Min(PreAllocateAmount, TrimAbove);
            }

            if(LimitInstance)
            {
                PreAllocateAmount = Mathf.Min(PreAllocateAmount, LimitAmount);
            }

            while(countAll < PreAllocateAmount)
            {
                IPooledObject obj = InternalGet(true);      // 强制创建新的实例，而不是从池中获取
                if (obj != null)
                {
                    Return(obj);
                }
            }
        }

        /// <summary>
        /// 从对象池中获取一个对象
        /// 根据配置可能返回null
        /// </summary>
        /// <returns></returns>
        public override IPooledObject Get()
        {
            return InternalGet();
        }

        /// <summary>
        /// 获取对象接口
        /// </summary>
        /// <param name="forceCreateNew"></param>
        /// <returns></returns>
        private IPooledObject InternalGet(bool forceCreateNew = false)
        {
            MonoPooledObject obj = null;
            if(forceCreateNew || m_UnusedObjects.Count == 0)
            {
                obj = CreateNew();
            }
            else
            {
                obj = m_UnusedObjects.Pop();
            }

            // 取出的对象默认放置在Group之下
            if (obj != null)
            {
                m_UsedObjects.AddLast(obj);
                obj.transform.parent = Group;
                obj.OnGet();
            }
            return obj;
        }

        /// <summary>
        /// Instantiate PrefabAsset
        /// </summary>
        /// <returns></returns>
        private MonoPooledObject CreateNew()
        {
            MonoPooledObject obj = null;
            if (PrefabAsset != null && (!LimitInstance || countAll < LimitAmount))
            {
                obj = (MonoPooledObject)PoolManagerEx.Instantiate(PrefabAsset);
                obj.Pool = this;
            }
            return obj;
        }

        /// <summary>
        /// 相比IPool.Get()接口，提供更丰富、使用简单的接口
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="pos"></param>
        /// <param name="rot"></param>
        /// <returns></returns>
        public MonoPooledObject Get(Transform parent, Vector3 pos = default(Vector3), Quaternion rot = default(Quaternion))
        {
            MonoPooledObject inst = Get() as MonoPooledObject;
            if (inst != null)
            {
                inst.transform.position = pos;
                inst.transform.rotation = rot;
                inst.transform.parent = parent != null ? parent : Group;
            }
            return inst;
        }

        public override void Return(IPooledObject item)
        {
            if (item == null)
                return;

            MonoPooledObject monoObj = (MonoPooledObject)item;
            m_UnusedObjects.Push(monoObj);
            m_UsedObjects.Remove(monoObj);
            monoObj.transform.parent = Group;           // 回收时放置Group下

            monoObj.OnRelease();

            // 基于性能考虑，仅回收时触发Trim
            TrimExcess();
        }

        /// <summary>
        /// 裁减未激活对象
        /// 注意：不是清空，不同于Clear
        /// </summary>
        private void TrimExcess()
        {
            if(TrimUnused && m_UnusedObjects.Count > TrimAbove)
            {
                while (m_UnusedObjects.Count > TrimAbove)
                {
                    MonoPooledObject inst = m_UnusedObjects.Pop();
                    if (inst != null)
                    {
                        PoolManagerEx.Destroy(inst.gameObject);
                    }
                }
            }
        }

        // 危：会清空当前使用和未使用中的所有对象
        public override void Clear()
        {
            Stack<MonoPooledObject>.Enumerator deactiveObjEnum = m_UnusedObjects.GetEnumerator();
            while(deactiveObjEnum.MoveNext())
            {
                if(deactiveObjEnum.Current != null)
                {
                    PoolManagerEx.Destroy(deactiveObjEnum.Current.gameObject);
                }
            }
            deactiveObjEnum.Dispose();
            m_UnusedObjects.Clear();

            BetterLinkedList<MonoPooledObject>.Enumerator activeObjEnum = m_UsedObjects.GetEnumerator();
            while(activeObjEnum.MoveNext())
            {
                if(activeObjEnum.Current != null)
                {
                    PoolManagerEx.Destroy(activeObjEnum.Current.gameObject);
                }
            }
            activeObjEnum.Dispose();
            m_UsedObjects.Clear();
        }

        public override void Trim()
        {
            TrimExcess();
        }

#if UNITY_EDITOR
        private void DisplayDebugInfo()
        {
            gameObject.name = string.Format("[Pool] {0}_{3} ({1}/{2})", PrefabAsset?.gameObject.name, countOfUsed, countAll, typeof(PrefabObjectPoolEx).Name);
        }
#endif
    }
}