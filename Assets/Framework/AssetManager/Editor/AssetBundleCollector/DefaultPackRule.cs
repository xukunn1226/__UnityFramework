using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Framework.AssetManagement;

namespace Framework.AssetManagement.AssetBundleCollector
{
    /// <summary>
    /// ���ļ�·��Ϊ��Դ����
    /// ���磺"Assets/Res/UI/Checker.png" -> "assets_res_ui_checker.bundle"
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
    /// �Ը����ļ���·����Ϊ��Դ����
    /// ���磺"Assets/Res/UI/Backpack/main.prefab" -> "assets_res_ui_backpack.bundle"
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
    /// ���ռ���·���¶����ļ���Ϊ��Դ����
    /// ע�⣺�����ļ����µ������ļ����һ����Դ��
    /// ���磺�ռ���·��Ϊ��"Assets/Res/Enviromnent/Building"
    /// ���磺"Assets/Res/Environment/Building/House/House.prefab" -> "assets_res_environment_building_house.bundle"
    /// ���磺"Assets/Res/Environment/Building/House/House_Albedo.png" -> "assets_res_environment_building_house.bundle"
    /// ���磺"Assets/Res/Environment/Building/House/House_Normal.png" -> "assets_res_environment_building_house.bundle"
    /// ���磺"Assets/Res/Environment/Building/House/House_Material.material" -> "assets_res_environment_building_house.bundle"
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
    /// ���ռ���·����Ϊ��Դ��
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
    /// ���ԭ���ļ�
    /// ע�⣺ԭ���ļ��������κ�������ϵ
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

            // ע�⣺ԭ���ļ�ֻ֧����������ϵ����Դ
            string[] depends = AssetDatabase.GetDependencies(data.AssetPath, true);
            if (depends.Length != 1)
                throw new System.Exception($"{nameof(PackRawFile)} is not support estension : {extension}");

            string bundleName = data.AssetPath;
            return EditorTools.GetRegularPath(bundleName).Replace('/', '_').Replace('.', '_');
        }
    }
}