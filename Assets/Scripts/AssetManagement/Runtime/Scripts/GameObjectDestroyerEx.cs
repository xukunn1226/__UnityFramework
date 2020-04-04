using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetManagement.Runtime
{
    public class GameObjectDestroyerEx : MonoBehaviour
    {
        internal AssetLoaderEx<GameObject>          loader      { get; set; }

        internal AssetLoaderAsyncEx<GameObject>     loaderAsync { get; set; }

        void OnDestroy()
        {
            if (loaderAsync != null)
            {
                AssetLoaderAsyncEx<GameObject>.Release(loaderAsync);
                loaderAsync = null;
            }

            if (loader != null)
            {
                AssetLoaderEx<GameObject>.Release(loader);
                loader = null;
            }
        }
    }
}