using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections.LowLevel.Unsafe;

namespace Framework.Core
{
    static public class StringUtility
    {
        //////////////////
        // 1、修改字符串内容会使得HashCode发生变化，而字符串作为字典的Key时是用其HashCode，需要特别注意
        unsafe static public string ToLowerNoAlloc(this string str)
        {
            fixed(char* c = str)
            {
                int length = str.Length;
                for(int i = 0; i < length; ++i)
                {
                    c[i] = char.ToLower(str[i]);
                }
            }
            return str;
        }

        unsafe static public int SplitNoAlloc(this string str, char separator, string[] results)
        {
            if(results.Length == 0)
                throw new System.ArgumentException("results buffer must large than zero");

            if(str.Length == 0)
            {
                results[0] = string.Empty;
                return 1;
            }

            int length = str.Length;
            int bufSize = results.Length;
            int count = 0;
            fixed(char* p = str)
            {
                int startIndex = 0;
                for(int i = 0; i < length; ++i)
                {
                    if (p[i] == separator && count < bufSize)
                    {
                        results[count++] = i - startIndex == 0 ? string.Empty : new string(p, startIndex, i - startIndex);
                        startIndex = i + 1;
                    }
                }

                if(count < bufSize)
                {
                    if (startIndex == length)
                    {
                        results[count++] = string.Empty;
                    }
                    else
                    {
                        results[count++] = new string(p, startIndex, length - startIndex);
                    }
                }
            }
            return count;
        }

        unsafe static public void SetLength(this string str, int length)
        {
            fixed(char* p = str)
            {
                int* ptr = (int*)p;
                ptr[-1] = length;
                p[length] = '\0';
            }
        }

        unsafe static public string Substring(this string str, int startIndex, int length = 0)
        {
            if(startIndex < 0)
                throw new System.ArgumentOutOfRangeException("startIndex < 0");

            if(length <= 0)
                length = str.Length - startIndex;

            if(startIndex + length > str.Length)
                throw new System.ArgumentOutOfRangeException($"{startIndex} + {length} > {str.Length}");

            fixed(char* p = str)
            {
                UnsafeUtility.MemMove(p, p + startIndex, sizeof(char) * length);
            }
            SetLength(str, length);

            return str;
        }
    }
}