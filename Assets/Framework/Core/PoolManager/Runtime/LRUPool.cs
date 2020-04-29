using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Framework.Cache
{
    public class LRUPool : MonoPoolBase
    {
        public Type m_Type;
        
        public override int countAll { get; }

        public override int countOfUsed { get; }

        public override int countOfUnused { get; }

        public override IPooledObject Get() { return null; }

        public override void Return(IPooledObject item) { }

        public override void Clear() { }

        public override void Trim() { }
    }
}