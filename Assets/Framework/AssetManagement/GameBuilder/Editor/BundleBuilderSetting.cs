using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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
        /// 是否使用LZ4压缩模式
        /// </summary>
        public bool             useLZ4Compress;

        public bool             rebuildBundles;

        /// <summary>
        /// 是否在bundle name附加hash
        /// </summary>
        public bool             appendHash;
        public bool             DisableWriteTypeTree;

        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(string.Format($"useLZ4Compress: {useLZ4Compress}  \n"));
            sb.Append(string.Format($"rebuildBundles: {rebuildBundles}  \n"));
            sb.Append(string.Format($"appendHash: {appendHash}  \n"));
            sb.Append(string.Format($"DisableWriteTypeTree: {DisableWriteTypeTree}  \n"));
            sb.Append(string.Format($"bundleCollectorConfigName: {bundleCollectorConfigName}  \n"));
            return sb.ToString();
        }
    }
}