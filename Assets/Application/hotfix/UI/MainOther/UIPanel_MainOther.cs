using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Logic
{
    public class UIPanel_MainOther : UIPanelBase
    {
        public UIPanel_MainOther(UIDefines_MainOther defines) : base(defines) {}

        public override void OnInit() { UIManager.Instance.RegisterUpdateEvent(defines.id); }
        public override void OnCreate(GameObject go) { }
        public override void OnShow(object userData = null) {}
        public override void OnUpdate(float deltaTime) {}
        public override void OnHide() {}
        public override void OnDestroy() {}
    }
}