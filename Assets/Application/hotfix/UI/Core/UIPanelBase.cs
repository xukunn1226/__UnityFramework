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
        public UIDefines defines { get; private set; }
        public UIPanelBase(UIDefines defines)
        {
            this.defines = defines;
        }
        private UIPanelBase() {}

        public abstract void OnInit();              // UIPanelBase创建时的回调，仅一次
        public abstract void OnCreate();            // UI资源实例化完成时的回调，可执行绑定操作，与OnDestroy对应
        public abstract void OnShow();              // 界面打开回调（资源已实例化）
        public abstract void OnUpdate();            // update the panel
        public abstract void OnHide();              // 界面关闭回调
        public abstract void OnDestroy();           // 界面资源销毁时调用，与OnCreate对应
    }
}