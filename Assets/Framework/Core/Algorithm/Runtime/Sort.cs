using System.Collections;
using System.Collections.Generic;
using System;

namespace Framework.Core
{
    static public class Sort
    {
        static public void BubbleSort<T>(List<T> arr) where T : IComparable<T>
        {
            bool swapped = true;
            int length = arr.Count;
            for(int outer = 0; outer < length - 1; ++outer)
            {
                if(!swapped) break;         // 上一轮没有发生交换，说明剩余部分已经有序

                swapped = false;
                for(int inner = 0; inner < length - 1 - outer; ++inner)
                {
                    if(arr[inner].CompareTo(arr[inner + 1]) > 0)
                    {
                        T tmp = arr[inner + 1];
                        arr[inner + 1] = arr[inner];
                        arr[inner] = tmp;
                        swapped = true;
                    }
                }
            }
        }

        static public void BubbleSort<T>(List<T> arr, Comparison<T> comparison)
        {
            bool swapped = true;
            int length = arr.Count;
            for(int outer = 0; outer < length - 1; ++outer)
            {
                if(!swapped) break;         // 上一轮没有发生交换，说明剩余部分已经有序

                swapped = false;
                for(int inner = 0; inner < length - 1 - outer; ++inner)
                {
                    if(comparison(arr[inner], arr[inner + 1]) > 0)
                    {
                        T tmp = arr[inner + 1];
                        arr[inner + 1] = arr[inner];
                        arr[inner] = tmp;
                        swapped = true;
                    }
                }
            }
        }

        public static void BubbleSort<T>(List<T> arr, IComparer<T> comparer)
        {
            if (comparer == null) comparer = Comparer<T>.Default;

            bool swapped = true;
            int length = arr.Count;
            for(int outer = 0; outer < length - 1; ++outer)
            {
                if(!swapped) break;         // 上一轮没有发生交换，说明剩余部分已经有序

                swapped = false;
                for(int inner = 0; inner < length - 1 - outer; ++inner)
                {
                    if(comparer.Compare(arr[inner], arr[inner + 1]) > 0)
                    {
                        T tmp = arr[inner + 1];
                        arr[inner + 1] = arr[inner];
                        arr[inner] = tmp;
                        swapped = true;
                    }
                }
            }
        }
        
        // static public void BubbleSort<T>(List<T> arr, IComparer<T> comparison) where T : IComparable
        // {
        //     bool swapped = true;
        //     int length = arr.Count;
        //     for(int i = 0; i < length - 1; ++i)
        //     {
        //         if(!swapped) break;         // 上一轮没有发生交换，说明剩余部分已经有序

        //         swapped = false;
        //         for(int j = 0; j < length - 1 - i; ++j)
        //         {
        //             if(comparison.Compare(arr[j], arr[j + 1]) > 0)
        //             {
        //                 T tmp = arr[j + 1];
        //                 arr[j + 1] = arr[j];
        //                 arr[j] = tmp;
        //                 swapped = true;
        //             }
        //         }
        //     }
        // }

        // static public void BubbleSort(List<int> arr)
        // {
        //     BubbleSort<int>(arr, s_Comp);
        // }

        // static private readonly Comparison<int> s_Comp = Default_Comparison;
        // static private int Default_Comparison(int a, int b)// where T : IComparable
        // {
        //     return a.CompareTo(b);
        // }
    }
}