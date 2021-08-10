using UnityEngine;
using UnityEditor;
using System.IO;

 [InitializeOnLoad]
 public class PreloadSigningAlias
 {
    static PreloadSigningAlias()
    {
        DirectoryInfo dir = Directory.GetParent(UnityEngine.Application.dataPath);
        string path = dir.ToString() + @"/user.keystore";//自己签名的路径
        if(File.Exists(path)) {
            PlayerSettings.Android.keystoreName = path;
        }
        PlayerSettings.Android.keystorePass = "lilith";
        PlayerSettings.Android.keyaliasName = "user";     
        PlayerSettings.Android.keyaliasPass = "lilith";
    }
 }