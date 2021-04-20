using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CSObjectWrapEditor;
using XLua;
using Framework.AssetManagement.GameBuilder;
using System.Reflection;
using System.Linq;

[InitializeOnLoad]
static public class XLuaConfig
{
    static private string s_LuaRootPath = "assets/application/xlua/lua";

    static XLuaConfig()
    {
        GeneratorConfig.common_path = UnityEngine.Application.dataPath + "/Application/XLua/Gen/";

        BundleBuilder.OnPreprocessBundleBuild += OnPreprocessBundleBuild;
        BundleBuilder.OnPostprocessBundleBuild += OnPostprocessBundleBuild;
        PlayerBuilder.OnPreprocessPlayerBuild += OnPreprocessPlayerBuild;
    }

    static private void OnPreprocessBundleBuild()
    {
        Debug.Log("OnPreprocessBundleBuild");

        // step 1. 把s_LuaRootPath下lua脚本复制到Temp/Lua，并添加后缀名.bytes
        const string targetPath = "Assets/Temp/Lua";
        if(Directory.Exists(targetPath))
        {
            Directory.Delete(targetPath, true);
        }
        Directory.CreateDirectory(targetPath);

        Framework.Core.Editor.EditorUtility.CopyUnityAsset(s_LuaRootPath, targetPath, ".bytes");
        
        // step 2. 设置更名后lua脚本的bundle name
        string[] guids = AssetDatabase.FindAssets("", new string[] {targetPath});
        List<string> directoryPathList = new List<string>();
        directoryPathList.Add(targetPath);
        foreach(var guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if(Directory.Exists(assetPath))
                directoryPathList.Add(assetPath);
        }

        string srcRoot = s_LuaRootPath.Substring(0, s_LuaRootPath.LastIndexOf("/")).ToLower();        // "assets/application/xlua"
        string dstRoot = targetPath.Substring(0, targetPath.LastIndexOf("/")).ToLower();              // "assets/temp"
        foreach(var dirPath in directoryPathList)
        {
            AssetImporter ti = AssetImporter.GetAtPath(dirPath);
            if(ti != null)
            {
                ti.assetBundleName = string.Format($"{srcRoot}/{dirPath.Substring(dstRoot.Length)}.ab");
            }
        }
    }

    static private void OnPostprocessBundleBuild()
    {
        Debug.Log("OnPostprocessBundleBuild");
    }

    static private void OnPreprocessPlayerBuild()
    {
        // 重新生成代码
        DelegateBridge.Gen_Flag = true;
        Generator.ClearAll();
        Generator.GenAll();
    }





    static List<string> excludedType = new List<string> {
       "HideInInspector", "ExecuteInEditMode",
       "AddComponentMenu", "ContextMenu",
       "RequireComponent", "DisallowMultipleComponent",
       "SerializeField", "AssemblyIsEditorAssembly",
       "Attribute", "Types",
       "UnitySurrogateSelector", "TrackedReference",
       "TypeInferenceRules", "FFTWindow",
       "RPC", "Network", "MasterServer",
       "BitStream", "HostData",
       "ConnectionTesterStatus", "GUI", "EventType",
       "EventModifiers", "FontStyle", "TextAlignment",
       "TextEditor", "TextEditorDblClickSnapping",
       "TextGenerator", "TextClipping", "Gizmos",
       "ADBannerView", "ADInterstitialAd",
       "Android", "Tizen", "jvalue",
       "iPhone", "iOS", "Windows", "CalendarIdentifier",
       "CalendarUnit", "CalendarUnit",
       "ClusterInput", "FullScreenMovieControlMode",
       "FullScreenMovieScalingMode", "Handheld",
       "LocalNotification", "NotificationServices",
       "RemoteNotificationType", "RemoteNotification",
       "SamsungTV", "TextureCompressionQuality",
       "TouchScreenKeyboardType", "TouchScreenKeyboard",
       "MovieTexture", "UnityEngineInternal",
       "Terrain", "Tree", "SplatPrototype",
       "DetailPrototype", "DetailRenderMode",
       "MeshSubsetCombineUtility", "AOT", "Social", "Enumerator",
       "SendMouseEvents", "Cursor", "Flash", "ActionScript",
       "OnRequestRebuild", "Ping",
       "ShaderVariantCollection", "SimpleJson.Reflection",
       "CoroutineTween", "GraphicRebuildTracker",
       "Advertisements", "UnityEditor", "WSA",
       "EventProvider", "Apple",
       "ClusterInput", "Motion",
       "UnityEngine.UI.ReflectionMethodsCache", "NativeLeakDetection",
       "NativeLeakDetectionMode", "WWWAudioExtensions", "UnityEngine.Experimental",
    };

