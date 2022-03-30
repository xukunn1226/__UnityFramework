using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.AssetBuilder
{
    public interface IAssetProcessor
    {
        bool IsMatch(string filename, string extension);
        string Execute(string assetPath);
    }
}