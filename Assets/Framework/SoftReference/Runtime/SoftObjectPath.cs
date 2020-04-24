using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework.Core
{
    public sealed class SoftObjectPath : MonoBehaviour
    {
#if UNITY_EDITOR
        public string m_GUID;
#endif
        [SerializeField]
        public string m_AssetPath;

        public string assetPath { get { return m_AssetPath; } }






        [Conditional("UNITY_EDITOR")]
        static public void Add(GameObject owner, ref SoftObjectPath comp)
        {
            if (owner == null)
                throw new System.ArgumentNullException("owner");

            if (comp == null)
            {
                comp = owner.AddComponent<SoftObjectPath>();
            }
        }

        [Conditional("UNITY_EDITOR")]
        static public void Remove(GameObject owner, ref SoftObjectPath comp)
        {
#if UNITY_EDITOR            
            if (!EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            if (EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
                return;
#endif

            if (owner == null)
                throw new System.ArgumentNullException("owner");

            if (comp == null)
                throw new System.Exception("SoftObjectPath comp == null");

            if (owner.transform != comp.transform)
                throw new System.Exception("SoftReference.transform != owner.transform");

            DestroyImmediate(comp, true);
        }
    }
}