using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

namespace Framework.AssetBuilder
{
    internal class AssetNameChecker : AssetPostprocessor
    {
        static private string k_Pattern = @"\s|[#\$%\^&()-\+{}\?*|\[\]]";
        static private Regex k_NameRegex = new Regex(k_Pattern, RegexOptions.IgnoreCase);

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string assetPath in importedAssets)
            {
                // 仅检查指定目录下的资源名
                if (AssetBuilderUtil.IsPassByWhiteList(assetPath))                
                {
                    if(!CheckNameRuler(assetPath))
                    {
                        string message = string.Format("{0} 命名不能包含如下特殊字符{1},请修正", assetPath, k_Pattern.Replace("\\", ""));
                        if(AssetBuilderSetting.GetDefault().ForceDisplayDialogWhenAssetNameNotMetSpec)
                            EditorUtility.DisplayDialog("错误", message, "OK");
                        Debug.LogError(message, AssetDatabase.LoadAssetAtPath<Object>(assetPath));
                    }
                }
            }
        }

        /// <summary>
        /// 是否符合命名规则
        /// </summary>
        /// <param name="name"></param>
        /// <returns>true: 符合；false: 不符合</returns>
        static internal bool CheckNameRuler(string name)
        {
            return !k_NameRegex.IsMatch(name);
        }
    }
}