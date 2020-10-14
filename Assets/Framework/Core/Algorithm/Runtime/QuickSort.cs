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
        static public void QuickSort<T>(this List<T> arr) where T : IComparable<T>
        {
            DoQuickSortNotR(arr, 0, arr.Count - 1);
        }

        static private void DoQuickSort<T>(List<T> arr, int left, int right) where T : IComparable<T>
        {
            if(left >= right)
                return;

            int index = Partition(arr, left, right);
            DoQuickSort(arr, left, index - 1);
            DoQuickSort(arr, index + 1, right);
        }

        static private void DoQuickSortNotR<T>(List<T> arr, int left, int right) where T : IComparable<T>
        {
            Stack<int> s = new Stack<int>();
            s.Push(left);
            s.Push(right);
            while(s.Count > 0)
            {
                int r = s.Pop();
                int l = s.Pop();

                int index = Partition(arr, l, r);
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
        static private int Partition<T>(List<T> arr, int left, int right) where T : IComparable<T>
        {
            int mid = GetMid(arr, left, right);
            Swap(arr, right, mid);

            int initKey = right;
            T key = arr[initKey];
            while(left < right)
            {
                /// 因为基准数选择的是最右边值，所以要先从左边查找（step1先于step2执行）

                // step1. 从左往右找到大于key的值
                while(left < right && arr[left].CompareTo(key) <= 0)
                {
                    ++left;
                }

                // step2. 从右往左找到小于key的值
                while(left < right && arr[right].CompareTo(key) >= 0)
                {
                    --right;
                }

                Swap(arr, left, right);
            }

            Swap(arr, left, initKey);
            return left;
        }

        // 挖坑法
        static private int Partition2<T>(List<T> arr, int left, int right) where T : IComparable<T>
        {
            int mid = GetMid(arr, left, right);
            Swap(arr, right, mid);
            
            T key = arr[right];
            while(left < right)
            {
                /// 因为基准数选择的是最右边值，所以要先从左边查找（step1先于step2执行）

                // step1. 从左往右找到大于key的值
                while(left < right && arr[left].CompareTo(key) <= 0)
                {
                    ++left;
                }
                arr[right] = arr[left];

                // step2. 从右往左找到小于key的值
                while(left < right && arr[right].CompareTo(key) >= 0)
                {
                    --right;
                }
                arr[left] = arr[right];
            }
            arr[right] = key;
            return right;
        }

        // 前后指针法
        static private int Partition3<T>(List<T> arr, int left, int right) where T : IComparable<T>
        {
            int mid = GetMid(arr, left, right);
            Swap(arr, right, mid);
            
            if(left < right)
            {
                T key = arr[right];
                int cur = left;
                int pre = cur - 1;
                while(cur < right)
                {
                    while(arr[cur].CompareTo(key) < 0 && ++pre != cur)
                    {
                        Swap(arr, cur, pre);
                    }
                    ++cur;
                }
                Swap(arr, ++pre, right);
                return pre;
            }
            return -1;
        }

        static private void Swap<T>(List<T> arr, int n, int m)
        {
            T tmp = arr[n];
            arr[n] = arr[m];
            arr[m] = tmp;
        }

        static private int GetMid<T>(List<T> arr, int left, int right) where T : IComparable<T>
        {
            int mid = left + ((right - left) >> 1);
            if(arr[left].CompareTo(arr[right]) <= 0)
            {
                if(arr[left].CompareTo(arr[mid]) < 0)
                    return mid;
                else if(arr[right].CompareTo(arr[mid]) < 0)
                    return right;
                else
                    return left;
            }
            else
            {
                if(arr[left].CompareTo(arr[mid]) < 0)
                    return left;
                else if(arr[right].CompareTo(arr[mid]) < 0)
                    return mid;
                else
                    return right;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////
        static public void QuickSort<T>(this List<T> arr, Comparison<T> comparison)
        {
            DoQuickSortNotR(arr, 0, arr.Count - 1, comparison);
        }

        static private void DoQuickSort<T>(List<T> arr, int left, int right, Comparison<T> comparison)
        {
            if(left >= right)
                return;

            int index = Partition(arr, left, right, comparison);
            DoQuickSort(arr, left, index - 1, comparison);
            DoQuickSort(arr, index + 1, right, comparison);
        }

        static private void DoQuickSortNotR<T>(List<T> arr, int left, int right, Comparison<T> comparison)
        {
            Stack<int> s = new Stack<int>();
            s.Push(left);
            s.Push(right);
            while(s.Count > 0)
            {
                int r = s.Pop();
                int l = s.Pop();

                int index = Partition(arr, l, r, comparison);
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
        static private int Partition<T>(List<T> arr, int left, int right, Comparison<T> comparison)
        {
            int mid = GetMid(arr, left, right, comparison);
            Swap(arr, right, mid);

            int initKey = right;
            T key = arr[initKey];
            while(left < right)
            {
                /// 因为基准数选择的是最右边值，所以要先从左边查找（step1先于step2执行）

                // step1. 从左往右找到大于key的值
                while(left < right && comparison(arr[left], key) <= 0)
                {
                    ++left;
                }

                // step2. 从右往左找到小于key的值
                while(left < right && comparison(arr[right], key) >= 0)
                {
                    --right;
                }

                Swap(arr, left, right);
            }

            Swap(arr, left, initKey);
            return left;
        }

        // 挖坑法
        static private int Partition2<T>(List<T> arr, int left, int right, Comparison<T> comparison)
        {
            int mid = GetMid(arr, left, right, comparison);
            Swap(arr, right, mid);
            
            T key = arr[right];
            while(left < right)
            {
                /// 因为基准数选择的是最右边值，所以要先从左边查找（step1先于step2执行）

                // step1. 从左往右找到大于key的值
                while(left < right && comparison(arr[left], key) <= 0)
                {
                    ++left;
                }
                arr[right] = arr[left];

                // step2. 从右往左找到小于key的值
                while(left < right && comparison(arr[right], key) >= 0)
                {
                    --right;
                }
                arr[left] = arr[right];
            }
            arr[right] = key;
            return right;
        }

        // 前后指针法
        static private int Partition3<T>(List<T> arr, int left, int right, Comparison<T> comparison)
        {
            int mid = GetMid(arr, left, right, comparison);
            Swap(arr, right, mid);
            
            if(left < right)
            {
                T key = arr[right];
                int cur = left;
                int pre = cur - 1;
                while(cur < right)
                {
                    while(comparison(arr[cur], key) < 0 && ++pre != cur)
                    {
                        Swap(arr, cur, pre);
                    }
                    ++cur;
                }
                Swap(arr, ++pre, right);
                return pre;
            }
            return -1;
        }

        static private int GetMid<T>(List<T> arr, int left, int right, Comparison<T> comparison)
        {
            int mid = left + ((right - left) >> 1);
            if(comparison(arr[left], arr[right]) <= 0)
            {
                if(comparison(arr[left], arr[mid]) < 0)
                    return mid;
                else if(comparison(arr[right], arr[mid]) < 0)
                    return right;
                else
                    return left;
            }
            else
            {
                if(comparison(arr[left], arr[mid]) < 0)
                    return left;
                else if(comparison(arr[right], arr[mid]) < 0)
                    return mid;
                else
                    return right;
            }
        }
    }
}