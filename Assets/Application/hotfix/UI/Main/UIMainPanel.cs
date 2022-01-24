using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Logic
{
    public class UIMainPanel : UIPanelBase
    {
        public new UIMainDefines defines { get; private set; }
        public UIMainPanel(UIMainDefines defines) : base(defines) {}

        public override void OnInit() {}
        public override void OnCreate() {}
        public override void OnOpen(bool isReady) {}
        public override void OnUpdate() {}
        public override void OnPause() {}
        public override void OnResume() {}
        public override void OnClose() {}
        public override void OnDestroy() {}
    }
}