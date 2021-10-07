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
			DrawStat(m_Target.m_Data);
	        GUILayout.EndArea();

    	    Handles.EndGUI();

			Repaint();
		}

        public override void OnInspectorGUI()
        {
			DrawStat(m_Target.m_Data);
			DrawChart(m_Target.m_Data);
        }

		static public void DrawChart(ParticleProfiler.ProfilerData profilerData)
		{			
			// 曲线图
			EditorGUILayout.Space();
			using (new EditorGUI.DisabledScope(false))
			{
				EditorGUILayout.Foldout(true, "时间曲线");
				{
					float curveHeight = 80;
					EditorGUI.indentLevel = 1;
					EditorGUILayout.CurveField("DrawCall", profilerData.DrawCallCurve, GUILayout.Height(curveHeight));
					EditorGUILayout.CurveField("粒子数量", profilerData.ParticleCountCurve, GUILayout.Height(curveHeight));
					EditorGUILayout.CurveField("三角面数", profilerData.TriangleCountCurve, GUILayout.Height(curveHeight));
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
						List<Texture> textures = profilerData.allTextures;
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
						List<Mesh> meshs = profilerData.allMeshes;
						foreach (var mesh in meshs)
						{
							EditorGUILayout.ObjectField($"三角面数 : {mesh.triangles.Length / 3}", mesh, typeof(MeshFilter), false, GUILayout.Width(300));
						}
					}
					EditorGUI.indentLevel = 0;
				}
			}
		}

		static public void DrawStat(ParticleProfiler.ProfilerData profilerData)
		{
			GUIStyle style = new GUIStyle();
    	    style.richText = true;

			EditorGUILayout.LabelField(FormatStat("材质数量", profilerData.materialCount, ParticleProfiler.kRecommendMaterialCount), style);
			EditorGUILayout.LabelField(FormatStat("纹理数量", profilerData.allTextures.Count, ParticleProfiler.kRecommendTextureCount), style);
			EditorGUILayout.LabelField(FormatMemoryStat("纹理内存(当前平台)", profilerData.textureMemory, ParticleProfiler.kRecommendedTextureMemorySize), style);
			EditorGUILayout.LabelField(FormatMemoryStat("纹理内存(ETC2)", profilerData.textureMemoryOnAndroid, ParticleProfiler.kRecommendedTextureMemorySize), style);
			EditorGUILayout.LabelField(FormatMemoryStat("纹理内存(ASTC6x6)", profilerData.textureMemoryOnIPhone, ParticleProfiler.kRecommendedTextureMemorySize), style);
			EditorGUILayout.LabelField(FormatStat("网格数量", profilerData.allMeshes.Count, ParticleProfiler.kRecommendMeshCount), style);
			EditorGUILayout.LabelField(FormatStat("粒子系统组件", profilerData.componentCount, ParticleProfiler.kRecommendParticleCompCount), style);
            
			EditorGUILayout.Space();
            // EditorGUILayout.LabelField($"模拟时长：{m_Target.elapsedTime}");
			EditorGUILayout.LabelField(FormatStat("DrawCall", profilerData.curDrawCall, profilerData.maxDrawCall, ParticleProfiler.kRecommendDrawCallCount), style);
			EditorGUILayout.LabelField(FormatStat("粒子数量", profilerData.curParticleCount, profilerData.maxParticleCount, ParticleProfiler.kRecommendParticleCount), style);
			EditorGUILayout.LabelField($"三角面数： {profilerData.curTriangles}      最大：{profilerData.maxTriangles}");			
		}

		static private string FormatStat(string label, float value, float threshold)
		{
			string v;
			if (value <= threshold)
            	v = string.Format("<color=green>{0}</color>", value);
        	else
            	v = string.Format("<color=red>{0}</color>", value);

			return string.Format($"{label}: {v}      建议：<{threshold}");
		}

		static private string FormatStat(string label, float value, float max, float threshold)
		{
			string v;
			if (value <= threshold)
            	v = string.Format("<color=green>{0}</color>", value);
        	else
            	v = string.Format("<color=red>{0}</color>", value);

			return string.Format($"{label}: {v}      最大：{max}      建议：<{threshold}");
		}

		static private string FormatMemoryStat(string label, long value, float threshold)
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