using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.HotFix
{
    public class Singleton<T> : SingletonBase where T : SingletonBase, new()
    {
        private static T m_Instance = null;

        public static T Instance
        {
            get
            {
                if(m_Instance == null || !SingletonBase.Exist(m_Instance))      // destroyAll并不会把Singleton从内存中清除，需要从逻辑上判断是否存在，不存在则再次创建
                {
                    m_Instance = new T();
                }
                return m_Instance;
            }
        }
        
        protected override void InternalInit()
        {}

        protected override void OnDestroy()
        {}

        protected override void OnUpdate(float deltaTime)
        {}
    }
}