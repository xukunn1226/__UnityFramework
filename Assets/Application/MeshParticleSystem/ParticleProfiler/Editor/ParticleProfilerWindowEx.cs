using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MeshParticleSystem.Profiler.Editor
{
	public class ParticleProfilerWindowEx : EditorWindow
	{
		static ParticleProfilerWindowEx m_Instance;

		[MenuItem("MyTools/Particle Profiler", false, 201)]
		static void ShowWindow()
		{
			if (m_Instance == null)
			{
				m_Instance = EditorWindow.GetWindow(typeof(ParticleProfilerWindowEx), false, "Particle Profiler", true) as ParticleProfilerWindowEx;
				m_Instance.minSize = new Vector2(800, 600);
			}
			m_Instance.Show();
		}

		private UnityEngine.Object m_ParticlePrefab = null;
		private ParticleProfilerEx m_Profiler;
		private ParticleProfilingData m_ProfilingData = new ParticleProfilingData();
		private double m_LastTime;

		private Vector2 _scrollPos1 = Vector2.zero;
		private Vector2 _scrollPos2 = Vector2.zero;
		private Vector2 _scrollPos3 = Vector2.zero;
		private bool _isShowCurves = true;
		private bool _isShowTextures = false;
		private bool _isShowMeshs = false;


		private void Awake()
		{
			m_LastTime = EditorApplication.timeSinceStartup;
		}

		private void Update()
		{
			// calc deltaTime
            float deltaTime = (float)(EditorApplication.timeSinceStartup - m_LastTime);
            m_LastTime = (float)EditorApplication.timeSinceStartup;

			if(m_Profiler != null)
			{
				m_Profiler.UpdateSimulate(deltaTime);
			}

			Repaint();
		}

		private void OnGUI()
		{
			EditorGUILayout.Space();

			// 测试特效
			m_ParticlePrefab = EditorGUILayout.ObjectField($"Select Particle", m_ParticlePrefab, typeof(UnityEngine.GameObject), false);

			// 测试按钮
			if (GUILayout.Button("<b>Test</b>"))
			{
				// 焦点锁定游戏窗口
				var gameViewType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView");
				EditorWindow gameView = EditorWindow.GetWindow(gameViewType);
				gameView.Focus();
				
				m_ProfilingData = ParticleProfilerEx.StartProfiler(AssetDatabase.GetAssetPath(m_ParticlePrefab));
				m_LastTime = (float)EditorApplication.timeSinceStartup;
			}

			EditorGUILayout.Space();
			EditorGUILayout.LabelField($"材质数量：{m_ProfilingData.materialCount}");
			EditorGUILayout.LabelField($"纹理数量：{m_ProfilingData.allTextures.Count}");
			EditorGUILayout.LabelField($"纹理内存：{EditorUtility.FormatBytes(m_ProfilingData.textureMemory)}");
			EditorGUILayout.LabelField($"粒子系统组件：{m_ProfilingData.allParticles.Count} 个");
			
			EditorGUILayout.Space();
			EditorGUILayout.LabelField($"DrawCall：{m_ProfilingData.curDrawCall}  最大：{m_ProfilingData.maxDrawCall}");
			EditorGUILayout.LabelField($"粒子数量：{m_ProfilingData.curParticleCount}  最大：{m_ProfilingData.maxParticleCount}");
			EditorGUILayout.LabelField($"三角面数：{m_ProfilingData.curTriangles}  最大：{m_ProfilingData.maxTriangles}");

			// 曲线图
			EditorGUILayout.Space();
			using (new EditorGUI.DisabledScope(false))
			{
				_isShowCurves = EditorGUILayout.Foldout(_isShowCurves, "时间曲线");
				if (_isShowCurves)
				{
					float curveHeight = 80;
					EditorGUI.indentLevel = 1;
					// EditorGUILayout.LabelField($"采样时长 {_profiler.CurveSampleTime} 秒");
					EditorGUILayout.CurveField("DrawCall", m_ProfilingData.DrawCallCurve, GUILayout.Height(curveHeight));
					EditorGUILayout.CurveField("粒子数量", m_ProfilingData.ParticleCountCurve, GUILayout.Height(curveHeight));
					EditorGUILayout.CurveField("三角面数", m_ProfilingData.TriangleCountCurve, GUILayout.Height(curveHeight));
					EditorGUI.indentLevel = 0;
				}
			}

			// 纹理列表
			EditorGUILayout.Space();
			using (new EditorGUI.DisabledScope(false))
			{
				_isShowTextures = EditorGUILayout.Foldout(_isShowTextures, "纹理列表");
				if (_isShowTextures)
				{
					EditorGUI.indentLevel = 1;
					_scrollPos1 = EditorGUILayout.BeginScrollView(_scrollPos1);
					{
						List<Texture> textures = m_ProfilingData.allTextures;
						foreach (var tex in textures)
						{
							EditorGUILayout.LabelField($"{tex.name}  尺寸:{tex.height }*{tex.width}  格式:{Utility.GetTextureFormatString(tex)}");
							EditorGUILayout.ObjectField("", tex, typeof(Texture), false, GUILayout.Width(80));
						}
					}
					EditorGUILayout.EndScrollView();
					EditorGUI.indentLevel = 0;
				}
			}

			// 网格列表
			EditorGUILayout.Space();
			using (new EditorGUI.DisabledScope(false))
			{
				_isShowMeshs = EditorGUILayout.Foldout(_isShowMeshs, "网格列表");
				if (_isShowMeshs)
				{
					EditorGUI.indentLevel = 1;
					_scrollPos2 = EditorGUILayout.BeginScrollView(_scrollPos2);
					{
						List<Mesh> meshs = m_ProfilingData.allMeshes;
						foreach (var mesh in meshs)
						{
							EditorGUILayout.ObjectField($"三角面数 : {mesh.triangles.Length / 3}", mesh, typeof(MeshFilter), false, GUILayout.Width(300));
						}
					}
					EditorGUILayout.EndScrollView();
					EditorGUI.indentLevel = 0;
				}
			}
		}

		private void OnDestroy()
		{
			// _profiler.DestroyPrefab();
		}
	}
}