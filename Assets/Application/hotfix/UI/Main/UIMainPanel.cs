using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Logic
{
    public class UIMainPanel : UIPanelBase
    {
        public new UIMainDefines defines { get; private set; }
        public UIMainPanel(UIMainDefines defines) : base(defines) {}

        public override void OnCreate() {}
        public override void OnOpen() {}
        public override void OnOpenAction() {}
        public override void OnUpdate() {}
        public override void OnPause() {}
        public override void OnResume() {}
        public override void OnCloseAction() {}
        public override void OnClose() {}
        public override void OnDestroy() {}
    }
}