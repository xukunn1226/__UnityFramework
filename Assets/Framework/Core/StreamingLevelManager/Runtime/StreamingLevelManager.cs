using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.Runtime;
using System;
using UnityEngine.SceneManagement;

namespace Framework.Core
{
    /// <summary>
    /// 场景管理器，负责场景之间切换逻辑
    /// 注意事项：
    /// 0、场景名小写，且不能重名
    /// 1、加载（additive）：若前置任务有single模式加载的场景需等待其完成
    /// 2、加载（single）：等待正在加载中的场景加载完成再执行
    /// 3、卸载：此场景已加载完成
    /// </summary>
    public sealed class StreamingLevelManager : SingletonMono<StreamingLevelManager>
    {
        static public Action<string>      onLevelLoadBegin;
        static public Action<string>      onLevelLoadEnd;
        static public Action<string>      onLevelUnloadBegin;
        static public Action<string>      onLevelUnloadEnd;
        
        // LoadScene: InQueue -> Streaming -> Done
        // UnloadScene: Done -> Streaming -> Discard
        internal enum StreamingState
        {
            InQueue,            // 队列中
            Streaming,          // 加载或卸载中
            Done,               // 加载完成
            Discard,            // 卸载完成
        }

        public class LevelContext
        {
            public string                               sceneName;                  // unique identifier
            public string                               scenePath;                  // 为了兼容“静态场景”与“动态场景”设计接口为scenePath（带后缀名），小写
            public bool                                 fromBundle;                 // 有效路径表示从AB包加载；null表示静态方式加载场景，需在Build Setting中预设
            public bool                                 additive;                   // true: add模式加载场景；false：替换之前场景
            private StreamingState                      m_State;
            internal StreamingState                     state
            {
                get
                {
                    return m_State;
                }
                set
                {
                    if(m_State != value)
                    {
                        StreamingState prevState = m_State;
                        m_State = value;

                        // LoadScene
                        if(prevState == StreamingState.InQueue && m_State == StreamingState.Streaming)
                        {
                            onLevelLoadBegin?.Invoke(sceneName);
                        }
                        if(prevState == StreamingState.Streaming && m_State == StreamingState.Done)
                        {
                            onLevelLoadEnd?.Invoke(sceneName);
                        }

                        // UnloadScene
                        if(prevState == StreamingState.Done && m_State == StreamingState.Streaming)
                        {
                            onLevelUnloadBegin?.Invoke(sceneName);
                        }
                        if(prevState == StreamingState.Streaming && m_State == StreamingState.Discard)
                        {
                            onLevelUnloadEnd?.Invoke(sceneName);
                        }
                    }
                }
            }
            internal SceneLoaderAsync                   loader;
            internal Scene                              scene;
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
        private Dictionary<string, LevelContext>        m_LevelsList = new Dictionary<string, LevelContext>();      // 当前加载中、卸载中、加载完成的场景列表

        protected override void Awake()
        {
            base.Awake();

            m_MasterLevel = new LevelContext() { sceneName = SceneManager.GetActiveScene().name, 
                                                 state = StreamingState.Done };
            
            m_LevelsList.Add(m_MasterLevel.sceneName, m_MasterLevel);

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        protected override void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            base.OnDestroy();
        }

        void OnActiveSceneChanged(Scene oldScene, Scene newScene)
        {
            Debug.Log($"OnActiveSceneChanged: [{Time.frameCount}]    oldScene [{oldScene.name}]    newScene [{newScene.name}]");

            // update newScene context
            LevelContext newContext = FindLevel(newScene.name);
            if(newContext == null)
                throw new Exception($"OnActiveSceneChanged: can't find newScene({newScene.name})");

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
        }

        void OnSceneUnloaded(Scene scene)
        {
            Debug.Log($"OnSceneUnloaded: [{Time.frameCount}]    Scene [{scene.name}]");

            LevelContext ctx = FindLevel(scene.name);
            if(ctx == null)
                throw new Exception($"OnSceneUnloaded: can't find {scene.name}");

            ctx.state = StreamingState.Discard;

            m_LevelsList.Remove(ctx.sceneName);           // 加载/卸载指令执行完成即删除
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
                    if(FindLevel(cmd.loadingContext.sceneName) == null)
                    {
                        m_LevelsList.Add(cmd.loadingContext.sceneName, cmd.loadingContext);
                        StartCoroutine(InternalLoadAsync(cmd.loadingContext));
                    }
                    else
                    {
                        Debug.LogWarning($"Scene({cmd.loadingContext.sceneName}) has already loaded.");
                    }
                    m_Commands.RemoveFirst();
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
                }
            }
        }

        public void LoadAsync(LevelContext context)
        {
            if (context == null)
                throw new System.ArgumentNullException("context");

            context.state = StreamingState.InQueue;
            context.loader = null;
            m_Commands.AddLast(new LevelCommand(context, true));
        }

        public void UnloadAsync(LevelContext context)
        {
            if (context == null)
                throw new System.ArgumentNullException("context");

            // context.state = StreamingState.InQueue;
            m_Commands.AddLast(new LevelCommand(context, false));
        }

        // 执行异步卸载场景
        private IEnumerator InternalUnloadAsync(LevelContext context)
        {
            if (context == null)
                throw new System.ArgumentNullException("context");

            if (context.loader == null)
                throw new System.ArgumentNullException("context.loader");

            Debug.Assert(context.state == StreamingState.Done);

            context.state = StreamingState.Streaming;
            yield return AssetManager.UnloadSceneAsync(context.loader);
        }

        // 执行异步加载场景
        private IEnumerator InternalLoadAsync(LevelContext context)
        {
            if (context == null)
                throw new System.ArgumentNullException("context");
            Debug.Assert(context.state == StreamingState.InQueue);

            SceneLoaderAsync loader;
            if (context.fromBundle)
            { // extract Asset Name from scenePath when load scene from bundle
                loader = AssetManager.LoadSceneFromBundleAsync(context.scenePath, context.additive ? LoadSceneMode.Additive : LoadSceneMode.Single);
            }
            else
            {
                loader = AssetManager.LoadSceneAsync(context.scenePath, context.additive ? LoadSceneMode.Additive : LoadSceneMode.Single);
            }
            context.loader = loader;
            context.state = StreamingState.Streaming;
            yield return loader;
        }
    }
}