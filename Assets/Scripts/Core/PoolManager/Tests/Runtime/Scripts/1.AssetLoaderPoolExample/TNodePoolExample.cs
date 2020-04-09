﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cache.Tests
{
    public class TNodePoolExample : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            TNode f1 = TNode.Get();
            TNode.Get();
            TNode.Get();
            TNode.Get();
            TNode.Get();

            TNode.Release(f1);

            //// 并没有真正销毁对象池，仅从PoolManager注销，仍被AssetLoader持有
            //PoolManager.UnregisterObjectPool(typeof(TNode));

            //// dangerous call:out of control
            //TNode.Get();
        }

        private void OnGUI()
        {
            if (GUI.Button(new Rect(100, 100, 150, 80), "Clear"))
            {
                PoolManager.RemoveObjectPool(typeof(TNode));
            }
        }
    }
}