using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Core.Tests
{
    public class TestGlobalConfigManager : MonoBehaviour
    {
        void Start()
        {
            GlobalConfigManager.Init();
            TestActorConfig actor = new TestActorConfig();
            actor.Load();
        }
    }
}