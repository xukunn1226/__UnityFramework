using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MeshParticleSystem.Profiler
{
    [CustomEditor(typeof(BatchAssetCollection))]
    public class BatchAssetCollectionEditor : UnityEditor.Editor
    {
        private BatchAssetCollection m_Target;
        void OnEnable()
        {
            m_Target = target as BatchAssetCollection;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if(GUILayout.Button("Add"))
            {
                // m_Target.AddDirectory("Assets/Application/MeshParticleSystem/Tests/Runtime/Res");

                EditorUtility.SetDirty(m_Target);
                AssetDatabase.SaveAssets();
            }
        }
    }
}