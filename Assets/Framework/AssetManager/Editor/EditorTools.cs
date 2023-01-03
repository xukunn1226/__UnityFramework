using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Framework.AssetManagement
{
    static public class EditorTools
    {
		/// <summary>
		/// 获取规范的路径
		/// </summary>
		public static string GetRegularPath(string path)
		{
			return path.Replace('\\', '/').Replace("\\", "/"); //替换为Linux路径格式
		}

		/// <summary>
		/// 获取项目工程路径
		/// </summary>
		public static string GetProjectPath()
		{
			string projectPath = Path.GetDirectoryName(Application.dataPath);
			return GetRegularPath(projectPath);
		}

		/// <summary>
		/// 转换文件的绝对路径为Unity资源路径
		/// 例如 D:\\YourPorject\\Assets\\Works\\file.txt 替换为 Assets/Works/file.txt
		/// </summary>
		public static string AbsolutePathToAssetPath(string absolutePath)
		{
			string content = GetRegularPath(absolutePath);
			return Substring(content, "Assets/", true);
		}

		/// <summary>
		/// 转换Unity资源路径为文件的绝对路径
		/// 例如：Assets/Works/file.txt 替换为 D:\\YourPorject/Assets/Works/file.txt
		/// </summary>
		public static string AssetPathToAbsolutePath(string assetPath)
		{
			string projectPath = GetProjectPath();
			return $"{projectPath}/{assetPath}";
		}

		/// <summary>
		/// 递归查找目标文件夹路径
		/// </summary>
		/// <param name="root">搜索的根目录</param>
		/// <param name="folderName">目标文件夹名称</param>
		/// <returns>返回找到的文件夹路径，如果没有找到返回空字符串</returns>
		public static string FindFolder(string root, string folderName)
		{
			DirectoryInfo rootInfo = new DirectoryInfo(root);
			DirectoryInfo[] infoList = rootInfo.GetDirectories();
			for (int i = 0; i < infoList.Length; i++)
			{
				string fullPath = infoList[i].FullName;
				if (infoList[i].Name == folderName)
					return fullPath;

				string result = FindFolder(fullPath, folderName);
				if (string.IsNullOrEmpty(result) == false)
					return result;
			}
			return string.Empty;
		}

		/// <summary>
		/// 截取字符串
		/// 获取匹配到的后面内容
		/// </summary>
		/// <param name="content">内容</param>
		/// <param name="key">关键字</param>
		/// <param name="includeKey">分割的结果里是否包含关键字</param>
		/// <param name="searchBegin">是否使用初始匹配的位置，否则使用末尾匹配的位置</param>
		private static string Substring(string content, string key, bool includeKey, bool firstMatch = true)
		{
			if (string.IsNullOrEmpty(key))
				return content;

			int startIndex = -1;
			if (firstMatch)
				startIndex = content.IndexOf(key); //返回子字符串第一次出现位置		
			else
				startIndex = content.LastIndexOf(key); //返回子字符串最后出现的位置

			// 如果没有找到匹配的关键字
			if (startIndex == -1)
				return content;

			if (includeKey)
				return content.Substring(startIndex);
			else
				return content.Substring(startIndex + key.Length);
		}
	}
}