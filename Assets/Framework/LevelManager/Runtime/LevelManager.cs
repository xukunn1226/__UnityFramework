using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.Runtime;
using System;

namespace Framework.WorldManager
{
    /// <summary>
    /// 场景管理器，负责场景之间切换逻辑
    /// </summary>
    public sealed class LevelManager : MonoBehaviour
    {
        public delegate void BeginWorldLoading(string worldIdentifier);
        public delegate void EndWorldLoading(string worldIdentifier);
        public delegate void UpdateWorldLoading(string worldIdentifier, float progress);
        public delegate void WorldAssetLoaded(string worldIdentifier);

        public event BeginWorldLoading  beginWorldLoading;
        public event EndWorldLoading    endWorldLoading;
        public event UpdateWorldLoading updateWorldLoading;
        public event WorldAssetLoaded   worldAssetLoaded;           // 场景资源加载完成，当allowSceneActivation为false时才触发，否则直接触发endWorldLoading

        public class LoadWorldContext
        {
            public string               worldIdentifier;            // for display or identifying
            public string               worldPath;                  // 为了兼容“静态场景”与“动态场景”设计接口为scenePath（带后缀名），且大小写敏感
            public bool                 fromBundle;                 // true：从AB包加载；false：需在Build Setting中预设
            public bool                 additive;                   // true: add模式加载场景；false：替换之前场景
            public bool                 allowSceneActivation;       // see AsyncOperation.allowSceneActivation
            public Predicate<string>    additionalLoading;          // 当allowSceneActivation == false时需要制定其他加载是否完成的回调

            public SceneLoaderAsync     loader { get; private set; }
        }

        private LoadWorldContext        m_MasterWorld;
        private List<LoadWorldContext>  m_SlaveWorld;

        private static LevelManager m_kInstance;
        static public LevelManager Instance
        {
            get
            {
                if (m_kInstance == null)
                {
                    GameObject go = new GameObject();
                    m_kInstance = go.AddComponent<LevelManager>();
                }
                return m_kInstance;
            }
        }

        private void Awake()
        {
            // 已有AssetManager，则自毁
            if (FindObjectsOfType<LevelManager>().Length > 1)
            {
                DestroyImmediate(this);
                throw new Exception("WorldManager has already exist...");
            }
            m_kInstance = this;

            gameObject.name = "[WorldManager]";
            transform.parent = null;
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            m_kInstance = null;
        }

        public void LoadAsync(LoadWorldContext parameters)
        {
            if (parameters == null)
                throw new System.ArgumentNullException("parameters");

            if(!parameters.additive)
            {

            }

            m_MasterWorld = parameters;
        }

        private IEnumerator UnloadAsync()
        {
            if (m_MasterWorld == null)
                yield break;

            if (m_MasterWorld.loader == null)
                throw new System.ArgumentNullException("m_LoaderParameters.loader");

            yield return AssetManager.UnloadSceneAsync(m_MasterWorld.loader);
            m_MasterWorld = null;
        }
    }
}