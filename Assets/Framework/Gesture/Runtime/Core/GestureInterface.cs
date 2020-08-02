using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework.Gesture.Runtime
{
    public interface IGestureHandler
    {
    }

    public interface IDiscreteGestureHandler<T> : IGestureHandler where T : GestureEventData, new()
    {
        void OnGesture(T eventData);
    }

    public interface IContinuousGestureHandler<T> : IGestureHandler where T : GestureEventData, new()
    {
        void OnGesture(T eventData);
    }

    // public interface ISelectHandler : IGestureHandler
    // {
    //     void OnSelect();
    // }

    // public interface IDeselectHandler : IGestureHandler
    // {
    //     void OnDeselect();
    // }
}