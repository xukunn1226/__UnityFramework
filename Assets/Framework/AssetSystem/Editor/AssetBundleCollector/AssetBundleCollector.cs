using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Framework.AssetManagement.AssetBundleCollector
{
    [Serializable]
    public class AssetBundleCollector
    {
        /// <summary>
        /// 收集路径GUID，文件夹或单个资源文件
        /// </summary>
        public string           CollectGUID;

        public ECollectorType   CollectorType = ECollectorType.MainAssetCollector;

        public string           PackRuleName;

        public string           FilterRuleName;

        public bool IsValid()
        {
            if(string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(CollectGUID)))
            {
                return false;
            }
            return true;
        }
    }

    public enum ECollectorType
    {
        /// <summary>
        /// 收集参与打包的主资源，写入资源清单列表，可通过代码加载
        /// </summary>
        MainAssetCollector,

        /// <summary>
        /// 收集参与打包的主资源，不写入资源清单列表，不可通过代码加载
        /// </summary>
        StaticAssetCollector,

        /// <summary>
        /// 收集参与打包的依赖资源，不写入资源清单列表，不可通过代码加载，如果没有被主资源对象引用，则不参与构建
        /// </summary>
        DependAssetCollector,
    }
}