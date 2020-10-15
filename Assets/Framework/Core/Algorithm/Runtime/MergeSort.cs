using System.Collections;
using System.Collections.Generic;
using System;

namespace Framework.Core
{
    static public partial class Algorithm
    {
        ////////////////////////////////// MergeSort
        /**
         * @name: 
         * @msg: 时间复杂度O(nlogn)，空间复杂度O(n)，稳定排序
         * @param {type} 
         * @return {type} 
         */        
        static public void MergeSort<T>(this IList<T> arr, Comparer<T> comparer = null)
        {
            comparer = comparer ?? Comparer<T>.Default;

            T[] result = new T[arr.Count];
            DoMergeSort(arr, result, 0, arr.Count - 1, comparer);
        }

        static private void DoMergeSort<T>(IList<T> arr, T[] result, int start, int end, Comparer<T> comparer)
        {
            if(start >= end)
                return;

            int length = end - start;
            int mid = start + (length >> 1);
            int start1 = start;
            int end1 = mid;
            int start2 = mid + 1;
            int end2 = end;
            DoMergeSort(arr, result, start1, end1, comparer);
            DoMergeSort(arr, result, start2, end2, comparer);

            int k = start;
            while(start1 <= end1 && start2 <= end2)
            {
                result[k++] = comparer.Compare(arr[start1], arr[start2]) < 0 ? arr[start1++] : arr[start2++];
            }

            while(start1 <= end1)
            {
                result[k++] = arr[start1++];
            }
            while(start2 <= end2)
            {
                result[k++] = arr[start2++];
            }

            for(k = start; k <= end; k++)
                arr[k] = result[k];
        }
    }
}