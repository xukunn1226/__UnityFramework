using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public class AdjustedPrefabObjectPool : MonoPoolBase
    {
        public override IPooledObject Get()
        {
            return null;
        }

        public override void Return(IPooledObject item)
        {

        }

        public override void Clear()
        { }
    }
}