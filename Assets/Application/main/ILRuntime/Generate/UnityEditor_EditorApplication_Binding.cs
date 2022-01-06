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
    unsafe class UnityEditor_EditorApplication_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(UnityEditor.EditorApplication);
            args = new Type[]{typeof(System.Action<UnityEditor.PlayModeStateChange>)};
            method = type.GetMethod("add_playModeStateChanged", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, add_playModeStateChanged_0);
            args = new Type[]{typeof(System.Action<UnityEditor.PlayModeStateChange>)};
            method = type.GetMethod("remove_playModeStateChanged", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, remove_playModeStateChanged_1);


        }


        static StackObject* add_playModeStateChanged_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Action<UnityEditor.PlayModeStateChange> @value = (System.Action<UnityEditor.PlayModeStateChange>)typeof(System.Action<UnityEditor.PlayModeStateChange>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);


            UnityEditor.EditorApplication.playModeStateChanged += value;

            return __ret;
        }

        static StackObject* remove_playModeStateChanged_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Action<UnityEditor.PlayModeStateChange> @value = (System.Action<UnityEditor.PlayModeStateChange>)typeof(System.Action<UnityEditor.PlayModeStateChange>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);


            UnityEditor.EditorApplication.playModeStateChanged -= value;

            return __ret;
        }



    }
}
