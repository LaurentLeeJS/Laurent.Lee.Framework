﻿/*
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

/*Type:ulong;long;uint;int;ushort;short;byte;sbyte;double;float;DateTime*/

namespace Laurent.Lee.CLB.Template
{
    /// <summary>
    /// 可枚举相关扩展
    /// </summary>
    public static partial class TmphIEnumerable
    {
        /// <summary>
        /// 获取最大值记录
        /// </summary>
        /// <param name="values">值集合</param>
        /// <param name="value">最大值</param>
        /// <returns>是否存在最大值</returns>
        public static bool max
            (this System.Collections.Generic.IEnumerable</*Type[0]*/ulong/*Type[0]*/> values, out /*Type[0]*/ulong/*Type[0]*/ value)
        {
            if (values != null)
            {
                bool isValue = false;
                value = /*Type[0]*/ulong/*Type[0]*/.MinValue;
                foreach (/*Type[0]*/ulong/*Type[0]*/ nextValue in values)
                {
                    if (nextValue > value) value = nextValue;
                    isValue = true;
                }
                if (isValue) return true;
            }
            value = /*Type[0]*/ulong/*Type[0]*/.MinValue;
            return false;
        }

        /// <summary>
        /// 获取最大值记录
        /// </summary>
        /// <param name="values">值集合</param>
        /// <param name="value">默认空值</param>
        /// <returns>最大值,失败返回默认空值</returns>
        public static /*Type[0]*/ulong/*Type[0]*/ max
            (this System.Collections.Generic.IEnumerable</*Type[0]*/ulong/*Type[0]*/> values, /*Type[0]*/ulong/*Type[0]*/ nullValue)
        {
            /*Type[0]*/
            ulong/*Type[0]*/ value;
            return max(values, out value) ? value : nullValue;
        }

