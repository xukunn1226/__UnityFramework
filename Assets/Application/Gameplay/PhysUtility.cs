using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class PhysUtility
{
    static private RaycastHit[] s_HitInfo = new RaycastHit[1];
    static private ref RaycastHit[] s_HitInfoRef => ref s_HitInfo;

    // 因使用NonAlloc版本，不保证返回的是最近命中对象
    static public bool Raycast(Ray ray, float maxDistance, int layerMask, ref RaycastHit hitInfo)
    {
        int count = Physics.RaycastNonAlloc(ray, s_HitInfo, maxDistance, layerMask);
        hitInfo = s_HitInfoRef[0];
        return count == 1;
    }

    static public ref RaycastHit RaycastEx(Ray ray, float maxDistance, int layerMask)
    {
        int count = Physics.RaycastNonAlloc(ray, s_HitInfo, maxDistance, layerMask);
        return s_HitInfoRef[0];
    }
}