    static bool isExcluded(Type type)
    {
       var fullName = type.FullName;
       for (int i = 0; i < excludedType.Count; i++)
       {
           if (fullName.Contains(excludedType[i]))
           {
               return true;
           }
       }
       return false;
    }

    // 通过此配置筛选出符合条件的官方程序集和第三方程序集
    static List<string> excludedAssembly = new List<string>()
    {
        "UnityEngine.XRModule",
        "UnityEngine.WindModule",
        "UnityEngine.VRModule",
        "UnityEngine.VideoModule",
        "UnityEngine.VFXModule",
        "UnityEngine.VehiclesModule",
        "UnityEngine.UnityWebRequestWWWModule",
        "UnityEngine.UnityWebRequestTextureModule",
        "UnityEngine.UnityWebRequestAudioModule",
        "UnityEngine.UnityWebRequestAssetBundleModule",
        "UnityEngine.UnityTestProtocolModule",
        "UnityEngine.UnityCurlModule",
        "UnityEngine.UnityConnectModule",
        "UnityEngine.UnityAnalyticsModule",
        "UnityEngine.UNETModule",
        "UnityEngine.UmbraModule",
        "UnityEngine.UIElementsNativeModule",
        "UnityEngine.UIElementsModule",
        "UnityEngine.TLSModule",
        "UnityEngine.TilemapModule",
        "UnityEngine.TerrainPhysicsModule",
        "UnityEngine.TerrainModule",
        "UnityEngine.SubsystemsModule",
        "UnityEngine.SubstanceModule",
        "UnityEngine.IMGUIModule",
        "UnityEngine.HotReloadModule",
        "UnityEngine.GridModule",
        "UnityEngine.GIModule",
        "UnityEngine.GameCenterModule",
        "UnityEngine.DSPGraphModule",
        "UnityEngine.ClothModule",
        "UnityEngine.ARModule",
        "UnityEngine.AIModule",
        "UnityEngine.AccessibilityModule",
        "UnityEngine.CrashReportingModule",
        "Unity.CompilationPipeline.Common",
        "Unity.Analytics",
        "Editor",
        "Test",
        "Legacy",
        "mscorlib",
        "System.",
        "Google.Protobuf",
        "nunit",
        "Mono.Security",
        "ICSharpCode.",
        "Newtonsoft.Json",
        "ExCSS.",
        "Framework.",
        "System",
        "Assembly-CSharp"
    };
    static bool isExcluded(Assembly ass)
    {
       var name = ass.GetName().Name.ToLower();
       for (int i = 0; i < excludedAssembly.Count; i++)
       {
           if (name.Contains(excludedAssembly[i].ToLower()))
           {
               return true;
           }
       }
       return false;
    }

    [LuaCallCSharp]
    public static IEnumerable<Type> LuaCallCSharp
    {
       get
       {
           List<string> namespaces = new List<string>() // 在这里添加名字空间
           {
               "UnityEngine",
               "UnityEngine.UI"
           };

           var basePath = Path.Combine(EditorApplication.applicationContentsPath, "PlaybackEngines/AndroidPlayer/Variations/il2cpp/Managed/");
           var files = new List<string>(Directory.GetFiles(basePath, "*.dll"));
           var unityTypes = (from assembly in files.Select(s => Assembly.LoadFrom(s))
                             where !(assembly.ManifestModule is System.Reflection.Emit.ModuleBuilder) && !isExcluded(assembly)
                             from type in assembly.GetExportedTypes()
                             where type.Namespace != null && namespaces.Contains(type.Namespace) && !isExcluded(type)
                                     && type.BaseType != typeof(MulticastDelegate) && !type.IsInterface && !type.IsEnum
                             select type);

           string[] customAssemblys = new string[] {
               "Assembly-CSharp",
           };
           var customTypes = (from assembly in customAssemblys.Select(s => Assembly.Load(s))
                              from type in assembly.GetExportedTypes()
                              where type.Namespace == null || !type.Namespace.StartsWith("XLua")
                                      && type.BaseType != typeof(MulticastDelegate) && !type.IsInterface && !type.IsEnum && !isExcluded(type)
                              select type);
           return unityTypes.Concat(customTypes);
       }
    }
    
