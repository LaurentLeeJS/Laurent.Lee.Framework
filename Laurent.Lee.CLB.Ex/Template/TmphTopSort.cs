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

/*Type:ulong,ulongRangeSorter,ulongSortIndex,ulongRangeIndexSorter;long,longRangeSorter,longSortIndex,longRangeIndexSorter;uint,uintRangeSorter,TmphUintSortIndex,uintRangeIndexSorter;int,intRangeSorter,intSortIndex,intRangeIndexSorter;double,doubleRangeSorter,doubleSortIndex,doubleRangeIndexSorter;float,floatRangeSorter,floatSortIndex,floatRangeIndexSorter;DateTime,dateTimeRangeSorter,dateTimeSortIndex,dateTimeRangeIndexSorter*/
/*Compare:,<,<=;Desc,>,>=*/

namespace Laurent.Lee.CLB.Algorithm
{
    /// <summary>
    /// 快速排序
    /// </summary>
    public static partial class TmphQuickSort
    {
        /// <summary>
        /// 排序取Top N
        /// </summary>
        /// <param name="values">待排序数组</param>
        /// <param name="count">排序数据数量</param>
        /// <returns>排序后的数据</returns>
        public unsafe static TmphSubArray</*Type[0]*/ulong/*Type[0]*/> GetTop/*Compare[0]*//*Compare[0]*/(/*Type[0]*/ulong/*Type[0]*/[] values, int count)
        {
            if (values == null) return default(TmphSubArray</*Type[0]*/ulong/*Type[0]*/>);
            if (count > 0)
            {
                if (count < values.Length)
                {
                    if (count <= values.Length >> 1) return getTop/*Compare[0]*//*Compare[0]*/(values, count);
                    values = getRemoveTop/*Compare[0]*//*Compare[0]*/(values, count);
                }
                else
                {
                    /*Type[0]*/
                    ulong/*Type[0]*/[] newValues = new /*Type[0]*/ulong/*Type[0]*/[values.Length];
                    Array.Copy(values, 0, newValues, 0, values.Length);
                    values = newValues;
                }
                return TmphSubArray</*Type[0]*/ulong/*Type[0]*/>.Unsafe(values, 0, values.Length);
            }
            return default(TmphSubArray</*Type[0]*/ulong/*Type[0]*/>);
        }

        /// <summary>
        /// 排序取Top N
        /// </summary>
        /// <param name="values">待排序数组</param>
        /// <param name="count">排序数据数量</param>
        /// <returns>排序后的数据</returns>
        public unsafe static TmphSubArray</*Type[0]*/ulong/*Type[0]*/> Top/*Compare[0]*//*Compare[0]*/(/*Type[0]*/ulong/*Type[0]*/[] values, int count)
        {
            if (values == null) return default(TmphSubArray</*Type[0]*/ulong/*Type[0]*/>);
            if (count > 0)
            {
                if (count < values.Length)
                {
                    if (count <= values.Length >> 1) return getTop/*Compare[0]*//*Compare[0]*/(values, count);
                    values = getRemoveTop/*Compare[0]*//*Compare[0]*/(values, count);
                }
                return TmphSubArray</*Type[0]*/ulong/*Type[0]*/>.Unsafe(values, 0, values.Length);
            }
            return default(TmphSubArray</*Type[0]*/ulong/*Type[0]*/>);
        }

