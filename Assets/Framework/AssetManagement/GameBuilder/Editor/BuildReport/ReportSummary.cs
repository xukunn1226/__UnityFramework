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

		/// <summary>
		/// 加密服务类名称
		/// </summary>
		// public string EncryptionServicesClassName;

		// 构建参数
		public EOutputNameStyle OutputNameStyle;
		public BuildCompression CompressOption;
		public bool DisableWriteTypeTree;

		// 构建结果
		public int AssetFileTotalCount;
		public int MainAssetTotalCount;
		public int AllBundleTotalCount;
		public long AllBundleTotalSize;
		// public int EncryptedBundleTotalCount;
		// public long EncryptedBundleTotalSize;
		public int RawBundleTotalCount;
		public long RawBundleTotalSize;
	}
}