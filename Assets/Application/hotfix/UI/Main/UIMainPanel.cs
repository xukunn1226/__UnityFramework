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
        public override void OnShow() {}
        public override void OnUpdate() {}
        public override void OnHide() {}
        public override void OnDestroy() {}
    }
}