using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Logic
{
    // https://jenocn.github.io/2019/07/UnityUiModel/
    public abstract class UIPanelBase
    {
        public UIDefines defines { get; private set; }
        public UIPanelBase(UIDefines defines)
        {
            this.defines = defines;
        }
        private UIPanelBase() {}

        public abstract void OnInit();              // called when the script is initialized, call OnInit only once during the lifetime of the UIPanel
        public abstract void OnCreate();            // prefab生成完毕时调用，可执行监听、绑定
        public abstract void OnOpen(bool isReady);  // 入栈立即调用，但此时资源可能尚未加载，isReady = true: 此时资源已准备就绪
        public abstract void OnOpenAction();        //
        public abstract void OnUpdate();            //
        public abstract void OnPause();             //
        public abstract void OnResume();            //
        public abstract void OnCloseAction();       //
        public abstract void OnClose();             // out of stack
        public abstract void OnDestroy();           // called when the UIPanel is destroyed, call OnDestroy only once during the lifetime of the UIPanel
    }
}