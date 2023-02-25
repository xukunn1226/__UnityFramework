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
    public class BundleViewer
    {
        private PropertyTree m_PropertyTree;
        private ReportBuild m_Reporter;

        [Searchable]
        public List<BundleItem> m_Items = new List<BundleItem>();

        public BundleViewer(ReportBuild reporter)
        {
            m_Reporter = reporter;
            m_PropertyTree = PropertyTree.Create(this);

            foreach (var item in m_Reporter.BundleInfos)
            {
                var assetItem = new BundleItem(item);
                m_Items.Add(assetItem);
            }
        }

        public void Draw()
        {
            m_PropertyTree?.Draw(false);
        }

        [System.Serializable]
        public class BundleItem
        {
            private ReportBundleInfo m_BundleInfo;

            public BundleItem(ReportBundleInfo reportBundleInfo)
            {
                m_BundleInfo = reportBundleInfo;
            }

            [OnInspectorGUI]
            [LabelText("资源包名")]
            public string BundleName { get { return m_BundleInfo.BundleName; } }

            [OnInspectorGUI]
            [LabelText("文件名")]
            public string FileName { get { return m_BundleInfo.FileName; } }

            [OnInspectorGUI]
            [LabelText("文件哈希")]
            public string FileHash { get { return m_BundleInfo.FileHash; } }

            [OnInspectorGUI]
            [LabelText("文件校验码")]
            public string FileCRC { get { return m_BundleInfo.FileCRC; } }

            [OnInspectorGUI]
            [LabelText("文件大小")]
            public string FileSize { get { return string.Format($"{m_BundleInfo.FileSize * 1.0f / 1024 / 1024:0.00} M"); } }

            [OnInspectorGUI]
            [LabelText("原生文件")]
            public string IsRawFile { get { return m_BundleInfo.IsRawFile.ToString(); } }
        }
    }
}