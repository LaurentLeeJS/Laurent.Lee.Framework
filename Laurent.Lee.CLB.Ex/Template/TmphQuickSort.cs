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

/*Type:ulong,ulongSortIndex;long,longSortIndex;uint,TmphUintSortIndex;int,intSortIndex;double,doubleSortIndex;float,floatSortIndex;DateTime,dateTimeSortIndex*/
/*Compare:,>,<;Desc,<,>*/

namespace Laurent.Lee.CLB.Algorithm
{
    /// <summary>
    /// 快速排序
    /// </summary>
    public static partial class TmphQuickSort
    {
        /// <summary>
        /// 快速排序子过程
        /// </summary>
        /// <param name="startIndex">起始位置</param>
        /// <param name="endIndex">结束位置-1</param>
        private unsafe static void sort/*Compare[0]*//*Compare[0]*/
            (/*Type[0]*/ulong/*Type[0]*/* startIndex, /*Type[0]*/ulong/*Type[0]*/* endIndex)
        {
            do
            {
                /*Type*/
                ulong/*Type*/ leftValue = *startIndex, rightValue = *endIndex;
                int average = (int)(endIndex - startIndex) >> 1;
                if (average == 0)
                {
                    if (leftValue /*Compare[1]*/>/*Compare[1]*/ rightValue)
                    {
                        *startIndex = rightValue;
                        *endIndex = leftValue;
                    }
                    break;
                }
                /*Type*/
                ulong/*Type*/* leftIndex = startIndex, rightIndex = endIndex, averageIndex = startIndex + average;
                /*Type*/
                ulong/*Type*/ value = *averageIndex;
                if (leftValue /*Compare[1]*/>/*Compare[1]*/ value)
                {
                    if (leftValue /*Compare[1]*/>/*Compare[1]*/ rightValue)
                    {
                        *rightIndex = leftValue;
                        if (value /*Compare[1]*/>/*Compare[1]*/ rightValue) *leftIndex = rightValue;
                        else
                        {
                            *leftIndex = value;
                            *averageIndex = value = rightValue;
                        }
                    }
                    else
                    {
                        *leftIndex = value;
                        *averageIndex = value = leftValue;
                    }
                }
                else
                {
                    if (value /*Compare[1]*/>/*Compare[1]*/ rightValue)
                    {
                        *rightIndex = value;
                        if (leftValue /*Compare[1]*/>/*Compare[1]*/ rightValue)
                        {
                            *leftIndex = rightValue;
                            *averageIndex = value = leftValue;
                        }
                        else *averageIndex = value = rightValue;
                    }
                }
                ++leftIndex;
                --rightIndex;
                do
                {
                    while (*leftIndex /*Compare[2]*/</*Compare[2]*/ value) ++leftIndex;
                    while (value /*Compare[2]*/</*Compare[2]*/ *rightIndex) --rightIndex;
                    if (leftIndex < rightIndex)
                    {
                        leftValue = *leftIndex;
                        *leftIndex = *rightIndex;
                        *rightIndex = leftValue;
                    }
                    else
                    {
                        if (leftIndex == rightIndex)
                        {
                            ++leftIndex;
                            --rightIndex;
                        }
                        break;
                    }
                }
                while (++leftIndex <= --rightIndex);
                if (rightIndex - startIndex <= endIndex - leftIndex)
                {
                    if (startIndex < rightIndex) sort/*Compare[0]*//*Compare[0]*/(startIndex, rightIndex);
                    startIndex = leftIndex;
                }
                else
                {
                    if (leftIndex < endIndex) sort/*Compare[0]*//*Compare[0]*/(leftIndex, endIndex);
                    endIndex = rightIndex;
                }
            }
            while (startIndex < endIndex);
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="values">待排序数组</param>
        public unsafe static void Sort/*Compare[0]*//*Compare[0]*/(/*Type[0]*/ulong/*Type[0]*/[] values)
        {
            if (values.Length > 1)
            {
                fixed (/*Type[0]*/ulong/*Type[0]*/* valueFixed = values)
                {
                    sort/*Compare[0]*//*Compare[0]*/(valueFixed, valueFixed + values.Length - 1);
                }
            }
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="values">待排序数组</param>
        /// <returns>排序后的新数组</returns>
        public unsafe static /*Type[0]*/ulong/*Type[0]*/[] GetSort/*Compare[0]*//*Compare[0]*/(/*Type[0]*/ulong/*Type[0]*/[] values)
        {
            if (values.Length != 0)
            {
                /*Type[0]*/
                ulong/*Type[0]*/[] newValue = new /*Type[0]*/ulong/*Type[0]*/[values.Length];
                Buffer.BlockCopy(values, 0, newValue, 0, values.Length * sizeof(/*Type[0]*/ulong/*Type[0]*/));
                fixed (/*Type[0]*/ulong/*Type[0]*/* newValueFixed = newValue, valueFixed = values)
                {
                    sort/*Compare[0]*//*Compare[0]*/(newValueFixed, newValueFixed + values.Length - 1);
                }
                return newValue;
            }
            return values;
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="values">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">排序数据数量</param>
        public unsafe static void Sort/*Compare[0]*//*Compare[0]*/(/*Type[0]*/ulong/*Type[0]*/[] values, int startIndex, int count)
        {
            fixed (/*Type[0]*/ulong/*Type[0]*/* valueFixed = values)
            {
                /*Type[0]*/
                ulong/*Type[0]*/* start = valueFixed + startIndex;
                sort/*Compare[0]*//*Compare[0]*/(start, start + count - 1);
            }
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="values">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">排序数据数量</param>
        /// <returns>排序后的新数组</returns>
        public unsafe static /*Type[0]*/ulong/*Type[0]*/[] GetSort/*Compare[0]*//*Compare[0]*/
            (/*Type[0]*/ulong/*Type[0]*/[] values, int startIndex, int count)
        {
            if (count != 0)
            {
                /*Type[0]*/
                ulong/*Type[0]*/[] newValues = new /*Type[0]*/ulong/*Type[0]*/[count];
                Buffer.BlockCopy(values, startIndex * sizeof(/*Type[0]*/ulong/*Type[0]*/), newValues, 0, count * sizeof(/*Type[0]*/ulong/*Type[0]*/));
                fixed (/*Type[0]*/ulong/*Type[0]*/* newValueFixed = newValues, valueFixed = values)
                {
                    if (--count > 0) sort/*Compare[0]*//*Compare[0]*/(newValueFixed, newValueFixed + count);
                }
                return newValues;
            }
            return TmphNullValue</*Type[0]*/ulong/*Type[0]*/>.Array;
        }

        /// <summary>
        /// 索引快速排序子过程
        /// </summary>
        /// <param name="startIndex">起始位置</param>
        /// <param name="endIndex">结束位置-1</param>
        public unsafe static void sort/*Compare[0]*//*Compare[0]*/
            (/*Type[1]*/ulongSortIndex/*Type[1]*/* startIndex, /*Type[1]*/ulongSortIndex/*Type[1]*/* endIndex)
        {
            do
            {
                /*Type[1]*/
                ulongSortIndex/*Type[1]*/ leftValue = *startIndex, rightValue = *endIndex;
                int average = (int)(endIndex - startIndex) >> 1;
                if (average == 0)
                {
                    if (leftValue.Value /*Compare[1]*/>/*Compare[1]*/ rightValue.Value)
                    {
                        *startIndex = rightValue;
                        *endIndex = leftValue;
                    }
                    break;
                }
                /*Type[1]*/
                ulongSortIndex/*Type[1]*/* leftIndex = startIndex, rightIndex = endIndex, averageIndex = startIndex + average;
                /*Type[1]*/
                ulongSortIndex/*Type[1]*/ indexValue = *averageIndex;
                if (leftValue.Value /*Compare[1]*/>/*Compare[1]*/ indexValue.Value)
                {
                    if (leftValue.Value /*Compare[1]*/>/*Compare[1]*/ rightValue.Value)
                    {
                        *rightIndex = leftValue;
                        if (indexValue.Value /*Compare[1]*/>/*Compare[1]*/ rightValue.Value) *leftIndex = rightValue;
                        else
                        {
                            *leftIndex = indexValue;
                            *averageIndex = indexValue = rightValue;
                        }
                    }
                    else
                    {
                        *leftIndex = indexValue;
                        *averageIndex = indexValue = leftValue;
                    }
                }
                else
                {
                    if (indexValue.Value /*Compare[1]*/>/*Compare[1]*/ rightValue.Value)
                    {
                        *rightIndex = indexValue;
                        if (leftValue.Value /*Compare[1]*/>/*Compare[1]*/ rightValue.Value)
                        {
                            *leftIndex = rightValue;
                            *averageIndex = indexValue = leftValue;
                        }
                        else *averageIndex = indexValue = rightValue;
                    }
                }
                ++leftIndex;
                --rightIndex;
                /*Type[0]*/
                ulong/*Type[0]*/ value = indexValue.Value;
                do
                {
                    while ((*leftIndex).Value /*Compare[2]*/</*Compare[2]*/ value) ++leftIndex;
                    while (value /*Compare[2]*/</*Compare[2]*/ (*rightIndex).Value) --rightIndex;
                    if (leftIndex < rightIndex)
                    {
                        leftValue = *leftIndex;
                        *leftIndex = *rightIndex;
                        *rightIndex = leftValue;
                    }
                    else
                    {
                        if (leftIndex == rightIndex)
                        {
                            ++leftIndex;
                            --rightIndex;
                        }
                        break;
                    }
                }
                while (++leftIndex <= --rightIndex);
                if (rightIndex - startIndex <= endIndex - leftIndex)
                {
                    if (startIndex < rightIndex) sort/*Compare[0]*//*Compare[0]*/(startIndex, rightIndex);
                    startIndex = leftIndex;
                }
                else
                {
                    if (leftIndex < endIndex) sort/*Compare[0]*//*Compare[0]*/(leftIndex, endIndex);
                    endIndex = rightIndex;
                }
            }
            while (startIndex < endIndex);
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="values">待排序数组</param>
        /// <param name="getKey">排序键值获取器</param>
        /// <returns>排序后的数组</returns>
        public unsafe static TValueType[] GetSort/*Compare[0]*//*Compare[0]*/<TValueType>
            (TValueType[] values, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getKey)
        {
            TmphUnmanagedPool pool = CLB.TmphUnmanagedPool.GetDefaultPool(values.Length * sizeof(/*Type[1]*/ulongSortIndex/*Type[1]*/));
            TmphPointer data = pool.Get(values.Length * sizeof(/*Type[1]*/ulongSortIndex/*Type[1]*/));
            try
            {
                return getSort/*Compare[0]*//*Compare[0]*/(values, getKey, (/*Type[1]*/ulongSortIndex/*Type[1]*/*)data.Data);
            }
            finally { pool.Push(ref data); }
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="values">待排序数组</param>
        /// <param name="getKey">排序键值获取器</param>
        /// <param name="fixedIndex">索引位置</param>
        /// <returns>排序后的数组</returns>
        private unsafe static TValueType[] getSort/*Compare[0]*//*Compare[0]*/<TValueType>
            (TValueType[] values, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getKey, /*Type[1]*/ulongSortIndex/*Type[1]*/* fixedIndex)
        {
            /*Type[1]*/
            ulongSortIndex/*Type[1]*/.Create(fixedIndex, values, getKey);
            sort/*Compare[0]*//*Compare[0]*/(fixedIndex, fixedIndex + values.Length - 1);
            return /*Type[1]*/ulongSortIndex/*Type[1]*/.Create(fixedIndex, values, values.Length);
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="values">待排序数组</param>
        /// <param name="getKey">排序键值获取器</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">排序数据数量</param>
        /// <returns>排序后的数组</returns>
        public unsafe static TValueType[] GetSort/*Compare[0]*//*Compare[0]*/<TValueType>
            (TValueType[] values, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getKey, int startIndex, int count)
        {
            TmphUnmanagedPool pool = CLB.TmphUnmanagedPool.GetDefaultPool(count * sizeof(/*Type[1]*/ulongSortIndex/*Type[1]*/));
            TmphPointer data = pool.Get(count * sizeof(/*Type[1]*/ulongSortIndex/*Type[1]*/));
            try
            {
                return getSort/*Compare[0]*//*Compare[0]*/(values, getKey, startIndex, count, (/*Type[1]*/ulongSortIndex/*Type[1]*/*)data.Data);
            }
            finally { pool.Push(ref data); }
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="values">待排序数组</param>
        /// <param name="getKey">排序键值获取器</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">排序数据数量</param>
        /// <param name="fixedIndex">索引位置</param>
        /// <returns>排序后的数组</returns>
        private unsafe static TValueType[] getSort/*Compare[0]*//*Compare[0]*/<TValueType>
            (TValueType[] values, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getKey, int startIndex, int count
            , /*Type[1]*/ulongSortIndex/*Type[1]*/* fixedIndex)
        {
            /*Type[1]*/
            ulongSortIndex/*Type[1]*/.Create(fixedIndex, values, getKey, startIndex, count);
            sort/*Compare[0]*//*Compare[0]*/(fixedIndex, fixedIndex + count - 1);
            return /*Type[1]*/ulongSortIndex/*Type[1]*/.Create(fixedIndex, values, count);
        }
    }
}