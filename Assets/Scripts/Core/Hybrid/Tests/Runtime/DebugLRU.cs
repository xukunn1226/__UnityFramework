using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core;

public class DebugLRU : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //TestLRU();
        TestLRUK();
    }

    void TestLRU()
    {
        LRUQueue<int, string> lru = new LRUQueue<int, string>(3);
        lru.OnDiscard += OnDiscard;

        lru.GetOrCreate(1, "aa");
        lru.PrintIt();

        lru.GetOrCreate(2, "bb");
        lru.PrintIt();

        lru.GetOrCreate(3, "cc");
        lru.PrintIt();

        lru.GetOrCreate(2, "bb");
        lru.PrintIt();

        lru.GetOrCreate(2, "ssf");
        lru.PrintIt();

        lru.GetOrCreate(4, "dd");
        lru.PrintIt();
    }

    private void OnDiscard(int key, string value)
    {
        Debug.Log($"OnDiscard {key} {value}");
    }

    void TestLRUK()
    {
        LRUKQueue<int, string> lru = new LRUKQueue<int, string>(3, 2);
        lru.OnDiscard += OnDiscard;

        lru.GetOrCreate(1, "aa");
        lru.PrintIt();

        lru.GetOrCreate(2, "bb");
        lru.PrintIt();

        lru.GetOrCreate(3, "cc");
        lru.PrintIt();

        lru.GetOrCreate(2, "bb");       // 再次访问，放入缓存队列
        lru.PrintIt();

        lru.GetOrCreate(4, "dd");
        lru.PrintIt();

        lru.GetOrCreate(5, "ee");       // 从历史队列移动至缓存队列
        lru.PrintIt();

        lru.GetOrCreate(3, "cc");
        lru.PrintIt();
    }
}
