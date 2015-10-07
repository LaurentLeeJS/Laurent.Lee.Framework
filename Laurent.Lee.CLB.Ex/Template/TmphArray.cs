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

/*Type:ulong;long;uint;int;ushort;short;byte;sbyte;double;float;DateTime*/

namespace Laurent.Lee.CLB.Template
{
    /// <summary>
    /// 数组扩展操作
    /// </summary>
    public static partial class TmphArrayExtension
    {
        /// <summary>
        /// 逆转数组
        /// </summary>
        /// <param name="array">数组数据</param>
        /// <param name="index">起始位置</param>
        /// <param name="length">翻转数据数量</param>
        public unsafe static void Reverse(/*Type[0]*/ulong/*Type[0]*/[] array, int index, int length)
        {
            fixed (/*Type[0]*/ulong/*Type[0]*/* valueFixed = array)
            {
                for (/*Type[0]*/ulong/*Type[0]*/* start = valueFixed + index, end = start + length; start < --end; ++start)
                {
                    /*Type[0]*/
                    ulong/*Type[0]*/ value = *start;
                    *start = *end;
                    *end = value;
                }
            }
        }

        /// <summary>
        /// 逆转数组
        /// </summary>
        /// <param name="array">数组数据</param>
        /// <returns>翻转后的新数组</returns>
        public static /*Type[0]*/ulong/*Type[0]*/[] reverse(this /*Type[0]*/ulong/*Type[0]*/[] array)
        {
            if (array == null || array.Length == 0) return TmphNullValue</*Type[0]*/ulong/*Type[0]*/>.Array;
            Reverse(array, 0, array.Length);
            return array;
        }

        /// <summary>
        /// 逆转数组
        /// </summary>
        /// <param name="array">数组数据</param>
        /// <param name="index">起始位置</param>
        /// <param name="length">翻转数据数量</param>
        /// <returns>翻转后的新数组</returns>
        public unsafe static /*Type[0]*/ulong/*Type[0]*/[] GetReverse(/*Type[0]*/ulong/*Type[0]*/[] array, int index, int length)
        {
            /*Type[0]*/
            ulong/*Type[0]*/[] newValues = new /*Type[0]*/ulong/*Type[0]*/[length];
            fixed (/*Type[0]*/ulong/*Type[0]*/* valueFixed = array, newValueFixed = newValues)
            {
                for (/*Type[0]*/ulong/*Type[0]*/* start = valueFixed + index, end = start + length, wirte = newValueFixed + length;
                    start != end;
                    *--wirte = *start++)
                    ;
            }
            return newValues;
        }

        /// <summary>
        /// 逆转数组
        /// </summary>
        /// <param name="array">数组数据</param>
        /// <returns>翻转后的新数组</returns>
        public static /*Type[0]*/ulong/*Type[0]*/[] getReverse(this /*Type[0]*/ulong/*Type[0]*/[] array)
        {
            if (array == null || array.Length == 0) return TmphNullValue</*Type[0]*/ulong/*Type[0]*/>.Array;
            return GetReverse(array, 0, array.Length);
        }

        /// <summary>
        /// 获取匹配数据位置
        /// </summary>
        /// <param name="values">数据指针</param>
        /// <param name="length">匹配数据数量</param>
        /// <param name="value">匹配数据</param>
        /// <returns>匹配位置,失败为null</returns>
        public unsafe static /*Type[0]*/ulong/*Type[0]*/* IndexOf
            (/*Type[0]*/ulong/*Type[0]*/* valueFixed, int length, /*Type[0]*/ulong/*Type[0]*/ value)
        {
            for (/*Type[0]*/ulong/*Type[0]*/* start = valueFixed, end = valueFixed + length; start != end; ++start)
            {
                if (*start == value) return start;
            }
            return null;
        }

        /// <summary>
        /// 获取匹配数据位置
        /// </summary>
        /// <param name="array">数组数据</param>
        /// <param name="value">匹配数据</param>
        /// <returns>匹配位置,失败为-1</returns>
        public unsafe static int indexOf(this /*Type[0]*/ulong/*Type[0]*/[] array, /*Type[0]*/ulong/*Type[0]*/ value)
        {
            if (array != null)
            {
                fixed (/*Type[0]*/ulong/*Type[0]*/* valueFixed = array)
                {
                    /*Type[0]*/
                    ulong/*Type[0]*/* valueIndex = IndexOf(valueFixed, array.Length, value);
                    if (valueIndex != null) return (int)(valueIndex - valueFixed);
                }
            }
            return -1;
        }

        /// <summary>
        /// 获取匹配数据位置
        /// </summary>
        /// <param name="values">数组数据</param>
        /// <param name="length">匹配数据数量</param>
        /// <param name="value">匹配数据</param>
        /// <returns>匹配位置,失败为null</returns>
        public unsafe static /*Type[0]*/ulong/*Type[0]*/* IndexOf
            (/*Type[0]*/ulong/*Type[0]*/* valueFixed, int length, Func</*Type[0]*/ulong/*Type[0]*/, bool> isValue)
        {
            for (/*Type[0]*/ulong/*Type[0]*/* start = valueFixed, end = valueFixed + length; start != end; ++start)
            {
                if (isValue(*start)) return start;
            }
            return null;
        }

