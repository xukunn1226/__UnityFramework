using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.IO;

namespace Framework.Core
{
    /// <summary>
    /// 1、下载backdoor.json
    /// 2、获取PlayerPrefs与backdoor中最新版本号对比，判断是否是最新版本或补丁已下载完成
    /// 3、初始化下载环境
    /// 4、下载补丁
    /// 5、标记PlayerPrefs完成
    /// </summary>
    public class Patcher : MonoBehaviour
    {
        static private string               CUR_APPVERSION              = "CurAppVersion_fe2679cf89a145ccb45b715568e6bc07";

        private int                         m_WorkerCount               = 5;
        private List<DownloadTask>          m_TaskWorkerList;
        private List<byte[]>                m_CachedBufferList;
        private const int                   m_BufferSize                = 1024 * 1024;

        private Backdoor                    m_Backdoor;                 // 服务器下载的backdoor
        private Diff                        m_Diff;                     // 补丁包差异配置数据
        private string                      m_CurVersion;               // 当前版本号（记录在PlayerPrefs）

        private string                      m_CdnURL;

        private void Awake()
        {
            m_CdnURL = string.Format($"{Application.dataPath}../Deployment/CDN");
        }

        private void OnEnable()
        {
            StartWork(m_CdnURL, 5);
        }

        public void StartWork(string cdnURL, int workerCount)
        {
            m_CdnURL = cdnURL;
            m_WorkerCount = workerCount;

            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            yield return StartCoroutine(DownloadBackdoor());

            if(m_Backdoor == null)
            {
                Debug.LogError($"Failed to download backdoor.json from {m_CdnURL}");
                yield break;
            }

            if (IsLatestVersion())
                yield break;

            yield return StartCoroutine(DownloadDiffConfig());

            if(m_Diff == null)
            {
                Debug.LogError("");
                yield break;
            }

            BeginDownload();
        }

        private IEnumerator DownloadBackdoor()
        {
            string localBackdoorURL = string.Format($"{Application.persistentDataPath}/backdoor.json");
            if (File.Exists(localBackdoorURL))
                File.Delete(localBackdoorURL);

            Uri remoteBackdoorURL = new Uri(string.Format($"{m_CdnURL}/backdoor.json"));
            UnityWebRequest www = UnityWebRequest.Get(remoteBackdoorURL);
            www.downloadHandler = new UnityEngine.Networking.DownloadHandlerFile(localBackdoorURL);
            yield return www.SendWebRequest();

            m_Backdoor = null;
            if(www.result == UnityWebRequest.Result.Success)
            {
                m_Backdoor = Backdoor.Deserialize(localBackdoorURL);
            }
        }

        /// <summary>
        /// 根据本地版本号与远端最新版本号判断是否是最新版本
        /// </summary>
        /// <returns></returns>
        private bool IsLatestVersion()
        {
            string localCurVersion = GetLocalCurVersion();

            AppVersion remoteCurVersion = AppVersion.CreateInstance<AppVersion>();
            remoteCurVersion.Set(m_Backdoor.CurVersion);

            return !string.IsNullOrEmpty(localCurVersion) && remoteCurVersion.CompareTo(localCurVersion) == 0;
        }

        /// <summary>
        /// 获取本地当前版本号
        /// </summary>
        /// <returns></returns>
        private string GetLocalCurVersion()
        {
            if (string.IsNullOrEmpty(m_CurVersion))
            {
                m_CurVersion = PlayerPrefs.GetString(CUR_APPVERSION);
                if (string.IsNullOrEmpty(m_CurVersion))
                { // 首次安装，尚未下载过会没有CUR_APPVERSION标记，则使用母包的版本号
                    AppVersion version = AppVersion.Load();
                    m_CurVersion = version.ToString();
                    AppVersion.Unload(version);
                }

                if (!string.IsNullOrEmpty(AppVersion.Check(m_CurVersion)))
                {
                    m_CurVersion = null;
                }
            }
            return m_CurVersion;
        }

        private IEnumerator DownloadDiffConfig()
        {
            string localDiffURL = string.Format($"{Application.persistentDataPath}/diff.json");
            if (File.Exists(localDiffURL))
                File.Delete(localDiffURL);
            
            string diffURL = Path.Combine(m_CdnURL, "patch", Utility.GetPlatformName(), m_Backdoor.CurVersion, GetLocalCurVersion(), "diff.json");
            Uri remoteDiffURL = new Uri(diffURL);
            UnityWebRequest www = UnityWebRequest.Get(remoteDiffURL);
            www.downloadHandler = new UnityEngine.Networking.DownloadHandlerFile(localDiffURL);
            yield return www.SendWebRequest();

            m_Diff = null;
            if (www.result == UnityWebRequest.Result.Success)
            {
                m_Diff = Diff.Deserialize(localDiffURL);
            }
        }

        private void BeginDownload()
        {
            // init task workers and buffer
            m_CachedBufferList = new List<byte[]>(m_WorkerCount);
            for (int i = 0; i < m_WorkerCount; ++i)
            {
                m_CachedBufferList.Add(new byte[m_BufferSize]);
            }
            m_TaskWorkerList = new List<DownloadTask>(m_WorkerCount);
            for (int i = 0; i < m_WorkerCount; ++i)
            {
                m_TaskWorkerList.Add(new DownloadTask(m_CachedBufferList[i]));
            }

            // 收集将下载的数据
        }
    }
}