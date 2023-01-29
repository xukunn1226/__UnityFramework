using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
	[CreateAssetMenu(fileName = "AssetManagerSettings", menuName = "AssetManager Settings/Create AssetManagerSettings")]
	public class AssetManagerSettings : ScriptableObject
	{
		/// <summary>
		/// AssetBundle文件的后缀名
		/// </summary>
		public string AssetBundleFileVariant = "ab";

		/// <summary>
		/// 原生文件的后缀名
		/// </summary>
		public string RawFileVariant = "rawdata";

		/// <summary>
		/// 清单文件名称
		/// </summary>
		public string PatchManifestFileName = "AssetManifest";


		/// <summary>
		/// 清单文件头标记
		/// </summary>
		public const uint PatchManifestFileSign = 0x594F4F;

		/// <summary>
		/// 清单文件极限大小（100MB）
		/// </summary>
		public const int PatchManifestFileMaxSize = 104857600;

		/// <summary>
		/// 清单文件格式版本
		/// </summary>
		public const int ManifestSerializationVersion = 1;

		/// <summary>
		/// 构建输出文件夹名称
		/// </summary>
		public const string OutputFolderName = "OutputCache";

		/// <summary>
		/// 构建输出的报告文件
		/// </summary>
		public const string ReportFileName = "BuildReport";

		/// <summary>
		/// Unity着色器资源包名称
		/// </summary>
		public const string UnityShadersBundleName = "unityshaders";

		/// <summary>
		/// 内置资源目录名称
		/// </summary>
		public const string StreamingAssetsBuildinFolder = "BuildinFiles";

		/// <summary>
		/// 编辑器模拟环境下异步加载时的延迟帧数
		/// </summary>
		public const int DelayedFrameNumInEditorSimulateMode = 2;

		/// <summary>
		/// 忽略的文件类型
		/// </summary>
		public static readonly string[] IgnoreFileExtensions = { "", ".so", ".dll", ".cs", ".js", ".boo", ".meta", ".cginc", ".asmref" };

        /// <summary>
        /// 忽略的文件夹
        /// </summary>
        static public readonly string[] IgnoreDirectoryName = { "Temp", "Editor", "RawData", "Resources", "Examples" };
    }
}