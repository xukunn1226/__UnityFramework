using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Cache;

namespace Cache.Tests
{
    /// <summary>
    /// 如何使用对象池实例化对象
    /// </summary>
    public class SpawnExample : MonoBehaviour
    {
        public Stuff[] Prefabs;

        IEnumerator Start()
        {
            yield return new WaitForSeconds(1);

            StartCoroutine(Spawn());
        }

        IEnumerator Spawn()
        {
            yield return new WaitForSeconds(1);

            SpawnStuff();

            StartCoroutine(Spawn());
        }

        // Method1：不创建特定Pool，使用MonoPooledObjectBase.Poo.Get()
        void SpawnStuff()
        {
            Stuff prefabAsset = Prefabs[Random.Range(0, Prefabs.Length)];

            Stuff inst = (Stuff)PoolManager.GetOrCreatePool(prefabAsset.gameObject).Get();

            inst.transform.localPosition = Random.insideUnitSphere * 5;
            inst.transform.localRotation = Random.rotation;
            inst.Body.useGravity = true;
        }

        private void OnGUI()
        {
            if (GUI.Button(new Rect(100, 100, 150, 80), "Run"))
            {
                StartCoroutine(Spawn());
            }

            if(GUI.Button(new Rect(100, 300, 150, 80), "Clear Pool"))
            {
                foreach (var prefabAsset in Prefabs)
                {
                    PoolManager.RemoveMonoPool<PrefabObjectPool>(prefabAsset);
                }
            }
        }
    }
}