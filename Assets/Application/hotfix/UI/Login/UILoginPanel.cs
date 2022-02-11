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
        public override void OnShow(object userData = null) {}
        public override void OnUpdate() {}
        public override void OnHide() {}
        public override void OnDestroy() {}
    }
}