        /// <summary>
        /// 排序取Top N
        /// </summary>
        /// <param name="values">待排序数组</param>
        /// <param name="count">排序数据数量</param>
        /// <returns>排序后的数据</returns>
        private unsafe static TmphSubArray</*Type[0]*/ulong/*Type[0]*/> getTop/*Compare[0]*//*Compare[0]*/(/*Type[0]*/ulong/*Type[0]*/[] values, int count)
        {
            uint sqrtMod;
            int length = Math.Min(Math.Max(count << 2, count + (int)TmphNumber.sqrt((uint)values.Length, out sqrtMod)), values.Length);
            /*Type[0]*/
            ulong/*Type[0]*/[] newValues = new /*Type[0]*/ulong/*Type[0]*/[length];
            fixed (/*Type[0]*/ulong/*Type[0]*/* newValueFixed = newValues, valueFixed = values)
            {
                /*Type[0]*/
                ulong/*Type[0]*/* readIndex = valueFixed + values.Length - length;
                Buffer.BlockCopy(values, (int)((byte*)readIndex - (byte*)valueFixed), newValues, 0, length * sizeof(/*Type[0]*/ulong/*Type[0]*/));
                //Unsafe.TmphMemory.Copy(readIndex, newValueFixed, length * sizeof(/*Type[0]*/ulong/*Type[0]*/));
                /*Type[0]*/
                ulong/*Type[0]*/* writeStat = newValueFixed + count, writeEnd = newValueFixed + --length, writeIndex = writeStat;
                /*Type[1]*/
                ulongRangeSorter/*Type[1]*//*Compare[0]*//*Compare[0]*/ sort
                    = new /*Type[1]*/ulongRangeSorter/*Type[1]*//*Compare[0]*//*Compare[0]*/
                    {
                        SkipCount = writeStat - 1,
                        GetEndIndex = writeStat - 1
                    };
                sort.Sort(newValueFixed, writeEnd);
                for (/*Type[0]*/ulong/*Type[0]*/ maxValue = *sort.SkipCount; readIndex != valueFixed;)
                {
                    if (*--readIndex /*Compare[1]*/</*Compare[1]*/ maxValue)
                    {
                        *writeIndex = *readIndex;
                        if (writeIndex == writeEnd)
                        {
                            sort.Sort(newValueFixed, writeEnd);
                            writeIndex = writeStat;
                            maxValue = *sort.SkipCount;
                        }
                        else ++writeIndex;
                    }
                }
                if (writeIndex != writeStat) sort.Sort(newValueFixed, --writeIndex);
            }
            return TmphSubArray</*Type[0]*/ulong/*Type[0]*/>.Unsafe(newValues, 0, count);
        }

        /// <summary>
        /// 排序去除Top N
        /// </summary>
        /// <param name="values">待排序数组</param>
        /// <param name="count">排序数据数量</param>
        /// <returns>排序后的数据</returns>
        private unsafe static /*Type[0]*/ulong/*Type[0]*/[] getRemoveTop/*Compare[0]*//*Compare[0]*/
            (/*Type[0]*/ulong/*Type[0]*/[] values, int count)
        {
            /*Type[0]*/
            ulong/*Type[0]*/[] newValues = new /*Type[0]*/ulong/*Type[0]*/[count];
            count = values.Length - count;
            uint sqrtMod;
            int length = Math.Min(Math.Max(count << 2, count + (int)TmphNumber.sqrt((uint)values.Length, out sqrtMod)), values.Length);
            /*Type[0]*/
            ulong/*Type[0]*/[] removeValues = new /*Type[0]*/ulong/*Type[0]*/[length];
            fixed (/*Type[0]*/ulong/*Type[0]*/* newValueFixed = newValues, removeFixed = removeValues, valueFixed = values)
            {
                int copyCount = length - count, copyLength = copyCount * sizeof(/*Type[0]*/ulong/*Type[0]*/);
                /*Type[0]*/
                ulong/*Type[0]*/* readIndex = valueFixed + values.Length - length, removeStart = removeFixed + copyCount;
                Buffer.BlockCopy(values, (int)((byte*)readIndex - (byte*)valueFixed), removeValues, 0, length * sizeof(/*Type[0]*/ulong/*Type[0]*/));
                //Unsafe.TmphMemory.Copy(readIndex, removeFixed, length * sizeof(/*Type[0]*/ulong/*Type[0]*/));
                /*Type[0]*/
                ulong/*Type[0]*/* removeEnd = removeFixed + --length, removeIndex = removeStart, writeIndex = newValueFixed + copyCount;
                /*Type[1]*/
                ulongRangeSorter/*Type[1]*//*Compare[0]*//*Compare[0]*/ sort
                    = new /*Type[1]*/ulongRangeSorter/*Type[1]*//*Compare[0]*//*Compare[0]*/
                    {
                        SkipCount = removeStart,
                        GetEndIndex = removeStart
                    };
                sort.Sort(removeFixed, removeEnd);
                Buffer.BlockCopy(removeValues, 0, newValues, 0, copyLength);
                //Unsafe.TmphMemory.Copy(removeFixed, newValueFixed, copyLength);
                for (/*Type[0]*/ulong/*Type[0]*/ maxValue = *removeStart; readIndex != valueFixed;)
                {
                    if (*--readIndex /*Compare[2]*/<=/*Compare[2]*/ maxValue) *writeIndex++ = *readIndex;
                    else
                    {
                        *--removeIndex = *readIndex;
                        if (removeIndex == removeFixed)
                        {
                            sort.Sort(removeFixed, removeEnd);
                            removeIndex = removeStart;
                            maxValue = *removeStart;
                            Buffer.BlockCopy(removeValues, 0, newValues, (int)((byte*)writeIndex - (byte*)newValueFixed), copyLength);
                            //Unsafe.TmphMemory.Copy(removeFixed, writeIndex, copyLength);
                            writeIndex += copyCount;
                        }
                    }
                }
                if (removeIndex != removeStart)
                {
                    sort.Sort(removeIndex, removeEnd);
                    Buffer.BlockCopy(removeValues, (int)((byte*)removeIndex - (byte*)removeFixed), newValues, (int)((byte*)writeIndex - (byte*)newValueFixed), (int)((byte*)removeStart - (byte*)removeIndex));
                    //Unsafe.TmphMemory.Copy(removeIndex, writeIndex, (int)(removeStart - removeIndex) * sizeof(/*Type[0]*/ulong/*Type[0]*/));
                }
            }
            return newValues;
        }

