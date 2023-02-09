using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    internal static class DownloadSystem
    {
        private static readonly Dictionary<string, DownloaderBase> m_DownloaderDic = new Dictionary<string, DownloaderBase>();
        private static readonly List<string> m_RemoveList = new List<string>(100);

        /// <summary>
        /// 启用断点续传功能文件的最小字节数
        /// </summary>
        public static int BreakpointResumeFileSize { set; get; } = int.MaxValue;

        /// <summary>
        /// 更新所有下载器
        /// </summary>
        public static void Update()
        {
            // 更新下载器
            m_RemoveList.Clear();
            foreach (var valuePair in m_DownloaderDic)
            {
                var downloader = valuePair.Value;
                downloader.Update();
                if (downloader.IsDone())
                    m_RemoveList.Add(valuePair.Key);
            }

            // 移除下载器
            foreach (var key in m_RemoveList)
            {
                m_DownloaderDic.Remove(key);
            }
        }

        /// <summary>
        /// 销毁所有下载器
        /// </summary>
        public static void DestroyAll()
        {
            foreach (var valuePair in m_DownloaderDic)
            {
                var downloader = valuePair.Value;
                downloader.Abort();
            }
            m_DownloaderDic.Clear();
            m_RemoveList.Clear();
            BreakpointResumeFileSize = int.MaxValue;
        }

        /// <summary>
        /// 开始下载资源文件
        /// 注意：只有第一次请求的参数才是有效的
        /// </summary>
        public static DownloaderBase BeginDownload(BundleInfo bundleInfo, int failedTryAgain, int timeout = 60)
        {
            // 查询存在的下载器
            if (m_DownloaderDic.TryGetValue(bundleInfo.descriptor.cachedFilePath, out var downloader))
            {
                return downloader;
            }

            // 创建新的下载器	
            {
                Debug.Log($"Beginning to download file : {bundleInfo.descriptor.fileName} URL : {bundleInfo.RemoteMainURL}");
                FileUtility.CreateFileDirectory(bundleInfo.descriptor.cachedFilePath);
                bool breakDownload = bundleInfo.descriptor.fileSize >= BreakpointResumeFileSize;
                DownloaderBase newDownloader = new FileDownloader(bundleInfo, breakDownload);
                newDownloader.SendRequest(failedTryAgain, timeout);
                m_DownloaderDic.Add(bundleInfo.descriptor.cachedFilePath, newDownloader);
                return newDownloader;
            }
        }

        /// <summary>
        /// 获取下载器的总数
        /// </summary>
        public static int GetDownloaderTotalCount()
        {
            return m_DownloaderDic.Count;
        }
    }
}