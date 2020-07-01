using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Cache;

namespace Framework.AssetManagement.Runtime
{
    public class GameObjectLoaderAsync : IEnumerator, ILinkedObjectPoolNode<GameObjectLoaderAsync>, IPooledObject
    {
        static private LinkedObjectPool<GameObjectLoaderAsync>    m_Pool;
        public static LinkedObjectPool<GameObjectLoaderAsync>     kPool { get { return m_Pool; } }

        private AssetLoaderAsync<GameObject> assetLoaderAsync { get; set; }

        public GameObject               asset       { get; internal set; }

        public GameObjectLoaderAsync()
        {
            assetLoaderAsync = null;
            asset = null;
        }

        static internal GameObjectLoaderAsync Get(string assetPath)
        {
            if (m_Pool == null)
            {
                m_Pool = new LinkedObjectPool<GameObjectLoaderAsync>(AssetManager.PreAllocateAssetLoaderPoolSize);
            }

            GameObjectLoaderAsync loader = (GameObjectLoaderAsync)m_Pool.Get();
            loader.assetLoaderAsync = AssetLoaderAsync<GameObject>.Get(assetPath);
            if(loader.assetLoaderAsync.asset != null)
            {
                loader.asset = Object.Instantiate(loader.assetLoaderAsync.asset);
            }
            return loader;
        }
        
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