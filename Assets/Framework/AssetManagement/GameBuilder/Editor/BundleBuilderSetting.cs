﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Framework.GameBuilder
{
    /// <summary>
    /// Bundle打包配置数据
    /// </summary>
    public class BundleBuilderSetting : ScriptableObject
    {
        /// <summary>
        /// bundles输出目录
        /// </summary>
        public string           outputPath      = "Deployment/AssetBundles";
        
        /// <summary>
        /// 是否使用LZ4压缩模式
        /// </summary>
        public bool             useLZ4Compress;

        /// <summary>
        /// 是否rebuild
        /// </summary>
        public bool             rebuildBundles;

        /// <summary>
        /// 是否在bundle name附加hash
        /// </summary>
        public bool             appendHash;

        /// <summary>
        /// 遇到error退出构建
        /// </summary>
        public bool             strictMode;

        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(string.Format($"outputPath: {outputPath}  \n"));
            sb.Append(string.Format($"useLZ4Compress: {useLZ4Compress}  \n"));
            sb.Append(string.Format($"rebuildBundles: {rebuildBundles}  \n"));
            sb.Append(string.Format($"appendHash: {appendHash}  \n"));
            sb.Append(string.Format($"strictMode: {strictMode}  \n"));
            return sb.ToString();
        }
    }

    static public class BundleBuilderSettingExtension
    {
        static public BuildAssetBundleOptions GenerateOptions(this BundleBuilderSetting para)
        {
            BuildAssetBundleOptions opt = BuildAssetBundleOptions.None;
            
            if (para.useLZ4Compress)
                opt |= BuildAssetBundleOptions.ChunkBasedCompression;
            else
                opt &= ~BuildAssetBundleOptions.ChunkBasedCompression;

            if (para.rebuildBundles)
                opt |= BuildAssetBundleOptions.ForceRebuildAssetBundle;
            else
                opt &= ~BuildAssetBundleOptions.ForceRebuildAssetBundle;

            if (para.appendHash)
                opt |= BuildAssetBundleOptions.AppendHashToAssetBundleName;
            else
                opt &= ~BuildAssetBundleOptions.AppendHashToAssetBundleName;

            if (para.strictMode)
                opt |= BuildAssetBundleOptions.StrictMode;
            else
                opt &= ~BuildAssetBundleOptions.StrictMode;

            return opt;
        }
    }
}