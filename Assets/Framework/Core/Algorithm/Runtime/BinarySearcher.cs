using System.Collections;
using System.Collections.Generic;
using System;

namespace Framework.Core
{
    static public partial class Algorithm
    {
        public class BinarySearcher<T> : IEnumerator<T>
        {
            private readonly IList<T>       m_Collection;
            private readonly Comparer<T>    m_Comparer;
            private T                       m_Item;
            private int                     m_CurrentItemIndex;
            private int                     m_LeftIndex;
            private int                     m_RightIndex;

            /// <summary>
            /// The value of the current item
            /// </summary>
            public T Current
            {
                get
                {
                    return m_Collection[m_CurrentItemIndex];
                }
            }

            object IEnumerator.Current => Current;

            /// <summary>
            /// Class constructor
            /// </summary>
            /// <param name="collection">A list</param>
            /// <param name="comparer">A comparer</param>
            public BinarySearcher(IList<T> collection, Comparer<T> comparer)
            {
                if (collection == null)
                {
                    throw new NullReferenceException("List is null");
                }
                m_Collection = collection;
                m_Comparer = comparer;
                m_Collection.HeapSort(comparer);
            }

            /// <summary>
            /// Apply Binary Search in a list.
            /// </summary>
            /// <param name="item">The item we search</param>
            /// <returns>If item found, its' index, -1 otherwise</returns>
            public int BinarySearch(T item)
            {
                bool notFound = true;

                if (item == null)
                {
                    throw new NullReferenceException("Item to search for is not set");
                }
                Reset();
                m_Item = item;

                while ((m_LeftIndex <= m_RightIndex) && notFound)
                {
                    notFound = MoveNext();
                }

                if (notFound)
                {
                    Reset();
                }
                return m_CurrentItemIndex;
            }

            /// <summary>
            /// An implementation of IEnumerator's MoveNext method.
            /// </summary>
            /// <returns>true if iteration can proceed to the next item, false otherwise</returns>
            public bool MoveNext()
            {
                m_CurrentItemIndex = this.m_LeftIndex + (this.m_RightIndex - this.m_LeftIndex) / 2;

                if (m_Comparer.Compare(m_Item, Current) < 0)
                {
                    m_RightIndex = m_CurrentItemIndex - 1;
                }
                else if (m_Comparer.Compare(m_Item, Current) > 0)
                {
                    m_LeftIndex = m_CurrentItemIndex + 1;
                }
                else
                {
                    return false;
                }
                return true;
            }

            public void Reset()
            {
                this.m_CurrentItemIndex = -1;
                m_LeftIndex = 0;
                m_RightIndex = m_Collection.Count - 1;
            }

            public void Dispose()
            {
                //not implementing this, since there are no managed resources to release 
            }
        }
    }
}

