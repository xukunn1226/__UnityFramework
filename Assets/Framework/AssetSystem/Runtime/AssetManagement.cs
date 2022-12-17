using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    static public class AssetManagement
    {
        static private bool         s_Init;
        static private GameObject   s_Driver;

        static public void Initialize()
        {
            if (s_Init)
                throw new System.Exception($"AssetManagement has already init..");

            s_Init = true;
            s_Driver = new GameObject($"AssetManagement");
            s_Driver.AddComponent<AssetDriver>();
            UnityEngine.Object.DontDestroyOnLoad(s_Driver);
            Debug.Log("AssetManagement initialize!");

            // init other system
            AsyncOperationSystem.Initialize();
        }

        static public void Destroy()
        {
            if (!s_Init)
                return;

            s_Init = false;

            // destroy other system
            AsyncOperationSystem.Destroy();

            if (s_Driver != null)
                UnityEngine.Object.Destroy(s_Driver);
            Debug.Log("AssetManagement destroy!");
        }

        static public void Update()
        {
            if (!s_Init)
                return;

            AsyncOperationSystem.Update();
        }
    }
}