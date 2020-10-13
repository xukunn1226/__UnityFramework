using System.Collections;
using System.Collections.Generic;
using System;

namespace Framework.Core
{
    static public partial class Algorithm
    {
        ////////////////////////////////// SelectionSort
        /**
         * @name: 
         * @msg: 时间复杂度O(n^2)，空间复杂度O(1)，不稳定排序
         * @param {type} 
         * @return {type} 
         */
        static public void SelectionSort<T>(this List<T> arr) where T : IComparable<T>
        {
            int minIndex;
            int length = arr.Count;
            for (int i = 0; i < length - 1; i++)
            {
                minIndex = i;
                for (int j = i + 1; j < length; j++)
                {
                    if (arr[minIndex].CompareTo(arr[j]) > 0)
                    {
                        minIndex = j;       // 每轮遍历找到最小值下标
                    }
                }
                T temp = arr[i];
                arr[i] = arr[minIndex];
                arr[minIndex] = temp;
            }
        }

        static public void SelectionSort<T>(this T[] arr) where T : IComparable<T>
        {
            int minIndex;
            int length = arr.Length;
            for (int i = 0; i < length - 1; i++)
            {
                minIndex = i;
                for (int j = i + 1; j < length; j++)
                {
                    if (arr[minIndex].CompareTo(arr[j]) > 0)
                    {
                        minIndex = j;       // 每轮遍历找到最小值下标
                    }
                }
                T temp = arr[i];
                arr[i] = arr[minIndex];
                arr[minIndex] = temp;
            }
        }

        static public void SelectionSort<T>(this List<T> arr, Comparison<T> comparison)
        {
            int minIndex;
            int length = arr.Count;
            for (int i = 0; i < length - 1; i++)
            {
                minIndex = i;
                for (int j = i + 1; j < length; j++)
                {
                    if (comparison(arr[minIndex], arr[j]) > 0)
                    {
                        minIndex = j;       // 每轮遍历找到最小值下标
                    }
                }
                T temp = arr[i];
                arr[i] = arr[minIndex];
                arr[minIndex] = temp;
            }
        }

        static public void SelectionSort<T>(this T[] arr, Comparison<T> comparison)
        {
            int minIndex;
            int length = arr.Length;
            for (int i = 0; i < length - 1; i++)
            {
                minIndex = i;
                for (int j = i + 1; j < length; j++)
                {
                    if (comparison(arr[minIndex], arr[j]) > 0)
                    {
                        minIndex = j;       // 每轮遍历找到最小值下标
                    }
                }
                T temp = arr[i];
                arr[i] = arr[minIndex];
                arr[minIndex] = temp;
            }
        }

        ////////////////////////////////// Fast SelectionSort
        static public void FastSelectionSort<T>(this List<T> arr) where T : IComparable<T>
        {
            int minIndex, maxIndex;
            int length = arr.Count;
            for (int i = 0; i < length / 2; i++)
            {
                minIndex = i;
                maxIndex = i;
                for (int j = i + 1; j < length - i; j++)
                {
                    if (arr[minIndex].CompareTo(arr[j]) > 0)
                    {
                        minIndex = j;
                    }
                    if (arr[maxIndex].CompareTo(arr[j]) < 0)
                    {
                        maxIndex = j;
                    }
                }

                // 将最小元素交换至首位
                T temp = arr[i];
                arr[i] = arr[minIndex];
                arr[minIndex] = temp;

                // 如果最大值的下标刚好是 i，由于 arr[i] 和 arr[minIndex] 已经交换了，所以这里要更新 maxIndex 的值。
                if (maxIndex == i) maxIndex = minIndex;

                // 将最大元素交换至末尾
                int lastIndex = length - 1 - i;
                temp = arr[lastIndex];
                arr[lastIndex] = arr[maxIndex];
                arr[maxIndex] = temp;
            }
        }

        static public void FastSelectionSort<T>(this T[] arr) where T : IComparable<T>
        {
            int minIndex, maxIndex;
            int length = arr.Length;
            for (int i = 0; i < length / 2; i++)
            {
                minIndex = i;
                maxIndex = i;
                for (int j = i + 1; j < length - i; j++)
                {
                    if (arr[minIndex].CompareTo(arr[j]) > 0)
                    {
                        minIndex = j;
                    }
                    if (arr[maxIndex].CompareTo(arr[j]) < 0)
                    {
                        maxIndex = j;
                    }
                }

                // 将最小元素交换至首位
                T temp = arr[i];
                arr[i] = arr[minIndex];
                arr[minIndex] = temp;

                // 如果最大值的下标刚好是 i，由于 arr[i] 和 arr[minIndex] 已经交换了，所以这里要更新 maxIndex 的值。
                if (maxIndex == i) maxIndex = minIndex;

                // 将最大元素交换至末尾
                int lastIndex = length - 1 - i;
                temp = arr[lastIndex];
                arr[lastIndex] = arr[maxIndex];
                arr[maxIndex] = temp;
            }
        }

        static public void FastSelectionSort<T>(this List<T> arr, Comparison<T> comparison)
        {
            int minIndex, maxIndex;
            int length = arr.Count;
            for (int i = 0; i < length / 2; i++)
            {
                minIndex = i;
                maxIndex = i;
                for (int j = i + 1; j < length - i; j++)
                {
                    if (comparison(arr[minIndex], arr[j]) > 0)
                    {
                        minIndex = j;
                    }
                    if (comparison(arr[maxIndex], arr[j]) < 0)
                    {
                        maxIndex = j;
                    }
                }

                // 将最小元素交换至首位
                T temp = arr[i];
                arr[i] = arr[minIndex];
                arr[minIndex] = temp;

                // 如果最大值的下标刚好是 i，由于 arr[i] 和 arr[minIndex] 已经交换了，所以这里要更新 maxIndex 的值。
                if (maxIndex == i) maxIndex = minIndex;

                // 将最大元素交换至末尾
                int lastIndex = length - 1 - i;
                temp = arr[lastIndex];
                arr[lastIndex] = arr[maxIndex];
                arr[maxIndex] = temp;
            }
        }

        static public void FastSelectionSort<T>(this T[] arr, Comparison<T> comparison)
        {
            int minIndex, maxIndex;
            int length = arr.Length;
            for (int i = 0; i < length / 2; i++)
            {
                minIndex = i;
                maxIndex = i;
                for (int j = i + 1; j < length - i; j++)
                {
                    if (comparison(arr[minIndex], arr[j]) > 0)
                    {
                        minIndex = j;
                    }
                    if (comparison(arr[maxIndex], arr[j]) < 0)
                    {
                        maxIndex = j;
                    }
                }

                // 将最小元素交换至首位
                T temp = arr[i];
                arr[i] = arr[minIndex];
                arr[minIndex] = temp;

                // 如果最大值的下标刚好是 i，由于 arr[i] 和 arr[minIndex] 已经交换了，所以这里要更新 maxIndex 的值。
                if (maxIndex == i) maxIndex = minIndex;

                // 将最大元素交换至末尾
                int lastIndex = length - 1 - i;
                temp = arr[lastIndex];
                arr[lastIndex] = arr[maxIndex];
                arr[maxIndex] = temp;
            }
        }
    }
}