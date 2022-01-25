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
    unsafe class Framework_Cache_LRUQueue_2_String_RectTransform_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(Framework.Cache.LRUQueue<System.String, UnityEngine.RectTransform>);
            args = new Type[]{typeof(System.String)};
            method = type.GetMethod("Exist", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Exist_0);
            args = new Type[]{typeof(System.String), typeof(UnityEngine.RectTransform)};
            method = type.GetMethod("Cache", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Cache_1);

            field = type.GetField("OnDiscard", flag);
            app.RegisterCLRFieldGetter(field, get_OnDiscard_0);
            app.RegisterCLRFieldSetter(field, set_OnDiscard_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_OnDiscard_0, AssignFromStack_OnDiscard_0);

            args = new Type[]{typeof(System.Int32)};
            method = type.GetConstructor(flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Ctor_0);

        }


        static StackObject* Exist_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.String @key = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            Framework.Cache.LRUQueue<System.String, UnityEngine.RectTransform> instance_of_this_method = (Framework.Cache.LRUQueue<System.String, UnityEngine.RectTransform>)typeof(Framework.Cache.LRUQueue<System.String, UnityEngine.RectTransform>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.Exist(@key);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* Cache_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            UnityEngine.RectTransform @value = (UnityEngine.RectTransform)typeof(UnityEngine.RectTransform).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.String @key = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            Framework.Cache.LRUQueue<System.String, UnityEngine.RectTransform> instance_of_this_method = (Framework.Cache.LRUQueue<System.String, UnityEngine.RectTransform>)typeof(Framework.Cache.LRUQueue<System.String, UnityEngine.RectTransform>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.Cache(@key, @value);

            return __ret;
        }


        static object get_OnDiscard_0(ref object o)
        {
            return ((Framework.Cache.LRUQueue<System.String, UnityEngine.RectTransform>)o).OnDiscard;
        }

        static StackObject* CopyToStack_OnDiscard_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((Framework.Cache.LRUQueue<System.String, UnityEngine.RectTransform>)o).OnDiscard;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_OnDiscard_0(ref object o, object v)
        {
            ((Framework.Cache.LRUQueue<System.String, UnityEngine.RectTransform>)o).OnDiscard = (System.Action<System.String, UnityEngine.RectTransform>)v;
        }

        static StackObject* AssignFromStack_OnDiscard_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Action<System.String, UnityEngine.RectTransform> @OnDiscard = (System.Action<System.String, UnityEngine.RectTransform>)typeof(System.Action<System.String, UnityEngine.RectTransform>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            ((Framework.Cache.LRUQueue<System.String, UnityEngine.RectTransform>)o).OnDiscard = @OnDiscard;
            return ptr_of_this_method;
        }


        static StackObject* Ctor_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 @capacity = ptr_of_this_method->Value;


            var result_of_this_method = new Framework.Cache.LRUQueue<System.String, UnityEngine.RectTransform>(@capacity);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }


    }
}
