using System.Collections;
using System.Collections.Generic;
using System;

namespace Framework.Core
{
    static public partial class Algorithm
    {
        /**
         * @name: 
         * @msg: 时间复杂度O(nlogn)，空间复杂度O(1)，不稳定排序
         * @param {type} 
         * @return {type} 
         */        
        static public void QuickSort<T>(this IList<T> arr, Comparer<T> comparer = null)
        {
            comparer = comparer ?? Comparer<T>.Default;
            DoQuickSortNotR(arr, 0, arr.Count - 1, comparer);
        }

        static private void DoQuickSort<T>(IList<T> arr, int left, int right, Comparer<T> comparer)
        {
            if(left >= right)
                return;

            int index = Partition(arr, left, right, comparer);
            DoQuickSort(arr, left, index - 1, comparer);
            DoQuickSort(arr, index + 1, right, comparer);
        }

        static private void DoQuickSortNotR<T>(IList<T> arr, int left, int right, Comparer<T> comparer)
        {
            Stack<int> s = new Stack<int>();
            s.Push(left);
            s.Push(right);
            while(s.Count > 0)
            {
                int r = s.Pop();
                int l = s.Pop();

                int index = Partition(arr, l, r, comparer);
                if(index - 1 > l)
                {
                    s.Push(l);
                    s.Push(index - 1);
                }
                if(index + 1 < r)
                {
                    s.Push(index + 1);
                    s.Push(r);
                }
            }
        }

        // 左右指针法
        static private int Partition<T>(IList<T> arr, int left, int right, Comparer<T> comparer)
        {
            int mid = GetMid(arr, left, right, comparer);
            arr.Swap(right, mid);

            int initKey = right;
            T key = arr[initKey];
            while(left < right)
            {
                /// 因为基准数选择的是最右边值，所以要先从左边查找（step1先于step2执行）

                // step1. 从左往右找到大于key的值
                while(left < right && comparer.Compare(arr[left], key) <= 0)
                {
                    ++left;
                }

                // step2. 从右往左找到小于key的值
                while(left < right && comparer.Compare(arr[right], key) >= 0)
                {
                    --right;
                }

                arr.Swap(left, right);
            }

            arr.Swap(left, initKey);
            return left;
        }

        // 挖坑法
        static private int Partition2<T>(IList<T> arr, int left, int right, Comparer<T> comparer)
        {
            int mid = GetMid(arr, left, right, comparer);
            arr.Swap(right, mid);
            
            T key = arr[right];
            while(left < right)
            {
                /// 因为基准数选择的是最右边值，所以要先从左边查找（step1先于step2执行）

                // step1. 从左往右找到大于key的值
                while(left < right && comparer.Compare(arr[left], key) <= 0)
                {
                    ++left;
                }
                arr[right] = arr[left];

                // step2. 从右往左找到小于key的值
                while(left < right && comparer.Compare(arr[right], key) >= 0)
                {
                    --right;
                }
                arr[left] = arr[right];
            }
            arr[right] = key;
            return right;
        }

        // 前后指针法
        static private int Partition3<T>(IList<T> arr, int left, int right, Comparer<T> comparer)
        {
            int mid = GetMid(arr, left, right, comparer);
            arr.Swap(right, mid);
            
            if(left < right)
            {
                T key = arr[right];
                int cur = left;
                int pre = cur - 1;
                while(cur < right)
                {
                    while(comparer.Compare(arr[cur], key) < 0 && ++pre != cur)
                    {
                        arr.Swap(cur, pre);
                    }
                    ++cur;
                }
                arr.Swap(++pre, right);
                return pre;
            }
            return -1;
        }

        static private void Swap<T>(this IList<T> arr, int n, int m)
        {
            T tmp = arr[n];
            arr[n] = arr[m];
            arr[m] = tmp;
        }

        static private int GetMid<T>(IList<T> arr, int left, int right, Comparer<T> comparer)
        {
            int mid = left + ((right - left) >> 1);
            if(comparer.Compare(arr[left], arr[right]) <= 0)
            {
                if(comparer.Compare(arr[left], arr[mid]) < 0)
                    return mid;
                else if(comparer.Compare(arr[right], arr[mid]) < 0)
                    return right;
                else
                    return left;
            }
            else
            {
                if(comparer.Compare(arr[left], arr[mid]) < 0)
                    return left;
                else if(comparer.Compare(arr[right], arr[mid]) < 0)
                    return mid;
                else
                    return right;
            }
        }        
    }
}