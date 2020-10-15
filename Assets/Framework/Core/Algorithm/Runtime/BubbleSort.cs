using System.Collections;
using System.Collections.Generic;
using System;

namespace Framework.Core
{
    static public partial class Algorithm
    {
        ////////////////////////////////// BubbleSort
        /**
         * @name: 
         * @msg: 时间复杂度O(n^2)，空间复杂度O(1)，稳定排序
         * @param {type} 
         * @return {type} 
         */
        static public void BubbleSort<T>(this IList<T> arr, Comparer<T> comparer = null)
        {
            comparer = comparer ?? Comparer<T>.Default;

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
        

        ////////////////////////////////// Fast BubbleSort
        static public void FastBubbleSort<T>(this IList<T> arr, Comparer<T> comparer = null)
        {
            comparer = comparer ?? Comparer<T>.Default;

            bool swapped = true;            
            int indexOfLastUnsortedElement = arr.Count - 1;        // 最后一个没有经过排序的元素的下标            
            int swappedIndex = -1;                                  // 上次发生交换的位置
            while (swapped)
            {
                swapped = false;
                for (int i = 0; i < indexOfLastUnsortedElement; i++)
                {
                    if(comparer.Compare(arr[i], arr[i + 1]) > 0)
                    {
                        T temp = arr[i];
                        arr[i] = arr[i + 1];
                        arr[i + 1] = temp;
                        
                        swapped = true;
                        swappedIndex = i;
                    }
                }
                // 最后一个没有经过排序的元素的下标就是最后一次发生交换的位置
                indexOfLastUnsortedElement = swappedIndex;
            }
        }
    }
}