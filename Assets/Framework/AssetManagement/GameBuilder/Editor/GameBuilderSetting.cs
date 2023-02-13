using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Framework.Core;

namespace Framework.AssetManagement.AssetEditorWindow
{
    /// <summary>
    /// app打包配置数据
    /// 对bundle和player配置数据集合的封装，在此基础上可配置出适配开发、发布、安卓、iOS等各种情况下的版本
    /// </summary>
    public class GameBuilderSetting : ScriptableObject
    {
        public string               displayName;
        public BuildTarget          buildTarget;
        public string               packageVersion = "0.0.1";
        public BundleBuilderSetting bundleSetting;
        public PlayerBuilderSetting playerSetting;

        public enum BuildMode
        {
            BundlesAndPlayer,
            Bundles,
            Player,
        }
        public BuildMode            buildMode;
    }
}