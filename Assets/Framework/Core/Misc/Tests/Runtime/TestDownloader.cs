using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using Framework.Core;

namespace Framework.Core.Tests
{
    public class TestDownloader : MonoBehaviour
    {
        private ExtractTask m_Task;

        // Start is called before the first frame update
        //void Start()
        //{
        //    ExtractTaskInfo info = new ExtractTaskInfo();
        //    string path = Path.Combine(Application.streamingAssetsPath, "windows/manifest");
        //    info.srcUri = new System.Uri(path);
        //    info.dstURL = "Assets/Temp/lilith.zip";
        //    info.retryCount = 0;
        //    //info.verifiedHash = "ec51729b856280fd70b3542dbb2052cb";
        //    info.onCompleted = OnExtractCompleted;
        //    info.onRequestError = OnRequestError;
        //    info.onProgress = OnProgress;
        //    info.onDownloadError = OnDownloadError;
            
        //    m_Task = new ExtractTask(new byte[1024*32]);
        //    StartCoroutine(m_Task.Run(info));
        //}

        IEnumerator Start()
        {
            yield return StartCoroutine(Extract());
        }

        IEnumerator Extract()
        {
            string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "windows/manifest");
            //string filePath = "https://raw.githubusercontent.com/xukunn1226/Repo/master/fasthalffloatconversion.pdf";
            System.Uri url = new System.Uri(filePath);
            byte[] data;
            //if(url.ToString().Contains("://") || url.ToString().Contains(":///"))
            {
                Debug.LogError("111111111111");
                
                
                UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(url);
                //www.downloadHandler = new DownloadHandlerFile(Application.persistentDataPath + "/MyFile.pdf", www, new byte[1024 * 1024]);
                yield return www.SendWebRequest();
                Debug.LogError($"---  {www.downloadedBytes}     {www.result}");


                data = www.downloadHandler.data;
                FileStream fs = new FileStream(Application.persistentDataPath + "/MyFile.pdf", FileMode.OpenOrCreate);
                fs.Write(data, 0, data.Length);
                fs.Flush();
                fs.Close();
                fs.Dispose();
            }
            Debug.LogError("222222222222");
        }

        private void OnDestroy()
        {
            m_Task?.Dispose();
        }

        private void OnExtractCompleted(ExtractTaskInfo data, bool success, int tryCount)
        {
            Debug.LogError($"œ¬‘ÿ£∫{data.dstURL} {(success ? "≥…π¶" : " ß∞‹")}");
        }

        private void OnRequestError(ExtractTaskInfo data, string error)
        {
            Debug.LogError("OnRequestError----:   " + error);
        }
        private void OnDownloadError(ExtractTaskInfo data, string error)
        {
            Debug.LogError("OnDownloadError----:   " + error);
        }
        private void OnProgress(ExtractTaskInfo data, ulong downedLength, ulong totalLength, float downloadSpeed)
        {
            Debug.LogError($"OnProgress: {Path.GetFileName(data.dstURL)}     {downedLength}/{totalLength}    downloadSpeed({downloadSpeed})");
        }

    }
}