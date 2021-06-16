using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Application.Runtime
{
    static public class EventManager
    {
        static private Dictionary<Enum, Action<EventArgs>>  m_Listeners = new Dictionary<Enum, Action<EventArgs>>();
        static private Dictionary<Type, EventArgs>          m_EventPool = new Dictionary<Type, EventArgs>();

        static public void AddEventListener(Enum type, Action<EventArgs> action)
        {
            if(action == null)
                throw new ArgumentNullException("AddEventListener.action");

            Action<EventArgs> actions = GetEventList(type);
            Delegate[] delegates = actions?.GetInvocationList();
            if(delegates != null)
            {
                if(Array.Exists(delegates, item => item == (Delegate)action))
                {
                    Debug.LogWarning($"duplicated event listener: {type}.{action}");
                }
                else
                {
                    actions += action;
                }
            }
            else
            {
                actions = action;
            }
            m_Listeners[type] = actions;
        }

        static public void RemoveEventListener(Enum type, Action<EventArgs> action)
        {
            if(action == null)
                throw new ArgumentNullException("RemoveEventListener.action");

            Action<EventArgs> actions = GetEventList(type);
            if(actions != null)
            {
                actions -= action;
                m_Listeners[type] = actions;
            }
            else
            {
                Debug.LogWarning($"there is no {type} event list");
            }
        }

        // 移除所有事件
        static public void RemoveAllListener()
        {
            m_Listeners.Clear();
        }

        // 移除某一种类的所有事件
        static public void RemoveEventListener(Enum type)
        {
            m_Listeners.Remove(type);
        }

        static public void Dispatch(EventArgs args)
        {
            Action<EventArgs> actions = GetEventList(args.eventType);
            if(actions == null)
            {
                Debug.LogWarning($"Dispatch failed, because there is no {args.eventType} event listener");
            }
            else
            {
                actions?.Invoke(args);
            }
            ReturnToPool(args);     // EventArgs使用完毕立即回收
        }

        static public T Allocate<T>() where T : EventArgs, new()
        {
            EventArgs args;
            Type type = typeof(T);
            if(m_EventPool.TryGetValue(type, out args) && args != null)
            {
                m_EventPool[type] = null;
                return (T)args;
            }
            return new T();
        }

        static private void ReturnToPool(EventArgs args)
        {
            Type type = args.GetType();            
            if(m_EventPool.ContainsKey(type))
            {
                m_EventPool[type] = args;
            }
            else
            {
                m_EventPool.Add(type, args);
            }
        }

        static private Action<EventArgs> GetEventList(Enum type)
        {
            Action<EventArgs> listener;
            if(m_Listeners.TryGetValue(type, out listener))
            {
                return listener;
            }
            return null;
        }
    }

    static public class EventManagerExtension
    {
        static public void Dispatch(this EventArgs args)
        {
            EventManager.Dispatch(args);
        }
    }
}