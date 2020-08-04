using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework.Gesture.Runtime
{
    public class PlayerInput :  MonoBehaviour
    {
        // static public PlayerInput Instance { get; private set; }

        public class HitEventData : BaseEventData
        {
            public RaycastHit hitInfo;
            public HitEventData(EventSystem eventSystem) : base(eventSystem)
            {}

            public override void Reset()
            {
                base.Reset();
                hitInfo = default(RaycastHit);
            }
        }
        private HitEventData m_HitEventData;
        public HitEventData hitEventData
        {
            get
            {
                if(m_HitEventData == null)
                    m_HitEventData = new HitEventData(EventSystem.current);
                m_HitEventData.Reset();
                return m_HitEventData;
            }
        }

        private GameObject m_CurrentSelected;           // 自定义的当前选中对象，不同于EventSystem.m_CurrentSelected
        public GameObject currentSelectedGameObject
        {
            get { return m_CurrentSelected; } 
        }

        private bool m_SelectionGuard;

        // void Awake()
        // {
        //     // 已有PlayerInput，则自毁
        //     if (FindObjectsOfType<PlayerInput>().Length > 1)
        //     {
        //         DestroyImmediate(this);
        //         throw new System.Exception("PlayerInput has already exist...");
        //     }

        //     Instance = this;
        // }

        // void OnDestroy()
        // {
        //     Instance = null;
        // }

        public void SetSelectedGameObject(GameObject selected, HitEventData eventData)
        {
            if (m_SelectionGuard)
            {
                Debug.LogError("Attempting to select " + selected +  "while already selecting an object.");
                return;
            }

            m_SelectionGuard = true;
            if (selected == m_CurrentSelected)
            {
                m_SelectionGuard = false;
                return;
            }

            ExecuteEvents.Execute(m_CurrentSelected, eventData, ExecuteEvents.deselectHandler);
            m_CurrentSelected = selected;
            ExecuteEvents.Execute(m_CurrentSelected, eventData, ExecuteEvents.selectHandler);
            m_SelectionGuard = false;
        }

        public void SetSelectedGameObject(GameObject selected)
        {
            SetSelectedGameObject(selected, hitEventData);
        }
    }
}