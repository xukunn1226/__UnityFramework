using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Runtime
{
    public interface IActor
    {
        ViewLayer minViewLayer { get; set; }
        ViewLayer maxViewLayer { get; set; }
        void OnUpdate();
    }
}