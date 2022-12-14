using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    public class AssetOperationHandle : OperationHandleBase
    {
        public AssetOperationHandle(ProviderBase provider) : base(provider)
        { }

        public override void InvokeCallback()
        {
        }
    }
}