    [MenuItem("Tools/TestXX")]
    static void TestXX()
    {
        //////////////////// 导出所有引擎和官方插件的可用类型
        List<string> namespaces = new List<string>() // 在这里添加名字空间
           {
               "UnityEngine",
               "UnityEngine.UI"
           };

        var assemblies = (  from assembly in AppDomain.CurrentDomain.GetAssemblies()
                            where !(assembly.ManifestModule is System.Reflection.Emit.ModuleBuilder) && !isExcluded(assembly)
                            select assembly );

        foreach(var ass in assemblies)
            Debug.Log(ass.GetName());

        var unityTypes = (  from ass in assemblies
                            from type in ass.GetExportedTypes()
                            where /*type.Namespace != null && namespaces.Contains(type.Namespace) &&*/ !isExcluded(type)
                            && type.BaseType != typeof(MulticastDelegate) && !type.IsInterface && !type.IsEnum
                            select type );

        // foreach(var type in unityTypes)
        //     Debug.Log(type);



        // string[] customAssemblys = new string[] {
        //        "Assembly-CSharp",
        //    };
        //    var customTypes = (from assembly in customAssemblys.Select(s => Assembly.Load(s))
        //                       from type in assembly.GetExportedTypes()
        //                       where type.Namespace == null || !type.Namespace.StartsWith("XLua")
        //                        && type.BaseType != typeof(MulticastDelegate) && !type.IsInterface && !type.IsEnum
        //                       select type);

        // foreach(var type in customTypes)
        // {
        //     Debug.Log(type);
        // }
    }

    // CSharp涉及到的delegate加到CSharpCallLua列表，后续可以直接用lua函数做callback
    [CSharpCallLua]
    public static List<Type> CSharpCallLua
    {
       get
       {
           var lua_call_csharp = LuaCallCSharp;
           var delegate_types = new List<Type>();
           var flag = BindingFlags.Public | BindingFlags.Instance
               | BindingFlags.Static | BindingFlags.IgnoreCase | BindingFlags.DeclaredOnly;
           foreach (var field in (from type in lua_call_csharp select type).SelectMany(type => type.GetFields(flag)))
           {
               if (typeof(Delegate).IsAssignableFrom(field.FieldType))
               {
                   delegate_types.Add(field.FieldType);
               }
           }

           foreach (var method in (from type in lua_call_csharp select type).SelectMany(type => type.GetMethods(flag)))
           {
               if (typeof(Delegate).IsAssignableFrom(method.ReturnType))
               {
                   delegate_types.Add(method.ReturnType);
               }
               foreach (var param in method.GetParameters())
               {
                   var paramType = param.ParameterType.IsByRef ? param.ParameterType.GetElementType() : param.ParameterType;
                   if (typeof(Delegate).IsAssignableFrom(paramType))
                   {
                       delegate_types.Add(paramType);
                   }
               }
           }
           return delegate_types.Where(t => t.BaseType == typeof(MulticastDelegate) && !hasGenericParameter(t) && !delegateHasEditorRef(t)).Distinct().ToList();
       }
    }

    static bool hasGenericParameter(Type type)
    {
       if (type.IsGenericTypeDefinition) return true;
       if (type.IsGenericParameter) return true;
       if (type.IsByRef || type.IsArray)
       {
           return hasGenericParameter(type.GetElementType());
       }
       if (type.IsGenericType)
       {
           foreach (var typeArg in type.GetGenericArguments())
           {
               if (hasGenericParameter(typeArg))
               {
                   return true;
               }
           }
       }
       return false;
    }

