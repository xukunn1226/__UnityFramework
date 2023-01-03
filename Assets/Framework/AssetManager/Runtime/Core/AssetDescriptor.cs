using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    /// <summary>
    /// ��Դ�ļ�������Ϣ
    /// </summary>
    [Serializable]
    public class AssetDescriptor
    {
        /// <summary>
        /// ��Դ·��
        /// </summary>
        public string       assetPath;

        /// <summary>
        /// ������Դ��ID
        /// </summary>
        public int          bundleID;

        /// <summary>
        /// ��������Դ��ID
        /// </summary>
        public int[]        dependIDs;

        private string      m_AddressableName;
        public string       addressableName
        {
            get
            {
                if(!string.IsNullOrEmpty(m_AddressableName))
                    return m_AddressableName;
                
                m_AddressableName = System.IO.Path.GetFileName(assetPath);
                return m_AddressableName;
            }
        }
    }
}