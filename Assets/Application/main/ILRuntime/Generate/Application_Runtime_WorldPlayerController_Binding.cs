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
    unsafe class Application_Runtime_WorldPlayerController_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            FieldInfo field;
            Type[] args;
            Type type = typeof(Application.Runtime.WorldPlayerController);

            field = type.GetField("onViewLayerUpdate", flag);
            app.RegisterCLRFieldGetter(field, get_onViewLayerUpdate_0);
            app.RegisterCLRFieldSetter(field, set_onViewLayerUpdate_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_onViewLayerUpdate_0, AssignFromStack_onViewLayerUpdate_0);


        }



        static object get_onViewLayerUpdate_0(ref object o)
        {
            return Application.Runtime.WorldPlayerController.onViewLayerUpdate;
        }

        static StackObject* CopyToStack_onViewLayerUpdate_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = Application.Runtime.WorldPlayerController.onViewLayerUpdate;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_onViewLayerUpdate_0(ref object o, object v)
        {
            Application.Runtime.WorldPlayerController.onViewLayerUpdate = (System.Action<Application.Runtime.ViewLayer, System.Single>)v;
        }

        static StackObject* AssignFromStack_onViewLayerUpdate_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Action<Application.Runtime.ViewLayer, System.Single> @onViewLayerUpdate = (System.Action<Application.Runtime.ViewLayer, System.Single>)typeof(System.Action<Application.Runtime.ViewLayer, System.Single>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            Application.Runtime.WorldPlayerController.onViewLayerUpdate = @onViewLayerUpdate;
            return ptr_of_this_method;
        }



    }
}
