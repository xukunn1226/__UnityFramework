using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Framework.Cache;
using Framework.Core;
using Application.Runtime;
using Framework.AssetManagement.Runtime;

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
    ///     * Tips需要归为特殊的一层吗？例如不入栈，
    /// TODO:
    ///     * 搭建美术UI特效制作流程，场景、预览等
    ///     * Font, TextMeshPro
    ///     * 各种常用UI控件：图文混排（https://zhuanlan.zhihu.com/p/33579005）
    ///     * control binding: https://github.com/Misaka-Mikoto-Tech/UIControlBinding
    ///     * HUD
    ///     * 特效、动画、safeArea、图文混排，TextMeshPro、DrawCall可视化显示、图集打包规则
    ///     * MessageBox
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
            public bool         isShow;         // 逻辑上是否显示
            public bool         isShowRes;      // UI资源当前的显示状态
            public bool         isLoadRes;      // 资源是否已加载
            public float        intervalTime;   // update间隔时间，<= 0表示每帧更新
            public float        elapsedTime;
        }

#if UNITY_EDITOR
        // called by TestUIManager.StartLogic for debug
        static public void StaticInit()
        {
            UIManager.Instance.Init();
        }

        static public UIManager Get()
        {
            return UIManager.Instance;
        }

        static public List<string> GetStackPanelInfo()
        {
            List<string> lst = new List<string>();
            LinkedListNode<PanelState> node = UIManager.Instance.m_PanelStack.Last;
            while(node != null)
            {
                string info = UIManager.Instance.IsFullscreen(node.Value.panel) ? "F" : "W";
                lst.Add(node.Value.panel.defines.id + string.Format($"({info})"));
                node = node.Previous;
            }
            return lst;
        }

        static public List<string> GetUpdatePanelInfo()
        {
            List<string> lst = new List<string>();
            Dictionary<string, PanelState>.Enumerator e = UIManager.Instance.m_UpdateDict.GetEnumerator();
            while(e.MoveNext())
            {
                if(e.Current.Value.isShowRes)
                {
                    lst.Add(e.Current.Value.panel.defines.id);
                }
            }
            return lst;
        }

        static public List<string> GetPersistentPoolInfo()
        {
            List<string> lst = new List<string>();
            Dictionary<string, RectTransform>.Enumerator e = UIManager.Instance.m_PersistentPool.GetEnumerator();
            while(e.MoveNext())
            {
                lst.Add(e.Current.Key);
            }
            return lst;
        }

        static public List<string> GetLRUPoolInfo()
        {
            List<string> lst = new List<string>();
            IEnumerator<string> e = UIManager.Instance.m_LRUPool.IterKey();
            while(e.MoveNext())
            {
                lst.Add(e.Current);
            }
            return lst;
        }

        static public int GetLRUMaxCount()
        {
            return UIManager.MaxCachedCount;
        }
