using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Runtime
{
    static public class PhysUtility
    {
        static private RaycastHit[]     s_HitInfo           = new RaycastHit[3];
        static private ref RaycastHit[] s_HitInfoRef        => ref s_HitInfo;
        static private RaycastHit       s_EmptyHitInfo      = new RaycastHit();
        static private ref RaycastHit   s_EmptyHitInfoRef   => ref s_EmptyHitInfo;

        static public bool Raycast(Ray ray, float maxDistance, int layerMask, ref RaycastHit hitInfo)
        {
            int count = Physics.RaycastNonAlloc(ray, s_HitInfo, maxDistance, layerMask);            
            if(count > 1)
            {
                System.Array.Sort(s_HitInfoRef, 0, count, RaycastHitComparer.instance);
            }
            hitInfo = count > 0 ? s_HitInfoRef[0] : s_EmptyHitInfo;
            return count > 0;
        }

        static public ref readonly RaycastHit Raycast(Ray ray, float maxDistance, int layerMask)
        {
            int count = Physics.RaycastNonAlloc(ray, s_HitInfo, maxDistance, layerMask);
            if(count > 1)
            {
                System.Array.Sort(s_HitInfoRef, 0, count, RaycastHitComparer.instance);
            }
            if (count > 0)
                return ref s_HitInfoRef[0];
            else
                return ref s_EmptyHitInfoRef;
        }

        private class RaycastHitComparer : IComparer<RaycastHit>
        {
            public static RaycastHitComparer instance = new RaycastHitComparer();
            public int Compare(RaycastHit x, RaycastHit y)
            {
                return x.distance.CompareTo(y.distance);
            }
        }
    }
}