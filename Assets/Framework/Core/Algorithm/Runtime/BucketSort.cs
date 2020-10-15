using System.Collections;
using System.Collections.Generic;
using System;

namespace Framework.Core
{
    static public partial class Algorithm
    {
        /**
         * @name: 
         * @msg: 时间复杂度O(m+n)，空间复杂度O(m+n)，稳定排序
         * @param {type} 
         * @return {type} 
         */
        static public void BucketSort(this IList<int> arr, bool isAscending = true)
        {
            int maxValue = arr.Max();
            int minValue = arr.Min();
            
            List<int>[] bucket = new List<int>[maxValue - minValue + 1];

            for (int i = 0; i < bucket.Length; i++)
            {
                bucket[i] = new List<int>();
            }

            foreach (int i in arr)
            {
                bucket[i - minValue].Add(i);
            }

            if (isAscending)
            {
                int k = 0;
                foreach (List<int> i in bucket)
                {
                    if (i.Count > 0)
                    {
                        foreach (int j in i)
                        {
                            arr[k] = j;
                            k++;
                        }
                    }
                }
            }
            else
            {
                int k = arr.Count - 1;
                foreach(List<int> i in bucket)
                {
                    if(i.Count > 0)
                    {
                        foreach(int j in i)
                        {
                            arr[k] = j;
                            k--;
                        }
                    }
                }
            }
        }

        static private int Max(this IList<int> arr)
        {
            int max = int.MinValue;
            foreach(int i in arr)
            {
                if(i > max)
                    max = i;
            }
            return max;
        }

        static private int Min(this IList<int> arr)
        {
            int min = int.MaxValue;
            foreach(int i in arr)
            {
                if(i < min)
                    min = i;
            }
            return min;
        }
    }
}