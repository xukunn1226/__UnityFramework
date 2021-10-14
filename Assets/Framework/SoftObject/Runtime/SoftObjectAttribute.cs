using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Framework.Core
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SoftObjectAttribute : PropertyAttribute
    {
    }
}