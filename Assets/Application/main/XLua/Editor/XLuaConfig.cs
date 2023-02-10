using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CSObjectWrapEditor;
using XLua;
using System.Reflection;
using System.Linq;
using UnityEditor.Build;

[InitializeOnLoad]
static public class XLuaConfig
{
    static public string s_LuaRootPath = "assets/application/main/xlua/lua";        // 所有lua文件的根目录

    static XLuaConfig()
    {
        // GeneratorConfig.common_path = UnityEngine.Application.dataPath + "/Application/XLua/Gen/";

        // BundleBuilder.OnPreprocessBundleBuild += OnPreprocessBundleBuild;
        // BundleBuilder.OnPostprocessBundleBuild += OnPostprocessBundleBuild;
        // PlayerBuilder.OnPreprocessPlayerBuild += OnPreprocessPlayerBuild;
    }

    // static private void OnPreprocessBundleBuild()
    // {
    //     Debug.Log("OnPreprocessBundleBuild");

    //     // BuildLuaAssetBundle();       // 另有更优方案，见LuaAsset, LuaImporter
    // }

    // static private void BuildLuaAssetBundle()
    // {
    //     // step 1. 把s_LuaRootPath下lua脚本复制到Temp/Lua，并添加后缀名.bytes
    //     const string targetPath = "Assets/Temp/Lua";
    //     if(Directory.Exists(targetPath))
    //     {
    //         Directory.Delete(targetPath, true);
    //     }
    //     Directory.CreateDirectory(targetPath);

    //     Framework.Core.Editor.EditorUtility.CopyUnityAsset(s_LuaRootPath, targetPath, ".bytes");
        
    //     // step 2. 设置更名后lua脚本的bundle name
    //     string[] guids = AssetDatabase.FindAssets("", new string[] {targetPath});
    //     List<string> directoryPathList = new List<string>();
    //     directoryPathList.Add(targetPath);
    //     foreach(var guid in guids)
    //     {
    //         string assetPath = AssetDatabase.GUIDToAssetPath(guid);
    //         if(Directory.Exists(assetPath))
    //             directoryPathList.Add(assetPath);
    //     }

    //     string srcRoot = s_LuaRootPath.Substring(0, s_LuaRootPath.LastIndexOf("/")).ToLower();        // "assets/application/xlua"
    //     string dstRoot = targetPath.Substring(0, targetPath.LastIndexOf("/")).ToLower();              // "assets/temp"
    //     foreach(var dirPath in directoryPathList)
    //     {
    //         AssetImporter ti = AssetImporter.GetAtPath(dirPath);
    //         if(ti != null)
    //         {
    //             ti.assetBundleName = string.Format($"{srcRoot}/{dirPath.Substring(dstRoot.Length)}.ab");
    //         }
    //     }
    // }

    // static private void OnPostprocessBundleBuild()
    // {
    //     Debug.Log("OnPostprocessBundleBuild");
    // }

    private class BuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 50; } }

        public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
        {
            // 重新生成代码
            //DelegateBridge.Gen_Flag = true;
            //Generator.ClearAll();
            //Generator.GenAll();
        }
    }

    // static private void OnPreprocessPlayerBuild()
    // {
    //     // 重新生成代码
    //     DelegateBridge.Gen_Flag = true;
    //     Generator.ClearAll();
    //     Generator.GenAll();
    // }





