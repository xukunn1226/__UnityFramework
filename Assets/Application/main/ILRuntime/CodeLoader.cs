using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ILRuntime.Runtime.Enviorment;
using System.Reflection;
using System.IO;
using System.Linq;

namespace Application.Runtime
{
	public enum CodeMode
	{
		Mono = 1,
		ILRuntime = 2,
        Mono2 = 3,
	}
	
    public class CodeLoader : System.IDisposable
    {
        public static CodeLoader    Instance = new CodeLoader();
        public System.Action        Update;
		public System.Action        OnApplicationQuit;
        public System.Action<bool>  OnApplicationFocus;
        public System.Action        OnDestroy;

        public CodeMode             codeMode    { get; set; }        
        private Assembly            m_Assembly;
        private AppDomain           m_AppDomain;
        private System.Type[]       allTypes;

        private CodeLoader()
        {}

        public void Dispose()
        {
            m_AppDomain?.Dispose();
        }

        public void Start(string dllPath, string dllFilename, string entryTypename, string entryMethodname)
        {
            switch(codeMode)
            {
                case CodeMode.Mono:
                {
                    byte[] assemblyBytes = File.ReadAllBytes(string.Format($"{dllPath}/{dllFilename}.dll"));
                    byte[] pdbBytes = File.ReadAllBytes(string.Format($"{dllPath}/{dllFilename}.pdb"));

                    m_Assembly = Assembly.Load(assemblyBytes, pdbBytes);
                    this.allTypes = m_Assembly.GetTypes();

                    IStaticMethod start = new MonoStaticMethod(m_Assembly, entryTypename, entryMethodname);
					start.Exec();
                    break;
                }
                case CodeMode.ILRuntime:
                {
                    m_AppDomain = new ILRuntime.Runtime.Enviorment.AppDomain(ILRuntime.Runtime.ILRuntimeJITFlags.None);

                    byte[] assemblyBytes = File.ReadAllBytes(string.Format($"{dllPath}/{dllFilename}.dll"));
                    byte[] pdbBytes = File.ReadAllBytes(string.Format($"{dllPath}/{dllFilename}.pdb"));
                    MemoryStream assemblyStream = new MemoryStream(assemblyBytes);
                    MemoryStream pdbStream = new MemoryStream(pdbBytes);

                    m_AppDomain.LoadAssembly(assemblyStream, pdbStream, new ILRuntime.Mono.Cecil.Pdb.PdbReaderProvider());
                    this.allTypes = m_AppDomain.LoadedTypes.Values.Select(x => x.ReflectionType).ToArray();

                    // binder & register
                    ILHelper.InitILRuntime(m_AppDomain);

                    IStaticMethod start = new ILStaticMethod(m_AppDomain, entryTypename, entryMethodname, 0);
					start.Exec();
                    break;
                }
                case CodeMode.Mono2:
                {
                    break;
                }
            }
        }

        public System.Type[] GetTypes()
		{
			return this.allTypes;
		}

        static public IStaticMethod GetStaticMethod(string typename, string methodname, int paramCount)
        {
            switch(CodeLoader.Instance.codeMode)
            {
                case CodeMode.Mono:
                {
                    return new MonoStaticMethod(CodeLoader.Instance.m_Assembly, typename, methodname);
                }
                case CodeMode.ILRuntime:
                {
                    return new ILStaticMethod(CodeLoader.Instance.m_AppDomain, typename, methodname, paramCount);
                }                
            }
            return null;
        }

        static public IMemberMethod GetMemberMethod(string typename, string methodname, int paramCount)
        {
            switch(CodeLoader.Instance.codeMode)
            {
                case CodeMode.Mono:
                {
                    return new MonoMemberMethod(CodeLoader.Instance.m_Assembly, typename, methodname);
                }
                case CodeMode.ILRuntime:
                {
                    return new ILMemberMethod(CodeLoader.Instance.m_AppDomain, typename, methodname, paramCount);
                }
            }
            return null;
        }
    }
}