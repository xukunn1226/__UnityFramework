using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Application.Runtime
{
    // https://github.com/Bian-Sh/UniEventSystem
    // http://www.manew.com/blog-52341-2366.html
    static public class EventManager
    {
        static private Dictionary<string, Delegate> m_Listeners = new Dictionary<string, Delegate>();

        static public bool HasEventListener(string evt)
        {
            return m_Listeners.ContainsKey(evt);
        }

        static private void AddEventListener(string evt, Delegate func)
        {
            if(func == null)
                throw new ArgumentNullException("AddEventListener.func");

            Delegate listener;
            if(m_Listeners.TryGetValue(evt, out listener))
            {
                Delegate[] actions = listener.GetInvocationList();
                if(actions == null)
                    throw new ArgumentNullException("actions");
                
                if(Array.Exists(actions, v => v == (Delegate)func))
                {
                    Debug.LogWarning($"duplicated event listener: {evt}.{func}");
                }
                else
                {
                    m_Listeners[evt] = Delegate.Combine(listener, func);
                }
            }
            else
            {
                m_Listeners[evt] = func;
            }
        }

        static public void AddEventListener(string evt, Action func)
        {
            AddEventListener(evt, (Delegate)func);
        }

        static public void AddEventListener<T>(string evt, Action<T> func)
        {
            AddEventListener(evt, (Delegate)func);
        }

        static public void AddEventListener<T1, T2>(string evt, Action<T1, T2> func)
        {
            AddEventListener(evt, (Delegate)func);
        }

        static public void AddEventListener<T1, T2, T3>(string evt, Action<T1, T2, T3> func)
        {
            AddEventListener(evt, (Delegate)func);
        }

        static public void AddEventListener<T1, T2, T3, T4>(string evt, Action<T1, T2, T3, T4> func)
        {
            AddEventListener(evt, (Delegate)func);
        }

        static private void RemoveEventListener(string evt, Delegate func)
        {
            if(func == null)
                throw new ArgumentNullException("RemoveEventListener.func");

            Delegate listener;
            if(m_Listeners.TryGetValue(evt, out listener))
            {
                listener = Delegate.Remove(listener, func);
                if(listener == null)
                {
                    m_Listeners.Remove(evt);
                }
                else
                {
                    m_Listeners[evt] = listener;
                }
            }
        }

        static public void RemoveEventListener(string evt, Action func)
        {
            RemoveEventListener(evt, (Delegate)func);
        }

        static public void RemoveEventListener<T>(string evt, Action<T> func)
        {
            RemoveEventListener(evt, (Delegate)func);
        }

        static public void RemoveEventListener<T1, T2>(string evt, Action<T1, T2> func)
        {
            RemoveEventListener(evt, (Delegate)func);
        }

        static public void RemoveEventListener<T1, T2, T3>(string evt, Action<T1, T2, T3> func)
        {
            RemoveEventListener(evt, (Delegate)func);
        }

        static public void RemoveEventListener<T1, T2, T3, T4>(string evt, Action<T1, T2, T3, T4> func)
        {
            RemoveEventListener(evt, (Delegate)func);
        }

        // 移除所有事件
        static public void RemoveAllListener()
        {
            m_Listeners.Clear();
        }

        // 移除某一种类的所有事件
        static public void RemoveEventListener(string evt)
        {
            m_Listeners.Remove(evt);
        }

        static public void Dispatch(string evt)
        {
            Delegate[] methods = GetMethods(evt);
            if(methods != null)
            {
                foreach(Delegate f in methods)
                {
                    try
                    {
                        if(f.Target != null)
                        {
                            ((Action)f).Invoke();
                        }
                    }
                    catch(Exception e)
                    {
                        Debug.LogError(e);
                    }
                }
            }
        }

        static public void Dispatch<T>(string evt, T arg)
        {
            Delegate[] methods = GetMethods(evt);
            if(methods != null)
            {
                foreach(Delegate f in methods)
                {
                    try
                    {
                        if(f.Target != null)
                        {
                            ((Action<T>)f).Invoke(arg);
                        }
                    }
                    catch(Exception e)
                    {
                        Debug.LogError(e);
                    }
                }
            }
        }

        static public void Dispatch<T1, T2>(string evt, T1 arg1, T2 arg2)
        {
            Delegate[] methods = GetMethods(evt);
            if(methods != null)
            {
                foreach(Delegate f in methods)
                {
                    try
                    {
                        if(f.Target != null)
                        {
                            ((Action<T1, T2>)f).Invoke(arg1, arg2);
                        }
                    }
                    catch(Exception e)
                    {
                        Debug.LogError(e);
                    }
                }
            }
        }

        static public void Dispatch<T1, T2, T3>(string evt, T1 arg1, T2 arg2, T3 arg3)
        {
            Delegate[] methods = GetMethods(evt);
            if(methods != null)
            {
                foreach(Delegate f in methods)
                {
                    try
                    {
                        if(f.Target != null)
                        {
                            ((Action<T1, T2, T3>)f).Invoke(arg1, arg2, arg3);
                        }
                    }
                    catch(Exception e)
                    {
                        Debug.LogError(e);
                    }
                }
            }
        }

        static public void Dispatch<T1, T2, T3, T4>(string evt, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            Delegate[] methods = GetMethods(evt);
            if(methods != null)
            {
                foreach(Delegate f in methods)
                {
                    try
                    {
                        if(f.Target != null)
                        {
                            ((Action<T1, T2, T3, T4>)f).Invoke(arg1, arg2, arg3, arg4);
                        }
                    }
                    catch(Exception e)
                    {
                        Debug.LogError(e);
                    }
                }
            }
        }

        static private Delegate[] GetMethods(string evt)
        {
            Delegate listener;
            if(m_Listeners.TryGetValue(evt, out listener))
            {
                return listener.GetInvocationList();
            }
            return null;
        }
    }
}