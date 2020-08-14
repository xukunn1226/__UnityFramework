using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class PhysUtility
{
    static private RaycastHit[]         s_HitInfo           = new RaycastHit[1];
    static private ref RaycastHit[]     s_HitInfoRef        => ref s_HitInfo;
    static private RaycastHit           s_EmptyHitInfo      = new RaycastHit();
    static private ref RaycastHit       s_EmptyHitInfoRef   => ref s_EmptyHitInfo;

    // 因使用NonAlloc版本，不保证返回的是最近命中对象
    static public bool Raycast(Ray ray, float maxDistance, int layerMask, ref RaycastHit hitInfo)
    {
        int count = Physics.RaycastNonAlloc(ray, s_HitInfo, maxDistance, layerMask);
        hitInfo = count == 1 ? s_HitInfoRef[0] : s_EmptyHitInfo;
        return count == 1;
    }

    static public ref readonly RaycastHit Raycast(Ray ray, float maxDistance, int layerMask)
    {
        int count = Physics.RaycastNonAlloc(ray, s_HitInfo, maxDistance, layerMask);
        if(count == 1)
            return ref s_HitInfoRef[0];
        else
            return ref s_EmptyHitInfoRef;
    }
}
