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
        static public void HeapSort<T>(this IList<T> arr, Comparer<T> comparer = null)
        {
            comparer = comparer ?? Comparer<T>.Default;

            int lastIndex = arr.Count - 1;
            arr.BuildMaxHeap(0, lastIndex, comparer);

            while (lastIndex >= 0)
            {
                arr.Swap(0, lastIndex);
                lastIndex--;
                arr.MaxHeapify(0, lastIndex, comparer);
            }
        }

        private static void BuildMaxHeap<T>(this IList<T> arr, int firstIndex, int lastIndex, Comparer<T> comparer)
        {
            int lastNodeWithChildren = lastIndex / 2;

            for (int node = lastNodeWithChildren; node >= 0; --node)
            {
                arr.MaxHeapify(node, lastIndex, comparer);
            }
        }

        static private void MaxHeapify<T>(this IList<T> arr, int nodeIndex, int lastIndex, Comparer<T> comparer)
        {
            // assume left(i) and right(i) are max-heaps
            int left = (nodeIndex * 2) + 1;
            int right = left + 1;
            int largest = nodeIndex;

            // If collection[left] > collection[nodeIndex]
            if (left <= lastIndex && comparer.Compare(arr[left], arr[nodeIndex]) > 0)
                largest = left;

            // If collection[right] > collection[largest]
            if (right <= lastIndex && comparer.Compare(arr[right], arr[largest]) > 0)
                largest = right;

            // Swap and heapify
            if (largest != nodeIndex)
            {
                arr.Swap(nodeIndex, largest);
                arr.MaxHeapify(largest, lastIndex, comparer);
            }
        }
    }
}
