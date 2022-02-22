using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Application.Logic
{
    public class UIPanel_Settings : UIPanelBase
    {
        private RectTransform m_Transform;

        public UIPanel_Settings(UIDefines_Settings defines) : base(defines) {}

        public override void OnInit() { UIManager.Instance.RegisterUpdateEvent(defines.id); }
        public override void OnCreate(GameObject go)
        {
            m_Transform = go.transform.Find("Window").GetComponent<RectTransform>();
        }
        public override void OnShow(object userData = null)
        {
            Tweener paneltweener = m_Transform.DOScale(new Vector3(1, 1, 1), 0.3f).SetEase(Ease.OutBack);
            paneltweener.SetAutoKill(false);
            //paneltweener.Pause();
            m_Transform.DOPlayForward();
        }
        public override void OnUpdate(float deltaTime) {}
        public override void OnHide()
        {
            m_Transform.DOPlayBackwards();
        }
        public override void OnDestroy() {}
    }
}