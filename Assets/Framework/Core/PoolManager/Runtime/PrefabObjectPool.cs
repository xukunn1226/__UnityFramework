using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;

namespace Framework.Cache
{
    public class PrefabObjectPool : MonoPoolBase
    {
        [Tooltip("预实例化数量")]
        [Range(0, 100)]
        public int                                          PreAllocateAmount   = 1;                // 预实例化数量

        [Tooltip("是否设定实例化上限值")]
        public bool                                         LimitInstance;                          // 是否设定实例化上限值

        [Tooltip("最大实例化数量（激活与未激活）")]
        [Range(1, 1000)]
        public int                                          LimitAmount         = 20;               // 最大实例化数量（激活与未激活）

        [Tooltip("是否开启清理未激活实例功能")]
        public bool                                         TrimUnused;                             // 是否开启清理未激活实例功能

        [Tooltip("至少保持deactive的数量，当自动清理开启有效")]
        [Range(1, 100)]
        public int                                          TrimAbove           = 10;               // 自动清理开启时至少保持unused的数量

        protected BetterLinkedList<MonoPooledObjectBase>    m_UsedObjects       = new BetterLinkedList<MonoPooledObjectBase>();

        protected Stack<MonoPooledObjectBase>               m_UnusedObjects     = new Stack<MonoPooledObjectBase>();

        public override int                                 countAll            { get { return countOfUsed + countOfUnused; } }

        public override int                                 countOfUsed         { get { return m_UsedObjects.Count; } }

        public override int                                 countOfUnused       { get { return m_UnusedObjects.Count; } }

        private void Awake()
        {
            // 动态创建对象池时，PrefabAsset为空
            if(PrefabAsset != null)
                PoolManager.AddMonoPool(this);

            Init();
        }

        private void Start()
        {
            // 
            if (PoolManager.Instance.gameObject != transform.root.gameObject)
            {
                transform.parent = PoolManager.Instance.transform;
            }
        }

        /// <summary>
        /// 初始化
        /// 动态创建对象池时需要主动调用
        /// </summary>
        public void Init()
        {
            Warmup();
        }

        private void OnDestroy()
        {
            PoolManager.RemoveMonoPool(this);
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
        public override void Warmup()
        {
            if (PrefabAsset == null)        // 动态创建Pool时可能相关参数仍未设置
                return;

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
            MonoPooledObjectBase obj = null;
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
        private MonoPooledObjectBase CreateNew()
        {
            MonoPooledObjectBase obj = null;
            if (PrefabAsset != null && (!LimitInstance || countAll < LimitAmount))
            {
                obj = (MonoPooledObjectBase)PoolManager.Instantiate(PrefabAsset);
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
        public MonoPooledObjectBase Get(Transform parent, Vector3 pos = default(Vector3), Quaternion rot = default(Quaternion))
        {
            MonoPooledObjectBase inst = Get() as MonoPooledObjectBase;
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

            MonoPooledObjectBase monoObj = (MonoPooledObjectBase)item;
            m_UnusedObjects.Push(monoObj);
            m_UsedObjects.Remove(monoObj);
            monoObj.transform.parent = Group;           // 回收时放置Group下

            monoObj.OnRelease();

            if(TrimUnused && m_UnusedObjects.Count > TrimAbove)
            {
                TrimExcess();
            }
        }

        /// <summary>
        /// 裁减未激活对象
        /// 注意：不是清空，不同于Clear
        /// </summary>
        public void TrimExcess()
        {
            while(m_UnusedObjects.Count > TrimAbove)
            {
                MonoPooledObjectBase inst = m_UnusedObjects.Pop();
                if (inst != null)
                {
                    PoolManager.Destroy(inst.gameObject);
                }
            }
        }

        public override void Clear()
        {
            Stack<MonoPooledObjectBase>.Enumerator deactiveObjEnum = m_UnusedObjects.GetEnumerator();
            while(deactiveObjEnum.MoveNext())
            {
                if(deactiveObjEnum.Current != null)
                {
                    PoolManager.Destroy(deactiveObjEnum.Current.gameObject);
                }
            }
            deactiveObjEnum.Dispose();
            m_UnusedObjects.Clear();

            BetterLinkedList<MonoPooledObjectBase>.Enumerator activeObjEnum = m_UsedObjects.GetEnumerator();
            while(activeObjEnum.MoveNext())
            {
                if(activeObjEnum.Current != null)
                {
                    PoolManager.Destroy(activeObjEnum.Current.gameObject);
                }
            }
            activeObjEnum.Dispose();
            m_UsedObjects.Clear();

#if UNITY_EDITOR
            if(ScriptDynamicAdded && PrefabAsset != null)
            { // 编辑器下可能会运行时添加脚本，为了保持资源的一致性需要还原之前状态
                Object.DestroyImmediate(PrefabAsset, true);
            }
#endif
        }

        public override void Trim()
        {
            m_UnusedObjects.TrimExcess();
        }

#if UNITY_EDITOR
        private void DisplayDebugInfo()
        {
            gameObject.name = string.Format("[Pool]{0} ({1}/{2})", PrefabAsset?.gameObject.name, countOfUsed, countAll);
        }
#endif
    }
}