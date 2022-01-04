using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.Runtime;
using Framework.Core;

namespace Application.Runtime
{
    public class ILStartup : MonoBehaviour
    {
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

		private void Start()
		{
			LoaderType type = Launcher.GetLauncherMode();
			string dllPath = UnityEngine.Application.streamingAssetsPath;
			switch(type)
			{
				case LoaderType.FromEditor:
				{
					dllPath = string.Format($"{UnityEngine.Application.dataPath}/../Library/ScriptAssemblies");
					break;
				}
				case LoaderType.FromStreamingAssets:
				{
					dllPath = string.Format($"{UnityEngine.Application.streamingAssetsPath}/{Utility.GetPlatformName()}");
					break;
				}
				case LoaderType.FromPersistent:
				{
					dllPath = string.Format($"{UnityEngine.Application.persistentDataPath}/{Utility.GetPlatformName()}");
					break;
				}
			}
			CodeLoader.Instance.Start(dllPath, "Application.Logic");
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