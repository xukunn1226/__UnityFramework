using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Runtime
{
    public enum LoaderType
    {
        FromEditor,                 // �༭��ģʽʹ�ã��ڲ�ʹ��AssetDatabase������Դ
        FromStreamingAssets,        // ��StreamingAssetsPath�¼�����Դ������ʱ�ͱ༭��ʱ����ʹ��
        FromPersistent,             // ��PersistentDataPath�¼�����Դ������ʱ�����Դ��streamingAssets��ѹ��persistentData
    }
}