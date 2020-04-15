using UnityEngine;
using UnityEditor;

namespace Framework.AssetBuilder
{
    public class ModelPostprocessor : AssetPostprocessor
    {
        void OnPreprocessModel()
        {
            ModelImporter modelImporter = assetImporter as ModelImporter;
#if UNITY_2019_1_OR_NEWER
            modelImporter.materialImportMode = ModelImporterMaterialImportMode.None;
#else
            modelImporter.importMaterials = false;
#endif
        }

        void OnPostprocessModel(GameObject root)
        {
            ModelImporter modelImporter = assetImporter as ModelImporter;
#if UNITY_2019_1_OR_NEWER
            if(modelImporter.materialImportMode == ModelImporterMaterialImportMode.None)
#else
            if (!modelImporter.importMaterials)
#endif
            {
                // 清空默认的内置引用资源
                Renderer[] rdrs = root.GetComponentsInChildren<Renderer>();
                for (int i = 0; i < rdrs.Length; ++i)
                {
                    Material[] mats = new Material[rdrs[i].sharedMaterials.Length];
                    rdrs[i].sharedMaterials = mats;
                }
            }
        }
    }
}
