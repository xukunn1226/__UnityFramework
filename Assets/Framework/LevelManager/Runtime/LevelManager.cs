using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.Runtime;
using System;
using UnityEngine.SceneManagement;

namespace Framework.LevelManager
{
    /// <summary>
    /// 场景管理器，负责场景之间切换逻辑
    /// </summary>
    public sealed class LevelManager : MonoBehaviour
    {
        public delegate void BeginLevelLoading(string levelIdentifier);
        public delegate void EndLevelLoading(string levelIdentifier);
        public delegate void UpdateLevelLoading(string levelIdentifier, float progress);

        public event BeginLevelLoading                  beginLevelLoading;
        public event EndLevelLoading                    endLevelLoading;
        public event UpdateLevelLoading                 updateLevelLoading;

        public class LoadLevelContext
        {
            public string                               identifier;                 // for display or identifying
            public string                               levelPath;                  // 为了兼容“静态场景”与“动态场景”设计接口为scenePath（带后缀名），且大小写敏感
            public bool                                 fromBundle;                 // true：从AB包加载；false：需在Build Setting中预设
            public string                               bundlePath;
            public bool                                 additive;                   // true: add模式加载场景；false：替换之前场景
            

            public bool                                 isMaster    { get; set; }   // is actived level
            public SceneLoaderAsync                     loader      { get; set; }
        }

        class LevelCommand
        {
            public bool                                 unload;                     // 加载 or 卸载场景指令

            public LoadLevelContext                     loadingContext;             // 加载场景指令数据

            public LevelCommand(bool unload, LoadLevelContext context)
            {
                this.unload = unload;
                this.loadingContext = context;
            }
        }
        private LinkedList<LevelCommand>                m_Commands = new LinkedList<LevelCommand>();

        private LoadLevelContext                        m_MasterLevel;
        private Dictionary<string, LoadLevelContext>    m_LevelsDict = new Dictionary<string, LoadLevelContext>();

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
                throw new Exception("LevelManager has already exist...");
            }
            m_kInstance = this;

            gameObject.name = "[LevelManager]";
            transform.parent = null;
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        private void OnDestroy()
        {
            m_kInstance = null;
        }

