using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Framework.AssetManagement.Runtime;

namespace Framework.AssetManagement.AssetEditorWindow
{
	[Serializable]
	public class ReportSummary
	{
		/// <summary>
		/// 引擎版本
		/// </summary>
		public string UnityVersion;

		/// <summary>
		/// 构建时间
		/// </summary>
		public string BuildDate;
		
		/// <summary>
		/// 构建耗时（单位：秒）
		/// </summary>
		public int BuildSeconds;

		/// <summary>
		/// 构建平台
		/// </summary>
		public BuildTarget BuildTarget;

		/// <summary>
		/// 构建包裹版本
		/// </summary>
		public string BuildPackageVersion;

		// 构建参数
		public EOutputNameStyle OutputNameStyle;
		public BuildCompression CompressOption;
		public bool DisableWriteTypeTree;

		/// <summary>
		/// 参与构建的资源总数
		/// </summary>
		public int AssetFileTotalCount;

		/// <summary>
		/// 写入清单的资源总数
		/// </summary>
		public int MainAssetTotalCount;

		/// <summary>
		/// 资源包总数
		/// </summary>
		public int AllBundleTotalCount;

		/// <summary>
		/// 资源包总大小，包含原生文件
		/// </summary>
		public long AllBundleTotalSize;

		/// <summary>
		/// 原生资源包总数
		/// </summary>
		public int RawBundleTotalCount;

		/// <summary>
		/// 原生资源包总大小
		/// </summary>
		public long RawBundleTotalSize;

		/// <summary>
		/// 平均资源包依赖数
		/// </summary>
		public float AverageDependBundlesCount;

		/// <summary>
		/// 最大的依赖资源包数
		/// </summary>
		public int MaxDependBundlesCount;
	}
}