//     // static List<string> excludedType = new List<string> {
//     //    "HideInInspector", "ExecuteInEditMode",
//     //    "AddComponentMenu", "ContextMenu",
//     //    "RequireComponent", "DisallowMultipleComponent",
//     //    "SerializeField", "AssemblyIsEditorAssembly",
//     //    "Attribute", "Types",
//     //    "UnitySurrogateSelector", "TrackedReference",
//     //    "TypeInferenceRules", "FFTWindow",
//     //    "RPC", "Network", "MasterServer",
//     //    "BitStream", "HostData",
//     //    "ConnectionTesterStatus", "GUI", "EventType",
//     //    "EventModifiers", "FontStyle", "TextAlignment",
//     //    "TextEditor", "TextEditorDblClickSnapping",
//     //    "TextGenerator", "TextClipping", "Gizmos",
//     //    "ADBannerView", "ADInterstitialAd",
//     //    "Android", "Tizen", "jvalue",
//     //    "iPhone", "iOS", "Windows", "CalendarIdentifier",
//     //    "CalendarUnit", "CalendarUnit",
//     //    "ClusterInput", "FullScreenMovieControlMode",
//     //    "FullScreenMovieScalingMode", "Handheld",
//     //    "LocalNotification", "NotificationServices",
//     //    "RemoteNotificationType", "RemoteNotification",
//     //    "SamsungTV", "TextureCompressionQuality",
//     //    "TouchScreenKeyboardType", "TouchScreenKeyboard",
//     //    "MovieTexture", "UnityEngineInternal",
//     //    "Terrain", "Tree", "SplatPrototype",
//     //    "DetailPrototype", "DetailRenderMode",
//     //    "MeshSubsetCombineUtility", "AOT", "Social", "Enumerator",
//     //    "SendMouseEvents", "Cursor", "Flash", "ActionScript",
//     //    "OnRequestRebuild", "Ping",
//     //    "ShaderVariantCollection", "SimpleJson.Reflection",
//     //    "CoroutineTween", "GraphicRebuildTracker",
//     //    "Advertisements", "UnityEditor", "WSA",
//     //    "EventProvider", "Apple",
//     //    "ClusterInput", "Motion",
//     //    "UnityEngine.UI.ReflectionMethodsCache", "NativeLeakDetection",
//     //    "NativeLeakDetectionMode", "WWWAudioExtensions", "UnityEngine.Experimental",
//     // };

//     // static bool isExcluded(Type type)
//     // {
//     //    var fullName = type.FullName;
//     //    for (int i = 0; i < excludedType.Count; i++)
//     //    {
//     //        if (fullName.Contains(excludedType[i]))
//     //        {
//     //            return true;
//     //        }
//     //    }
//     //    return false;
//     // }

//     // // 通过此配置筛选出符合条件的官方程序集和第三方程序集
//     // static List<string> excludedAssembly = new List<string>()
//     // {
//     //     "UnityEngine.XRModule",
//     //     "UnityEngine.WindModule",
//     //     "UnityEngine.VRModule",
//     //     "UnityEngine.VideoModule",
//     //     "UnityEngine.VFXModule",
//     //     "UnityEngine.VehiclesModule",
//     //     "UnityEngine.UnityWebRequestWWWModule",
//     //     "UnityEngine.UnityWebRequestTextureModule",
//     //     "UnityEngine.UnityWebRequestAudioModule",
//     //     "UnityEngine.UnityWebRequestAssetBundleModule",
//     //     "UnityEngine.UnityTestProtocolModule",
//     //     "UnityEngine.UnityCurlModule",
//     //     "UnityEngine.UnityConnectModule",
//     //     "UnityEngine.UnityAnalyticsModule",
//     //     "UnityEngine.UNETModule",
//     //     "UnityEngine.UmbraModule",
//     //     "UnityEngine.UIElementsNativeModule",
//     //     "UnityEngine.UIElementsModule",
//     //     "UnityEngine.TLSModule",
//     //     "UnityEngine.TilemapModule",
//     //     "UnityEngine.TerrainPhysicsModule",
//     //     "UnityEngine.TerrainModule",
//     //     "UnityEngine.SubsystemsModule",
//     //     "UnityEngine.SubstanceModule",
//     //     "UnityEngine.IMGUIModule",
//     //     "UnityEngine.HotReloadModule",
//     //     "UnityEngine.GridModule",
//     //     "UnityEngine.GIModule",
//     //     "UnityEngine.GameCenterModule",
//     //     "UnityEngine.DSPGraphModule",
//     //     "UnityEngine.ClothModule",
//     //     "UnityEngine.ARModule",
//     //     "UnityEngine.AIModule",
//     //     "UnityEngine.AccessibilityModule",
//     //     "UnityEngine.CrashReportingModule",
//     //     "Unity.CompilationPipeline.Common",
//     //     "Unity.Analytics",
//     //     "Editor",
//     //     "Test",
//     //     "Legacy",
//     //     "mscorlib",
//     //     "System.",
//     //     "Google.Protobuf",
//     //     "nunit",
//     //     "Mono.Security",
//     //     "ICSharpCode.",
//     //     "Newtonsoft.Json",
//     //     "ExCSS.",
//     //     "Framework.",
//     //     "System",
//     //     "Assembly-CSharp"
//     // };
//     // static bool isExcluded(Assembly ass)
//     // {
//     //    var name = ass.GetName().Name.ToLower();
//     //    for (int i = 0; i < excludedAssembly.Count; i++)
//     //    {
//     //        if (name.Contains(excludedAssembly[i].ToLower()))
//     //        {
//     //            return true;
//     //        }
//     //    }
//     //    return false;
//     // }

