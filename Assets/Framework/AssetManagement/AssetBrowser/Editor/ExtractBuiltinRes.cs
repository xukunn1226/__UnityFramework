using System.IO;
using UnityEngine;
using UnityEditor;

namespace Framework.AssetManagement.AssetBrowser
{
    internal class ExtractBuiltinRes
    {
        static private string s_commonPath      = "Assets/BuiltinRes";
        static private string s_pathOfMaterial  = s_commonPath + "/Material";
        static private string s_pathOfTexture   = s_commonPath + "/Texture";
        static private string s_pathOfSprite    = s_commonPath + "/Sprite";
        static private string s_pathOfMesh      = s_commonPath + "/Mesh";
        static private string s_pathOfFont      = s_commonPath + "/Font";

        [MenuItem("Assets/AssetBrowser/Extract Builtin Res")]
        static private void BuiltinExtract()
        {
            Directory.CreateDirectory(s_pathOfMaterial);
            Directory.CreateDirectory(s_pathOfTexture);
            Directory.CreateDirectory(s_pathOfSprite);
            Directory.CreateDirectory(s_pathOfMesh);
            Directory.CreateDirectory(s_pathOfFont);

            UnityEngine.Object[] builtinExtra = AssetDatabase.LoadAllAssetsAtPath("Resources/unity_builtin_extra");         // shader, material, texture2D, sprite, lightmapParameters
            foreach (var obj in builtinExtra)
            {
                string typeName = obj.GetType().Name;

                if(typeName == "Material")
                {
                    ExtractBuiltin_Generic(obj, s_pathOfMaterial, ".mat");
                }
                else if(typeName == "Mesh")
                {
                    ExtractBuiltin_Generic(obj, s_pathOfMesh, ".asset");
                }
                else if(typeName == "Sprite")
                {
                    ExtractBuiltin_Generic(obj, s_pathOfSprite, ".asset");
                }
                else if(typeName == "Texture2D")
                {
                    ExtractBuiltin_Texture2D(obj as Texture2D, s_pathOfTexture);
                }
            }

            UnityEngine.Object[] defaultResources = AssetDatabase.LoadAllAssetsAtPath("Library/unity default resources");   // monoScript, shader, computeShader, texture2D, material, font, mesh, sprite, GUISkin
            foreach (var obj in defaultResources)
            {
                string typeName = obj.GetType().Name;

                if(typeName == "Material")
                {
                    ExtractBuiltin_Generic(obj, s_pathOfMaterial, ".mat");
                }
                else if (typeName == "Mesh")
                {
                    ExtractBuiltin_Generic(obj, s_pathOfMesh, ".asset");
                }
                else if(typeName == "Font")
                {
                    ExtractBuiltin_Generic(obj, s_pathOfFont, ".fontsettings");
                }
                else if (typeName == "Sprite")
                {
                    ExtractBuiltin_Generic(obj, s_pathOfSprite, ".asset");
                }
                else if (typeName == "Texture2D")
                {
                    ExtractBuiltin_Texture2D(obj as Texture2D, s_pathOfTexture);
                }
            }

            AssetDatabase.Refresh();
        }

        static private void ExtractBuiltin_Generic(UnityEngine.Object asset, string directory, string extension)
        {
            UnityEngine.Object clone = Object.Instantiate(asset);

            AssetDatabase.CreateAsset(clone, FormatAssetPath(asset.name, directory, extension));
        }        

        static private void ExtractBuiltin_Texture2D(UnityEngine.Texture2D srcTex, string directory)
        {
            if (srcTex == null)
                throw new System.ArgumentNullException("srcTex", "srcTex == null");

            if(srcTex.format == TextureFormat.ARGB32 || srcTex.format == TextureFormat.RGBA32 || srcTex.format == TextureFormat.RGB24 || srcTex.format == TextureFormat.Alpha8)
            {
                Texture2D texCopy = new Texture2D(srcTex.width, srcTex.height, srcTex.format, srcTex.mipmapCount > 1);
                texCopy.LoadRawTextureData(srcTex.GetRawTextureData());

                byte[] data = texCopy.EncodeToPNG();
                File.WriteAllBytes(FormatAssetPath(srcTex.name, directory, ".png"), data);
            }
            else
            {
                Debug.LogWarningFormat("Failed to extract texture[{0}] because of format not support [{1}]", srcTex.name, srcTex.format.ToString());
            }
        }

        static private string FormatAssetPath(string filename, string directory, string extension)
        {
            return directory + "/" + filename + extension;
        }
    }
}