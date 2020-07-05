using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MeshParticleSystem.Profiler
{
    [CustomEditor(typeof(ParticleProfiler))]
    public class ParticleProfilerEditor : UnityEditor.Editor
    {
        private ParticleProfiler m_Target;

        void OnEnable()
        {
            m_Target = (ParticleProfiler)target;
        }

		void OnSceneGUI()
		{
			Handles.BeginGUI();

        	GUILayout.BeginArea(new Rect(10, 80, 300, 300));
	        ShowStat();
	        GUILayout.EndArea();

    	    Handles.EndGUI();

			Repaint();
		}

        public override void OnInspectorGUI()
        {
			ShowStat();

			// 曲线图
			EditorGUILayout.Space();
			using (new EditorGUI.DisabledScope(false))
			{
				EditorGUILayout.Foldout(true, "时间曲线");
				{
					float curveHeight = 80;
					EditorGUI.indentLevel = 1;
					EditorGUILayout.CurveField("DrawCall", m_Target.m_Data.DrawCallCurve, GUILayout.Height(curveHeight));
					EditorGUILayout.CurveField("粒子数量", m_Target.m_Data.ParticleCountCurve, GUILayout.Height(curveHeight));
					EditorGUILayout.CurveField("三角面数", m_Target.m_Data.TriangleCountCurve, GUILayout.Height(curveHeight));
					EditorGUI.indentLevel = 0;
				}
			}

			// 纹理列表
			EditorGUILayout.Space();
			using (new EditorGUI.DisabledScope(false))
			{
				EditorGUILayout.Foldout(true, "纹理列表");
				{
					EditorGUI.indentLevel = 1;
					{
						List<Texture> textures = m_Target.m_Data.allTextures;
						foreach (var tex in textures)
						{
							EditorGUILayout.LabelField($"{tex.name}  尺寸:{tex.height }*{tex.width}  格式:{Utility.GetTextureFormatString(tex)}");
							EditorGUILayout.ObjectField("", tex, typeof(Texture), false, GUILayout.Width(80));
						}
					}
					EditorGUI.indentLevel = 0;
				}
			}

			// 网格列表
			EditorGUILayout.Space();
			using (new EditorGUI.DisabledScope(false))
			{
				EditorGUILayout.Foldout(true, "网格列表");
				{
					EditorGUI.indentLevel = 1;
					{
						List<Mesh> meshs = m_Target.m_Data.allMeshes;
						foreach (var mesh in meshs)
						{
							EditorGUILayout.ObjectField($"三角面数 : {mesh.triangles.Length / 3}", mesh, typeof(MeshFilter), false, GUILayout.Width(300));
						}
					}
					EditorGUI.indentLevel = 0;
				}
			}
        }

		private void ShowStat()
		{
			GUIStyle style = new GUIStyle();
    	    style.richText = true;

			EditorGUILayout.LabelField(FormatStat("材质数量", m_Target.m_Data.materialCount, ParticleProfiler.kRecommendMaterialCount), style);
			EditorGUILayout.LabelField(FormatStat("纹理数量", m_Target.m_Data.allTextures.Count, ParticleProfiler.kRecommendTextureCount), style);
			EditorGUILayout.LabelField(FormatMemoryStat("纹理内存(当前平台)", m_Target.m_Data.textureMemory, ParticleProfiler.kRecommendedTextureMemorySize), style);
			EditorGUILayout.LabelField(FormatMemoryStat("纹理内存(ETC2)", m_Target.m_Data.textureMemoryOnAndroid, ParticleProfiler.kRecommendedTextureMemorySize), style);
			EditorGUILayout.LabelField(FormatMemoryStat("纹理内存(ASTC6x6)", m_Target.m_Data.textureMemoryOnIPhone, ParticleProfiler.kRecommendedTextureMemorySize), style);
			EditorGUILayout.LabelField(FormatStat("网格数量", m_Target.m_Data.allMeshes.Count, ParticleProfiler.kRecommendMeshCount), style);
			EditorGUILayout.LabelField(FormatStat("粒子系统组件", m_Target.m_Data.componentCount, ParticleProfiler.kRecommendParticleCompCount), style);
            
			EditorGUILayout.Space();
            EditorGUILayout.LabelField($"模拟时长：{m_Target.elapsedTime}");
			EditorGUILayout.LabelField(FormatStat("DrawCall", m_Target.m_Data.curDrawCall, m_Target.m_Data.maxDrawCall, ParticleProfiler.kRecommendDrawCallCount), style);
			EditorGUILayout.LabelField(FormatStat("粒子数量", m_Target.m_Data.curParticleCount, m_Target.m_Data.maxParticleCount, ParticleProfiler.kRecommendParticleCount), style);
			EditorGUILayout.LabelField($"三角面数： {m_Target.m_Data.curTriangles}      最大：{m_Target.m_Data.maxTriangles}");
		}

		private string FormatStat(string label, float value, float threshold)
		{
			string v;
			if (value <= threshold)
            	v = string.Format("<color=green>{0}</color>", value);
        	else
            	v = string.Format("<color=red>{0}</color>", value);

			return string.Format($"{label}: {v}      建议：<{threshold}");
		}

		private string FormatStat(string label, float value, float max, float threshold)
		{
			string v;
			if (value <= threshold)
            	v = string.Format("<color=green>{0}</color>", value);
        	else
            	v = string.Format("<color=red>{0}</color>", value);

			return string.Format($"{label}: {v}      最大：{max}      建议：<{threshold}");
		}

		private string FormatMemoryStat(string label, long value, float threshold)
		{
			string v;
			if (value <= threshold)
            	v = string.Format("<color=green>{0}</color>", EditorUtility.FormatBytes(value));
        	else
            	v = string.Format("<color=red>{0}</color>", EditorUtility.FormatBytes(value));

			return string.Format($"{label}: {v}      建议：<{threshold}");
		}
    }
}