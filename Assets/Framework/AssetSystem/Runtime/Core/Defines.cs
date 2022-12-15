using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    public enum EPlayMode
    {
        /// <summary>
        /// �༭����ģ��ģʽ
        /// </summary>
        FromEditor,

        /// <summary>
        /// ����ģʽ
        /// </summary>
        FromStreaming,

        /// <summary>
        /// ����ģʽ
        /// </summary>
        FromHost,
    }

    /// <summary>
    /// Bundle�ļ��ļ��ط�ʽ����AssetBundle.LoadFromFile
    /// </summary>
    public enum EBundleLoadMethod
    {
        LoadFromFile        = 0,
        LoadFromFileOffset  = 1,            // �ӽ����ô˷�ʽ
        //LoadFromMemory      = 2,            // �ݲ�֧�֣��˷�ʽ����������Ǽӽ��ܣ�����׿ƽ̨����Ҫ��ѹ��persistent data path����ʵ�ã��ʷ���
    }

    /// <summary>
    /// Bundle�Ӻδ�������Դ
    /// </summary>
    public enum ELoadMethod
    {
        LoadFromStreaming   = 0,        // ������Դ
        LoadFromCache       = 1,        // ������Դ
        LoadFromRemote      = 2,        // �������
        LoadFromEditor      = 3,        // �༭��ģʽ��
    }

    /// <summary>
    /// ��Դ���ļ���״̬
    /// </summary>
    public enum EBundleLoadStatus
    {
        None                = 0,        // δ��״̬
        Succeed             = 1,        // ���سɹ�
        Failed              = 2,        // ����ʧ��
    }

    /// <summary>
    /// ��Դ�ṩ�ߵ�״̬
    /// </summary>
    public enum EProviderStatus
    {
        None                = 0,
        CheckBundle         = 1,
        Loading             = 2,
        Checking            = 3,
        Succeed             = 4,
        Failed              = 5,
    }

    public enum EOperationStatus
    {
        None,
        Succeed,
        Failed,
    }
}