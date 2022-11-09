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
    unsafe class Framework_AssetManagement_Runtime_AssetManager_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(Framework.AssetManagement.Runtime.AssetManager);
            args = new Type[]{typeof(System.String)};
            method = type.GetMethod("InstantiatePrefab", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, InstantiatePrefab_0);
            args = new Type[]{typeof(System.String)};
            method = type.GetMethod("Instantiate", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Instantiate_1);
            args = new Type[]{typeof(Framework.AssetManagement.Runtime.GameObjectLoader)};
            method = type.GetMethod("ReleaseInst", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, ReleaseInst_2);
            Dictionary<string, List<MethodInfo>> genericMethods = new Dictionary<string, List<MethodInfo>>();
            List<MethodInfo> lst = null;                    
            foreach(var m in type.GetMethods())
            {
                if(m.IsGenericMethodDefinition)
                {
                    if (!genericMethods.TryGetValue(m.Name, out lst))
                    {
                        lst = new List<MethodInfo>();
                        genericMethods[m.Name] = lst;
                    }
                    lst.Add(m);
                }
            }
            args = new Type[]{typeof(UnityEngine.Rendering.RenderPipelineAsset)};
            if (genericMethods.TryGetValue("LoadAsset", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(Framework.AssetManagement.Runtime.AssetLoader<UnityEngine.Rendering.RenderPipelineAsset>), typeof(System.String)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, LoadAsset_3);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(UnityEngine.Rendering.RenderPipelineAsset)};
            if (genericMethods.TryGetValue("UnloadAsset", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(void), typeof(Framework.AssetManagement.Runtime.AssetLoader<UnityEngine.Rendering.RenderPipelineAsset>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, UnloadAsset_4);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(UnityEngine.U2D.SpriteAtlas)};
            if (genericMethods.TryGetValue("UnloadAsset", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(void), typeof(Framework.AssetManagement.Runtime.AssetLoader<UnityEngine.U2D.SpriteAtlas>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, UnloadAsset_5);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(UnityEngine.U2D.SpriteAtlas)};
            if (genericMethods.TryGetValue("LoadAsset", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(Framework.AssetManagement.Runtime.AssetLoader<UnityEngine.U2D.SpriteAtlas>), typeof(System.String)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, LoadAsset_6);

                        break;
                    }
                }
            }


        }


        static StackObject* InstantiatePrefab_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.String @assetPath = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = Framework.AssetManagement.Runtime.AssetManager.InstantiatePrefab(@assetPath);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* Instantiate_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.String @assetPath = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = Framework.AssetManagement.Runtime.AssetManager.Instantiate(@assetPath);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* ReleaseInst_2(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            Framework.AssetManagement.Runtime.GameObjectLoader @loader = (Framework.AssetManagement.Runtime.GameObjectLoader)typeof(Framework.AssetManagement.Runtime.GameObjectLoader).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            Framework.AssetManagement.Runtime.AssetManager.ReleaseInst(@loader);

            return __ret;
        }

        static StackObject* LoadAsset_3(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.String @assetPath = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = Framework.AssetManagement.Runtime.AssetManager.LoadAsset<UnityEngine.Rendering.RenderPipelineAsset>(@assetPath);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* UnloadAsset_4(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            Framework.AssetManagement.Runtime.AssetLoader<UnityEngine.Rendering.RenderPipelineAsset> @loader = (Framework.AssetManagement.Runtime.AssetLoader<UnityEngine.Rendering.RenderPipelineAsset>)typeof(Framework.AssetManagement.Runtime.AssetLoader<UnityEngine.Rendering.RenderPipelineAsset>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            Framework.AssetManagement.Runtime.AssetManager.UnloadAsset<UnityEngine.Rendering.RenderPipelineAsset>(@loader);

            return __ret;
        }

        static StackObject* UnloadAsset_5(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            Framework.AssetManagement.Runtime.AssetLoader<UnityEngine.U2D.SpriteAtlas> @loader = (Framework.AssetManagement.Runtime.AssetLoader<UnityEngine.U2D.SpriteAtlas>)typeof(Framework.AssetManagement.Runtime.AssetLoader<UnityEngine.U2D.SpriteAtlas>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            Framework.AssetManagement.Runtime.AssetManager.UnloadAsset<UnityEngine.U2D.SpriteAtlas>(@loader);

            return __ret;
        }

        static StackObject* LoadAsset_6(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.String @assetPath = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = Framework.AssetManagement.Runtime.AssetManager.LoadAsset<UnityEngine.U2D.SpriteAtlas>(@assetPath);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }



    }
}
