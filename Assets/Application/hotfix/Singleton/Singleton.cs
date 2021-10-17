using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Runtime
{
    public class Singleton<T> : SingletonBase where T : SingletonBase, new()
    {
        private static T m_Instance = null;

        public static T Instance
        {
            get
            {
                if(m_Instance == null)
                {
                    m_Instance = new T();
                }
                return m_Instance;
            }
        }
        
        protected override void Init()
        {}

        protected override void OnDestroy()
        {}

        protected override void OnUpdate(float deltaTime)
        {}
    }
}