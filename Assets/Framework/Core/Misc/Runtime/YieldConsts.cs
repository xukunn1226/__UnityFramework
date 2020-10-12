using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Core
{
    static public class YieldConsts
    {
        static private int                      s_CacheSize                 = 10;
        static private WaitForSeconds[]         s_WaitForSeconds01Cache     = new WaitForSeconds[s_CacheSize + 1];
        static private WaitForSeconds[]         s_WaitForSeconds03Cache     = new WaitForSeconds[s_CacheSize + 1];

        static private WaitForEndOfFrame        s_WaitForEndOfFrame;
        static private WaitForFixedUpdate       s_WaitForFixedUpdate;

        static public WaitForSeconds GetWaitForSeconds_01(float seconds)
        {
            const float gap = 0.1f;
            int index = Round(seconds, gap);

            if (s_WaitForSeconds01Cache[index] == null)
            {
                s_WaitForSeconds01Cache[index] = index == 0 ? new WaitForSeconds(0.1f) : new WaitForSeconds(index * gap);
            }
            return s_WaitForSeconds01Cache[index];
        }
        
        static public WaitForSeconds GetWaitForSeconds_03(float seconds)
        {
            const float gap = 0.3f;
            int index = Round(seconds, gap);

            if (s_WaitForSeconds03Cache[index] == null)
            {
                s_WaitForSeconds03Cache[index] = index == 0 ? new WaitForSeconds(0.1f) : new WaitForSeconds(index * gap);
            }
            return s_WaitForSeconds03Cache[index];
        }

        static private int Round(float v, float gap)
        {
            return Mathf.Clamp(Mathf.FloorToInt(v / gap + 0.5f), 0, s_CacheSize);
        }

        static public WaitForEndOfFrame GetWaitForEndOfFrame()
        {
            if (s_WaitForEndOfFrame == null)
                s_WaitForEndOfFrame = new WaitForEndOfFrame();
            return s_WaitForEndOfFrame;
        }

        static public WaitForFixedUpdate GetWaitForFixedUpdate()
        {
            if (s_WaitForFixedUpdate == null)
                s_WaitForFixedUpdate = new WaitForFixedUpdate();
            return s_WaitForFixedUpdate;
        }
    }
}