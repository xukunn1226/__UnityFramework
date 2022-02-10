using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Logic
{
    // https://jenocn.github.io/2019/07/UnityUiModel/
    /// <summary>
    /// 界面逻辑对象
    /// </summary>
    public abstract class UIPanelBase
    {
        private List<UIPanelBase>   m_Children              = new List<UIPanelBase>();
        public UIDefines            defines                 { get; private set; }
        public bool                 isOverrideParentId      { get; private set; }
        public string               overrideParentId        { get; private set; }
        public string               parentId                { get { return isOverrideParentId ? overrideParentId : defines.parentId; } }

        public UIPanelBase(UIDefines defines) { this.defines = defines; }
        private UIPanelBase() {}

        public virtual void OnInit() {}             // UIPanelBase创建时的回调，仅一次
        public virtual void OnCreate() {}           // UI资源实例化完成时的回调，可执行绑定操作，与OnDestroy对应
        public virtual void OnShow() {}             // 界面打开回调（资源已实例化）
        public virtual void OnUpdate() {}           // update the panel
        public virtual void OnHide() {}             // 界面关闭回调
        public virtual void OnDestroy() {}          // 界面资源销毁时调用，与OnCreate对应

        public void Unload()
        {
            UIManager.Instance.Close(defines.id);
        }

        public void AddChild(UIPanelBase child)
        {}

        public void RemoveChild(UIPanelBase child)
        {}

        /// <summary>
        /// 是否需要入栈管理，根据所处layer设定
        /// </summary>
        /// <returns></returns>
        static protected bool NeedStack(UIDefines def)
        {
            if(def.layer == UILayer.Fullscreen || def.layer == UILayer.Windowed)
                return true;
            return false;
        }
    }
}