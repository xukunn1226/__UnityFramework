using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    public class InitializationOperation : AsyncOperationBase
    {
        internal override void Start()
        { }

        internal override void Update()
        {
            status = EOperationStatus.Succeed;
        }
    }
}