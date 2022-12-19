using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.Runtime;

public class AM_Startup : MonoBehaviour
{
    private void OnEnable()
    {
        StartCoroutine(AssetManagerEx.Initialize());
    }

    private void OnDisable()
    {
        AssetManagerEx.Destroy();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
