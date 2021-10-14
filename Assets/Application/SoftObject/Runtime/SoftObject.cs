using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;
using Framework.AssetManagement.Runtime;
using Framework.Cache;
using Object = UnityEngine.Object;
// #if UNITY_EDITOR
// using UnityEditor;
// using Framework.Core.Editor;
// #endif

namespace Application.Runtime
{
    public sealed class SoftObject : SoftObjectPath
    {
        private GameObjectLoader            m_PrefabLoader;
        private GameObjectLoaderAsync       m_PrefabLoaderAsync;
        private AssetLoader<Object>         m_Loader;
        private AssetLoaderAsync<Object>    m_LoaderAsync;

        private MonoPoolBase                m_ScriptedPool;         // 脚本创建的对象池
        private string                      m_Path;

        public GameObject Instantiate()
        {
            // 已异步加载，不能再次加载
            if (m_PrefabLoaderAsync != null)
                throw new System.InvalidOperationException($"{bundleName}:{assetName} has already loaded async");

            // 已同步加载，不能再次加载
            if (m_PrefabLoader != null)
                throw new System.InvalidOperationException($"{bundleName}:{assetName} has already loaded, plz unload it");

            m_PrefabLoader = AssetManager.Instantiate(bundleName, assetName);
            return m_PrefabLoader?.asset;
        }

        public GameObjectLoaderAsync InstantiateAsync()
        {
            // 已异步加载，不能再次加载
            if (m_PrefabLoaderAsync != null)
                throw new System.InvalidOperationException($"{bundleName}:{assetName} has already loaded async");

            // 已同步加载，不能再次加载
            if (m_PrefabLoader != null)
                throw new System.InvalidOperationException($"{bundleName}:{assetName} has already loaded, plz unload it");

            m_PrefabLoaderAsync = AssetManager.InstantiateAsync(bundleName, assetName);
            return m_PrefabLoaderAsync;
        }

        public void ReleaseInst()
        {
            if (m_PrefabLoader != null)
            {
                AssetManager.ReleaseInst(m_PrefabLoader);
                m_PrefabLoader = null;
            }

            if (m_PrefabLoaderAsync != null)
            {
                AssetManager.ReleaseInst(m_PrefabLoaderAsync);
                m_PrefabLoaderAsync = null;
            }
        }

        public Object LoadAsset()
        {
            // 已同步加载，不能再次加载
            if (m_Loader != null)
                throw new System.InvalidOperationException($"{bundleName}:{assetName} has already loaded, plz unload it");

            // 已异步加载，不能再次加载
            if (m_LoaderAsync != null)
                throw new System.InvalidOperationException($"{bundleName}:{assetName} has already loaded async");

            m_Loader = AssetManager.LoadAsset<Object>(bundleName, assetName);
            return m_Loader.asset;
        }

        public AssetLoaderAsync<Object> LoadAssetAsync()
        {
            // 已异步加载，不能再次加载
            if (m_LoaderAsync != null)
                throw new System.InvalidOperationException($"{bundleName}:{assetName} has already loaded async");

            // 已同步加载，不能再次加载
            if (m_Loader != null)
                throw new System.InvalidOperationException($"{bundleName}:{assetName} has already loaded, plz unload it");

            m_LoaderAsync = AssetManager.LoadAssetAsync<Object>(bundleName, assetName);
            return m_LoaderAsync;
        }

        public void UnloadAsset()
        {
            // 资源不能以两种方式同时加载
            if (m_Loader != null && m_LoaderAsync != null)
                throw new System.Exception("m_Loader != null && m_LoaderAsync != null");

            if (m_Loader != null)
            {
                AssetManager.UnloadAsset(m_Loader);
                m_Loader = null;
            }

            if (m_LoaderAsync != null)
            {
                AssetManager.UnloadAsset(m_LoaderAsync);
                m_LoaderAsync = null;
            }
        }

        /// <summary>
        /// 从对象池中创建对象（对象池由脚本创建）
        /// </summary>
        /// <typeparam name="TPooledObject"></typeparam>
        /// <typeparam name="TPool"></typeparam>
        /// <returns></returns>
        public IPooledObject SpawnFromPool<TPooledObject, TPool>() where TPooledObject : MonoPooledObject where TPool : MonoPoolBase
        {
            if (m_ScriptedPool == null)
            {
                AssetManager.ParseBundleAndAssetName(bundleName, assetName, out m_Path);
                #if UNITY_EDITOR
                Debug.Assert(UnityEditor.AssetDatabase.LoadAssetAtPath<Object>(m_Path) != null);
                #endif
                m_ScriptedPool = PoolManager.GetOrCreatePool<TPooledObject, TPool>(m_Path);
            }
            return m_ScriptedPool.Get();
        }

        public IPooledObject SpawnFromPool<TPooledObject>() where TPooledObject : MonoPooledObject
        {
            return SpawnFromPool<TPooledObject, PrefabObjectPool>();
        }

        public IPooledObject SpawnFromPool()
        {
            return SpawnFromPool<MonoPooledObject, PrefabObjectPool>();
        }

        /// <summary>
        /// 销毁对象池，与SpawnFromPool对应
        /// </summary>
        public void DestroyPool()
        {
            if (m_ScriptedPool == null)
                throw new System.ArgumentNullException("Pool", "Scripted Pool not initialize");

            PoolManager.RemoveMonoPool(m_Path);
        }
    }

// #if UNITY_EDITOR

// [CustomEditor(typeof(SoftObject))]
// public class SoftObjectEditor : SoftObjectPathEditor
// {
//     private SerializedProperty m_LRUedPoolAssetProp;
//     private SerializedProperty m_UseLRUManageProp;

//     public override void OnEnable()
//     {
//         base.OnEnable();
//         m_LRUedPoolAssetProp = serializedObject.FindProperty("m_LRUedPoolAsset");
//         m_UseLRUManageProp = serializedObject.FindProperty("m_UseLRUManage");
//     }

//     public override void OnInspectorGUI()
//     {
//         base.OnInspectorGUI();

//         EditorGUILayout.Separator();

//         serializedObject.Update();

//         m_UseLRUManageProp.boolValue = EditorGUILayout.Toggle("Use LRU", m_UseLRUManageProp.boolValue);

//         EditorGUI.BeginDisabledGroup(!m_UseLRUManageProp.boolValue);
//         EditorGUILayout.ObjectField(m_LRUedPoolAssetProp, new GUIContent("LRU Pool", "If you would use LRU Pool manage the asset"));
//         EditorGUI.EndDisabledGroup();

//         serializedObject.ApplyModifiedProperties();
//     }
// }

// #endif
}