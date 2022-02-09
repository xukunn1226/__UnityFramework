using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Framework.Cache;
using Framework.Core;
using Application.Runtime;

namespace Application.Logic
{
    /// <summary>
    /// 界面管理器
    ///     * 层级管理：Main, Fullscreen, Windowed, Tips, Loading, Alert
    ///     * UI资源生命周期管理：常驻内存（persistent pool）或lru pool
    /// 问题：
    ///     * 如何加载父子窗口界面
    ///     * 出栈、入栈的需求，例如打开一个全屏界面有必要把栈中的界面出栈吗
    ////// </summary>
    public class UIManager : Singleton<UIManager>
    {
        private Dictionary<string, UIPanelBase>     m_PanelDict         = new Dictionary<string, UIPanelBase>();                // 逻辑对象队列，一旦创建了将不会销毁
        private LRUQueue<string, RectTransform>     m_LRUPool           = new LRUQueue<string, RectTransform>(MaxCachedCount);  // 显示对象队列，显示对象可能被销毁
        private const int                           MaxCachedCount      = 3;                                                    // 可最大缓存的显示对象数量
        private Dictionary<string, RectTransform>   m_PersistentPool    = new Dictionary<string, RectTransform>();              // 常驻内存的显示对象
        private Stack<UIPanelBase>                  m_PanelStack        = new Stack<UIPanelBase>();                             // 操作记录栈
        private Canvas                              m_Root;
        private const string                        m_UIRootName        = "UIRoot";
        private Dictionary<string, RectTransform>   m_LayerDict         = new Dictionary<string, RectTransform>();              // 层级节点

        // called when TestUIManager.StartLogic uses reflection for debug 
        static public void StaticInit()
        {
            UIManager.Instance.Init();
        }

        protected override void InternalInit()
        {
            InitCanvas();
            InitLayer();
            UIDefines.Init();
            m_LRUPool.OnDiscard += OnDiscard;
        }

        /// <summary>
        /// 初始化UIRoot
        /// </summary>
        private void InitCanvas()
        {
            Canvas[] canvas = GameObject.FindObjectsOfType<Canvas>();
            if(canvas == null || canvas.Length == 0)
                throw new System.ArgumentNullException("can't find any canvas");

            int count = canvas.Length;
            for(int i = 0; i < count; ++i)
            {
                if(string.Compare(canvas[i].gameObject.name, m_UIRootName, true) == 0)
                {
                    m_Root = canvas[i];
                    break;
                }
            }
            if(m_Root == null)
                throw new System.ArgumentNullException($"InitCanvas: can't find any canvas which name is {m_UIRootName}");
        }

        /// <summary>
        /// 初始化层级数据
        /// </summary>
        private void InitLayer()
        {
            IEnumerator e = UILayer.LayerToLoad();
            while(e.MoveNext())
            {
                string str = (string)e.Current;
                RectTransform tran = (RectTransform)m_Root.transform.Find(str);
                if(tran == null)
                {
                    Debug.LogError($"InitLayer: can't find layer of root [{str}]");
                    continue;
                }

                m_LayerDict.Add(str, tran);
            }
        }

        public void Load(string id, object para = null)
        {
            UIDefines def = UIDefines.Get(id);
            if(def == null)
                throw new System.ArgumentNullException($"UIDefines == null  id: {id}");
            
            UIPanelBase panel = GetOrCreatePanel(def);          // trigger OnInit
            Push(panel);                                        // push stack

            RectTransform rect = FindResource(def);
            if(rect != null)
            {
                panel.OnShow();
                CacheResource(def, rect);
            }

            
            if(rect != null)
            {
                m_LRUPool.Cache(id, rect);
            }
            else
            {
                // Trigger OnCreate
                AsyncLoaderManager.Instance.AsyncLoad(def.assetPath, OnPrefabLoadCompleted, def);
            }
        }

        /// <summary>
        /// 获取或创建界面逻辑对象
        /// </summary>
        /// <param name="def"></param>
        /// <returns></returns>
        private UIPanelBase GetOrCreatePanel(UIDefines def)
        {
            UIPanelBase panel;
            if(!m_PanelDict.TryGetValue(def.id, out panel))
            {
                panel = (UIPanelBase)Activator.CreateInstance(def.typeOfPanel, new object[] {def});
                panel.OnInit();
                m_PanelDict.Add(def.id, panel);
            }
            return panel;
        }

        /// <summary>
        /// 界面逻辑对象入栈，此时资源可能尚未实例化
        /// </summary>
        /// <param name="panel"></param>
        private void Push(UIPanelBase panel)
        {
            m_PanelStack.Push(panel);
            // panel.OnShow();
        }

        /// <summary>
        /// 界面逻辑对象出栈，此时资源可能尚未加载
        /// </summary>
        private void Pop()
        {
            UIPanelBase panel;
            if(!m_PanelStack.TryPop(out panel))
            {
                Debug.LogError($"");
            }
        }

        /// <summary>
        /// 查找UI资源
        /// </summary>
        /// <param name="def"></param>
        /// <returns></returns>
        private RectTransform FindResource(UIDefines def)
        {
            RectTransform rect;
            if(def.isPersistent)
            {
                m_PersistentPool.TryGetValue(def.id, out rect);
            }
            else
            {
                rect = m_LRUPool.Exist(def.id);
            }
            return rect;
        }

        /// <summary>
        /// 缓存UI资源
        /// </summary>
        /// <param name="def"></param>
        /// <param name="panel"></param>
        private void CacheResource(UIDefines def, RectTransform panel)
        {
            if(!def.isPersistent)
            {
                m_LRUPool.Cache(def.id, panel);
            }
        }

        public void Unload(string id)
        {}

        protected override void OnDestroy()
        {
            m_LRUPool.OnDiscard -= OnDiscard;
        }

        private void OnDiscard(string id, RectTransform item)
        {
        }

        void OnPrefabLoadCompleted(GameObject go, System.Object userData)
        {
            UIDefines def = (UIDefines)userData;
            if(go == null)
            {
                GameDebug.LogError($"failed to instantiate ui prefab: {def?.assetPath}");
                return;
            }

            UIPanelBase panel = GetOrCreatePanel(def);
            panel.OnCreate();
        }

        protected override void OnUpdate(float deltaTime)
        {}
    }
}