using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Framework.AssetManagement.AssetBuilder
{
    public class AllAssetsPostprocessor : AssetPostprocessor
    {
        // static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        // {
        //     if(UnityEngine.Application.isBatchMode)
        //         return;

        //     for (int i = 0; i < importedAssets.Length; ++i)
        //     {
        //         if(importedAssets[i].EndsWith(".anim"))
        //         {
        //             // OptimizeAnim(importedAssets[i]);
        //             // Debug.Log($"import animation clip: {importedAssets[i]}");
        //         }
        //     }
        // }
    }
}