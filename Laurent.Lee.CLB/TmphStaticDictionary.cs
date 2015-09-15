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
using System.Collections.Generic;

namespace Laurent.Lee.CLB
{
    /// <summary>
    ///     静态哈希字典
    /// </summary>
    /// <typeparam name="TKeyType">键值类型</typeparam>
    /// <typeparam name="TValueType">数据类型</typeparam>
    public class TmphStaticDictionary<TKeyType, TValueType> : TmphStaticHash<TmphKeyValue<TKeyType, TValueType>>
        where TKeyType : IEquatable<TKeyType>
    {
        /// <summary>
        ///     静态哈希字典
        /// </summary>
        /// <param name="values">初始化键值对集合</param>
        public unsafe TmphStaticDictionary(TmphKeyValue<TKeyType, TValueType>[] values)
        {
            if (values.length() != 0)
            {
                var pool = TmphUnmanagedPool.GetDefaultPool(values.Length * sizeof(int));
                var data = pool.Get(values.Length * sizeof(int));
                try
                {
                    GetValues(values, data.Int);
                }
                finally
                {
                    pool.Push(ref data);
                }
            }
            else
            {
                Indexs = DefaultIndexs;
                Array = TmphNullValue<TmphKeyValue<TKeyType, TValueType>>.Array;
            }
        }

        /// <summary>
        ///     静态哈希字典
        /// </summary>
        /// <param name="values">初始化键值对集合</param>
        public TmphStaticDictionary(IEnumerable<TmphKeyValue<TKeyType, TValueType>> values)
            : this(values.getSubArray().ToArray())
        {
        }

        /// <summary>
        ///     静态哈希字典
        /// </summary>
        /// <param name="values">初始化键值对集合</param>
        public TmphStaticDictionary(ICollection<TmphKeyValue<TKeyType, TValueType>> values)
            : this(values.GetArray())
        {
        }

        /// <summary>
        ///     静态哈希字典
        /// </summary>
        /// <param name="values">初始化键值对集合</param>
        public TmphStaticDictionary(ICollection<KeyValuePair<TKeyType, TValueType>> values)
            : this(values.GetKeyValueArray())
        {
        }

        /// <summary>
        ///     静态哈希字典
        /// </summary>
        /// <param name="values">初始化键值对集合</param>
        public TmphStaticDictionary(TmphList<TmphKeyValue<TKeyType, TValueType>> values)
            : this(values.toArray())
        {
        }

        /// <summary>
        ///     静态哈希字典
        /// </summary>
        /// <param name="values">初始化键值对集合</param>
        public TmphStaticDictionary(TmphCollection<TmphKeyValue<TKeyType, TValueType>> values)
            : this(values.toArray())
        {
        }

        /// <summary>
        ///     静态哈希字典
        /// </summary>
        /// <param name="values">数据集合</param>
        /// <param name="getKey">哈希键值获取器</param>
        public TmphStaticDictionary(IEnumerable<TValueType> values, Func<TValueType, TKeyType> getKey)
            : this(values.getSubArray(value => new TmphKeyValue<TKeyType, TValueType>(getKey(value), value)).ToArray())
        {
        }

        /// <summary>
        ///     静态哈希字典
        /// </summary>
        /// <param name="values">数据集合</param>
        /// <param name="getKey">哈希键值获取器</param>
        public TmphStaticDictionary(ICollection<TValueType> values, Func<TValueType, TKeyType> getKey)
            : this(values.GetKeyValueArray(getKey))
        {
        }

        /// <summary>
        ///     静态哈希字典
        /// </summary>
        /// <param name="values">数据集合</param>
        /// <param name="getKey">哈希键值获取器</param>
        public TmphStaticDictionary(TmphList<TValueType> values, Func<TValueType, TKeyType> getKey)
            : this(values.getKeyValueArray(getKey))
        {
        }

        /// <summary>
        ///     静态哈希字典
        /// </summary>
        /// <param name="values">数据集合</param>
        /// <param name="getKey">哈希键值获取器</param>
        public TmphStaticDictionary(TmphCollection<TValueType> values, Func<TValueType, TKeyType> getKey)
            : this(values.getKeyValueArray(getKey))
        {
        }

        /// <summary>
        ///     静态哈希字典
        /// </summary>
        /// <param name="values">数据集合</param>
        /// <param name="getKey">哈希键值获取器</param>
        public TmphStaticDictionary(TValueType[] values, Func<TValueType, TKeyType> getKey)
            : this(values.getKeyValueArray(getKey))
        {
        }

        /// <summary>
        ///     获取或设置值
        /// </summary>
        /// <param name="key">哈希键值</param>
        /// <returns>数据值</returns>
        public TValueType this[TKeyType key]
        {
            get
            {
                var index = IndexOf(key);
                if (index == -1) TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
                return Array[index].Value;
            }
            set
            {
                var index = IndexOf(key);
                if (index == -1) TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
                Array[index].Set(key, value);
            }
        }

        /// <summary>
        ///     键值集合
        /// </summary>
        public TmphSubArray<TKeyType> Keys
        {
            get { return GetList(value => value.Key); }
        }

        /// <summary>
        ///     数据集合
        /// </summary>
        public TmphSubArray<TValueType> Values
        {
            get { return GetList(value => value.Value); }
        }

        /// <summary>
        ///     匹配键值重置数据
        /// </summary>
        /// <param name="key">哈希键值</param>
        /// <param name="getValue">数据获取器</param>
        /// <returns>是否存在匹配键值</returns>
        public bool Set(TKeyType key, Func<TValueType, TValueType> getValue)
        {
            var index = IndexOf(key);
            if (index != -1)
            {
                Array[index].Set(key, getValue(Array[index].Value));
                return true;
            }
            return false;
        }

        /// <summary>
        ///     获取匹配数据
        /// </summary>
        /// <param name="key">哈希键值</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>匹配数据,失败返回默认空值</returns>
        public TValueType Get(TKeyType key, TValueType nullValue)
        {
            var index = IndexOf(key);
            return index != -1 ? Array[index].Value : nullValue;
        }

        /// <summary>
        ///     获取匹配数据
        /// </summary>
        /// <param name="key">哈希键值</param>
        /// <param name="value">匹配数据</param>
        /// <returns>是否存在匹配数据</returns>
        public bool Get(TKeyType key, ref TValueType value)
        {
            var index = IndexOf(key);
            if (index != -1)
            {
                value = Array[index].Value;
                return true;
            }
            return false;
        }

        /// <summary>
        ///     获取哈希数据数组
        /// </summary>
        /// <param name="values">数据集合</param>
        /// <param name="hashs">哈希集合</param>
        private unsafe void GetValues(TmphKeyValue<TKeyType, TValueType>[] values, int* hashs)
        {
            var hash = hashs;
            foreach (var value in values) *hash++ = GetHashCode(value.Key);
            Array = base.GetValues(values, hashs);
        }

        /// <summary>
        ///     获取键值匹配数组位置
        /// </summary>
        /// <param name="key">哈希键值</param>
        /// <returns>数组位置</returns>
        protected int IndexOf(TKeyType key)
        {
            for (var range = Indexs[GetHashCode(key) & IndexAnd];
                range.StartIndex != range.EndIndex;
                ++range.StartIndex)
            {
                if (Array[range.StartIndex].Key.Equals(key)) return range.StartIndex;
            }
            return -1;
        }

        /// <summary>
        ///     判断是否存在键值
        /// </summary>
        /// <param name="key">哈希键值</param>
        /// <returns>是否存在键值</returns>
        public bool ContainsKey(TKeyType key)
        {
            return IndexOf(key) != -1;
        }

        /// <summary>
        ///     是否存在匹配数据
        /// </summary>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>是否存在匹配数据</returns>
        public bool ContainsValue(Func<TValueType, bool> isValue)
        {
            return IsExist(value => isValue(value.Value));
        }

        /// <summary>
        ///     移除数据
        /// </summary>
        /// <param name="key">哈希键值</param>
        /// <param name="value">被移除数据</param>
        /// <returns>是否存在被移除数据</returns>
        public bool Remove(TKeyType key, out TValueType value)
        {
            TmphKeyValue<TKeyType, TValueType> keyValue;
            if (Remove(key, nextValue => nextValue.Key, out keyValue))
            {
                value = keyValue.Value;
                return true;
            }
            value = default(TValueType);
            return false;
        }

        /// <summary>
        ///     获取匹配的数据集合
        /// </summary>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>匹配的数据集合</returns>
        public TmphSubArray<TmphKeyValue<TKeyType, TValueType>> GetSubArray(Func<TValueType, bool> isValue)
        {
            return GetList(value => isValue(value.Value));
        }
    }
}