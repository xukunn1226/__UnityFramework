using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Application.Logic
{
    /// <summary>
    /// 事件系统 ———— ！！！系统存在漏洞，不推荐使用：当传入匿名函数时有问题
    /// Usage：
    ///     1、Defines: 事件枚举（HPEvent）及事件对象（EventArgs_HP）
    ///     2、Receiver: EventManager.AddEventListener(HPEvent.HPChange, OnFoo1);
    ///     3、Dispatcher: EventManager.Allocate<EventArgs_HP>().Set(HPEvent.HPChange, 2).Dispatch();
    /// Lack: 
    ///     the gc brings by enum as a key
    /// Note:
    ///     AddEventListener(Enum type, Action<EventArgs> listener, bool useWeakReference = false, bool isOnce = false)
    ///     监听Monobehaviour对象不推荐使用weakReference，因为其有生命周期函数（OnDestroy），引擎底层有对其进行管理
    ///     监听非Monobehaviour对象可以使用weakReference，一旦使用可以不用RemoveEventListener，在其被GC后系统自动删除
    /// <summary>
    // static public class EventManagerEx
    // {
    //     static private Dictionary<Type, EventArgs>              m_EventPool = new Dictionary<Type, EventArgs>();
    //     static private Dictionary<Enum, List<EventReference>>   m_Listeners = new Dictionary<Enum, List<EventReference>>();
    //     static private List<EventReference>                     m_GCList    = new List<EventReference>();

    //     public class EventReference
    //     {
    //         private WeakReference       m_WeakRef;
    //         private Action<EventArgs>   m_Listener;
    //         private bool                m_isOnce;

    //         public bool                 isOnce { get { return m_isOnce; } }
    //         public Action<EventArgs>    listener
    //         {
    //             get
    //             {
    //                 if(m_Listener != null)
    //                 {
    //                     return m_Listener;
    //                 }
    //                 else if(m_WeakRef != null)
    //                 {
    //                     return m_WeakRef.Target as Action<EventArgs>;
    //                 }
    //                 return null;
    //             }
    //         }

    //         public EventReference(Action<EventArgs> listener, bool useWeakReference = false, bool isOnce = false)
    //         {
    //             if(useWeakReference)
    //             {
    //                 m_WeakRef = new WeakReference(listener);
    //             }
    //             else
    //             {
    //                 m_Listener = listener;
    //             }
    //             m_isOnce = isOnce;
    //         }
    //     }
        
    //     static private List<EventReference> GetEventList(Enum type)
    //     {
    //         List<EventReference> listener;
    //         if(m_Listeners.TryGetValue(type, out listener))
    //         {
    //             return listener;
    //         }
    //         return null;
    //     }

    //     static public void AddEventListener(Enum type, Action<EventArgs> listener, bool useWeakReference = false, bool isOnce = false)
    //     {
    //         if(listener == null)
    //             throw new ArgumentNullException("AddEventListener.listener");

    //         List<EventReference> events = GetEventList(type);
    //         if(events == null)
    //         {
    //             events = new List<EventReference>();
    //             m_Listeners.Add(type, events);
    //         }

    //         if(events.Exists(item => item.listener == listener))
    //         {
    //             Debug.LogWarning($"duplicated event listener: {type}@{listener}");
    //             return;
    //         }

    //         events.Add(new EventReference(listener, useWeakReference, isOnce));
    //     }

    //     static public void RemoveEventListener(Enum type, Action<EventArgs> listener)
    //     {
    //         if(listener == null)
    //             throw new ArgumentNullException("RemoveEventListener.listener");

    //         List<EventReference> events = GetEventList(type);
    //         if(events == null)
    //         {
    //             Debug.LogWarning($"there is no {type} event list");
    //             return;
    //         }

    //         int index = events.FindIndex(item => item.listener == listener);
    //         if(index == -1)
    //         {
    //             Debug.LogWarning($"there is no listener: {listener}");
    //             return;
    //         }
    //         events.RemoveAt(index);

    //         if(events.Count == 0)
    //         {
    //             m_Listeners.Remove(type);
    //         }
    //     }

    //     // 移除所有事件
    //     static public void RemoveAllListener()
    //     {
    //         m_Listeners.Clear();
    //     }

    //     // 移除某一种类的所有事件
    //     static public void RemoveEventListener(Enum type)
    //     {
    //         m_Listeners.Remove(type);
    //     }

    //     static public void Dispatch(EventArgs args)
    //     {
    //         List<EventReference> events = GetEventList(args.eventType);
    //         if(events != null)
    //         {
    //             events.ForEach(item =>
    //             {
    //                 if(item.listener != null)
    //                 {
    //                     try
    //                     {
    //                         item.listener.Invoke(args);
    //                     }
    //                     catch(Exception e)
    //                     {
    //                         Debug.LogError($"Dispatch: {e.Message}");
    //                     }
    //                 }

    //                 // collect the pending removed EventReference    
    //                 if(item.listener == null || item.isOnce)
    //                 {
    //                     m_GCList.Add(item);
    //                 }
    //             });

    //             m_GCList.ForEach(item => events.Remove(item));
    //             m_GCList.Clear();
    //         }
    //         ReturnToPool(args);     // EventArgs使用完毕立即回收
    //     }

    //     static public T Allocate<T>() where T : EventArgs, new()
    //     {
    //         EventArgs args;
    //         Type type = typeof(T);
    //         if(m_EventPool.TryGetValue(type, out args) && args != null)
    //         {
    //             m_EventPool[type] = null;
    //             return (T)args;
    //         }
    //         return new T();
    //     }

    //     static private void ReturnToPool(EventArgs args)
    //     {
    //         Type type = args.GetType();            
    //         if(m_EventPool.ContainsKey(type))
    //         {
    //             m_EventPool[type] = args;
    //         }
    //         else
    //         {
    //             m_EventPool.Add(type, args);
    //         }
    //     }        
    // }

    // static public class EventManagerExtension
    // {
    //     static public void Dispatch(this EventArgs args)
    //     {
    //         EventManager.Dispatch(args);
    //     }
    // }
}