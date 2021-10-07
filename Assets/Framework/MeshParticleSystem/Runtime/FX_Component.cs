using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshParticleSystem
{
    public abstract class FX_Component : MonoBehaviour
    {
        [SerializeField][HideInInspector]
        private float m_Speed = 1;

        public float speed
        {
            get { return m_Speed; }
            set { m_Speed = value; }
        }

        public float elapsedTime { get; protected set; }           // 由各组件内部使用，重置时强制设置为0

        public float deltaTime
        {
            get
            {
                if (m_State == PlayState.Play)
                    return Time.deltaTime * speed;

                return 0;
            }
        }

        enum PlayState
        {
            Play    = 1,
            Pause   = 2,
            Stop    = 4,
        }
        private PlayState m_State = PlayState.Play;

        public bool isPlaying
        {
            get
            {
                return (m_State & PlayState.Play) != 0;
            }
        }

        public bool isPaused
        {
            get
            {
                return (m_State & PlayState.Pause) != 0;
            }
        }

        public bool isStoped
        {
            get
            {
                return (m_State & PlayState.Stop) != 0;
            }
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
            Init();
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
            Init();
        }

        // 初始化（重置）组件状态
        protected virtual void Init()
        {
            elapsedTime = 0;
        }

        // 记录初始信息，有些组件无需记录，视组件功能而定
        public virtual void RecordInit()
        { }
    }
}