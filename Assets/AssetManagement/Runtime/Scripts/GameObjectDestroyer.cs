using System.Collections.Generic;
using UnityEngine;

namespace AssetManagement.Runtime
{
    public class GameObjectDestroyer : MonoBehaviour
    {
        public LinkedListNode<AssetLoader<GameObject>>          loader      { get; set; }
    
        public LinkedListNode<AssetLoaderAsync<GameObject>>     loaderAsync { get; set; }
        
        void OnDestroy()
        {
            if(loaderAsync != null)
            {
                AssetLoaderAsync<GameObject>.Release(loaderAsync);
                loaderAsync = null;
            }

            if (loader != null)
            {
                AssetLoader<GameObject>.Release(loader);
                loader = null;
            }
        }
    }
}