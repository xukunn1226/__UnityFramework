using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Framework.AssetManagement
{
    static public class EditorTools
    {
		/// <summary>
		/// ��ȡ�淶��·��
		/// </summary>
		public static string GetRegularPath(string path)
		{
			return path.Replace('\\', '/').Replace("\\", "/"); //�滻ΪLinux·����ʽ
		}

		/// <summary>
		/// ��ȡ��Ŀ����·��
		/// </summary>
		public static string GetProjectPath()
		{
			string projectPath = Path.GetDirectoryName(Application.dataPath);
			return GetRegularPath(projectPath);
		}

		/// <summary>
		/// ת���ļ��ľ���·��ΪUnity��Դ·��
		/// ���� D:\\YourPorject\\Assets\\Works\\file.txt �滻Ϊ Assets/Works/file.txt
		/// </summary>
		public static string AbsolutePathToAssetPath(string absolutePath)
		{
			string content = GetRegularPath(absolutePath);
			return Substring(content, "Assets/", true);
		}

		/// <summary>
		/// ת��Unity��Դ·��Ϊ�ļ��ľ���·��
		/// ���磺Assets/Works/file.txt �滻Ϊ D:\\YourPorject/Assets/Works/file.txt
		/// </summary>
		public static string AssetPathToAbsolutePath(string assetPath)
		{
			string projectPath = GetProjectPath();
			return $"{projectPath}/{assetPath}";
		}

		/// <summary>
		/// �ݹ����Ŀ���ļ���·��
		/// </summary>
		/// <param name="root">�����ĸ�Ŀ¼</param>
		/// <param name="folderName">Ŀ���ļ�������</param>
		/// <returns>�����ҵ����ļ���·�������û���ҵ����ؿ��ַ���</returns>
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
		/// ��ȡ�ַ���
		/// ��ȡƥ�䵽�ĺ�������
		/// </summary>
		/// <param name="content">����</param>
		/// <param name="key">�ؼ���</param>
		/// <param name="includeKey">�ָ�Ľ�����Ƿ�����ؼ���</param>
		/// <param name="searchBegin">�Ƿ�ʹ�ó�ʼƥ���λ�ã�����ʹ��ĩβƥ���λ��</param>
		private static string Substring(string content, string key, bool includeKey, bool firstMatch = true)
		{
			if (string.IsNullOrEmpty(key))
				return content;

			int startIndex = -1;
			if (firstMatch)
				startIndex = content.IndexOf(key); //�������ַ�����һ�γ���λ��		
			else
				startIndex = content.LastIndexOf(key); //�������ַ��������ֵ�λ��

			// ���û���ҵ�ƥ��Ĺؼ���
			if (startIndex == -1)
				return content;

			if (includeKey)
				return content.Substring(startIndex);
			else
				return content.Substring(startIndex + key.Length);
		}
	}
}