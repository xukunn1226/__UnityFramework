using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Application.Logic
{
    public class ZActor : ZEntity
    {
        private List<ZComp> m_CompsList = new List<ZComp>();

        public ZActor()
        {}

        public ZActor(string name)
        {
            this.name = name;
        }

        public override void Destroy()
        {
            // 以添加组件的反序来执行
            for(int i = m_CompsList.Count - 1; i >= 0; --i)
            {
                m_CompsList[i].Destroy();
            }
            m_CompsList.Clear();
            base.Destroy();
        }

        // IL jit模式会抛异常，暂屏蔽
        // public ZComp AddComponent(Type compType, IDataSource data = null)
        // {
        //     ZComp comp = (ZComp)Activator.CreateInstance(compType);
        //     if(comp == null)
        //         throw new ArgumentException($"the type of {compType} is not ZComp");
        //     comp.actor = this;
        //     m_CompsList.Add(comp);
        //     comp.Prepare(data);
        //     return comp;
        // }

        public T AddComponent<T>(IDataSource data = null) where T : ZComp, new()
        {
            // T comp = (T)Activator.CreateInstance(typeof(T), new object[] { this });          // ILRuntime暂不支持传参数的CreateInstance
            T comp = (T)Activator.CreateInstance<T>();
            comp.actor = this;
            m_CompsList.Add(comp);
            comp.Prepare(data);
            return comp;
        }

        public void RemoveComponent(ZComp comp)
        {
            int index = m_CompsList.BinarySearch(comp);
            Debug.Assert(index != -1);
            m_CompsList[index].Destroy();
            m_CompsList.RemoveAt(index);
        }

        public ZComp GetComponent(Type type)
        {
            // return the first result
            foreach(var comp in m_CompsList)
            {
                if(comp.GetType() == type)
                    return comp;
            }
            return null;
        }

        public T GetComponent<T>() where T : ZComp
        {
            foreach(var comp in m_CompsList)
            {
                if(comp.GetType() == typeof(T))
                    return (T)comp;
            }
            return default(T);
        }

        public ZComp[] GetComponents(Type type)
        {
            List<ZComp> comps = new List<ZComp>();
            foreach(var comp in m_CompsList)
            {
                if(comp.GetType() == type)
                {
                    comps.Add(comp);
                }
            }
            return comps.ToArray();
        }

        public T[] GetComponents<T>() where T : ZComp
        {
            List<T> comps = new List<T>();
            foreach(var comp in m_CompsList)
            {
                if(comp.GetType() == typeof(T))
                {
                    comps.Add((T)comp);
                }
            }
            return comps.ToArray();
        }
    }
}