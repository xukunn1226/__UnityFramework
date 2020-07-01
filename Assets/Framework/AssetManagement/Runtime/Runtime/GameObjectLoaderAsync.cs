using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Cache;

namespace Framework.AssetManagement.Runtime
{
    public class GameObjectLoaderAsync : IEnumerator, ILinkedObjectPoolNode<GameObjectLoaderAsync>, IPooledObject
    {
        public GameObject               asset       { get; internal set; }

        
        // private bool IsDone()
        // {
        //     if (m_Request == null)
        //         return true;

        //     if (m_Request.isDone)
        //         asset = m_Request.asset as T;
        //     return m_Request.isDone;
        // }

        object IEnumerator.Current
        {
            get { return asset; }
        }

        bool IEnumerator.MoveNext()
        {
            // return !IsDone();
            return false;
        }

        void IEnumerator.Reset()
        {
        }

        LinkedObjectPool<GameObjectLoaderAsync>       ILinkedObjectPoolNode<GameObjectLoaderAsync>.List     { get; set; }

        ILinkedObjectPoolNode<GameObjectLoaderAsync>  ILinkedObjectPoolNode<GameObjectLoaderAsync>.Next     { get; set; }

        ILinkedObjectPoolNode<GameObjectLoaderAsync>  ILinkedObjectPoolNode<GameObjectLoaderAsync>.Prev     { get; set; }

        void IPooledObject.OnInit() { }

        /// <summary>
        /// 从对象池中拿出时的回调
        /// </summary>
        void IPooledObject.OnGet() { }

        /// <summary>
        /// 放回对象池时的回调
        /// </summary>
        void IPooledObject.OnRelease()
        {
            // Unload();
            Pool = null;
        }

        /// <summary>
        /// 放回对象池
        /// </summary>
        void IPooledObject.ReturnToPool()
        {
            Pool?.Return(this);
        }

        public IPool Pool { get; set; }
    }
}