using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.Runtime;

public class NewBehaviourScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        AssetManager.InstantiatePrefab("assets/res/players/symbol.prefab");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
