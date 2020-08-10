using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Framework.Core.Editor
{
    [CustomEditor(typeof(SoftObjectPath), true)]
    public class SoftObjectPathEditor : UnityEditor.Editor
    {
        private SoftObjectPath m_Target;
        private SerializedProperty m_GUIDProp;
        private SerializedProperty m_AssetPathProp;
        private SerializedProperty m_BundleNameProp;
        private SerializedProperty m_AssetNameProp;

        private long m_FileID;

        public virtual void OnEnable()
        {
            m_Target = (SoftObjectPath)serializedObject.targetObject;
            m_GUIDProp = serializedObject.FindProperty("m_GUID");
            m_AssetPathProp = serializedObject.FindProperty("m_AssetPath");
            m_BundleNameProp = serializedObject.FindProperty("m_BundleName");
            m_AssetNameProp = serializedObject.FindProperty("m_AssetName");
            m_FileID = RedirectorDB.GetLocalID(m_Target);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            string assetPath = AssetDatabase.GUIDToAssetPath(m_GUIDProp.stringValue);
            Object obj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            m_Target.assetPath = assetPath.ToLower();

            EditorGUI.BeginChangeCheck();
            Object oldObj = obj;
            obj = EditorGUILayout.ObjectField("Ref Object", obj, typeof(Object), false);        // only allow assigning project assets
            if (EditorGUI.EndChangeCheck())
            {
                // 更新SoftObject
                if (obj == null)
                {
                    m_GUIDProp.stringValue = null;
                    m_Target.assetPath = null;
                }
                else
                {
                    m_Target.assetPath = AssetDatabase.GetAssetPath(obj).ToLower();
                    m_GUIDProp.stringValue = AssetDatabase.AssetPathToGUID(m_Target.assetPath);
                }
            }

            m_AssetPathProp.stringValue = m_Target.assetPath;
            m_BundleNameProp.stringValue = m_Target.bundleName;
            m_AssetNameProp.stringValue = m_Target.assetName;

            GUI.enabled = false;
            EditorGUILayout.TextField("Asset Path", m_Target.assetPath);                    // 资源地址
            EditorGUILayout.TextField("Bundle Name", m_Target.bundleName);
            EditorGUILayout.TextField("Asset Name", m_Target.assetName);
            EditorGUILayout.TextField("GUID",       m_GUIDProp.stringValue);                // 资源GUID，与Asset Path一致
            EditorGUILayout.TextField("FileID",     m_FileID.ToString());                   // SoftObjectPath在挂载对象上的FileID
            GUI.enabled = true;

            serializedObject.ApplyModifiedProperties();
        }
    }
}