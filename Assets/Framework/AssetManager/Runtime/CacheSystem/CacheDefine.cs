using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    /// <summary>
	/// 下载文件校验等级
	/// </summary>
	public enum EVerifyLevel
    {
        /// <summary>
        /// 验证文件大小
        /// </summary>
        Low,

        /// <summary>
        /// 验证文件大小和CRC
        /// </summary>
        High,
    }

    /// <summary>
	/// 下载文件校验结果
	/// </summary>
	internal enum EVerifyResult
    {
        /// <summary>
        /// 文件不存在
        /// </summary>
        FileNotExisted = -4,

        /// <summary>
        /// 文件内容不足（小于正常大小）
        /// </summary>
        FileNotComplete = -3,

        /// <summary>
        /// 文件内容溢出（超过正常大小）
        /// </summary>
        FileOverflow = -2,

        /// <summary>
        /// 文件内容不匹配
        /// </summary>
        FileCrcError = -1,

        /// <summary>
        /// 验证异常
        /// </summary>
        Exception = 0,

        /// <summary>
        /// 验证成功
        /// </summary>
        Succeed = 1,
    }

    internal class VerifyInfo
    {
        /// <summary>
        /// 验证的资源文件是否为内置资源
        /// </summary>
        public bool IsBuildinFile { private set; get; }

        /// <summary>
        /// 验证的资源包实例
        /// </summary>
        public BundleDescriptor VerifyBundle { private set; get; }

        /// <summary>
        /// 验证的文件路径
        /// </summary>
        public string VerifyFilePath { private set; get; }

        /// <summary>
        /// 验证结果
        /// </summary>
        public EVerifyResult Result;

        public VerifyInfo(bool isBuildinFile, BundleDescriptor verifyBundle)
        {
            IsBuildinFile = isBuildinFile;
            VerifyBundle = verifyBundle;
            VerifyFilePath = verifyBundle.cachedFilePath;
        }
    }
}