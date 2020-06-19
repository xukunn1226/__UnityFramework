//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

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
				m_Instance = EditorWindow.GetWindow(typeof(ParticleProfilerWindowEx), false, "特效分析器", true) as ParticleProfilerWindowEx;
				m_Instance.minSize = new Vector2(800, 600);
			}

			m_Instance.Show();
		}

		/// <summary>
		/// 特效预制体
		/// </summary>
		private UnityEngine.Object m_ParticlePrefab = null;

		/// <summary>
		/// 粒子测试类
		/// </summary>
		private ParticleProfiler _profiler = new ParticleProfiler();

		private ParticleProfilerEx m_Profiler;
		private float m_LastTime;


		private double _lastTime = 0;
		private Vector2 _scrollPos1 = Vector2.zero;
		private Vector2 _scrollPos2 = Vector2.zero;
		private Vector2 _scrollPos3 = Vector2.zero;
		private bool _isShowCurves = true;
		private bool _isShowTextures = false;
		private bool _isShowMeshs = false;
		private bool _isShowTips = false;
		private Texture2D _texTips = null;


		private void Awake()
		{
			_lastTime = EditorApplication.timeSinceStartup;
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
				
				ParticleProfilerEx.StartProfiler(AssetDatabase.GetAssetPath(m_ParticlePrefab));
				m_LastTime = (float)EditorApplication.timeSinceStartup;
			}

			// 粒子基本信息
			EditorGUILayout.Space();
			EditorGUILayout.LabelField($"材质数量：{_profiler.MaterialCount}");
			EditorGUILayout.LabelField($"纹理数量：{_profiler.TextureCount}");
			EditorGUILayout.LabelField($"纹理内存：{EditorUtility.FormatBytes(_profiler.TextureMemory)}");
			EditorGUILayout.LabelField($"粒子系统组件：{_profiler.ParticleSystemComponentCount} 个");

			// 粒子动态信息
			EditorGUILayout.Space();
			EditorGUILayout.LabelField($"DrawCall：{_profiler.DrawCallCurrentNum}  最大：{_profiler.DrawCallMaxNum}");
			EditorGUILayout.LabelField($"粒子数量：{_profiler.ParticleCurrentCount}  最大：{_profiler.ParticleMaxCount}");
			EditorGUILayout.LabelField($"三角面数：{_profiler.TriangleCurrentCount}  最大：{_profiler.TriangleMaxCount}");

			// 曲线图
			EditorGUILayout.Space();
			using (new EditorGUI.DisabledScope(false))
			{
				_isShowCurves = EditorGUILayout.Foldout(_isShowCurves, "时间曲线");
				if (_isShowCurves)
				{
					float curveHeight = 80;
					EditorGUI.indentLevel = 1;
					EditorGUILayout.LabelField($"采样时长 {_profiler.CurveSampleTime} 秒");
					EditorGUILayout.CurveField("DrawCall", _profiler.DrawCallCurve, GUILayout.Height(curveHeight));
					EditorGUILayout.CurveField("粒子数量", _profiler.ParticleCountCurve, GUILayout.Height(curveHeight));
					EditorGUILayout.CurveField("三角面数", _profiler.TriangleCountCurve, GUILayout.Height(curveHeight));
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
						List<Texture> textures = _profiler.AllTextures;
						foreach (var tex in textures)
						{
							EditorGUILayout.LabelField($"{tex.name}  尺寸:{tex.height }*{tex.width}  格式:{ParticleProfiler.GetTextureFormatString(tex)}");
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
						List<Mesh> meshs = _profiler.AllMeshs;
						foreach (var mesh in meshs)
						{
							EditorGUILayout.ObjectField($"三角面数 : {mesh.triangles.Length / 3}", mesh, typeof(MeshFilter), false, GUILayout.Width(300));
						}
					}
					EditorGUILayout.EndScrollView();
					EditorGUI.indentLevel = 0;
				}
			}

			// 过程化检测结果
			EditorGUILayout.Space();
			using (new EditorGUI.DisabledScope(false))
			{
				_isShowTips = EditorGUILayout.Foldout(_isShowTips, "过程化检测结果");
				if (_isShowTips)
				{
					EditorGUI.indentLevel = 1;
					_scrollPos3 = EditorGUILayout.BeginScrollView(_scrollPos3);
					{
						GUILayout.Button(_texTips); //绘制提示图片
						EditorGUILayout.HelpBox($"以下粒子系统组件不支持过程化模式！具体原因查看气泡提示", MessageType.Warning, true);
#if UNITY_2018_4_OR_NEWER
						List<ParticleSystem> particleList = _profiler.AllParticles;
						foreach (var ps in particleList)
						{
							if (ps.proceduralSimulationSupported == false)
								EditorGUILayout.ObjectField($"{ps.gameObject.name}", ps.gameObject, typeof(GameObject), false, GUILayout.Width(300));
						}
#else
					EditorGUILayout.LabelField("当前版本不支持过程化检测，请升级至2018.4版本或最新版本");
#endif
					}
					EditorGUILayout.EndScrollView();
					EditorGUI.indentLevel = 0;
				}
			}
		}
		private void OnDestroy()
		{
			_profiler.DestroyPrefab();
		}
	}
}