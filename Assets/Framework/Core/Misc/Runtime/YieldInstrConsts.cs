using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Core
{
    static public class YieldInstrConsts
    {
        static private int[] ss = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        static private int s_CacheSize = 10;
        static private WaitForSeconds[] s_WaitForSecondsCache = new WaitForSeconds[s_CacheSize];

        static private WaitForEndOfFrame s_WaitForEndOfFrame;
        static private WaitForFixedUpdate s_WaitForFixedUpdate;

        static public WaitForSeconds GetWaitForSeconds(float seconds)
        {
            int sec = Mathf.Clamp(Mathf.FloorToInt(seconds + 0.5f), 0, s_CacheSize >> 1);

            int index = Mathf.Min(2 * sec, s_CacheSize - 1);

            if (s_WaitForSecondsCache[index] == null)
            {
                s_WaitForSecondsCache[index] = new WaitForSeconds(sec);
            }
            return s_WaitForSecondsCache[index];
        }

        static private int Round(float v, float gap)
        {
            return 0;
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