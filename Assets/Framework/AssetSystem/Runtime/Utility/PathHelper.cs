using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Framework.AssetManagement.Runtime
{
    internal static class PathHelper
    {
		private const string CacheFolderName = "CacheFiles";

		/// <summary>
		/// ��ȡ�淶����·��
		/// </summary>
		public static string GetRegularPath(string path)
		{
			return path.Replace('\\', '/').Replace("\\", "/"); //�滻ΪLinux·����ʽ
		}

		/// <summary>
		/// ��ȡ�ļ����ڵ�Ŀ¼·����Linux��ʽ��
		/// </summary>
		public static string GetDirectory(string filePath)
		{
			string directory = Path.GetDirectoryName(filePath);
			return GetRegularPath(directory);
		}

		/// <summary>
		/// ��ȡ�������ļ��еļ���·��
		/// </summary>
		public static string MakeStreamingLoadPath(string path)
		{
			return StringUtility.Format("{0}/{1}/{2}", UnityEngine.Application.streamingAssetsPath, AssetManagerSettings.StreamingAssetsBuildinFolder, path);
		}

		/// <summary>
		/// ��ȡ����ɳ���ļ��еļ���·��
		/// </summary>
		public static string MakePersistentLoadPath(string path)
		{
			string root = MakePersistentRootPath();
			return StringUtility.Format("{0}/{1}", root, path);
		}

		/// <summary>
		/// ��ȡɳ���ļ���·��
		/// </summary>
		public static string MakePersistentRootPath()
		{
#if UNITY_EDITOR
			// ע�⣺Ϊ�˷�����Բ鿴���༭���°Ѵ洢Ŀ¼�ŵ���Ŀ��
			string projectPath = GetDirectory(UnityEngine.Application.dataPath);
			return StringUtility.Format("{0}/Sandbox", projectPath);
#else
			return StringUtility.Format("{0}/Sandbox", UnityEngine.Application.persistentDataPath);
#endif
		}

		/// <summary>
		/// ��ȡWWW���ر�����Դ��·��
		/// </summary>
		public static string ConvertToWWWPath(string path)
		{
#if UNITY_EDITOR
			return StringUtility.Format("file:///{0}", path);
#elif UNITY_IPHONE
			return StringUtility.Format("file://{0}", path);
#elif UNITY_ANDROID
			return path;
#elif UNITY_STANDALONE
			return StringUtility.Format("file:///{0}", path);
#elif UNITY_WEBGL
			return path;
#endif
		}

		/// <summary>
		/// ɾ��ɳ����Ŀ¼
		/// </summary>
		public static void DeleteSandbox()
		{
			string directoryPath = PathHelper.MakePersistentLoadPath(string.Empty);
			if (Directory.Exists(directoryPath))
				Directory.Delete(directoryPath, true);
		}

		/// <summary>
		/// ɾ��ɳ���ڵĻ����ļ���
		/// </summary>
		public static void DeleteCacheFolder()
		{
			string root = PathHelper.MakePersistentLoadPath(CacheFolderName);
			if (Directory.Exists(root))
				Directory.Delete(root, true);
		}

        /// <summary>
        /// ��ȡ�����ļ���·��
        /// </summary>
        public static string GetCacheFolderPath(string packageName)
        {
            string root = PathHelper.MakePersistentLoadPath(CacheFolderName);
            return $"{root}/{packageName}";
        }

        #region ɳ�����嵥���
        /// <summary>
        /// ��ȡɳ�����嵥�ļ���·��
        /// </summary>
        public static string GetCacheManifestFilePath(string packageName)
        {
            string fileName = AssetManagerSettingsData.GetPatchManifestFileNameWithoutVersion(packageName);
            return PathHelper.MakePersistentLoadPath(fileName);
        }

        /// <summary>
        /// ����ɳ�����嵥�ļ�
        /// </summary>
        public static AssetManifest LoadCacheManifestFile(string packageName)
		{
			Debug.Log($"Load sandbox patch manifest file : {packageName}");
			string filePath = GetCacheManifestFilePath(packageName);
			byte[] bytesData = File.ReadAllBytes(filePath);
			return AssetManifest.DeserializeFromBinary(bytesData);
		}

		/// <summary>
		/// �洢ɳ�����嵥�ļ�
		/// </summary>
		public static AssetManifest SaveCacheManifestFile(string packageName, byte[] fileBytesData)
		{
			Debug.Log($"Save sandbox patch manifest file : {packageName}");
			var manifest = AssetManifest.DeserializeFromBinary(fileBytesData);
			string savePath = GetCacheManifestFilePath(packageName);
			FileUtility.CreateFile(savePath, fileBytesData);
			return manifest;
		}

		/// <summary>
		/// ���ɳ�����嵥�ļ��Ƿ����
		/// </summary>
		public static bool CheckCacheManifestFileExists(string packageName)
		{
			string filePath = GetCacheManifestFilePath(packageName);
			return File.Exists(filePath);
		}

		/// <summary>
		/// ɾ��ɳ�����嵥�ļ�
		/// </summary>
		public static bool DeleteCacheManifestFile(string packageName)
		{
			string filePath = GetCacheManifestFilePath(packageName);
			if (File.Exists(filePath))
			{
				UnityEngine.Debug.LogWarning($"Invalid cache manifest file have been removed : {filePath}");
				File.Delete(filePath);
				return true;
			}
			else
			{
				return false;
			}
		}
		#endregion
	}
}