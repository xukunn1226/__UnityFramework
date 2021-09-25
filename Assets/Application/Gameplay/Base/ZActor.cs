using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Application.Runtime
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

        public override void Uninit()
        {
            foreach(var comp in m_CompsList)
            {
                comp.Uninit();
            }
            m_CompsList.Clear();
            base.Uninit();
        }

        public ZComp AddComponent(Type compType)
        {
            ZComp comp = (ZComp)Activator.CreateInstance(compType, new object[] { this });
            if(comp == null)
                throw new ArgumentException($"the type of {compType} is not ZComp");
            m_CompsList.Add(comp);
            comp.Init();
            return comp;
        }

        public T AddComponent<T>() where T : ZComp
        {
            T comp = (T)Activator.CreateInstance(typeof(T), new object[] { this });
            m_CompsList.Add(comp);
            comp.Init();
            return comp;
        }

        public void RemoveComponent(ZComp comp)
        {
            int index = m_CompsList.BinarySearch(comp);
            Debug.Assert(index != -1);
            m_CompsList[index].Uninit();
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