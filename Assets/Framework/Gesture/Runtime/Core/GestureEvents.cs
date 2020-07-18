using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Framework.Gesture.Runtime
{
    static public class GestureEvents
    {
        private delegate void DiscreteEventFunction<T>(IDiscreteGestureHandler<T> handler, T eventData) where T : GestureEventData, new();
        private delegate void ContinuousEventFunction<T>(IContinuousGestureHandler<T> handler, T eventData) where T : GestureEventData, new();

        private static void ExecuteReady<T>(IDiscreteGestureHandler<T> handler, T eventData) where T : GestureEventData, new()
        {
            handler?.OnGestureReady(eventData);
        }
        
        private static void ExecuteRecognized<T>(IDiscreteGestureHandler<T> handler, T eventData) where T : GestureEventData, new()
        {
            handler?.OnGestureRecognized(eventData);
        }
        
        private static void ExecuteFailed<T>(IDiscreteGestureHandler<T> handler, T eventData) where T : GestureEventData, new()
        {
            handler?.OnGestureFailed(eventData);
        }

        private static void ExecuteReady<T>(IContinuousGestureHandler<T> handler, T eventData) where T : GestureEventData, new()
        {
            handler?.OnGestureReady(eventData);
        }

        private static void ExecuteProgress<T>(IContinuousGestureHandler<T> handler, T eventData) where T : GestureEventData, new()
        {
            handler?.OnGestureProgress(eventData);
        }
        
        private static void ExecuteStarted<T>(IContinuousGestureHandler<T> handler, T eventData) where T : GestureEventData, new()
        {
            handler?.OnGestureStarted(eventData);
        }
        
        private static void ExecuteEnded<T>(IContinuousGestureHandler<T> handler, T eventData) where T : GestureEventData, new()
        {
            handler?.OnGestureEnded(eventData);
        }
        
        private static void ExecuteFailed<T>(IContinuousGestureHandler<T> handler, T eventData) where T : GestureEventData, new()
        {
            handler?.OnGestureFailed(eventData);
        }

        private static readonly ObjectPool<List<IGestureHandler>> s_HandlerListPool = new ObjectPool<List<IGestureHandler>>(null, l => l.Clear());

        private static bool Execute<T, K>(GameObject target, K eventData, DiscreteEventFunction<K> functor) where T : IDiscreteGestureHandler<K> where K : GestureEventData, new()
        {
            var internalHandlers = s_HandlerListPool.Get();
            GetEventList<T>(target, internalHandlers);
            //  if (s_InternalHandlers.Count > 0)
            //      Debug.Log("Executinng " + typeof (T) + " on " + target);

            for (var i = 0; i < internalHandlers.Count; i++)
            {
                T arg;
                try
                {
                    arg = (T)internalHandlers[i];
                }
                catch (Exception e)
                {
                    var temp = internalHandlers[i];
                    Debug.LogException(new Exception(string.Format("Type {0} expected {1} received.", typeof(T).Name, temp.GetType().Name), e));
                    continue;
                }

                try
                {
                    functor((IDiscreteGestureHandler<K>)arg, eventData);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            var handlerCount = internalHandlers.Count;
            s_HandlerListPool.Release(internalHandlers);
            return handlerCount > 0;
        }

        private static bool Execute<T, K>(GameObject target, K eventData, ContinuousEventFunction<K> functor) where T : IContinuousGestureHandler<K> where K : GestureEventData, new()
        {
            var internalHandlers = s_HandlerListPool.Get();
            GetEventList<T>(target, internalHandlers);
            //  if (s_InternalHandlers.Count > 0)
            //      Debug.Log("Executinng " + typeof (T) + " on " + target);

            for (var i = 0; i < internalHandlers.Count; i++)
            {
                T arg;
                try
                {
                    arg = (T)internalHandlers[i];
                }
                catch (Exception e)
                {
                    var temp = internalHandlers[i];
                    Debug.LogException(new Exception(string.Format("Type {0} expected {1} received.", typeof(T).Name, temp.GetType().Name), e));
                    continue;
                }

                try
                {
                    functor((IContinuousGestureHandler<K>)arg, eventData);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            var handlerCount = internalHandlers.Count;
            s_HandlerListPool.Release(internalHandlers);
            return handlerCount > 0;
        }

        private static void GetEventChain(GameObject root, IList<Transform> eventChain)
        {
            eventChain.Clear();
            if (root == null)
                return;

            var t = root.transform;
            while (t != null)
            {
                eventChain.Add(t);
                t = t.parent;
            }
        }

        /// <summary>
        /// Execute the specified event on the first game object underneath the current touch.
        /// </summary>
        private static readonly List<Transform> s_InternalTransformList = new List<Transform>(30);

        private static GameObject ExecuteHierarchy<T, K>(GameObject root, K eventData, DiscreteEventFunction<K> callbackFunction) where T : IDiscreteGestureHandler<K> where K : GestureEventData, new()
        {
            GetEventChain(root, s_InternalTransformList);

            for (var i = 0; i < s_InternalTransformList.Count; i++)
            {
                var transform = s_InternalTransformList[i];
                if (Execute<T, K>(transform.gameObject, eventData, callbackFunction))
                    return transform.gameObject;
            }
            return null;
        }

        private static GameObject ExecuteHierarchy<T, K>(GameObject root, K eventData, ContinuousEventFunction<K> callbackFunction) where T : IContinuousGestureHandler<K> where K : GestureEventData, new()
        {
            GetEventChain(root, s_InternalTransformList);

            for (var i = 0; i < s_InternalTransformList.Count; i++)
            {
                var transform = s_InternalTransformList[i];
                if (Execute<T, K>(transform.gameObject, eventData, callbackFunction))
                    return transform.gameObject;
            }
            return null;
        }

        private static bool ShouldSendToComponent<T>(Component component) where T : IGestureHandler
        {
            var valid = component is T;
            if (!valid)
                return false;

            var behaviour = component as Behaviour;
            if (behaviour != null)
                return behaviour.isActiveAndEnabled;
            return true;
        }

        /// <summary>
        /// Get the specified object's event event.
        /// </summary>
        private static void GetEventList<T>(GameObject go, IList<IGestureHandler> results) where T : IGestureHandler
        {
            // Debug.LogWarning("GetEventList<" + typeof(T).Name + ">");
            if (results == null)
                throw new ArgumentException("Results array is null", "results");

            if (go == null || !go.activeInHierarchy)
                return;

            var components = ListPool<Component>.Get();
            go.GetComponents(components);
            for (var i = 0; i < components.Count; i++)
            {
                if (!ShouldSendToComponent<T>(components[i]))
                    continue;

                // Debug.Log(string.Format("{2} found! On {0}.{1}", go, s_GetComponentsScratch[i].GetType(), typeof(T)));
                results.Add(components[i] as IGestureHandler);
            }
            ListPool<Component>.Release(components);
            // Debug.LogWarning("end GetEventList<" + typeof(T).Name + ">");
        }

        /// <summary>
        /// Whether the specified game object will be able to handle the specified event.
        /// </summary>
        private static bool CanHandleEvent<T>(GameObject go) where T : IGestureHandler
        {
            var internalHandlers = s_HandlerListPool.Get();
            
            GetEventList<T>(go, internalHandlers);
            var handlerCount = internalHandlers.Count;
            s_HandlerListPool.Release(internalHandlers);
            return handlerCount != 0;
        }

        /// <summary>
        /// Bubble the specified event on the game object, figuring out which object will actually receive the event.
        /// </summary>
        private static GameObject GetEventHandler<T>(GameObject root) where T : IGestureHandler
        {
            if (root == null)
                return null;

            Transform t = root.transform;
            while (t != null)
            {
                if (CanHandleEvent<T>(t.gameObject))
                    return t.gameObject;
                t = t.parent;
            }
            return null;
        }



        public static bool ExecuteReady_Discrete<T, K>(GameObject target, K eventData) where T : IDiscreteGestureHandler<K> where K : GestureEventData, new()
        {
            return Execute<T, K>(target, eventData, ExecuteReady);
        }
        public static bool ExecuteRecognized_Discrete<T, K>(GameObject target, K eventData) where T : IDiscreteGestureHandler<K> where K : GestureEventData, new()
        {
            return Execute<T, K>(target, eventData, ExecuteRecognized);
        }
        public static bool ExecuteFailed_Discrete<T, K>(GameObject target, K eventData) where T : IDiscreteGestureHandler<K> where K : GestureEventData, new()
        {
            return Execute<T, K>(target, eventData, ExecuteFailed);
        }
        public static bool ExecuteReady_Continous<T, K>(GameObject target, K eventData) where T : IContinuousGestureHandler<K> where K : GestureEventData, new()
        {
            return Execute<T, K>(target, eventData, ExecuteReady);
        }
        public static bool ExecuteProgress_Continous<T, K>(GameObject target, K eventData) where T : IContinuousGestureHandler<K> where K : GestureEventData, new()
        {
            return Execute<T, K>(target, eventData, ExecuteProgress);
        }
        public static bool ExecuteStarted_Continous<T, K>(GameObject target, K eventData) where T : IContinuousGestureHandler<K> where K : GestureEventData, new()
        {
            return Execute<T, K>(target, eventData, ExecuteStarted);
        }
        public static bool ExecuteEnded_Continous<T, K>(GameObject target, K eventData) where T : IContinuousGestureHandler<K> where K : GestureEventData, new()
        {
            return Execute<T, K>(target, eventData, ExecuteEnded);
        }
        public static bool ExecuteFailed_Continous<T, K>(GameObject target, K eventData) where T : IContinuousGestureHandler<K> where K : GestureEventData, new()
        {
            return Execute<T, K>(target, eventData, ExecuteFailed);
        }



        private static GameObject ExecuteReadyHierarchy_Discrete<T, K>(GameObject root, K eventData) where T : IDiscreteGestureHandler<K> where K : GestureEventData, new()
        {
            return ExecuteHierarchy<T, K>(root, eventData, ExecuteReady);
        }
        private static GameObject ExecuteRecognizedHierarchy_Discrete<T, K>(GameObject root, K eventData) where T : IDiscreteGestureHandler<K> where K : GestureEventData, new()
        {
            return ExecuteHierarchy<T, K>(root, eventData, ExecuteRecognized);
        }
        private static GameObject ExecuteFailedHierarchy_Discrete<T, K>(GameObject root, K eventData) where T : IDiscreteGestureHandler<K> where K : GestureEventData, new()
        {
            return ExecuteHierarchy<T, K>(root, eventData, ExecuteFailed);
        }
        private static GameObject ExecuteReadyHierarchy_Continuous<T, K>(GameObject root, K eventData) where T : IContinuousGestureHandler<K> where K : GestureEventData, new()
        {
            return ExecuteHierarchy<T, K>(root, eventData, ExecuteReady);
        }
        private static GameObject ExecuteProgressHierarchy_Continuous<T, K>(GameObject root, K eventData) where T : IContinuousGestureHandler<K> where K : GestureEventData, new()
        {
            return ExecuteHierarchy<T, K>(root, eventData, ExecuteProgress);
        }
        private static GameObject ExecuteStartedHierarchy_Continuous<T, K>(GameObject root, K eventData) where T : IContinuousGestureHandler<K> where K : GestureEventData, new()
        {
            return ExecuteHierarchy<T, K>(root, eventData, ExecuteStarted);
        }
        private static GameObject ExecuteEndedHierarchy_Continuous<T, K>(GameObject root, K eventData) where T : IContinuousGestureHandler<K> where K : GestureEventData, new()
        {
            return ExecuteHierarchy<T, K>(root, eventData, ExecuteEnded);
        }
        private static GameObject ExecuteFailedHierarchy_Continuous<T, K>(GameObject root, K eventData) where T : IContinuousGestureHandler<K> where K : GestureEventData, new()
        {
            return ExecuteHierarchy<T, K>(root, eventData, ExecuteFailed);
        }
    }
}