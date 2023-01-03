using System.Collections;
using System.Collections.Generic;
using System;

namespace Framework.AssetManagement.Runtime
{
	[Serializable]
	public class DebugBundleInfo : IComparer<DebugBundleInfo>, IComparable<DebugBundleInfo>
	{
		/// <summary>
		/// 资源包名称
		/// </summary>
		public string BundleName;

		/// <summary>
		/// 引用计数
		/// </summary>
		public int RefCount;

		/// <summary>
		/// 加载状态
		/// </summary>
		public string Status;

		public int CompareTo(DebugBundleInfo other)
		{
			return Compare(this, other);
		}
		public int Compare(DebugBundleInfo a, DebugBundleInfo b)
		{
			return string.CompareOrdinal(a.BundleName, b.BundleName);
		}
	}
}