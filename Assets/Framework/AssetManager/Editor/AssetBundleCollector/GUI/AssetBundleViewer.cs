using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using Sirenix.OdinInspector;
using System;
using System.Reflection;

namespace Framework.AssetManagement.AssetEditorWindow
{
    public class AssetBundleViewer : OdinEditorWindow
    {
        [HideInInspector]
        public List<BuildBundleInfo> BuildBundleInfos;

        [Searchable]
        [LabelText("资源包列表")]
        public List<BundleViewer> BundleListViewer = new List<BundleViewer>();

        [Searchable]
        [LabelText("资源列表")]
        public List<AssetViewer> AssetListViewer = new List<AssetViewer>();

        public static void Open(BuildMapContext context)
        {
            var window = GetWindow<AssetBundleViewer>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(1000, 500);
            window.titleContent = new GUIContent("资源包预览");
            window.SetContext(context);
        }

        private void SetContext(BuildMapContext context)
        {
            BuildBundleInfos = context.BuildBundleInfos;

            // init BundleListViewer
            BundleListViewer.Clear();
            foreach (var bundleInfo in BuildBundleInfos)
            {
                BundleListViewer.Add(new BundleViewer(bundleInfo));
            }

            // init AssetListViewer
            Dictionary<string, AssetViewer> list = new Dictionary<string, AssetViewer>();
            foreach (var bundleInfo in BuildBundleInfos)
            {
                foreach(var assetInfo in bundleInfo.BuildinAssets)
                {
                    if (list.TryGetValue(assetInfo.AssetPath, out AssetViewer assetViewer) == false)
                    {
                        assetViewer = new AssetViewer(assetInfo);
                        list.Add(assetInfo.AssetPath, assetViewer);
                    }
                }
            }

            AssetListViewer.Clear();
            foreach(var pairs in list)
            {
                AssetListViewer.Add(pairs.Value);
            }
        }

        protected override void OnBeginDrawEditors()
        {
            base.OnBeginDrawEditors();
            SirenixEditorGUI.BeginHorizontalToolbar();
            {
                GUILayout.FlexibleSpace();
                if (SirenixEditorGUI.ToolbarButton("检查"))
                {

                }
            }
            SirenixEditorGUI.EndHorizontalToolbar();
        }

        [HideReferenceObjectPicker]
        public class BundleViewer
        {
            private BuildBundleInfo m_Context;
            private bool m_Foldout;

            public BundleViewer(BuildBundleInfo context)
            {
                m_Context = context;
            }

            [OnInspectorGUI]
            [LabelText("资源包名")]
            public string BundleName { get { return m_Context?.BundleName; } }

            [OnInspectorGUI]
            private void ShowAssetsList()
            {
                if (m_Context == null || m_Context.BuildinAssets.Count == 0)
                    return;

                m_Foldout = EditorGUILayout.Foldout(m_Foldout, $"资源列表：{m_Context.BuildinAssets.Count}");
                EditorGUI.indentLevel++;
                if (m_Foldout)
                {
                    foreach (var assetInfo in m_Context.BuildinAssets)
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField($"{assetInfo.AssetPath}");
                            //EditorGUILayout.LabelField($"{assetInfo.MainBundleName}");
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUI.indentLevel--;
            }
        }

        [HideReferenceObjectPicker]
        public class AssetViewer
        {
            private BuildAssetInfo m_Context;
            private bool m_Foldout1;
            private bool m_Foldout2;

            public AssetViewer(BuildAssetInfo context)
            {
                m_Context = context;
            }

            [OnInspectorGUI]
            [LabelText("资源名")]
            public string AssetPath { get { return m_Context?.AssetPath; } }

            [OnInspectorGUI]
            [LabelText("所属资源包")]
            public string MainBundleName { get { return m_Context?.MainBundleName; } }

            [OnInspectorGUI]
            [LabelText("收集类型")]
            public string CollectorType { get { return m_Context?.CollectorType.ToString(); } }

            [OnInspectorGUI]
            private void ShowAssetsList()
            {
                if (m_Context == null)
                    return;

                m_Foldout1 = EditorGUILayout.Foldout(m_Foldout1, $"依赖的资源：{m_Context.AllDependAssetInfos.Count}");
                EditorGUI.indentLevel++;
                if (m_Foldout1)
                {
                    foreach (var assetInfo in m_Context.AllDependAssetInfos)
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField($"{assetInfo.AssetPath}");
                            EditorGUILayout.LabelField($"{assetInfo.MainBundleName}");
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUI.indentLevel--;

                m_Foldout2 = EditorGUILayout.Foldout(m_Foldout2, $"依赖的资源包：{m_Context.AllDependBundleNames.Count}");
                EditorGUI.indentLevel++;
                if(m_Foldout2)
                {
                    foreach(var bundleName in m_Context.AllDependBundleNames)
                    {
                        EditorGUILayout.LabelField($"{bundleName}");
                    }
                }
                EditorGUI.indentLevel--;
            }
        }
    }
}