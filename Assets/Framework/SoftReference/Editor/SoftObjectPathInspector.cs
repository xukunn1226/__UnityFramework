using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Framework.Core.Editor
{
    [CustomEditor(typeof(SoftObjectPath))]
    public class SoftObjectPathInspector : UnityEditor.Editor
    {
        private SerializedProperty m_GUIDProp;
        private SerializedProperty m_AssetPathProp;

        private long m_FileID;

        private void OnEnable()
        {
            m_GUIDProp      = serializedObject.FindProperty("m_GUID");
            m_AssetPathProp = serializedObject.FindProperty("m_AssetPath");
            
            string guid;
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(serializedObject.targetObject, out guid, out m_FileID);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            string assetPath = AssetDatabase.GUIDToAssetPath(m_GUIDProp.stringValue);
            Object obj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);

            EditorGUI.BeginChangeCheck();
            obj = EditorGUILayout.ObjectField("Ref Object", obj, typeof(Object), false);
            if (EditorGUI.EndChangeCheck())
            {
                if (obj == null)
                {
                    m_AssetPathProp.stringValue = null;
                    m_GUIDProp.stringValue = null;
                }
                else
                {
                    m_AssetPathProp.stringValue = AssetDatabase.GetAssetPath(obj).ToLower();
                    m_GUIDProp.stringValue = AssetDatabase.AssetPathToGUID(m_AssetPathProp.stringValue);
                }
            }

            GUI.enabled = false;
            EditorGUILayout.TextField("GUID", m_GUIDProp.stringValue);
            EditorGUILayout.TextField("Asset Path", m_AssetPathProp.stringValue);
            EditorGUILayout.TextField("FileID", m_FileID.ToString());
            GUI.enabled = true;

            serializedObject.ApplyModifiedProperties();
        }
    }
}