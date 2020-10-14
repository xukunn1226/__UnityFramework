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
        static public void MergeSort<T>(this List<T> arr) where T : IComparable<T>
        {
            T[] result = new T[arr.Count];
            DoMergeSort(arr, result, 0, arr.Count - 1);
        }

        static private void DoMergeSort<T>(List<T> arr, T[] result, int start, int end) where T : IComparable<T>
        {
            if(start >= end)
                return;

            int length = end - start;
            int mid = start + (length >> 1);
            int start1 = start;
            int end1 = mid;
            int start2 = mid + 1;
            int end2 = end;
            DoMergeSort(arr, result, start1, end1);
            DoMergeSort(arr, result, start2, end2);

            int k = start;
            while(start1 <= end1 && start2 <= end2)
            {
                result[k++] = arr[start1].CompareTo(arr[start2]) < 0 ? arr[start1++] : arr[start2++];
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

        static public void MergeSort<T>(this List<T> arr, Comparison<T> comparison)
        {
            T[] result = new T[arr.Count];
            DoMergeSort(arr, result, 0, arr.Count - 1, comparison);
        }

        static private void DoMergeSort<T>(List<T> arr, T[] result, int start, int end, Comparison<T> comparison)
        {
            if(start >= end)
                return;

            int length = end - start;
            int mid = start + (length >> 1);
            int start1 = start;
            int end1 = mid;
            int start2 = mid + 1;
            int end2 = end;
            DoMergeSort(arr, result, start1, end1, comparison);
            DoMergeSort(arr, result, start2, end2, comparison);

            int k = start;
            while(start1 <= end1 && start2 <= end2)
            {
                result[k++] = comparison(arr[start1], arr[start2]) < 0 ? arr[start1++] : arr[start2++];
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