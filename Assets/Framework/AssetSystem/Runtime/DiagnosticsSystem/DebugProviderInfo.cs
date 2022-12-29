using System.Collections;
using System.Collections.Generic;
using System;

namespace Framework.AssetManagement.Runtime
{
	[Serializable]
	public class DebugProviderInfo : IComparer<DebugProviderInfo>, IComparable<DebugProviderInfo>
	{
		/// <summary>
		/// ��Դ����·��
		/// </summary>
		public string AssetPath;

		/// <summary>
		/// ��Դ�����ĳ���
		/// </summary>
		public string SpawnScene;

		/// <summary>
		/// ��Դ������ʱ��
		/// </summary>
		public string SpawnTime;

		/// <summary>
		/// ���غ�ʱ����λ�����룩
		/// </summary>
		public long LoadingTime;

		/// <summary>
		/// ���ü���
		/// </summary>
		public int RefCount;

		/// <summary>
		/// ����״̬
		/// </summary>
		public string Status;

		/// <summary>
		/// ��������Դ���б�
		/// </summary>
		public List<DebugBundleInfo> DependBundleInfos;

		public int CompareTo(DebugProviderInfo other)
		{
			return Compare(this, other);
		}
		public int Compare(DebugProviderInfo a, DebugProviderInfo b)
		{
			return string.CompareOrdinal(a.AssetPath, b.AssetPath);
		}
	}
}