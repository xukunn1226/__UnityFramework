* IPool、IPooledObject
* 基于接口的设计：IPooledObject——不同对象因应用需求不同，进出缓存池行为差异性大
	特效还原状态（脚本，ParticleSystem，TrailRender）
	物理属性
	UI（disable、enable、out of screen）

	IPool——缓存策略的差异性、属性的差异性(PreAllocateAmount, LimitAmount, TrimAbove)

* 四种使用方式
	1、静态IPool + 静态IPooledObject
	2、静态IPool + 动态IPooledObject
	3、动态IPool + 静态IPooledObject
	4、动态IPool + 动态IPooledObject
* 待缓存对象可静态配置MonoPooledObjectBase，如果此资源不被对象池管理则不受影响
* 同一个资源可被不同对象池使用