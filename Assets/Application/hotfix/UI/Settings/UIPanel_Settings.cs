using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Application.Logic
{
    public class UIPanel_Settings : UIPanelBase
    {
        private RectTransform m_Transform;
        private Tweener m_PanelTweener;
        private Button btn_Close;
        private Transform m_MaskTrans;

        public UIPanel_Settings(UIDefines_Settings defines) : base(defines) {}

        public override void OnInit() { UIManager.Instance.RegisterUpdateEvent(defines.id); }
        public override void OnCreate(GameObject go)
        {
            // binding event
            m_Transform = go.transform.Find("Window").GetComponent<RectTransform>();
            btn_Close = go.transform.Find("Window/Button_Close").GetComponent<Button>();
            btn_Close.onClick.AddListener(delegate ()
            {
                this.Close();
            });

            m_MaskTrans = go.transform.Find("BackgroundFade");
            EventTrigger eventTrigger = m_MaskTrans.GetComponent<EventTrigger>();
            if (eventTrigger == null)
            {
                eventTrigger = m_MaskTrans.gameObject.AddComponent<EventTrigger>();
            }
            eventTrigger.AddEventTriggerListener(EventTriggerType.PointerClick, (data) => { this.Close(); });

            // 建议放入OnCreate创建
            m_PanelTweener = m_Transform.DOScale(new Vector3(1, 1, 1), 0.3f).SetEase(Ease.OutBack);
            m_PanelTweener.SetAutoKill(false);
            m_PanelTweener.Pause();
        }

        public override void OnShowAction()
        {
            Debug.Log("OnShowAction");
            
            m_Transform.DOPlayForward();
        }

        public override void OnHideAction(Action onComplete)
        {
            Debug.Log("OnHideAction");
            
            m_Transform.DOPlayBackwards();
            m_PanelTweener.onRewind = () => { onComplete?.Invoke(); };
        }

        public override void OnDestroy()
        {
            m_PanelTweener.Kill();
        }        
    }
}