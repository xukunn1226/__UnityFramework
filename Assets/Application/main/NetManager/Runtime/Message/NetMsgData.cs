using Framework.Cache;
using System;

namespace Application.Runtime
{
        
    public class NetMsgData : IPooledObject
    {
        static private ObjectPool<NetMsgData> m_Pool;
        static private int m_kInitSize = 20;
        static public int MsgMaxSize = 1024;
        public int MsgID = 0;
        public byte[] MsgData = null;
        public int MsgLen = 0;

        public IPool Pool 
        {
            get
            {
                if (m_Pool == null)
                {
                    m_Pool = new ObjectPool<NetMsgData>(m_kInitSize);
                }
                return m_Pool;
            }
            set
            {
                throw new System.AccessViolationException();
            }
        }

        public int GetTypeID()
        {
            return MsgID >> 16;
        }

        public int GetMsgID()
        {
            return MsgID & 0xffff;
        }

        public bool CopyFrom(byte[] data, int srcOffset, int copyLen)
        {
            if (copyLen < MsgMaxSize)
            {
                Buffer.BlockCopy(data, srcOffset, MsgData, 0, copyLen);
                MsgLen = copyLen;
                return true;
            }

            UnityEngine.Debug.LogError($"NetMsgData->CopyFrom copyLen too large : {copyLen}");
            return false;
        }
        

        public void OnInit()
        {
            //Debug.Log("NetMsgData::OnInit");
        }

        public void OnGet()
        {
            //Debug.Log("NetMsgData::OnGet");
            if (MsgData == null)
            {
                MsgData = new byte[MsgMaxSize];
            }
            MsgLen = 0;
        }

        public void OnRelease()
        {
            //Debug.Log("NetMsgData::OnRelease");
        }

        public void ReturnToPool()
        {
            //Debug.Log("NetMsgData::ReturnToPool");
        }

        public static NetMsgData Get()
        {
            if (m_Pool == null)
            {
                m_Pool = new ObjectPool<NetMsgData>(m_kInitSize);
            }

            return (NetMsgData)m_Pool.Get();
        }

        public static void Release(NetMsgData f)
        {
            if (m_Pool == null)
            {
                UnityEngine.Debug.LogError($"Pool[{f.GetType().Name}] not exist");
                return;
            }

            m_Pool.Return(f);
        }
    }
}
