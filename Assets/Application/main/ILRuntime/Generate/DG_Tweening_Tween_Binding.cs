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
    unsafe class DG_Tweening_Tween_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            FieldInfo field;
            Type[] args;
            Type type = typeof(DG.Tweening.Tween);

            field = type.GetField("onRewind", flag);
            app.RegisterCLRFieldGetter(field, get_onRewind_0);
            app.RegisterCLRFieldSetter(field, set_onRewind_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_onRewind_0, AssignFromStack_onRewind_0);


        }



        static object get_onRewind_0(ref object o)
        {
            return ((DG.Tweening.Tween)o).onRewind;
        }

        static StackObject* CopyToStack_onRewind_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((DG.Tweening.Tween)o).onRewind;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_onRewind_0(ref object o, object v)
        {
            ((DG.Tweening.Tween)o).onRewind = (DG.Tweening.TweenCallback)v;
        }

        static StackObject* AssignFromStack_onRewind_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            DG.Tweening.TweenCallback @onRewind = (DG.Tweening.TweenCallback)typeof(DG.Tweening.TweenCallback).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            ((DG.Tweening.Tween)o).onRewind = @onRewind;
            return ptr_of_this_method;
        }



    }
}
