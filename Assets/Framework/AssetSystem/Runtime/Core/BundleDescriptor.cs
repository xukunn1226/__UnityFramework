using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    /// <summary>
    /// 资源包描述信息
    /// </summary>
    [Serializable]
    internal class BundleDescriptor
    {
        /// <summary>
        /// 资源包名称，包括AssetBundle及RawFile
        /// </summary>
        public string   bundleName;

        /// <summary>
        /// 文件Hash
        /// </summary>
        public string   fileHash;

        /// <summary>
        /// 文件CRC
        /// </summary>
        public string   fileCRC;

        /// <summary>
        /// 文件大小（字节数）
        /// </summary>
        public long     fileSize;

        /// <summary>
        /// 是否是原生文件（第三库有自己的二进制数据格式，例如config.db，FMOD等）
        /// </summary>
        public bool     isRawFile;

        /// <summary>
        /// 加载方式，见EBundleLoadMethod
        /// </summary>
        public byte     loadMethod;

        public BundleDescriptor()
        { }

        public bool Equals(BundleDescriptor other)
        {
            if (fileHash == other.fileHash)
                return true;
            return false;
        }
    }
}