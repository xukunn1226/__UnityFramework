using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Gesture.Runtime
{
    public abstract class GestureRecognizer : MonoBehaviour
    {
        public int RequiredPointerCount = 1;
    }
}