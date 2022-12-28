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
        [MenuItem("Tools/Assets Management/��Դ�����Թ���")]
        private static void OpenWindow()
        {
            var window = GetWindow<AssetBundleDebuggerWindow>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 600);
            window.titleContent = new GUIContent("��Դ�����Թ���");
        }

        //protected override OdinMenuTree BuildMenuTree()
        //{
        //    var tree = new OdinMenuTree(false);
        //    return tree;
        //}
    }
}