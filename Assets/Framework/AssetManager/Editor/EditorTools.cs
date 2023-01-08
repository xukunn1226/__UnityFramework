using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Linq;

namespace Framework.AssetManagement.AssetEditorWindow
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
		/// 搜集资源
		/// </summary>
		/// <param name="searchType">搜集的资源类型</param>
		/// <param name="searchInFolders">指定搜索的文件夹列表</param>
		/// <returns>返回搜集到的资源路径列表</returns>
		public static string[] FindAssets(EAssetSearchType searchType, string[] searchInFolders)
        {
            // 注意：AssetDatabase.FindAssets()不支持末尾带分隔符的文件夹路径
            for (int i = 0; i < searchInFolders.Length; i++)
            {
                string folderPath = searchInFolders[i];
                searchInFolders[i] = folderPath.TrimEnd('/');
            }

            // 注意：获取指定目录下的所有资源对象（包括子文件夹）
            string[] guids;
            if (searchType == EAssetSearchType.All)
                guids = AssetDatabase.FindAssets(string.Empty, searchInFolders);
            else
                guids = AssetDatabase.FindAssets($"t:{searchType}", searchInFolders);

            // 注意：AssetDatabase.FindAssets()可能会获取到重复的资源
            List<string> result = new List<string>();
            for (int i = 0; i < guids.Length; i++)
            {
                string guid = guids[i];
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (result.Contains(assetPath) == false)
                {
                    result.Add(assetPath);
                }
            }

            // 返回结果
            return result.ToArray();
        }

        /// <summary>
        /// 搜集资源
        /// </summary>
        /// <param name="searchType">搜集的资源类型</param>
        /// <param name="searchInFolder">指定搜索的文件夹</param>
        /// <returns>返回搜集到的资源路径列表</returns>
        public static string[] FindAssets(EAssetSearchType searchType, string searchInFolder)
        {
            return FindAssets(searchType, new string[] { searchInFolder });
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

        /// <summary>
		/// 获取带继承关系的所有类的类型
		/// </summary>
		public static List<Type> GetAssignableTypes(System.Type parentType)
        {
            TypeCache.TypeCollection collection = TypeCache.GetTypesDerivedFrom(parentType);
            return collection.ToList();
        }
    }
}