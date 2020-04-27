using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class YieldInstrConsts
{
    static private int k_CacheSize = 10;
    static private WaitForSeconds[] k_WaitForSecondsCache = new WaitForSeconds[k_CacheSize];

    static public WaitForSeconds GetWaitForSeconds(float seconds)
    {
        int sec = Mathf.Clamp(Mathf.FloorToInt(seconds + 0.5f), 0, k_CacheSize >> 1);

        int index = Mathf.Min(2 * sec, k_CacheSize - 1);
        
        if(k_WaitForSecondsCache[index] == null)
        {
            k_WaitForSecondsCache[index] = new WaitForSeconds(sec);
        }
        return k_WaitForSecondsCache[index];
    }
}
