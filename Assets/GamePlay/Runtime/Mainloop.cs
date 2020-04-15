using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.Runtime;

public class Mainloop : MonoBehaviour
{
    public LoaderType   m_LoaderType;

    private void Awake()
    {
        AssetManager.Init(m_LoaderType);
    }

    private void OnDestroy()
    {
        AssetManager.Uninit();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