//     // [MenuItem("Tools/TestXX")]
//     // static void TestXX()
//     // {
//     //     //////////////////// 导出所有引擎和官方插件的可用类型
//     //     List<string> namespaces = new List<string>() // 在这里添加名字空间
//     //        {
//     //            "UnityEngine",
//     //            "UnityEngine.UI"
//     //        };

//     //     var assemblies = (  from assembly in AppDomain.CurrentDomain.GetAssemblies()
//     //                         where !(assembly.ManifestModule is System.Reflection.Emit.ModuleBuilder) && !isExcluded(assembly)
//     //                         select assembly );

//     //     // foreach(var ass in assemblies)
//     //     //     Debug.Log(ass.GetName());

//     //     var unityTypes = (  from ass in assemblies
//     //                         from type in ass.GetExportedTypes()
//     //                         where (type.Namespace != null && namespaces.Contains(type.Namespace)) && !isExcluded(type)
//     //                         && type.BaseType != typeof(MulticastDelegate) && !type.IsInterface && !type.IsEnum
//     //                         select type );

//     //     // foreach(var type in unityTypes)
//     //     //     Debug.Log(type);


//     //     string[] customAssemblys = new string[] {
//     //            "Assembly-CSharp",
//     //            "Framework.Core.Runtime",
//     //            "Framework.Gesture.Runtime",
//     //            "Framework.Network.Runtime",
//     //            "Framework.UI.Runtime"
//     //        };
//     //        var customTypes = (from assembly in customAssemblys.Select(s => Assembly.Load(s))
//     //                           from type in assembly.GetExportedTypes()
//     //                           where type.Namespace == null || !type.Namespace.StartsWith("XLua")
//     //                            && type.BaseType != typeof(MulticastDelegate) /*&& !type.IsInterface*/ // && !type.IsEnum
//     //                           select type);

//     //     foreach(var type in customTypes)
//     //     {
//     //         Debug.Log(type);
//     //     }
//     // }



//     //lua中要使用到C#库的配置，比如C#标准库，或者Unity API，第三方库等。
//     [LuaCallCSharp]
//     public static List<Type> LuaCallCSharp = new List<Type>() {
//                 typeof(System.Object),
//                 typeof(UnityEngine.Object),
//                 typeof(Vector2),
//                 typeof(Vector3),
//                 typeof(Vector4),
//                 typeof(Quaternion),
//                 typeof(Color),
//                 typeof(Ray),
//                 typeof(Bounds),
//                 typeof(Ray2D),
//                 typeof(Time),
//                 typeof(GameObject),
//                 typeof(Component),
//                 typeof(Behaviour),
//                 typeof(Transform),
//                 typeof(Resources),
//                 typeof(TextAsset),
//                 typeof(Keyframe),
//                 typeof(AnimationCurve),
//                 typeof(AnimationClip),
//                 typeof(MonoBehaviour),
//                 typeof(ParticleSystem),
//                 typeof(ParticleSystem.MinMaxCurve),
//                 typeof(ParticleSystem.MainModule),
//                 typeof(ParticleSystem.EmissionModule),
//                 typeof(ParticleSystem.ShapeModule),
//                 typeof(ParticleSystem.CollisionModule),
//                 typeof(ParticleSystem.TriggerModule),
//                 typeof(ParticleSystem.SubEmittersModule),
//                 typeof(ParticleSystem.TextureSheetAnimationModule),
//                 typeof(ParticleSystem.Particle),
//                 typeof(ParticleSystem.CollisionModule),
//                 typeof(ParticleSystem.Burst),
//                 typeof(ParticleSystem.MinMaxGradient),
//                 typeof(ParticleSystem.EmitParams),
//                 typeof(ParticleSystem.PlaybackState),
//                 typeof(ParticleSystem.Trails),
//                 typeof(ParticleSystem.ColliderData),
//                 typeof(ParticleSystem.VelocityOverLifetimeModule),
//                 typeof(ParticleSystem.LimitVelocityOverLifetimeModule),
//                 typeof(ParticleSystem.InheritVelocityModule),
//                 typeof(ParticleSystem.LifetimeByEmitterSpeedModule),
//                 typeof(ParticleSystem.ForceOverLifetimeModule),
//                 typeof(ParticleSystem.ColorOverLifetimeModule),
//                 typeof(ParticleSystem.ColorBySpeedModule),
//                 typeof(ParticleSystem.SizeOverLifetimeModule),
//                 typeof(ParticleSystem.SizeBySpeedModule),
//                 typeof(ParticleSystem.RotationOverLifetimeModule),
//                 typeof(ParticleSystem.RotationBySpeedModule),
//                 typeof(ParticleSystem.ExternalForcesModule),
//                 typeof(ParticleSystem.NoiseModule),
//                 typeof(ParticleSystem.LightsModule),
//                 typeof(ParticleSystem.TrailModule),
//                 typeof(ParticleSystem.CustomDataModule),
//                 typeof(SkinnedMeshRenderer),
//                 typeof(Renderer),
//                 typeof(Light),
//                 typeof(Mathf),
//                 typeof(System.Collections.Generic.List<int>),
//                 typeof(Action<string>),
//                 typeof(UnityEngine.Debug),
//                 typeof(WaitForSeconds)
//             };

