using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Core
{
    public class MathUtility
    {
        // http://mathworld.wolfram.com/SpherePointPicking.html
        //public static Vector3 RandCone(Vector3 dir, float coneHalfAngleRad)
        //{
        //    if(coneHalfAngleRad > 0)
        //    {
        //        float u = Random.Range(0, 1);
        //        float v = Random.Range(0, 1);

        //        float theta = 2 * Mathf.PI * u;
        //        float phi = Mathf.Acos(2 * v - 1);
        //        phi = phi % coneHalfAngleRad;
        //    }
        //    else
        //    {
        //        return dir.normalized;
        //    }
        //    return Vector3.one;
        //}

        //public static Vector3 RandCone(Vector3 dir, float horizontalConeHalfAngleRad, float verticalConeHalfAngleRad)
        //{
        //    return Vector3.one;
        //}

        /// <summary>
        /// 返回射线dir基于法线normal的平面的反射向量
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="normal"></param>
        /// <returns></returns>
        public static Vector3 GetReflectionVector(Vector3 dir, Vector3 normal)
        {
            normal.Normalize();
            return dir - 2 * Vector3.Dot(dir, normal) * normal;
        }

        static public int NextPowerOfTwo(int n)
        {
            if (IsPowerOfTwo(n))
            {
                return n;
            }

            int p = 1;
            while (p < n)
                p <<= 1;
            return p;
        }

        static public bool IsPowerOfTwo(int n)
        {
            if (n > 0 && (n & (n - 1)) == 0)
                return true;
            return false;
        }

        // 把num向上取整到大于它的最小整数（b为2^n）倍数
        static public int AroundTo(int num, int b)
        {
            return (num + (b - 1)) & ~(b-1);
        }

        public static float GridSnap(float location, float grid)
        {
            if (grid == 0)
                return location;

            return Mathf.Floor((location + 0.5f * grid) / grid) * grid;
        }

        /// <summary>
        /// 两角度值之间的最小角度
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float FindDeltaAngleDegrees(float a, float b)
        {
            float delta = b - a;
            if(delta > 180)
            {
                delta = delta - 360;
            }
            else if(delta < -180)
            {
                delta = delta + 360;
            }
            return delta;
        }

        public static float FindDeltaAngleRadians(float a, float b)
        {
            float delta = b - a;
            if(delta > Mathf.PI)
            {
                delta = delta - 2 * Mathf.PI;
            }
            else if(delta < -Mathf.PI)
            {
                delta = delta + 2 * Mathf.PI;
            }
            return delta;
        }

        /// <summary>
        /// unwind it back into that range [-PI, +PI]
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static float UnwindRadians(float a)
        {
            while(a > Mathf.PI)
            {
                a -= 2.0f * Mathf.PI;
            }

            while(a < -Mathf.PI)
            {
                a += 2.0f * Mathf.PI;
            }

            return a;
        }

        public static float UnwindDegrees(float a)
        {
            while(a > 180)
            {
                a -= 360;
            }

            while(a < -180)
            {
                a += 360;
            }

            return a;
        }

        /// <summary>
        /// 返回相对baseAngle的角度差
        /// </summary>
        /// <param name="baseAngle"></param>
        /// <param name="angle"></param>
        /// <returns>[-180, 180]</returns>
        public static float WindRelativeAngleDegrees(float baseAngle, float angle)
        {
            float diff = baseAngle - angle;
            float absDiff = Mathf.Abs(diff);
            if(absDiff > 180)
            {
                angle += 360.0f * Mathf.Sign(diff) * Mathf.Floor(absDiff / 360.0f + 0.5f);
            }
            return angle;
        }

        public static float GetRangePct(float minValue, float maxValue, float value)
        {            
            if(Mathf.Approximately(minValue, maxValue))
            {
                return value >= maxValue ? 1.0f : 0.0f;
            }
            else
            {
                return (value - minValue) / (maxValue - minValue);
            }
        }

        public static float BiLerp(float p00, float p10, float p01, float p11, float fracX, float fracY)
        {
            return Mathf.Lerp(Mathf.Lerp(p00, p10, fracX), Mathf.Lerp(p01, p11, fracX), fracY);
        }

        public static float CubicInterp(float p0, float t0, float p1, float t1, float alpha)
        {
            float alpha2 = alpha * alpha;
            float alpha3 = alpha2 * alpha;
            return ((2 * alpha3) - (3 * alpha2) + 1) * p0 + ((alpha3 - 2 * alpha2 + alpha) * t0) + ((alpha3 - alpha2) * t1) + ((-2 * alpha3) + (3 * alpha2)) * p1;
        }

        ///////////////////////////////// easing function
        /// reference: http://gizma.com/easing/   https://easings.net/

        // Linear
        // easeInQuad
        // easeInCubic
        // easeInQuart
        // easeInQuint
        // easeInSine
        // easeInExpo
        // easeInCirc
        // easeInBack
        // easeInElastic
        // easeInBounce

        

        public static float Linear(float a, float b, float alpha)
        {
            return Mathf.Lerp(a, b, alpha);
        }

        // exp == 2, 3, 4, 5分别对应Quad, Cubic, Quart, Quint
        public static float EaseInPow(float a, float b, float alpha, float exp)
        {
            float modifiedAlpha = Mathf.Pow(alpha, exp);
            return Mathf.Lerp(a, b, modifiedAlpha);
        }

        public static float EaseOutPow(float a, float b, float alpha, float exp)
        {
            float modifiedAlpha = 1.0f - Mathf.Pow(1.0f - alpha, exp);
            return Mathf.Lerp(a, b, modifiedAlpha);
        }

        public static float EaseInOutPow(float a, float b, float alpha, float exp)
        {
            return Mathf.Lerp(a, b, (alpha < 0.5f) ? 
                                    EaseInPow(0, 1, alpha * 2, exp) * 0.5f : 
                                    EaseOutPow(0, 1, alpha * 2 - 1, exp) * 0.5f + 0.5f);
        }

        public static float EaseInSine(float a, float b, float alpha)
        {
            float modifiedAlpha = 1.0f - Mathf.Cos(alpha * Mathf.PI * 0.5f);
            return Mathf.Lerp(a, b, modifiedAlpha);
        }

        public static float EaseOutSine(float a, float b, float alpha)
        {
            float modifiedAlpha = Mathf.Sin(alpha * Mathf.PI * 0.5f);
            return Mathf.Lerp(a, b, modifiedAlpha);
        }

        public static float EaseInOutSine(float a, float b, float alpha)
        {
            // return Mathf.Lerp(a, b, (alpha < 0.5f) ?
            //                         EaseInSine(0, 1, alpha * 2) * 0.5f :
            //                         EaseOutSine(0, 1, alpha * 2 - 1) * 0.5f + 0.5f);
            float modifiedAlpha = -(Mathf.Cos(Mathf.PI * alpha) - 1) * 0.5f;
            return Mathf.Lerp(a, b, modifiedAlpha);
        }

        public static float EaseInExpo(float a, float b, float alpha)
        {
            float modifiedAlpha = (alpha == 0) ? 0 : Mathf.Pow(2, 10.0f * (alpha - 1.0f));
            return Mathf.Lerp(a, b, modifiedAlpha);
        }

        public static float EaseOutExpo(float a, float b, float alpha)
        {
            float modifiedAlpha = (alpha == 1) ? 1 : 1.0f - Mathf.Pow(2, -10.0f * alpha);
            return Mathf.Lerp(a, b, modifiedAlpha);
        }

        public static float EaseInOutExpo(float a, float b, float alpha)
        {
            // return Mathf.Lerp(a, b, (alpha < 0.5f) ?
            //                         EaseInExpo(0, 1, alpha * 2) * 0.5f :
            //                         EaseOutExpo(0, 1, alpha * 2 - 1) * 0.5f + 0.5f);
            float modifiedAlpha = alpha == 0
                                    ? 0
                                    : alpha == 1
                                    ? 1
                                    : alpha < 0.5f
                                    ? Mathf.Pow(2, 20 * alpha - 10) * 0.5f
                                    : (2 - Mathf.Pow(2, -20 * alpha + 10)) * 0.5f;
            return Mathf.Lerp(a, b, modifiedAlpha);
        }

        public static float EaseInCircular(float a, float b, float alpha)
        {
            float modifiedAlpha = 1.0f - Mathf.Sqrt(1.0f - alpha * alpha);
            return Mathf.Lerp(a, b, modifiedAlpha);
        }

        public static float EaseOutCircular(float a, float b, float alpha)
        {
            float modifiedAlpha = Mathf.Sqrt(1.0f - (alpha - 1.0f) * (alpha - 1.0f));
            return Mathf.Lerp(a, b, modifiedAlpha);
        }

        public static float EaseInOutCircular(float a, float b, float alpha)
        {
            // return Mathf.Lerp(a, b, (alpha < 0.5f) ?
            //                         EaseInCircular(0, 1, alpha * 2) * 0.5f :
            //                         EaseInOutCircular(0, 1, alpha * 2 - 1) * 0.5f + 0.5f);
            float modifiedAlpha = alpha < 0.5f
                                    ? (1 - Mathf.Sqrt(1 - Mathf.Pow(2 * alpha, 2))) * 0.5f
                                    : (Mathf.Sqrt(1 - Mathf.Pow(-2 * alpha + 2, 2)) + 1) * 0.5f;
            return Mathf.Lerp(a, b, modifiedAlpha);
        }

        public static float EaseInBack(float a, float b, float alpha)
        {
            const float c1 = 1.70158f;
            const float c2 = c1 + 1;
            float modifiedAlpha = c2 * Mathf.Pow(alpha, 3) - c1 * Mathf.Pow(alpha, 2);
            return Mathf.Lerp(a, b, modifiedAlpha);
        }

        public static float EaseOutBack(float a, float b, float alpha)
        {
            const float c1 = 1.70158f;
            const float c2 = c1 + 1;
            float modifiedAlpha = 1 + c2 * Mathf.Pow(alpha - 1, 3) + c1 * Mathf.Pow(alpha - 1, 2);
            return Mathf.Lerp(a, b, modifiedAlpha);
        }

        public static float EaseInOutBack(float a, float b, float alpha)
        {
            const float c1 = 1.70158f;
            const float c2 = c1 * 1.525f;
            float modifiedAlpha = alpha < 0.5f 
                                    ? (Mathf.Pow(2 * alpha, 2) * ((c2 + 1) * 2 * alpha - c2)) * 0.5f 
                                    : (Mathf.Pow(2 * alpha - 2, 2) * ((c2 + 1) * (alpha * 2 - 2) + c2) + 2) * 0.5f;
            return Mathf.Lerp(a, b, modifiedAlpha);
        }

        public static float EaseInElastic(float a, float b, float alpha)
        {
            const float c = (Mathf.PI * 2) / 3;
            float modifiedAlpha = alpha == 0 
                                    ? 0 
                                    : alpha == 1 
                                    ? 1 
                                    : -Mathf.Pow(2, 10 * alpha - 10) * Mathf.Sin((alpha * 10 - 10.75f) * c);
            return Mathf.Lerp(a, b, modifiedAlpha);
        }

        public static float EaseOutElastic(float a, float b, float alpha)
        {
            const float c = (Mathf.PI * 2) / 3;
            float modifiedAlpha = alpha == 0 
                                    ? 0 
                                    : alpha == 1 
                                    ? 1 
                                    : Mathf.Pow(2, -10 * alpha) * Mathf.Sin((alpha * 10 - 10.75f) * c) + 1;
            return Mathf.Lerp(a, b, modifiedAlpha);
        }

        public static float EaseInOutElastic(float a, float b, float alpha)
        {
            const float c = (Mathf.PI * 2) / 4.5f;
            float modifiedAlpha = alpha == 0
                                    ? 0
                                    : alpha == 1
                                    ? 1
                                    : alpha < 0.5f
                                    ? -(Mathf.Pow(2, 20 * alpha - 10) * Mathf.Sin((20 * alpha - 11.125f) * c)) * 0.5f
                                    : (Mathf.Pow(2, -20 * alpha + 10) * Mathf.Sin((20 * alpha - 11.125f) * c)) * 0.5f + 1;
            return Mathf.Lerp(a, b, modifiedAlpha);
        }

        public static float EaseInBounce(float a, float b, float alpha)
        {
            float modifiedAlpha = 1 - InternalEaseOutBounce(1 - alpha);
            return Mathf.Lerp(a, b, modifiedAlpha);
        }

        public static float EaseOutBounce(float a, float b, float alpha)
        {
            return Mathf.Lerp(a, b, InternalEaseOutBounce(alpha));
        }

        public static float EaseInOutBounce(float a, float b, float alpha)
        {
            float modifiedAlpha = alpha < 0.5f ? (1 - InternalEaseOutBounce(1 - 2 * alpha)) * 0.5f : (1 + InternalEaseOutBounce(2 * alpha - 1)) * 0.5f;
            return Mathf.Lerp(a, b, modifiedAlpha);
        }

        private static float InternalEaseOutBounce(float alpha)
        {
            const float n = 7.5625f;
            const float d = 2.75f;
            float modifiedAlpha = 0;
            if(alpha < 1 / d)
            {
                modifiedAlpha = n * alpha * alpha;
            }
            else if(alpha < 2 / d)
            {
                modifiedAlpha = n * ((alpha - 1.5f) / d) * alpha + 0.75f;
            }
            else if(alpha < 2.5f / d)
            {
                modifiedAlpha = n * ((alpha - 2.25f) / d) * alpha + 0.9375f;
            }
            else
            {
                modifiedAlpha = n * ((alpha - 2.625f) / d) * alpha + 0.984375f;
            }
            return modifiedAlpha;
        }
    }
}

// https://blog.csdn.net/u014271114/article/details/47703061
// https://blog.csdn.net/jebe7282/article/details/7521067/
// https://www.cnblogs.com/mrsunny/archive/2011/06/21/2086080.html