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
        public string   assetPath;

        /// <summary>
        /// ������Դ��ID
        /// </summary>
        public int      bundleID;

        /// <summary>
        /// ��������Դ��ID
        /// </summary>
        public int[]    dependIDs;
    }
}