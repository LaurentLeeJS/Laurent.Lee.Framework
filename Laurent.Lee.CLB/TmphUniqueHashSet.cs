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
    ///     唯一静态哈希
    /// </summary>
    /// <typeparam name="TValueType">数据类型</typeparam>
    public sealed class TmphUniqueHashSet<TValueType> where TValueType : struct, IEquatable<TValueType>
    {
        /// <summary>
        ///     哈希数据数组
        /// </summary>
        private readonly TValueType[] _array;

        /// <summary>
        ///     唯一静态哈希
        /// </summary>
        /// <param name="values">数据集合</param>
        /// <param name="size">哈希容器尺寸</param>
        public unsafe TmphUniqueHashSet(TValueType[] values, int size)
        {
            if (values.length() > size || size <= 0) TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            _array = new TValueType[size];
            var length = ((size + 31) >> 5) << 2;
            byte* isValue = stackalloc byte[length];
            var map = new TmphFixedMap(isValue, length);
            foreach (var value in values)
            {
                var index = value.GetHashCode();
                if ((uint)index >= size) TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
                if (map.Get(index)) TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
                map.Set(index);
                _array[index] = value;
            }
        }

        /// <summary>
        ///     判断是否存在某值
        /// </summary>
        /// <param name="value">待匹配值</param>
        /// <returns>是否存在某值</returns>
        public bool Contains(TValueType value)
        {
            var index = value.GetHashCode();
            return (uint)index < _array.Length && value.Equals(_array[index]);
        }
    }
}