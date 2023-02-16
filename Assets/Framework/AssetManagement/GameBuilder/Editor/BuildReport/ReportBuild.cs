using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Framework.AssetManagement.Runtime;

namespace Framework.AssetManagement.AssetEditorWindow
{
	/// <summary>
	/// 构建报告
	/// </summary>
	[Serializable]
	public class ReportBuild
	{
		/// <summary>
		/// 汇总信息
		/// </summary>
		public ReportSummary Summary = new ReportSummary();

		/// <summary>
		/// 资源对象列表
		/// </summary>
		public List<ReportAssetInfo> AssetInfos = new List<ReportAssetInfo>();

		/// <summary>
		/// 资源包列表
		/// </summary>
		public List<ReportBundleInfo> BundleInfos = new List<ReportBundleInfo>();


		/// <summary>
		/// 获取资源包信息类
		/// </summary>
		public ReportBundleInfo GetBundleInfo(string bundleName)
		{
			foreach (var bundleInfo in BundleInfos)
			{
				if (bundleInfo.BundleName == bundleName)
					return bundleInfo;
			}
			throw new Exception($"Not found bundle : {bundleName}");
		}

		/// <summary>
		/// 获取资源信息类
		/// </summary>
		public ReportAssetInfo GetAssetInfo(string assetPath)
		{
			foreach (var assetInfo in AssetInfos)
			{
				if (assetInfo.AssetPath == assetPath)
					return assetInfo;
			}
			throw new Exception($"Not found asset : {assetPath}");
		}

		public static void Serialize(string savePath, ReportBuild buildReport)
		{
			if (File.Exists(savePath))
				File.Delete(savePath);

			string json = JsonUtility.ToJson(buildReport, true);
			FileUtility.CreateFile(savePath, json);
		}

		public static ReportBuild Deserialize(string jsonData)
		{
			ReportBuild report = JsonUtility.FromJson<ReportBuild>(jsonData);
			return report;
		}
	}
}