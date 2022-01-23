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
    /// 层级管理：
    ///     Main, Fullscreen, Windowed, Tips, Loading, Alert
    /// Panel生命周期管理：
    /// </summary>
    public class UIManager : Singleton<UIManager>
    {
        private Dictionary<string, UIPanelBase>     m_PanelDict     = new Dictionary<string, UIPanelBase>();                // 逻辑对象队列，包含了显示对象队列
        private LRUQueue<string, RectTransform>     m_LRUPool       = new LRUQueue<string, RectTransform>(MaxCachedCount);  // 显示对象队列，显示对象销毁并不会导致逻辑对象销毁
        private const int                           MaxCachedCount  = 3;                                                    // 最大缓存prefab数量
        private Stack<UIPanelBase>                  m_PanelStack    = new Stack<UIPanelBase>();                             // 操作记录栈
        private Canvas                              m_Root;
        private const string                        m_UIRootName    = "UIRoot";
        private Dictionary<string, RectTransform>   m_LayerDict     = new Dictionary<string, RectTransform>();              // 层级节点        

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

            // get or create UIPanel, trigger OnInit
            UIPanelBase panel = GetOrCreatePanel(def);

            RectTransform rect = m_LRUPool.Exist(id);
            // Trigger OnOpen
            Push(panel, rect != null);
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

        private UIPanelBase GetOrCreatePanel(UIDefines def)
        {
            UIPanelBase panel;
            if(!m_PanelDict.TryGetValue(def.id, out panel))
            {
                panel = (UIPanelBase)Activator.CreateInstance(def.typeOfPanel, new object[] {panel});
                panel.OnInit();
                m_PanelDict.Add(def.id, panel);
            }
            return panel;
        }

        private void Push(UIPanelBase panel, bool isReady)
        {
            m_PanelStack.Push(panel);
            panel.OnOpen(isReady);
        }

        private void Pop()
        {
            UIPanelBase panel;
            if(m_PanelStack.TryPop(out panel))
            {
                // panel.OnCloseAction();
                panel.OnClose();
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