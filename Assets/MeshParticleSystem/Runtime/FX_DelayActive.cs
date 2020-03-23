using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshParticleSystem
{
    public class FX_DelayActive : MonoBehaviour
    {
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
                            if (p.GetComponent<FX_Root>() != null)
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
            //Debug.LogFormat("OnEnable: {0}     {1}      {2}", gameObject.name, Time.time, Time.frameCount);
            
            // 上层尚未执行完delayActive，则等待
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
                //Debug.LogFormat("Invoke: {0}     {1}      {2}", gameObject.name, Time.time, Time.frameCount);

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
            //Debug.LogFormat("OnStartActive: {0}     {1}      {2}", gameObject.name, Time.time, Time.frameCount);

            if (parent == null || parent.isFinishActive)
            {
                isFinishActive = true;
                gameObject.SetActive(true);
            }
        }
    }
}