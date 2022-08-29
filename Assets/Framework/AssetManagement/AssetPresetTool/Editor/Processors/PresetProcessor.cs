using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Presets;
using UnityEditor;
using System;
using Framework.AssetManagement.AssetTreeView;

namespace Framework.AssetManagement.AssetProcess
{
    // preset 处理器
    [Serializable]
    public class PresetProcessor : ProcessorBaseData
    {
        public static string DisplayName = "Preset处理器";
        public string PresetGUID = "";

        public override void OnProcessorApply(Category category)
        {

            Debug.Log($"On processor apply preset todo : {PresetGUID}");
        }
        public override void ProceeosrDraw(Rect cellRect, TreeViewItem<ProcessorTreeViewItem> item, int column)
        {
            EditorGUIUtility.labelWidth = cellRect.width / 3;
            // preset data
            PresetProcessor presetProcessor = item.data.processor.data as PresetProcessor;
            string guid = presetProcessor.PresetGUID;
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Preset currentPreset = AssetDatabase.LoadAssetAtPath<Preset>(assetPath);
            Preset afterPreset = EditorGUI.ObjectField(cellRect, "preset", currentPreset, typeof(Preset), false) as Preset;
            long localId;
            bool findRes = AssetDatabase.TryGetGUIDAndLocalFileIdentifier<Preset>(afterPreset, out guid, out localId);
            if (findRes)
            {
                presetProcessor.PresetGUID = guid;
            }
            else
            {
                presetProcessor.PresetGUID = "";
                cellRect.y += 25;
                cellRect.x += cellRect.width/2;
                GUIStyle notiflyStyle = new GUIStyle(GUI.skin.label);
                notiflyStyle.normal.textColor = Color.red;
                EditorGUI.LabelField(cellRect, "preset 为空！", notiflyStyle);
            }
        }
    }
}
