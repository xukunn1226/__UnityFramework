﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework.Core
{
    public class SoftObjectPath : MonoBehaviour
    {
#if UNITY_EDITOR
        public string m_GUID;
#endif

        [SerializeField]
        private string m_AssetPath;

        public string assetPath 
        { 
            get { return m_AssetPath; }
            set
            {
                m_AssetPath = value;
            }
        }

        static public bool IsValid(SoftObjectPath sop)
        {
            return sop != null && !string.IsNullOrEmpty(sop.assetPath);
        }
    }
}