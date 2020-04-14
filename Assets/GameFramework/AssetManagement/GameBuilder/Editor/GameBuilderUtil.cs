using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace AssetManagement.GameBuilder
{
    static internal class GameBuilderUtil
    {
        static public string kDefaultSettingPath = "Assets/AssetManagement/GameBuilder/Data";

        //[MenuItem("Assets Management/Create BundleBuilder Setting", false, 21)]
        //static private void CreateBundlesSetting()
        //{
        //    BundleBuilderSetting asset = Utility.GetOrCreateEditorConfigObject<BundleBuilderSetting>(kDefaultSettingPath);
        //    if (asset != null)
        //        Selection.activeObject = asset;
        //}

        //[MenuItem("Assets Management/Create PlayerBuilder Setting", false, 22)]
        //static private void CreatePlayerSetting()
        //{
        //    PlayerBuilderSetting asset = Utility.GetOrCreateEditorConfigObject<PlayerBuilderSetting>(kDefaultSettingPath);
        //    if (asset != null)
        //        Selection.activeObject = asset;
        //}

        //[MenuItem("Assets Management/Create GameBuilder Setting", false, 23)]
        //static private void CreateGameSetting()
        //{
        //    GameBuilderSetting asset = Utility.GetOrCreateEditorConfigObject<GameBuilderSetting>(kDefaultSettingPath);
        //    if (asset != null)
        //        Selection.activeObject = asset;
        //}

        public static BuildTargetGroup GetBuildTargetGroup(BuildTarget buildTarget)
        {
            switch (buildTarget)
            {
                case BuildTarget.iOS:
                    return BuildTargetGroup.iOS;

                case BuildTarget.Android:
                    return BuildTargetGroup.Android;

                case BuildTarget.tvOS:
                    return BuildTargetGroup.tvOS;

                case BuildTarget.XboxOne:
                    return BuildTargetGroup.XboxOne;

                case BuildTarget.PS4:
                    return BuildTargetGroup.PS4;

                case BuildTarget.WSAPlayer:
                    return BuildTargetGroup.WSA;

                case BuildTarget.WebGL:
                    return BuildTargetGroup.WebGL;

                default:
                    return BuildTargetGroup.Standalone;
            }
        }
    }
}