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

        public abstract void OnCreate();
        public abstract void OnOpen();
        public abstract void OnOpenAction();
        public abstract void OnUpdate();
        public abstract void OnPause();
        public abstract void OnResume();
        public abstract void OnCloseAction();
        public abstract void OnClose();
        public abstract void OnDestroy();        
    }
}