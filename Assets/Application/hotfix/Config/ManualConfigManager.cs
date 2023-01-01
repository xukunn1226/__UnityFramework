using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;
using Application.Runtime;
using System.Linq;

namespace Application.Logic
{
    public partial class ConfigManager : Singleton<ConfigManager>
    {
        private SqlData         m_Sql;
        static private string   m_DataPath;
        static public string    dataPath
        {
            get
            {
                if(string.IsNullOrEmpty(m_DataPath))
                {
                    switch(Launcher.GetLauncherMode())
                    {
                        case LoaderType.FromEditor:
                            m_DataPath = ConfigBuilderSetting.DatabaseFilePath;
                            break;
                        case LoaderType.FromStreamingAssets:
                        case LoaderType.FromPersistent:
                            m_DataPath = string.Format($"{UnityEngine.Application.persistentDataPath}/{Utility.GetPlatformName()}/{System.IO.Path.GetFileName(ConfigBuilderSetting.DatabaseFilePath)}");
                            break;
                    }
                }
                return m_DataPath;
            }
        }

        protected override void InternalInit()
        {
            m_Sql = new SqlData(dataPath);
            // #if UNITY_EDITOR
            // UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            // #endif
        }

        // #if UNITY_EDITOR
        // void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        // {
        //     if(state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
        //     {
        //         m_Sql?.Close();
        //         UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        //     }
        // }
        // #endif

        protected override void OnDestroy()
        {
            m_Sql?.Close();
        }

        // 提取配置数据库从streamingAssets至persistentDataPath
        static public IEnumerator ExtractDatabase()
        {
            string srcPath = string.Format($"{UnityEngine.Application.streamingAssetsPath}/{Utility.GetPlatformName()}/{System.IO.Path.GetFileName(ConfigBuilderSetting.DatabaseFilePath)}");
            string dstPath = string.Format($"{UnityEngine.Application.persistentDataPath}/{Utility.GetPlatformName()}/{System.IO.Path.GetFileName(ConfigBuilderSetting.DatabaseFilePath)}");

            DownloadTask task = new DownloadTask(new byte[1024]);
            DownloadTaskInfo info           = new DownloadTaskInfo();
                    info.srcUri             = new System.Uri(srcPath);
                    info.dstURL             = dstPath;
                    info.verifiedHash       = null;
                    info.retryCount         = 3;
                    // info.onProgress         = OnProgress;
                    info.onCompleted        = OnCompleted;
                    info.onRequestError     = OnRequestError;
                    info.onDownloadError    = OnDownloadError;
            return task.Run(info);
        }

        // private void OnProgress(DownloadTaskInfo taskInfo, ulong downedLength, ulong totalLength, float downloadSpeed)
        // {
        //     Debug.Log($"OnProgress: {System.IO.Path.GetFileName(taskInfo.dstURL)}     {downedLength}/{totalLength}    downloadSpeed({downloadSpeed})");
        // }

        static private void OnCompleted(DownloadTaskInfo taskInfo, bool success, int tryCount)
        {
            Debug.Log($"配置数据库下载：{taskInfo.dstURL} {(success ? "成功" : "失败")}");
        }

        static private void OnRequestError(DownloadTaskInfo taskInfo, string error)
        {
            Debug.LogError(string.Format($"配置数据库  OnRequestError: {error} : {taskInfo.srcUri}"));
        }

        static private void OnDownloadError(DownloadTaskInfo taskInfo, string error)
        {
            Debug.LogError(string.Format($"配置数据库  OnDownloadError: {error} : {taskInfo.srcUri}"));
        }
        
        private void Parse(ref List<string> ret, string content)
        {
            string[] pairs = content.Split(';').Where(s => !string.IsNullOrEmpty(s)).ToArray();
            for(int i = 0; i < pairs.Length; ++i)
            {                
                ret.Add(pairs[i]);
            }
        }

        private void Parse(ref List<int> ret, string content)
        {
            string[] pairs = content.Split(';').Where(s => !string.IsNullOrEmpty(s)).ToArray();
            for(int i = 0; i < pairs.Length; ++i)
            {                
                ret.Add(int.Parse(pairs[i]));
            }
        }

        private void Parse(ref List<float> ret, string content)
        {
            string[] pairs = content.Split(';').Where(s => !string.IsNullOrEmpty(s)).ToArray();
            for(int i = 0; i < pairs.Length; ++i)
            {                
                ret.Add(float.Parse(pairs[i]));
            }
        }

        private void Parse(ref Dictionary<int, int> ret, string content)
        {
            string[] pairs = content.Split(';').Where(s => !string.IsNullOrEmpty(s)).ToArray();
            for(int i = 0; i < pairs.Length; ++i)
            {
                string[] items = pairs[i].Split(':');
                Debug.Assert(items.Length == 2);
                ret.Add(int.Parse(items[0]), int.Parse(items[1]));
            }
        }

        private void Parse(ref Dictionary<int, string> ret, string content)
        {
            string[] pairs = content.Split(';').Where(s => !string.IsNullOrEmpty(s)).ToArray();
            for(int i = 0; i < pairs.Length; ++i)
            {
                string[] items = pairs[i].Split(':');
                Debug.Assert(items.Length == 2);
                ret.Add(int.Parse(items[0]), items[1]);
            }
        }

        private void Parse(ref Dictionary<int, float> ret, string content)
        {
            string[] pairs = content.Split(';').Where(s => !string.IsNullOrEmpty(s)).ToArray();
            for(int i = 0; i < pairs.Length; ++i)
            {
                string[] items = pairs[i].Split(':');
                Debug.Assert(items.Length == 2);
                ret.Add(int.Parse(items[0]), float.Parse(items[1]));
            }
        }

        private void Parse(ref Dictionary<string, int> ret, string content)
        {
            string[] pairs = content.Split(';').Where(s => !string.IsNullOrEmpty(s)).ToArray();
            for(int i = 0; i < pairs.Length; ++i)
            {
                string[] items = pairs[i].Split(':');
                Debug.Assert(items.Length == 2);
                ret.Add(items[0], int.Parse(items[1]));
            }
        }

        private void Parse(ref Dictionary<string, string> ret, string content)
        {
            string[] pairs = content.Split(';').Where(s => !string.IsNullOrEmpty(s)).ToArray();
            for(int i = 0; i < pairs.Length; ++i)
            {
                string[] items = pairs[i].Split(':');
                Debug.Assert(items.Length == 2);
                ret.Add(items[0], items[1]);
            }
        }

        private void Parse(ref Dictionary<string, float> ret, string content)
        {
            string[] pairs = content.Split(';').Where(s => !string.IsNullOrEmpty(s)).ToArray();
            for(int i = 0; i < pairs.Length; ++i)
            {
                string[] items = pairs[i].Split(':');
                Debug.Assert(items.Length == 2);
                ret.Add(items[0], float.Parse(items[1]));
            }
        }
    }
}