#endif        

        protected override void InternalInit()
        {
            // AtlasManager.InitPersistentAtlas();
            InitCanvas();
            InitLayer();
            UIDefines.Init();
            m_LRUPool.OnDiscard += OnDiscard;
            m_UpdateEnum = m_UpdateDict.GetEnumerator();
        }

        protected override void OnDestroy()
        {
            ClearResourceList();
            m_LRUPool.OnDiscard -= OnDiscard;
            m_UpdateEnum.Dispose();
            AtlasManager.UninitPersistentAtlas();
        }
        
        /// <summary>
        /// 剔除出缓存队列
        /// </summary>
        /// <param name="id"></param>
        /// <param name="item"></param>
        private void OnDiscard(string id, RectTransform item)
        {
            Debug.Log($"UIManager.OnDiscard     id: {id}");
            PanelState ps = GetOrCreatePanel(UIDefines.Get(id));
            ps.isLoadRes = false;
            ps.panel.OnDestroy();
            UnityEngine.Object.Destroy(item.gameObject);
        }

        /// <summary>
        /// 销毁显示队列
        /// </summary>
        private void ClearResourceList()
        {
            m_LRUPool.Clear();
            foreach(var item in m_PersistentPool)
            {
                UnityEngine.Object.Destroy(item.Value.gameObject);
            }
            m_PersistentPool.Clear();
        }

        protected override void OnUpdate(float deltaTime)
        {
            UnityEngine.Profiling.Profiler.BeginSample("UIManager.OnUpdate");
            // 处于显示状态才触发OnUpdate
            ((IEnumerator)m_UpdateEnum).Reset();
            while(m_UpdateEnum.MoveNext())
            {
                PanelState ps = m_UpdateEnum.Current.Value;
                if(!ps.isShowRes)
                {
                    continue;
                }

                ps.elapsedTime += deltaTime;
                if(ps.elapsedTime >= ps.intervalTime)
                {
                    ps.panel.OnUpdate(ps.elapsedTime);
                    ps.elapsedTime = 0;
                }
            }
            UnityEngine.Profiling.Profiler.EndSample();
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

        public Sprite GetSprite(string atlasName, string spriteName)
        {
            return AtlasManager.GetSprite(atlasName, spriteName);
        }

        private RectTransform GetLayerNode(string layer)
        {
            RectTransform node;
            if(!m_LayerDict.TryGetValue(layer, out node))
            {
                Debug.Log($"UIManager: layer ({layer}) not exists");
            }
            return node;
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
            
            // execute order: push stack -> load resource -> show panel
            PanelState ps = GetOrCreatePanel(def);
            PushPanel(ps);
            if(GetOrLoadResource(def, userData))
            { // 资源已加载执行后续流程
                OnPostResourceLoaded(ps, userData);
            }
        }

        /// <summary>
        /// 关闭界面
        /// </summary>
        /// <param name="id"></param>
        public void Close(string id)
        {
            UIDefines def = UIDefines.Get(id);
            if(def == null)
                throw new System.ArgumentNullException($"UIDefines == null  id: {id}");

            // execute order: pop stack -> hide panel
            PanelState ps = GetOrCreatePanel(def);            
            PopPanel(ps);
            HidePanel(ps);
        }

        /// <summary>
        /// 关闭最上层的全屏或没有父窗口的非全屏界面
        /// </summary>
        /// <returns></returns>
        public bool CloseTop()
        {
            PanelState pendingClosePanel = null;
            LinkedListNode<PanelState> lastNode = m_PanelStack.Last;            
            while(lastNode != null)
            {
                if(IsFullscreen(lastNode.Value.panel) || !HasParent(lastNode.Value.panel))
                {
                    pendingClosePanel = lastNode.Value;
                    break;
                }
                lastNode = lastNode.Previous;
            }
            if(pendingClosePanel == null)
                return false;
            Close(pendingClosePanel.panel.defines.id);
            return true;
        }

        public void CloseAll()
        {
            while(CloseTop()) { }
        }

        public void ReturnBack()
        {
            LinkedListNode<PanelState> lastNode = m_PanelStack.Last;
            while(lastNode != null)
            {
                if(lastNode.Value.panel.OnReturnBack())
                    break;
                lastNode = lastNode.Previous;
            }
        }

        private bool GetOrLoadResource(UIDefines def, System.Object userData)
        {
            if(FindResource(def) != null)
                return true;

            // 资源未加载则发起异步加载流程
            //AsyncLoaderManager.Instance.AsyncLoadPrefab(def.assetPath, OnPrefabLoadCompleted, new System.Object[] { def, userData });

            m_UserData = new System.Object[] { def, userData };
            var op = AssetManagerEx.LoadPrefabAsync(def.assetPath);
            op.Completed += OnUIPrefabCompleted;
            return false;
        }

        private System.Object[] m_UserData;
        /// <summary>
        /// 资源加载完成的回调
        /// </summary>
        private void OnUIPrefabCompleted(PrefabOperationHandle op)
        {
            if(op.status == EOperationStatus.Failed)
            {
                return;
            }

            System.Object[] data = (System.Object[])m_UserData;
            UIDefines def = (UIDefines)data[0];
            System.Object state = data[1];

            PanelState ps = GetOrCreatePanel(def);
            AddResource(ps.panel, op.gameObject);
            ps.isLoadRes = true;

            OnPostResourceLoaded(ps, data[1]);
        }

        ///// <summary>
        ///// 资源加载完成的回调
        ///// </summary>
        ///// <param name="go"></param>
        ///// <param name="userData"></param>
        //private void OnPrefabLoadCompleted(GameObject go, System.Object userData)
        //{
        //    System.Object[] data = (System.Object[])userData;
        //    UIDefines def = (UIDefines)data[0];
        //    System.Object state = data[1];

        //    if(go == null)
        //    {
        //        GameDebug.LogError($"failed to instantiate ui prefab: {def?.assetPath}");
        //        return;
        //    }

        //    PanelState ps = GetOrCreatePanel(def);
        //    AddResource(ps.panel, go);
        //    ps.isLoadRes = true;

        //    OnPostResourceLoaded(ps, data[1]);
        //}

        /// <summary>
        /// 执行资源加载完成后的流程
        /// </summary>
        private void OnPostResourceLoaded(PanelState ps, System.Object userData)
        {
            // 资源加载完时可能逻辑上已标记为不显示了，则显示上关闭界面
            //if(!ps.isShow)
            //{
            //    DeactivePanel(ps);
            //    return;
            //}

            ShowPanel(ps, userData);
            CacheResource(ps);
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
                ps.panel = panel;
                m_PanelDict.Add(def.id, ps);

                panel.OnInit();
            }
            return ps;
        }

        /// <summary>
        /// 界面逻辑对象入栈，可能触发其他界面的OnHide
        /// </summary>
        /// <param name="panel"></param>
        private void PushPanel(PanelState ps)
        {
            if(!CanStack(ps))
                return;

            if(m_PanelStack.Find(ps) != null)
            {
                Debug.LogWarning($"UIManager.Push panel({ps.panel.defines.id}) already in stack");
                return;
            }

            // 全屏界面进栈将触发已在栈中的其他界面OnHide
            if(IsFullscreen(ps.panel))
            {
                // 倒序遍历直至另一个全屏界面为止的所有界面OnHide
                LinkedListNode<PanelState> lastNode = m_PanelStack.Last;
                while(lastNode != null)
                {
                    HidePanel(lastNode.Value);
                    if(IsFullscreen(lastNode.Value.panel))
                    {
                        break;                        
                    }
                    else
                    {
                        lastNode = lastNode.Previous;
                    }
                }
            }

            m_PanelStack.AddLast(ps);
        }

        /// <summary>
        /// 界面逻辑对象出栈，可能触发其他界面的OnHide或OnShow
        /// </summary>
        private void PopPanel(PanelState ps)
        {
            if(!CanStack(ps))
                return;

            if(m_PanelStack.Find(ps) == null)
            {
                Debug.LogWarning($"UIManager.Pop panel({ps.panel.defines.id}) is not in stack");
                return;
            }

            // 检查弹出的PanelState合法性
            if(IsFullscreen(ps.panel))
            { // 如果是全屏界面必须是栈中最后一个全屏界面
                if(FindLastFullscreenPanel() != ps)
                {
                    Debug.LogWarning($"can't pop non-last fullscreen panel");
                    return;
                }
            }
            else
            { // 如果是非全屏界面必须在最后一个全屏界面之后
                LinkedListNode<PanelState> lastNode = m_PanelStack.Last;
                while (lastNode != null)
                {
                    if(IsFullscreen(lastNode.Value.panel))
                    {
                        Debug.LogWarning($"can't find the pending pop panel before the last fullscreen panel");
                        return;
                    }
                    if (lastNode.Value == ps)
                        break;
                    lastNode = lastNode.Previous;
                }
            }

            // 全屏界面出栈将触发排在自身之后的非全屏界面OnHide
            if(IsFullscreen(ps.panel))
            {
                LinkedListNode<PanelState> lastNode = m_PanelStack.Last;
                while(lastNode != null && lastNode.Value != ps)
                {
                    HidePanel(lastNode.Value);
                    lastNode = lastNode.Previous;
                }
            }

            m_PanelStack.Remove(ps);

            // 全屏界面出栈将触发栈中最近的另一个全屏界面OnShow
            // Full_A -> NonFull_B -> NonFull_C -> Full_D -> NonFull_E
            // 如果弹出Full_D，则触发NonFull_E.OnHide & Full_A.OnShow & NonFull_B.OnShow & NonFull_C.OnShow
            if(IsFullscreen(ps.panel))
            {
                // 倒序遍历直至另一个全屏界面为止的所有界面OnShow
                LinkedListNode<PanelState> lastNode = m_PanelStack.Last;
                while(lastNode != null)
                {
                    ShowPanel(lastNode.Value);
                    if(IsFullscreen(lastNode.Value.panel))
                        break;
                    lastNode = lastNode.Previous;
                }
            }
        }

        /// <summary>
        /// 弹出栈中最上层的所有没有父窗口的非全屏界面
        /// </summary>
        public void CloseAllWindowedPanel()
        {
            LinkedListNode<PanelState> lastNode = m_PanelStack.Last;
            while(lastNode != null)
            {
                if(IsFullscreen(lastNode.Value.panel) || HasParent(lastNode.Value.panel))
                    break;
                PopPanel(lastNode.Value);
            }
        }

        /// <summary>
        /// 显示界面（逻辑上），可能触发显示上的打开
        /// </summary>
        /// <param name="ps"></param>
        /// <param name="userData"></param>
        private void ShowPanel(PanelState ps, System.Object userData = null)
        {
            if(!ps.isShow && !ps.isShowRes && ps.isLoadRes)
            {
                // 显示Panel实例
                ActivePanel(ps.panel);

                ps.panel.OnShowAction();

                // Debug.Log($"ShowPanel: {ps.panel.defines.id}");
                ps.panel.OnShow(userData);
            }
            ps.isShow = true;
        }

        /// <summary>
        /// 关闭界面（逻辑上），可能触发显示上的关闭
        /// </summary>
        /// <param name="ps"></param>
        private void HidePanel(PanelState ps)
        {
            if( ps.isShowRes &&         // 因为异步加载，界面可能尚未实例化，只有已打开时才触发OnHide
                ps.isShow)              // 逻辑的显示状态为true表示需要显示界面
            {
                ps.panel.OnHideAction(() => 
                {
                    // Debug.Log($"HidePanel: {ps.panel.defines.id}");
                    ps.panel.OnHide();

                    // 隐藏Panel实例
                    DeactivePanel(ps.panel);
                });                
            }
            ps.isShow = false;
        }

        /// <summary>
        /// 打开界面（显示上）
        /// </summary>
        /// <param name="panel"></param>
        private void ActivePanel(UIPanelBase panel)
        {
            if(panel == null)
                throw new System.ArgumentNullException($"UIManager.ActivePanel: UIPanelBase == null");

            PanelState ps = GetOrCreatePanel(panel.defines);
            if(ps == null)
                throw new System.ArgumentNullException($"UIManager.ActivePanel: PanelState == null");

            switch(panel.defines.hideMode)
            {
                case EHideMode.SetActive:
                    panel.transform.gameObject.SetActive(true);
                    break;
                case EHideMode.DisableCanvas:
                    if(panel.canvas != null)
                    {
                        panel.canvas.enabled = true;
                    }
                    else
                    {
                        panel.transform.gameObject.SetActive(true);
                    }
                    break;
                case EHideMode.OutOfScreen:                    
                    break;
                case EHideMode.OutOfViewLayer:
                    break;
            }
            ps.isShowRes = true;

            panel.transform.SetParent(GetParentTransform(panel), false);
        }

        private RectTransform GetParentTransform(UIPanelBase panel)
        {
            RectTransform trans = null;
            if(HasParent(panel))
            { // 优先以父窗口为挂点
                trans = FindResource(UIDefines.Get(panel.parentId));
            }
            if(trans == null)
            { // 其次以层级节点为挂点
                trans = GetLayerNode(panel.defines.layer);
            }
            return trans;
        }

        /// <summary>
        /// 关闭界面（显示上）
        /// </summary>
        /// <param name="panel"></param>
        private void DeactivePanel(UIPanelBase panel)
        {
            if(panel == null)
                throw new System.ArgumentNullException($"UIManager.DeactivePanel: UIPanelBase == null");

            PanelState ps = GetOrCreatePanel(panel.defines);
            if(ps == null)
                throw new System.ArgumentNullException($"UIManager.DeactivePanel: PanelState == null");

            switch(panel.defines.hideMode)
            {
                case EHideMode.SetActive:
                    panel.transform.gameObject.SetActive(false);
                    break;
                case EHideMode.DisableCanvas:
                    if(panel.canvas != null)
                    {
                        panel.canvas.enabled = false;
                    }
                    else
                    {
                        panel.transform.gameObject.SetActive(false);
                    }
                    break;
                case EHideMode.OutOfScreen:
                    break;
                case EHideMode.OutOfViewLayer:
                    break;
            }
            ps.isShowRes = false;
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
            panel.InternalCreate(go);
            panel.OnCreate(go);       // trigger OnCreate
        }

        public void RegisterUpdateEvent(string id, float intervalTime = 0)
        {
            PanelState ps = GetOrCreatePanel(UIDefines.Get(id));
            ps.intervalTime = intervalTime;
            ps.elapsedTime = 0;
            m_UpdateDict.Add(id, ps);
        }

        public void UnregisterUpdateEvent(string id)
        {
            PanelState ps = GetOrCreatePanel(UIDefines.Get(id));
            m_UpdateDict.Remove(id);
        }
        
        private PanelState FindLastFullscreenPanel()
        {
            LinkedListNode<PanelState> lastNode = m_PanelStack.Last;
            while(lastNode != null)
            {
                if(IsFullscreen(lastNode.Value.panel))
                    return lastNode.Value;
                lastNode = lastNode.Previous;
            }
            return null;
        }

        /// <summary>
        /// 是否需要入栈管理
        /// </summary>
        /// <returns></returns>
        private bool CanStack(PanelState ps)
        {
            if(ps.panel.defines.layer == UILayer.Fullscreen || (ps.panel.defines.layer == UILayer.Windowed && !HasParent(ps.panel)))
                return true;
            return false;
        }

        private bool IsFullscreen(UIPanelBase panel)
        {
            return panel.defines.layer == UILayer.Fullscreen;
        }        

        private bool HasParent(UIPanelBase panel)
        {
            return !string.IsNullOrEmpty(panel.parentId);
        }

        public void OpenMessageBox(string msg, string confirmMsg, string cancelMsg, System.Action confirmCallback, System.Action cancelCallback)
        {
            UIPanel_MessageBox.MessageInfo userData = new UIPanel_MessageBox.MessageInfo() { msg = msg, confirmMsg = confirmMsg, cancelMsg = cancelMsg, confirmCallback = confirmCallback, cancelCallback = cancelCallback };
            Open(UIPanelID.MessageBox, userData);
        }
    }
}