using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Core
{
    public class SingletonMonoBase : MonoBehaviour
    {
        static private List<SingletonMonoBase> s_SingletonMonoList = new List<SingletonMonoBase>();

        static public void Add(SingletonMonoBase singleton)
        {
            s_SingletonMonoList.Add(singleton);
        }

        // !!! 特殊处理：不开放删除接口，由DestroyAll统一处理
        // static public void Remove(SingletonMonoBase singleton)
        // {
        //     s_SingletonMonoList.Remove(singleton);
        // }

        static public void DestroyAll()
        {
            for(int i = s_SingletonMonoList.Count - 1; i >= 0; --i)
            {
                SingletonMonoBase s = s_SingletonMonoList[i];
                UnityEngine.Object.Destroy(s.gameObject);
            }
            s_SingletonMonoList.Clear();
        }
    }
}