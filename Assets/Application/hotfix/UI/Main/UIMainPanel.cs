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
        public override void OnCreate(GameObject go) { base.OnCreate(go); }
        public override void OnShow(object userData = null) {}
        public override void OnUpdate(float deltaTime) {}
        public override void OnHide() {}
        public override void OnDestroy() {}
    }
}