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
        public delegate void LevelCommandBegin(string identifier, bool isLoaded);
        public delegate void LevelCommandEnd(string identifier, bool isLoaded);
        
        static public event LevelCommandBegin           levelCommandBegin;
        static public event LevelCommandEnd             levelCommandEnd;
        
        public class LoadLevelContext
        {
            public string                               identifier;                 // for display or identifying
            public string                               levelPath;                  // 为了兼容“静态场景”与“动态场景”设计接口为scenePath（带后缀名），且大小写敏感
            public bool                                 fromBundle;                 // true：从AB包加载；false：需在Build Setting中预设
            public string                               bundlePath;
            public bool                                 additive;                   // true: add模式加载场景；false：替换之前场景
            
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

        public void LoadAsync(LoadLevelContext context)
        {
            if (m_Commands.Count != 0)
                throw new System.InvalidOperationException("load commands havn't finished.");
            
            if (context == null)
                throw new System.ArgumentNullException("parameters");

            if (m_LevelsDict.ContainsKey(context.identifier))
                throw new Exception($"{context.identifier} has already loaded");

            if(m_MasterLevel == null)
            { // 首次加载场景时强制以single方式加载
                context.additive = false;
            }

            if(context.additive)
            {
                m_Commands.AddLast(new LevelCommand(false, context));
            }
            else
            {
                context.additive = m_MasterLevel == null ? false : true;        // 非首次加载强制以add方式加载
                m_Commands.AddLast(new LevelCommand(false, context));
                foreach(var ctx in m_LevelsDict)
                {
                    m_Commands.AddLast(new LevelCommand(true, ctx.Value));
                }
                m_MasterLevel = context;
            }

            StartCoroutine(ExecuteCommands(context.identifier, true));
        }

        public void UnloadLevelAsync(string identifier)
        {
            if (m_Commands.Count != 0)
                throw new System.InvalidOperationException("load commands havn't finished.");

            if (m_MasterLevel == null)
                throw new System.ArgumentNullException("m_MasterLevel");

            if (m_LevelsDict.Count == 1)
                throw new System.Exception("Unloading the last loaded scene is not supported");

            LoadLevelContext context;
            if(!m_LevelsDict.TryGetValue(identifier, out context))
                throw new System.Exception($"level {identifier} not loaded");

            m_Commands.AddLast(new LevelCommand(true, context));

            StartCoroutine(ExecuteCommands(identifier, false));
        }

        private IEnumerator ExecuteCommands(string identifier, bool isLoaded)
        {
            levelCommandBegin?.Invoke(identifier, isLoaded);

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

            levelCommandEnd?.Invoke(identifier, isLoaded);
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