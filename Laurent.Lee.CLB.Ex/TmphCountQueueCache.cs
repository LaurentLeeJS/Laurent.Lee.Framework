/*
-------------------------------------------------- -----------------------------------------
The frame content is protected by copyright law. In order to facilitate individual learning,
allows to download the program source information, but does not allow individuals or a third
party for profit, the commercial use of the source information. Without consent,
does not allow any form (even if partial, or modified) database storage,
copy the source of information. If the source content provided by third parties,
which corresponds to the third party content is also protected by copyright.

If you are found to have infringed copyright behavior, please give me a hint. THX!

Here in particular it emphasized that the third party is not allowed to contact addresses
published in this "version copyright statement" to send advertising material.
I will take legal means to resist sending spam.
-------------------------------------------------- ----------------------------------------
The framework under the GNU agreement, Detail View GNU License.
If you think about this item affection join the development team,
Please contact me: LaurentLeeJS@gmail.com
-------------------------------------------------- ----------------------------------------
Laurent.Lee.Framework Coded by Laurent Lee
*/

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