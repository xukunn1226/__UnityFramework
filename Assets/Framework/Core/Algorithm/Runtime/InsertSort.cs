using System.Collections;
using System.Collections.Generic;
using System;

namespace Framework.Core
{
    static public partial class Algorithm
    {
        static public void insertSort<T>(this List<T> arr) where T : IComparable<T>
        {
            // 从第二个数开始，往前插入数字
            int length = arr.Count;
            for (int i = 1; i < length; i++)
            {
                // j 记录当前数字下标
                int j = i;
                // 当前数字比前一个数字小，则将当前数字与前一个数字交换
                while (j >= 1 && arr[j].CompareTo(arr[j - 1]) < 0)
                {
                    T temp = arr[j];
                    arr[j] = arr[j - 1];
                    arr[j - 1] = temp;
                    // 更新当前数字下标
                    j--;
                }
            }
        }
        
        static public void insertSort<T>(this T[] arr) where T : IComparable<T>
        {
            // 从第二个数开始，往前插入数字
            int length = arr.Length;
            for (int i = 1; i < length; i++)
            {
                // j 记录当前数字下标
                int j = i;
                // 当前数字比前一个数字小，则将当前数字与前一个数字交换
                while (j >= 1 && arr[j].CompareTo(arr[j - 1]) < 0)
                {
                    T temp = arr[j];
                    arr[j] = arr[j - 1];
                    arr[j - 1] = temp;
                    // 更新当前数字下标
                    j--;
                }
            }
        }

        static public void insertSort<T>(this List<T> arr, Comparison<T> comparison)
        {
            // 从第二个数开始，往前插入数字
            int length = arr.Count;
            for (int i = 1; i < length; i++)
            {
                // j 记录当前数字下标
                int j = i;
                // 当前数字比前一个数字小，则将当前数字与前一个数字交换
                while (j >= 1 && comparison(arr[j], arr[j - 1]) < 0)
                {
                    T temp = arr[j];
                    arr[j] = arr[j - 1];
                    arr[j - 1] = temp;
                    // 更新当前数字下标
                    j--;
                }
            }
        }

        static public void insertSort<T>(this T[] arr, Comparison<T> comparison)
        {
            // 从第二个数开始，往前插入数字
            int length = arr.Length;
            for (int i = 1; i < length; i++)
            {
                // j 记录当前数字下标
                int j = i;
                // 当前数字比前一个数字小，则将当前数字与前一个数字交换
                while (j >= 1 && comparison(arr[j], arr[j - 1]) < 0)
                {
                    T temp = arr[j];
                    arr[j] = arr[j - 1];
                    arr[j - 1] = temp;
                    // 更新当前数字下标
                    j--;
                }
            }
        }



        // static public void InsertSort2<T>(this List<T> arr) where T : IComparable<T>
        // {
        //     // 从第二个数开始，往前插入数字
        //     int length = arr.Count;
        //     for (int i = 1; i < length; i++)
        //     {
        //         T currentNumber = arr[i];
        //         int j = i - 1;
        //         // 寻找插入位置的过程中，不断地将比 currentNumber 大的数字向后挪
        //         while (j >= 0 && currentNumber.CompareTo(arr[j]) < 0)
        //         {
        //             arr[j + 1] = arr[j];
        //             j--;
        //         }
        //         // 两种情况会跳出循环：1. 遇到一个小于或等于 currentNumber 的数字，跳出循环，currentNumber 就坐到它后面。
        //         // 2. 已经走到数列头部，仍然没有遇到小于或等于 currentNumber 的数字，也会跳出循环，此时 j 等于 -1，currentNumber 就坐到数列头部。
        //         arr[j + 1] = currentNumber;
        //     }
        // }        
    }
}