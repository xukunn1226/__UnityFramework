using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Logic
{
    /// <summary>
    /// 界面管理器
    /// 
    /// </summary>
    public class UIManager : Singleton<UIManager>
    {
        private Dictionary<UIPanelType, UIPanelBase> m_PanelDict = new Dictionary<UIPanelType, UIPanelBase>();
        private Stack<UIPanelBase> m_PanelStack = new Stack<UIPanelBase>();

        protected override void InternalInit()
        {
            
        }

        public void Load(UIPanelType type)
        {}

        public void Unload(UIPanelType type)
        {}
    }
}