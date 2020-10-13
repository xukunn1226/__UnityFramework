using System.Collections;
using System.Collections.Generic;
using System;

namespace Framework.Core
{
    static public partial class Algorithm
    {
        static public void QuickSort<T>(this List<T> arr) where T : IComparable<T>
        {
            DoQuickSort(arr, 0, arr.Count - 1);
        }

        static private void DoQuickSort<T>(List<T> arr, int left, int right) where T : IComparable<T>
        {
            if(left >= right)
                return;

            int index = Partition(arr, left, right);
            DoQuickSort(arr, left, index - 1);
            DoQuickSort(arr, index + 1, right);
        }

        static private int Partition<T>(List<T> arr, int left, int right) where T : IComparable<T>
        {
            int initKey = right;
            T key = arr[initKey];
            while(left < right)
            {
                /// 因为基准数是最右边值，所以要先从左边查找

                // 从左往右寻找大于key的值
                while(left < right && arr[left].CompareTo(key) <= 0)
                {
                    ++left;
                }

                // 从右往左寻找小于key的值
                while(left < right && arr[right].CompareTo(key) >= 0)
                {
                    --right;
                }

                T tmp = arr[left];
                arr[left] = arr[right];
                arr[right] = tmp;
            }

            T tmp2 = arr[left];
            arr[left] = arr[initKey];
            arr[initKey] = tmp2;
            return left;
        }
    }
}