using UnityEditor;
using UnityEngine;
using System.Reflection;

namespace Framework.AssetManagement.AssetBuilder
{
    public class TexturePostprocessor : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            TextureImporter ti = assetImporter as TextureImporter;

            // 设置平台无关属性
            TextureImporterSettings importerSettings = new TextureImporterSettings();
            ti.ReadTextureSettings(importerSettings);
            // importerSettings.sRGBTexture = true;
            // importerSettings.mipmapEnabled = true;
            ti.SetTextureSettings(importerSettings);

            int width, height;
            GetTextureOriginalSize(ti, out width, out height);

            bool shouldScale = false;
            if(Mathf.IsPowerOfTwo(width) && Mathf.IsPowerOfTwo(height))
            {
                shouldScale = true;
            }

            Debug.Log($"width: {width}  height:{height}");

            // standalone
            TextureImporterPlatformSettings standalone_platformSetting = ti.GetPlatformTextureSettings("Standalone");
            standalone_platformSetting.overridden = true;
            standalone_platformSetting.allowsAlphaSplitting = false;
            standalone_platformSetting.compressionQuality = 50;
            standalone_platformSetting.maxTextureSize = shouldScale ? Mathf.Max(width, height) >> 1 : 4096;
            ti.SetPlatformTextureSettings(standalone_platformSetting);

            // android
            TextureImporterPlatformSettings android_platformSetting = ti.GetPlatformTextureSettings("Android");
            android_platformSetting.overridden = true;
            android_platformSetting.allowsAlphaSplitting = false;
            android_platformSetting.compressionQuality = 50;
            android_platformSetting.maxTextureSize = shouldScale ? Mathf.Max(width, height) >> 1 : 4096;
            ti.SetPlatformTextureSettings(android_platformSetting);

            // ios
            TextureImporterPlatformSettings ios_platformSetting = ti.GetPlatformTextureSettings("iPhone");
            ios_platformSetting.overridden = true;
            ios_platformSetting.allowsAlphaSplitting = false;
            ios_platformSetting.compressionQuality = 50;
            ios_platformSetting.maxTextureSize = shouldScale ? Mathf.Max(width, height) >> 1 : 4096;
            ti.SetPlatformTextureSettings(ios_platformSetting);
        }

        private static void GetTextureOriginalSize(TextureImporter ti, out int width, out int height)
        {
            if (ti == null)
            {
                width = 0;
                height = 0;
                return;
            }

            object[] args = new object[2] { 0, 0 };
            MethodInfo mi = typeof(TextureImporter).GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
            mi.Invoke(ti, args);

            width = (int)args[0];
            height = (int)args[1];
        }
    }
}

