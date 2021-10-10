using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Cache
{
    /// <summary>
    /// 具有生命周期Prefab的缓存池
    /// </summary>
    public class LivingPrefabObjectPool : PrefabObjectPoolEx
    {
        public int      NormalSpeedLimitAmount = 20;

        public float    AmplifiedSpeed = 1;

        public override IPooledObject Get()
        {
            IPooledObject obj = base.Get();

            if(countOfUsed > NormalSpeedLimitAmount)
            {
                ILifeTime lifeObj = obj as ILifeTime;
                if (lifeObj != null)
                    lifeObj.Speed = AmplifiedSpeed;
            }

            return obj;
        }
    }
}