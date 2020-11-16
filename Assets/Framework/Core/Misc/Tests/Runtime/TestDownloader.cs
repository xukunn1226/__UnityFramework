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
            UnityWebRequest www = new UnityWebRequest(Application.streamingAssetsPath + "/" + Utility.GetPlatformName() + "/assets/application/tests/runtime/res/prefabpooledobject.ab");
            if(!Directory.Exists("assets/11/22"))
                Directory.CreateDirectory("assets/11/22");
            DownloadHandlerFile downloader = new DownloadHandlerFile("assets/11/22/prefabpooledobject.ab", new byte[1024*1]);
            www.disposeDownloadHandlerOnDispose = true;
            www.SetRequestHeader("Range", "bytes=" + downloader.DownedLength + "-");
            www.downloadHandler = downloader;
            yield return www.SendWebRequest();
        }
    }
}