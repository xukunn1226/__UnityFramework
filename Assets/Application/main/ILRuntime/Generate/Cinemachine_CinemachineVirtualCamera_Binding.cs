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
    unsafe class Cinemachine_CinemachineVirtualCamera_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            FieldInfo field;
            Type[] args;
            Type type = typeof(Cinemachine.CinemachineVirtualCamera);

            field = type.GetField("m_Follow", flag);
            app.RegisterCLRFieldGetter(field, get_m_Follow_0);
            app.RegisterCLRFieldSetter(field, set_m_Follow_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_m_Follow_0, AssignFromStack_m_Follow_0);


        }



        static object get_m_Follow_0(ref object o)
        {
            return ((Cinemachine.CinemachineVirtualCamera)o).m_Follow;
        }

        static StackObject* CopyToStack_m_Follow_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((Cinemachine.CinemachineVirtualCamera)o).m_Follow;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_m_Follow_0(ref object o, object v)
        {
            ((Cinemachine.CinemachineVirtualCamera)o).m_Follow = (UnityEngine.Transform)v;
        }

        static StackObject* AssignFromStack_m_Follow_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            UnityEngine.Transform @m_Follow = (UnityEngine.Transform)typeof(UnityEngine.Transform).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            ((Cinemachine.CinemachineVirtualCamera)o).m_Follow = @m_Follow;
            return ptr_of_this_method;
        }



    }
}
