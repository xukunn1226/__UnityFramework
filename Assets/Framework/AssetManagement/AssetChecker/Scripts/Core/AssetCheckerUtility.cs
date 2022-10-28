using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.AssetChecker
{
    static public class AssetCheckerUtility
    {
        static public string TrimProjectFolder(string path)
        {
            if(path.StartsWith(UnityEngine.Application.dataPath, System.StringComparison.CurrentCultureIgnoreCase))
            {
                return path.Substring(UnityEngine.Application.dataPath.Length - 6);
            }
            return path;
        }
    }
}