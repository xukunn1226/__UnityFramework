using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ShowOverdraw))]
public class ShowOverdrawEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ShowOverdraw obj = (ShowOverdraw)target;

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField("RenderTexture", obj.m_RT, typeof(RenderTexture), false);
        
        EditorGUILayout.LabelField("Frame Count", obj.m_FrameCount.ToString());
        EditorGUILayout.LabelField("Pixel Total", obj.GetAveragePixDraw().ToString());
        EditorGUILayout.LabelField("Actual Pixel Total", obj.GetAverageActualPixDraw().ToString());
        EditorGUILayout.LabelField("Fill Rate", obj.GetAverageFillrate().ToString());
        
        EditorGUI.EndDisabledGroup();
    }
}