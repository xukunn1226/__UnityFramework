using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Core
{
    public class SingletonMonoBase : MonoBehaviour
    {
        static protected bool applicationIsQuitting = false;
        static private List<SingletonMonoBase> s_SingletonMonoList = new List<SingletonMonoBase>();

        static protected void Add(SingletonMonoBase singleton)
        {
            s_SingletonMonoList.Add(singleton);
        }

        static public void DestroyAll()
        {
            for(int i = s_SingletonMonoList.Count - 1; i >= 0; --i)
            {
                SingletonMonoBase s = s_SingletonMonoList[i];
                UnityEngine.Object.Destroy(s.gameObject);
            }
            s_SingletonMonoList.Clear();
        }

        /// <summary>
        /// 所有单件销毁后，重设标志位才可以重新实例化单件
        /// </summary>
        static public void Restart()
        {
            applicationIsQuitting = false;
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
}