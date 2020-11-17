using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

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
            info.userData = 23;
            info.srcURL = Application.streamingAssetsPath + "/" + Utility.GetPlatformName() + "/assets/application/tests/runtime/res/prefabpooledobject.ab";
            info.dstURL = "11/22/abcd";
            
            ExtractTask task = new ExtractTask(new byte[1024]);
            yield return StartCoroutine(task.Run(info));

            info.userData = 34;
            info.srcURL = Application.streamingAssetsPath + "/" + Utility.GetPlatformName() + "/assets/application/tests/runtime/res/prefabpooledobject.ab";
            info.dstURL = "11/33/efg.cc";
            yield return StartCoroutine(task.Run(info));

            task.Dispose();
        }
    }
}