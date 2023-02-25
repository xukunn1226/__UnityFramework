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
    public class AssetViewer
    {
        private PropertyTree m_PropertyTree;
        private ReportBuild m_Reporter;

        [Searchable]
        public List<AssetItem> m_Items = new List<AssetItem>();

        public AssetViewer(ReportBuild reporter)
        {
            m_Reporter = reporter;
            m_PropertyTree = PropertyTree.Create(this);

            foreach(var item in m_Reporter.AssetInfos)
            {
                var assetItem = new AssetItem(item);
                m_Items.Add(assetItem);
            }
        }

        public void Draw()
        {
            m_PropertyTree?.Draw(false);
        }

        [System.Serializable]
        public class AssetItem
        {
            private ReportAssetInfo m_AssetInfo;
            private bool m_Foldout1;
            private bool m_Foldout2;

            public AssetItem(ReportAssetInfo reportAssetInfo)
            {
                m_AssetInfo = reportAssetInfo;
            }

            [OnInspectorGUI]
            [LabelText("资源名")]
            public string AssetPath { get { return m_AssetInfo.AssetPath; } }

            [OnInspectorGUI]
            [LabelText("GUID")]
            public string GUID { get { return m_AssetInfo.AssetGUID; } }

            [OnInspectorGUI]
            [LabelText("所属资源包名")]
            public string MainBundleName { get { return m_AssetInfo.MainBundleName; } }

            [OnInspectorGUI]
            [LabelText("所属资源包大小")]
            public string MainBundleSize { get { return string.Format($"{m_AssetInfo.MainBundleSize*1.0f/1024/1024:0.00} M"); } }

            [OnInspectorGUI]
            private void ShowAssetsList()
            {
                if (m_AssetInfo == null)
                    return;

                m_Foldout1 = EditorGUILayout.Foldout(m_Foldout1, $"依赖的资源列表：{m_AssetInfo.DependAssets.Count}");
                EditorGUI.indentLevel++;
                if (m_Foldout1)
                {
                    foreach (var assetInfo in m_AssetInfo.DependAssets)
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField($"{assetInfo}");
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUI.indentLevel--;

                m_Foldout2 = EditorGUILayout.Foldout(m_Foldout2, $"依赖的资源包列表：{m_AssetInfo.DependBundles.Count}");
                EditorGUI.indentLevel++;
                if (m_Foldout2)
                {
                    foreach (var assetInfo in m_AssetInfo.DependBundles)
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField($"{assetInfo}");
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUI.indentLevel--;
            }
        }
    }
}