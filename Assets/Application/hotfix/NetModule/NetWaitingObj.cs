using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Runtime
{
    public class NetWaitingObj
    {
        private int msgId;

        private bool m_received = false;

        //private float m_timeout = 5.0f;

        public NetWaitingObj(int msgId)
        {
            this.msgId = msgId;
        }

        public IEnumerator Wait()
        {
            while (!m_received)
            {
                yield return 0;
            }
        }

        public void OnReceive()
        {
            m_received = true;
        }



    }
}
