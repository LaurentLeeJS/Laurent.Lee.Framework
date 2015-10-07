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