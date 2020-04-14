using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AssetManagement.AssetBrowser
{
    /// <summary>
    /// 修复特效冗余mesh
    /// </summary>
    class FixParticleSystemRender : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (Application.isBatchMode)
                return;

            for (int i = 0; i < importedAssets.Length; ++i)
            {
                AssetBrowserUtil.FixRedundantMeshOfParticleSystemRender(importedAssets[i], false);     // EditorUtility.SetDirty will trigger OnPostprocessAllAssets callback, so disable it
            }
        }
    }
}