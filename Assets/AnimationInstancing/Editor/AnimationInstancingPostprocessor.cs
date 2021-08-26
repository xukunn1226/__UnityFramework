using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using AnimationInstancingModule.Runtime;

namespace AnimationInstancingModule.Editor
{
    public class AnimationTexturePostprocessor : AssetPostprocessor
    {
        // 模型强制打开isReadable
        void OnPreprocessModel()
        {
            if(UnityEngine.Application.isBatchMode)
                return;
            
            string path = assetPath.Substring(0, AnimationInstancingGenerator.s_AnimationInstancingRoot.Length);
            if(string.Compare(path, AnimationInstancingGenerator.s_AnimationInstancingRoot, true) != 0)
                return;

            ModelImporter mi = assetImporter as ModelImporter;
            mi.isReadable = true;
        }

        // void OnPreprocessTexture()
        // {
        //     if(UnityEngine.Application.isBatchMode)
        //         return;

        //     string path = assetPath.Substring(0, AnimationInstancingGenerator.s_AnimationDataPath.Length);
        //     if(string.Compare(path, AnimationInstancingGenerator.s_AnimationDataPath, true) != 0)
        //         return;
            
        //     TextureImporter ti = assetImporter as TextureImporter;
        //     // 设置平台无关属性
        //     TextureImporterSettings importerSettings = new TextureImporterSettings();
        //     ti.ReadTextureSettings(importerSettings);
        //     {
        //         importerSettings.textureType = TextureImporterType.Default;
        //         importerSettings.textureShape = TextureImporterShape.Texture2D;
        //         importerSettings.alphaSource = TextureImporterAlphaSource.FromInput;
        //         importerSettings.sRGBTexture = false;
        //         importerSettings.npotScale = TextureImporterNPOTScale.None;
        //         importerSettings.readable = true;
        //         importerSettings.streamingMipmaps = false;
        //         importerSettings.mipmapEnabled = false;
        //         importerSettings.filterMode = FilterMode.Point;
        //     }
        //     ti.SetTextureSettings(importerSettings);

        //     // standalone
        //     TextureImporterPlatformSettings standalone_platformSetting = ti.GetPlatformTextureSettings("Standalone");
        //     {
        //         standalone_platformSetting.overridden = true;
        //         standalone_platformSetting.allowsAlphaSplitting = false;
        //         standalone_platformSetting.compressionQuality = 50;
        //         standalone_platformSetting.format = TextureImporterFormat.RGBAHalf;
        //     }
        //     ti.SetPlatformTextureSettings(standalone_platformSetting);

        //     // android
        //     TextureImporterPlatformSettings android_platformSetting = ti.GetPlatformTextureSettings("Android");
        //     {
        //         android_platformSetting.overridden = true;
        //         android_platformSetting.allowsAlphaSplitting = false;
        //         android_platformSetting.compressionQuality = 50;
        //         android_platformSetting.format = TextureImporterFormat.RGBAHalf;
        //     }
        //     ti.SetPlatformTextureSettings(android_platformSetting);

        //     // ios
        //     TextureImporterPlatformSettings ios_platformSetting = ti.GetPlatformTextureSettings("iPhone");
        //     {
        //         ios_platformSetting.overridden = true;
        //         ios_platformSetting.allowsAlphaSplitting = false;
        //         ios_platformSetting.compressionQuality = 50;
        //         ios_platformSetting.format = TextureImporterFormat.RGBAHalf;
        //     }
        //     ti.SetPlatformTextureSettings(ios_platformSetting);
        // }
    }
}