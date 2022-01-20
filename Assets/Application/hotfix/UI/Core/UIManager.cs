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
        private Dictionary<UIPanelType, UIPanelBase> m_PanelDict = new Dictionary<UIPanelType, UIPanelBase>();
        private Stack<UIPanelBase> m_PanelStack = new Stack<UIPanelBase>();

        static public void StaticInit()
        {
            UIManager.Instance.Init();
        }

        protected override void InternalInit()
        {
        }

        public void Load(UIPanelType type)
        {}

        public void Unload(UIPanelType type)
        {}
    }
}