using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Framework.AssetManagement.Runtime
{
    /// <summary>
    /// 资源运行时信息
    /// </summary>
    internal class AssetInfo
    {
        private readonly AssetDescriptor    m_AssetDescriptor;
        private string                      m_GUID;

        internal string guid
        {
            get
            {
                if (!string.IsNullOrEmpty(m_GUID))
                    return m_GUID;

                if (assetType == null)
                    m_GUID = $"{assetPath}[null]";
                else
                    m_GUID = $"{assetPath}[{assetType.Name}]";
                return m_GUID;
            }
        }

        public bool     isValid     { get { return m_AssetDescriptor != null; } }
        public string   assetPath   { private set; get; }
        public Type     assetType   { private set; get; }

        private AssetInfo() { }

        internal AssetInfo(AssetDescriptor assetDescriptor, Type assetType = null)
        {
            if (assetDescriptor == null)
                throw new Exception("assetDescriptor == null");

            this.m_AssetDescriptor  = assetDescriptor;
            this.assetType          = assetType;
            this.assetPath          = assetDescriptor.assetPath;
        }
    }
}