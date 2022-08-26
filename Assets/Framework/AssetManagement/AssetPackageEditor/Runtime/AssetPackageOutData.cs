using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Framework.AssetManagement.AssetPackageEditor.Runtime
{

    [System.Serializable]
    public class AssetPackageOutData
    {
        Dictionary<string, AssetPackageItem> packages = new Dictionary<string, AssetPackageItem>();
    }

    [System.Serializable]
    public class AssetPackageItem
    {
        string packageID;
        List<string> Path = new List<string>();
    }

}