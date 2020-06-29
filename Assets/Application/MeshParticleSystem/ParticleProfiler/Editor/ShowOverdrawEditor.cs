using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MeshParticleSystem.Profiler
{
    [CustomEditor(typeof(ShowOverdraw))]
    public class ShowOverdrawEditor : UnityEditor.Editor
    {
        private ShowOverdraw m_Target;

        void OnEnable()
        {
            m_Target = (ShowOverdraw)target;
        }
        public override void OnInspectorGUI()
        {
            EditorGUILayout.ObjectField("RenderTexture", m_Target.m_RT, typeof(RenderTexture), false);
        
            DrawStat();
        }

        void OnSceneGUI()
        {
            Handles.BeginGUI();
        	GUILayout.BeginArea(new Rect(10, 320, 300, 300));
	        
            DrawStat();

	        GUILayout.EndArea();
    	    Handles.EndGUI();

            Repaint();
        }

        void DrawStat()
        {
            GUIStyle style = new GUIStyle();
    	    style.richText = true;

            EditorGUILayout.LabelField("Frame Count", m_Target.m_Data.m_FrameCount.ToString(), style);
            EditorGUILayout.LabelField("Pixel Total", m_Target.GetAveragePixDraw().ToString(), style);
            EditorGUILayout.LabelField("Actual Pixel Total", m_Target.GetAverageActualPixDraw().ToString(), style);
            
            float fillRate = m_Target.GetAverageFillrate();
            string s;
            if(fillRate < ShowOverdraw.kRecommendFillrate)
                s = string.Format("<color=green>{0}</color>    建议：<{1}", fillRate, ShowOverdraw.kRecommendFillrate);
            else
                s = string.Format("<color=red>{0}</color>    建议：<{1}", fillRate, ShowOverdraw.kRecommendFillrate);
            EditorGUILayout.LabelField("Fill Rate", s, style);
        }
    }
}