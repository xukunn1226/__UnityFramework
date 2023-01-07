using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

namespace Framework.AssetManagement.Runtime
{
	[Serializable]
	public class DebugReport
	{
		/// <summary>
		/// 游戏帧
		/// </summary>
		public int FrameCount;

		/// <summary>
		/// 调试的包裹数据列表
		/// </summary>
		public List<DebugProviderInfo> DebugProviderInfos = new List<DebugProviderInfo>(1000);


		/// <summary>
		/// 序列化
		/// </summary>
		public static byte[] Serialize(DebugReport debugReport)
		{
			return Encoding.UTF8.GetBytes(JsonUtility.ToJson(debugReport));
		}

		/// <summary>
		/// 反序列化
		/// </summary>
		public static DebugReport Deserialize(byte[] data)
		{
			return JsonUtility.FromJson<DebugReport>(Encoding.UTF8.GetString(data));
		}
	}
}