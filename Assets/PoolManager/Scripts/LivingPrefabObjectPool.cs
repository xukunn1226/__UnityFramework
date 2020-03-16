using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    /// <summary>
    /// 具有生命周期Prefab的缓存池
    /// </summary>
    public class LivingPrefabObjectPool : PrefabObjectPool
    {
        //public int LimitAmount = 20;
        public float Speed = 1;

        //public override IPooledObject Get()
        //{
        //    if (PrefabAsset == null)
        //    {
        //        Debug.LogError("LivingPrefabObjectPool::Get() return null, because of PrefabAsset == null");
        //        return null;
        //    }

        //    LivingPooledObject obj;
        //    if (m_DeactiveObjects.Count > 0)
        //    {
        //        obj = m_DeactiveObjects[m_DeactiveObjects.Count - 1];
        //        m_DeactiveObjects.RemoveAt(m_DeactiveObjects.Count - 1);
        //    }
        //    else
        //    {
        //        obj = (LivingPooledObject)PoolManager.Instantiate(PrefabAsset);
        //        obj.Pool = this;
        //        ++countAll;
        //    }

        //    if (obj != null)
        //    {                
        //        obj.transform.parent = Group;       // 每次取出时默认放置Group下，因为parent可能会被改变
        //        obj.OnGet();

        //        if(countActive > LimitAmount)
        //        {
        //            obj.SetSpeed(Speed);
        //        }
        //    }

        //    return obj;
        //}
    }
}