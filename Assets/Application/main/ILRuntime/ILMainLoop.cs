using System.Collections.Generic;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Stack;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.Utils;
using ILRuntime.CLR.TypeSystem;
using System.IO;

namespace Application.Runtime
{
    public class ILMainLoop
    {
        static private AppDomain    m_AppDomain;
        static private FileStream   m_DllStream;
        static private object       m_ILEnv;
        static private IMethod      m_TickMethod;

        static public void Init(string dllPath)
        {
            m_AppDomain = new ILRuntime.Runtime.Enviorment.AppDomain();

            m_DllStream = new FileStream(dllPath, FileMode.Open, FileAccess.Read);
            try
            {
                m_AppDomain.LoadAssembly(m_DllStream, null, new ILRuntime.Mono.Cecil.Pdb.PdbReaderProvider());
            }
            catch
            {
                UnityEngine.Debug.LogError("加载热更DLL失败");
            }

            InitializeILRuntime();
            InitILEnv();
        }

        static public void Uninit()
        {
            m_DllStream?.Close();
            m_DllStream = null;
        }

        static public void Tick()
        {
            if(m_TickMethod != null)
            {
                using(var ctx = m_AppDomain.BeginInvoke(m_TickMethod))
                {
                    ctx.PushObject(m_ILEnv);
                    ctx.PushFloat(UnityEngine.Time.deltaTime);
                    ctx.Invoke();
                }
            }
        }

        static private void InitILEnv()
        {
            m_ILEnv = m_AppDomain.Instantiate("Application.HotFix.ILEnv");
            IType type = m_AppDomain.LoadedTypes["Application.HotFix.ILEnv"];
            m_TickMethod = type.GetMethod("Tick", 1);
        }

        unsafe static private void InitializeILRuntime()
        {
#if DEBUG && (UNITY_EDITOR || UNITY_ANDROID || UNITY_IPHONE)
            //由于Unity的Profiler接口只允许在主线程使用，为了避免出异常，需要告诉ILRuntime主线程的线程ID才能正确将函数运行耗时报告给Profiler
            m_AppDomain.UnityMainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
#endif

            /////////////////// Step1. Register delegate
            // 注册仅需要注册不同的参数搭配即可
            m_AppDomain.DelegateManager.RegisterMethodDelegate<int>();
            m_AppDomain.DelegateManager.RegisterMethodDelegate<float>();
            m_AppDomain.DelegateManager.RegisterMethodDelegate<bool>();
            m_AppDomain.DelegateManager.RegisterMethodDelegate<string>();
            // //带返回值的委托的话需要用RegisterFunctionDelegate，返回类型为最后一个
            m_AppDomain.DelegateManager.RegisterFunctionDelegate<int, string>();


            /////////////////// Step2. Register delegate convertor
            //ILRuntime内部是用Action和Func这两个系统内置的委托类型来创建实例的，所以其他的委托类型都需要写转换器
            //将Action或者Func转换成目标委托类型
            // m_AppDomain.DelegateManager.RegisterDelegateConvertor<TestDelegateMethod>((action) =>
            // {
            //     //转换器的目的是把Action或者Func转换成正确的类型，这里则是把Action<int>转换成TestDelegateMethod
            //     return new TestDelegateMethod((a) =>
            //     {
            //         //调用委托实例
            //         ((System.Action<int>)action)(a);
            //     });
            // });
            //对于TestDelegateFunction同理，只是是将Func<int, string>转换成TestDelegateFunction
            // m_AppDomain.DelegateManager.RegisterDelegateConvertor<TestDelegateFunction>((action) =>
            // {
            //     return new TestDelegateFunction((a) =>
            //     {
            //         return ((System.Func<int, string>)action)(a);
            //     });
            // });

            //下面再举一个这个Demo中没有用到，但是UGUI经常遇到的一个委托，例如UnityAction<float>
            m_AppDomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction<float>>((action) =>
            {
                return new UnityEngine.Events.UnityAction<float>((a) =>
                {
                    ((System.Action<float>)action)(a);
                });
            });




            ////////////////////// Step3. Register CLR method redirection
            var mi = typeof(UnityEngine.Debug).GetMethod("Log", new System.Type[] { typeof(object) });
            m_AppDomain.RegisterCLRMethodRedirection(mi, Log_11);


            ////////////////////// Step4. Register adapter
            RegisterAdapter(m_AppDomain);


            //初始化CLR绑定请放在初始化的最后一步！！
            // ILRuntime.Runtime.Generated.CLRBindings.Initialize(m_AppDomain);
        }

        static public void RegisterAdapter(ILRuntime.Runtime.Enviorment.AppDomain domain)
        {
            //这里需要注册所有热更DLL中用到的跨域继承Adapter，否则无法正确抓取引用
            // domain.RegisterCrossBindingAdaptor(new MonoBehaviourAdapter());
            // domain.RegisterCrossBindingAdaptor(new CoroutineAdapter());
            // // domain.RegisterCrossBindingAdaptor(new TestClassBaseAdapter());
            // domain.RegisterValueTypeBinder(typeof(UnityEngine.Vector3), new Vector3Binder());
            // domain.RegisterValueTypeBinder(typeof(UnityEngine.Vector2), new Vector2Binder());
            // domain.RegisterValueTypeBinder(typeof(UnityEngine.Quaternion), new QuaternionBinder());
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
    }
}