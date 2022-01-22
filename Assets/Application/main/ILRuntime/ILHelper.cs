using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ILRuntime.Runtime.Stack;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.Utils;
using Framework.Core;
using System;
using System.IO;
using System.Net;

namespace Application.Runtime
{
    public class ILHelper
    {
        unsafe public static void InitILRuntime(ILRuntime.Runtime.Enviorment.AppDomain appdomain)
        {
#if DEBUG && (UNITY_EDITOR || UNITY_ANDROID || UNITY_IPHONE)
            //由于Unity的Profiler接口只允许在主线程使用，为了避免出异常，需要告诉ILRuntime主线程的线程ID才能正确将函数运行耗时报告给Profiler
            appdomain.UnityMainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
            appdomain.DebugService.StartDebugService(56000);
#endif

            // 注册委托
            appdomain.DelegateManager.RegisterMethodDelegate<List<object>>();
            appdomain.DelegateManager.RegisterMethodDelegate<object>();
            appdomain.DelegateManager.RegisterMethodDelegate<bool>();
            appdomain.DelegateManager.RegisterMethodDelegate<string>();
            appdomain.DelegateManager.RegisterMethodDelegate<float>();
            appdomain.DelegateManager.RegisterMethodDelegate<long, int>();
            appdomain.DelegateManager.RegisterMethodDelegate<long, MemoryStream>();
            appdomain.DelegateManager.RegisterMethodDelegate<long, IPEndPoint>();
            appdomain.DelegateManager.RegisterMethodDelegate<ILTypeInstance>();
            appdomain.DelegateManager.RegisterMethodDelegate<AsyncOperation>();
            appdomain.DelegateManager.RegisterMethodDelegate<Application.Runtime.ViewLayer, float>();
            appdomain.DelegateManager.RegisterMethodDelegate<Framework.Core.DownloadTaskInfo, System.Boolean, System.Int32>();
            appdomain.DelegateManager.RegisterMethodDelegate<Framework.Core.DownloadTaskInfo, System.String>();            




            
            
            appdomain.DelegateManager.RegisterFunctionDelegate<UnityEngine.Events.UnityAction>();
            appdomain.DelegateManager.RegisterFunctionDelegate<ILTypeInstance, bool>();
            appdomain.DelegateManager.RegisterFunctionDelegate<System.Collections.Generic.KeyValuePair<System.String, System.Int32>, System.String>();
            appdomain.DelegateManager.RegisterFunctionDelegate<System.Collections.Generic.KeyValuePair<System.Int32, System.Int32>, System.Boolean>();
            appdomain.DelegateManager.RegisterFunctionDelegate<System.Collections.Generic.KeyValuePair<System.String, System.Int32>, System.Int32>();
            appdomain.DelegateManager.RegisterFunctionDelegate<List<int>, int>();
            appdomain.DelegateManager.RegisterFunctionDelegate<List<int>, bool>();
            appdomain.DelegateManager.RegisterFunctionDelegate<int, bool>();//Linq
            appdomain.DelegateManager.RegisterFunctionDelegate<int, int, int>();//Linq
            appdomain.DelegateManager.RegisterFunctionDelegate<KeyValuePair<int, List<int>>, bool>();
            appdomain.DelegateManager.RegisterFunctionDelegate<KeyValuePair<int, int>, KeyValuePair<int, int>, int>();
            appdomain.DelegateManager.RegisterFunctionDelegate<System.Type, System.Boolean>();
            



            appdomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction>((act) =>
            {
                return new UnityEngine.Events.UnityAction(() =>
                {
                    ((Action)act)();
                });
            });            
            appdomain.DelegateManager.RegisterDelegateConvertor<Comparison<KeyValuePair<int, int>>>((act) =>
            {
                return new Comparison<KeyValuePair<int, int>>((x, y) =>
                {
                    return ((Func<KeyValuePair<int, int>, KeyValuePair<int, int>, int>)act)(x, y);
                });
            });
            appdomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction<System.Boolean>>((act) =>
            {
                return new UnityEngine.Events.UnityAction<System.Boolean>((arg0) =>
                {
                    ((Action<System.Boolean>)act)(arg0);
                });
            });





            // 注册适配器
            RegisterAdaptor(appdomain);





            // 注册重定向函数
            LitJson.JsonMapper.RegisterILRuntimeCLRRedirection(appdomain);
            var mi = typeof(UnityEngine.Debug).GetMethod("Log", new System.Type[] { typeof(object) });
            appdomain.RegisterCLRMethodRedirection(mi, Log_11);

            //注册ProtoBuf的CLR
            // PType.RegisterILRuntimeCLRRedirection(appdomain);
           
            
            ////////////////////////////////////
            // CLR绑定的注册，一定要记得将CLR绑定的注册写在CLR重定向的注册后面，因为同一个方法只能被重定向一次，只有先注册的那个才能生效
            ////////////////////////////////////
            Type t = Type.GetType("ILRuntime.Runtime.Generated.CLRBindings");
            if (t != null)
            {
                t.GetMethod("Initialize")?.Invoke(null, new object[] { appdomain });
            }
            //ILRuntime.Runtime.Generated.CLRBindings.Initialize(appdomain);
        }
        