//     //C#静态调用Lua的配置（包括事件的原型），仅可以配delegate，interface
//     [CSharpCallLua]
//     public static List<Type> CSharpCallLua = new List<Type>() {
//                 typeof(Action),
//                 typeof(Func<double, double, double>),
//                 typeof(Action<string>),
//                 typeof(Action<double>),
//                 typeof(Action<bool>),
//                 typeof(UnityEngine.Events.UnityAction),
//                 typeof(System.Collections.IEnumerator)
//             };

//     //黑名单
//     [BlackList]
//     public static List<List<string>> BlackList = new List<List<string>>()  {
//                 new List<string>(){"System.Xml.XmlNodeList", "ItemOf"},
//                 new List<string>(){"UnityEngine.WWW", "movie"},
//     #if UNITY_WEBGL
//                 new List<string>(){"UnityEngine.WWW", "threadPriority"},
//     #endif
//                 new List<string>(){"UnityEngine.Texture2D", "alphaIsTransparency"},
//                 new List<string>(){"UnityEngine.Security", "GetChainOfTrustValue"},
//                 new List<string>(){"UnityEngine.CanvasRenderer", "onRequestRebuild"},
//                 new List<string>(){"UnityEngine.Light", "areaSize"},
//                 new List<string>(){"UnityEngine.Light", "lightmapBakeType"},
//                 new List<string>(){"UnityEngine.Light", "SetLightDirty"},
//                 new List<string>(){"UnityEngine.Light", "shadowRadius"},
//                 new List<string>(){"UnityEngine.Light", "shadowAngle"},
//                 new List<string>(){"UnityEngine.Light", "shadowRadius"},
//                 new List<string>(){"UnityEngine.Light", "shadowAngle"},
//                 new List<string>(){"UnityEngine.WWW", "MovieTexture"},
//                 new List<string>(){"UnityEngine.WWW", "GetMovieTexture"},
//                 new List<string>(){"UnityEngine.AnimatorOverrideController", "PerformOverrideClipListCleanup"},
//     #if !UNITY_WEBPLAYER
//                 new List<string>(){"UnityEngine.Application", "ExternalEval"},
//     #endif
//                 new List<string>(){"UnityEngine.GameObject", "networkView"}, //4.6.2 not support
//                 new List<string>(){"UnityEngine.Component", "networkView"},  //4.6.2 not support
//                 new List<string>(){"System.IO.FileInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
//                 new List<string>(){"System.IO.FileInfo", "SetAccessControl", "System.Security.AccessControl.FileSecurity"},
//                 new List<string>(){"System.IO.DirectoryInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
//                 new List<string>(){"System.IO.DirectoryInfo", "SetAccessControl", "System.Security.AccessControl.DirectorySecurity"},
//                 new List<string>(){"System.IO.DirectoryInfo", "CreateSubdirectory", "System.String", "System.Security.AccessControl.DirectorySecurity"},
//                 new List<string>(){"System.IO.DirectoryInfo", "Create", "System.Security.AccessControl.DirectorySecurity"},
//                 new List<string>(){"UnityEngine.MonoBehaviour", "runInEditMode"},
//             };
}
