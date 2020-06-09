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
        public enum RecognitionState
        {
            Ready,
            Started,
            InProgress,
            Failed,
            Ended,
        }

        private RecognitionState m_CurState = RecognitionState.Ready;
        private RecognitionState m_PrevState = RecognitionState.Ready;

        public RecognitionState State
        {
            get { return m_CurState; }
            set
            {
                if(m_CurState != value)
                {
                    m_PrevState = m_CurState;
                    m_CurState = value;

                    OnStateChanged();
                }
            }
        }

        public RecognitionState PrevState
        {
            get { return m_PrevState; }
        }

        public int Priority;        // 消息处理优先级，值越小越优先处理消息

        [Min(1)][SerializeField]
        private int m_RequiredPointerCount = 1;

        public virtual int requiredPointerCount
        {
            get { return m_RequiredPointerCount; }
            set { m_RequiredPointerCount = value; }
        }
        
        protected virtual void OnStateChanged()
        {
            RaiseEvent();
        }
        protected abstract void RaiseEvent();

        protected virtual void OnEnable()
        {
            GestureRecognizerManager.AddRecognizer(this);
        }

        protected virtual void OnDisable()
        {
            GestureRecognizerManager.RemoveRecognizer(this);
        }

        // 消息处理优先级的缘由，忽略Update，由GestureRecognizerManager统一处理
        internal abstract void InternalUpdate();
    }

    public abstract class GestureRecognizer<T> : GestureRecognizer where T : GestureEventData, new()
    {
        protected T m_EventData = new T();
 
        protected void AddPointer(PointerEventData eventData)
        {
            m_EventData.AddPointerData(eventData);
        }

        protected void RemovePointer(PointerEventData eventData)
        {
            m_EventData.RemovePointerData(eventData);
        }

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
        protected virtual void OnEnded() {}
        protected virtual void OnFailed() {}        
    }


    #if UNITY_EDITOR
    [CustomEditor(typeof(GestureRecognizer), true)]
    public class GestureRecognizerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GestureRecognizer gesture = target as GestureRecognizer;
            GUI.enabled = false;
            EditorGUILayout.EnumFlagsField("State", gesture.State);
            GUI.enabled = true;
        }
    }
    #endif
}