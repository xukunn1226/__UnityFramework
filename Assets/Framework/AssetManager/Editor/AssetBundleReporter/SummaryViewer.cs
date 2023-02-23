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
        private ReportBuild m_Reporter;

        public SummaryViewer(ReportBuild reporter)
        {
            m_Reporter = reporter;
            m_PropertyTree = PropertyTree.Create(this);
        }

        public void Draw()
        {
            m_PropertyTree?.Draw(false);
        }

        [OnInspectorGUI]
        void OnShow()
        {
            EditorGUILayout.LabelField("333333333333");
            EditorGUILayout.LabelField("22222222222222222");
        }
    }
}