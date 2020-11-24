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
        IEnumerator Start()
        {
            // UnityWebRequest www = new UnityWebRequest(Application.streamingAssetsPath + "/" + Utility.GetPlatformName() + "/assets/application/tests/runtime/res/prefabpooledobject.ab");
            // // if(!Directory.Exists("assets/11/22"))
            // //     Directory.CreateDirectory("assets/11/22");
            // DownloadHandlerFile downloader = new DownloadHandlerFile(new byte[1024*1]);
            // www.disposeDownloadHandlerOnDispose = false;
            // www.SetRequestHeader("Range", "bytes=" + downloader.DownedLength + "-");
            // www.downloadHandler = downloader;
            // yield return www.SendWebRequest();

            // www.Dispose();


            ExtractTaskInfo info = new ExtractTaskInfo();
            info.srcUri = new System.Uri(Path.Combine(Application.streamingAssetsPath, "lilith.zip"));
            //info.srcURL = "assets/11223.pdf";
            info.dstURL = "Assets/Temp/lilith.zip";
            info.retryCount = 0;
            info.verifiedHash = "ec51729b856280fd70b3542dbb2052cb";
            info.onCompleted = OnExtractCompleted;
            
            m_Task = new ExtractTask(new byte[1024*32]);
            yield return StartCoroutine(m_Task.Run(info));

            // Debug.Log($"------{task.isRunning}");

            //info.srcURL = Application.streamingAssetsPath + "/" + Utility.GetPlatformName() + "/assets/application/tests/runtime/res/prefabpooledobject.ab";
            //info.dstURL = "Assets/Temp/11/33/efg.cc";
            //yield return StartCoroutine(task.Run(info));
        }

        private void OnDestroy()
        {
            Debug.LogWarning("------------------");
            m_Task?.Dispose();
        }

        private void OnExtractCompleted(ExtractTaskInfo data, bool success)
        {
            Debug.Log($"œ¬‘ÿ£∫{data.dstURL} {(success ? "≥…π¶" : " ß∞‹")}");
        }
    }
}