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
            info.srcURL = Application.streamingAssetsPath + "/" + Utility.GetPlatformName() + "/assets/application/tests/runtime/res/prefabpooledobject.ab";
            info.dstURL = "Assets/Temp/11/22/abcd";
            info.retryCount = 3;
            info.verifiedHash = "sdfsdfsdf";
            info.onCompleted = OnExtractCompleted;
            
            ExtractTask task = new ExtractTask(new byte[1024*1024]);
            yield return StartCoroutine(task.Run(info));

            // Debug.Log($"------{task.isRunning}");

            //info.srcURL = Application.streamingAssetsPath + "/" + Utility.GetPlatformName() + "/assets/application/tests/runtime/res/prefabpooledobject.ab";
            //info.dstURL = "Assets/Temp/11/33/efg.cc";
            //yield return StartCoroutine(task.Run(info));

            task.Dispose();
        }

        private void OnExtractCompleted(ExtractTaskInfo data, bool success)
        {
            Debug.Log($"œ¬‘ÿ£∫{data.dstURL} {(success ? "≥…π¶" : " ß∞‹")}");
        }
    }
}