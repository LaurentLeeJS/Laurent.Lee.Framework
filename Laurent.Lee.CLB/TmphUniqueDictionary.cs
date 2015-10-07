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
    ///     唯一静态哈希字典
    /// </summary>
    /// <typeparam name="TKeyType">键值类型</typeparam>
    /// <typeparam name="TValueType">数据类型</typeparam>
    public sealed class TmphUniqueDictionary<TKeyType, TValueType> where TKeyType : struct, IEquatable<TKeyType>
    {
        /// <summary>
        ///     哈希数据数组
        /// </summary>
        private readonly TmphKeyValue<TKeyType, TValueType>[] _array;

        /// <summary>
        ///     唯一静态哈希字典
        /// </summary>
        /// <param name="keys">键值数据集合</param>
        /// <param name="getValue">数据获取器</param>
        /// <param name="size">哈希容器尺寸</param>
        public unsafe TmphUniqueDictionary(TKeyType[] keys, Func<TKeyType, TValueType> getValue, int size)
        {
            var count = keys.length();
            if (count > size || size <= 0) TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            if (getValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            _array = new TmphKeyValue<TKeyType, TValueType>[size];
            if (count != 0)
            {
                var length = ((size + 31) >> 5) << 2;
                byte* isValue = stackalloc byte[length];
                var map = new TmphFixedMap(isValue, length);
                foreach (var key in keys)
                {
                    var index = key.GetHashCode();
                    if ((uint)index >= size) TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
                    if (map.Get(index)) TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
                    map.Set(index);
                    if (getValue != null) _array[index] = new TmphKeyValue<TKeyType, TValueType>(key, getValue(key));
                }
            }
        }

        /// <summary>
        ///     唯一静态哈希字典
        /// </summary>
        /// <param name="values">数据集合</param>
        /// <param name="size">哈希容器尺寸</param>
        public TmphUniqueDictionary(TmphList<TmphKeyValue<TKeyType, TValueType>> values, int size)
        {
            var count = values.Count();
            if (count > size || size <= 0) TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            _array = new TmphKeyValue<TKeyType, TValueType>[size];
            if (count != 0) FromArray(values.array, count, size);
        }

        /// <summary>
        ///     唯一静态哈希字典
        /// </summary>
        /// <param name="values">数据集合</param>
        /// <param name="size">哈希容器尺寸</param>
        public TmphUniqueDictionary(TmphKeyValue<TKeyType, TValueType>[] values, int size)
        {
            var count = values.length();
            if (count > size || size <= 0) TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            _array = new TmphKeyValue<TKeyType, TValueType>[size];
            if (count != 0) FromArray(values, count, size);
        }

        /// <summary>
        ///     唯一静态哈希字典
        /// </summary>
        /// <param name="values">数据集合</param>
        /// <param name="count">数据数量</param>
        /// <param name="size">哈希容器尺寸</param>
        private unsafe void FromArray(TmphKeyValue<TKeyType, TValueType>[] values, int count, int size)
        {
            var length = ((size + 31) >> 5) << 2;
            byte* isValue = stackalloc byte[length];
            var map = new TmphFixedMap(isValue, length);
            do
            {
                var value = values[--count];
                var index = value.Key.GetHashCode();
                if ((uint)index >= size) TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
                if (map.Get(index)) TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
                map.Set(index);
                _array[index] = value;
            } while (count != 0);
        }

        /// <summary>
        ///     判断是否存在某键值
        /// </summary>
        /// <param name="key">待匹配键值</param>
        /// <returns>是否存在某键值</returns>
        public bool ContainsKey(TKeyType key)
        {
            var index = key.GetHashCode();
            return (uint)index < _array.Length && key.Equals(_array[index].Key);
        }

        /// <summary>
        ///     获取匹配数据
        /// </summary>
        /// <param name="key">哈希键值</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>匹配数据,失败返回默认空值</returns>
        public TValueType Get(TKeyType key, TValueType nullValue)
        {
            var index = key.GetHashCode();
            return (uint)index < _array.Length && key.Equals(_array[index].Key) ? _array[index].Value : nullValue;
        }
    }
}