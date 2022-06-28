using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    [System.Serializable]
    public class CustomManifest
    {
        [SerializeField]
        Dictionary<string, string>          m_AssetInfos    = new Dictionary<string, string>();             // key:     identifier，建议使用assetPath
                                                                                                            // value:   bundle name
        [SerializeField]
        Dictionary<string, List<string>>    m_BundleInfos   = new Dictionary<string, List<string>>();       // key:     bundle name
                                                                                                            // value:   dependencies bundle name
    }
}