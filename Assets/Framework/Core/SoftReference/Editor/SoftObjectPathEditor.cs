using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

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
            Object oldObj = obj;

            EditorGUI.BeginChangeCheck();
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

                // 更新被替换资源的DB，不需要更新新资源的DB，因为reimport asset会触发更新
                if (oldObj != null)
                {
                    string oldAssetPath = AssetDatabase.GetAssetPath(oldObj);
                    string oldGUID = AssetDatabase.AssetPathToGUID(oldAssetPath);

                    string userAssetPath = AssetDatabase.GetAssetPath(target);
                    if (string.IsNullOrEmpty(userAssetPath))
                    { // scene object
                        Scene scene = SceneManager.GetActiveScene();
                        if (!scene.IsValid())
                            return;

                        RedirectorDB.UnloadRefObject(oldGUID, AssetDatabase.AssetPathToGUID(scene.path), m_FileID);
                    }
                    else
                    {
                        RedirectorDB.UnloadRefObject(oldGUID, AssetDatabase.AssetPathToGUID(userAssetPath), m_FileID);
                    }
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