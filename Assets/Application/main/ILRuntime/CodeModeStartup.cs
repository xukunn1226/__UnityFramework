using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Runtime
{
    public class CodeModeStartup : MonoBehaviour
    {
        public CodeMode CodeMode = CodeMode.Mono;
		
		private void Awake()
		{
			System.AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
			{
				Debug.LogError(e.ExceptionObject.ToString());
			};
						
			DontDestroyOnLoad(gameObject);

			CodeLoader.Instance.codeMode = this.CodeMode;
		}

		private void Start()
		{
			CodeLoader.Instance.Start(string.Format($"{UnityEngine.Application.dataPath}/../Library/ScriptAssemblies"), "Application.Logic");
		}

		private void Update()
		{
			CodeLoader.Instance.Update();
		}

		private void OnApplicationQuit()
		{
			CodeLoader.Instance.OnApplicationQuit();
			CodeLoader.Instance.Dispose();
		}
    }
}