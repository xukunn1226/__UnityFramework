using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Framework.AssetManagement.Runtime
{
    static internal class CacheSystem
    {
        private readonly static Dictionary<string, BundleDescriptor> m_CachedDic = new Dictionary<string, BundleDescriptor>(1000);

        /// <summary>
        /// 初始化时的验证级别
        /// </summary>
        public static EVerifyLevel InitVerifyLevel { set; get; } = EVerifyLevel.Low;

        /// <summary>
		/// 查询是否为验证文件
		/// 注意：被收录的文件完整性是绝对有效的
		/// </summary>
		public static bool IsCached(BundleDescriptor patchBundle)
        {
            string cacheKey = patchBundle.CacheKey;
            if (m_CachedDic.ContainsKey(cacheKey))
            {
                string filePath = patchBundle.cachedFilePath;
                if (File.Exists(filePath))
                {
                    return true;
                }
                else
                {
                    m_CachedDic.Remove(cacheKey);
                    Debug.LogError($"Cache file is missing : {filePath}");
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 缓存补丁包文件
        /// </summary>
        public static void CacheBundle(BundleDescriptor bundleDesc)
        {
            string cacheKey = bundleDesc.CacheKey;
            if (m_CachedDic.ContainsKey(cacheKey) == false)
            {
                string filePath = bundleDesc.cachedFilePath;
                Debug.Log($"Cache verify file : {filePath}");
                m_CachedDic.Add(cacheKey, bundleDesc);
            }
        }

        /// <summary>
		/// 清空所有数据
		/// </summary>
		public static void ClearAll()
        {
            m_CachedDic.Clear();
        }

        /// <summary>
		/// 验证补丁包文件
		/// </summary>
		public static EVerifyResult VerifyBundle(BundleDescriptor bundleDesc, EVerifyLevel verifyLevel)
        {
            return VerifyContentInternal(bundleDesc.cachedFilePath, bundleDesc.fileSize, bundleDesc.fileCRC, verifyLevel);
        }

        /// <summary>
        /// 验证并缓存补丁包文件
        /// </summary>
        public static EVerifyResult VerifyAndCacheBundle(BundleDescriptor bundleDesc, EVerifyLevel verifyLevel)
        {
            var verifyResult = VerifyContentInternal(bundleDesc.cachedFilePath, bundleDesc.fileSize, bundleDesc.fileCRC, verifyLevel);
            if (verifyResult == EVerifyResult.Succeed)
                CacheBundle(bundleDesc);
            return verifyResult;
        }

        /// <summary>
        /// 验证文件完整性
        /// </summary>
        private static EVerifyResult VerifyContentInternal(string filePath, long fileSize, string fileCRC, EVerifyLevel verifyLevel)
        {
            try
            {
                if (File.Exists(filePath) == false)
                    return EVerifyResult.FileNotExisted;

                // 先验证文件大小
                long size = FileUtility.GetFileSize(filePath);
                if (size < fileSize)
                    return EVerifyResult.FileNotComplete;
                else if (size > fileSize)
                    return EVerifyResult.FileOverflow;

                // 再验证文件CRC
                if (verifyLevel == EVerifyLevel.High)
                {
                    string crc = HashUtility.FileCRC32(filePath);
                    if (crc == fileCRC)
                        return EVerifyResult.Succeed;
                    else
                        return EVerifyResult.FileCrcError;
                }
                else
                {
                    return EVerifyResult.Succeed;
                }
            }
            catch (System.Exception)
            {
                return EVerifyResult.Exception;
            }
        }
    }
}