
namespace CacheMech
{
    /// <summary>
    /// 可缓存对象接口
    /// </summary>
    public interface IPooledObject
    {
        /// <summary>
        /// 创建时的回调
        /// </summary>
        void OnInit();

        /// <summary>
        /// 从对象池中拿出时的回调
        /// </summary>
        void OnGet();

        /// <summary>
        /// 放回对象池时的回调
        /// </summary>
        void OnRelease();

        /// <summary>
        /// 放回对象池
        /// </summary>
        void ReturnToPool();

        IPool Pool { get; set; }
    }
}