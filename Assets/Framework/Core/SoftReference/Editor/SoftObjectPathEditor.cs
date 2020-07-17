using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Framework.Core.Editor
{
    [CustomEditor(typeof(SoftObjectPath), true)]
    public class SoftObjectPathEditor : UnityEditor.Editor
    {
        private SerializedProperty m_GUIDProp;
        private SerializedProperty m_AssetPathProp;

        private long m_FileID;

        public virtual void OnEnable()
        {
            m_GUIDProp = serializedObject.FindProperty("m_GUID");
            m_AssetPathProp = serializedObject.FindProperty("m_AssetPath");
            m_FileID = RedirectorDB.GetLocalID(serializedObject.targetObject);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            string assetPath = AssetDatabase.GUIDToAssetPath(m_GUIDProp.stringValue);
            Object obj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);

            EditorGUI.BeginChangeCheck();
            Object oldObj = obj;
            obj = EditorGUILayout.ObjectField("Ref Object", obj, typeof(Object), false);        // only allow assigning project assets
            if (EditorGUI.EndChangeCheck())
            {
                // 更新SoftObject
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
            EditorGUILayout.TextField("Asset Path", m_AssetPathProp.stringValue);           // 资源地址
            EditorGUILayout.TextField("GUID",       m_GUIDProp.stringValue);                // 资源GUID，与Asset Path一致
            EditorGUILayout.TextField("FileID",     m_FileID.ToString());                   // SoftObjectPath在挂载对象上的FileID
            GUI.enabled = true;

            serializedObject.ApplyModifiedProperties();
        }
    }
}