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
    unsafe class StarterAssets_MyPlayerBehaviour_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(StarterAssets.MyPlayerBehaviour);
            args = new Type[]{typeof(System.Boolean)};
            method = type.GetMethod("SetGrounded", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, SetGrounded_0);
            args = new Type[]{};
            method = type.GetMethod("get_velocity", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_velocity_1);
            args = new Type[]{typeof(UnityEngine.Vector3)};
            method = type.GetMethod("SetMotion", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, SetMotion_2);
            args = new Type[]{typeof(System.Single)};
            method = type.GetMethod("SetSpeed", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, SetSpeed_3);
            args = new Type[]{typeof(System.Single)};
            method = type.GetMethod("SetMotionSpeed", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, SetMotionSpeed_4);
            args = new Type[]{typeof(System.Boolean)};
            method = type.GetMethod("SetJump", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, SetJump_5);
            args = new Type[]{typeof(System.Boolean)};
            method = type.GetMethod("SetFreeFall", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, SetFreeFall_6);


        }


        static StackObject* SetGrounded_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Boolean @grounded = ptr_of_this_method->Value == 1;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            StarterAssets.MyPlayerBehaviour instance_of_this_method = (StarterAssets.MyPlayerBehaviour)typeof(StarterAssets.MyPlayerBehaviour).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.SetGrounded(@grounded);

            return __ret;
        }

        static StackObject* get_velocity_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            StarterAssets.MyPlayerBehaviour instance_of_this_method = (StarterAssets.MyPlayerBehaviour)typeof(StarterAssets.MyPlayerBehaviour).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.velocity;

            if (ILRuntime.Runtime.Generated.CLRBindings.s_UnityEngine_Vector3_Binding_Binder != null) {
                ILRuntime.Runtime.Generated.CLRBindings.s_UnityEngine_Vector3_Binding_Binder.PushValue(ref result_of_this_method, __intp, __ret, __mStack);
                return __ret + 1;
            } else {
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
            }
        }

        static StackObject* SetMotion_2(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            UnityEngine.Vector3 @motion = new UnityEngine.Vector3();
            if (ILRuntime.Runtime.Generated.CLRBindings.s_UnityEngine_Vector3_Binding_Binder != null) {
                ILRuntime.Runtime.Generated.CLRBindings.s_UnityEngine_Vector3_Binding_Binder.ParseValue(ref @motion, __intp, ptr_of_this_method, __mStack, true);
            } else {
                @motion = (UnityEngine.Vector3)typeof(UnityEngine.Vector3).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)16);
                __intp.Free(ptr_of_this_method);
            }

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            StarterAssets.MyPlayerBehaviour instance_of_this_method = (StarterAssets.MyPlayerBehaviour)typeof(StarterAssets.MyPlayerBehaviour).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.SetMotion(@motion);

            return __ret;
        }

        static StackObject* SetSpeed_3(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Single @value = *(float*)&ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            StarterAssets.MyPlayerBehaviour instance_of_this_method = (StarterAssets.MyPlayerBehaviour)typeof(StarterAssets.MyPlayerBehaviour).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.SetSpeed(@value);

            return __ret;
        }

        static StackObject* SetMotionSpeed_4(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Single @value = *(float*)&ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            StarterAssets.MyPlayerBehaviour instance_of_this_method = (StarterAssets.MyPlayerBehaviour)typeof(StarterAssets.MyPlayerBehaviour).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.SetMotionSpeed(@value);

            return __ret;
        }

        static StackObject* SetJump_5(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Boolean @jump = ptr_of_this_method->Value == 1;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            StarterAssets.MyPlayerBehaviour instance_of_this_method = (StarterAssets.MyPlayerBehaviour)typeof(StarterAssets.MyPlayerBehaviour).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.SetJump(@jump);

            return __ret;
        }

        static StackObject* SetFreeFall_6(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Boolean @fall = ptr_of_this_method->Value == 1;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            StarterAssets.MyPlayerBehaviour instance_of_this_method = (StarterAssets.MyPlayerBehaviour)typeof(StarterAssets.MyPlayerBehaviour).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.SetFreeFall(@fall);

            return __ret;
        }



    }
}
