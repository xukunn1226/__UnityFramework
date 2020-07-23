using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tests
{
    static public class Util
    {
        static public void Forecah<TKey, TValue>(this Dictionary<TKey, TValue> dict, System.Action<TKey, TValue> EnumeratorFunc)
        {
            if (dict == null || EnumeratorFunc == null)
                throw new System.ArgumentNullException();
            var i = dict.GetEnumerator();
            while (i.MoveNext())
            {
                EnumeratorFunc(i.Current.Key, i.Current.Value);
            }
        }
    }
}