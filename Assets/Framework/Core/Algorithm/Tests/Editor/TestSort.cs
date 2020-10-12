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
            // Sort.BubbleSort(arr);

            Sort.BubbleSort<int>(arr, Comparer<int>.Default);
        }
    }
}