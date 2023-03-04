using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

namespace Framework.AssetManagement.Runtime
{
    [Serializable]
    public class AssetManifest
    {
        /// <summary>
        /// 序列化版本号
        /// </summary>
        public int                      SerializedVersion;

        /// <summary>
        /// 版本号
        /// </summary>
        public string                   PackageVersion;

		public string					PackageName			{ get { return $"v{PackageVersion}"; } }
		
		/// <summary>
		/// 文件名称样式
		/// </summary>
		public int						OutputNameStyle;

		/// <summary>
		/// 资源列表
		/// </summary>
		public List<AssetDescriptor>    AssetList = new List<AssetDescriptor>();

        /// <summary>
        /// 资源包列表
        /// </summary>
        public List<BundleDescriptor>   BundleList = new List<BundleDescriptor>();

        /// <summary>
        /// 资源集合
        /// KEY: assetPath；VALUE: AssetDescriptor
        /// </summary>
        [NonSerialized]
        public readonly Dictionary<string, AssetDescriptor>     AssetDict = new Dictionary<string, AssetDescriptor>();

        /// <summary>
        /// 资源包集合
        /// KEY: bundleName；VALUE: BundleDescriptor
        /// </summary>
        [NonSerialized]
        public readonly Dictionary<string, BundleDescriptor>    BundleDict = new Dictionary<string, BundleDescriptor>();

		private bool m_EnableToLower;

		public void Init(bool enableToLower)
        {
			m_EnableToLower = enableToLower;
        }

        public BundleDescriptor GetMainBundleDesc(string assetPath)
        {
			if(m_EnableToLower)
				assetPath = assetPath.ToLower();

            if(AssetDict.TryGetValue(assetPath, out AssetDescriptor descriptor))
            {
                int bundleID = descriptor.bundleID;
                if(bundleID >= 0 && bundleID < BundleList.Count)
                {
                    return BundleList[bundleID];
                }
                else
                {
                    throw new Exception($"Invalid BundleID: {bundleID}  AssetPath: {assetPath}");
                }
            }
            else
            {
                throw new Exception($"Can't find {assetPath} from AssetDict");
            }
        }

        public BundleDescriptor[] GetAllDependencies(string assetPath)
        {
			if (m_EnableToLower)
				assetPath = assetPath.ToLower();

			if (AssetDict.TryGetValue(assetPath, out AssetDescriptor descriptor))
            {
                List<BundleDescriptor> result = new List<BundleDescriptor>(descriptor.dependIDs.Length);
                foreach (var dependID in descriptor.dependIDs)
                {
                    if (dependID >= 0 && dependID < BundleList.Count)
                    {
                        var dependPatchBundle = BundleList[dependID];
                        result.Add(dependPatchBundle);
                    }
                    else
                    {
                        throw new Exception($"Invalid bundle id : {dependID} Asset path : {assetPath}");
                    }
                }
                return result.ToArray();
            }
            else
            {
                throw new Exception($"Can't find {assetPath} from AssetDict");
            }
        }

        /// <summary>
        /// 尝试获取补丁资源
        /// </summary>
        public bool TryGetAssetDesc(string assetPath, out AssetDescriptor result)
        {
			if (m_EnableToLower)
				assetPath = assetPath.ToLower();

			return AssetDict.TryGetValue(assetPath, out result);
        }

        /// <summary>
        /// 尝试获取补丁资源包
        /// </summary>
        public bool TryGetBundleDesc(string bundleName, out BundleDescriptor result)
        {
            return BundleDict.TryGetValue(bundleName, out result);
        }

        public static void SerializeToJson(string savePath, AssetManifest manifest)
        {
            string json = JsonUtility.ToJson(manifest, true);
            FileUtility.CreateFile(savePath, json);
        }

		/// <summary>
		/// 序列化（二进制文件）
		/// </summary>
		public static void SerializeToBinary(string savePath, AssetManifest assetManifest)
		{
			using (FileStream fs = new FileStream(savePath, FileMode.Create))
			{
				// 创建缓存器
				BufferWriter buffer = new BufferWriter(AssetManagerSettings.PatchManifestFileMaxSize);

				// 写入文件标记
				buffer.WriteUInt32(AssetManagerSettings.PatchManifestFileSign);

				// 写入文件版本
				buffer.WriteInt32(assetManifest.SerializedVersion);

				// 写入文件头信息
				buffer.WriteInt32(assetManifest.OutputNameStyle);
				buffer.WriteUTF8(assetManifest.PackageVersion);

				// 写入资源列表
				buffer.WriteInt32(assetManifest.AssetList.Count);
				for (int i = 0; i < assetManifest.AssetList.Count; i++)
				{
					var patchAsset = assetManifest.AssetList[i];
					buffer.WriteUTF8(patchAsset.assetPath);
					buffer.WriteInt32(patchAsset.bundleID);
					buffer.WriteInt32Array(patchAsset.dependIDs);
				}

				// 写入资源包列表
				buffer.WriteInt32(assetManifest.BundleList.Count);
				for (int i = 0; i < assetManifest.BundleList.Count; i++)
				{
					var patchBundle = assetManifest.BundleList[i];
					buffer.WriteUTF8(patchBundle.bundleName);
					buffer.WriteUTF8(patchBundle.fileHash);
					buffer.WriteUTF8(patchBundle.fileCRC);
					buffer.WriteInt64(patchBundle.fileSize);
					buffer.WriteBool(patchBundle.isRawFile);
					buffer.WriteByte(patchBundle.loadMethod);
				}

				// 写入文件流
				buffer.WriteToStream(fs);
				fs.Flush();
			}
		}

