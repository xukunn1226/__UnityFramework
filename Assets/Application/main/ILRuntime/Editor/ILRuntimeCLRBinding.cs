using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace Application.Runtime
{
    public static class ILRuntimeCLRBinding
    {
        [MenuItem("Tools/ILRuntime/通过自动分析热更DLL生成CLR绑定")]
        public static void GenerateCLRBindingByAnalysis()
        {
            //用新的分析热更dll调用引用来生成绑定代码
            ILRuntime.Runtime.Enviorment.AppDomain domain = new ILRuntime.Runtime.Enviorment.AppDomain();
            using (FileStream fs = new FileStream($"Library/ScriptAssemblies/{ILStartup.dllFilename}.dll", FileMode.Open, FileAccess.Read))
            {
                domain.LoadAssembly(fs);
                
                ILHelper.RegisterAdaptor(domain);
                
                ILRuntime.Runtime.CLRBinding.BindingCodeGenerator.GenerateBindingCode(domain, "Assets/Application/main/ILRuntime/Generate");
            }

            AssetDatabase.Refresh();

            Debug.Log("生成CLR绑定文件完成");
        }

        [MenuItem("Tools/ILRuntime/生成跨域继承适配器")]
        private static void GenerateCrossbindAdapter()
        {
            //由于跨域继承特殊性太多，自动生成无法实现完全无副作用生成，所以这里提供的代码自动生成主要是给大家生成个初始模版，简化大家的工作
            //大多数情况直接使用自动生成的模版即可，如果遇到问题可以手动去修改生成后的文件，因此这里需要大家自行处理是否覆盖的问题
            // using (System.IO.StreamWriter sw = new System.IO.StreamWriter("Assets/Application/main/ILRuntime/Adaptor/MonoBehaviourAdapter.cs"))
            // {
            //     sw.WriteLine(ILRuntime.Runtime.Enviorment.CrossBindingCodeGenerator.GenerateCrossBindingAdapterCode(
            //         typeof (MonoBehaviour), "Application.Runtime"));
            // }

            // using (System.IO.StreamWriter sw = new System.IO.StreamWriter("Assets/Application/main/ILRuntime/Adaptor/IAsyncStateMachineAdapter.cs"))
            // {
            //     sw.WriteLine(ILRuntime.Runtime.Enviorment.CrossBindingCodeGenerator.GenerateCrossBindingAdapterCode(
            //         typeof (IAsyncStateMachine), "Application.Runtime"));
            // }

            // using (System.IO.StreamWriter sw = new System.IO.StreamWriter("Assets/Application/main/ILRuntime/Adaptor/IEnumerableAdapter.cs"))
            // {
            //     sw.WriteLine(ILRuntime.Runtime.Enviorment.CrossBindingCodeGenerator.GenerateCrossBindingAdapterCode(
            //         typeof (IEnumerable<object>), "Application.Runtime"));
            // }

            AssetDatabase.Refresh();

            Debug.Log("生成适配器完成");
        }        
    }
}