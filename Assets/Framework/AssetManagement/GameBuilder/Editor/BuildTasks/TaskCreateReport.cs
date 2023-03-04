using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using Framework.AssetManagement.Runtime;

namespace Framework.AssetManagement.AssetEditorWindow
{
	[TaskAttribute("step6. 创建构建报告文件")]
	public class TaskCreateReport : IGameBuildTask
	{
		void IGameBuildTask.Run(BuildContext context)
		{
			var buildParameters = context.GetContextObject<BuildParametersContext>();
			var buildMapContext = context.GetContextObject<BuildMapContext>();
			var assetManifestContext = context.GetContextObject<AssetManifestContext>();
			buildParameters.StopWatch();

			CreateReportFile(buildParameters, buildMapContext, assetManifestContext);

			float buildSeconds = buildParameters.GetBuildingSeconds();
			BuildRunner.Info($"构建消耗时间： {buildSeconds}s");
		}

		private void CreateReportFile(BuildParametersContext buildParametersContext, BuildMapContext buildMapContext, AssetManifestContext manifestContext)
		{
			var gameBuilderSetting = buildParametersContext.gameBuilderSetting;

			string bundleOutputDirectory = buildParametersContext.GetCacheBundlesOutput();
			AssetManifest assetManifest = manifestContext.Manifest;
			ReportBuild buildReport = new ReportBuild();

			// 概述信息
			{
				buildReport.Summary.UnityVersion = UnityEngine.Application.unityVersion;
				buildReport.Summary.BuildDate = DateTime.Now.ToString();
				buildReport.Summary.BuildSeconds = (int)buildParametersContext.GetBuildingSeconds();
				buildReport.Summary.BuildTarget = gameBuilderSetting.buildTarget;
				buildReport.Summary.BuildPackageVersion = gameBuilderSetting.packageVersion;

				// 构建参数
				buildReport.Summary.OutputNameStyle = gameBuilderSetting.bundleSetting.nameStyle;
				buildReport.Summary.CompressOption = gameBuilderSetting.bundleSetting.useLZ4Compress ? UnityEngine.BuildCompression.LZ4 : UnityEngine.BuildCompression.Uncompressed;
				buildReport.Summary.DisableWriteTypeTree = gameBuilderSetting.bundleSetting.disableWriteTypeTree;

				// 构建结果
				buildReport.Summary.AssetFileTotalCount = buildMapContext.AssetFileCount;
				buildReport.Summary.MainAssetTotalCount = GetMainAssetCount(assetManifest);
				buildReport.Summary.AllBundleTotalCount = GetAllBundleCount(assetManifest);
				buildReport.Summary.AllBundleTotalSize = GetAllBundleSize(assetManifest);
				buildReport.Summary.RawBundleTotalCount = GetRawBundleCount(assetManifest);
				buildReport.Summary.RawBundleTotalSize = GetRawBundleSize(assetManifest);
                buildReport.Summary.AverageDependBundlesCount = GetAverageDependBundleCount(assetManifest);
                buildReport.Summary.MaxDependBundlesCount = GetMaxDependBundleCount(assetManifest);
                buildReport.Summary.AverageBundleSize = GetAverageBundleSize(assetManifest);
            }

			// 资源对象列表
			buildReport.AssetInfos = new List<ReportAssetInfo>(assetManifest.AssetList.Count);
			foreach (var assetDesc in assetManifest.AssetList)
			{
				var mainBundle = assetManifest.BundleList[assetDesc.bundleID];
				ReportAssetInfo reportAssetInfo = new ReportAssetInfo();
				reportAssetInfo.AssetPath = assetDesc.assetPath;
				reportAssetInfo.AssetGUID = AssetDatabase.AssetPathToGUID(assetDesc.assetPath);
				reportAssetInfo.MainBundleName = mainBundle.bundleName;
				reportAssetInfo.MainBundleSize = mainBundle.fileSize;
				reportAssetInfo.DependBundles = GetDependBundles(assetManifest, assetDesc);
				reportAssetInfo.DependAssets = GetDependAssets(buildMapContext, mainBundle.bundleName, assetDesc.assetPath);
				buildReport.AssetInfos.Add(reportAssetInfo);
			}

			// 资源包列表
			buildReport.BundleInfos = new List<ReportBundleInfo>(assetManifest.BundleList.Count);
			foreach (var bundleDesc in assetManifest.BundleList)
			{
				ReportBundleInfo reportBundleInfo = new ReportBundleInfo();
				reportBundleInfo.BundleName = bundleDesc.bundleName;
				reportBundleInfo.FileName = bundleDesc.fileName;
				reportBundleInfo.FileHash = bundleDesc.fileHash;
				reportBundleInfo.FileCRC = bundleDesc.fileCRC;
				reportBundleInfo.FileSize = bundleDesc.fileSize;
				reportBundleInfo.IsRawFile = bundleDesc.isRawFile;
				reportBundleInfo.LoadMethod = (EBundleLoadMethod)bundleDesc.loadMethod;
				buildReport.BundleInfos.Add(reportBundleInfo);
			}

			// 序列化文件
			string fileName = AssetManagerSettingsData.GetReportFileName(gameBuilderSetting.packageVersion);
			string filePath = $"{bundleOutputDirectory}/{fileName}";
			ReportBuild.Serialize(filePath, buildReport);
			BuildRunner.Log($"资源构建报告文件创建完成：{filePath}");
		}

