using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MeshParticleSystem.Profiler.Editor
{
	public class ParticleProfilerWindow : EditorWindow
	{
		static ParticleProfilerWindow m_Instance;

		[MenuItem("MyTools/Particle Profiler", false, 201)]
		static void ShowWindow()
		{
			if (m_Instance == null)
			{
				m_Instance = EditorWindow.GetWindow(typeof(ParticleProfilerWindow), false, "Particle Profiler", true) as ParticleProfilerWindow;
				// m_Instance.minSize = new Vector2(800, 600);
			}
			m_Instance.Show();
		}

		private UnityEngine.Object m_ParticlePrefab;
		private Vector2 m_ScrollPos;

		private void Update()
		{
			Repaint();
		}

		private void OnGUI()
		{
			EditorGUILayout.Space();

			m_ParticlePrefab = EditorGUILayout.ObjectField($"Select Particle", m_ParticlePrefab, typeof(UnityEngine.GameObject), false);

			if (GUILayout.Button("Profile"))
			{
				// 焦点锁定游戏窗口
				var gameViewType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView");
				EditorWindow gameView = EditorWindow.GetWindow(gameViewType);
				gameView.Focus();
				
				ParticleProfiler.StartProfiler(AssetDatabase.GetAssetPath(m_ParticlePrefab));
			}

			m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos);

			EditorGUILayout.Space();
			EditorGUILayout.LabelField($"采样时长: {ParticleProfiler.elapsedTime.ToString()}");

			EditorGUILayout.Space();
			EditorGUILayout.LabelField($"材质数量：{ParticleProfiler.profilingData.materialCount}");
			EditorGUILayout.LabelField($"纹理数量：{ParticleProfiler.profilingData.allTextures.Count}");
			EditorGUILayout.LabelField($"纹理内存：{EditorUtility.FormatBytes(ParticleProfiler.profilingData.textureMemory)}");
			EditorGUILayout.LabelField($"粒子系统组件：{ParticleProfiler.profilingData.allParticles.Count} 个");
			
			EditorGUILayout.Space();
			EditorGUILayout.LabelField($"DrawCall：{ParticleProfiler.profilingData.curDrawCall}  最大：{ParticleProfiler.profilingData.maxDrawCall}");
			EditorGUILayout.LabelField($"粒子数量：{ParticleProfiler.profilingData.curParticleCount}  最大：{ParticleProfiler.profilingData.maxParticleCount}");
			EditorGUILayout.LabelField($"三角面数：{ParticleProfiler.profilingData.curTriangles}  最大：{ParticleProfiler.profilingData.maxTriangles}");

			// 曲线图
			EditorGUILayout.Space();
			using (new EditorGUI.DisabledScope(false))
			{
				EditorGUILayout.Foldout(true, "时间曲线");
				{
					float curveHeight = 80;
					EditorGUI.indentLevel = 1;
					EditorGUILayout.CurveField("DrawCall", ParticleProfiler.profilingData.DrawCallCurve, GUILayout.Height(curveHeight));
					EditorGUILayout.CurveField("粒子数量", ParticleProfiler.profilingData.ParticleCountCurve, GUILayout.Height(curveHeight));
					EditorGUILayout.CurveField("三角面数", ParticleProfiler.profilingData.TriangleCountCurve, GUILayout.Height(curveHeight));
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
						List<Texture> textures = ParticleProfiler.profilingData.allTextures;
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
						List<Mesh> meshs = ParticleProfiler.profilingData.allMeshes;
						foreach (var mesh in meshs)
						{
							EditorGUILayout.ObjectField($"三角面数 : {mesh.triangles.Length / 3}", mesh, typeof(MeshFilter), false, GUILayout.Width(300));
						}
					}
					EditorGUI.indentLevel = 0;
				}
			}

			EditorGUILayout.EndScrollView();
		}
	}
}