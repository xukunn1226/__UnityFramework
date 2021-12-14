using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Core
{
    /// <summary>
    /// Be aware this will not prevent a non singleton constructor
    ///   such as `T myT = new T();`
    /// To prevent that, add `protected T () {}` to your singleton class.
    /// 
    /// As a note, this is made as MonoBehaviour because we need Coroutines.
    /// </summary>
    public class SingletonMono<T> : SingletonMonoBase where T : MonoBehaviour
    {
        private static bool applicationIsQuitting = false;
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (applicationIsQuitting)
                {
                    Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
                        "' already destroyed on application quit." +
                        " Won't create again - returning null.");
                    return null;
                }

                if (_instance == null)
                {
                    GameObject singleton = new GameObject(typeof(T).Name);
                    _instance = singleton.AddComponent<T>();
                }

                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if(FindObjectsOfType(typeof(T)).Length > 1)
            {
                DestroyImmediate(this);
                throw new System.Exception("[Singleton] Something went really wrong " +
                                " - there should never be more than 1 singleton!    " + typeof(T).ToString());
            }

            _instance = this as T;
            _instance.gameObject.name = "[S] " + typeof(T).Name;
            
            transform.parent = null;
            DontDestroyOnLoad(_instance.gameObject);

            Add(this);
        }

        /// <summary>
        /// When Unity quits, it destroys objects in a random order.
        /// In principle, a Singleton is only destroyed when application quits.
        /// If any script calls Instance after it have been destroyed, 
        ///   it will create a buggy ghost object that will stay on the Editor scene
        ///   even after stopping playing the Application. Really bad!
        /// So, this was made to be sure we're not creating that buggy ghost object.
        /// </summary>
        protected virtual void OnDestroy()
        {
            applicationIsQuitting = true;        // !!!特殊处理：单件统一在重启游戏时删除并再次创建，故游戏时单件将始终存在
        }
    }





    
    // public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
    // {
    //     private static T _instance;

    //     //private static object _lock = new object();

    //     public static T Instance
    //     {
    //         get
    //         {
    //             if (applicationIsQuitting && (T)FindObjectOfType(typeof(T)) == null)
    //             {
    //                 Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
    //                     "' already destroyed on application quit." +
    //                     " Won't create again - returning null.");
    //                 return null;
    //             }

    //             //lock (_lock)
    //             {
    //                 if (_instance == null)
    //                 {
    //                     _instance = (T)FindObjectOfType(typeof(T));

    //                     if (FindObjectsOfType(typeof(T)).Length > 1)
    //                     {
    //                         Debug.LogError("[Singleton] Something went really wrong " +
    //                             " - there should never be more than 1 singleton!" +
    //                             " Reopening the scene might fix it.");
    //                         return _instance;
    //                     }

    //                     if (_instance == null)
    //                     {
    //                         GameObject singleton = new GameObject();
    //                         _instance = singleton.AddComponent<T>();
    //                         singleton.name = "(singleton) " + typeof(T).ToString();

    //                         DontDestroyOnLoad(singleton);

    //                         Debug.Log("[Singleton] An instance of " + typeof(T) +
    //                             " is needed in the scene, so '" + singleton +
    //                             "' was created with DontDestroyOnLoad.");
    //                     }
    //                     else
    //                     {
    //                         Debug.Log("[Singleton] Using instance already created: " +
    //                             _instance.gameObject.name);
                            
    //                         _instance.gameObject.transform.parent = null;
    //                         DontDestroyOnLoad(_instance.gameObject);
    //                     }
    //                 }

    //                 return _instance;
    //             }
    //         }
    //     }

    //     private static bool applicationIsQuitting = false;
    //     /// <summary>
    //     /// When Unity quits, it destroys objects in a random order.
    //     /// In principle, a Singleton is only destroyed when application quits.
    //     /// If any script calls Instance after it have been destroyed, 
    //     ///   it will create a buggy ghost object that will stay on the Editor scene
    //     ///   even after stopping playing the Application. Really bad!
    //     /// So, this was made to be sure we're not creating that buggy ghost object.
    //     /// </summary>
    //     public void OnDestroy()
    //     {
    //         applicationIsQuitting = true;
    //     }
    // }
}