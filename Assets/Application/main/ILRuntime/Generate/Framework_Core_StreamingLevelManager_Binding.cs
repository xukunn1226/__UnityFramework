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
    unsafe class Framework_Core_StreamingLevelManager_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(Framework.Core.StreamingLevelManager);
            args = new Type[]{typeof(Framework.Core.StreamingLevelManager.LevelContext)};
            method = type.GetMethod("LoadAsync", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, LoadAsync_0);

            field = type.GetField("onLevelLoadEnd", flag);
            app.RegisterCLRFieldGetter(field, get_onLevelLoadEnd_0);
            app.RegisterCLRFieldSetter(field, set_onLevelLoadEnd_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_onLevelLoadEnd_0, AssignFromStack_onLevelLoadEnd_0);


        }


        static StackObject* LoadAsync_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            Framework.Core.StreamingLevelManager.LevelContext @context = (Framework.Core.StreamingLevelManager.LevelContext)typeof(Framework.Core.StreamingLevelManager.LevelContext).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            Framework.Core.StreamingLevelManager instance_of_this_method = (Framework.Core.StreamingLevelManager)typeof(Framework.Core.StreamingLevelManager).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.LoadAsync(@context);

            return __ret;
        }


        static object get_onLevelLoadEnd_0(ref object o)
        {
            return Framework.Core.StreamingLevelManager.onLevelLoadEnd;
        }

        static StackObject* CopyToStack_onLevelLoadEnd_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = Framework.Core.StreamingLevelManager.onLevelLoadEnd;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_onLevelLoadEnd_0(ref object o, object v)
        {
            Framework.Core.StreamingLevelManager.onLevelLoadEnd = (System.Action<System.String>)v;
        }

        static StackObject* AssignFromStack_onLevelLoadEnd_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Action<System.String> @onLevelLoadEnd = (System.Action<System.String>)typeof(System.Action<System.String>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            Framework.Core.StreamingLevelManager.onLevelLoadEnd = @onLevelLoadEnd;
            return ptr_of_this_method;
        }



    }
}
