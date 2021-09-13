using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;

namespace Application.Runtime
{
    public enum EasingFunction
    {
        Linear,
        EaseInQuad,
        EaseOutQuad,
        EaseInOutQuad,
        EaseInCubic,
        EaseOutCubic,
        EaseInOutCubic,
        EaseInQuart,
        EaseOutQuart,
        EaseInOutQuart,
        EaseInQuint,
        EaseOutQuint,
        EaseInOutQuint,
        EaseInSine,
        EaseOutSine,
        EaseInOutSine,
        EaseInExpo,
        EaseOutExpo,
        EaseInOutExpo,
        EaseInCirc,
        EaseOutCirc,
        EaseInOutCirc,
        EaseInBack,
        EaseOutBack,
        EaseInOutBack,
        EaseInElastic,
        EaseOutElastic,
        EaseInOutElastic,
        EaseInBounce,
        EaseOutBounce,
        EaseInOutBounce,
    }

    public static class EasingHelper
    {
        static public float GetEasingAlpha(EasingFunction function, float alpha)
        {
            float t = 0;
            switch(function)
            {
                case EasingFunction.Linear:
                    t = MathUtility.Linear(0, 1, alpha);
                    break;
                case EasingFunction.EaseInQuad:
                    t = MathUtility.EaseInPow(0, 1, alpha, 2);
                    break;
                case EasingFunction.EaseOutQuad:
                    t = MathUtility.EaseOutPow(0, 1, alpha, 2);
                    break;
                case EasingFunction.EaseInOutQuad:
                    t = MathUtility.EaseInOutPow(0, 1, alpha, 2);
                    break;
                case EasingFunction.EaseInCubic:
                    t = MathUtility.EaseInPow(0, 1, alpha, 3);
                    break;
                case EasingFunction.EaseOutCubic:
                    t = MathUtility.EaseOutPow(0, 1, alpha, 3);
                    break;
                case EasingFunction.EaseInOutCubic:
                    t = MathUtility.EaseInOutPow(0, 1, alpha, 3);
                    break;
                case EasingFunction.EaseInQuart:
                    t = MathUtility.EaseInPow(0, 1, alpha, 4);
                    break;
                case EasingFunction.EaseOutQuart:
                    t = MathUtility.EaseOutPow(0, 1, alpha, 4);
                    break;
                case EasingFunction.EaseInOutQuart:
                    t = MathUtility.EaseInOutPow(0, 1, alpha, 4);
                    break;
                case EasingFunction.EaseInQuint:
                    t = MathUtility.EaseInPow(0, 1, alpha, 5);
                    break;
                case EasingFunction.EaseOutQuint:
                    t = MathUtility.EaseOutPow(0, 1, alpha, 5);
                    break;
                case EasingFunction.EaseInOutQuint:
                    t = MathUtility.EaseInOutPow(0, 1, alpha, 5);
                    break;
                case EasingFunction.EaseInSine:
                    t = MathUtility.EaseInSine(0, 1, alpha);
                    break;
                case EasingFunction.EaseOutSine:
                    t = MathUtility.EaseOutSine(0, 1, alpha);
                    break;
                case EasingFunction.EaseInOutSine:
                    t = MathUtility.EaseInOutSine(0, 1, alpha);
                    break;
                case EasingFunction.EaseInExpo:
                    t = MathUtility.EaseInExpo(0, 1, alpha);
                    break;
                case EasingFunction.EaseOutExpo:
                    t = MathUtility.EaseOutExpo(0, 1, alpha);
                    break;
                case EasingFunction.EaseInOutExpo:
                    t = MathUtility.EaseInOutExpo(0, 1, alpha);
                    break;
                case EasingFunction.EaseInCirc:
                    t = MathUtility.EaseInCircular(0, 1, alpha);
                    break;
                case EasingFunction.EaseOutCirc:
                    t = MathUtility.EaseOutCircular(0, 1, alpha);
                    break;
                case EasingFunction.EaseInOutCirc:
                    t = MathUtility.EaseInOutCircular(0, 1, alpha);
                    break;
                case EasingFunction.EaseInBack:
                    t = MathUtility.EaseInBack(0, 1, alpha);
                    break;
                case EasingFunction.EaseOutBack:
                    t = MathUtility.EaseOutBack(0, 1, alpha);
                    break;
                case EasingFunction.EaseInOutBack:
                    t = MathUtility.EaseInOutBack(0, 1, alpha);
                    break;
                case EasingFunction.EaseInElastic:
                    t = MathUtility.EaseInElastic(0, 1, alpha);
                    break;
                case EasingFunction.EaseOutElastic:
                    t = MathUtility.EaseOutElastic(0, 1, alpha);
                    break;
                case EasingFunction.EaseInOutElastic:
                    t = MathUtility.EaseInOutElastic(0, 1, alpha);
                    break;
                case EasingFunction.EaseInBounce:
                    t = MathUtility.EaseInBounce(0, 1, alpha);
                    break;
                case EasingFunction.EaseOutBounce:
                    t = MathUtility.EaseOutBounce(0, 1, alpha);
                    break;
                case EasingFunction.EaseInOutBounce:
                    t = MathUtility.EaseInOutBounce(0, 1, alpha);
                    break;
            }
            return t;
        }
    }

    [System.Serializable]
    public class PositionEasingEvent
    {
        public Vector3          curPosition;
        public Vector3          targetPosition;
        public float            time;
        public EasingFunction   function;
        private float           m_Time;

        public PositionEasingEvent(Vector3 curPos, Vector3 targetPos, float time, EasingFunction function)
        {
            this.curPosition    = curPos;
            this.targetPosition = targetPos;
            this.time           = Mathf.Max(0.01f, time);
            this.function       = function;
        }

        public bool Poll(float deltaTime, out Vector3 pos)
        {
            m_Time += deltaTime;
            pos = Vector3.Lerp(curPosition, targetPosition, EasingHelper.GetEasingAlpha(function, m_Time / time));

            return m_Time >= time;
        }
    }
}