using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Framework.AssetManagement.Runtime;
using System;
using UnityEngine.Build.Pipeline;
using System.Linq;

namespace Framework.AssetManagement.AssetEditorWindow
{
    [TaskAttribute("Step5. 创建清单文件")]
    public class TaskCreateManifest : IGameBuildTask
    {
        void IGameBuildTask.Run(BuildContext context)
        {
            var buildMapContext = context.GetContextObject<BuildMapContext>();
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildResultContext = context.GetContextObject<BuildResultContext>();
            string cacheBundlesOutput = buildParametersContext.GetCacheBundlesOutput();

            // 创建清单文件
            AssetManifest manifest = new AssetManifest();
            manifest.SerializedVersion = 1;
            manifest.PackageVersion = buildParametersContext.GetPackageVersion();
            manifest.OutputNameStyle = (int)buildParametersContext.gameBuilderSetting.bundleSetting.nameStyle;       // see EOutputNameStyle

            foreach (var bundleInfo in buildMapContext.BuildBundleInfos)
            {
                manifest.BundleList.Add(bundleInfo.CreateBundleDescriptor());
            }

            foreach (var bundleInfo in buildMapContext.BuildBundleInfos)
            {
                foreach (var assetInfo in bundleInfo.BuildinAssets)
                {
                    var assetDesc = new AssetDescriptor();
                    assetDesc.assetPath = assetInfo.AssetPath.ToLower();

                    // find main bundle
                    int index = manifest.BundleList.FindIndex(item => item.bundleName == assetInfo.MainBundleName);
                    if (index == -1)
                        throw new Exception($"should never get here! Can't find bundle {assetInfo.MainBundleName} from BundleList");
                    assetDesc.bundleID = index;

                    // fill depend bundles
                    List<int> dependIDs = new List<int>();
                    foreach (var dependBundleName in assetInfo.AllDependBundleNames)
                    {
                        index = manifest.BundleList.FindIndex(item => item.bundleName == dependBundleName);
                        if (index == -1)
                            throw new Exception($"should never get here! Can't find bundle {dependBundleName} from BundleList");
                        dependIDs.Add(index);
                    }
                    assetDesc.dependIDs = dependIDs.ToArray();
                    manifest.AssetList.Add(assetDesc);
                }
            }

            // 更新资源包之间的引用关系
            UpdateScriptPipelineReference(manifest, buildResultContext);

            // 创建清单文件文本
            string fileName = AssetManagerSettingsData.GetManifestJsonFileName(buildParametersContext.GetPackageVersion());
            string filePath = $"{cacheBundlesOutput}/{fileName}";
            AssetManifest.SerializeToJson(filePath, manifest);
            BuildRunner.Log($"创建补丁清单文件：{filePath}");

            // 创建清单二进制文件
            fileName = AssetManagerSettingsData.GetManifestBinaryFileName(buildParametersContext.GetPackageVersion());
            filePath = $"{cacheBundlesOutput}/{fileName}";
            AssetManifest.SerializeToBinary(filePath, manifest);
            string packageHash = HashUtility.FileMD5(filePath);
            BuildRunner.Log($"创建补丁清单文件：{filePath}");

            AssetManifestContext patchManifestContext = new AssetManifestContext();
            byte[] bytesData = FileUtility.ReadAllBytes(filePath);
            patchManifestContext.Manifest = AssetManifest.DeserializeFromBinary(bytesData);
            context.SetContextObject(patchManifestContext);

            // 创建清单的哈希文件
            fileName = AssetManagerSettingsData.GetManifestHashFileName(buildParametersContext.GetPackageVersion());
            filePath = $"{cacheBundlesOutput}/{fileName}";
            FileUtility.CreateFile(filePath, packageHash);
            BuildRunner.Log($"创建补丁清单哈希文件：{filePath}");

            // 创建清单版本文件
            fileName = AssetManagerSettingsData.GetManifestVersionFileName();
            filePath = $"{cacheBundlesOutput}/{fileName}";
            FileUtility.CreateFile(filePath, buildParametersContext.GetPackageVersion());
            BuildRunner.Log($"创建补丁清单版本文件：{filePath}");

            // 输出UnityManifest，调试用
            var unityManifest = ScriptableObject.CreateInstance<CompatibilityAssetBundleManifest>();
            unityManifest.SetResults(buildResultContext.Results.BundleInfos);
            fileName = "UnityManifest.asset";
            string srcFilePath = $"Assets/Temp";
            string dstFilePath = $"{cacheBundlesOutput}/{fileName}";
            EditorTools.CreateDirectory(srcFilePath);
            AssetDatabase.CreateAsset(unityManifest, $"{srcFilePath}/{fileName}");
            EditorTools.CopyFile($"{srcFilePath}/{fileName}", dstFilePath, true);
        }

