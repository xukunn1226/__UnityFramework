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
    unsafe class Application_Runtime_CodeLoader_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            FieldInfo field;
            Type[] args;
            Type type = typeof(Application.Runtime.CodeLoader);

            field = type.GetField("Instance", flag);
            app.RegisterCLRFieldGetter(field, get_Instance_0);
            app.RegisterCLRFieldSetter(field, set_Instance_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_Instance_0, AssignFromStack_Instance_0);
            field = type.GetField("Update", flag);
            app.RegisterCLRFieldGetter(field, get_Update_1);
            app.RegisterCLRFieldSetter(field, set_Update_1);
            app.RegisterCLRFieldBinding(field, CopyToStack_Update_1, AssignFromStack_Update_1);


        }



        static object get_Instance_0(ref object o)
        {
            return Application.Runtime.CodeLoader.Instance;
        }

        static StackObject* CopyToStack_Instance_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = Application.Runtime.CodeLoader.Instance;
            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_Instance_0(ref object o, object v)
        {
            Application.Runtime.CodeLoader.Instance = (Application.Runtime.CodeLoader)v;
        }

        static StackObject* AssignFromStack_Instance_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            Application.Runtime.CodeLoader @Instance = (Application.Runtime.CodeLoader)typeof(Application.Runtime.CodeLoader).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            Application.Runtime.CodeLoader.Instance = @Instance;
            return ptr_of_this_method;
        }

        static object get_Update_1(ref object o)
        {
            return ((Application.Runtime.CodeLoader)o).Update;
        }

        static StackObject* CopyToStack_Update_1(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((Application.Runtime.CodeLoader)o).Update;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_Update_1(ref object o, object v)
        {
            ((Application.Runtime.CodeLoader)o).Update = (System.Action)v;
        }

        static StackObject* AssignFromStack_Update_1(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Action @Update = (System.Action)typeof(System.Action).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            ((Application.Runtime.CodeLoader)o).Update = @Update;
            return ptr_of_this_method;
        }



    }
}
