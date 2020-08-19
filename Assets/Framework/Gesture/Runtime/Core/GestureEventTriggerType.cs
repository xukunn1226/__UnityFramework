using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Gesture.Runtime
{
    public enum GestureEventTriggerType
    {
        ScreenLongPress     = 0,
        ScreenDrag          = 1,
        ScreenPinch         = 2,
        ScreenPointerDown   = 3,
        ScreenPointerUp     = 4,
        ObjectBeginDrag     = 5,
        ObjectDragging      = 6,
        ObjectEndDrag       = 7,
    }
}