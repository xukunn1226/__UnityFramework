using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    /// <summary>
    /// ��Դ��������Ϣ
    /// </summary>
    [Serializable]
    public class BundleDescriptor
    {
        /// <summary>
        /// ��Դ�����ƣ�����AssetBundle��RawFile
        /// </summary>
        public string   bundleName;

        /// <summary>
        /// �ļ�Hash
        /// </summary>
        public string   fileHash;

        /// <summary>
        /// �ļ�CRC
        /// </summary>
        public string   fileCRC;

        /// <summary>
        /// �ļ���С���ֽ�����
        /// </summary>
        public long     fileSize;

        /// <summary>
        /// �Ƿ���ԭ���ļ��������������Լ��Ķ��������ݸ�ʽ������config.db��FMOD�ȣ�
        /// </summary>
        public bool     isRawFile;

        /// <summary>
        /// ���ط�ʽ����EBundleLoadMethod
        /// </summary>
        public byte     loadMethod;

        private string  m_StreamingFilePath;
        public string streamingFilePath
        {
            get
            {
                if (!string.IsNullOrEmpty(m_StreamingFilePath))
                    return m_StreamingFilePath;

                m_StreamingFilePath = string.Empty;
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

                m_CachedFilePath = string.Empty;
                return m_CachedFilePath;
            }
        }
        
        /// <summary>
         /// �ļ�����
         /// </summary>
        private string m_FileName;
        public string FileName
        {
            get
            {
                if (string.IsNullOrEmpty(m_FileName))
                    throw new Exception("Should never get here !");
                return m_FileName;
            }
        }

        /// <summary>
        /// �����ѯKey
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
		/// ������Դ��
		/// </summary>
		public void ParseBundle(string packageName, int nameStype)
        {
            //_packageName = packageName;
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