using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using Framework.Core;
// using Framework.AssetManagement.Runtime;
// using Framework.Cache;
using Object = UnityEngine.Object;
using UnityEditor;
using Framework.Core.Editor;

namespace Framework.Core.Editor
{
// [CustomEditor(typeof(SoftObject))]
// public class SoftObjectEditor : SoftObjectPathEditor
// {
//     private SerializedProperty m_LRUedPoolAssetProp;
//     private SerializedProperty m_UseLRUManageProp;

//     public override void OnEnable()
//     {
//         base.OnEnable();
//         m_LRUedPoolAssetProp = serializedObject.FindProperty("m_LRUedPoolAsset");
//         m_UseLRUManageProp = serializedObject.FindProperty("m_UseLRUManage");
//     }

//     public override void OnInspectorGUI()
//     {
//         base.OnInspectorGUI();

//         EditorGUILayout.Separator();

//         serializedObject.Update();

//         m_UseLRUManageProp.boolValue = EditorGUILayout.Toggle("Use LRU", m_UseLRUManageProp.boolValue);

//         EditorGUI.BeginDisabledGroup(!m_UseLRUManageProp.boolValue);
//         EditorGUILayout.ObjectField(m_LRUedPoolAssetProp, new GUIContent("LRU Pool", "If you would use LRU Pool manage the asset"));
//         EditorGUI.EndDisabledGroup();

//         serializedObject.ApplyModifiedProperties();
//     }
// }
}