        /// <summary>
        /// 获取最大值记录
        /// </summary>
        /// <typeparam name="TValueType">值类型</typeparam>
        /// <param name="values">值集合</param>
        /// <param name="getKey">获取排序键的委托</param>
        /// <param name="value">最大值</param>
        /// <returns>是否存在最大值</returns>
        public static bool max<TValueType>
            (this System.Collections.Generic.IEnumerable<TValueType> values, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getKey
            , out TValueType value)
        {
            value = default(TValueType);
            if (values != null)
            {
                if (getKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                int count = -1;
                /*Type[0]*/
                ulong/*Type[0]*/ key = /*Type[0]*/ulong/*Type[0]*/.MinValue;
                foreach (TValueType nextValue in values)
                {
                    if (++count == 0) key = getKey(value = nextValue);
                    else
                    {
                        /*Type[0]*/
                        ulong/*Type[0]*/ nextKey = getKey(nextValue);
                        if (nextKey > key)
                        {
                            value = nextValue;
                            key = nextKey;
                        }
                    }
                }
                if (count != -1) return true;
            }
            return false;
        }

        /// <summary>
        /// 获取最大值记录
        /// </summary>
        /// <typeparam name="TValueType">值类型</typeparam>
        /// <param name="values">值集合</param>
        /// <param name="getKey">获取排序键的委托</param>
        /// <param name="value">默认空值</param>
        /// <returns>最大值,失败返回默认空值</returns>
        public static TValueType max<TValueType>
            (this System.Collections.Generic.IEnumerable<TValueType> values, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getKey
            , TValueType nullValue)
        {
            TValueType value;
            return max(values, getKey, out value) ? value : nullValue;
        }

        /// <summary>
        /// 获取最大值记录
        /// </summary>
        /// <typeparam name="TValueType">值类型</typeparam>
        /// <param name="values">值集合</param>
        /// <param name="getKey">获取排序键的委托</param>
        /// <param name="value">最大值</param>
        /// <returns>是否存在最大值</returns>
        public static bool maxKey<TValueType>
            (this System.Collections.Generic.IEnumerable<TValueType> values, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getKey
            , out /*Type[0]*/ulong/*Type[0]*/ value)
        {
            if (values != null)
            {
                if (getKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                int count = -1;
                value = /*Type[0]*/ulong/*Type[0]*/.MinValue;
                foreach (TValueType nextValue in values)
                {
                    if (++count == 0) value = getKey(nextValue);
                    else
                    {
                        /*Type[0]*/
                        ulong/*Type[0]*/ nextKey = getKey(nextValue);
                        if (nextKey > value) value = nextKey;
                    }
                }
                if (count != -1) return true;
            }
            value = /*Type[0]*/ulong/*Type[0]*/.MinValue;
            return false;
        }

        /// <summary>
        /// 获取最大值记录
        /// </summary>
        /// <typeparam name="TValueType">值类型</typeparam>
        /// <param name="values">值集合</param>
        /// <param name="getKey">获取排序键的委托</param>
        /// <param name="value">默认空值</param>
        /// <returns>最大值,失败返回默认空值</returns>
        public static /*Type[0]*/ulong/*Type[0]*/ maxKey<TValueType>
            (this System.Collections.Generic.IEnumerable<TValueType> values, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getKey
            , /*Type[0]*/ulong/*Type[0]*/ nullValue)
        {
            /*Type[0]*/
            ulong/*Type[0]*/ value;
            return maxKey(values, getKey, out value) ? value : nullValue;
        }

        /// <summary>
        /// 获取最小值记录
        /// </summary>
        /// <param name="values">值集合</param>
        /// <param name="value">最小值</param>
        /// <returns>是否存在最小值</returns>
        public static bool min
            (this System.Collections.Generic.IEnumerable</*Type[0]*/ulong/*Type[0]*/> values, out /*Type[0]*/ulong/*Type[0]*/ value)
        {
            if (values != null)
            {
                bool isValue = false;
                value = /*Type[0]*/ulong/*Type[0]*/.MinValue;
                foreach (/*Type[0]*/ulong/*Type[0]*/ nextValue in values)
                {
                    if (nextValue < value) value = nextValue;
                    isValue = true;
                }
                if (isValue) return true;
            }
            value = /*Type[0]*/ulong/*Type[0]*/.MinValue;
            return false;
        }

        /// <summary>
        /// 获取最小值记录
        /// </summary>
        /// <param name="values">值集合</param>
        /// <param name="value">默认空值</param>
        /// <returns>最小值,失败返回默认空值</returns>
        public static /*Type[0]*/ulong/*Type[0]*/ min
            (this System.Collections.Generic.IEnumerable</*Type[0]*/ulong/*Type[0]*/> values, /*Type[0]*/ulong/*Type[0]*/ nullValue)
        {
            /*Type[0]*/
            ulong/*Type[0]*/ value;
            return min(values, out value) ? value : nullValue;
        }

        /// <summary>
        /// 获取最小值记录
        /// </summary>
        /// <typeparam name="TValueType">值类型</typeparam>
        /// <param name="values">值集合</param>
        /// <param name="getKey">获取排序键的委托</param>
        /// <param name="value">最小值</param>
        /// <returns>是否存在最小值</returns>
        public static bool min<TValueType>
            (this System.Collections.Generic.IEnumerable<TValueType> values, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getKey
            , out TValueType value)
        {
            value = default(TValueType);
            if (values != null)
            {
                if (getKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                int count = -1;
                /*Type[0]*/
                ulong/*Type[0]*/ key = /*Type[0]*/ulong/*Type[0]*/.MinValue;
                foreach (TValueType nextValue in values)
                {
                    if (++count == 0) key = getKey(value = nextValue);
                    else
                    {
                        /*Type[0]*/
                        ulong/*Type[0]*/ nextKey = getKey(nextValue);
                        if (nextKey < key)
                        {
                            value = nextValue;
                            key = nextKey;
                        }
                    }
                }
                if (count != -1) return true;
            }
            return false;
        }

        /// <summary>
        /// 获取最小值记录
        /// </summary>
        /// <typeparam name="TValueType">值类型</typeparam>
        /// <param name="values">值集合</param>
        /// <param name="getKey">获取排序键的委托</param>
        /// <param name="value">默认空值</param>
        /// <returns>最小值,失败返回默认空值</returns>
        public static TValueType min<TValueType>
            (this System.Collections.Generic.IEnumerable<TValueType> values, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getKey
            , TValueType nullValue)
        {
            TValueType value;
            return min(values, getKey, out value) ? value : nullValue;
        }

        /// <summary>
        /// 获取最小值记录
        /// </summary>
        /// <typeparam name="TValueType">值类型</typeparam>
        /// <param name="values">值集合</param>
        /// <param name="getKey">获取排序键的委托</param>
        /// <param name="value">最小值</param>
        /// <returns>是否存在最小值</returns>
        public static bool minKey<TValueType>
            (this System.Collections.Generic.IEnumerable<TValueType> values, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getKey
            , out /*Type[0]*/ulong/*Type[0]*/ value)
        {
            if (values != null)
            {
                if (getKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                int count = -1;
                value = /*Type[0]*/ulong/*Type[0]*/.MinValue;
                foreach (TValueType nextValue in values)
                {
                    if (++count == 0) value = getKey(nextValue);
                    else
                    {
                        /*Type[0]*/
                        ulong/*Type[0]*/ nextKey = getKey(nextValue);
                        if (nextKey < value) value = nextKey;
                    }
                }
                if (count != -1) return true;
            }
            value = /*Type[0]*/ulong/*Type[0]*/.MinValue;
            return false;
        }

        /// <summary>
        /// 获取最小值记录
        /// </summary>
        /// <typeparam name="TValueType">值类型</typeparam>
        /// <param name="values">值集合</param>
        /// <param name="getKey">获取排序键的委托</param>
        /// <param name="value">默认空值</param>
        /// <returns>最小值,失败返回默认空值</returns>
        public static /*Type[0]*/ulong/*Type[0]*/ minKey<TValueType>
            (this System.Collections.Generic.IEnumerable<TValueType> values, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getKey
            , /*Type[0]*/ulong/*Type[0]*/ nullValue)
        {
            /*Type[0]*/
            ulong/*Type[0]*/ value;
            return minKey(values, getKey, out value) ? value : nullValue;
        }
    }
}