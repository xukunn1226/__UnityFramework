using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    static public class AssetManagerSettingsData
    {
		private static AssetManagerSettings _setting = null;
		public static AssetManagerSettings Setting
		{
			get
			{
				if (_setting == null)
					LoadSettingData();
				return _setting;
			}
		}

		/// <summary>
		/// ���������ļ�
		/// </summary>
		private static void LoadSettingData()
		{
			_setting = Resources.Load<AssetManagerSettings>("AssetManagerSettings");
			if (_setting == null)
			{
				Debug.Log("YooAsset use default settings.");
				_setting = ScriptableObject.CreateInstance<AssetManagerSettings>();
			}
			else
			{
				Debug.Log("YooAsset use user settings.");
			}
		}

		/// <summary>
		/// ��ȡ���������ļ���
		/// </summary>
		public static string GetReportFileName(string packageName, string packageVersion)
		{
			return $"{AssetManagerSettings.ReportFileName}_{packageName}_{packageVersion}.json";
		}

		/// <summary>
		/// ��ȡ�����嵥�ļ������汾�ŵ�����
		/// </summary>
		public static string GetPatchManifestFileNameWithoutVersion(string packageName)
		{
			return $"{Setting.PatchManifestFileName}_{packageName}.bytes";
		}

		/// <summary>
		/// ��ȡ�����嵥�ļ���������
		/// </summary>
		public static string GetPatchManifestBinaryFileName(string packageName, string packageVersion)
		{
			return $"{Setting.PatchManifestFileName}_{packageName}_{packageVersion}.bytes";
		}

		/// <summary>
		/// ��ȡ�����嵥�ļ���������
		/// </summary>
		public static string GetPatchManifestJsonFileName(string packageName, string packageVersion)
		{
			return $"{Setting.PatchManifestFileName}_{packageName}_{packageVersion}.json";
		}

		/// <summary>
		/// ��ȡ�����嵥��ϣ�ļ���������
		/// </summary>
		public static string GetPatchManifestHashFileName(string packageName, string packageVersion)
		{
			return $"{Setting.PatchManifestFileName}_{packageName}_{packageVersion}.hash";
		}

		/// <summary>
		/// ��ȡ�����嵥�汾�ļ���������
		/// </summary>
		public static string GetPatchManifestVersionFileName(string packageName)
		{
			return $"{Setting.PatchManifestFileName}_{packageName}.version";
		}

		/// <summary>
		/// ��ȡ��ɫ����Դ��ȫ���ƣ�������׺����
		/// </summary>
		public static string GetUnityShadersBundleFullName()
		{
			return $"{AssetManagerSettings.UnityShadersBundleName}.{Setting.AssetBundleFileVariant}";
		}
	}
}