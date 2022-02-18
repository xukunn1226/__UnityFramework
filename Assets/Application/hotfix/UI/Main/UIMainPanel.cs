using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Logic
{
    public class UIMainPanel : UIPanelBase
    {
        public UIMainPanel(UIMainDefines defines) : base(defines) { }

        public override void OnInit() { UIManager.Instance.RegisterUpdateEvent(defines.id); }
        public override void OnCreate(GameObject go) { }
        public override void OnShow(object userData = null)
        {
            UIManager.Instance.Open(UIPanelID.MainResources);
            UIManager.Instance.Open(UIPanelID.MainOther);
        }

        public override void OnUpdate(float deltaTime) {}
        public override void OnHide()
        {
            UIManager.Instance.Close(UIPanelID.MainResources);
            UIManager.Instance.Close(UIPanelID.MainOther);
        }
        public override void OnDestroy() {}
    }
}