using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Core
{
    static public class YieldInstrConsts
    {
        static private int k_CacheSize = 10;
        static private WaitForSeconds[] k_WaitForSecondsCache = new WaitForSeconds[k_CacheSize];

        static private WaitForEndOfFrame k_WaitForEndOfFrame;
        static private WaitForFixedUpdate k_WaitForFixedUpdate;

        static public WaitForSeconds GetWaitForSeconds(float seconds)
        {
            int sec = Mathf.Clamp(Mathf.FloorToInt(seconds + 0.5f), 0, k_CacheSize >> 1);

            int index = Mathf.Min(2 * sec, k_CacheSize - 1);

            if (k_WaitForSecondsCache[index] == null)
            {
                k_WaitForSecondsCache[index] = new WaitForSeconds(sec);
            }
            return k_WaitForSecondsCache[index];
        }

        static public WaitForEndOfFrame GetWaitForEndOfFrame()
        {
            if (k_WaitForEndOfFrame == null)
                k_WaitForEndOfFrame = new WaitForEndOfFrame();
            return k_WaitForEndOfFrame;
        }

        static public WaitForFixedUpdate GetWaitForFixedUpdate()
        {
            if (k_WaitForFixedUpdate == null)
                k_WaitForFixedUpdate = new WaitForFixedUpdate();
            return k_WaitForFixedUpdate;
        }
    }
}