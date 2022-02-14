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
    ///     * 全屏界面及没有父窗口的界面需要入栈
    /// 问题：
    ///     * 如何加载父子窗口界面
    ///     * 出栈、入栈的需求，例如打开一个全屏界面有必要把栈中的界面出栈吗
    ///     * 业务模块的数据管理器能互相访问吗？
    ///     * 哪些界面类型需要进栈？fullscreen，windowed，pop，tips？
    ///     * Popup界面类型如何定义？
    ///     * Tips属于界面吗？需要特殊接口加载？
    ///     * 哪些情况加载出来的界面需要动态调整位置？tips？
    ////// </summary>
    public class UIManager : Singleton<UIManager>
    {
        private Dictionary<string, PanelState>      m_PanelDict         = new Dictionary<string, PanelState>();                 // 逻辑对象队列，一旦创建了将不会销毁
        private LRUQueue<string, RectTransform>     m_LRUPool           = new LRUQueue<string, RectTransform>(MaxCachedCount);  // 显示对象队列1（LRU管理，可被销毁）
        private const int                           MaxCachedCount      = 3;                                                    // 可最大缓存的显示对象数量
        private Dictionary<string, RectTransform>   m_PersistentPool    = new Dictionary<string, RectTransform>();              // 显示对象队列2（常驻内存）
        private LinkedList<PanelState>              m_PanelStack        = new LinkedList<PanelState>();                         // 操作记录栈
        private Canvas                              m_Root;
        private const string                        m_UIRootName        = "UIRoot";
        private Dictionary<string, RectTransform>   m_LayerDict         = new Dictionary<string, RectTransform>();              // 层级节点
        private Dictionary<string, PanelState>      m_UpdateDict        = new Dictionary<string, PanelState>();                 // 更新列表
        private Dictionary<string, PanelState>.Enumerator m_UpdateEnum;

        class PanelState
        {
            public UIPanelBase  panel;
            public bool         inStack;        // 是否在栈中
            public bool         isShow;         // 逻辑上是否显示
            public bool         isShowRes;      // UI资源当前的显示状态
            public bool         needUpdate;     // 是否需要update
            public float        intervalTime;   // update间隔时间，<= 0表示每帧更新
            public float        elapsedTime;
        }

        // called by TestUIManager.StartLogic for debug 
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
            m_UpdateEnum = m_UpdateDict.GetEnumerator();
        }

        protected override void OnDestroy()
        {
            m_LRUPool.OnDiscard -= OnDiscard;
            m_UpdateEnum.Dispose();
        }

        protected override void OnUpdate(float deltaTime)
        {
            // 处于显示状态才触发OnUpdate
            ((IEnumerator)m_UpdateEnum).Reset();
            while(m_UpdateEnum.MoveNext())
            {
                PanelState ps = m_UpdateEnum.Current.Value;
                if(!ps.isShowRes || !ps.needUpdate)
                {
                    continue;
                }

                ps.elapsedTime += deltaTime;
                if(ps.elapsedTime >= ps.intervalTime)
                {
                    ps.panel.OnUpdate(deltaTime);
                    ps.elapsedTime = 0;
                }
            }
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

        /// <summary>
        /// 打开界面
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userData"></param>
        public void Open(string id, object userData = null)
        {
            UIDefines def = UIDefines.Get(id);
            if(def == null)
                throw new System.ArgumentNullException($"UIDefines == null  id: {id}");
            
            PanelState ps = GetOrCreatePanel(def);
            PushPanel(ps);
            if(FindResource(def) != null)
            { // 资源已加载执行后续流程
                OnPostResourceLoaded(ps, userData);
            }
            else
            { // 资源未加载则发起异步加载流程
                AsyncLoaderManager.Instance.AsyncLoad(def.assetPath, OnPrefabLoadCompleted, new System.Object[] { def, userData });
            }
        }

        public void Close(string id)
        {
            UIDefines def = UIDefines.Get(id);
            if(def == null)
                throw new System.ArgumentNullException($"UIDefines == null  id: {id}");

            PanelState ps = GetOrCreatePanel(def);
            HidePanel(ps);
            PopPanel(ps);
        }

        /// <summary>
        /// 资源加载完成的回调
        /// </summary>
        /// <param name="go"></param>
        /// <param name="userData"></param>
        private void OnPrefabLoadCompleted(GameObject go, System.Object userData)
        {
            System.Object[] data = (System.Object[])userData;
            UIDefines def = (UIDefines)data[0];
            System.Object state = data[1];

            if(go == null)
            {
                GameDebug.LogError($"failed to instantiate ui prefab: {def?.assetPath}");
                return;
            }

            PanelState ps = GetOrCreatePanel(def);
            AddResource(ps.panel, go);

            OnPostResourceLoaded(ps, data[1]);
        }

        /// <summary>
        /// 执行资源加载完成后的流程
        /// </summary>
        private void OnPostResourceLoaded(PanelState ps, System.Object userData)
        {
            if(ps.isShow)
            {
                ShowPanel(ps, userData);
                CacheResource(ps);
            }
            else
            {
                // 显示上关闭了界面
                DeactivePanel(ps);
            }
        }

        /// <summary>
        /// 获取或创建界面逻辑对象
        /// </summary>
        /// <param name="def"></param>
        /// <returns></returns>
        private PanelState GetOrCreatePanel(UIDefines def)
        {
            PanelState ps;
            if(!m_PanelDict.TryGetValue(def.id, out ps))
            {
                ps = new PanelState();
                UIPanelBase panel = (UIPanelBase)Activator.CreateInstance(def.typeOfPanel, new object[] {def});
                panel.OnInit();
                ps.panel = panel;
                m_PanelDict.Add(def.id, ps);
            }
            return ps;
        }

        /// <summary>
        /// 界面逻辑对象入栈，此时资源可能尚未实例化
        /// </summary>
        /// <param name="panel"></param>
        private void PushPanel(PanelState ps)
        {
            if(!ps.panel.CanStack())
                return;

            #if UNITY_EDITOR
            if(m_PanelStack.Find(ps) != null)
            {
                Debug.LogError($"UIManager.Push panel({ps.panel.defines.id}) already in stack");
                return;
            }
            #endif

            // 全屏界面进栈将触发已在栈中的其他界面OnHide
            if(ps.panel.IsFullscreen())
            {
                // 倒序遍历直至另一个全屏界面为止的所有界面OnHide
                LinkedListNode<PanelState> lastNode = m_PanelStack.Last;
                while(lastNode != null)
                {
                    HidePanel(lastNode.Value);
                    if(lastNode.Value.panel.IsFullscreen())
                    {
                        break;                        
                    }
                    else
                    {
                        lastNode = lastNode.Previous;
                    }
                }
            }

            ps.inStack = true;
            m_PanelStack.AddLast(ps);
        }

        /// <summary>
        /// 界面逻辑对象出栈，此时资源可能尚未加载
        /// </summary>
        private void PopPanel(PanelState ps)
        {
            if(!ps.panel.CanStack())
                return;

            #if UNITY_EDITOR
            if(m_PanelStack.Find(ps) == null)
            {
                Debug.LogError($"UIManager.Pop panel({ps.panel.defines.id}) is not in stack");
                return;
            }
            #endif

            ps.inStack = false;
            m_PanelStack.Remove(ps);

            // 全屏界面出栈将触发栈中最近的另一个全屏界面OnShow
            if(ps.panel.IsFullscreen())
            {
                // 倒序遍历直至另一个全屏界面为止的所有界面OnShow
                LinkedListNode<PanelState> lastNode = m_PanelStack.Last;
                while(lastNode != null)
                {
                    ShowPanel(lastNode.Value);
                    if(lastNode.Value.panel.IsFullscreen())
                    {
                        break;
                    }
                    else
                    {
                        lastNode = lastNode.Previous;
                    }
                }
            }
        }

        /// <summary>
        /// 弹出栈中的所有非全屏界面
        /// </summary>
        private void PopAllWindowedPanel()
        {

        }

        /// <summary>
        /// 显示界面（逻辑上）
        /// </summary>
        /// <param name="ps"></param>
        /// <param name="userData"></param>
        private void ShowPanel(PanelState ps, System.Object userData = null)
        {
            if(ps.isShow)
            {
                ps.panel.OnShow(userData);
            }
            ps.isShow = true;
        }

        /// <summary>
        /// 关闭界面（逻辑上）
        /// </summary>
        /// <param name="ps"></param>
        private void HidePanel(PanelState ps)
        {
            if( ps.isShowRes &&         // 因为异步加载，界面可能尚未实例化，只有已打开时才触发OnHide
                ps.isShow)              // 逻辑的显示状态为true表示需要显示界面
            {
                ps.panel.OnHide();

                // 隐藏Panel实例
                DeactivePanel(ps);
                ps.isShowRes = false;
            }
            ps.isShow = false;
        }

        /// <summary>
        /// 打开界面（显示上）
        /// </summary>
        /// <param name="ps"></param>
        private void ActivePanel(PanelState ps)
        {
            switch(ps.panel.defines.hideMode)
            {
                case EHideMode.SetActive:
                    ps.panel.transform.gameObject.SetActive(true);
                    break;
                case EHideMode.DisableCanvas:
                    if(ps.panel.canvas != null)
                    {
                        ps.panel.canvas.enabled = true;
                    }
                    else
                    {
                        ps.panel.transform.gameObject.SetActive(true);
                    }
                    break;
                case EHideMode.OutOfScreen:                    
                    break;
                case EHideMode.OutOfViewLayer:
                    break;
            }
        }

        /// <summary>
        /// 关闭界面（显示上）
        /// </summary>
        /// <param name="ps"></param>
        private void DeactivePanel(PanelState ps)
        {
            switch(ps.panel.defines.hideMode)
            {
                case EHideMode.SetActive:
                    ps.panel.transform.gameObject.SetActive(false);
                    break;
                case EHideMode.DisableCanvas:
                    if(ps.panel.canvas != null)
                    {
                        ps.panel.canvas.enabled = false;
                    }
                    else
                    {
                        ps.panel.transform.gameObject.SetActive(false);
                    }
                    break;
                case EHideMode.OutOfScreen:
                    break;
                case EHideMode.OutOfViewLayer:
                    break;
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
        /// <param name="ps"></param>
        private void CacheResource(PanelState ps)
        {
            if(!ps.panel.defines.isPersistent)
            {
                m_LRUPool.Cache(ps.panel.defines.id, ps.panel.transform);
            }
        }

        /// <summary>
        /// 资源加载完成，进入显示对象列表
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="go"></param>
        private void AddResource(UIPanelBase panel, GameObject go)
        {
            RectTransform rect = go.GetComponent<RectTransform>();
            if(panel.defines.isPersistent)
            {
                m_PersistentPool.Add(panel.defines.id, rect);
            }
            else
            {
                m_LRUPool.Cache(panel.defines.id, rect);
            }
            panel.OnCreate(go);       // trigger OnCreate
        }

        public void RegisterUpdateEvent(string id, float intervalTime = 0)
        {
            PanelState ps = GetOrCreatePanel(UIDefines.Get(id));
            ps.needUpdate = true;
            ps.intervalTime = intervalTime;
            ps.elapsedTime = 0;
            m_UpdateDict.Add(id, ps);
        }

        public void UnregisterUpdateEvent(string id)
        {
            PanelState ps = GetOrCreatePanel(UIDefines.Get(id));
            ps.needUpdate = false;
            m_UpdateDict.Remove(id);
        }

        /// <summary>
        /// 剔除出缓存队列
        /// </summary>
        /// <param name="id"></param>
        /// <param name="item"></param>
        private void OnDiscard(string id, RectTransform item)
        {
            Debug.Log($"UIManager.OnDiscard     id: {id}");
            UIPanelBase panel = GetOrCreatePanel(UIDefines.Get(id)).panel;
            panel.OnDestroy();
            UnityEngine.Object.Destroy(item.gameObject);
        }
    }
}