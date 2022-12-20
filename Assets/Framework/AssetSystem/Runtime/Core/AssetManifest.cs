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
        /// ���л��汾��
        /// </summary>
        public int                      SerializedVersion;

        /// <summary>
        /// �汾��
        /// </summary>
        public string                   PackageVersion;

		public string					PackageName			{ get { return $"v{PackageVersion}"; } }
		
		/// <summary>
		/// �ļ�������ʽ
		/// </summary>
		public int						OutputNameStyle;

		/// <summary>
		/// ��Դ�б�
		/// </summary>
		public List<AssetDescriptor>    AssetList = new List<AssetDescriptor>();

        /// <summary>
        /// ��Դ���б�
        /// </summary>
        public List<BundleDescriptor>   BundleList = new List<BundleDescriptor>();

        /// <summary>
        /// ��Դ����
        /// KEY: assetPath��VALUE: AssetDescriptor
        /// </summary>
        [NonSerialized]
        public readonly Dictionary<string, AssetDescriptor>     AssetDict = new Dictionary<string, AssetDescriptor>();

        /// <summary>
        /// ��Դ������
        /// KEY: bundleName��VALUE: BundleDescriptor
        /// </summary>
        [NonSerialized]
        public readonly Dictionary<string, BundleDescriptor>    BundleDict = new Dictionary<string, BundleDescriptor>();
				
        public BundleDescriptor GetMainBundleDesc(string assetPath)
        {
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
            if(AssetDict.TryGetValue(assetPath, out AssetDescriptor descriptor))
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
        /// ���Ի�ȡ������Դ
        /// </summary>
        public bool TryGetAssetDesc(string assetPath, out AssetDescriptor result)
        {
            return AssetDict.TryGetValue(assetPath, out result);
        }

        /// <summary>
        /// ���Ի�ȡ������Դ��
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
		/// ���л����������ļ���
		/// </summary>
		public static void SerializeToBinary(string savePath, AssetManifest assetManifest)
		{
			using (FileStream fs = new FileStream(savePath, FileMode.Create))
			{
				// ����������
				BufferWriter buffer = new BufferWriter(AssetManagerSettings.PatchManifestFileMaxSize);

				// д���ļ����
				buffer.WriteUInt32(AssetManagerSettings.PatchManifestFileSign);

				// д���ļ��汾
				buffer.WriteInt32(assetManifest.SerializedVersion);

				// д���ļ�ͷ��Ϣ
				buffer.WriteInt32(assetManifest.OutputNameStyle);
				buffer.WriteUTF8(assetManifest.PackageVersion);

				// д����Դ�б�
				buffer.WriteInt32(assetManifest.AssetList.Count);
				for (int i = 0; i < assetManifest.AssetList.Count; i++)
				{
					var patchAsset = assetManifest.AssetList[i];
					buffer.WriteUTF8(patchAsset.assetPath);
					buffer.WriteInt32(patchAsset.bundleID);
					buffer.WriteInt32Array(patchAsset.dependIDs);
				}

				// д����Դ���б�
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

				// д���ļ���
				buffer.WriteToStream(fs);
				fs.Flush();
			}
		}

		/// <summary>
		/// �����л����������ļ���
		/// </summary>
		public static AssetManifest DeserializeFromBinary(byte[] binaryData)
		{
			// ����������
			BufferReader buffer = new BufferReader(binaryData);

			// ��ȡ�ļ����
			uint fileSign = buffer.ReadUInt32();
			if (fileSign != AssetManagerSettings.PatchManifestFileSign)
				throw new Exception("Invalid manifest file !");

			AssetManifest manifest = new AssetManifest();
			{
				// ��ȡ�ļ��汾
				manifest.SerializedVersion = buffer.ReadInt32();
				if (manifest.SerializedVersion != AssetManagerSettings.ManifestSerializationVersion)
					throw new Exception($"The manifest file version are not compatible : {manifest.SerializedVersion} != {AssetManagerSettings.ManifestSerializationVersion}");

				// ��ȡ�ļ�ͷ��Ϣ
				manifest.OutputNameStyle = buffer.ReadInt32();
				manifest.PackageVersion = buffer.ReadUTF8();

				// ��ȡ��Դ�б�
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

				// ��ȡ��Դ���б�
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
				// ע�⣺���ǲ�����ԭʼ·����������
				string assetPath = patchAsset.assetPath;
				if (manifest.AssetDict.ContainsKey(assetPath))
					throw new Exception($"AssetPath have existed : {assetPath}");
				else
					manifest.AssetDict.Add(assetPath, patchAsset);
			}

			return manifest;
		}

		/// <summary>
		/// ����Bundle�ļ�����ʽ����
		/// </summary>
		public static string CreateBundleFileName(int nameStype, string bundleName, string fileHash)
		{
			if (nameStype == 1)
			{
				return fileHash;
			}
			else if (nameStype == 2)
			{
				string tempFileExtension = System.IO.Path.GetExtension(bundleName);
				return $"{fileHash}{tempFileExtension}";
			}
			else if (nameStype == 3)
			{
				string tempFileExtension = System.IO.Path.GetExtension(bundleName);
				string tempBundleName = bundleName.Replace('/', '_').Replace(tempFileExtension, "");
				return $"{tempBundleName}_{fileHash}";
			}
			else if (nameStype == 4)
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