using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        private Dictionary<int, UIPanelBase>        m_PanelDict     = new Dictionary<int, UIPanelBase>();           // 
        private Stack<UIPanelBase>                  m_PanelStack    = new Stack<UIPanelBase>();                     // 
        private Canvas                              m_Root;
        private const string                        m_UIRootName    = "UIRoot";
        private Dictionary<string, RectTransform>   m_LayerDict     = new Dictionary<string, RectTransform>();      // 层级节点

        static public void StaticInit()
        {
            UIManager.Instance.Init();
        }

        protected override void InternalInit()
        {
            InitCanvas();
            InitLayer();
            UIDefines.Init();
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
                throw new System.ArgumentNullException("canvas == null");
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

        public void Load(int id)
        {}

        public void Unload(int id)
        {}
    }
}