        /// <summary>
        /// 获取匹配数据位置
        /// </summary>
        /// <param name="array">数据数组</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>匹配位置,失败为-1</returns>
        public unsafe static int indexOf(this /*Type[0]*/ulong/*Type[0]*/[] array, Func</*Type[0]*/ulong/*Type[0]*/, bool> isValue)
        {
            if (array != null)
            {
                if (isValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                fixed (/*Type[0]*/ulong/*Type[0]*/* valueFixed = array)
                {
                    /*Type[0]*/
                    ulong/*Type[0]*/* valueIndex = IndexOf(valueFixed, array.Length, isValue);
                    if (valueIndex != null) return (int)(valueIndex - valueFixed);
                }
            }
            return -1;
        }

        /// <summary>
        /// 获取第一个匹配值
        /// </summary>
        /// <param name="array">数组数据</param>
        /// <param name="isValue">数据匹配器</param>
        /// <param name="index">起始位置</param>
        /// <returns>第一个匹配值,失败为default(/*Type[0]*/ulong/*Type[0]*/)</returns>
        public unsafe static /*Type[0]*/ulong/*Type[0]*/ firstOrDefault
            (this /*Type[0]*/ulong/*Type[0]*/[] array, Func</*Type[0]*/ulong/*Type[0]*/, bool> isValue, int index)
        {
            if (array != null && (uint)index < (uint)array.Length)
            {
                if (isValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                fixed (/*Type[0]*/ulong/*Type[0]*/* valueFixed = array)
                {
                    /*Type[0]*/
                    ulong/*Type[0]*/* valueIndex = IndexOf(valueFixed + index, array.Length - index, isValue);
                    if (valueIndex != null) return *valueIndex;
                }
            }
            return default(/*Type[0]*/ulong/*Type[0]*/);
        }

        /// <summary>
        /// 获取匹配数量
        /// </summary>
        /// <param name="array">数组数据</param>
        /// <param name="index">起始位置</param>
        /// <param name="count">匹配数据数量</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>匹配数量</returns>
        public unsafe static int count
            (this /*Type[0]*/ulong/*Type[0]*/[] array, Func</*Type[0]*/ulong/*Type[0]*/, bool> isValue)
        {
            if (array != null)
            {
                if (isValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                int value = 0;
                fixed (/*Type[0]*/ulong/*Type[0]*/* valueFixed = array)
                {
                    for (/*Type[0]*/ulong/*Type[0]*/* end = valueFixed + array.Length; end != valueFixed;)
                    {
                        if (isValue(*--end)) ++value;
                    }
                }
                return value;
            }
            return 0;
        }

        /// <summary>
        /// 替换数据
        /// </summary>
        /// <param name="array">数组数据</param>
        /// <param name="value">新值</param>
        /// <param name="isValue">数据匹配器</param>
        public unsafe static /*Type[0]*/ulong/*Type[0]*/[] replaceFirst
            (this /*Type[0]*/ulong/*Type[0]*/[] array, /*Type[0]*/ulong/*Type[0]*/ value, Func</*Type[0]*/ulong/*Type[0]*/, bool> isValue)
        {
            if (array != null)
            {
                if (isValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                fixed (/*Type[0]*/ulong/*Type[0]*/* valueFixed = array)
                {
                    /*Type[0]*/
                    ulong/*Type[0]*/* valueIndex = IndexOf(valueFixed, array.Length, isValue);
                    if (valueIndex != null) *valueIndex = value;
                }
                return array;
            }
            return TmphNullValue</*Type[0]*/ulong/*Type[0]*/>.Array;
        }

        /// <summary>
        /// 数据转换
        /// </summary>
        /// <typeparam name="TValueType">数组类型</typeparam>
        /// <param name="array">数据数组</param>
        /// <param name="getValue">数据获取器</param>
        /// <returns>目标数组</returns>
        public unsafe static /*Type[0]*/ulong/*Type[0]*/[] getArray<TValueType>
            (this TValueType[] array, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getValue)
        {
            if (array.length() != 0)
            {
                if (getValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                /*Type[0]*/
                ulong/*Type[0]*/[] newValues = new /*Type[0]*/ulong/*Type[0]*/[array.Length];
                fixed (/*Type[0]*/ulong/*Type[0]*/* newValueFixed = newValues)
                {
                    /*Type[0]*/
                    ulong/*Type[0]*/* writeValue = newValueFixed;
                    foreach (TValueType value in array) *writeValue++ = getValue(value);
                }
                return newValues;
            }
            return TmphNullValue</*Type[0]*/ulong/*Type[0]*/>.Array;
        }

        /// <summary>
        /// 获取匹配集合
        /// </summary>
        /// <param name="array">数组数据</param>
        /// <param name="index">起始位置</param>
        /// <param name="length">翻转数据数量</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>匹配集合</returns>
        public unsafe static TmphSubArray</*Type[0]*/ulong/*Type[0]*/> GetFind
            (this /*Type[0]*/ulong/*Type[0]*/[] array, int index, int length, Func</*Type[0]*/ulong/*Type[0]*/, bool> isValue)
        {
            /*Type[0]*/
            ulong/*Type[0]*/[] newValues = new /*Type[0]*/ulong/*Type[0]*/[length < sizeof(int) ? sizeof(int) : length];
            fixed (/*Type[0]*/ulong/*Type[0]*/* newValueFixed = newValues, valueFixed = array)
            {
                /*Type[0]*/
                ulong/*Type[0]*/* write = newValueFixed;
                for (/*Type[0]*/ulong/*Type[0]*/* start = valueFixed + index, end = valueFixed + length; start != end; ++start)
                {
                    if (isValue(*start)) *write++ = *start;
                }
                return TmphSubArray</*Type[0]*/ulong/*Type[0]*/>.Unsafe(newValues, 0, (int)(write - newValueFixed));
            }
        }

        /// <summary>
        /// 获取匹配集合
        /// </summary>
        /// <param name="array">数组数据</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>匹配集合</returns>
        public unsafe static TmphSubArray</*Type[0]*/ulong/*Type[0]*/> getFind
            (this /*Type[0]*/ulong/*Type[0]*/[] array, Func</*Type[0]*/ulong/*Type[0]*/, bool> isValue)
        {
            if (array.length() != 0)
            {
                if (isValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                return GetFind(array, 0, array.Length, isValue);
            }
            return default(TmphSubArray</*Type[0]*/ulong/*Type[0]*/>);
        }

        /// <summary>
        /// 获取匹配数组
        /// </summary>
        /// <param name="array">数组数据</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>匹配数组</returns>
        public unsafe static /*Type[0]*/ulong/*Type[0]*/[] getFindArray
            (this /*Type[0]*/ulong/*Type[0]*/[] array, Func</*Type[0]*/ulong/*Type[0]*/, bool> isValue)
        {
            if (isValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            int length = array.length();
            if (length != 0)
            {
                TmphMemoryPool pool = Laurent.Lee.CLB.TmphMemoryPool.GetDefaultPool(length = ((length + 31) >> 5) << 2);
                byte[] data = pool.Get(length);
                try
                {
                    fixed (byte* dataFixed = data)
                    {
                        Array.Clear(data, 0, length);
                        return GetFindArray(array, 0, array.Length, isValue, new TmphFixedMap(dataFixed));
                    }
                }
                finally { pool.Push(ref data); }
            }
            return TmphNullValue</*Type[0]*/ulong/*Type[0]*/>.Array;
        }

        /// <summary>
        /// 获取匹配数组
        /// </summary>
        /// <param name="array">数组数据</param>
        /// <param name="index">起始位置</param>
        /// <param name="count">匹配数据数量</param>
        /// <param name="isValue">数据匹配器</param>
        /// <param name="map">匹配结果位图</param>
        /// <returns>匹配数组</returns>
        public unsafe static /*Type[0]*/ulong/*Type[0]*/[] GetFindArray
            (/*Type[0]*/ulong/*Type[0]*/[] array, int index, int count, Func</*Type[0]*/ulong/*Type[0]*/, bool> isValue, TmphFixedMap map)
        {
            int length = 0, mapIndex = 0;
            fixed (/*Type[0]*/ulong/*Type[0]*/* valueFixed = array)
            {
                /*Type[0]*/
                ulong/*Type[0]*/* startFixed = valueFixed + index, end = startFixed + count;
                for (/*Type[0]*/ulong/*Type[0]*/* start = startFixed; start != end; ++start, ++mapIndex)
                {
                    if (isValue(*start))
                    {
                        ++length;
                        map.Set(mapIndex);
                    }
                }
                if (length != 0)
                {
                    /*Type[0]*/
                    ulong/*Type[0]*/[] newValues = new /*Type[0]*/ulong/*Type[0]*/[length];
                    fixed (/*Type[0]*/ulong/*Type[0]*/* newValueFixed = newValues)
                    {
                        /*Type[0]*/
                        ulong/*Type[0]*/* write = newValueFixed + length;
                        while (mapIndex != 0)
                        {
                            if (map.Get(--mapIndex)) *--write = startFixed[mapIndex];
                        }
                    }
                    return newValues;
                }
            }
            return TmphNullValue</*Type[0]*/ulong/*Type[0]*/>.Array;
        }

        /// <summary>
        /// 获取最大值
        /// </summary>
        /// <param name="array">数据数组</param>
        /// <param name="value">最大值</param>
        /// <returns>是否存在最大值</returns>
        public unsafe static bool max(this /*Type[0]*/ulong/*Type[0]*/[] array, out /*Type[0]*/ulong/*Type[0]*/ value)
        {
            if (array.length() != 0)
            {
                fixed (/*Type[0]*/ulong/*Type[0]*/* valueFixed = array)
                {
                    value = *valueFixed;
                    for (/*Type[0]*/ulong/*Type[0]*/* start = valueFixed + 1, end = valueFixed + array.Length; start != end; ++start)
                    {
                        if (*start > value) value = *start;
                    }
                    return true;
                }
            }
            value = /*Type[0]*/ulong/*Type[0]*/.MinValue;
            return false;
        }

        /// <summary>
        /// 获取最大值
        /// </summary>
        /// <param name="array">数据数组</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最大值,失败返回默认空值</returns>
        public unsafe static /*Type[0]*/ulong/*Type[0]*/ max(this /*Type[0]*/ulong/*Type[0]*/[] array, /*Type[0]*/ulong/*Type[0]*/ nullValue)
        {
            /*Type[0]*/
            ulong/*Type[0]*/ value;
            return max(array, out value) ? value : nullValue;
        }

        /// <summary>
        /// 获取最大值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数据数组</param>
        /// <param name="getKey">数据获取器</param>
        /// <param name="value">最大值</param>
        /// <returns>是否存在最大值</returns>
        public static bool maxKey<TValueType>
            (this TValueType[] array, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getKey, out /*Type[0]*/ulong/*Type[0]*/ value)
        {
            if (array.length() != 0)
            {
                value = getKey(array[0]);
                foreach (TValueType nextValue in array)
                {
                    /*Type[0]*/
                    ulong/*Type[0]*/ nextKey = getKey(nextValue);
                    if (nextKey > value) value = nextKey;
                }
                return true;
            }
            value = /*Type[0]*/ulong/*Type[0]*/.MinValue;
            return false;
        }

        /// <summary>
        /// 获取最大值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数据数组</param>
        /// <param name="getKey">数据获取器</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最大值,失败返回默认空值</returns>
        public static /*Type[0]*/ulong/*Type[0]*/ maxKey<TValueType>
            (this TValueType[] array, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getKey, /*Type[0]*/ulong/*Type[0]*/ nullValue)
        {
            /*Type[0]*/
            ulong/*Type[0]*/ value;
            return maxKey(array, getKey, out value) ? value : nullValue;
        }

        /// <summary>
        /// 获取最大值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数据数组</param>
        /// <param name="getKey">数据获取器</param>
        /// <param name="value">最大值</param>
        /// <returns>是否存在最大值</returns>
        public static bool max<TValueType>
            (this TValueType[] array, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getKey, out TValueType value)
        {
            if (array.length() != 0)
            {
                /*Type[0]*/
                ulong/*Type[0]*/ maxKey = getKey(value = array[0]);
                foreach (TValueType nextValue in array)
                {
                    /*Type[0]*/
                    ulong/*Type[0]*/ nextKey = getKey(nextValue);
                    if (nextKey > maxKey)
                    {
                        maxKey = nextKey;
                        value = nextValue;
                    }
                }
                return true;
            }
            value = default(TValueType);
            return false;
        }

        /// <summary>
        /// 获取最大值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数据数组</param>
        /// <param name="getKey">数据获取器</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最大值,失败返回默认空值</returns>
        public static TValueType max<TValueType>(this TValueType[] array, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getKey, TValueType nullValue)
        {
            TValueType value;
            return max(array, getKey, out value) ? value : nullValue;
        }

        /// <summary>
        /// 获取最小值
        /// </summary>
        /// <param name="array">数据数组</param>
        /// <param name="value">最小值</param>
        /// <returns>是否存在最小值</returns>
        public unsafe static bool min(this /*Type[0]*/ulong/*Type[0]*/[] array, out /*Type[0]*/ulong/*Type[0]*/ value)
        {
            if (array.length() != 0)
            {
                fixed (/*Type[0]*/ulong/*Type[0]*/* valueFixed = array)
                {
                    value = *valueFixed;
                    for (/*Type[0]*/ulong/*Type[0]*/* start = valueFixed + 1, end = valueFixed + array.Length; start != end; ++start)
                    {
                        if (*start < value) value = *start;
                    }
                    return true;
                }
            }
            value = /*Type[0]*/ulong/*Type[0]*/.MaxValue;
            return false;
        }

        /// <summary>
        /// 获取最小值
        /// </summary>
        /// <param name="array">数据数组</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最小值,失败返回默认空值</returns>
        public unsafe static /*Type[0]*/ulong/*Type[0]*/ min(this /*Type[0]*/ulong/*Type[0]*/[] array, /*Type[0]*/ulong/*Type[0]*/ nullValue)
        {
            /*Type[0]*/
            ulong/*Type[0]*/ value;
            return min(array, out value) ? value : nullValue;
        }

        /// <summary>
        /// 获取最小值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数据数组</param>
        /// <param name="getKey">数据获取器</param>
        /// <param name="value">最小值</param>
        /// <returns>是否存在最小值</returns>
        public static bool minKey<TValueType>
            (this TValueType[] array, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getKey, out /*Type[0]*/ulong/*Type[0]*/ value)
        {
            if (array.length() != 0)
            {
                value = getKey(array[0]);
                foreach (TValueType nextValue in array)
                {
                    /*Type[0]*/
                    ulong/*Type[0]*/ nextKey = getKey(nextValue);
                    if (nextKey < value) value = nextKey;
                }
                return true;
            }
            value = /*Type[0]*/ulong/*Type[0]*/.MaxValue;
            return false;
        }

        /// <summary>
        /// 获取最小值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数据数组</param>
        /// <param name="getKey">数据获取器</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最小值,失败返回默认空值</returns>
        public static /*Type[0]*/ulong/*Type[0]*/ minKey<TValueType>
            (this TValueType[] array, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getKey, /*Type[0]*/ulong/*Type[0]*/ nullValue)
        {
            /*Type[0]*/
            ulong/*Type[0]*/ value;
            return minKey(array, getKey, out value) ? value : nullValue;
        }

        /// <summary>
        /// 获取最小值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数据数组</param>
        /// <param name="getKey">数据获取器</param>
        /// <param name="value">最小值</param>
        /// <returns>是否存在最小值</returns>
        public static bool min<TValueType>(this TValueType[] array, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getKey, out TValueType value)
        {
            if (array.length() != 0)
            {
                value = array[0];
                /*Type[0]*/
                ulong/*Type[0]*/ minKey = getKey(value);
                foreach (TValueType nextValue in array)
                {
                    /*Type[0]*/
                    ulong/*Type[0]*/ nextKey = getKey(nextValue);
                    if (nextKey < minKey)
                    {
                        minKey = nextKey;
                        value = nextValue;
                    }
                }
                return true;
            }
            value = default(TValueType);
            return false;
        }

        /// <summary>
        /// 获取最小值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数据数组</param>
        /// <param name="getKey">数据获取器</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最小值,失败返回默认空值</returns>
        public static TValueType min<TValueType>(this TValueType[] array, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getKey, TValueType nullValue)
        {
            TValueType value;
            return min(array, getKey, out value) ? value : nullValue;
        }
    }

    /// <summary>
    /// 数组子串扩展
    /// </summary>
    public static partial class TmphSubArrayExtension
    {
        /// <summary>
        /// 逆转数组
        /// </summary>
        /// <param name="array">数组数据</param>
        /// <returns>翻转后的新数组</returns>
        public static /*Type[0]*/ulong/*Type[0]*/[] getReverse(this TmphSubArray</*Type[0]*/ulong/*Type[0]*/> array)
        {
            if (array.Count == 0) return TmphNullValue</*Type[0]*/ulong/*Type[0]*/>.Array;
            return TmphArrayExtension.GetReverse(array.Array, array.StartIndex, array.Count);
        }

        /// <summary>
        /// 逆转数组
        /// </summary>
        /// <param name="array">数组数据</param>
        /// <returns>翻转后的新数组</returns>
        public static TmphSubArray</*Type[0]*/ulong/*Type[0]*/> reverse(this TmphSubArray</*Type[0]*/ulong/*Type[0]*/> array)
        {
            if (array.Count > 1) TmphArrayExtension.Reverse(array.Array, array.StartIndex, array.Count);
            return array;
        }

        /// <summary>
        /// 获取匹配数据位置
        /// </summary>
        /// <param name="array">数组数据</param>
        /// <param name="value">匹配数据</param>
        /// <returns>匹配位置,失败为-1</returns>
        public unsafe static int indexOf(this TmphSubArray</*Type[0]*/ulong/*Type[0]*/> array, /*Type[0]*/ulong/*Type[0]*/ value)
        {
            if (array.Count != 0)
            {
                fixed (/*Type[0]*/ulong/*Type[0]*/* valueFixed = array.Array)
                {
                    /*Type[0]*/
                    ulong/*Type[0]*/* start = valueFixed + array.StartIndex, index = TmphArrayExtension.IndexOf(start, array.Count, value);
                    if (index != null) return (int)(index - start);
                }
            }
            return -1;
        }

        /// <summary>
        /// 获取匹配数据位置
        /// </summary>
        /// <param name="array">数据数组</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>匹配位置,失败为-1</returns>
        public unsafe static int indexOf(this TmphSubArray</*Type[0]*/ulong/*Type[0]*/> array, Func</*Type[0]*/ulong/*Type[0]*/, bool> isValue)
        {
            if (array.Count != 0)
            {
                if (isValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                fixed (/*Type[0]*/ulong/*Type[0]*/* valueFixed = array.Array)
                {
                    /*Type[0]*/
                    ulong/*Type[0]*/* start = valueFixed + array.StartIndex, index = TmphArrayExtension.IndexOf(start, array.Count, isValue);
                    if (index != null) return (int)(index - valueFixed);
                }
            }
            return -1;
        }

        /// <summary>
        /// 获取第一个匹配值
        /// </summary>
        /// <param name="array">数组数据</param>
        /// <param name="isValue">数据匹配器</param>
        /// <param name="index">起始位置</param>
        /// <returns>第一个匹配值,失败为default(/*Type[0]*/ulong/*Type[0]*/)</returns>
        public unsafe static /*Type[0]*/ulong/*Type[0]*/ firstOrDefault
            (this TmphSubArray</*Type[0]*/ulong/*Type[0]*/> array, Func</*Type[0]*/ulong/*Type[0]*/, bool> isValue, int index)
        {
            if ((uint)index < (uint)array.Count)
            {
                if (isValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                fixed (/*Type[0]*/ulong/*Type[0]*/* valueFixed = array.Array)
                {
                    /*Type[0]*/
                    ulong/*Type[0]*/* valueIndex = TmphArrayExtension.IndexOf(valueFixed + array.StartIndex + index, array.Count - index, isValue);
                    if (valueIndex != null) return *valueIndex;
                }
            }
            return default(/*Type[0]*/ulong/*Type[0]*/);
        }

        /// <summary>
        /// 获取匹配数量
        /// </summary>
        /// <param name="array">数组数据</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>匹配数量</returns>
        public unsafe static int count(this TmphSubArray</*Type[0]*/ulong/*Type[0]*/> array, Func</*Type[0]*/ulong/*Type[0]*/, bool> isValue)
        {
            if (array.Count != 0)
            {
                if (isValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                int value = 0;
                fixed (/*Type[0]*/ulong/*Type[0]*/* valueFixed = array.Array)
                {
                    /*Type[0]*/
                    ulong/*Type[0]*/* start = valueFixed + array.StartIndex, end = start + array.Count;
                    do
                    {
                        if (isValue(*start)) ++value;
                    }
                    while (++start != end);
                }
                return value;
            }
            return 0;
        }

        /// <summary>
        /// 替换数据
        /// </summary>
        /// <param name="array">数组数据</param>
        /// <param name="value">新值</param>
        /// <param name="isValue">数据匹配器</param>
        public unsafe static TmphSubArray</*Type[0]*/ulong/*Type[0]*/> replaceFirst
            (this TmphSubArray</*Type[0]*/ulong/*Type[0]*/> array, /*Type[0]*/ulong/*Type[0]*/ value, Func</*Type[0]*/ulong/*Type[0]*/, bool> isValue)
        {
            if (array.Count != 0)
            {
                if (isValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                fixed (/*Type[0]*/ulong/*Type[0]*/* valueFixed = array.Array)
                {
                    /*Type[0]*/
                    ulong/*Type[0]*/* valueIndex = TmphArrayExtension.IndexOf(valueFixed + array.StartIndex, array.Count, isValue);
                    if (valueIndex != null) *valueIndex = value;
                }
            }
            return array;
        }

        /// <summary>
        /// 获取匹配集合
        /// </summary>
        /// <param name="array">数组数据</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>匹配集合</returns>
        public unsafe static TmphSubArray</*Type[0]*/ulong/*Type[0]*/> find
            (this TmphSubArray</*Type[0]*/ulong/*Type[0]*/> array, Func</*Type[0]*/ulong/*Type[0]*/, bool> isValue)
        {
            if (array.Count != 0)
            {
                if (isValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                fixed (/*Type[0]*/ulong/*Type[0]*/* valueFixed = array.Array)
                {
                    /*Type[0]*/
                    ulong/*Type[0]*/* write = valueFixed + array.StartIndex, start = write, end = write + array.Count;
                    do
                    {
                        if (isValue(*start)) *write++ = *start;
                    }
                    while (++start != end);
                    return TmphSubArray</*Type[0]*/ulong/*Type[0]*/>.Unsafe(array.Array, array.StartIndex, (int)(write - valueFixed) - array.StartIndex);
                }
            }
            return array;
        }

        /// <summary>
        /// 获取匹配集合
        /// </summary>
        /// <param name="array">数组数据</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>匹配集合</returns>
        public unsafe static TmphSubArray</*Type[0]*/ulong/*Type[0]*/> getFind
            (this TmphSubArray</*Type[0]*/ulong/*Type[0]*/> array, Func</*Type[0]*/ulong/*Type[0]*/, bool> isValue)
        {
            if (array.Count != 0)
            {
                if (isValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                return TmphArrayExtension.GetFind(array.Array, array.StartIndex, array.Count, isValue);
            }
            return default(TmphSubArray</*Type[0]*/ulong/*Type[0]*/>);
        }

        /// <summary>
        /// 获取匹配数组
        /// </summary>
        /// <param name="array">数组数据</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>匹配数组</returns>
        public unsafe static /*Type[0]*/ulong/*Type[0]*/[] getFindArray
            (this TmphSubArray</*Type[0]*/ulong/*Type[0]*/> array, Func</*Type[0]*/ulong/*Type[0]*/, bool> isValue)
        {
            if (array.Count != 0)
            {
                if (isValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                int length = ((array.Count + 31) >> 5) << 2;
                TmphMemoryPool pool = Laurent.Lee.CLB.TmphMemoryPool.GetDefaultPool(length);
                byte[] data = pool.Get(length);
                try
                {
                    fixed (byte* dataFixed = data)
                    {
                        Array.Clear(data, 0, length);
                        return TmphArrayExtension.GetFindArray(array.Array, array.StartIndex, array.Count, isValue, new TmphFixedMap(dataFixed));
                    }
                }
                finally { pool.Push(ref data); }
            }
            return TmphNullValue</*Type[0]*/ulong/*Type[0]*/>.Array;
        }

        /// <summary>
        /// 数组类型转换
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数据数组</param>
        /// <param name="getValue">数据获取器</param>
        /// <returns>目标数组</returns>
        public unsafe static /*Type[0]*/ulong/*Type[0]*/[] getArray<TValueType>
            (this TmphSubArray<TValueType> TmphSubArray, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getValue)
        {
            if (TmphSubArray.Count != 0)
            {
                if (getValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                TValueType[] array = TmphSubArray.Array;
                /*Type[0]*/
                ulong/*Type[0]*/[] newValues = new /*Type[0]*/ulong/*Type[0]*/[TmphSubArray.Count];
                fixed (/*Type[0]*/ulong/*Type[0]*/* newValueFixed = newValues)
                {
                    /*Type[0]*/
                    ulong/*Type[0]*/* write = newValueFixed;
                    int index = TmphSubArray.StartIndex, endIndex = index + TmphSubArray.Count;
                    do
                    {
                        *write++ = getValue(array[index++]);
                    }
                    while (index != endIndex);
                }
                return newValues;
            }
            return TmphNullValue</*Type[0]*/ulong/*Type[0]*/>.Array;
        }

        /// <summary>
        /// 数组类型转换
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数据数组</param>
        /// <param name="getValue">数据获取器</param>
        /// <returns>目标数组</returns>
        public unsafe static TValueType[] getArray<TValueType>
            (this TmphSubArray</*Type[0]*/ulong/*Type[0]*/> array, Func</*Type[0]*/ulong/*Type[0]*/, TValueType> getValue)
        {
            if (array.Count != 0)
            {
                if (getValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                TValueType[] newValues = new TValueType[array.Count];
                fixed (/*Type[0]*/ulong/*Type[0]*/* arrayFixed = array.Array)
                {
                    /*Type[0]*/
                    ulong/*Type[0]*/* start = arrayFixed + array.StartIndex, end = start + array.Count;
                    int index = 0;
                    do
                    {
                        newValues[index++] = getValue(*start);
                    }
                    while (++start != end);
                }
                return newValues;
            }
            return TmphNullValue<TValueType>.Array;
        }

        /// <summary>
        /// 遍历foreach
        /// </summary>
        /// <param name="array">数据数组</param>
        /// <param name="method">调用函数</param>
        /// <returns>数据数组</returns>
        public unsafe static TmphSubArray</*Type[0]*/ulong/*Type[0]*/> each
            (this TmphSubArray</*Type[0]*/ulong/*Type[0]*/> array, Action</*Type[0]*/ulong/*Type[0]*/> method)
        {
            if (array.Count != 0)
            {
                if (method == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                fixed (/*Type[0]*/ulong/*Type[0]*/* valueFixed = array.Array)
                {
                    for (/*Type[0]*/ulong/*Type[0]*/* start = valueFixed + array.StartIndex, end = start + array.Count; start != end; method(*start++)) ;
                }
            }
            return array;
        }

        /// <summary>
        /// 获取最大值
        /// </summary>
        /// <param name="array">数据数组</param>
        /// <param name="value">最大值</param>
        /// <returns>是否存在最大值</returns>
        public unsafe static bool max(this TmphSubArray</*Type[0]*/ulong/*Type[0]*/> array, out /*Type[0]*/ulong/*Type[0]*/ value)
        {
            if (array.Count != 0)
            {
                fixed (/*Type[0]*/ulong/*Type[0]*/* valueFixed = array.Array)
                {
                    /*Type[0]*/
                    ulong/*Type[0]*/* start = valueFixed + array.StartIndex, end = start + array.Count;
                    for (value = *start; ++start != end;)
                    {
                        if (*start > value) value = *start;
                    }
                    return true;
                }
            }
            value = /*Type[0]*/ulong/*Type[0]*/.MinValue;
            return false;
        }

        /// <summary>
        /// 获取最大值
        /// </summary>
        /// <param name="array">数据数组</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最大值,失败返回默认空值</returns>
        public unsafe static /*Type[0]*/ulong/*Type[0]*/ max(this TmphSubArray</*Type[0]*/ulong/*Type[0]*/> array, /*Type[0]*/ulong/*Type[0]*/ nullValue)
        {
            /*Type[0]*/
            ulong/*Type[0]*/ value;
            return max(array, out value) ? value : nullValue;
        }

        /// <summary>
        /// 获取最大值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="TmphSubArray">数据数组</param>
        /// <param name="getKey">数据获取器</param>
        /// <param name="value">最大值</param>
        /// <returns>是否存在最大值</returns>
        public static bool maxKey<TValueType>
            (this TmphSubArray<TValueType> TmphSubArray, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getKey, out /*Type[0]*/ulong/*Type[0]*/ value)
        {
            if (TmphSubArray.Count != 0)
            {
                TValueType[] array = TmphSubArray.Array;
                int index = TmphSubArray.StartIndex, endIndex = index + TmphSubArray.Count;
                value = getKey(array[index]);
                while (++index != endIndex)
                {
                    /*Type[0]*/
                    ulong/*Type[0]*/ nextKey = getKey(array[index]);
                    if (nextKey > value) value = nextKey;
                }
                return true;
            }
            value = /*Type[0]*/ulong/*Type[0]*/.MinValue;
            return false;
        }

        /// <summary>
        /// 获取最大值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数据数组</param>
        /// <param name="getKey">数据获取器</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最大值,失败返回默认空值</returns>
        public static /*Type[0]*/ulong/*Type[0]*/ maxKey<TValueType>
            (this TmphSubArray<TValueType> array, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getKey, /*Type[0]*/ulong/*Type[0]*/ nullValue)
        {
            /*Type[0]*/
            ulong/*Type[0]*/ value;
            return maxKey(array, getKey, out value) ? value : nullValue;
        }

        /// <summary>
        /// 获取最大值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="TmphSubArray">数据数组</param>
        /// <param name="getKey">数据获取器</param>
        /// <param name="value">最大值</param>
        /// <returns>是否存在最大值</returns>
        public static bool max<TValueType>
            (this TmphSubArray<TValueType> TmphSubArray, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getKey, out TValueType value)
        {
            if (TmphSubArray.Count != 0)
            {
                TValueType[] array = TmphSubArray.Array;
                int index = TmphSubArray.StartIndex, endIndex = index + TmphSubArray.Count;
                /*Type[0]*/
                ulong/*Type[0]*/ maxKey = getKey(value = array[index]);
                while (++index != endIndex)
                {
                    TValueType nextValue = array[index];
                    /*Type[0]*/
                    ulong/*Type[0]*/ nextKey = getKey(nextValue);
                    if (nextKey > maxKey)
                    {
                        maxKey = nextKey;
                        value = nextValue;
                    }
                }
                return true;
            }
            value = default(TValueType);
            return false;
        }

        /// <summary>
        /// 获取最大值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数据数组</param>
        /// <param name="getKey">数据获取器</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最大值,失败返回默认空值</returns>
        public static TValueType max<TValueType>(this TmphSubArray<TValueType> array, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getKey, TValueType nullValue)
        {
            TValueType value;
            return max(array, getKey, out value) ? value : nullValue;
        }

        /// <summary>
        /// 获取最小值
        /// </summary>
        /// <param name="array">数据数组</param>
        /// <param name="value">最小值</param>
        /// <returns>是否存在最小值</returns>
        public unsafe static bool min(this TmphSubArray</*Type[0]*/ulong/*Type[0]*/> array, out /*Type[0]*/ulong/*Type[0]*/ value)
        {
            if (array.Count != 0)
            {
                fixed (/*Type[0]*/ulong/*Type[0]*/* valueFixed = array.Array)
                {
                    /*Type[0]*/
                    ulong/*Type[0]*/* start = valueFixed + array.StartIndex, end = start + array.Count;
                    for (value = *start; ++start != end;)
                    {
                        if (*start < value) value = *start;
                    }
                    return true;
                }
            }
            value = /*Type[0]*/ulong/*Type[0]*/.MinValue;
            return false;
        }

        /// <summary>
        /// 获取最小值
        /// </summary>
        /// <param name="array">数据数组</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最小值,失败返回默认空值</returns>
        public unsafe static /*Type[0]*/ulong/*Type[0]*/ min(this TmphSubArray</*Type[0]*/ulong/*Type[0]*/> array, /*Type[0]*/ulong/*Type[0]*/ nullValue)
        {
            /*Type[0]*/
            ulong/*Type[0]*/ value;
            return min(array, out value) ? value : nullValue;
        }

        /// <summary>
        /// 获取最小值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="TmphSubArray">数据数组</param>
        /// <param name="getKey">数据获取器</param>
        /// <param name="value">最小值</param>
        /// <returns>是否存在最小值</returns>
        public static bool minKey<TValueType>
            (this TmphSubArray<TValueType> TmphSubArray, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getKey, out /*Type[0]*/ulong/*Type[0]*/ value)
        {
            if (TmphSubArray.Count != 0)
            {
                TValueType[] array = TmphSubArray.Array;
                int index = TmphSubArray.StartIndex, endIndex = index + TmphSubArray.Count;
                value = getKey(array[index]);
                while (++index != endIndex)
                {
                    /*Type[0]*/
                    ulong/*Type[0]*/ nextKey = getKey(array[index]);
                    if (nextKey < value) value = nextKey;
                }
                return true;
            }
            value = /*Type[0]*/ulong/*Type[0]*/.MinValue;
            return false;
        }

        /// <summary>
        /// 获取最小值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数据数组</param>
        /// <param name="getKey">数据获取器</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最小值,失败返回默认空值</returns>
        public static /*Type[0]*/ulong/*Type[0]*/ minKey<TValueType>
            (this TmphSubArray<TValueType> array, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getKey, /*Type[0]*/ulong/*Type[0]*/ nullValue)
        {
            /*Type[0]*/
            ulong/*Type[0]*/ value;
            return minKey(array, getKey, out value) ? value : nullValue;
        }

        /// <summary>
        /// 获取最小值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="TmphSubArray">数据数组</param>
        /// <param name="getKey">数据获取器</param>
        /// <param name="value">最小值</param>
        /// <returns>是否存在最小值</returns>
        public static bool min<TValueType>
            (this TmphSubArray<TValueType> TmphSubArray, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getKey, out TValueType value)
        {
            if (TmphSubArray.Count != 0)
            {
                TValueType[] array = TmphSubArray.Array;
                int index = TmphSubArray.StartIndex, endIndex = index + TmphSubArray.Count;
                /*Type[0]*/
                ulong/*Type[0]*/ minKey = getKey(value = array[index]);
                while (++index != endIndex)
                {
                    TValueType nextValue = array[index];
                    /*Type[0]*/
                    ulong/*Type[0]*/ nextKey = getKey(nextValue);
                    if (nextKey < minKey)
                    {
                        minKey = nextKey;
                        value = nextValue;
                    }
                }
                return true;
            }
            value = default(TValueType);
            return false;
        }

        /// <summary>
        /// 获取最小值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数据数组</param>
        /// <param name="getKey">数据获取器</param>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最小值,失败返回默认空值</returns>
        public static TValueType min<TValueType>(this TmphSubArray<TValueType> array, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getKey, TValueType nullValue)
        {
            TValueType value;
            return min(array, getKey, out value) ? value : nullValue;
        }
    }
}

namespace Laurent.Lee.CLB.Unsafer
{
    /// <summary>
    /// 数组非安全扩展操作(请自行确保数据可靠性)
    /// </summary>
    public static partial class TmphArrayExtension
    {
        /// <summary>
        /// 移动数据块
        /// </summary>
        /// <param name="array">待处理数组</param>
        /// <param name="index">原始数据位置</param>
        /// <param name="writeIndex">目标数据位置</param>
        /// <param name="count">移动数据数量</param>
        public unsafe static void Move(/*Type[0]*/ulong/*Type[0]*/[] array, int index, int writeIndex, int count)
        {
#if MONO
            int endIndex = index + count;
            if (index < writeIndex && endIndex > writeIndex)
            {
                fixed (/*Type[0]*/ulong/*Type[0]*/* valueFixed = array)
                {
                    for (/*Type[0]*/ulong/*Type[0]*/* write = valueFixed + writeIndex + count, start = valueFixed + index, end = valueFixed + endIndex;
                        end != start;
                        *--write = *--end) ;
                }
            }
            else Array.Copy(array, index, array, writeIndex, count);
#else
            fixed (/*Type[0]*/ulong/*Type[0]*/* valueFixed = array) Win32.TmphKernel32.RtlMoveMemory(valueFixed + writeIndex, valueFixed + index, count * sizeof(/*Type[0]*/ulong/*Type[0]*/));
#endif
        }
    }
}