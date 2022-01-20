using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.Runtime;
using System.IO;
using System.Reflection;
using ILRuntime.Runtime.Enviorment;

namespace Application.Runtime.Tests
{
    public class TestUIManager : MonoBehaviour
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
			if(Launcher.GetLauncherMode() == Framework.AssetManagement.Runtime.LoaderType.FromStreamingAssets)
            { // 仅FromStreamingAssets时需要提取，FromEditor从本地读取，FromPersistent会首次启动时提取
                yield return ILHelper.ExtractHotFixDLL();
				yield return ILHelper.ExtractHotFixPDB();
            }

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
				case LoaderType.FromPersistent:
				{
					dllPath = string.Format($"{UnityEngine.Application.persistentDataPath}/{Framework.Core.Utility.GetPlatformName()}");
					break;
				}
			}
			StartLogic(dllPath, dllFilename);
		}

        public void StartLogic(string dllPath, string dllFilename)
        {
            switch(codeMode)
            {
                case CodeMode.Mono:
                {
                    byte[] assemblyBytes = File.ReadAllBytes(string.Format($"{dllPath}/{dllFilename}.dll"));
                    byte[] pdbBytes = File.ReadAllBytes(string.Format($"{dllPath}/{dllFilename}.pdb"));

                    Assembly m_Assembly = Assembly.Load(assemblyBytes, pdbBytes);

                    IStaticMethod start = new MonoStaticMethod(m_Assembly, "Application.Logic.UIManager", "StaticInit");
					start.Exec();
                    break;
                }
                case CodeMode.ILRuntime:
                {
                    AppDomain m_AppDomain = new ILRuntime.Runtime.Enviorment.AppDomain(ILRuntime.Runtime.ILRuntimeJITFlags.None);

                    byte[] assemblyBytes = File.ReadAllBytes(string.Format($"{dllPath}/{dllFilename}.dll"));
                    byte[] pdbBytes = File.ReadAllBytes(string.Format($"{dllPath}/{dllFilename}.pdb"));
                    MemoryStream assemblyStream = new MemoryStream(assemblyBytes);
                    MemoryStream pdbStream = new MemoryStream(pdbBytes);

                    m_AppDomain.LoadAssembly(assemblyStream, pdbStream, new ILRuntime.Mono.Cecil.Pdb.PdbReaderProvider());

                    // binder & register
                    ILHelper.InitILRuntime(m_AppDomain);

                    IStaticMethod start = new ILStaticMethod(m_AppDomain, "Application.Logic.UIManager", "Init", 0);
					start.Exec();
                    break;
                }
            }
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