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
    public class BundleDescriptor
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
        /// 是否是原生文件（第三方库有自己的二进制数据格式，例如config.db，FMOD等）
        /// </summary>
        public bool     isRawFile;

        /// <summary>
        /// 加载方式，见EBundleLoadMethod
        /// </summary>
        public byte     loadMethod;

        private string  m_PackageName;

        private string  m_StreamingFilePath;
        public string streamingFilePath
        {
            get
            {
                if (!string.IsNullOrEmpty(m_StreamingFilePath))
                    return m_StreamingFilePath;

                m_StreamingFilePath = PathHelper.MakeStreamingLoadPath(fileName);
                return m_StreamingFilePath;
            }
        }

        private string  m_CachedFilePath;
        public string cachedFilePath
        {
            get
            {
                if(!string.IsNullOrEmpty(m_CachedFilePath))
                    return m_CachedFilePath;

                string cacheRoot = PathHelper.GetCacheFolderPath(m_PackageName);
                m_CachedFilePath = $"{cacheRoot}/{fileName}";
                return m_CachedFilePath;
            }
        }
        
        /// <summary>
         /// 文件名称
         /// </summary>
        private string m_FileName;
        public string fileName
        {
            get
            {
                if (string.IsNullOrEmpty(m_FileName))
                    throw new Exception("Should never get here !");
                return m_FileName;
            }
        }

        /// <summary>
        /// 缓存查询Key
        /// </summary>
        private string m_CacheKey;
        public string CacheKey
        {
            get
            {
                if (string.IsNullOrEmpty(m_CacheKey))
                    throw new Exception("Should never get here !");
                return m_CacheKey;
            }
        }

        public BundleDescriptor()
        { }

        /// <summary>
		/// 解析资源包
		/// </summary>
		public void ParseBundle(string packageName, int nameStype)
        {
            m_PackageName = packageName;
            m_CacheKey = $"{packageName}-{fileHash}";
            m_FileName = AssetManifest.CreateBundleFileName(nameStype, bundleName, fileHash);
        }

        public bool Equals(BundleDescriptor other)
        {
            if (fileHash == other.fileHash)
                return true;
            return false;
        }
    }
}