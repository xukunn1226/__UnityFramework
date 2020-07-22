using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Gesture.Runtime
{
    public enum GestureEventTriggerType
    {
        LongPressRecognized     = 0,
        
        PinchStarted            = 1,
        PinchInProgress         = 2,
        PinchEnded              = 3,
        PinchFailed             = 4,

        ScreenDragStarted       = 5,
        ScreenDragInProgress    = 6,
        ScreenDragEnded         = 7,
        ScreenDragFailed        = 8,
    }
}