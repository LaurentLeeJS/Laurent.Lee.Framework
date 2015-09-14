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

/*Type:ulong;long;uint;int;DateTime*/

namespace Laurent.Lee.CLB
{
    /// <summary>
    /// 数组扩展操作
    /// </summary>
    public static partial class TmphArrayExtension
    {
        /// <summary>
        /// 数据去重
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数据数组</param>
        /// <returns>目标数据集合</returns>
        public unsafe static TmphSubArray</*Type[0]*/ulong/*Type[0]*/> distinct(this /*Type[0]*/ulong/*Type[0]*/[] array)
        {
            if (array == null) return default(TmphSubArray</*Type[0]*/ulong/*Type[0]*/>);
            if (array.Length <= 1) return TmphSubArray</*Type[0]*/ulong/*Type[0]*/>.Unsafe(array, 0, array.Length);
            TmphArrayExtension.sort(array, 0, array.Length);
            fixed (/*Type[0]*/ulong/*Type[0]*/* valueFixed = array)
            {
                /*Type[0]*/
                ulong/*Type[0]*/* start = valueFixed + 1, end = valueFixed + array.Length, write = valueFixed;
                do
                {
                    if (*start != *write) *++write = *start;
                }
                while (++start != end);
                return TmphSubArray</*Type[0]*/ulong/*Type[0]*/>.Unsafe(array, 0, (int)(write - valueFixed) + 1);
            }
        }

        /// <summary>
        /// 数据去重
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数据数组</param>
        /// <param name="getValue">数据获取器</param>
        /// <returns>目标数据集合</returns>
        public static TValueType[] distinct<TValueType>
            (this /*Type[0]*/ulong/*Type[0]*/[] array, Func</*Type[0]*/ulong/*Type[0]*/, TValueType> getValue)
        {
            return new TmphSubArray</*Type[0]*/ulong/*Type[0]*/>(array).distinct(getValue);
        }

        /// <summary>
        /// 数据去重
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数据数组</param>
        /// <param name="getValue">数据获取器</param>
        /// <returns>目标数据集合</returns>
        public static TmphSubArray</*Type[0]*/ulong/*Type[0]*/> distinct<TValueType>
            (this TValueType[] array, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getValue)
        {
            if (getValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            /*Type[0]*/
            ulong/*Type[0]*/[] newValues = array.getArray(getValue);
            TmphArrayExtension.sort(newValues, 0, newValues.Length);
            return newValues.distinct();
        }

        /// <summary>
        /// 求交集
        /// </summary>
        /// <param name="left">左侧数据</param>
        /// <param name="right">右侧数据</param>
        /// <returns>数据交集</returns>
        public unsafe static TmphSubArray</*Type[0]*/ulong/*Type[0]*/> intersect(this /*Type[0]*/ulong/*Type[0]*/[] left, /*Type[0]*/ulong/*Type[0]*/[] right)
        {
            int leftLength = left.length(), rightLength = right.length();
            if (leftLength != 0 && rightLength != 0)
            {
                /*Type[0]*/
                ulong/*Type[0]*/[] min = leftLength <= rightLength ? left : right, values = new /*Type[0]*/ulong/*Type[0]*/[min.Length];
                fixed (/*Type[0]*/ulong/*Type[0]*/* valueFixed = values)
                {
                    /*Type[0]*/
                    ulong/*Type[0]*/* write = valueFixed;
                    TmphStaticHashSet</*Type[0]*/ulong/*Type[0]*/> hash = new TmphStaticHashSet</*Type[0]*/ulong/*Type[0]*/>(min);
                    foreach (/*Type[0]*/ulong/*Type[0]*/ value in leftLength <= rightLength ? right : left)
                    {
                        if (hash.Contains(value)) *write++ = value;
                    }
                    return TmphSubArray</*Type[0]*/ulong/*Type[0]*/>.Unsafe(values, 0, (int)(write - valueFixed));
                }
            }
            return default(TmphSubArray</*Type[0]*/ulong/*Type[0]*/>);
        }

        /// <summary>
        /// 数据排序分组数量
        /// </summary>
        /// <param name="array">数据数组</param>
        /// <returns>分组数量</returns>
        public static TmphKeyValue</*Type[0]*/ulong/*Type[0]*/, int>[] sortGroupCount(this /*Type[0]*/ulong/*Type[0]*/[] array)
        {
            return new TmphSubArray</*Type[0]*/ulong/*Type[0]*/>(array).sortGroupCount();
        }

        /// <summary>
        /// 数据排序分组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数据数组</param>
        /// <param name="getValue">数据获取器</param>
        /// <returns>目标数据集合</returns>
        public static TmphSubArray<TmphSubArray<TValueType>> sortGroup<TValueType>
            (this TValueType[] array, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getValue)
        {
            return new TmphSubArray<TValueType>(array).sortGroup(getValue);
        }

        /// <summary>
        /// 数据排序分组数量
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数据数组</param>
        /// <param name="getValue">数据获取器</param>
        /// <returns>数据排序分组数量</returns>
        public static int sortGroupCount<TValueType>
            (this TValueType[] array, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getValue)
        {
            return new TmphSubArray<TValueType>(array).sortGroupCount(getValue);
        }
    }

