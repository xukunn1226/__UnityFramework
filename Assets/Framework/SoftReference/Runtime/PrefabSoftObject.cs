using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Framework.Core
{
    /// <summary>
    /// 
    /// </summary>
    [ExecuteInEditMode]
    public class PrefabSoftObject : MonoBehaviour
    {
        //[ReadOnly]
        public SoftObjectPath m_SoftReference;

        //private void Awake()
        //{
        //    SoftObjectPath.Add(gameObject, ref m_SoftReference);
        //}

        //private void OnDestroy()
        //{
        //    SoftObjectPath.Remove(gameObject, ref m_SoftReference);
        //}
    }
}