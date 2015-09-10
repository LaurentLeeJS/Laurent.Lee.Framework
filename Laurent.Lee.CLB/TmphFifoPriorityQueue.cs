using System;
using System.Collections.Generic;

namespace Laurent.Lee.CLB
{
    /// <summary>
    ///     先进先出优先队列
    /// </summary>
    /// <typeparam name="TKeyType">键值类型</typeparam>
    /// <typeparam name="TValueType">数据类型</typeparam>
    public sealed class TmphFifoPriorityQueue<TKeyType, TValueType> where TKeyType : IEquatable<TKeyType>
    {
        /// <summary>
        ///     数据集合
        /// </summary>
        private readonly Dictionary<TKeyType, TmphNode> values = TmphDictionary<TKeyType>.Create<TmphNode>();

        /// <summary>
        ///     为节点
        /// </summary>
        private TmphNode end;

        /// <summary>
        ///     头节点
        /// </summary>
        private TmphNode header;

        /// <summary>
        ///     数据数量
        /// </summary>
        public int Count
        {
            get { return values.Count; }
        }

        /// <summary>
        ///     数据对象
        /// </summary>
        /// <param name="key">查询键值</param>
        /// <returns>数据对象</returns>
        public TValueType this[TKeyType key]
        {
            get
            {
                var node = getNode(key);
                return node != null ? node.Value : default(TValueType);
            }
            set { Set(key, value); }
        }

        /// <summary>
        ///     获取数据
        /// </summary>
        /// <param name="key">键值</param>
        /// <returns>数据对象</returns>
        private TmphNode getNode(TKeyType key)
        {
            TmphNode node;
            if (values.TryGetValue(key, out node))
            {
                if (node != end)
                {
                    var previous = node.Previous;
                    if (previous == null) (header = node.Next).Previous = null;
                    else (previous.Next = node.Next).Previous = previous;
                    end.Next = node;
                    node.Previous = end;
                    node.Next = null;
                    end = node;
                }
            }
            return node;
        }

        /// <summary>
        ///     判断是否存在键值
        /// </summary>
        /// <param name="key">键值</param>
        /// <returns>是否存在键值</returns>
        public bool ContainsKey(TKeyType key)
        {
            return values.ContainsKey(key);
        }

        /// <summary>
        ///     获取数据
        /// </summary>
        /// <param name="key">键值</param>
        /// <param name="nullValue">失败空值</param>
        /// <returns>数据对象</returns>
        public TValueType Get(TKeyType key, TValueType nullValue)
        {
            var node = getNode(key);
            return node != null ? node.Value : nullValue;
        }

        ///// <summary>
        ///// 获取数据(不调整位置)
        ///// </summary>
        ///// <param name="key">键值</param>
        ///// <param name="nullValue">失败空值</param>
        ///// <returns>数据对象</returns>
        //public TValueType GetOnly(TKeyType key, TValueType nullValue)
        //{
        //    node value;
        //    return values.TryGetValue(key, out value) ? value.Value : nullValue;
        //}
        /// <summary>
        ///     设置数据
        /// </summary>
        /// <param name="key">键值</param>
        /// <param name="value">数据对象</param>
        /// <returns>被替换的数据对象,没有返回default(TValueType)</returns>
        public TValueType Set(TKeyType key, TValueType value)
        {
            var node = getNode(key);
            if (node != null)
            {
                var oldValue = node.Value;
                node.Value = value;
                return oldValue;
            }
            values.Add(key, node = new TmphNode { Value = value, Key = key, Previous = end });
            if (end == null) header = end = node;
            else
            {
                end.Next = node;
                end = node;
            }
            return default(TValueType);
        }

        /// <summary>
        ///     弹出一个键值对
        /// </summary>
        /// <returns>键值对</returns>
        public TmphKeyValue<TKeyType, TValueType> Pop()
        {
            if (header != null)
            {
                var node = header;
                if ((header = header.Next) == null) end = null;
                else header.Previous = null;
                values.Remove(node.Key);
                return new TmphKeyValue<TKeyType, TValueType>(node.Key, node.Value);
            }
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            return default(TmphKeyValue<TKeyType, TValueType>);
        }

        /// <summary>
        ///     清除数据
        /// </summary>
        public void Clear()
        {
            values.Clear();
            header = end = null;
        }

        /// <summary>
        ///     删除一个数据
        /// </summary>
        /// <param name="key">键值</param>
        /// <param name="value">被删除数据对象</param>
        /// <returns>是否删除了数据对象</returns>
        public bool Remove(TKeyType key, out TValueType value)
        {
            TmphNode node;
            if (values.TryGetValue(key, out node))
            {
                values.Remove(key);
                if (node.Previous == null)
                {
                    header = node.Next;
                    if (header == null) end = null;
                    else header.Previous = null;
                }
                else if (node.Next == null) (end = node.Previous).Next = null;
                else
                {
                    node.Previous.Next = node.Next;
                    node.Next.Previous = node.Previous;
                }
                value = node.Value;
                return true;
            }
            value = default(TValueType);
            return false;
        }

        /// <summary>
        ///     数据节点
        /// </summary>
        private sealed class TmphNode
        {
            /// <summary>
            ///     键值
            /// </summary>
            public TKeyType Key;

            /// <summary>
            ///     后一个节点
            /// </summary>
            public TmphNode Next;

            /// <summary>
            ///     前一个节点
            /// </summary>
            public TmphNode Previous;

            /// <summary>
            ///     数据
            /// </summary>
            public TValueType Value;
        }
    }
}