        /// <summary>
        /// 排序取Top N
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="values">待排序数组</param>
        /// <param name="getKey">排序键值获取器</param>
        /// <param name="count">排序数据数量</param>
        /// <returns>排序后的数据</returns>
        public static TValueType[] GetTop/*Compare[0]*//*Compare[0]*/<TValueType>
            (TValueType[] values, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getKey, int count)
        {
            if (count > 0)
            {
                if (count < values.length())
                {
                    if (count <= values.Length >> 1) return getTop/*Compare[0]*//*Compare[0]*/(values, getKey, count);
                    else return getRemoveTop/*Compare[0]*//*Compare[0]*/(values, getKey, count);
                }
                return values.copy();
            }
            return TmphNullValue<TValueType>.Array;
        }

        /// <summary>
        /// 排序取Top N
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="values">待排序数组</param>
        /// <param name="getKey">排序键值获取器</param>
        /// <param name="count">排序数据数量</param>
        /// <returns>排序后的数据</returns>
        public static TValueType[] Top/*Compare[0]*//*Compare[0]*/<TValueType>
            (TValueType[] values, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getKey, int count)
        {
            if (count > 0)
            {
                if (count < values.length())
                {
                    if (count <= values.Length >> 1) return getTop/*Compare[0]*//*Compare[0]*/(values, getKey, count);
                    else return getRemoveTop/*Compare[0]*//*Compare[0]*/(values, getKey, count);
                }
                return values.notNull();
            }
            return TmphNullValue<TValueType>.Array;
        }

        /// <summary>
        /// 排序取Top N
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="values">待排序数组</param>
        /// <param name="getKey">排序键值获取器</param>
        /// <param name="count">排序数据数量</param>
        /// <returns>排序后的数据</returns>
        private unsafe static TValueType[] getTop/*Compare[0]*//*Compare[0]*/<TValueType>
            (TValueType[] values, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getKey, int count)
        {
            uint sqrtMod;
            int length = Math.Min(Math.Max(count << 2, count + (int)TmphNumber.sqrt((uint)values.Length, out sqrtMod)), values.Length);
            TmphUnmanagedPool pool = CLB.TmphUnmanagedPool.GetDefaultPool(length * sizeof(/*Type[2]*/ulongSortIndex/*Type[2]*/));
            TmphPointer data = pool.Get(length * sizeof(/*Type[2]*/ulongSortIndex/*Type[2]*/));
            try
            {
                return getTop/*Compare[0]*//*Compare[0]*/(values, getKey, count, length, (/*Type[2]*/ulongSortIndex/*Type[2]*/*)data.Data);
            }
            finally { pool.Push(ref data); }
        }

        /// <summary>
        /// 排序取Top N
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="values">待排序数组</param>
        /// <param name="getKey">排序键值获取器</param>
        /// <param name="count">排序数据数量</param>
        /// <param name="length">排序缓存区尺寸</param>
        /// <param name="indexFixed">索引位置</param>
        /// <returns>排序后的数据</returns>
        private unsafe static TValueType[] getTop/*Compare[0]*//*Compare[0]*/<TValueType>
            (TValueType[] values, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getKey, int count, int length
            , /*Type[2]*/ulongSortIndex/*Type[2]*/* indexFixed)
        {
            /*Type[2]*/
            ulongSortIndex/*Type[2]*/* writeEnd = indexFixed;
            int index = 0;
            while (index != length) (*writeEnd++).Set(getKey(values[index]), index++);
            /*Type[2]*/
            ulongSortIndex/*Type[2]*/* writeStat = indexFixed + count, writeIndex = writeStat;
            /*Type[3]*/
            ulongRangeIndexSorter/*Type[3]*//*Compare[0]*//*Compare[0]*/ sort
                = new /*Type[3]*/ulongRangeIndexSorter/*Type[3]*//*Compare[0]*//*Compare[0]*/
                {
                    SkipCount = writeStat - 1,
                    GetEndIndex = writeStat - 1
                };
            sort.Sort(indexFixed, --writeEnd);
            for (/*Type[0]*/ulong/*Type[0]*/ maxValue = (*sort.SkipCount).Value; index != values.Length; ++index)
            {
                /*Type[0]*/
                ulong/*Type[0]*/ value = getKey(values[index]);
                if (value /*Compare[1]*/</*Compare[1]*/ maxValue)
                {
                    (*writeIndex).Set(value, index);
                    if (writeIndex == writeEnd)
                    {
                        sort.Sort(indexFixed, writeEnd);
                        writeIndex = writeStat;
                        maxValue = (*sort.SkipCount).Value;
                    }
                    else ++writeIndex;
                }
            }
            if (writeIndex != writeStat) sort.Sort(indexFixed, --writeIndex);
            TValueType[] newValues = new TValueType[count];
            for (writeIndex = indexFixed, index = 0; index != count; ++index)
            {
                newValues[index] = values[(*writeIndex++).Index];
            }
            return newValues;
        }

