using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using Sirenix.OdinInspector;

namespace Framework.AssetManagement.AssetEditorWindow
{
    public class SummaryViewer
    {
        private PropertyTree m_PropertyTree;

        [System.Serializable]
        public class SummaryItem
        {
            public string 概览;
            public string 参数;
        }
        [TableList]
        public List<SummaryItem> m_SummaryItems = new List<SummaryItem>();

        public SummaryViewer(ReportBuild reporter)
        {
            m_PropertyTree = PropertyTree.Create(this);

            m_SummaryItems.Add(new SummaryItem() { 概览 = "引擎版本",                     参数 = reporter.Summary.UnityVersion });
            m_SummaryItems.Add(new SummaryItem() { 概览 = "构建时间",                     参数 = reporter.Summary.BuildDate });
            m_SummaryItems.Add(new SummaryItem() { 概览 = "构建耗时",                     参数 = string.Format($"{reporter.Summary.BuildSeconds}s") });
            m_SummaryItems.Add(new SummaryItem() { 概览 = "构建平台",                     参数 = reporter.Summary.BuildTarget.ToString() });
            m_SummaryItems.Add(new SummaryItem() { 概览 = "构建版本",                     参数 = reporter.Summary.BuildPackageVersion });
            m_SummaryItems.Add(new SummaryItem() { 概览 = "命名规范",                     参数 = reporter.Summary.OutputNameStyle.ToString() });
            m_SummaryItems.Add(new SummaryItem() { 概览 = "压缩类型",                     参数 = reporter.Summary.CompressOption.ToString() });
            m_SummaryItems.Add(new SummaryItem() { 概览 = "DisableWriteTypeTree",        参数 = reporter.Summary.DisableWriteTypeTree.ToString() });
            m_SummaryItems.Add(new SummaryItem() { 概览 = "文件资源总数",                 参数 = reporter.Summary.AssetFileTotalCount.ToString() });
            m_SummaryItems.Add(new SummaryItem() { 概览 = "清单中文件资源总数",            参数 = reporter.Summary.MainAssetTotalCount.ToString() });
            m_SummaryItems.Add(new SummaryItem() { 概览 = "资源包总数",                   参数 = reporter.Summary.AllBundleTotalCount.ToString() });
            m_SummaryItems.Add(new SummaryItem() { 概览 = "资源包总大小（包含原生文件）",   参数 = string.Format($"{reporter.Summary.AllBundleTotalSize*1.0f/1024/1024:0.00} M") });
            m_SummaryItems.Add(new SummaryItem() { 概览 = "原生资源包总数",                参数 = reporter.Summary.RawBundleTotalCount.ToString() });
            m_SummaryItems.Add(new SummaryItem() { 概览 = "原生资源包总大小",              参数 = string.Format($"{reporter.Summary.RawBundleTotalSize*1.0f/1024/1024:0.00} M") });
            m_SummaryItems.Add(new SummaryItem() { 概览 = "平均资源包依赖数",              参数 = string.Format($"{reporter.Summary.AverageDependBundlesCount:0.00}") });
            m_SummaryItems.Add(new SummaryItem() { 概览 = "最大的依赖资源包数",            参数 = reporter.Summary.MaxDependBundlesCount.ToString() });
            m_SummaryItems.Add(new SummaryItem() { 概览 = "平均资源包大小",                参数 = string.Format($"{reporter.Summary.AverageBundleSize*1.0f/1024/1024:0.00} M") });
        }

        public void Draw()
        {
            m_PropertyTree?.Draw(false);
        }
    }
}