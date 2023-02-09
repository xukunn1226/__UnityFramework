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
    unsafe class AnimationInstancingModule_Runtime_AnimationInstancing_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(AnimationInstancingModule.Runtime.AnimationInstancing);
            args = new Type[]{typeof(AnimationInstancingModule.Runtime.AnimationInstancing.onAnimationEvent)};
            method = type.GetMethod("add_OnAnimationEvent", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, add_OnAnimationEvent_0);
            args = new Type[]{typeof(AnimationInstancingModule.Runtime.AnimationInstancing.onAnimationEvent)};
            method = type.GetMethod("remove_OnAnimationEvent", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, remove_OnAnimationEvent_1);
            args = new Type[]{typeof(System.String), typeof(System.Single)};
            method = type.GetMethod("PlayAnimation", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, PlayAnimation_2);
            args = new Type[]{typeof(System.Single)};
            method = type.GetMethod("set_playSpeed", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_playSpeed_3);

            field = type.GetField("isShow", flag);
            app.RegisterCLRFieldGetter(field, get_isShow_0);
            app.RegisterCLRFieldSetter(field, set_isShow_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_isShow_0, AssignFromStack_isShow_0);


        }


        static StackObject* add_OnAnimationEvent_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            AnimationInstancingModule.Runtime.AnimationInstancing.onAnimationEvent @value = (AnimationInstancingModule.Runtime.AnimationInstancing.onAnimationEvent)typeof(AnimationInstancingModule.Runtime.AnimationInstancing.onAnimationEvent).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            AnimationInstancingModule.Runtime.AnimationInstancing instance_of_this_method = (AnimationInstancingModule.Runtime.AnimationInstancing)typeof(AnimationInstancingModule.Runtime.AnimationInstancing).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.OnAnimationEvent += value;

            return __ret;
        }

        static StackObject* remove_OnAnimationEvent_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            AnimationInstancingModule.Runtime.AnimationInstancing.onAnimationEvent @value = (AnimationInstancingModule.Runtime.AnimationInstancing.onAnimationEvent)typeof(AnimationInstancingModule.Runtime.AnimationInstancing.onAnimationEvent).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            AnimationInstancingModule.Runtime.AnimationInstancing instance_of_this_method = (AnimationInstancingModule.Runtime.AnimationInstancing)typeof(AnimationInstancingModule.Runtime.AnimationInstancing).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.OnAnimationEvent -= value;

            return __ret;
        }

        static StackObject* PlayAnimation_2(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Single @transitionDuration = *(float*)&ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.String @name = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            AnimationInstancingModule.Runtime.AnimationInstancing instance_of_this_method = (AnimationInstancingModule.Runtime.AnimationInstancing)typeof(AnimationInstancingModule.Runtime.AnimationInstancing).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.PlayAnimation(@name, @transitionDuration);

            return __ret;
        }

        static StackObject* set_playSpeed_3(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Single @value = *(float*)&ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            AnimationInstancingModule.Runtime.AnimationInstancing instance_of_this_method = (AnimationInstancingModule.Runtime.AnimationInstancing)typeof(AnimationInstancingModule.Runtime.AnimationInstancing).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.playSpeed = value;

            return __ret;
        }


        static object get_isShow_0(ref object o)
        {
            return ((AnimationInstancingModule.Runtime.AnimationInstancing)o).isShow;
        }

        static StackObject* CopyToStack_isShow_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((AnimationInstancingModule.Runtime.AnimationInstancing)o).isShow;
            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method ? 1 : 0;
            return __ret + 1;
        }

        static void set_isShow_0(ref object o, object v)
        {
            ((AnimationInstancingModule.Runtime.AnimationInstancing)o).isShow = (System.Boolean)v;
        }

        static StackObject* AssignFromStack_isShow_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Boolean @isShow = ptr_of_this_method->Value == 1;
            ((AnimationInstancingModule.Runtime.AnimationInstancing)o).isShow = @isShow;
            return ptr_of_this_method;
        }



    }
}
