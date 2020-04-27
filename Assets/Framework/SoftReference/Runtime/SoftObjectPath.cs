using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
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

        public string m_AssetPath;

        public string assetPath { get { return m_AssetPath; } }
    }
}