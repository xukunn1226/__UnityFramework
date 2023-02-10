using System;

namespace Framework.AssetManagement.AssetEditorWindow
{
	[AttributeUsage(AttributeTargets.Class)]
	public class TaskAttribute : Attribute
	{
		public string Desc;
		public TaskAttribute(string desc)
		{
			Desc = desc;
		}
	}
}