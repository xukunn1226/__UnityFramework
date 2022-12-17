using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.Runtime;

public class AM_Startup : MonoBehaviour
{
    private void OnEnable()
    {
        AssetManagement.Initialize();
    }

    private void OnDisable()
    {
        AssetManagement.Destroy();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
