using System;
using System.Collections;
using System.Collections.Generic;

namespace Framework.AssetManagement.AssetEditorWindow
{
	public class BuildContext
	{
		private readonly Dictionary<System.Type, IContextObject> m_ContextObjects = new Dictionary<System.Type, IContextObject>();

		/// <summary>
		/// 清空所有上下文
		/// </summary>
		public void ClearAllContext()
		{
			m_ContextObjects.Clear();
		}

		/// <summary>
		/// 设置上下文
		/// </summary>
		public void SetContextObject(IContextObject contextObject)
		{
			if (contextObject == null)
				throw new ArgumentNullException("contextObject");

			var type = contextObject.GetType();
			if (m_ContextObjects.ContainsKey(type))
				throw new Exception($"Context object {type} is already existed.");

			m_ContextObjects.Add(type, contextObject);
		}

		/// <summary>
		/// 获取上下文
		/// </summary>
		public T GetContextObject<T>() where T : IContextObject
		{
			var type = typeof(T);
			if (m_ContextObjects.TryGetValue(type, out IContextObject contextObject))
			{
				return (T)contextObject;
			}
			else
			{
				throw new Exception($"Not found context object : {type}");
			}
		}
	}
}