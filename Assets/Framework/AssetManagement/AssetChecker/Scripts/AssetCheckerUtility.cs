using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Framework.AssetManagement.AssetChecker
{    
    static public class AssetCheckerUtility
    {
        static public string TrimProjectFolder(string path)
        {
            if(path.StartsWith(UnityEngine.Application.dataPath, System.StringComparison.CurrentCultureIgnoreCase))
            {
                return path.Substring(UnityEngine.Application.dataPath.Length - 6);
            }
            return path;
        }
    }










    public class BaseFuncComponentParam
    {
        public string ComponentTypeName = "";
        public string ComponentParamJson = "";
    }

    public class BaseFuncComponentParam<T> : BaseFuncComponentParam
    {
        static public T CreateComponent(BaseFuncComponentParam param)
        {
            var subTypes = TypeCache.GetTypesDerivedFrom(typeof(T));
            foreach (var subType in subTypes)
            {
                if (subType.Name == param.ComponentTypeName)
                {
                    var component = Activator.CreateInstance(subType);
                    if (component != null)
                    {
                        var paramJson = param.ComponentParamJson;
                        if (!string.IsNullOrEmpty(paramJson))
                        {
                            var paramObj = JsonUtility.FromJson(paramJson, subType);
                            if (paramObj != null)
                            {
                                var paramType = paramObj.GetType();
                                var fields = paramType.GetFields();
                                foreach (var field in fields)
                                {
                                    var value = field.GetValue(paramObj);
                                    field.SetValue(component, value);
                                }
                            }
                        }
                    }

                    return (T)component;
                }
            }

            return default(T);
        }

        static public BaseFuncComponentParam CreateParam(T component)
        {
            var param = new BaseFuncComponentParam();
            param.ComponentTypeName = component.GetType().Name;
            param.ComponentParamJson = EditorJsonUtility.ToJson(component);
            param.ComponentParamJson = Newtonsoft.Json.JsonConvert.SerializeObject(component);
            return param;
        }
    }


    //static public class AssetChecker_Test
    //{
    //    [UnityEditor.MenuItem("Tools/AssetChecker_Test/Foo")]
    //    static private void Foo()
    //    {
    //        AssetFilter_Path filter = new AssetFilter_Path();
    //        filter.input = new List<string> { "Assets/Resources" };
    //        filter.pattern = @"s/Wind";
    //        List<string> ret = filter.DoFilter();

    //        int count = 0;
    //        foreach (string s in ret)
    //        {
    //            Debug.Log(string.Format($"{count++}: {s}"));
    //        }
    //    }

    //    [UnityEditor.MenuItem("Tools/AssetChecker_Test/Foo2")]
    //    static private void Foo2()
    //    {
    //        AssetFilter_Filename filter = new AssetFilter_Filename();
    //        filter.input = new List<string> { "Assets/Resources" };
    //        filter.nameFilter = @"a*(?!(meta)$)";
    //        filter.typeFilter = AssetFilter_Filename.UnityType.Object;
    //        List<string> ret = filter.DoFilter();

    //        int count = 0;
    //        foreach (string s in ret)
    //        {
    //            Debug.Log(string.Format($"{count++}: {s}"));
    //        }
    //    }

    //    [UnityEditor.MenuItem("Tools/AssetChecker_Test/TestSerialize")]
    //    static private void TestSerialize()
    //    {
    //        AssetProcessor_Mesh processor = new AssetProcessor_Mesh();
    //        processor.threshold = 312;

    //        BaseFuncComponentParam param = BaseFuncComponentParam<AssetProcessor_Mesh>.CreateParam(processor);
    //        Debug.Log($"{param.ComponentParamJson}");

    //        AssetProcessor_Mesh p = (AssetProcessor_Mesh)BaseFuncComponentParam<IAssetProcessor>.CreateComponent(param);
    //        Debug.Log($"p: {p.threshold}");
    //    }

    //    [UnityEditor.MenuItem("Tools/AssetChecker_Test/Test Checker Serialize")]
    //    static private void TestCheckerSerialize()
    //    {
    //        AssetCheckerOverview overview = new AssetCheckerOverview();

    //        AssetChecker checker = new AssetChecker();
    //        checker.Desc = "≤‚ ‘Checker";
    //        ((AssetFilter_Path)checker.PathFilter.filter).input = new List<string>() { "Assets/Resources" };
    //        checker.Processor = new AssetProcessor_Mesh();
    //        ((AssetProcessor_Mesh)checker.Processor).threshold = 111;

    //        overview.Add(checker);

    //        AssetCheckerOverview.Save(overview);

    //        AssetCheckerOverview ov = AssetCheckerOverview.GetOrCreate();
    //    }
    //}
}