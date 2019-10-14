using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

public class ObjectPoolExample : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Foo f1 = Foo.Get();
        Foo f2 = Foo.Get();

        Foo.Release(f1);
        PoolManager.UnregisterObjectPool(typeof(Foo));

        Foo.Get();
    }
}
