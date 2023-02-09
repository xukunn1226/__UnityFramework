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
    unsafe class Framework_AssetManagement_Runtime_SceneOperationHandle_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(Framework.AssetManagement.Runtime.SceneOperationHandle);
            args = new Type[]{typeof(System.Action<Framework.AssetManagement.Runtime.SceneOperationHandle>)};
            method = type.GetMethod("add_Completed", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, add_Completed_0);


        }


        static StackObject* add_Completed_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Action<Framework.AssetManagement.Runtime.SceneOperationHandle> @value = (System.Action<Framework.AssetManagement.Runtime.SceneOperationHandle>)typeof(System.Action<Framework.AssetManagement.Runtime.SceneOperationHandle>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            Framework.AssetManagement.Runtime.SceneOperationHandle instance_of_this_method = (Framework.AssetManagement.Runtime.SceneOperationHandle)typeof(Framework.AssetManagement.Runtime.SceneOperationHandle).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.Completed += value;

            return __ret;
        }



    }
}
