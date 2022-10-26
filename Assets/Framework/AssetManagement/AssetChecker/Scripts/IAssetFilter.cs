using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.AssetChecker
{
    public interface IAssetFilter
    {
        List<string> Run();
    }
}