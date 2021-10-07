﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Cache;

namespace Tests
{
    public class UILRUPooledObject : MonoBehaviour, IPooledObject
    {
        /// <summary>
        /// 创建时的回调
        /// </summary>
        void IPooledObject.OnInit()
        { }

        /// <summary>
        /// 从对象池中拿出时的回调
        /// </summary>
        public void OnGet()
        {
            // gameObject.SetActive(true);
        }

        /// <summary>
        /// 放回对象池时的回调
        /// </summary>
        public void OnRelease()
        {
            gameObject.SetActive(false);
        }

        public void OnTouch()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// 放回对象池
        /// </summary>
        void IPooledObject.ReturnToPool()
        {
            throw new System.NotImplementedException();
        }

        IPool IPooledObject.Pool { get; set; }

    }
}