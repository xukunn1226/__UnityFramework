using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    public class PrefabDestroyer : MonoBehaviour
    {
        public PrefabLoader loader { get; set; }
        public PrefabLoaderAsync loaderAsync { get; set; }

        void OnDestroy()
        {
            if(loader != null)
            {
                PrefabLoader.Release(loader);
                loader = null;
            }
            if(loaderAsync != null)
            {
                PrefabLoaderAsync.Release(loaderAsync);
                loaderAsync = null;
            }
        }
    }
}