		/// <summary>
		/// 获取资源对象依赖的所有资源包
		/// </summary>
		private List<string> GetDependBundles(AssetManifest manifest, AssetDescriptor assetDesc)
		{
			List<string> dependBundles = new List<string>(assetDesc.dependIDs.Length);
			foreach (int index in assetDesc.dependIDs)
			{
				string dependBundleName = manifest.BundleList[index].bundleName;
				dependBundles.Add(dependBundleName);
			}
			return dependBundles;
		}

		/// <summary>
		/// 获取资源对象依赖的其它所有资源
		/// </summary>
		private List<string> GetDependAssets(BuildMapContext buildMapContext, string bundleName, string assetPath)
		{
			List<string> result = new List<string>();
			if (buildMapContext.TryGetBundleInfo(bundleName, out BuildBundleInfo bundleInfo))
			{
				BuildAssetInfo findAssetInfo = null;
				foreach (var buildinAsset in bundleInfo.BuildinAssets)
				{
					if(string.Compare(buildinAsset.AssetPath, assetPath, true) == 0)
					{
						findAssetInfo = buildinAsset;
						break;
					}
				}
				if (findAssetInfo == null)
				{
					throw new Exception($"Not found asset {assetPath} in bunlde {bundleName}");
				}
				foreach (var dependAssetInfo in findAssetInfo.AllDependAssetInfos)
				{
					result.Add(dependAssetInfo.AssetPath);
				}
			}
			else
			{
				throw new Exception($"Not found bundle : {bundleName}");
			}
			return result;
		}

		private int GetMainAssetCount(AssetManifest manifest)
		{
			return manifest.AssetList.Count;
		}

		private int GetAllBundleCount(AssetManifest manifest)
		{
			return manifest.BundleList.Count;
		}

		private long GetAllBundleSize(AssetManifest manifest)
		{
			long fileBytes = 0;
			foreach (var bundleDesc in manifest.BundleList)
			{
				fileBytes += bundleDesc.fileSize;
			}
			return fileBytes;
		}

		private int GetEncryptedBundleCount(AssetManifest manifest)
		{
			int fileCount = 0;
			foreach (var bundleDesc in manifest.BundleList)
			{
				if (bundleDesc.loadMethod != (byte)EBundleLoadMethod.LoadFromFile)
					fileCount++;
			}
			return fileCount;
		}

		private long GetEncryptedBundleSize(AssetManifest manifest)
		{
			long fileBytes = 0;
			foreach (var bundleDesc in manifest.BundleList)
			{
				if (bundleDesc.loadMethod != (byte)EBundleLoadMethod.LoadFromFile)
					fileBytes += bundleDesc.fileSize;
			}
			return fileBytes;
		}

		private int GetRawBundleCount(AssetManifest manifest)
		{
			int fileCount = 0;
			foreach (var bundleDesc in manifest.BundleList)
			{
				if (bundleDesc.isRawFile)
					fileCount++;
			}
			return fileCount;
		}

		private long GetRawBundleSize(AssetManifest manifest)
		{
			long fileBytes = 0;
			foreach (var bundleDesc in manifest.BundleList)
			{
				if (bundleDesc.isRawFile)
					fileBytes += bundleDesc.fileSize;
			}
			return fileBytes;
		}

		private float GetAverageDependBundleCount(AssetManifest manifest)
		{
            int allDependBundleTotal = 0;
            foreach(var assetDesc in manifest.AssetList)
            {
                allDependBundleTotal += assetDesc.dependIDs.Length;
            }
			return allDependBundleTotal * 1.0f / manifest.AssetList.Count;
		}

        private int GetMaxDependBundleCount(AssetManifest manifest)
        {
            int maxCount = -1;
            foreach(var assetDesc in manifest.AssetList)
            {
                if(assetDesc.dependIDs.Length > maxCount)
                    maxCount = assetDesc.dependIDs.Length;
            }
            return maxCount;
        }

        private long GetAverageBundleSize(AssetManifest manifest)
        {
            long size = 0;
            foreach(var bundleDesc in manifest.BundleList)
            {
                size += bundleDesc.fileSize;
            }
            return (long)(size * 1.0f / manifest.BundleList.Count);
        }
	}
}