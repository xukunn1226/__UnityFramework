using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    /// <summary>
    /// 资源文件描述信息
    /// </summary>
    [Serializable]
    public class AssetDescriptor
    {
        /// <summary>
        /// 资源路径
        /// </summary>
        public string       assetPath;

        /// <summary>
        /// 所属资源包ID
        /// </summary>
        public int          bundleID;

        /// <summary>
        /// 依赖的资源包ID
        /// </summary>
        public int[]        dependIDs;

        private string      m_AddressableName;
        public string       addressableName
        {
            get
            {
                //if(!string.IsNullOrEmpty(m_AddressableName))
                //    return m_AddressableName;

                //m_AddressableName = System.IO.Path.GetFileName(assetPath);
                //return m_AddressableName;
                return assetPath;
            }
        }
    }
}