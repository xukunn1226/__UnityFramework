using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Framework.AssetManagement.AssetBundleCollector
{
    [Serializable]
    public class AssetBundleCollector
    {
        /// <summary>
        /// �ռ�·��GUID���ļ��л򵥸���Դ�ļ�
        /// </summary>
        public string           CollectGUID;

        public ECollectorType   CollectorType = ECollectorType.MainAssetCollector;

        public string           PackRuleName;

        public string           FilterRuleName;

        public bool IsValid()
        {
            if(string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(CollectGUID)))
            {
                return false;
            }
            return true;
        }
    }

    public enum ECollectorType
    {
        /// <summary>
        /// �ռ�������������Դ��д����Դ�嵥�б���ͨ���������
        /// </summary>
        MainAssetCollector,

        /// <summary>
        /// �ռ�������������Դ����д����Դ�嵥�б�����ͨ���������
        /// </summary>
        StaticAssetCollector,

        /// <summary>
        /// �ռ���������������Դ����д����Դ�嵥�б�����ͨ��������أ����û�б�����Դ�������ã��򲻲��빹��
        /// </summary>
        DependAssetCollector,
    }
}