    static bool typeHasEditorRef(Type type)
    {
       if (type.Namespace != null && (type.Namespace == "UnityEditor" || type.Namespace.StartsWith("UnityEditor.")))
       {
           return true;
       }
       if (type.IsNested)
       {
           return typeHasEditorRef(type.DeclaringType);
       }
       if (type.IsByRef || type.IsArray)
       {
           return typeHasEditorRef(type.GetElementType());
       }
       if (type.IsGenericType)
       {
           foreach (var typeArg in type.GetGenericArguments())
           {
               if (typeHasEditorRef(typeArg))
               {
                   return true;
               }
           }
       }
       return false;
    }

    static bool delegateHasEditorRef(Type delegateType)
    {
       if (typeHasEditorRef(delegateType)) return true;
       var method = delegateType.GetMethod("Invoke");
       if (method == null)
       {
           return false;
       }
       if (typeHasEditorRef(method.ReturnType)) return true;
       return method.GetParameters().Any(pinfo => typeHasEditorRef(pinfo.ParameterType));
    }

    [BlackList]
    public static List<List<string>> BlackList = new List<List<string>>()  {
                new List<string>(){"System.Xml.XmlNodeList", "ItemOf"},
                new List<string>(){"UnityEngine.WWW", "movie"},
    #if UNITY_WEBGL
                new List<string>(){"UnityEngine.WWW", "threadPriority"},
    #endif
                new List<string>(){"UnityEngine.Texture2D", "alphaIsTransparency"},
                new List<string>(){"UnityEngine.Security", "GetChainOfTrustValue"},
                new List<string>(){"UnityEngine.CanvasRenderer", "onRequestRebuild"},
                new List<string>(){"UnityEngine.Light", "areaSize"},
                new List<string>(){"UnityEngine.Light", "lightmapBakeType"},
                new List<string>(){"UnityEngine.WWW", "MovieTexture"},
                new List<string>(){"UnityEngine.WWW", "GetMovieTexture"},
                new List<string>(){"UnityEngine.AnimatorOverrideController", "PerformOverrideClipListCleanup"},
    #if !UNITY_WEBPLAYER
                new List<string>(){"UnityEngine.Application", "ExternalEval"},
    #endif
                new List<string>(){"UnityEngine.GameObject", "networkView"}, //4.6.2 not support
                new List<string>(){"UnityEngine.Component", "networkView"},  //4.6.2 not support
                new List<string>(){"System.IO.FileInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
                new List<string>(){"System.IO.FileInfo", "SetAccessControl", "System.Security.AccessControl.FileSecurity"},
                new List<string>(){"System.IO.DirectoryInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
                new List<string>(){"System.IO.DirectoryInfo", "SetAccessControl", "System.Security.AccessControl.DirectorySecurity"},
                new List<string>(){"System.IO.DirectoryInfo", "CreateSubdirectory", "System.String", "System.Security.AccessControl.DirectorySecurity"},
                new List<string>(){"System.IO.DirectoryInfo", "Create", "System.Security.AccessControl.DirectorySecurity"},
                new List<string>(){"UnityEngine.MonoBehaviour", "runInEditMode"},
            };

#if UNITY_2018_1_OR_NEWER
    [BlackList]
    public static Func<MemberInfo, bool> MethodFilter = (memberInfo) =>
    {
        if (memberInfo.DeclaringType.IsGenericType && memberInfo.DeclaringType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            if (memberInfo.MemberType == MemberTypes.Constructor)
            {
                ConstructorInfo constructorInfo = memberInfo as ConstructorInfo;
                var parameterInfos = constructorInfo.GetParameters();
                if (parameterInfos.Length > 0)
                {
                    if (typeof(System.Collections.IEnumerable).IsAssignableFrom(parameterInfos[0].ParameterType))
                    {
                        return true;
                    }
                }
            }
            else if (memberInfo.MemberType == MemberTypes.Method)
            {
                var methodInfo = memberInfo as MethodInfo;
                if (methodInfo.Name == "TryAdd" || methodInfo.Name == "Remove" && methodInfo.GetParameters().Length == 2)
                {
                    return true;
                }
            }
        }
        return false;
    };
#endif

    



    //lua中要使用到C#库的配置，比如C#标准库，或者Unity API，第三方库等。
    // [LuaCallCSharp]
    // public static List<Type> LuaCallCSharp = new List<Type>() {
    //             typeof(System.Object),
    //             typeof(UnityEngine.Object),
    //             typeof(Vector2),
    //             typeof(Vector3),
    //             typeof(Vector4),
    //             typeof(Quaternion),
    //             typeof(Color),
    //             typeof(Ray),
    //             typeof(Bounds),
    //             typeof(Ray2D),
    //             typeof(Time),
    //             typeof(GameObject),
    //             typeof(Component),
    //             typeof(Behaviour),
    //             typeof(Transform),
    //             typeof(Resources),
    //             typeof(TextAsset),
    //             typeof(Keyframe),
    //             typeof(AnimationCurve),
    //             typeof(AnimationClip),
    //             typeof(MonoBehaviour),
    //             typeof(ParticleSystem),
    //             typeof(SkinnedMeshRenderer),
    //             typeof(Renderer),
    //             typeof(Light),
    //             typeof(Mathf),
    //             typeof(System.Collections.Generic.List<int>),
    //             typeof(Action<string>),
    //             typeof(UnityEngine.Debug),
    //             typeof(WaitForSeconds)
    //         };

    //C#静态调用Lua的配置（包括事件的原型），仅可以配delegate，interface
    // [CSharpCallLua]
    // public static List<Type> CSharpCallLua = new List<Type>() {
    //             typeof(Action),
    //             typeof(Func<double, double, double>),
    //             typeof(Action<string>),
    //             typeof(Action<double>),
    //             typeof(Action<bool>),
    //             typeof(UnityEngine.Events.UnityAction),
    //             typeof(System.Collections.IEnumerator)
    //         };

    //黑名单
    // [BlackList]
    // public static List<List<string>> BlackList = new List<List<string>>()  {
    //             new List<string>(){"System.Xml.XmlNodeList", "ItemOf"},
    //             new List<string>(){"UnityEngine.WWW", "movie"},
    // #if UNITY_WEBGL
    //             new List<string>(){"UnityEngine.WWW", "threadPriority"},
    // #endif
    //             new List<string>(){"UnityEngine.Texture2D", "alphaIsTransparency"},
    //             new List<string>(){"UnityEngine.Security", "GetChainOfTrustValue"},
    //             new List<string>(){"UnityEngine.CanvasRenderer", "onRequestRebuild"},
    //             new List<string>(){"UnityEngine.Light", "areaSize"},
    //             new List<string>(){"UnityEngine.Light", "lightmapBakeType"},
    //             new List<string>(){"UnityEngine.Light", "SetLightDirty"},
    //             new List<string>(){"UnityEngine.Light", "shadowRadius"},
    //             new List<string>(){"UnityEngine.Light", "shadowAngle"},
    //             new List<string>(){"UnityEngine.Light", "shadowRadius"},
    //             new List<string>(){"UnityEngine.Light", "shadowAngle"},
    //             new List<string>(){"UnityEngine.WWW", "MovieTexture"},
    //             new List<string>(){"UnityEngine.WWW", "GetMovieTexture"},
    //             new List<string>(){"UnityEngine.AnimatorOverrideController", "PerformOverrideClipListCleanup"},
    // #if !UNITY_WEBPLAYER
    //             new List<string>(){"UnityEngine.Application", "ExternalEval"},
    // #endif
    //             new List<string>(){"UnityEngine.GameObject", "networkView"}, //4.6.2 not support
    //             new List<string>(){"UnityEngine.Component", "networkView"},  //4.6.2 not support
    //             new List<string>(){"System.IO.FileInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
    //             new List<string>(){"System.IO.FileInfo", "SetAccessControl", "System.Security.AccessControl.FileSecurity"},
    //             new List<string>(){"System.IO.DirectoryInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
    //             new List<string>(){"System.IO.DirectoryInfo", "SetAccessControl", "System.Security.AccessControl.DirectorySecurity"},
    //             new List<string>(){"System.IO.DirectoryInfo", "CreateSubdirectory", "System.String", "System.Security.AccessControl.DirectorySecurity"},
    //             new List<string>(){"System.IO.DirectoryInfo", "Create", "System.Security.AccessControl.DirectorySecurity"},
    //             new List<string>(){"UnityEngine.MonoBehaviour", "runInEditMode"},
    //         };
}
