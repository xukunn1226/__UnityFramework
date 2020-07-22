using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Gesture.Runtime
{
    public interface IGestureHandler
    {
    }

    public interface IDiscreteGestureHandler<T> : IGestureHandler where T : GestureEventData, new()
    {
        // void OnGestureReady(T eventData);

        void OnGestureRecognized(T eventData);

        // void OnGestureFailed(T eventData);
    }

    public interface IContinuousGestureHandler<T> : IGestureHandler where T : GestureEventData, new()
    {
        // void OnGestureReady(T eventData);

        void OnGestureStarted(T eventData);

        void OnGestureProgress(T eventData);

        void OnGestureEnded(T eventData);

        void OnGestureFailed(T eventData);
    }
}