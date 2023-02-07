using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.Runtime;
using Framework.Core;

namespace Application.Runtime
{
    public class ILStartup : MonoBehaviour
    {
		static public string dllFilename = "Application.Logic";		// Assets/Application/hotfix/Application.Logic.asmdef

		[SerializeField]
        private CodeMode 	m_CodeMode = CodeMode.Mono;
		public CodeMode 	codeMode
		{
			get { return m_CodeMode; }
			set { CodeLoader.Instance.codeMode = value; }
		}
		
		private void Awake()
		{
			System.AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
			{
				Debug.LogError(e.ExceptionObject.ToString());
			};
			
			codeMode = this.m_CodeMode;
		}

		private void OnDestroy()
		{
			CodeLoader.Instance.OnDestroy?.Invoke();
			CodeLoader.Instance.Dispose();
		}

		IEnumerator Start()
		{
			if(Launcher.GetPlayMode() == EPlayMode.FromStreaming)
            { // 仅FromStreamingAssets时需要提取，FromEditor从本地读取，FromPersistent会首次启动时提取
                yield return ILHelper.ExtractHotFixDLL();
				yield return ILHelper.ExtractHotFixPDB();
            }

			EPlayMode mode = Launcher.GetPlayMode();
			string dllPath = UnityEngine.Application.streamingAssetsPath;
			switch(mode)
			{
				case EPlayMode.FromEditor:
				{
					dllPath = string.Format($"{UnityEngine.Application.dataPath}/../Library/ScriptAssemblies");
					break;
				}
				case EPlayMode.FromStreaming:
				case EPlayMode.FromHost:
				{
					dllPath = string.Format($"{UnityEngine.Application.persistentDataPath}");
					break;
				}
			}
			CodeLoader.Instance.Start(dllPath, dllFilename, "Application.Logic.Entry", "Start");
		}

		private void Update()
		{
			CodeLoader.Instance.Update?.Invoke();
		}

		private void OnApplicationQuit()
		{
			CodeLoader.Instance.OnApplicationQuit?.Invoke();
			CodeLoader.Instance.Dispose();
		}

		private void OnApplicationFocus(bool isFocus)
		{

		}
    }
}