		/// <summary>
		/// 反序列化（二进制文件）
		/// </summary>
		public static AssetManifest DeserializeFromBinary(byte[] binaryData)
		{
			// 创建缓存器
			BufferReader buffer = new BufferReader(binaryData);

			// 读取文件标记
			uint fileSign = buffer.ReadUInt32();
			if (fileSign != AssetManagerSettings.PatchManifestFileSign)
				throw new Exception("Invalid manifest file !");

			AssetManifest manifest = new AssetManifest();
			{
				// 读取文件版本
				manifest.SerializedVersion = buffer.ReadInt32();
				if (manifest.SerializedVersion != AssetManagerSettings.ManifestSerializationVersion)
					throw new Exception($"The manifest file version are not compatible : {manifest.SerializedVersion} != {AssetManagerSettings.ManifestSerializationVersion}");

				// 读取文件头信息
				manifest.OutputNameStyle = buffer.ReadInt32();
				manifest.PackageVersion = buffer.ReadUTF8();

				// 读取资源列表
				int patchAssetCount = buffer.ReadInt32();
				manifest.AssetList = new List<AssetDescriptor>(patchAssetCount);
				for (int i = 0; i < patchAssetCount; i++)
				{
					var patchAsset = new AssetDescriptor();
					patchAsset.assetPath = buffer.ReadUTF8();
					patchAsset.bundleID = buffer.ReadInt32();
					patchAsset.dependIDs = buffer.ReadInt32Array();
					manifest.AssetList.Add(patchAsset);
				}

				// 读取资源包列表
				int patchBundleCount = buffer.ReadInt32();
				manifest.BundleList = new List<BundleDescriptor>(patchBundleCount);
				for (int i = 0; i < patchBundleCount; i++)
				{
					var patchBundle = new BundleDescriptor();
					patchBundle.bundleName = buffer.ReadUTF8();
					patchBundle.fileHash = buffer.ReadUTF8();
					patchBundle.fileCRC = buffer.ReadUTF8();
					patchBundle.fileSize = buffer.ReadInt64();
					patchBundle.isRawFile = buffer.ReadBool();
					patchBundle.loadMethod = buffer.ReadByte();
					manifest.BundleList.Add(patchBundle);
				}
			}

			// BundleList
			foreach (var patchBundle in manifest.BundleList)
			{
				patchBundle.ParseBundle("", manifest.OutputNameStyle);
				manifest.BundleDict.Add(patchBundle.bundleName, patchBundle);
			}

			// AssetList
			foreach (var patchAsset in manifest.AssetList)
			{
				// 注意：我们不允许原始路径存在重名
				string assetPath = patchAsset.assetPath;
				if (manifest.AssetDict.ContainsKey(assetPath))
					throw new Exception($"AssetPath have existed : {assetPath}");
				else
					manifest.AssetDict.Add(assetPath, patchAsset);
			}

			return manifest;
		}

		/// <summary>
		/// 生成Bundle文件的正式名称
		/// </summary>
		public static string CreateBundleFileName(int nameStype, string bundleName, string fileHash)
		{
			if ((EOutputNameStyle)nameStype == EOutputNameStyle.BundleName)
			{
                return bundleName;
            }
			else if ((EOutputNameStyle)nameStype == EOutputNameStyle.HashName_Extension)
			{
				string tempFileExtension = System.IO.Path.GetExtension(bundleName);
				return $"{fileHash}{tempFileExtension}";
			}
			else if ((EOutputNameStyle)nameStype == EOutputNameStyle.BundleName_HashName)
			{
				string tempFileExtension = System.IO.Path.GetExtension(bundleName);
				string tempBundleName = bundleName.Replace('/', '_').Replace(tempFileExtension, "");
				return $"{tempBundleName}_{fileHash}";
			}
			else if ((EOutputNameStyle)nameStype == EOutputNameStyle.BundleName_HashName_Extension)
			{
				string tempFileExtension = System.IO.Path.GetExtension(bundleName);
				string tempBundleName = bundleName.Replace('/', '_').Replace(tempFileExtension, "");
				return $"{tempBundleName}_{fileHash}{tempFileExtension}";
			}
			else
			{
				throw new NotImplementedException();
			}
		}
	}
}