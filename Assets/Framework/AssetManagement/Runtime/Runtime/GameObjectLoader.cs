using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Cache;

namespace Framework.AssetManagement.Runtime
{
    public class GameObjectLoader : ILinkedObjectPoolNode<GameObjectLoader>, IPooledObject
    {
        static private LinkedObjectPool<GameObjectLoader>       m_Pool;
        static public LinkedObjectPool<GameObjectLoader>        kPool   { get { return m_Pool; } }

        public GameObject               asset       { get; internal set; }

        private AssetLoader<GameObject> assetLoader { get; set; }

        static internal GameObjectLoader Get(string assetPath)
        {
            if (m_Pool == null)
            {
                m_Pool = new LinkedObjectPool<GameObjectLoader>(AssetManager.PreAllocateAssetLoaderPoolSize);
            }

            GameObjectLoader loader = (GameObjectLoader)m_Pool.Get();
            loader.assetLoader = AssetLoader<GameObject>.Get(assetPath);
            if(loader.assetLoader.asset != null)
            {
                loader.asset = Object.Instantiate(loader.assetLoader.asset);
            }
            else
            {
                loader.Unload();
            }
            return loader;
        }

        static internal void Release(GameObjectLoader loader)
        {
            if (m_Pool == null || loader == null)
                throw new System.ArgumentNullException();

            m_Pool.Return(loader);
        }

        private void Unload()
        {
            if(asset != null)
            {
                Object.Destroy(asset);
                asset = null;
            }

            if(assetLoader != null)
            {
                AssetLoader<GameObject>.Release(assetLoader);
                assetLoader = null;
            }
        }

        
        LinkedObjectPool<GameObjectLoader>        ILinkedObjectPoolNode<GameObjectLoader>.List      { get; set; }

        ILinkedObjectPoolNode<GameObjectLoader>   ILinkedObjectPoolNode<GameObjectLoader>.Next      { get; set; }

        ILinkedObjectPoolNode<GameObjectLoader>   ILinkedObjectPoolNode<GameObjectLoader>.Prev      { get; set; }

        
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
            Unload();
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