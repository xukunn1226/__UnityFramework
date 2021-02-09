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
    /// 规则：
    /// 1、additive模式加载：若前置任务有single模式加载的场景需等待其完成
    /// 2、single模式加载：等待加载中的场景加载完成再执行
    /// 3、卸载：此场景已加载完成
    /// </summary>
    public sealed class LevelManager : MonoBehaviour
    {
        public delegate void LevelCommandBegin(string identifier, bool isLoaded);
        public delegate void LevelCommandEnd(string identifier, bool isLoaded);
        
        static public event LevelCommandBegin           levelCommandBegin;
        static public event LevelCommandEnd             levelCommandEnd;
        
        internal enum StreamingState
        {
            InQueue,            // 队列中
            Streaming,          // 加载或卸载中
            Done,               // 加载或卸载完成
        }

        public class LevelContext
        {
            public string                               sceneName;                  // unique identifier
            public string                               scenePath;                  // 为了兼容“静态场景”与“动态场景”设计接口为scenePath（带后缀名），小写
            public string                               bundlePath;                 // 有效路径表示从AB包加载；null表示静态方式加载场景，需在Build Setting中预设
            public bool                                 additive;                   // true: add模式加载场景；false：替换之前场景            
            internal StreamingState                     state;
            internal SceneLoaderAsync                   loader;
            internal Scene                              scene;
            internal bool                               isMaster;                   // whether or not active scene
        }

        class LevelCommand
        {
            public bool                                 isLoad;                     // true: 加载场景; false:卸载场景

            public LevelContext                         loadingContext;             // 加载场景指令数据

            public LevelCommand(LevelContext context, bool isLoad)
            {
                this.isLoad = isLoad;
                this.loadingContext = context;
            }
        }
        private LinkedList<LevelCommand>                m_Commands = new LinkedList<LevelCommand>();

        private LevelContext                            m_MasterLevel;                                              // 当前激活的场景        
        private Dictionary<string, LevelContext>        m_LevelsList = new Dictionary<string, LevelContext>();
        
        private static LevelManager s_Instance;
        static public LevelManager Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    GameObject go = new GameObject();
                    s_Instance = go.AddComponent<LevelManager>();
                }
                return s_Instance;
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
            s_Instance = this;

            gameObject.name = "[LevelManager]";
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            DontDestroyOnLoad(gameObject);

            m_MasterLevel = new LevelContext() { sceneName = SceneManager.GetActiveScene().name, 
                                                 state = StreamingState.Done, 
                                                 isMaster = true};
            m_LevelsList.Add(m_MasterLevel.sceneName, m_MasterLevel);

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        private void OnDestroy()
        {
            s_Instance = null;
        }

        void OnActiveSceneChanged(Scene oldScene, Scene newScene)
        {
            Debug.Log($"OnActiveSceneChanged: [{Time.frameCount}]    oldScene [{oldScene.name}]    newScene [{newScene.name}]");

            // update newScene context
            LevelContext newContext = FindLevel(newScene.name);
            if(newContext == null)
                throw new Exception($"OnActiveSceneChanged: can't find newScene({newScene.name})");
            newContext.isMaster = true;

            // update oldScene context
            LevelContext oldContext = FindLevel(newScene.name);
            if(oldContext != null)
            { // oldScene可能已卸载
                oldContext.isMaster = false;
            }

            // update master level
            m_MasterLevel = newContext;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"OnSceneLoaded: [{Time.frameCount}]    Scene [{scene.name}]   Mode [{mode}]");

            LevelContext ctx = FindLevel(scene.name);
            if(ctx == null)
                throw new Exception($"OnSceneLoaded: can't find {scene.name}");

            ctx.scene = scene;
            ctx.state = StreamingState.Done;

            levelCommandEnd?.Invoke(scene.name, true);
        }

        void OnSceneUnloaded(Scene scene)
        {
            Debug.Log($"OnSceneUnloaded: [{Time.frameCount}]    Scene [{scene.name}]");

            LevelContext ctx = FindLevel(scene.name);
            if(ctx == null)
                throw new Exception($"OnSceneUnloaded: can't find {scene.name}");

            m_LevelsList.Remove(ctx.sceneName);           // 加载/卸载指令执行完成即删除
            levelCommandEnd?.Invoke(scene.name, false);
        }

        private LevelContext FindLevel(string sceneName)
        {
            if(m_MasterLevel == null)
                throw new ArgumentNullException("m_MasterLevel");

            LevelContext context;
            m_LevelsList.TryGetValue(sceneName, out context);
            return context;
        }

        public void SetActiveScene(string sceneName)
        {
            LevelContext ctx = FindLevel(sceneName);
            if(ctx == null)
                throw new Exception($"SetActiveScene: can't find {sceneName}");

            if(ctx.state != StreamingState.Done)
                throw new Exception($"can't set the scene active, because it not load completed.    {sceneName}");

            SceneManager.SetActiveScene(ctx.scene);
        }

        // Life Cycle: InQueue -> Streaming(load) -> Done -> Streaming(unload) -> Discard
        private void Update()
        {
            if(m_Commands.Count == 0)
                return;
            
            LevelCommand cmd = m_Commands.First.Value;
            if(cmd.isLoad)
            {
                bool waiting = false;
                if(cmd.loadingContext.additive)
                { // 若前置任务有single模式加载的场景，需等待其完成                    
                    foreach(var info in m_LevelsList)
                    {
                        if(!info.Value.additive && info.Value.state != StreamingState.Done)
                        {
                            waiting = true;
                            break;
                        }
                    }
                }
                else
                { // 等待加载中或卸载中的场景执行完，再执行
                    foreach(var info in m_LevelsList)
                    {
                        if(info.Value.state != StreamingState.Done)
                        {
                            waiting = true;
                            break;      // 前置有任务未完成是继续等待
                        }
                    }                    
                }

                if(!waiting)
                {
                    StartCoroutine(InternalLoadAsync(cmd.loadingContext));
                    m_Commands.RemoveFirst();
                    m_LevelsList.Add(cmd.loadingContext.sceneName, cmd.loadingContext);

                    levelCommandBegin?.Invoke(cmd.loadingContext.sceneName, true);
                }
            }
            else
            { // 卸载：必须此场景已加载完成
                LevelContext ctx = FindLevel(cmd.loadingContext.sceneName);
                if(ctx == null)
                    throw new ArgumentNullException("ctx");

                if (ctx.state == StreamingState.Done)
                {
                    StartCoroutine(InternalUnloadAsync(cmd.loadingContext));
                    m_Commands.RemoveFirst();

                    levelCommandBegin?.Invoke(cmd.loadingContext.sceneName, false);
                }
            }
        }

        public void LoadAsync(LevelContext context)
        {
            if (context == null)
                throw new System.ArgumentNullException("context");

            context.state = StreamingState.InQueue;
            context.loader = null;
            context.isMaster = false;
            m_Commands.AddLast(new LevelCommand(context, true));
        }

        public void UnloadAsync(LevelContext context)
        {
            if (context == null)
                throw new System.ArgumentNullException("context");

            m_Commands.AddLast(new LevelCommand(context, false));
        }

        // 执行异步卸载场景
        private IEnumerator InternalUnloadAsync(LevelContext context)
        {
            if (context == null)
                throw new System.ArgumentNullException("context");

            if (context.loader == null)
                throw new System.ArgumentNullException("context.loader");

            context.state = StreamingState.Streaming;
            yield return AssetManager.UnloadSceneAsync(context.loader);
        }

        // 执行异步加载场景
        private IEnumerator InternalLoadAsync(LevelContext context)
        {
            if (context == null)
                throw new System.ArgumentNullException("context");

            SceneLoaderAsync loader;
            if (!string.IsNullOrEmpty(context.bundlePath))
            { // extract Asset Name from scenePath when load scene from bundle
                string scenePath = System.IO.Path.GetFileNameWithoutExtension(context.scenePath);
                loader = AssetManager.LoadSceneAsync(context.bundlePath, scenePath, context.additive ? LoadSceneMode.Additive : LoadSceneMode.Single);
            }
            else
            {
                loader = AssetManager.LoadSceneAsync(context.scenePath, context.additive ? LoadSceneMode.Additive : LoadSceneMode.Single);
            }
            context.loader = loader;
            context.state = StreamingState.Streaming;
            yield return loader;
        }

        // static private bool ParseLevelPath(string levelPath, out string bundlePath, out string levelName)
        // {
        //     bundlePath = null;
        //     levelName = null;

        //     int index = levelPath.LastIndexOf(@"/");
        //     if (index == -1)
        //         return false;

        //     levelName = levelPath.Substring(index + 1);
        //     bundlePath = levelPath.Substring(0, index) + ".ab";
        //     return true;
        // }
    }
}