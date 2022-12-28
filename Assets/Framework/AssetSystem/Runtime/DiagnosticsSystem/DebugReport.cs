using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

namespace Framework.AssetManagement.Runtime
{
	[Serializable]
	internal class DebugReport
	{
		/// <summary>
		/// ��Ϸ֡
		/// </summary>
		public int FrameCount;

		/// <summary>
		/// ���Եİ��������б�
		/// </summary>
		public List<DebugProviderInfo> DebugProviderInfos = new List<DebugProviderInfo>(1000);


		/// <summary>
		/// ���л�
		/// </summary>
		public static byte[] Serialize(DebugReport debugReport)
		{
			return Encoding.UTF8.GetBytes(JsonUtility.ToJson(debugReport));
		}

		/// <summary>
		/// �����л�
		/// </summary>
		public static DebugReport Deserialize(byte[] data)
		{
			return JsonUtility.FromJson<DebugReport>(Encoding.UTF8.GetString(data));
		}
	}
}