using UnityEditor;
using UnityEngine;

namespace Application.Editor
{
    public class MaterialPostprocessor : AssetPostprocessor
    {
        // static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        // {
        //     if(Application.isBatchMode)
        //         return;
                
        //     foreach(var assetPath in importedAssets)
        //     {
        //         if(assetPath.EndsWith(".mat"))
        //         {
        //             Material mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
        //             if(mat != null && !mat.enableInstancing)
        //             {
        //                 mat.enableInstancing = true;
        //                 EditorUtility.SetDirty(mat);
        //             }
        //         }
        //     }
        // }
    }
}