        public static void RegisterAdaptor(ILRuntime.Runtime.Enviorment.AppDomain appdomain)
        {
            // 这里需要注册所有热更DLL中用到的跨域继承Adapter，否则无法正确抓取引用
            appdomain.RegisterCrossBindingAdaptor(new IAsyncStateMachineAdapter());
            appdomain.RegisterCrossBindingAdaptor(new MonoBehaviourAdapter());
            appdomain.RegisterCrossBindingAdaptor(new CoroutineAdaptor());
            appdomain.RegisterCrossBindingAdaptor(new IEnumerable_1_ObjectAdapter());
            appdomain.RegisterValueTypeBinder(typeof(UnityEngine.Vector3), new Vector3Binder());
            appdomain.RegisterValueTypeBinder(typeof(UnityEngine.Vector2), new Vector2Binder());
            appdomain.RegisterValueTypeBinder(typeof(UnityEngine.Quaternion), new QuaternionBinder());
        }

        //编写重定向方法对于刚接触ILRuntime的朋友可能比较困难，比较简单的方式是通过CLR绑定生成绑定代码，然后在这个基础上改，比如下面这个代码是从UnityEngine_Debug_Binding里面复制来改的
        //如何使用CLR绑定请看相关教程和文档
        unsafe static StackObject* Log_11(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            //ILRuntime的调用约定为被调用者清理堆栈，因此执行这个函数后需要将参数从堆栈清理干净，并把返回值放在栈顶，具体请看ILRuntime实现原理文档
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            //这个是最后方法返回后esp栈指针的值，应该返回清理完参数并指向返回值，这里是只需要返回清理完参数的值即可
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            //取Log方法的参数，如果有两个参数的话，第一个参数是esp - 2,第二个参数是esp -1, 因为Mono的bug，直接-2值会错误，所以要调用ILIntepreter.Minus
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);

            //这里是将栈指针上的值转换成object，如果是基础类型可直接通过ptr->Value和ptr->ValueLow访问到值，具体请看ILRuntime实现原理文档
            object message = typeof(object).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            //所有非基础类型都得调用Free来释放托管堆栈
            __intp.Free(ptr_of_this_method);

            //在真实调用Debug.Log前，我们先获取DLL内的堆栈
            var stacktrace = __domain.DebugService.GetStackTrace(__intp);

            //我们在输出信息后面加上DLL堆栈
            UnityEngine.Debug.Log(message + "\n" + stacktrace);

            return __ret;
        }

        static public IEnumerator ExtractHotFixDLL()
        {
            string srcDLLPath = string.Format($"{UnityEngine.Application.streamingAssetsPath}/{Utility.GetPlatformName()}/{Path.GetFileName(ILStartup.dllFilename)}.dll");
            string dstDLLPath = string.Format($"{UnityEngine.Application.persistentDataPath}/{Utility.GetPlatformName()}/{Path.GetFileName(ILStartup.dllFilename)}.dll");

            DownloadTask task       = new DownloadTask(new byte[1024]);
            DownloadTaskInfo info   = new DownloadTaskInfo();
            info.srcUri             = new System.Uri(srcDLLPath);
            info.dstURL             = dstDLLPath;
            info.verifiedHash       = null;
            info.retryCount         = 3;
            yield return task.Run(info);
        }

        static public IEnumerator ExtractHotFixPDB()
        {
            string srcPDBPath = string.Format($"{UnityEngine.Application.streamingAssetsPath}/{Utility.GetPlatformName()}/{Path.GetFileName(ILStartup.dllFilename)}.pdb");
            string dstPDBPath = string.Format($"{UnityEngine.Application.persistentDataPath}/{Utility.GetPlatformName()}/{Path.GetFileName(ILStartup.dllFilename)}.pdb");

            DownloadTask task       = new DownloadTask(new byte[1024]);
            DownloadTaskInfo info   = new DownloadTaskInfo();
            info.srcUri             = new System.Uri(srcPDBPath);
            info.dstURL             = dstPDBPath;
            info.verifiedHash       = null;
            info.retryCount         = 3;
            yield return task.Run(info);
        }
    }
}