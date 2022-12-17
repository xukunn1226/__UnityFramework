using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    public class AssetDriver : MonoBehaviour
    {
        private void Update()
        {
            AssetManagement.Update();
        }
    }
}