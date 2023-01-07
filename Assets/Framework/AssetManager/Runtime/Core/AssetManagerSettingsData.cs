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
		/// 加载配置文件
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
		/// 获取构建报告文件名
		/// </summary>
		public static string GetReportFileName(string packageName, string packageVersion)
		{
			return $"{AssetManagerSettings.ReportFileName}_{packageName}_{packageVersion}.json";
		}

		/// <summary>
		/// 获取补丁清单文件不带版本号的名称
		/// </summary>
		public static string GetPatchManifestFileNameWithoutVersion(string packageName)
		{
			return $"{Setting.PatchManifestFileName}_{packageName}.bytes";
		}

		/// <summary>
		/// 获取补丁清单文件完整名称
		/// </summary>
		public static string GetPatchManifestBinaryFileName(string packageName, string packageVersion)
		{
			return $"{Setting.PatchManifestFileName}_{packageName}_{packageVersion}.bytes";
		}

		/// <summary>
		/// 获取补丁清单文件完整名称
		/// </summary>
		public static string GetPatchManifestJsonFileName(string packageName, string packageVersion)
		{
			return $"{Setting.PatchManifestFileName}_{packageName}_{packageVersion}.json";
		}

		/// <summary>
		/// 获取补丁清单哈希文件完整名称
		/// </summary>
		public static string GetPatchManifestHashFileName(string packageName, string packageVersion)
		{
			return $"{Setting.PatchManifestFileName}_{packageName}_{packageVersion}.hash";
		}

		/// <summary>
		/// 获取补丁清单版本文件完整名称
		/// </summary>
		public static string GetPatchManifestVersionFileName(string packageName)
		{
			return $"{Setting.PatchManifestFileName}_{packageName}.version";
		}

		/// <summary>
		/// 获取着色器资源包全名称（包含后缀名）
		/// </summary>
		public static string GetUnityShadersBundleFullName()
		{
			return $"{AssetManagerSettings.UnityShadersBundleName}.{Setting.AssetBundleFileVariant}";
		}
	}
}