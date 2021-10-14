using System.Collections;
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
        public string       m_GUID;

        [SerializeField]
        private string      m_AssetPath;
        public string       assetPath
        { 
            get { return m_AssetPath; }
            set
            {
                m_AssetPath = value;

                m_BundleName = null;
                m_AssetName = null;

                int index = assetPath.LastIndexOf(@"/");
                if (index == -1)
                    return;

                m_AssetName = assetPath.Substring(index + 1);
                m_BundleName = assetPath.Substring(0, index) + ".ab";

                m_GUID = AssetDatabase.AssetPathToGUID(m_AssetPath);
            }
        }
#endif

#pragma warning disable 0649
        [SerializeField]
        private string      m_BundleName;
        [SerializeField]
        private string      m_AssetName;
#pragma warning restore 0649

        public string       bundleName    { get { return m_BundleName; } }
        public string       assetName     { get { return m_AssetName; } }

        static public bool IsValid(SoftObjectPath sop)
        {
            return sop != null && !string.IsNullOrEmpty(sop.bundleName) && !string.IsNullOrEmpty(sop.assetName);
        }
    }
}