        /// <summary>
        /// 排序去除Top N
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="values">待排序数组</param>
        /// <param name="getKey">排序键值获取器</param>
        /// <param name="count">排序数据数量</param>
        /// <returns>排序后的数据</returns>
        private unsafe static TValueType[] getRemoveTop/*Compare[0]*//*Compare[0]*/<TValueType>
            (TValueType[] values, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getKey, int count)
        {
            TValueType[] newValues = new TValueType[count];
            count = values.Length - count;
            uint sqrtMod;
            int length = Math.Min(Math.Max(count << 2, count + (int)TmphNumber.sqrt((uint)values.Length, out sqrtMod)), values.Length);
            TmphUnmanagedPool pool = CLB.TmphUnmanagedPool.GetDefaultPool(length * sizeof(/*Type[2]*/ulongSortIndex/*Type[2]*/));
            TmphPointer data = pool.Get(length * sizeof(/*Type[2]*/ulongSortIndex/*Type[2]*/));
            try
            {
                removeTop/*Compare[0]*//*Compare[0]*/(values, getKey, count, newValues, length, (/*Type[2]*/ulongSortIndex/*Type[2]*/*)data.Data);
            }
            finally { pool.Push(ref data); }
            return newValues;
        }

        /// <summary>
        /// 排序去除Top N
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="values">待排序数组</param>
        /// <param name="getKey">排序键值获取器</param>
        /// <param name="count">排序数据数量</param>
        /// <param name="newValues">目标数据数组</param>
        /// <param name="length">排序缓存区尺寸</param>
        /// <param name="indexFixed">索引位置</param>
        private unsafe static void removeTop/*Compare[0]*//*Compare[0]*/<TValueType>
            (TValueType[] values, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getKey, int count, TValueType[] newValues, int length
            , /*Type[2]*/ulongSortIndex/*Type[2]*/* removeFixed)
        {
            int index = 0, writeIndex = 0;
            /*Type[2]*/
            ulongSortIndex/*Type[2]*/* removeEnd = removeFixed;
            while (index != length) (*removeEnd++).Set(getKey(values[index]), index++);
            /*Type[2]*/
            ulongSortIndex/*Type[2]*/* removeStart = removeFixed + (count = length - count), removeIndex = removeFixed;
            /*Type[3]*/
            ulongRangeIndexSorter/*Type[3]*//*Compare[0]*//*Compare[0]*/ sort
                = new /*Type[3]*/ulongRangeIndexSorter/*Type[3]*//*Compare[0]*//*Compare[0]*/
                {
                    SkipCount = removeStart,
                    GetEndIndex = removeStart
                };
            sort.Sort(removeFixed, --removeEnd);
            while (writeIndex != count) newValues[writeIndex++] = values[(*removeIndex++).Index];
            for (/*Type[0]*/ulong/*Type[0]*/ maxValue = (*removeStart).Value; index != values.Length; ++index)
            {
                /*Type[0]*/
                ulong/*Type[0]*/ value = getKey(values[index]);
                if (value /*Compare[2]*/<=/*Compare[2]*/ maxValue) newValues[writeIndex++] = values[index];
                else
                {
                    (*--removeIndex).Set(value, index);
                    if (removeIndex == removeFixed)
                    {
                        sort.Sort(removeFixed, removeEnd);
                        for (removeIndex = removeFixed; removeIndex != removeStart; newValues[writeIndex++] = values[(*removeIndex++).Index]) ;
                        maxValue = (*removeStart).Value;
                    }
                }
            }
            if (removeIndex != removeStart)
            {
                sort.Sort(removeIndex, removeEnd);
                while (removeIndex != removeStart) newValues[writeIndex++] = values[(*removeIndex++).Index];
            }
        }
    }
}