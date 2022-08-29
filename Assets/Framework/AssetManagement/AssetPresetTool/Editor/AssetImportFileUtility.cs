using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Framework.AssetManagement.AssetProcess
{
    public class AssetFileUtility 
    {
        public static List<string> GetMatchFiles(List<string> Paths, List<FilterMode> modes,List<bool> include)
        {
            List<string> res = new List<string>();
            if (Paths != null && Paths.Count > 0)
            {
                for (int i = Paths.Count - 1; i >= 0; i--)
                {
                    var r = Paths[i];
                    if (string.IsNullOrEmpty(r) || r == "")
                    {
                        Paths.RemoveAt(i);
                        include.RemoveAt(i);
                    }
                }
                if (Paths.Count == 0)
                {
                    return res;
                }
                // todo
                for (int z = 0; z < Paths.Count; z++)
                {
                    string[] subPath =new string[1];
                    subPath[0] = Paths[z];
                    string currentPath = Paths[z];
                    string assetTypePattern = "";
                    var assetTypeFitlerModes = from mode in modes where mode.isAssetFitlerMode() select mode.applyStr;
                    if (assetTypeFitlerModes !=null)
                    {
                        string typeFiler = assetTypeFitlerModes.FirstOrDefault(mode => !string.IsNullOrEmpty(mode));
                        if (!string.IsNullOrEmpty(typeFiler))
                        {
                            assetTypePattern = $"t:{typeFiler}";
                        }
                    }
                    
                    string[] GUIDs = AssetDatabase.FindAssets(assetTypePattern, subPath);
                    for (int i = 0; i < GUIDs.Length; i++)
                    {
                        string assetPath = AssetDatabase.GUIDToAssetPath(GUIDs[i]);
                        string assetName = Path.GetFileName(assetPath);
                        bool match = true;
                        if (include[z])
                        {
                            for (int a = 0; a < modes.Count; a++)
                            {
                                match = modes[a].isMatch(assetName);
                                if (!match) break;
                            }
                        }
                        else if (!include[z])
                        {
                            match = true;
                            var subFolders = AssetDatabase.GetSubFolders(currentPath);
                            foreach (var subFolder in subFolders)
                            {
                                if (assetPath.Contains(subFolder))
                                    match = false;
                            }
                        }
                        if (match)
                        {
                            res.Add(assetPath);
                        }
                    }

                }
            }
            return res;
        }
    }
}
