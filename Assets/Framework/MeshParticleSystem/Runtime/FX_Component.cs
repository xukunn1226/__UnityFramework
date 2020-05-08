using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework.MeshParticleSystem
{
    public abstract class FX_Component : MonoBehaviour
    {
        public float speed { get; set; } = 1;

        public float elapsedTime { get; protected set; }

        public float deltaTime
        {
            get
            {
                if (m_State == PlayState.Play)
                    return Time.deltaTime * speed;

                return 0;
            }
        }

        public enum PlayState
        {
            Play    = 1,
            Pause   = 2,
            Stop    = 4,
        }
        private PlayState m_State = PlayState.Play;

        public bool isPlaying()
        {
            return (m_State & PlayState.Play) != 0;
        }

        public bool isPaused()
        {
            return (m_State & PlayState.Pause) != 0;
        }

        public bool isStoped()
        {
            return (m_State & PlayState.Stop) != 0;
        }

        public void Play()
        {
            m_State = PlayState.Play;
        }

        public void Pause()
        {
            m_State = PlayState.Pause;
        }

        public void Stop()
        {
            m_State = PlayState.Stop;
            InitEx();
        }

        public void Restart()
        {
            enabled = !enabled;
            enabled = !enabled;
            Play();
        }

        // 通过SetActive重置状态
        protected virtual void OnEnable()
        {
            InitEx();
        }

        protected virtual void InitEx()
        {
            elapsedTime = 0;
        }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(FX_Component), true)]
    public class FX_ComponentInspector : UnityEditor.Editor
    {
        private void OnEnable()
        {
            if (!Application.isPlaying)
                UnityEditor.EditorApplication.update += UnityEditor.EditorApplication.QueuePlayerLoopUpdate;
        }

        private void OnDisable()
        {
            if (!Application.isPlaying)
                UnityEditor.EditorApplication.update -= UnityEditor.EditorApplication.QueuePlayerLoopUpdate;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Play"))
            {
                ((FX_Component)target).Play();
            }
            if (GUILayout.Button("Pause"))
            {
                ((FX_Component)target).Pause();
            }
            if (GUILayout.Button("Stop"))
            {
                ((FX_Component)target).Stop();
            }
            if (GUILayout.Button("Restart"))
            {
                ((FX_Component)target).Restart();
            }
        }
    }
#endif
}