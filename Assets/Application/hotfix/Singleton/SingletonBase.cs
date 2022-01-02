using System.Collections;
using System.Collections.Generic;

namespace Application.Logic
{
    public abstract class SingletonBase
    {
        static private LinkedList<SingletonBase> s_SingletonList = new LinkedList<SingletonBase>();
        static public int frameCount { get; private set; }

        static private void Add(SingletonBase singleton)
        {
            UnityEngine.Debug.Assert(s_SingletonList.Find( singleton ) == null);
            
            singleton.Init();
            s_SingletonList.AddLast(singleton);
        }

        static private void Remove(SingletonBase singleton)
        {
            UnityEngine.Debug.Assert(s_SingletonList.Find( singleton ) != null);

            singleton.pendingKill = true;
        }

        static internal bool Exist(SingletonBase singleton)
        {
            return s_SingletonList.Find(singleton) != null;
        }

        static public void DestroyAll()
        {
            foreach(var s in s_SingletonList)
            {
                s.pendingKill = true;
            }
        }

        static public void Update(float deltaTime)
        {
            ++frameCount;
            LinkedListNode<SingletonBase> cur = s_SingletonList.First;
            while(cur != null)
            {
                SingletonBase s = cur.Value;
                if(s.pendingKill)
                {
                    s.OnDestroy();
                    s_SingletonList.RemoveFirst();
                    cur = s_SingletonList.First;
                }
                else
                {
                    if(s.updateIntervalTime <= 0)
                    {
                        s.OnUpdate(deltaTime);
                    }
                    else
                    {
                        s.elapsedUpdateTime += deltaTime;
                        if(s.elapsedUpdateTime >= s.updateIntervalTime)
                        {
                            s.OnUpdate(deltaTime);
                            s.elapsedUpdateTime = (s.elapsedUpdateTime * 1000) % (s.updateIntervalTime * 1000);
                        }
                    }
                    cur = cur.Next;
                }
            }
        }





        protected bool  pendingKill                 { get; set; }
        private float   m_UpdateIntervalTime;       // 更新间隔，<= 0表示每帧更新        
        public float    updateIntervalTime
        {
            get { return m_UpdateIntervalTime; }
            set
            {
                if(m_UpdateIntervalTime != value)
                {
                    m_UpdateIntervalTime = value;
                    elapsedUpdateTime = 0;
                }
            }
        }        
        protected float elapsedUpdateTime           { get; set; }

        public SingletonBase()
        {
            SingletonBase.Add(this);
        }

        public void Destroy()
        {
            SingletonBase.Remove(this);
        }

        private bool m_Init;
        public void Init()
        {
            if(!m_Init)
            {
                InternalInit();
                m_Init = true;
            }
        }

        protected abstract void InternalInit();

        protected abstract void OnDestroy();

        protected abstract void OnUpdate(float deltaTime);
    }
}