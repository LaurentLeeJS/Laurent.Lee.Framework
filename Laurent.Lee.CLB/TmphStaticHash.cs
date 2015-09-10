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
    ///     静态哈希基类
    /// </summary>
    /// <typeparam name="TValueType">数据类型</typeparam>
    public abstract class TmphStaticHash<TValueType> : TmphStaticHashIndex
    {
        /// <summary>
        ///     哈希数据数组
        /// </summary>
        protected TValueType[] Array;

        /// <summary>
        ///     是否空集合
        /// </summary>
        /// <returns>是否空集合</returns>
        public unsafe bool IsEmpty()
        {
            fixed (TmphRange* indexFixed = Indexs)
            {
                for (var index = indexFixed + Indexs.Length; index != indexFixed;)
                {
                    --index;
                    if ((*index).StartIndex != (*index).EndIndex) return false;
                }
            }
            return true;
        }

        /// <summary>
        ///     是否存在匹配数据
        /// </summary>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>是否存在匹配数据</returns>
        public unsafe bool IsExist(Func<TValueType, bool> isValue)
        {
            fixed (TmphRange* indexFixed = Indexs)
            {
                for (var index = indexFixed + Indexs.Length; index != indexFixed;)
                {
                    for (var range = *--index; range.StartIndex != range.EndIndex; ++range.StartIndex)
                    {
                        if (isValue(Array[range.StartIndex])) return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        ///     移除数据
        /// </summary>
        /// <typeparam name="TKeyType">哈希键类型</typeparam>
        /// <param name="key">哈希键值</param>
        /// <param name="getKey">哈希键值获取器</param>
        /// <param name="value">被移除数据</param>
        /// <returns>是否存在被移除数据</returns>
        protected bool Remove<TKeyType>(TKeyType key, Func<TValueType, TKeyType> getKey, out TValueType value)
            where TKeyType : IEquatable<TKeyType>
        {
            var index = (GetHashCode(key) & IndexAnd);
            for (var range = Indexs[index]; range.StartIndex != range.EndIndex; ++range.StartIndex)
            {
                value = Array[range.StartIndex];
                if (getKey(value).Equals(key))
                {
                    Indexs[index].EndIndex = --range.EndIndex;
                    Array[range.StartIndex] = Array[range.EndIndex];
                    return true;
                }
            }
            value = default(TValueType);
            return false;
        }

        /// <summary>
        ///     数据转换成单向动态数组
        /// </summary>
        /// <returns>单向动态数组</returns>
        public unsafe TmphSubArray<TValueType> GetList()
        {
            var values = new TmphSubArray<TValueType>(Array.Length);
            fixed (TmphRange* indexFixed = Indexs)
            {
                for (var index = indexFixed + Indexs.Length; index != indexFixed;)
                {
                    for (var range = *--index; range.StartIndex != range.EndIndex; ++range.StartIndex)
                    {
                        values.UnsafeAdd(Array[range.StartIndex]);
                    }
                }
            }
            return values;
        }

        /// <summary>
        ///     获取匹配的数据集合
        /// </summary>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>匹配的数据集合</returns>
        protected unsafe TmphSubArray<TValueType> GetList(Func<TValueType, bool> isValue)
        {
            var values = new TmphSubArray<TValueType>(Array.Length);
            fixed (TmphRange* indexFixed = Indexs)
            {
                for (var index = indexFixed + Indexs.Length; index != indexFixed;)
                {
                    for (var range = *--index; range.StartIndex != range.EndIndex; ++range.StartIndex)
                    {
                        var value = Array[range.StartIndex];
                        if (isValue(value)) values.UnsafeAdd(value);
                    }
                }
            }
            return values;
        }

        /// <summary>
        ///     获取匹配的数据集合
        /// </summary>
        /// <typeparam name="TListType">目标数据类型</typeparam>
        /// <param name="getValue">数据转换器</param>
        /// <returns>匹配的数据集合</returns>
        protected unsafe TmphSubArray<TListType> GetList<TListType>(Func<TValueType, TListType> getValue)
        {
            var values = new TmphSubArray<TListType>(Array.Length);
            fixed (TmphRange* indexFixed = Indexs)
            {
                for (var index = indexFixed + Indexs.Length; index != indexFixed;)
                {
                    for (var range = *--index; range.StartIndex != range.EndIndex; ++range.StartIndex)
                    {
                        values.UnsafeAdd(getValue(Array[range.StartIndex]));
                    }
                }
            }
            return values;
        }
    }
}