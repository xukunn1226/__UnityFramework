using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CSObjectWrapEditor;
using XLua;
using Framework.AssetManagement.GameBuilder;

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

    //lua中要使用到C#库的配置，比如C#标准库，或者Unity API，第三方库等。
    [LuaCallCSharp]
    public static List<Type> LuaCallCSharp = new List<Type>() {
                typeof(System.Object),
                typeof(UnityEngine.Object),
                typeof(Vector2),
                typeof(Vector3),
                typeof(Vector4),
                typeof(Quaternion),
                typeof(Color),
                typeof(Ray),
                typeof(Bounds),
                typeof(Ray2D),
                typeof(Time),
                typeof(GameObject),
                typeof(Component),
                typeof(Behaviour),
                typeof(Transform),
                typeof(Resources),
                typeof(TextAsset),
                typeof(Keyframe),
                typeof(AnimationCurve),
                typeof(AnimationClip),
                typeof(MonoBehaviour),
                typeof(ParticleSystem),
                typeof(SkinnedMeshRenderer),
                typeof(Renderer),
                typeof(Light),
                typeof(Mathf),
                typeof(System.Collections.Generic.List<int>),
                typeof(Action<string>),
                typeof(UnityEngine.Debug),
                typeof(WaitForSeconds)
            };

    //C#静态调用Lua的配置（包括事件的原型），仅可以配delegate，interface
    [CSharpCallLua]
    public static List<Type> CSharpCallLua = new List<Type>() {
                typeof(Action),
                typeof(Func<double, double, double>),
                typeof(Action<string>),
                typeof(Action<double>),
                typeof(Action<bool>),
                typeof(UnityEngine.Events.UnityAction),
                typeof(System.Collections.IEnumerator)
            };

    //黑名单
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
                new List<string>(){"UnityEngine.Light", "SetLightDirty"},
                new List<string>(){"UnityEngine.Light", "shadowRadius"},
                new List<string>(){"UnityEngine.Light", "shadowAngle"},
                new List<string>(){"UnityEngine.Light", "shadowRadius"},
                new List<string>(){"UnityEngine.Light", "shadowAngle"},
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
}
