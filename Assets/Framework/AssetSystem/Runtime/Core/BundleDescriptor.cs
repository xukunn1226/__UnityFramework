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
    internal class BundleDescriptor
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
        /// �Ƿ���ԭ���ļ������������Լ��Ķ��������ݸ�ʽ������config.db��FMOD�ȣ�
        /// </summary>
        public bool     isRawFile;

        /// <summary>
        /// ���ط�ʽ����EBundleLoadMethod
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