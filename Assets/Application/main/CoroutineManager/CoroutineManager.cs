using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;

namespace Application.Runtime
{
    public class CoroutineManager : SingletonMono<CoroutineManager>
    {
        protected override void OnDestroy()
        {
            StopAllCoroutines();
            base.OnDestroy();
        }
    }
}