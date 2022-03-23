using UnityEditor;
using UnityEngine;
using System.IO;

namespace Framework.AssetManagement.AssetBuilder
{
    public class AnimationPostprocessor : AssetPostprocessor
    {
        [MenuItem("Assets/美术资源工具/提取动画文件", false, 999)]
        static public void ExtractAnimClip()
        {
            if(Selection.assetGUIDs.Length == 0)
                return;

            UnityEditor.EditorUtility.DisplayProgressBar("Extract AnimClip", "", 0);
            for(int i = 0; i < Selection.assetGUIDs.Length; ++i)
            {
                ExtractAnimClip(AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[i]), true);
                UnityEditor.EditorUtility.DisplayProgressBar("Extract AnimClip", i + "/" + Selection.assetGUIDs.Length, i / (float)Selection.assetGUIDs.Length);
            }
            UnityEditor.EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
        }

        static public void ExtractAnimClip(string assetPath, bool bForceCreate = false)
        {
            UnityEngine.Object[] objs = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);
            if (objs != null && objs.Length != 0)
            {
                foreach (UnityEngine.Object obj in objs)
                {
                    AnimationClip clip = obj as AnimationClip;
                    if(clip == null)
                        continue;
                    
                    //提取 *.anim
                    string filePath = assetPath.Substring(0, assetPath.LastIndexOf("/") + 1) + Path.GetFileNameWithoutExtension(assetPath) + ".anim";
                    {
                        UnityEngine.Object clone = UnityEngine.Object.Instantiate(clip);

                        // 优化精度
                        AssetPostprocessorHelper.OptimizeAnim2(clone as AnimationClip, false);
                        // AssetPostprocessorHelper.OptimizeAnim(clone as AnimationClip);

                        AssetDatabase.CreateAsset(clone, filePath);
                        AssetDatabase.ImportAsset(filePath, ImportAssetOptions.ForceUpdate);
                        Debug.Log($"提取动画完成：{filePath}");
                    }
                }
            }
        }
    }    
}