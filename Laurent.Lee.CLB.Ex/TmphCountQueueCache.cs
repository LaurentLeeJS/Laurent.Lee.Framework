using System;

namespace Laurent.Lee.CLB
{
    /// <summary>
    /// 计数队列缓存
    /// </summary>
    /// <typeparam name="TKeyType">关键字类型</typeparam>
    /// <typeparam name="TValueType">数据类型</typeparam>
    public sealed class TmphCountQueueCache<TKeyType, TValueType>
        where TKeyType : struct, IEquatable<TKeyType>
        where TValueType : class
    {
        /// <summary>
        /// 缓存队列
        /// </summary>
        private TmphFifoPriorityQueue<TKeyType, TValueType> queue = new TmphFifoPriorityQueue<TKeyType, TValueType>();

        /// <summary>
        /// 最大尺寸
        /// </summary>
        private int maxSize;

        /// <summary>
        /// 计数队列
        /// </summary>
        /// <param name="maxSize">最大尺寸</param>
        public TmphCountQueueCache(int maxSize)
        {
            this.maxSize = maxSize;
            if (maxSize <= 0) maxSize = 1;
        }

        /// <summary>
        /// 获取或者设置缓存
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns>缓存值</returns>
        public TValueType this[TKeyType key]
        {
            get { return queue[key]; }
            set
            {
                queue.Set(key, value);
                if (queue.Count > maxSize) queue.Pop();
            }
        }

        /// <summary>
        /// 判断是否包含关键字
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns></returns>
        public bool ContainsKey(TKeyType key)
        {
            return queue.ContainsKey(key);
        }

        /// <summary>
        /// 清除缓存
        /// </summary>
        public void Clear()
        {
            queue.Clear();
        }

        /// <summary>
        /// 删除缓存值
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns>是否存在缓存值</returns>
        public bool Remove(TKeyType key)
        {
            TValueType value;
            return queue.Remove(key, out value);
        }
    }
}