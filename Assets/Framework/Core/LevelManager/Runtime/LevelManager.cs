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
    /// NOTE: sceneName can't repeat
    /// 规则：
    /// 1、additive模式加载：可控制是否并发执行，若前置任务有single模式加载的场景需等待其完成
    /// 2、single模式加载：等待加载中的场景加载完成再执行
    /// </summary>
    public sealed class LevelManager : MonoBehaviour
    {
        static public bool s_StreamingConcurrentlyWhenLoading = true;          // 是否并发执行多个场景的异步加载
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
            internal bool                               isFirst;                    // 第一个场景不是由LevelManager载入，需要特殊处理
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

        private LevelContext                            m_MasterLevel;              // 当前激活的场景
        private Dictionary<string, LevelContext>        m_LevelsDict = new Dictionary<string, LevelContext>();      // 所有场景的集合

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
                                                 isMaster = true, 
                                                 isFirst = true };
            m_LevelsDict.Add(m_MasterLevel.sceneName, m_MasterLevel);

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
            // if(mode == LoadSceneMode.Additive)
            // {
            //     ctx.isMaster = false;
            // }
            // else
            // {
            //     ctx.isMaster = true;
            //     m_MasterLevel = ctx;
            // }
        }

        void OnSceneUnloaded(Scene scene)
        {
           Debug.Log($"OnSceneUnloaded: [{Time.frameCount}]    Scene [{scene.name}]");
        }

        private LevelContext FindLevel(string sceneName)
        {
            if(m_MasterLevel == null)
                throw new ArgumentNullException("m_MasterLevel");

            if(m_MasterLevel.sceneName == sceneName)
                return m_MasterLevel;

            LevelContext context;
            m_LevelsDict.TryGetValue(sceneName, out context);
            return context;
        }

        public void SetActiveScene(string sceneName)
        {
            LevelContext ctx = FindLevel(sceneName);
            if(ctx == null)
                throw new Exception($"SetActiveScene: can't find {sceneName}");

            SceneManager.SetActiveScene(ctx.scene);
        }

/// 1、additive模式加载：可控制是否并发执行，若前置任务有single模式加载的场景需等待其完成
/// 2、single模式加载：等待加载中的场景加载完成或卸载中的场景卸载完成，再执行
        public void LoadAsync(LevelContext context)
        {
            if(m_MasterLevel == null)
                throw new Exception($"m_MasterLevel == null");
            
            if (context == null)
                throw new System.ArgumentNullException("context");

            // if (FindLevel(context.sceneName) != null)
            //     throw new Exception($"{context.sceneName} has already loaded");

            // single模式加载时需要卸载add模式加载的场景
            if(!context.additive)
            {
                foreach(var ctx in m_LevelsDict)
                {
                    if(ctx.Value.state == StreamingState.Done)
                        m_Commands.AddLast(new LevelCommand(ctx.Value, false));
                }
            }
            m_Commands.AddLast(new LevelCommand(context, true));

            StartCoroutine(ExecuteCommands(context.sceneName, true));
        }

        public void UnloadLevelAsync(string sceneName)
        {
            if (m_Commands.Count != 0)
                throw new System.InvalidOperationException("load commands havn't finished.");

            if (m_MasterLevel == null)
                throw new System.ArgumentNullException("m_MasterLevel");

            if (m_LevelsDict.Count == 1)
                throw new System.Exception("Unloading the last loaded scene is not supported");

            LevelContext context;
            if(!m_LevelsDict.TryGetValue(sceneName, out context))
                throw new System.Exception($"level {sceneName} not loaded");

            m_Commands.AddLast(new LevelCommand(context, false));

            StartCoroutine(ExecuteCommands(sceneName, false));
        }

        private IEnumerator ExecuteCommands(string sceneName, bool isLoaded)
        {
            levelCommandBegin?.Invoke(sceneName, isLoaded);

            while(m_Commands.Count > 0)
            {
                LevelCommand cmd = m_Commands.First.Value;
                
                if(cmd.isLoad)
                {
                    yield return UnloadLevelAsync(cmd);

                    // 指令执行完更新数据现场
                    m_LevelsDict.Remove(cmd.loadingContext.sceneName);
                    m_Commands.RemoveFirst();
                }
                else
                {
                    yield return LoadLevelAsync(cmd);

                    // 指令执行完更新数据现场
                    m_LevelsDict.Add(cmd.loadingContext.sceneName, cmd.loadingContext);
                    m_Commands.RemoveFirst();
                }
            }

            levelCommandEnd?.Invoke(sceneName, isLoaded);
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

            LevelContext context = cmd.loadingContext;
            if (context == null)
                throw new System.ArgumentNullException("cmd.loadingContext");

            SceneLoaderAsync loader;
            if (!string.IsNullOrEmpty(context.bundlePath))
                loader = AssetManager.LoadSceneAsync(context.bundlePath, context.scenePath, context.additive ? LoadSceneMode.Additive : LoadSceneMode.Single);
            else
                loader = AssetManager.LoadSceneAsync(context.scenePath, context.additive ? LoadSceneMode.Additive : LoadSceneMode.Single);
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