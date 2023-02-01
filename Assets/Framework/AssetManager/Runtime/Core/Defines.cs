using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    public enum EPlayMode
    {
        /// <summary>
        /// 编辑器下模拟模式
        /// </summary>
        FromEditor,

        /// <summary>
        /// 单机模式
        /// </summary>
        FromStreaming,

        /// <summary>
        /// 联机模式
        /// </summary>
        FromHost,
    }

    /// <summary>
    /// Bundle文件的加载方式，见AssetBundle.LoadFromFile
    /// </summary>
    public enum EBundleLoadMethod
    {
        LoadFromFile        = 0,
        LoadFromFileOffset  = 1,        // 加解密用此方式
        //LoadFromMemory = 2,             // 暂不支持，此方式的最大作用是加解密，但安卓平台下需要解压至persistent data path，不实用，故废弃
    }

    /// <summary>
    /// Bundle从何处加载资源
    /// </summary>
    public enum ELoadMethod
    {
        LoadFromStreaming   = 0,        // 内置资源
        LoadFromCache       = 1,        // 补丁资源
        LoadFromRemote      = 2,        // 边玩边下
        LoadFromEditor      = 3,        // 编辑器模式下
    }

    /// <summary>
    /// 资源包的加载状态
    /// </summary>
    public enum EBundleLoadStatus
    {
        None                = 0,        // 未定状态
        Succeed             = 1,        // 加载成功
        Failed              = 2,        // 加载失败
    }

    /// <summary>
    /// 资源提供者的状态
    /// </summary>
    public enum EProviderStatus
    {
        None                = 0,        // 未开始
        CheckBundle         = 1,        // 检测bundle的完成状态
        Loading             = 2,        // 加载资源
        Checking            = 3,        // 检测加载结果
        Succeed             = 4,
        Failed              = 5,
    }

    public enum EOperationStatus
    {
        None,
        Succeed,
        Failed,
    }

    public enum ELoadingPriority
    {
        Low,
        Normal,
        High,
    }
}