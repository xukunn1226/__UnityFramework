using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Cache
{
    public abstract class LRUMonoPool : MonoBehaviour, IPool
    {
        public int countAll         { get; }

        public int countOfUsed      { get; }

        public int countOfUnused    { get; }

        /// <summary>
        /// 获取缓存对象
        /// </summary>
        /// <returns></returns>
        public IPooledObject Get() { throw new System.NotImplementedException(); }

        /// <summary>
        /// 返回缓存对象
        /// </summary>
        /// <param name="item"></param>
        public abstract void Return(IPooledObject item);


        /// <summary>
        /// 清空缓存池对象
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// trim excess data
        /// </summary>
        void IPool.Trim() {}
    }
}