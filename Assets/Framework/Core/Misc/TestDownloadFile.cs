using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using Framework.Core;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework.Core.Tests
{
    public class TestDownloadFile : MonoBehaviour
    {
#if UNITY_EDITOR
        [MenuItem("Tools/Build Game")]
        static private void Build()
        {
            BuildPlayerOptions opts = new BuildPlayerOptions();
            if (Directory.Exists("111"))
                Directory.CreateDirectory("111");

            opts.locationPathName = "111/Test.exe";
            opts.targetGroup = BuildTargetGroup.Standalone;
            opts.target = BuildTarget.StandaloneWindows;
            opts.options = BuildOptions.Development;
            BuildPipeline.BuildPlayer(opts);
        }
#endif

        private void OnGUI()
        {
            if (GUI.Button(new Rect(300, 200, 200, 120), "Test1---------"))
            {
                StartCoroutine(DownloadFromLocal());
            }

            if (GUI.Button(new Rect(600, 200, 200, 120), "Test2"))
            {
                StartCoroutine(DownloadFromWeb());
            }

            if (GUI.Button(new Rect(300, 500, 200, 120), "Test3"))
            {
                StartCoroutine(DownloadFromLocalEx());
            }

            if (GUI.Button(new Rect(600, 500, 200, 120), "Test4"))
            {
                StartCoroutine(DownloadFromWebEx());
            }
        }

        IEnumerator DownloadFromLocal()
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, "windows/manifest");
            System.Uri url = new System.Uri(filePath);
            if (url.ToString().Contains("://") || url.ToString().Contains(":///"))
            {
                Debug.LogError($"url: {url.ToString()}");

                UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(url);
                yield return www.SendWebRequest();
                Debug.LogError($"---  {www.downloadedBytes}     {www.result}");


                byte[] data = www.downloadHandler.data;
                if (!Directory.Exists("Rep"))
                    Directory.CreateDirectory("Rep");
                FileStream fs = new FileStream("Rep/MyFile1.pdf", FileMode.OpenOrCreate);
                fs.Write(data, 0, data.Length);
                fs.Flush();
                fs.Close();
                fs.Dispose();
            }
            Debug.LogError("222222222222");
        }

        IEnumerator DownloadFromWeb()
        {
            string filePath = "https://raw.githubusercontent.com/xukunn1226/Repo/master/fasthalffloatconversion.pdf";
            System.Uri url = new System.Uri(filePath);
            if (url.ToString().Contains("://") || url.ToString().Contains(":///"))
            {
                Debug.LogError($"url: {url.ToString()}");

                UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(url);
                yield return www.SendWebRequest();
                Debug.LogError($"---  {www.downloadedBytes}     {www.result}");

                byte[] data = www.downloadHandler.data;
                if (!Directory.Exists("Rep"))
                    Directory.CreateDirectory("Rep");
                FileStream fs = new FileStream("Rep/MyFile2.pdf", FileMode.Create);
                fs.Write(data, 0, data.Length);
                fs.Flush();
                fs.Close();
                fs.Dispose();
            }
            Debug.LogError("222222222222");
        }




        IEnumerator DownloadFromLocalEx()
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, "windows/manifest");
            System.Uri url = new System.Uri(filePath);
            if (url.ToString().Contains("://") || url.ToString().Contains(":///"))
            {
                Debug.LogError($"url: {url.ToString()}");

                if (!Directory.Exists("Rep"))
                    Directory.CreateDirectory("Rep");

                UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(url);
                www.downloadHandler = new DownloadHandlerFile("Rep/MyFile3.pdf", www, new byte[1024 * 1024]);
                yield return www.SendWebRequest();
                Debug.LogError($"---  {www.downloadedBytes}     {www.result}");
            }
            Debug.LogError("222222222222");
        }

        IEnumerator DownloadFromWebEx()
        {
            string filePath = "https://raw.githubusercontent.com/xukunn1226/Repo/master/fasthalffloatconversion.pdf";
            System.Uri url = new System.Uri(filePath);
            if (url.ToString().Contains("://") || url.ToString().Contains(":///"))
            {
                Debug.LogError($"url: {url.ToString()}");

                if (!Directory.Exists("Rep"))
                    Directory.CreateDirectory("Rep");

                UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(url);
                www.downloadHandler = new DownloadHandlerFile("Rep/MyFile4.pdf", www, new byte[1024 * 1024]);
                yield return www.SendWebRequest();
                Debug.LogError($"---  {www.downloadedBytes}     {www.result}");
            }
            Debug.LogError("222222222222");
        }
    }
}