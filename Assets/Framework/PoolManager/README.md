# PoolManager


## 类图
![image](https://github.com/xukunn1226/PoolManager/blob/master/Images/20191011135925.png)

## 效果图
![image](https://github.com/xukunn1226/PoolManager/blob/master/Images/SpawnerRing.gif)

是不是跟[catlikecoding](https://catlikecoding.com/unity/tutorials/object-pools/)很像？好的东西喜欢人总是很多，参考了部分代码及资源^_^


![image](https://github.com/xukunn1226/PoolManager/blob/master/Images/Decal.gif)

FPS游戏中弹痕数量很多，优化思路是超过一定数量的Decal调整其生命周期，加快回收，达到减少同时在场景中存在的目的。示例中5个以内的Decal维持生命周期（5s）不变，超过5个的将加速播放。


## 功能
- 基于Prefab与System.Object的缓存对象分开管理
- 灵活的扩展框架，可自定义**缓存对象**及**缓存池**，满足不同应用场合需求
- **PoolManager**统一管理所有缓存池
- 内置强大的几款常用缓存池
    - ObjectPool：System.object对象池
    - PrefabObjectPool：MonoBehaviour对象池
    - LivingPrefabObjectPool：具有生命周期

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
