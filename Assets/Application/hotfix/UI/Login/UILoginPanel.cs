using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Logic
{
    public class UILoginPanel : UIPanelBase
    {
        public new UILoginDefines defines { get; private set; }
        public UILoginPanel(UILoginDefines defines) : base(defines) {}

        public override void OnInit() {}
        public override void OnCreate() {}
        public override void OnOpen(bool isReady) {}
        public override void OnOpenAction() {}
        public override void OnUpdate() {}
        public override void OnPause() {}
        public override void OnResume() {}
        public override void OnCloseAction() {}
        public override void OnClose() {}
        public override void OnDestroy() {}
    }
}