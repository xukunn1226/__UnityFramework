using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.IO;

namespace Framework.Core
{
    /// <summary>
    /// 1������backdoor.json
    /// 2����ȡPlayerPrefs��backdoor�����°汾�ŶԱȣ��ж��Ƿ������°汾�򲹶����������
    /// 3����ʼ�����ػ���
    /// 4�����ز���
    /// 5�����PlayerPrefs���
    /// </summary>
    public class Patcher : MonoBehaviour
    {
        static private string               CUR_APPVERSION              = "CurAppVersion_fe2679cf89a145ccb45b715568e6bc07";

        private int                         m_WorkerCount               = 5;
        private List<DownloadTask>          m_TaskWorkerList;
        private List<byte[]>                m_CachedBufferList;
        private const int                   m_BufferSize                = 1024 * 1024;

        private AppVersion                  m_CurVersion;

        private void OnEnable()
        {
            StartCoroutine(DownloadBackdoor());
        }

        public void StartWork(int workerCount)
        { }

        private IEnumerator DownloadBackdoor()
        {
            string s = Path.Combine("Deployment/CDN", "backdoor.json");
            Uri backdoorURL = new Uri(s);
            UnityWebRequest www = UnityWebRequest.Get(backdoorURL);
            www.downloadHandler = new UnityEngine.Networking.DownloadHandlerFile(string.Format($"{Application.persistentDataPath}/backdoor.json"));
            yield return www.SendWebRequest();

            if(www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Success");
            }
        }
    }
}