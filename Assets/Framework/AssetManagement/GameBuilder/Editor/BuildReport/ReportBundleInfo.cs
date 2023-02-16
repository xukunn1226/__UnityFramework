using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Framework.AssetManagement.Runtime;

namespace Framework.AssetManagement.AssetEditorWindow
{
	[Serializable]
	public class ReportBundleInfo
	{
		/// <summary>
		/// 资源包名称
		/// </summary>
		public string BundleName;

		/// <summary>
		/// 文件名称
		/// </summary>
		public string FileName;

		/// <summary>
		/// 文件哈希值
		/// </summary>
		public string FileHash;

		/// <summary>
		/// 文件校验码
		/// </summary>
		public string FileCRC;

		/// <summary>
		/// 文件大小（字节数）
		/// </summary>
		public long FileSize;

		/// <summary>
		/// 是否为原生文件
		/// </summary>
		public bool IsRawFile;

		/// <summary>
		/// 加载方法
		/// </summary>
		public EBundleLoadMethod LoadMethod;
	}
}