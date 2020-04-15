﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Cache
{
    public interface ILifeTime
    {
        float LifeTime  { get; set; }

        float Speed     { get; set; }
    }
}