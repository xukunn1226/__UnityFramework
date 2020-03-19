using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CacheMech;
using UnityEditor;

namespace CacheMech
{
    public partial class PoolManager
    {
        /// <summary>
        /// wrapper of GetOrCreatePool, use default "AssetLoader"
        /// note: int i = 0, ugly code because of compile error
        /// </summary>
        /// <typeparam name="TPooledObject"></typeparam>
        /// <typeparam name="TPool"></typeparam>
        /// <param name="assetPath"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        static public TPool GetOrCreatePool<TPooledObject, TPool>(string assetPath, int i = 0) where TPooledObject : MonoPooledObjectBase where TPool : MonoPoolBase
        {
            return GetOrCreatePool<TPooledObject, TPool, AssetLoader>(assetPath);
        }

        static public PrefabObjectPool GetOrCreatePool<TPooledObject>(string assetPath) where TPooledObject : MonoPooledObjectBase
        {
            return GetOrCreatePool<TPooledObject, AssetLoader>(assetPath);
        }
    }
}




public class AssetLoader : IAssetLoaderProxy
{
    private UnityEngine.Object m_Asset;

    public UnityEngine.Object asset { get { return m_Asset; } }

    public UnityEngine.Object Load(string assetPath)
    {
        m_Asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
        return m_Asset;
    }

    public void Unload()
    {
    }
}