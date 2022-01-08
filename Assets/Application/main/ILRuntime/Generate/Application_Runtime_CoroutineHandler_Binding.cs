using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using ILRuntime.Reflection;
using ILRuntime.CLR.Utils;

namespace ILRuntime.Runtime.Generated
{
    unsafe class Application_Runtime_CoroutineHandler_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            FieldInfo field;
            Type[] args;
            Type type = typeof(Application.Runtime.CoroutineHandler);

            field = type.GetField("OnCompleted", flag);
            app.RegisterCLRFieldGetter(field, get_OnCompleted_0);
            app.RegisterCLRFieldSetter(field, set_OnCompleted_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_OnCompleted_0, AssignFromStack_OnCompleted_0);


        }



        static object get_OnCompleted_0(ref object o)
        {
            return ((Application.Runtime.CoroutineHandler)o).OnCompleted;
        }

        static StackObject* CopyToStack_OnCompleted_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((Application.Runtime.CoroutineHandler)o).OnCompleted;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_OnCompleted_0(ref object o, object v)
        {
            ((Application.Runtime.CoroutineHandler)o).OnCompleted = (Application.Runtime.CoroutineHandler.FinishedHandler)v;
        }

        static StackObject* AssignFromStack_OnCompleted_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            Application.Runtime.CoroutineHandler.FinishedHandler @OnCompleted = (Application.Runtime.CoroutineHandler.FinishedHandler)typeof(Application.Runtime.CoroutineHandler.FinishedHandler).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            ((Application.Runtime.CoroutineHandler)o).OnCompleted = @OnCompleted;
            return ptr_of_this_method;
        }



    }
}
