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
    unsafe class Application_Runtime_ConfigBuilderSetting_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            FieldInfo field;
            Type[] args;
            Type type = typeof(Application.Runtime.ConfigBuilderSetting);

            field = type.GetField("DatabaseFilePath", flag);
            app.RegisterCLRFieldGetter(field, get_DatabaseFilePath_0);
            app.RegisterCLRFieldSetter(field, set_DatabaseFilePath_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_DatabaseFilePath_0, AssignFromStack_DatabaseFilePath_0);


        }



        static object get_DatabaseFilePath_0(ref object o)
        {
            return Application.Runtime.ConfigBuilderSetting.DatabaseFilePath;
        }

        static StackObject* CopyToStack_DatabaseFilePath_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = Application.Runtime.ConfigBuilderSetting.DatabaseFilePath;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_DatabaseFilePath_0(ref object o, object v)
        {
            Application.Runtime.ConfigBuilderSetting.DatabaseFilePath = (System.String)v;
        }

        static StackObject* AssignFromStack_DatabaseFilePath_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.String @DatabaseFilePath = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            Application.Runtime.ConfigBuilderSetting.DatabaseFilePath = @DatabaseFilePath;
            return ptr_of_this_method;
        }



    }
}
