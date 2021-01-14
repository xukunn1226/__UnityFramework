using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;

public class NewBehaviourScript : MonoBehaviour
{
    void Start()
    {
        GlobalConfigManager.Init();
        Actor actor = new Actor();
        actor.Load();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
