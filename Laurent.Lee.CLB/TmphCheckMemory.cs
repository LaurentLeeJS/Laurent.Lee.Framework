using System;

namespace Laurent.Lee.CLB
{
    /// <summary>
    ///     内存检测
    /// </summary>
    public static class TmphCheckMemory
    {
        /// <summary>
        ///     检测类型集合
        /// </summary>
        private static TmphArrayPool<Type> _pool = TmphArrayPool<Type>.Create();

        /// <summary>
        ///     添加类型
        /// </summary>
        /// <param name="type"></param>
        public static void Add(Type type)
        {
            _pool.Push(type);
        }

        /// <summary>
        ///     获取检测类型集合
        /// </summary>
        /// <returns></returns>
        public static TmphSubArray<Type> GetTypes()
        {
            var count = _pool.Count;
            return TmphSubArray<Type>.Unsafe(_pool.Array, 0, count);
        }
    }
}