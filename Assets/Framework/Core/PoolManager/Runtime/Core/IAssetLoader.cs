using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Cache
{
    /// <summary>
    /// load asset by asset path
    /// </summary>
    public interface IAssetLoader
    {
        GameObject asset { get; }

        GameObject Load(string assetPath);

        void Unload();
    }
}