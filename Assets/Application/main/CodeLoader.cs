using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ILRuntime.Runtime.Enviorment;
using System.Reflection;
using System.IO;

namespace Application.Runtime
{
	public enum CodeMode
	{
		Mono = 1,
		ILRuntime = 2
	}
	
    public class CodeLoader : System.IDisposable
    {
        public static CodeLoader Instance = new CodeLoader();

        public CodeMode     codeMode    { get; set; }
        private Assembly    m_Assembly;
        private AppDomain   m_AppDomain;

        private CodeLoader()
        {}

        public void Dispose()
        {
            m_AppDomain?.Dispose();
        }

        public void Start(string dllPath)
        {
            switch(codeMode)
            {
                case CodeMode.Mono:
                    break;
                case CodeMode.ILRuntime:
                {
                    m_AppDomain = new AppDomain();

                    byte[] assBytes = File.ReadAllBytes(dllPath);
					// byte[] pdbBytes = File.ReadAllBytes(Path.);
                    MemoryStream assStream = new MemoryStream(assBytes);
                    m_AppDomain.LoadAssembly(assStream, null, new ILRuntime.Mono.Cecil.Pdb.PdbReaderProvider());

                    // binder & register




                    IStaticMethod start = new ILStaticMethod(m_AppDomain, "", "Start", 0);
					start.Exec();
                }
                break;
            }
        }
    }
}