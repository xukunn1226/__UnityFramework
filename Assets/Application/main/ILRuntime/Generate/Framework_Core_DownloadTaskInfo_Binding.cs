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
    unsafe class Framework_Core_DownloadTaskInfo_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(Framework.Core.DownloadTaskInfo);

            field = type.GetField("srcUri", flag);
            app.RegisterCLRFieldGetter(field, get_srcUri_0);
            app.RegisterCLRFieldSetter(field, set_srcUri_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_srcUri_0, AssignFromStack_srcUri_0);
            field = type.GetField("dstURL", flag);
            app.RegisterCLRFieldGetter(field, get_dstURL_1);
            app.RegisterCLRFieldSetter(field, set_dstURL_1);
            app.RegisterCLRFieldBinding(field, CopyToStack_dstURL_1, AssignFromStack_dstURL_1);
            field = type.GetField("verifiedHash", flag);
            app.RegisterCLRFieldGetter(field, get_verifiedHash_2);
            app.RegisterCLRFieldSetter(field, set_verifiedHash_2);
            app.RegisterCLRFieldBinding(field, CopyToStack_verifiedHash_2, AssignFromStack_verifiedHash_2);
            field = type.GetField("retryCount", flag);
            app.RegisterCLRFieldGetter(field, get_retryCount_3);
            app.RegisterCLRFieldSetter(field, set_retryCount_3);
            app.RegisterCLRFieldBinding(field, CopyToStack_retryCount_3, AssignFromStack_retryCount_3);
            field = type.GetField("onCompleted", flag);
            app.RegisterCLRFieldGetter(field, get_onCompleted_4);
            app.RegisterCLRFieldSetter(field, set_onCompleted_4);
            app.RegisterCLRFieldBinding(field, CopyToStack_onCompleted_4, AssignFromStack_onCompleted_4);
            field = type.GetField("onRequestError", flag);
            app.RegisterCLRFieldGetter(field, get_onRequestError_5);
            app.RegisterCLRFieldSetter(field, set_onRequestError_5);
            app.RegisterCLRFieldBinding(field, CopyToStack_onRequestError_5, AssignFromStack_onRequestError_5);
            field = type.GetField("onDownloadError", flag);
            app.RegisterCLRFieldGetter(field, get_onDownloadError_6);
            app.RegisterCLRFieldSetter(field, set_onDownloadError_6);
            app.RegisterCLRFieldBinding(field, CopyToStack_onDownloadError_6, AssignFromStack_onDownloadError_6);

            args = new Type[]{};
            method = type.GetConstructor(flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Ctor_0);

        }



        static object get_srcUri_0(ref object o)
        {
            return ((Framework.Core.DownloadTaskInfo)o).srcUri;
        }

        static StackObject* CopyToStack_srcUri_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((Framework.Core.DownloadTaskInfo)o).srcUri;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_srcUri_0(ref object o, object v)
        {
            ((Framework.Core.DownloadTaskInfo)o).srcUri = (System.Uri)v;
        }

        static StackObject* AssignFromStack_srcUri_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Uri @srcUri = (System.Uri)typeof(System.Uri).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            ((Framework.Core.DownloadTaskInfo)o).srcUri = @srcUri;
            return ptr_of_this_method;
        }

        static object get_dstURL_1(ref object o)
        {
            return ((Framework.Core.DownloadTaskInfo)o).dstURL;
        }

        static StackObject* CopyToStack_dstURL_1(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((Framework.Core.DownloadTaskInfo)o).dstURL;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_dstURL_1(ref object o, object v)
        {
            ((Framework.Core.DownloadTaskInfo)o).dstURL = (System.String)v;
        }

        static StackObject* AssignFromStack_dstURL_1(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.String @dstURL = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            ((Framework.Core.DownloadTaskInfo)o).dstURL = @dstURL;
            return ptr_of_this_method;
        }

        static object get_verifiedHash_2(ref object o)
        {
            return ((Framework.Core.DownloadTaskInfo)o).verifiedHash;
        }

        static StackObject* CopyToStack_verifiedHash_2(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((Framework.Core.DownloadTaskInfo)o).verifiedHash;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_verifiedHash_2(ref object o, object v)
        {
            ((Framework.Core.DownloadTaskInfo)o).verifiedHash = (System.String)v;
        }

        static StackObject* AssignFromStack_verifiedHash_2(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.String @verifiedHash = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            ((Framework.Core.DownloadTaskInfo)o).verifiedHash = @verifiedHash;
            return ptr_of_this_method;
        }

        static object get_retryCount_3(ref object o)
        {
            return ((Framework.Core.DownloadTaskInfo)o).retryCount;
        }

        static StackObject* CopyToStack_retryCount_3(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((Framework.Core.DownloadTaskInfo)o).retryCount;
            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static void set_retryCount_3(ref object o, object v)
        {
            ((Framework.Core.DownloadTaskInfo)o).retryCount = (System.Int32)v;
        }

        static StackObject* AssignFromStack_retryCount_3(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Int32 @retryCount = ptr_of_this_method->Value;
            ((Framework.Core.DownloadTaskInfo)o).retryCount = @retryCount;
            return ptr_of_this_method;
        }

        static object get_onCompleted_4(ref object o)
        {
            return ((Framework.Core.DownloadTaskInfo)o).onCompleted;
        }

        static StackObject* CopyToStack_onCompleted_4(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((Framework.Core.DownloadTaskInfo)o).onCompleted;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_onCompleted_4(ref object o, object v)
        {
            ((Framework.Core.DownloadTaskInfo)o).onCompleted = (System.Action<Framework.Core.DownloadTaskInfo, System.Boolean, System.Int32>)v;
        }

        static StackObject* AssignFromStack_onCompleted_4(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Action<Framework.Core.DownloadTaskInfo, System.Boolean, System.Int32> @onCompleted = (System.Action<Framework.Core.DownloadTaskInfo, System.Boolean, System.Int32>)typeof(System.Action<Framework.Core.DownloadTaskInfo, System.Boolean, System.Int32>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            ((Framework.Core.DownloadTaskInfo)o).onCompleted = @onCompleted;
            return ptr_of_this_method;
        }

        static object get_onRequestError_5(ref object o)
        {
            return ((Framework.Core.DownloadTaskInfo)o).onRequestError;
        }

        static StackObject* CopyToStack_onRequestError_5(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((Framework.Core.DownloadTaskInfo)o).onRequestError;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_onRequestError_5(ref object o, object v)
        {
            ((Framework.Core.DownloadTaskInfo)o).onRequestError = (System.Action<Framework.Core.DownloadTaskInfo, System.String>)v;
        }

        static StackObject* AssignFromStack_onRequestError_5(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Action<Framework.Core.DownloadTaskInfo, System.String> @onRequestError = (System.Action<Framework.Core.DownloadTaskInfo, System.String>)typeof(System.Action<Framework.Core.DownloadTaskInfo, System.String>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            ((Framework.Core.DownloadTaskInfo)o).onRequestError = @onRequestError;
            return ptr_of_this_method;
        }

        static object get_onDownloadError_6(ref object o)
        {
            return ((Framework.Core.DownloadTaskInfo)o).onDownloadError;
        }

        static StackObject* CopyToStack_onDownloadError_6(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((Framework.Core.DownloadTaskInfo)o).onDownloadError;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_onDownloadError_6(ref object o, object v)
        {
            ((Framework.Core.DownloadTaskInfo)o).onDownloadError = (System.Action<Framework.Core.DownloadTaskInfo, System.String>)v;
        }

        static StackObject* AssignFromStack_onDownloadError_6(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Action<Framework.Core.DownloadTaskInfo, System.String> @onDownloadError = (System.Action<Framework.Core.DownloadTaskInfo, System.String>)typeof(System.Action<Framework.Core.DownloadTaskInfo, System.String>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            ((Framework.Core.DownloadTaskInfo)o).onDownloadError = @onDownloadError;
            return ptr_of_this_method;
        }


        static StackObject* Ctor_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var result_of_this_method = new Framework.Core.DownloadTaskInfo();

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }


    }
}