         private readonly Dictionary<string, int> m_CachedBundleID = new Dictionary<string, int>(10000);         // 记录bunldeName对应的BundleID
		private readonly Dictionary<string, string[]> m_CachedBundleDepends = new Dictionary<string, string[]>(10000);      // 记录bundleName对应依赖的bundleNames

		private void UpdateScriptPipelineReference(AssetManifest manifest, BuildResultContext buildResultContext)
		{
			int progressValue;
			int totalCount = manifest.BundleList.Count;

			// 缓存资源包ID
			m_CachedBundleID.Clear();
			progressValue = 0;
			foreach (var bundleDesc in manifest.BundleList)
			{
				int bundleID = GetAssetBundleID(bundleDesc.bundleName, manifest);
				m_CachedBundleID.Add(bundleDesc.bundleName, bundleID);
				EditorTools.DisplayProgressBar("缓存资源包索引", ++progressValue, totalCount);
			}
			EditorTools.ClearProgressBar();

			// 缓存资源包依赖
			m_CachedBundleDepends.Clear();
			progressValue = 0;
			foreach (var bundleDesc in manifest.BundleList)
			{
				if (bundleDesc.isRawFile)
				{
					m_CachedBundleDepends.Add(bundleDesc.bundleName, new string[] { });
					continue;
				}

				if (buildResultContext.Results.BundleInfos.ContainsKey(bundleDesc.bundleName) == false)
					throw new Exception($"Not found bundle in SBP build results : {bundleDesc.bundleName}");

				var depends = buildResultContext.Results.BundleInfos[bundleDesc.bundleName].Dependencies;
				m_CachedBundleDepends.Add(bundleDesc.bundleName, depends);
				EditorTools.DisplayProgressBar("缓存资源包依赖列表", ++progressValue, totalCount);
			}
			EditorTools.ClearProgressBar();

			// 计算资源包引用列表
			// foreach (var bundleDesc in manifest.BundleList)
			// {
			// 	bundleDesc.referenceIDs = GetBundleRefrenceIDs(manifest, bundleDesc);
			// 	EditorTools.DisplayProgressBar("计算资源包引用关系", ++progressValue, totalCount);
			// }
			// EditorTools.ClearProgressBar();

            // 计算资源包引用列表（优化）
            GetBundleRefrenceIDsEx(manifest);
		}

        private int GetAssetBundleID(string bundleName, AssetManifest manifest)
		{
			for (int index = 0; index < manifest.BundleList.Count; index++)
			{
				if (manifest.BundleList[index].bundleName == bundleName)
					return index;
			}
			throw new Exception($"Not found bundle name : {bundleName}");
		}

        /// <summary>
        /// 找到依赖了targetBundle的资源包列表
        /// </summary>
        /// <param name="manifest"></param>
        /// <param name="targetBundle"></param>
        /// <returns></returns>
        private int[] GetBundleRefrenceIDs(AssetManifest manifest, BundleDescriptor targetBundle)
		{
			List<string> referenceList = new List<string>();
			foreach (var bundleDesc in manifest.BundleList)
			{
				string bundleName = bundleDesc.bundleName;
				if (bundleName == targetBundle.bundleName)
					continue;

				string[] dependencies = GetCachedBundleDepends(bundleName);
				if (dependencies.Contains(targetBundle.bundleName))
				{
					referenceList.Add(bundleName);
				}
			}

			List<int> result = new List<int>();
			foreach (var bundleName in referenceList)
			{
				int bundleID = GetCachedBundleID(bundleName);
				if (result.Contains(bundleID) == false)
					result.Add(bundleID);
			}
			return result.ToArray();
		}

        private void GetBundleRefrenceIDsEx(AssetManifest manifest)
		{
            for(int bundleID = 0; bundleID < manifest.BundleList.Count; ++bundleID)
            {
                var bundleDesc = manifest.BundleList[bundleID];
                if(bundleDesc.isRawFile)
                    continue;
                    
                HashSet<int> referenceList = new HashSet<int>();
                foreach(var assetInfo in manifest.AssetList)
                {
                    if(assetInfo.dependIDs.Contains(bundleID))
                    {
                        referenceList.Add(assetInfo.bundleID);
                    }
                    bundleDesc.referenceIDs = referenceList.ToArray();
                }
            }
		}

        private int GetCachedBundleID(string bundleName)
		{
			if (m_CachedBundleID.TryGetValue(bundleName, out int value) == false)
			{
				throw new Exception($"Not found cached bundle ID : {bundleName}");
			}
			return value;
		}

        private string[] GetCachedBundleDepends(string bundleName)
		{
			if (m_CachedBundleDepends.TryGetValue(bundleName, out string[] value) == false)
			{
				throw new Exception($"Not found cached bundle depends : {bundleName}");
			}
			return value;
		}
    }
}