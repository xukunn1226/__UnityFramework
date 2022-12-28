using System.Collections;
using System.Collections.Generic;
using System;

namespace Framework.AssetManagement.Runtime
{
	[Serializable]
	internal class DebugBundleInfo : IComparer<DebugBundleInfo>, IComparable<DebugBundleInfo>
	{
		/// <summary>
		/// ��Դ������
		/// </summary>
		public string BundleName;

		/// <summary>
		/// ���ü���
		/// </summary>
		public int RefCount;

		/// <summary>
		/// ����״̬
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