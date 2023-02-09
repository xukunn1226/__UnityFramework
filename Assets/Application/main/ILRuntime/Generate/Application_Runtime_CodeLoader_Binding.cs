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
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(Application.Runtime.CodeLoader);
            args = new Type[]{};
            method = type.GetMethod("GetTypes", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetTypes_0);

            field = type.GetField("Instance", flag);
            app.RegisterCLRFieldGetter(field, get_Instance_0);
            app.RegisterCLRFieldSetter(field, set_Instance_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_Instance_0, AssignFromStack_Instance_0);
            field = type.GetField("Update", flag);
            app.RegisterCLRFieldGetter(field, get_Update_1);
            app.RegisterCLRFieldSetter(field, set_Update_1);
            app.RegisterCLRFieldBinding(field, CopyToStack_Update_1, AssignFromStack_Update_1);
            field = type.GetField("OnApplicationQuit", flag);
            app.RegisterCLRFieldGetter(field, get_OnApplicationQuit_2);
            app.RegisterCLRFieldSetter(field, set_OnApplicationQuit_2);
            app.RegisterCLRFieldBinding(field, CopyToStack_OnApplicationQuit_2, AssignFromStack_OnApplicationQuit_2);
            field = type.GetField("OnApplicationFocus", flag);
            app.RegisterCLRFieldGetter(field, get_OnApplicationFocus_3);
            app.RegisterCLRFieldSetter(field, set_OnApplicationFocus_3);
            app.RegisterCLRFieldBinding(field, CopyToStack_OnApplicationFocus_3, AssignFromStack_OnApplicationFocus_3);
            field = type.GetField("OnDestroy", flag);
            app.RegisterCLRFieldGetter(field, get_OnDestroy_4);
            app.RegisterCLRFieldSetter(field, set_OnDestroy_4);
            app.RegisterCLRFieldBinding(field, CopyToStack_OnDestroy_4, AssignFromStack_OnDestroy_4);


        }


        static StackObject* GetTypes_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            Application.Runtime.CodeLoader instance_of_this_method = (Application.Runtime.CodeLoader)typeof(Application.Runtime.CodeLoader).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.GetTypes();

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
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

        static object get_OnApplicationQuit_2(ref object o)
        {
            return ((Application.Runtime.CodeLoader)o).OnApplicationQuit;
        }

        static StackObject* CopyToStack_OnApplicationQuit_2(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((Application.Runtime.CodeLoader)o).OnApplicationQuit;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_OnApplicationQuit_2(ref object o, object v)
        {
            ((Application.Runtime.CodeLoader)o).OnApplicationQuit = (System.Action)v;
        }

        static StackObject* AssignFromStack_OnApplicationQuit_2(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Action @OnApplicationQuit = (System.Action)typeof(System.Action).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            ((Application.Runtime.CodeLoader)o).OnApplicationQuit = @OnApplicationQuit;
            return ptr_of_this_method;
        }

        static object get_OnApplicationFocus_3(ref object o)
        {
            return ((Application.Runtime.CodeLoader)o).OnApplicationFocus;
        }

        static StackObject* CopyToStack_OnApplicationFocus_3(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((Application.Runtime.CodeLoader)o).OnApplicationFocus;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_OnApplicationFocus_3(ref object o, object v)
        {
            ((Application.Runtime.CodeLoader)o).OnApplicationFocus = (System.Action<System.Boolean>)v;
        }

        static StackObject* AssignFromStack_OnApplicationFocus_3(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Action<System.Boolean> @OnApplicationFocus = (System.Action<System.Boolean>)typeof(System.Action<System.Boolean>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            ((Application.Runtime.CodeLoader)o).OnApplicationFocus = @OnApplicationFocus;
            return ptr_of_this_method;
        }

        static object get_OnDestroy_4(ref object o)
        {
            return ((Application.Runtime.CodeLoader)o).OnDestroy;
        }

        static StackObject* CopyToStack_OnDestroy_4(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((Application.Runtime.CodeLoader)o).OnDestroy;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_OnDestroy_4(ref object o, object v)
        {
            ((Application.Runtime.CodeLoader)o).OnDestroy = (System.Action)v;
        }

        static StackObject* AssignFromStack_OnDestroy_4(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Action @OnDestroy = (System.Action)typeof(System.Action).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            ((Application.Runtime.CodeLoader)o).OnDestroy = @OnDestroy;
            return ptr_of_this_method;
        }



    }
}
