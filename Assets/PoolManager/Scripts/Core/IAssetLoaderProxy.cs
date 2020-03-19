using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cache
{
    /// <summary>
    /// load asset by asset path
    /// </summary>
    public interface IAssetLoaderProxy
    {
        UnityEngine.Object asset { get; }

        UnityEngine.Object Load(string assetPath);

        void Unload();
    }
}