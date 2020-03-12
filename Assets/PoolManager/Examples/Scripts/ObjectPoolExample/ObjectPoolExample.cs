using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

public class ObjectPoolExample : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        AssetLoader f1 = AssetLoader.Get();
        AssetLoader f2 = AssetLoader.Get();

        AssetLoader.Release(f1);
        PoolManager.UnregisterObjectPool(typeof(AssetLoader));

        AssetLoader.Get();
    }
}
