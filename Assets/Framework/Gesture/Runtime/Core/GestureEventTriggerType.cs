using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Gesture.Runtime
{
    public enum GestureEventTriggerType
    {
        LongPressReady          = 0,
        LongPressRecognized     = 1,
        LongPressFailed         = 2,
        
        PinchReady              = 3,
        PinchStarted            = 4,
        PinchInProgress         = 5,
        PinchEnded              = 6,
        PinchFailed             = 7,

        ScreenDragReady         = 8,
        ScreenDragRecognized    = 9,
        ScreenDragFailed        = 10,
    }
}