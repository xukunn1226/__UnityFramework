using System.Collections;
using System.Collections.Generic;
using System;
using NUnit.Framework;

namespace Framework.Core.Tests
{
    public class TestSort
    {
        [Test]
        public void TestSortSimplePasses()
        {
            List<int> arr = new List<int>() {4, 6, 1, 0, 9, 3};
            // arr.BubbleSort();
            arr.FastBubbleSort();

            List<int> arr2 = new List<int>() {4, 6, 6, 0, 9, 3, 8};
            // arr2.SelectionSort();
            arr2.FastSelectionSort();

            List<int> arr3 = new List<int>() {4, 6, 6, 0, 9, 3, 8};
            arr3.InsertSort();

            List<int> arr4 = new List<int>() {4, 5, 5, 1, 7, 6, 9, 2, 8, 0, 3, 5};
            arr4.QuickSort();
        
            List<int> arr5 = new List<int>() {4, 5, 5, 1, 7, 6, 9, 2, 8, 0, 3, 5};
            arr5.MergeSort();
        }
    }
}