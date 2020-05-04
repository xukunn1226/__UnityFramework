using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.MeshParticleSystem
{
    public class FX_DelayActive : FX_Component, IReplay
    {
#if UNITY_2019_1_OR_NEWER
        [Min(0.01f)]
#endif
        public float Delay;                      // 相对父节点（FX_DelayActive）的延迟时间

        private bool isFinishActive              // 是否已激活
        {
            get; set;
        }

        private bool               m_isFoundParent;
        private FX_DelayActive     m_Parent;
        private FX_DelayActive     parent
        {
            get
            {
                if (!m_isFoundParent)
                {
                    Transform p = transform.parent;
                    while (p != null)
                    {
                        FX_DelayActive d = p.GetComponent<FX_DelayActive>();
                        if (d != null)
                        {
                            m_Parent = d;
                            break;
                        }
                        else
                        {
                            if (p.GetComponent<IFX_Root>() != null)
                                break;      // 根节点停止查询
                            p = p.transform.parent;
                        }
                    }
                    m_isFoundParent = true;
                }
                return m_Parent;
            }
        }

        private void OnEnable()
        {
            //Debug.LogFormat("OnEnable: Name[{0}]     Time[{1}]      Frame[{2}]", gameObject.name, Time.time, Time.frameCount);

            // 若有父节点（FX_DelayActive），且尚未active则等待
            if (parent != null && !parent.isFinishActive)
            {
                return;
            }
            
            if (Delay <= 0)
            {
                isFinishActive = true;
                return;
            }

            if (!isFinishActive)
            {
                //Debug.LogFormat("Invoke: Name[{0}]     Time[{1}]      Frame[{2}]", gameObject.name, Time.time, Time.frameCount);

                Invoke("OnStartActive", Delay);

                gameObject.SetActive(false);
            }
        }

        private void OnDisable()
        {
            isFinishActive = false;
        }

        void OnStartActive()
        {
            //Debug.LogFormat("---------------OnStartActive: Name[{0}]     Time[{1}]      Frame[{2}]", gameObject.name, Time.time, Time.frameCount);

            if (parent == null || parent.isFinishActive)
            {
                isFinishActive = true;
                gameObject.SetActive(true);
            }
        }

        public void Replay()
        {
            enabled = !enabled;
            enabled = !enabled;
        }
    }
}