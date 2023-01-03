using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Framework.AssetManagement;

namespace Framework.AssetManagement.AssetBundleCollector
{
    /// <summary>
    /// 以文件路径为资源包名
    /// 例如："Assets/Res/UI/Checker.png" -> "assets_res_ui_checker.bundle"
    /// </summary>
    public class PackFile : IPackRule
    {
        string IPackRule.GetBundleName(PackRuleData data)
        {
            string bundleName = Runtime.StringUtility.RemoveExtension(data.AssetPath);
            return EditorTools.GetRegularPath(bundleName).Replace('/', '_');
        }
    }

    /// <summary>
    /// 以父类文件夹路径作为资源包名
    /// 例如："Assets/Res/UI/Backpack/main.prefab" -> "assets_res_ui_backpack.bundle"
    /// </summary>
    public class PackDirectory : IPackRule
    {
        string IPackRule.GetBundleName(PackRuleData data)
        {
            string bundleName = System.IO.Path.GetDirectoryName(data.AssetPath);
            return EditorTools.GetRegularPath(bundleName).Replace('/', '_');
        }
    }

    /// <summary>
    /// 以收集器路径下顶级文件夹为资源包名
    /// 注意：顶级文件夹下的所有文件打进一个资源包
    /// 例如：收集器路径为："Assets/Res/Enviromnent/Building"
    /// 例如："Assets/Res/Environment/Building/House/House.prefab" -> "assets_res_environment_building_house.bundle"
    /// 例如："Assets/Res/Environment/Building/House/House_Albedo.png" -> "assets_res_environment_building_house.bundle"
    /// 例如："Assets/Res/Environment/Building/House/House_Normal.png" -> "assets_res_environment_building_house.bundle"
    /// 例如："Assets/Res/Environment/Building/House/House_Material.material" -> "assets_res_environment_building_house.bundle"
    /// </summary>
    public class PackTopDirectory : IPackRule
    {
        string IPackRule.GetBundleName(PackRuleData data)
        {
            string collectPath = EditorTools.GetRegularPath(System.IO.Path.GetDirectoryName(data.CollectPath));
            string assetPath = data.AssetPath.Replace(collectPath, string.Empty).TrimStart('/');
            string[] splits = assetPath.Split('/');
            if(splits.Length > 0)
            {
                string bundleName = $"{collectPath}/{splits[0]}";
                return EditorTools.GetRegularPath(bundleName.TrimEnd('/')).Replace('/', '_');
            }
            throw new System.Exception($"Not found root directory : {assetPath}");
        }
    }

    /// <summary>
    /// 以收集器路径作为资源包
    /// </summary>
    public class PackCollector : IPackRule
    {
        string IPackRule.GetBundleName(PackRuleData data)
        {
            string collectPath = EditorTools.GetRegularPath(System.IO.Path.GetDirectoryName(data.CollectPath));
            return EditorTools.GetRegularPath(collectPath).Replace('/', '_');
        }
    }

    /// <summary>
    /// 打包原生文件
    /// 注意：原生文件不可有任何依赖关系
    /// </summary>
    public class PackRawFile : IPackRule
    {
        string IPackRule.GetBundleName(PackRuleData data)
        {
            string extension = Runtime.StringUtility.RemoveFirstChar(System.IO.Path.GetExtension(data.AssetPath));
            if (extension == EditorDefine.EAssetFileExtension.unity.ToString() || extension == EditorDefine.EAssetFileExtension.prefab.ToString() ||
                extension == EditorDefine.EAssetFileExtension.mat.ToString() || extension == EditorDefine.EAssetFileExtension.controller.ToString() ||
                extension == EditorDefine.EAssetFileExtension.fbx.ToString() || extension == EditorDefine.EAssetFileExtension.anim.ToString() ||
                extension == EditorDefine.EAssetFileExtension.shader.ToString())
            {
                throw new System.Exception($"{nameof(PackRawFile)} is not support file estension : {extension}");
            }

            // 注意：原生文件只支持无依赖关系的资源
            string[] depends = AssetDatabase.GetDependencies(data.AssetPath, true);
            if (depends.Length != 1)
                throw new System.Exception($"{nameof(PackRawFile)} is not support estension : {extension}");

            string bundleName = data.AssetPath;
            return EditorTools.GetRegularPath(bundleName).Replace('/', '_').Replace('.', '_');
        }
    }
}