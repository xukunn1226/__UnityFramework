using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Framework.AssetManagement.Runtime;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using Sirenix.OdinInspector;

namespace Framework.AssetManagement.AssetBundleDebugger
{
    public class AssetBundleDebuggerWindow : OdinEditorWindow
    {
        [MenuItem("Tools/Assets Management/资源包调试工具")]
        private static void OpenWindow()
        {
            var window = GetWindow<AssetBundleDebuggerWindow>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 600);
            window.titleContent = new GUIContent("资源包调试工具");
        }

        //protected override OdinMenuTree BuildMenuTree()
        //{
        //    var tree = new OdinMenuTree(false);
        //    return tree;
        //}
    }
}