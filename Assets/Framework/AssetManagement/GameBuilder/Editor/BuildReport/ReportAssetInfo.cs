using System;
using System.Collections;
using System.Collections.Generic;

namespace Framework.AssetManagement.AssetEditorWindow
{
	[Serializable]
	public class ReportAssetInfo
	{
		/// <summary>
		/// 资源路径
		/// </summary>
		public string AssetPath;

		/// <summary>
		/// 资源GUID
		/// 说明：Meta文件记录的GUID
		/// </summary>
		public string AssetGUID;
	
		/// <summary>
		/// 所属资源包名称
		/// </summary>
		public string MainBundleName;

		/// <summary>
		/// 所属资源包的大小
		/// </summary>
		public long MainBundleSize;
		
		/// <summary>
		/// 依赖的资源包名称列表
		/// </summary>
		public List<string> DependBundles = new List<string>();

		/// <summary>
		/// 依赖的资源路径列表
		/// </summary>
		public List<string> DependAssets = new List<string>();
	}
}