        void OnActiveSceneChanged(Scene oldScene, Scene newScene)
        {
            Debug.Log($"OnActiveSceneChanged: [{Time.frameCount}]    oldScene [{oldScene.name}]    newScene [{newScene.name}]");
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"OnSceneLoaded: [{Time.frameCount}]    Scene [{scene.name}]   Mode [{mode}]");
        }

        void OnSceneUnloaded(Scene scene)
        {
            Debug.Log($"OnSceneUnloaded: [{Time.frameCount}]    Scene [{scene.name}]");
        }

        public void LoadAsync(LoadLevelContext parameters)
        {
            if (parameters == null)
                throw new System.ArgumentNullException("parameters");

            if(parameters.additive)
            {
                SceneLoaderAsync loader = null;
                if (parameters.fromBundle)
                    loader = AssetManager.LoadSceneAsync(parameters.bundlePath, parameters.levelPath, LoadSceneMode.Additive);
                else
                    loader = AssetManager.LoadSceneAsync(parameters.levelPath, LoadSceneMode.Additive);
                parameters.loader = loader;
                //m_SlaveLevel.AddLast(parameters);

                //InternalBeginLevelLoading(parameters);
            }
            else
            {

            }
        }

        //private void InternalBeginLevelLoading(LoadLevelContext context)
        //{
        //    beginLevelLoading?.Invoke(context.identifier);
        //    //m_LoadingContext = context;
        //    StartCoroutine(context.loader);
        //}

        //private void InternalUpdateLevelLoading()
        //{
        //    if (m_LoadingContext == null)
        //        return;

        //    updateLevelLoading?.Invoke(m_LoadingContext.identifier, m_LoadingContext.loader.loadAsyncOp.progress);
        //}

        //private void InternalEndLevelLoading()
        //{
        //    if (m_LoadingContext == null)
        //        return;

        //    if(m_LoadingContext.loader.loadAsyncOp.isDone)
        //    {
        //        endLevelLoading?.Invoke(m_LoadingContext.identifier);
        //        m_LoadingContext = null;
        //    }
        //}

        //private void Update()
        //{
        //    InternalUpdateLevelLoading();

        //    InternalEndLevelLoading();
        //}


        //private IEnumerator UnloadAllSlaveLevelAsync()
        //{
        //    while(m_SlaveLevel.Count > 0)
        //    {
        //        yield return UnloadSlaveLevelAsync(m_SlaveLevel.First.Value);
        //    }
        //}

        //private IEnumerator UnloadSlaveLevelAsync(LoadLevelContext context)
        //{
        //    if (context == null || context.loader == null)
        //        throw new ArgumentNullException("context or context.loader");

        //    yield return AssetManager.UnloadSceneAsync(context.loader);
        //    m_SlaveLevel.Remove(context);
        //}

        public void UnloadLevelAsync(string identifier)
        {
            if (m_Commands.Count != 0)
                throw new System.InvalidOperationException("load commands havn't finished.");

            if (m_MasterLevel == null)
                throw new System.ArgumentNullException("m_MasterLevel");

            if (m_LevelsDict.Count == 0)
                throw new System.Exception("Unloading the last loaded scene is not supported");

            LoadLevelContext context;
            if(!m_LevelsDict.TryGetValue(identifier, out context))
                throw new System.Exception($"level {identifier} not loaded");

            m_Commands.AddLast(new LevelCommand(true, context));

            StartCoroutine(ExecuteCommands());
        }

        private IEnumerator ExecuteCommands()
        {
            while(m_Commands.Count > 0)
            {
                LevelCommand cmd = m_Commands.First.Value;
                
                if(cmd.unload)
                {
                    yield return UnloadLevelAsync(cmd);

                    // 指令执行完更新数据现场
                    m_LevelsDict.Remove(cmd.loadingContext.identifier);
                    m_Commands.RemoveFirst();
                }
                else
                {
                    yield return LoadLevelAsync(cmd);

                    // 指令执行完更新数据现场
                    m_LevelsDict.Add(cmd.loadingContext.identifier, cmd.loadingContext);
                    m_Commands.RemoveFirst();
                }
            }
        }

        // 执行异步卸载场景
        private IEnumerator UnloadLevelAsync(LevelCommand cmd)
        {
            if (cmd == null)
                throw new System.ArgumentNullException("cmd");

            if (cmd.loadingContext == null)
                throw new System.ArgumentNullException("cmd.loadingContext");

            if (cmd.loadingContext.loader == null)
                throw new System.ArgumentNullException("cmd.loadingContext.loader");

            yield return AssetManager.UnloadSceneAsync(cmd.loadingContext.loader);
        }

        // 执行异步加载场景
        private IEnumerator LoadLevelAsync(LevelCommand cmd)
        {
            if (cmd == null)
                throw new System.ArgumentNullException("cmd");

            LoadLevelContext context = cmd.loadingContext;
            if (context == null)
                throw new System.ArgumentNullException("cmd.loadingContext");

            SceneLoaderAsync loader;
            if (context.fromBundle)
                loader = AssetManager.LoadSceneAsync(context.bundlePath, context.levelPath, context.additive ? LoadSceneMode.Additive : LoadSceneMode.Single);
            else
                loader = AssetManager.LoadSceneAsync(context.levelPath, context.additive ? LoadSceneMode.Additive : LoadSceneMode.Single);
            context.loader = loader;
            yield return loader;
        }

        static private bool ParseLevelPath(string levelPath, out string bundlePath, out string levelName)
        {
            bundlePath = null;
            levelName = null;

            int index = levelPath.LastIndexOf(@"/");
            if (index == -1)
                return false;

            levelName = levelPath.Substring(index + 1);
            bundlePath = levelPath.Substring(0, index) + ".ab";
            return true;
        }
    }
}