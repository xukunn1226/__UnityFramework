using System.Collections;
using System.Collections.Generic;
using System;

namespace Framework.Core
{
    static public partial class Algorithm
    {
        /**
         * @name: 
         * @msg: 时间复杂度O(nlogn)，空间复杂度O(1)，稳定排序
         * @param {type} 
         * @return {type} 
         */
        static public void ShellSort<T>(this IList<T> arr, Comparer<T> comparer = null)
        {
            comparer = comparer ?? Comparer<T>.Default;

            bool flag = true;
            int gap = arr.Count;            
            while (flag || (gap > 1))
            {
                flag = false;
                gap = (gap + 1) / 2;
                for (int i = 0; i < (arr.Count - gap); i++)
                {
                    if (comparer.Compare(arr[i + gap], arr[i]) < 0)
                    {
                        arr.Swap(i + gap, i);
                        flag = true;
                    }
                }
            }
        }
    }
}