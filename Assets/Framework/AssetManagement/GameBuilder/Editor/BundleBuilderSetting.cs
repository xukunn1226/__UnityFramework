using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Framework.AssetManagement.Runtime;

namespace Framework.AssetManagement.AssetEditorWindow
{
    /// <summary>
    /// Bundle打包配置数据
    /// </summary>
    public class BundleBuilderSetting : ScriptableObject
    {
        /// <summary>
        /// 资源包收集配置名
        /// </summary>
        public string           bundleCollectorConfigName;

        /// <summary>
        /// 资源包的名称样式
        /// </summary>
        public EOutputNameStyle nameStyle = EOutputNameStyle.BundleName;
        
        /// <summary>
        /// 是否使用LZ4压缩模式
        /// </summary>
        public bool             useLZ4Compress;

        public bool             rebuildBundles;

        /// <summary>
        /// 是否在bundle name附加hash
        /// </summary>
        public bool             appendHash;
        public bool             disableWriteTypeTree;

        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(string.Format($"useLZ4Compress: {useLZ4Compress}  \n"));
            sb.Append(string.Format($"rebuildBundles: {rebuildBundles}  \n"));
            sb.Append(string.Format($"appendHash: {appendHash}  \n"));
            sb.Append(string.Format($"DisableWriteTypeTree: {disableWriteTypeTree}  \n"));
            sb.Append(string.Format($"bundleCollectorConfigName: {bundleCollectorConfigName}  \n"));
            sb.Append(string.Format($"nameStyle: {nameStyle}  \n"));
            return sb.ToString();
        }
    }
}