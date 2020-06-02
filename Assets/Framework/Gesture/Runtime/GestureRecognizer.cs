using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Gesture.Runtime
{
    public abstract class GestureRecognizer : MonoBehaviour
    {
        public int RequiredPointerCount = 1;

        private void Update()
        {

        }
    }

    public abstract class GestureRecognizer<T> : GestureRecognizer where T : GestureEventData, new()
    {

    }
}