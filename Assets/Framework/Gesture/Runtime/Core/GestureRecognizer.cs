using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework.Gesture.Runtime
{
    public abstract class GestureRecognizer : MonoBehaviour
    {
        [HideInInspector] public int Priority;        // 消息处理优先级，值越小越优先处理消息

        public virtual int requiredPointerCount
        {
            get { return 1; }
        }
        
        protected virtual void OnEnable()
        {
            GestureRecognizerManager.AddRecognizer(this);
        }

        protected virtual void OnDisable()
        {
            GestureRecognizerManager.RemoveRecognizer(this);
        }
        internal abstract void InternalUpdate();
        internal abstract void ProcessWhenLostFocus();

        protected virtual GameObject eventTarget { get { return gameObject; } }
    }

    public abstract class GestureRecognizer<T> : GestureRecognizer where T : GestureEventData, new()
    {
        protected T m_EventData = new T();
        protected PlayerInputModule m_InputModule;
        protected PlayerInputModule InputModule
        {
            get
            {
                if(m_InputModule == null)
                {
                    m_InputModule = EventSystem.current.currentInputModule as PlayerInputModule;
                }
                return m_InputModule;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            m_EventData.OnStateChanged += OnStateChanged;
        }

        protected override void OnDisable()
        {
            m_EventData.OnStateChanged -= OnStateChanged;
            base.OnDisable();
        }

        private void OnStateChanged(GestureEventData eventData)
        {
            RaiseEvent(eventData);
        }
        protected abstract void RaiseEvent(GestureEventData eventData);

        protected void ClearPointers()
        {
            m_EventData.ClearPointerDatas();
        }

        protected virtual bool CanBegin()
        {
            return m_EventData.pointerCount == requiredPointerCount;
        }

        protected virtual void OnBegin()
        {
            m_EventData.StartTime = Time.time;
            m_EventData.PressPosition = m_EventData.GetAveragePressPosition(requiredPointerCount);
            m_EventData.Position = m_EventData.GetAveragePosition(requiredPointerCount);
        }

        protected abstract RecognitionState OnProgress();

        // 消息处理优先级的缘由，忽略Update，由GestureRecognizerManager统一处理
        internal override void InternalUpdate()
        {
            if(m_EventData.State == RecognitionState.Ready)
            {
                if(CanBegin())
                {
                    OnBegin();
                    m_EventData.State = RecognitionState.Started;
                }
            }
            
            if(m_EventData.State == RecognitionState.Started)
                m_EventData.State = RecognitionState.InProgress;

            switch(m_EventData.State)
            {
                case RecognitionState.InProgress:
                    m_EventData.State = OnProgress();
                    break;
                case RecognitionState.Failed:
                case RecognitionState.Ended:
                    m_EventData.ClearPointerDatas();
                    m_EventData.State = RecognitionState.Ready;
                    break;
            }
        }

        internal override void ProcessWhenLostFocus()
        {
            // 强制处于运行态的recognizer结束
            if(m_EventData.State == RecognitionState.InProgress)
            {
                m_EventData.State = RecognitionState.Ended;
            }            
        }
    }


    // #if UNITY_EDITOR
    // [CustomEditor(typeof(GestureRecognizer), true)]
    // public class GestureRecognizerEditor : Editor
    // {
    //     public override void OnInspectorGUI()
    //     {
    //         DrawDefaultInspector();

    //         GestureRecognizer gesture = target as GestureRecognizer;
    //         GUI.enabled = false;
    //         EditorGUILayout.EnumFlagsField("State", gesture.eventData.State);
    //         GUI.enabled = true;
    //     }
    // }
    // #endif
}