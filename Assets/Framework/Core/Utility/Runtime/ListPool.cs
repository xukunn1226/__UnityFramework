using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Core
{
    public static class ListPool<T>
    {
        // Object pool to avoid allocations.
        private static readonly SimpleObjectPool<List<T>> s_ListPool = new SimpleObjectPool<List<T>>(null, Clear);
        static void Clear(List<T> l) { l.Clear(); }

        public static List<T> Get()
        {
            return s_ListPool.Get();
        }

        public static void Release(List<T> toRelease)
        {
            s_ListPool.Release(toRelease);
        }
    }
}