    /// <summary>
    /// 数组子串扩展操作
    /// </summary>
    public static partial class TmphSubArrayExtension
    {
        /// <summary>
        /// 数据去重
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数据数组</param>
        /// <returns>目标数据集合</returns>
        public unsafe static TmphSubArray</*Type[0]*/ulong/*Type[0]*/> distinct(this TmphSubArray</*Type[0]*/ulong/*Type[0]*/> array)
        {
            if (array.Count > 1)
            {
                TmphArrayExtension.sort(array.Array, array.StartIndex, array.Count);
                fixed (/*Type[0]*/ulong/*Type[0]*/* valueFixed = array.Array)
                {
                    /*Type[0]*/
                    ulong/*Type[0]*/* write = valueFixed + array.StartIndex, start = write + 1, end = write + array.Count;
                    do
                    {
                        if (*start != *write) *++write = *start;
                    }
                    while (++start != end);
                    return TmphSubArray</*Type[0]*/ulong/*Type[0]*/>.Unsafe(array.Array, array.StartIndex, (int)(write - valueFixed) + 1 - array.StartIndex);
                }
            }
            return array;
        }

        /// <summary>
        /// 数据去重
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数据数组</param>
        /// <param name="getValue">数据获取器</param>
        /// <returns>目标数据集合</returns>
        public unsafe static TValueType[] distinct<TValueType>
            (this TmphSubArray</*Type[0]*/ulong/*Type[0]*/> array, Func</*Type[0]*/ulong/*Type[0]*/, TValueType> getValue)
        {
            if (array.Count != 0)
            {
                if (getValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                TmphArrayExtension.sort(array.Array, array.StartIndex, array.Count);
                fixed (/*Type[0]*/ulong/*Type[0]*/* valueFixed = array.Array)
                {
                    /*Type[0]*/
                    ulong/*Type[0]*/* start = valueFixed + array.StartIndex, end = start + array.Count;
                    /*Type[0]*/
                    ulong/*Type[0]*/ value = *start;
                    int count = 1;
                    while (++start != end)
                    {
                        if (*start != value)
                        {
                            ++count;
                            value = *start;
                        }
                    }
                    TValueType[] values = new TValueType[count];
                    values[0] = getValue(value = *(start = valueFixed + array.StartIndex));
                    count = 1;
                    while (++start != end)
                    {
                        if (*start != value) values[count++] = getValue(value = *start);
                    }
                    return values;
                }
            }
            return TmphNullValue<TValueType>.Array;
        }

        /// <summary>
        /// 数据去重
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数据数组</param>
        /// <param name="getValue">数据获取器</param>
        /// <returns>目标数据集合</returns>
        public unsafe static TmphSubArray</*Type[0]*/ulong/*Type[0]*/> distinct<TValueType>
            (this TmphSubArray<TValueType> array, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getValue)
        {
            if (array.Count != 0)
            {
                if (getValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                /*Type[0]*/
                ulong/*Type[0]*/[] newValues = array.getArray(getValue);
                TmphArrayExtension.sort(newValues, 0, newValues.Length);
                return newValues.distinct();
            }
            return default(TmphSubArray</*Type[0]*/ulong/*Type[0]*/>);
        }

        /// <summary>
        /// 数据排序分组数量
        /// </summary>
        /// <param name="array">数据数组</param>
        /// <returns>分组数量</returns>
        public unsafe static TmphKeyValue</*Type[0]*/ulong/*Type[0]*/, int>[] sortGroupCount(this TmphSubArray</*Type[0]*/ulong/*Type[0]*/> array)
        {
            if (array.Count != 0)
            {
                TmphArrayExtension.sort(array.Array, array.StartIndex, array.Count);
                fixed (/*Type[0]*/ulong/*Type[0]*/* valueFixed = array.Array)
                {
                    /*Type[0]*/
                    ulong/*Type[0]*/* start = valueFixed + array.StartIndex, lastStart = start, end = start + array.Count;
                    /*Type[0]*/
                    ulong/*Type[0]*/ value = *start;
                    int count = 1;
                    while (++start != end)
                    {
                        if (*start != value)
                        {
                            ++count;
                            value = *start;
                        }
                    }
                    TmphKeyValue</*Type[0]*/ulong/*Type[0]*/, int>[] values = new TmphKeyValue</*Type[0]*/ulong/*Type[0]*/, int>[count];
                    value = *(start = lastStart);
                    count = 0;
                    while (++start != end)
                    {
                        if (*start != value)
                        {
                            values[count++].Set(value, (int)(start - lastStart));
                            value = *start;
                            lastStart = start;
                        }
                    }
                    values[count].Set(value, (int)(start - lastStart));
                    return values;
                }
            }
            return TmphNullValue<TmphKeyValue</*Type[0]*/ulong/*Type[0]*/, int>>.Array;
        }

        /// <summary>
        /// 数据排序分组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数据数组</param>
        /// <param name="getValue">数据获取器</param>
        /// <returns>目标数据集合</returns>
        public unsafe static TmphSubArray<TmphSubArray<TValueType>> sortGroup<TValueType>
            (this TmphSubArray<TValueType> array, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getValue)
        {
            if (array.Count != 0)
            {
                if (getValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                TValueType[] sortArray = TmphArrayExtension.getSort(array.Array, getValue, array.StartIndex, array.Count);
                TmphSubArray<TValueType>[] values = new TmphSubArray<TValueType>[sortArray.Length];
                /*Type[0]*/
                ulong/*Type[0]*/ key = getValue(sortArray[0]);
                int startIndex = 0, valueIndex = 0;
                for (int index = 1; index != sortArray.Length; ++index)
                {
                    /*Type[0]*/
                    ulong/*Type[0]*/ nextKey = getValue(sortArray[index]);
                    if (key != nextKey)
                    {
                        values[valueIndex++].UnsafeSet(sortArray, startIndex, index - startIndex);
                        key = nextKey;
                        startIndex = index;
                    }
                }
                values[valueIndex++].UnsafeSet(sortArray, startIndex, sortArray.Length - startIndex);
                return TmphSubArray<TmphSubArray<TValueType>>.Unsafe(values, 0, valueIndex);
            }
            return default(TmphSubArray<TmphSubArray<TValueType>>);
        }

        /// <summary>
        /// 数据排序分组数量
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数据数组</param>
        /// <param name="getValue">数据获取器</param>
        /// <returns>目标数据集合数量</returns>
        public unsafe static int sortGroupCount<TValueType>
            (this TmphSubArray<TValueType> array, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getValue)
        {
            if (array.Count != 0)
            {
                if (getValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                TValueType[] sortArray = TmphArrayExtension.getSort(array.Array, getValue, array.StartIndex, array.Count);
                /*Type[0]*/
                ulong/*Type[0]*/ key = getValue(sortArray[0]);
                int count = 0;
                for (int index = 1; index != sortArray.Length; ++index)
                {
                    /*Type[0]*/
                    ulong/*Type[0]*/ nextKey = getValue(sortArray[index]);
                    if (key != nextKey)
                    {
                        ++count;
                        key = nextKey;
                    }
                }
                return count + 1;
            }
            return 0;
        }
    }
}