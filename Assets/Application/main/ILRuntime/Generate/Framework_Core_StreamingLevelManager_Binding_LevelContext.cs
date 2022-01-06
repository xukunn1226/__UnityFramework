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
    unsafe class Framework_Core_StreamingLevelManager_Binding_LevelContext_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(Framework.Core.StreamingLevelManager.LevelContext);

            field = type.GetField("sceneName", flag);
            app.RegisterCLRFieldGetter(field, get_sceneName_0);
            app.RegisterCLRFieldSetter(field, set_sceneName_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_sceneName_0, AssignFromStack_sceneName_0);
            field = type.GetField("scenePath", flag);
            app.RegisterCLRFieldGetter(field, get_scenePath_1);
            app.RegisterCLRFieldSetter(field, set_scenePath_1);
            app.RegisterCLRFieldBinding(field, CopyToStack_scenePath_1, AssignFromStack_scenePath_1);
            field = type.GetField("additive", flag);
            app.RegisterCLRFieldGetter(field, get_additive_2);
            app.RegisterCLRFieldSetter(field, set_additive_2);
            app.RegisterCLRFieldBinding(field, CopyToStack_additive_2, AssignFromStack_additive_2);
            field = type.GetField("bundlePath", flag);
            app.RegisterCLRFieldGetter(field, get_bundlePath_3);
            app.RegisterCLRFieldSetter(field, set_bundlePath_3);
            app.RegisterCLRFieldBinding(field, CopyToStack_bundlePath_3, AssignFromStack_bundlePath_3);

            args = new Type[]{};
            method = type.GetConstructor(flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Ctor_0);

        }



        static object get_sceneName_0(ref object o)
        {
            return ((Framework.Core.StreamingLevelManager.LevelContext)o).sceneName;
        }

        static StackObject* CopyToStack_sceneName_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((Framework.Core.StreamingLevelManager.LevelContext)o).sceneName;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_sceneName_0(ref object o, object v)
        {
            ((Framework.Core.StreamingLevelManager.LevelContext)o).sceneName = (System.String)v;
        }

        static StackObject* AssignFromStack_sceneName_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.String @sceneName = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            ((Framework.Core.StreamingLevelManager.LevelContext)o).sceneName = @sceneName;
            return ptr_of_this_method;
        }

        static object get_scenePath_1(ref object o)
        {
            return ((Framework.Core.StreamingLevelManager.LevelContext)o).scenePath;
        }

        static StackObject* CopyToStack_scenePath_1(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((Framework.Core.StreamingLevelManager.LevelContext)o).scenePath;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_scenePath_1(ref object o, object v)
        {
            ((Framework.Core.StreamingLevelManager.LevelContext)o).scenePath = (System.String)v;
        }

        static StackObject* AssignFromStack_scenePath_1(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.String @scenePath = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            ((Framework.Core.StreamingLevelManager.LevelContext)o).scenePath = @scenePath;
            return ptr_of_this_method;
        }

        static object get_additive_2(ref object o)
        {
            return ((Framework.Core.StreamingLevelManager.LevelContext)o).additive;
        }

        static StackObject* CopyToStack_additive_2(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((Framework.Core.StreamingLevelManager.LevelContext)o).additive;
            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method ? 1 : 0;
            return __ret + 1;
        }

        static void set_additive_2(ref object o, object v)
        {
            ((Framework.Core.StreamingLevelManager.LevelContext)o).additive = (System.Boolean)v;
        }

        static StackObject* AssignFromStack_additive_2(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Boolean @additive = ptr_of_this_method->Value == 1;
            ((Framework.Core.StreamingLevelManager.LevelContext)o).additive = @additive;
            return ptr_of_this_method;
        }

        static object get_bundlePath_3(ref object o)
        {
            return ((Framework.Core.StreamingLevelManager.LevelContext)o).bundlePath;
        }

        static StackObject* CopyToStack_bundlePath_3(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((Framework.Core.StreamingLevelManager.LevelContext)o).bundlePath;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_bundlePath_3(ref object o, object v)
        {
            ((Framework.Core.StreamingLevelManager.LevelContext)o).bundlePath = (System.String)v;
        }

        static StackObject* AssignFromStack_bundlePath_3(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.String @bundlePath = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            ((Framework.Core.StreamingLevelManager.LevelContext)o).bundlePath = @bundlePath;
            return ptr_of_this_method;
        }


        static StackObject* Ctor_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var result_of_this_method = new Framework.Core.StreamingLevelManager.LevelContext();

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }


    }
}
