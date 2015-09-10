/*
 *  Copyright 2015 Tony Lee

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
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