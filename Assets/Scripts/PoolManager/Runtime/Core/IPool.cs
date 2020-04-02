﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cache
{
    /// <summary>
    /// 缓存池对象接口
    /// </summary>
    public interface IPool
    {
        int countAll        { get; }

        int countOfUsed     { get; }

        int countOfUnused   { get; }

        /// <summary>
        /// 获取缓存对象
        /// </summary>
        /// <returns></returns>
        IPooledObject Get();

        /// <summary>
        /// 返回缓存对象
        /// </summary>
        /// <param name="item"></param>
        void Return(IPooledObject item);

        /// <summary>
        /// 清空缓存池对象
        /// </summary>
        void Clear();

        /// <summary>
        /// trim excess data
        /// </summary>
        void Trim();
    }
}