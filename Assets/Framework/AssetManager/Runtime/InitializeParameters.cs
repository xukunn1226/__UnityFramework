using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    public class InitializeParameters
    {
		public EPlayMode PlayMode;

		/// <summary>
		/// 文件解密服务接口
		/// </summary>
		public IDecryptionServices DecryptionServices = null;

		public IBundleServices BundleServices = null;
		
		/// <summary>
		/// 资源定位地址大小写不敏感
		/// 注意：默认值为False
		/// </summary>
		public bool LocationToLower = false;

		/// <summary>
		/// 资源加载的最大数量
		/// 注意：默认值为MaxValue
		/// </summary>
		public int AssetLoadingMaxNumber = int.MaxValue;

		/// <summary>
		/// 默认的资源服务器下载地址
		/// </summary>
		public string DefaultHostServer = string.Empty;

		/// <summary>
		/// 备用的资源服务器下载地址
		/// </summary>
		public string FallbackHostServer = string.Empty;
	}
}