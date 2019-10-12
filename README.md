# PoolManager


## 类图
![image](https://github.com/xukunn1226/PoolManager/blob/master/Images/20191011135925.png)


## 功能
- 基于Prefab与System.Object的缓存对象分开管理
- 灵活的扩展框架，可自定义**缓存对象**及**缓存池**，满足不同应用场合需求
- **PoolManager**统一管理所有缓存池
- 内置强大的几款常用缓存池
    - ObjectPool
    - PrefabObjectPool
    - AdjustedPrefabObjectPool

## 如何使用
> **基于System.Object缓存对象的使用**

1. 创建`public class Foo : IPooledObject`
2. 使用
    ```C#
    Foo f = Foo.Get();
    Foo.Release(f);
    ```

> **基于MonoBehaviour缓存对象的使用**

1. 创建`public class Stuff : MonoPooledObjectBase`
2. 使用
    ```C#
    Stuff prefab = Prefabs[Random.Range(0, Prefabs.Length)];

    Stuff inst = (Stuff)prefab.Pool.Get();      // 内部默认创建了PrefabObjectPool
    inst.transform.localPosition = Random.insideUnitSphere * 5;
    inst.transform.localRotation = Random.rotation;
    ```

    ```C#
    Stuff prefab = Prefabs[Random.Range(0, Prefabs.Length)];

    // 内部查找操作影响性能，建议持有Pool对象
    PrefabObjectPool pool = PoolManager.GetOrCreatePool<PrefabObjectPool>(prefab);
    Stuff inst = (Stuff)pool.Get();

    inst.transform.localPosition = Random.insideUnitSphere * 5;
    inst.transform.localRotation = Random.rotation;
    inst.Body.useGravity = false;
    ```
> **PoolManager**    
    ```C#
    PrefabObjectPool GetOrCreatePool(MonoPooledObjectBase asset)
    T GetOrCreatePool<T>(MonoPooledObjectBase asset)
    void RemovePool<T>(MonoPooledObjectBase asset)
    MonoPoolBase GetPool(MonoPooledObjectBase asset)
    ```
