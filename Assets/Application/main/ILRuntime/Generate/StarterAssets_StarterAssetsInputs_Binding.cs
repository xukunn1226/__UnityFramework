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
    unsafe class StarterAssets_StarterAssetsInputs_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            FieldInfo field;
            Type[] args;
            Type type = typeof(StarterAssets.StarterAssetsInputs);

            field = type.GetField("onMove", flag);
            app.RegisterCLRFieldGetter(field, get_onMove_0);
            app.RegisterCLRFieldSetter(field, set_onMove_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_onMove_0, AssignFromStack_onMove_0);
            field = type.GetField("onLook", flag);
            app.RegisterCLRFieldGetter(field, get_onLook_1);
            app.RegisterCLRFieldSetter(field, set_onLook_1);
            app.RegisterCLRFieldBinding(field, CopyToStack_onLook_1, AssignFromStack_onLook_1);
            field = type.GetField("onJump", flag);
            app.RegisterCLRFieldGetter(field, get_onJump_2);
            app.RegisterCLRFieldSetter(field, set_onJump_2);
            app.RegisterCLRFieldBinding(field, CopyToStack_onJump_2, AssignFromStack_onJump_2);
            field = type.GetField("onSprint", flag);
            app.RegisterCLRFieldGetter(field, get_onSprint_3);
            app.RegisterCLRFieldSetter(field, set_onSprint_3);
            app.RegisterCLRFieldBinding(field, CopyToStack_onSprint_3, AssignFromStack_onSprint_3);
            field = type.GetField("analogMovement", flag);
            app.RegisterCLRFieldGetter(field, get_analogMovement_4);
            app.RegisterCLRFieldSetter(field, set_analogMovement_4);
            app.RegisterCLRFieldBinding(field, CopyToStack_analogMovement_4, AssignFromStack_analogMovement_4);
            field = type.GetField("move", flag);
            app.RegisterCLRFieldGetter(field, get_move_5);
            app.RegisterCLRFieldSetter(field, set_move_5);
            app.RegisterCLRFieldBinding(field, CopyToStack_move_5, AssignFromStack_move_5);


        }



        static object get_onMove_0(ref object o)
        {
            return ((StarterAssets.StarterAssetsInputs)o).onMove;
        }

        static StackObject* CopyToStack_onMove_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((StarterAssets.StarterAssetsInputs)o).onMove;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_onMove_0(ref object o, object v)
        {
            ((StarterAssets.StarterAssetsInputs)o).onMove = (System.Action<UnityEngine.Vector2>)v;
        }

        static StackObject* AssignFromStack_onMove_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Action<UnityEngine.Vector2> @onMove = (System.Action<UnityEngine.Vector2>)typeof(System.Action<UnityEngine.Vector2>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            ((StarterAssets.StarterAssetsInputs)o).onMove = @onMove;
            return ptr_of_this_method;
        }

        static object get_onLook_1(ref object o)
        {
            return ((StarterAssets.StarterAssetsInputs)o).onLook;
        }

        static StackObject* CopyToStack_onLook_1(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((StarterAssets.StarterAssetsInputs)o).onLook;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_onLook_1(ref object o, object v)
        {
            ((StarterAssets.StarterAssetsInputs)o).onLook = (System.Action<UnityEngine.Vector2>)v;
        }

        static StackObject* AssignFromStack_onLook_1(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Action<UnityEngine.Vector2> @onLook = (System.Action<UnityEngine.Vector2>)typeof(System.Action<UnityEngine.Vector2>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            ((StarterAssets.StarterAssetsInputs)o).onLook = @onLook;
            return ptr_of_this_method;
        }

        static object get_onJump_2(ref object o)
        {
            return ((StarterAssets.StarterAssetsInputs)o).onJump;
        }

        static StackObject* CopyToStack_onJump_2(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((StarterAssets.StarterAssetsInputs)o).onJump;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_onJump_2(ref object o, object v)
        {
            ((StarterAssets.StarterAssetsInputs)o).onJump = (System.Action<System.Boolean>)v;
        }

        static StackObject* AssignFromStack_onJump_2(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Action<System.Boolean> @onJump = (System.Action<System.Boolean>)typeof(System.Action<System.Boolean>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            ((StarterAssets.StarterAssetsInputs)o).onJump = @onJump;
            return ptr_of_this_method;
        }

        static object get_onSprint_3(ref object o)
        {
            return ((StarterAssets.StarterAssetsInputs)o).onSprint;
        }

        static StackObject* CopyToStack_onSprint_3(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((StarterAssets.StarterAssetsInputs)o).onSprint;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_onSprint_3(ref object o, object v)
        {
            ((StarterAssets.StarterAssetsInputs)o).onSprint = (System.Action<System.Boolean>)v;
        }

        static StackObject* AssignFromStack_onSprint_3(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Action<System.Boolean> @onSprint = (System.Action<System.Boolean>)typeof(System.Action<System.Boolean>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            ((StarterAssets.StarterAssetsInputs)o).onSprint = @onSprint;
            return ptr_of_this_method;
        }

        static object get_analogMovement_4(ref object o)
        {
            return ((StarterAssets.StarterAssetsInputs)o).analogMovement;
        }

        static StackObject* CopyToStack_analogMovement_4(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((StarterAssets.StarterAssetsInputs)o).analogMovement;
            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method ? 1 : 0;
            return __ret + 1;
        }

        static void set_analogMovement_4(ref object o, object v)
        {
            ((StarterAssets.StarterAssetsInputs)o).analogMovement = (System.Boolean)v;
        }

        static StackObject* AssignFromStack_analogMovement_4(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Boolean @analogMovement = ptr_of_this_method->Value == 1;
            ((StarterAssets.StarterAssetsInputs)o).analogMovement = @analogMovement;
            return ptr_of_this_method;
        }

        static object get_move_5(ref object o)
        {
            return ((StarterAssets.StarterAssetsInputs)o).move;
        }

        static StackObject* CopyToStack_move_5(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((StarterAssets.StarterAssetsInputs)o).move;
            if (ILRuntime.Runtime.Generated.CLRBindings.s_UnityEngine_Vector2_Binding_Binder != null) {
                ILRuntime.Runtime.Generated.CLRBindings.s_UnityEngine_Vector2_Binding_Binder.PushValue(ref result_of_this_method, __intp, __ret, __mStack);
                return __ret + 1;
            } else {
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
            }
        }

        static void set_move_5(ref object o, object v)
        {
            ((StarterAssets.StarterAssetsInputs)o).move = (UnityEngine.Vector2)v;
        }

        static StackObject* AssignFromStack_move_5(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            UnityEngine.Vector2 @move = new UnityEngine.Vector2();
            if (ILRuntime.Runtime.Generated.CLRBindings.s_UnityEngine_Vector2_Binding_Binder != null) {
                ILRuntime.Runtime.Generated.CLRBindings.s_UnityEngine_Vector2_Binding_Binder.ParseValue(ref @move, __intp, ptr_of_this_method, __mStack, true);
            } else {
                @move = (UnityEngine.Vector2)typeof(UnityEngine.Vector2).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)16);
            }
            ((StarterAssets.StarterAssetsInputs)o).move = @move;
            return ptr_of_this_method;
        }



    }
}
