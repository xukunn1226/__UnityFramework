using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public interface ILifeTime
    {
        float LifeTime  { get; set; }

        float Speed     { get; set; }
    }
}