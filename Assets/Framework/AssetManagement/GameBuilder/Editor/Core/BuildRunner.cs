using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Framework.AssetManagement.AssetEditorWindow
{
	public class BuildRunner
	{
		public static bool EnableLog = true;

		/// <summary>
		/// 执行构建流程
		/// </summary>
		/// <returns>如果成功返回TRUE，否则返回FALSE</returns>
		public static GameBuildResult Run(List<IGameBuildTask> pipeline, BuildContext context)
		{
			if (pipeline == null)
				throw new ArgumentNullException("pipeline");
			if (context == null)
				throw new ArgumentNullException("context");

			GameBuildResult buildResult = new GameBuildResult();
			buildResult.Success = true;
			for (int i = 0; i < pipeline.Count; i++)
			{
                IGameBuildTask task = pipeline[i];
				try
				{
					var taskAttribute = task.GetType().GetCustomAttribute<TaskAttribute>();
					Log($"---------------------------------------->{taskAttribute.Desc}<---------------------------------------");
					task.Run(context);
				}
				catch (Exception e)
				{
					buildResult.FailedTask = task.GetType().Name;
					buildResult.FailedInfo = e.ToString();
					buildResult.Success = false;
					break;
				}
			}

			// 返回运行结果
			return buildResult;
		}

		/// <summary>
		/// 日志输出
		/// </summary>
		public static void Log(string info)
		{
			if (EnableLog)
			{
				UnityEngine.Debug.Log(info);
			}
		}

		/// <summary>
		/// 日志输出
		/// </summary>
		public static void Info(string info)
		{
			UnityEngine.Debug.Log(info);
		}
	}
}