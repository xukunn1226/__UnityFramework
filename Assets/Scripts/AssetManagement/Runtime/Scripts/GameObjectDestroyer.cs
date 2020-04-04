using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetManagement.Runtime
{
    public class GameObjectDestroyer : MonoBehaviour
    {
        internal AssetLoader<GameObject>          loader      { get; set; }

        internal AssetLoaderAsync<GameObject>     loaderAsync { get; set; }

        void OnDestroy()
        {
            if (loaderAsync != null)
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