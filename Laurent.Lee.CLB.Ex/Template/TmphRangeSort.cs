using System;

/*Type:ulong,ulongRangeSorter,ulongSortIndex,ulongRangeIndexSorter;long,longRangeSorter,longSortIndex,longRangeIndexSorter;uint,uintRangeSorter,TmphUintSortIndex,uintRangeIndexSorter;int,intRangeSorter,intSortIndex,intRangeIndexSorter;double,doubleRangeSorter,doubleSortIndex,doubleRangeIndexSorter;float,floatRangeSorter,floatSortIndex,floatRangeIndexSorter;DateTime,dateTimeRangeSorter,dateTimeSortIndex,dateTimeRangeIndexSorter*/
/*Compare:,>,<;Desc,<,>*/

namespace Laurent.Lee.CLB.Algorithm
{
    /// <summary>
    /// 快速排序
    /// </summary>
    public static partial class TmphQuickSort
    {
        /// <summary>
        /// 范围排序器(一般用于获取分页)
        /// </summary>
        public unsafe struct /*Type[1]*/ulongRangeSorter/*Type[1]*//*Compare[0]*//*Compare[0]*/
        {
            /// <summary>
            /// 跳过数据指针
            /// </summary>
            public /*Type[0]*/ulong/*Type[0]*/* SkipCount;

            /// <summary>
            /// 最后一条记录指针-1
            /// </summary>
            public /*Type[0]*/ulong/*Type[0]*/* GetEndIndex;

            /// <summary>
            /// 范围排序
            /// </summary>
            /// <param name="startIndex">起始指针</param>
            /// <param name="endIndex">结束指针-1</param>
            public void Sort(/*Type[0]*/ulong/*Type[0]*/* startIndex, /*Type[0]*/ulong/*Type[0]*/* endIndex)
            {
                do
                {
                    /*Type[0]*/
                    ulong/*Type[0]*/ leftValue = *startIndex, rightValue = *endIndex;
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
                    /*Type[0]*/
                    ulong/*Type[0]*/* averageIndex = startIndex + average, leftIndex = startIndex, rightIndex = endIndex;
                    /*Type[0]*/
                    ulong/*Type[0]*/ value = *averageIndex;
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
                        if (startIndex < rightIndex && rightIndex >= SkipCount) Sort(startIndex, rightIndex);
                        if (leftIndex > GetEndIndex) break;
                        startIndex = leftIndex;
                    }
                    else
                    {
                        if (leftIndex < endIndex && leftIndex <= GetEndIndex) Sort(leftIndex, endIndex);
                        if (rightIndex < SkipCount) break;
                        endIndex = rightIndex;
                    }
                }
                while (startIndex < endIndex);
            }
        }

        /// <summary>
        /// 范围排序
        /// </summary>
        /// <param name="values">待排序数组</param>
        /// <param name="skipCount">跳过数据数量</param>
        /// <param name="getCount">排序数据数量</param>
        /// <returns>排序后的数据</returns>
        public unsafe static TmphSubArray</*Type[0]*/ulong/*Type[0]*/> RangeSort/*Compare[0]*//*Compare[0]*/
            (/*Type[0]*/ulong/*Type[0]*/[] values, int skipCount, int getCount)
        {
            TmphArray.TmphRange range = new TmphArray.TmphRange(values.length(), skipCount, getCount);
            if ((getCount = range.GetCount) != 0)
            {
                fixed (/*Type[0]*/ulong/*Type[0]*/* valueFixed = values)
                {
                    new /*Type[1]*/ulongRangeSorter/*Type[1]*//*Compare[0]*//*Compare[0]*/
                    {
                        SkipCount = valueFixed + range.SkipCount,
                        GetEndIndex = valueFixed + range.EndIndex - 1
                    }.Sort(valueFixed, valueFixed + values.Length - 1);
                }
                return TmphSubArray</*Type[0]*/ulong/*Type[0]*/>.Unsafe(values, range.SkipCount, getCount);
            }
            return default(TmphSubArray</*Type[0]*/ulong/*Type[0]*/>);
        }

        /// <summary>
        /// 范围排序
        /// </summary>
        /// <param name="values">待排序数组</param>
        /// <param name="skipCount">跳过数据数量</param>
        /// <param name="getCount">排序数据数量</param>
        /// <returns>排序后的新数据</returns>
        public unsafe static TmphSubArray</*Type[0]*/ulong/*Type[0]*/> GetRangeSort/*Compare[0]*//*Compare[0]*/
            (/*Type[0]*/ulong/*Type[0]*/[] values, int skipCount, int getCount)
        {
            TmphArray.TmphRange range = new TmphArray.TmphRange(values.length(), skipCount, getCount);
            if ((getCount = range.GetCount) != 0)
            {
                /*Type[0]*/
                ulong/*Type[0]*/[] newValues = new /*Type[0]*/ulong/*Type[0]*/[values.Length];
                Buffer.BlockCopy(values, 0, newValues, 0, values.Length * sizeof(/*Type[0]*/ulong/*Type[0]*/));
                fixed (/*Type[0]*/ulong/*Type[0]*/* newValueFixed = newValues, valueFixed = values)
                {
                    new /*Type[1]*/ulongRangeSorter/*Type[1]*//*Compare[0]*//*Compare[0]*/
                    {
                        SkipCount = newValueFixed + range.SkipCount,
                        GetEndIndex = newValueFixed + range.EndIndex - 1
                    }.Sort(newValueFixed, newValueFixed + values.Length - 1);
                }
                return TmphSubArray</*Type[0]*/ulong/*Type[0]*/>.Unsafe(newValues, range.SkipCount, getCount);
            }
            return default(TmphSubArray</*Type[0]*/ulong/*Type[0]*/>);
        }

        /// <summary>
        /// 范围排序
        /// </summary>
        /// <param name="values">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">排序范围数据数量</param>
        /// <param name="skipCount">跳过数据数量</param>
        /// <param name="getCount">排序数据数量</param>
        /// <returns>排序后的数据</returns>
        public unsafe static TmphSubArray</*Type[0]*/ulong/*Type[0]*/> RangeSort/*Compare[0]*//*Compare[0]*/
            (/*Type[0]*/ulong/*Type[0]*/[] values, int startIndex, int count, int skipCount, int getCount)
        {
            TmphArray.TmphRange range = new TmphArray.TmphRange(values.length(), startIndex, count);
            if ((count = range.GetCount) != 0)
            {
                TmphArray.TmphRange getRange = new TmphArray.TmphRange(count, skipCount, getCount);
                if ((getCount = getRange.GetCount) != 0)
                {
                    skipCount = range.SkipCount + getRange.SkipCount;
                    fixed (/*Type[0]*/ulong/*Type[0]*/* valueFixed = values)
                    {
                        /*Type[0]*/
                        ulong/*Type[0]*/* skip = valueFixed + skipCount, start = valueFixed + range.SkipCount;
                        new /*Type[1]*/ulongRangeSorter/*Type[1]*//*Compare[0]*//*Compare[0]*/
                        {
                            SkipCount = skip,
                            GetEndIndex = skip + getCount - 1
                        }.Sort(start, start + --count);
                    }
                    return TmphSubArray</*Type[0]*/ulong/*Type[0]*/>.Unsafe(values, skipCount, getCount);
                }
            }
            return default(TmphSubArray</*Type[0]*/ulong/*Type[0]*/>);
        }

        /// <summary>
        /// 范围排序
        /// </summary>
        /// <param name="values">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">排序范围数据数量</param>
        /// <param name="skipCount">跳过数据数量</param>
        /// <param name="getCount">排序数据数量</param>
        /// <returns>排序后的新数据</returns>
        public unsafe static TmphSubArray</*Type[0]*/ulong/*Type[0]*/> GetRangeSort/*Compare[0]*//*Compare[0]*/
            (/*Type[0]*/ulong/*Type[0]*/[] values, int startIndex, int count, int skipCount, int getCount)
        {
            TmphArray.TmphRange range = new TmphArray.TmphRange(values.length(), startIndex, count);
            if ((count = range.GetCount) != 0)
            {
                TmphArray.TmphRange getRange = new TmphArray.TmphRange(count, skipCount, getCount);
                if ((getCount = getRange.GetCount) != 0)
                {
                    /*Type[0]*/
                    ulong/*Type[0]*/[] newValues = new /*Type[0]*/ulong/*Type[0]*/[count];
                    Buffer.BlockCopy(values, range.SkipCount * sizeof(/*Type[0]*/ulong/*Type[0]*/), newValues, 0, count * sizeof(/*Type[0]*/ulong/*Type[0]*/));
                    fixed (/*Type[0]*/ulong/*Type[0]*/* newValueFixed = newValues, valueFixed = values)
                    {
                        new /*Type[1]*/ulongRangeSorter/*Type[1]*//*Compare[0]*//*Compare[0]*/
                        {
                            SkipCount = newValueFixed + getRange.SkipCount,
                            GetEndIndex = newValueFixed + getRange.SkipCount + getCount - 1
                        }.Sort(newValueFixed, newValueFixed + count - 1);
                    }
                    return TmphSubArray</*Type[0]*/ulong/*Type[0]*/>.Unsafe(newValues, getRange.SkipCount, getCount);
                }
            }
            return default(TmphSubArray</*Type[0]*/ulong/*Type[0]*/>);
        }

        /// <summary>
        /// 索引范围排序器
        /// </summary>
        public unsafe struct /*Type[3]*/ulongRangeIndexSorter/*Type[3]*//*Compare[0]*//*Compare[0]*/
        {
            /// <summary>
            /// 跳过数据指针
            /// </summary>
            public /*Type[2]*/ulongSortIndex/*Type[2]*/* SkipCount;

            /// <summary>
            /// 最后一条记录指针-1
            /// </summary>
            public /*Type[2]*/ulongSortIndex/*Type[2]*/* GetEndIndex;

            /// <summary>
            /// 范围排序
            /// </summary>
            /// <param name="startIndex">起始指针</param>
            /// <param name="endIndex">结束指针-1</param>
            public void Sort(/*Type[2]*/ulongSortIndex/*Type[2]*/* startIndex, /*Type[2]*/ulongSortIndex/*Type[2]*/* endIndex)
            {
                do
                {
                    /*Type[2]*/
                    ulongSortIndex/*Type[2]*/ leftValue = *startIndex, rightValue = *endIndex;
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
                    /*Type[2]*/
                    ulongSortIndex/*Type[2]*/* averageIndex = startIndex + average, leftIndex = startIndex, rightIndex = endIndex;
                    /*Type[2]*/
                    ulongSortIndex/*Type[2]*/ indexValue = *averageIndex;
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
                        if (startIndex < rightIndex && rightIndex >= SkipCount) Sort(startIndex, rightIndex);
                        if (leftIndex > GetEndIndex) break;
                        startIndex = leftIndex;
                    }
                    else
                    {
                        if (leftIndex < endIndex && leftIndex <= GetEndIndex) Sort(leftIndex, endIndex);
                        if (rightIndex < SkipCount) break;
                        endIndex = rightIndex;
                    }
                }
                while (startIndex < endIndex);
            }
        }

        /// <summary>
        /// 数组范围排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="values">待排序数组</param>
        /// <param name="getKey">排序键值获取器</param>
        /// <param name="skipCount">跳过数据数量</param>
        /// <param name="getCount">排序数据数量</param>
        /// <returns>排序后的数组</returns>
        public unsafe static TValueType[] GetRangeSort/*Compare[0]*//*Compare[0]*/<TValueType>
            (TValueType[] values, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getKey, int skipCount, int getCount)
        {
            TmphArray.TmphRange range = new TmphArray.TmphRange(values.length(), skipCount, getCount);
            if ((getCount = range.GetCount) != 0)
            {
                if (getKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                TmphUnmanagedPool pool = CLB.TmphUnmanagedPool.GetDefaultPool(values.Length * sizeof(/*Type[2]*/ulongSortIndex/*Type[2]*/));
                TmphPointer data = pool.Get(values.Length * sizeof(/*Type[2]*/ulongSortIndex/*Type[2]*/));
                try
                {
                    return getRangeSort/*Compare[0]*//*Compare[0]*/(values, getKey, range.SkipCount, getCount, (/*Type[2]*/ulongSortIndex/*Type[2]*/*)data.Data);
                }
                finally { pool.Push(ref data); }
            }
            return TmphNullValue<TValueType>.Array;
        }

        /// <summary>
        /// 数组范围排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="values">待排序数组</param>
        /// <param name="getKey">排序键值获取器</param>
        /// <param name="skipCount">跳过数据数量</param>
        /// <param name="getCount">排序数据数量</param>
        /// <param name="fixedIndex">索引位置</param>
        /// <returns>排序后的数组</returns>
        private unsafe static TValueType[] getRangeSort/*Compare[0]*//*Compare[0]*/<TValueType>
            (TValueType[] values, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getKey, int skipCount, int getCount
            , /*Type[2]*/ulongSortIndex/*Type[2]*/* fixedIndex)
        {
            /*Type[2]*/
            ulongSortIndex/*Type[2]*/* writeIndex = fixedIndex;
            for (int index = 0; index != values.Length; (*writeIndex++).Set(getKey(values[index]), index++)) ;
            return getRangeSort/*Compare[0]*//*Compare[0]*/(values, skipCount, getCount, fixedIndex);
        }

        /// <summary>
        /// 数组范围排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="values">待排序数组</param>
        /// <param name="skipCount">跳过数据数量</param>
        /// <param name="getCount">排序数据数量</param>
        /// <param name="fixedIndex">索引位置</param>
        /// <returns>排序后的数组</returns>
        private unsafe static TValueType[] getRangeSort/*Compare[0]*//*Compare[0]*/<TValueType>
            (TValueType[] values, int skipCount, int getCount, /*Type[2]*/ulongSortIndex/*Type[2]*/* fixedIndex)
        {
            new /*Type[3]*/ulongRangeIndexSorter/*Type[3]*//*Compare[0]*//*Compare[0]*/
            {
                SkipCount = fixedIndex + skipCount,
                GetEndIndex = fixedIndex + skipCount + getCount - 1
            }.Sort(fixedIndex, fixedIndex + values.Length - 1);
            TValueType[] newValues = new TValueType[getCount];
            /*Type[2]*/
            ulongSortIndex/*Type[2]*/* writeIndex = fixedIndex + skipCount;
            for (int index = 0; index != newValues.Length; ++index) newValues[index] = values[(*writeIndex++).Index];
            return newValues;
        }

        /// <summary>
        /// 数组范围排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="values">待排序数组</param>
        /// <param name="indexs">排序索引</param>
        /// <param name="skipCount">跳过数据数量</param>
        /// <param name="getCount">排序数据数量</param>
        /// <returns>排序后的数组</returns>
        public unsafe static TValueType[] GetRangeSort/*Compare[0]*//*Compare[0]*/<TValueType>
            (TValueType[] values, /*Type[2]*/ulongSortIndex/*Type[2]*/[] indexs, int skipCount, int getCount)
        {
            if (values.length() != indexs.length()) TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            TmphArray.TmphRange range = new TmphArray.TmphRange(values.length(), skipCount, getCount);
            if ((getCount = range.GetCount) != 0)
            {
                fixed (/*Type[2]*/ulongSortIndex/*Type[2]*/* fixedIndex = indexs) return getRangeSort/*Compare[0]*//*Compare[0]*/(values, skipCount, getCount, fixedIndex);
            }
            return TmphNullValue<TValueType>.Array;
        }

        /// <summary>
        /// 数组范围排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="values">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">排序范围数据数量</param>
        /// <param name="getKey">排序键值获取器</param>
        /// <param name="skipCount">跳过数据数量</param>
        /// <param name="getCount">排序数据数量</param>
        /// <returns>排序后的数组</returns>
        public unsafe static TValueType[] GetRangeSort/*Compare[0]*//*Compare[0]*/<TValueType>
            (TValueType[] values, int startIndex, int count, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getKey, int skipCount, int getCount)
        {
            TmphArray.TmphRange range = new TmphArray.TmphRange(values.length(), startIndex, count);
            if ((count = range.GetCount) != 0)
            {
                TmphArray.TmphRange getRange = new TmphArray.TmphRange(count, skipCount, getCount);
                if ((getCount = getRange.GetCount) != 0)
                {
                    if (getKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                    TmphUnmanagedPool pool = CLB.TmphUnmanagedPool.GetDefaultPool(count * sizeof(/*Type[2]*/ulongSortIndex/*Type[2]*/));
                    TmphPointer data = pool.Get(count * sizeof(/*Type[2]*/ulongSortIndex/*Type[2]*/));
                    try
                    {
                        return getRangeSort/*Compare[0]*//*Compare[0]*/
                            (values, range.SkipCount, count, getKey, getRange.SkipCount, getCount, (/*Type[2]*/ulongSortIndex/*Type[2]*/*)data.Data);
                    }
                    finally { pool.Push(ref data); }
                }
            }
            return TmphNullValue<TValueType>.Array;
        }

        /// <summary>
        /// 数组范围排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="values">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">排序范围数据数量</param>
        /// <param name="getKey">排序键值获取器</param>
        /// <param name="skipCount">跳过数据数量</param>
        /// <param name="getCount">排序数据数量</param>
        /// <param name="fixedIndex">索引位置</param>
        /// <returns>排序后的数组</returns>
        private unsafe static TValueType[] getRangeSort/*Compare[0]*//*Compare[0]*/<TValueType>
            (TValueType[] values, int startIndex, int count, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getKey, int skipCount, int getCount
            , /*Type[2]*/ulongSortIndex/*Type[2]*/* fixedIndex)
        {
            /*Type[2]*/
            ulongSortIndex/*Type[2]*/* writeIndex = fixedIndex;
            for (int index = startIndex, endIndex = startIndex + count; index != endIndex; (*writeIndex++).Set(getKey(values[index]), index++)) ;
            new /*Type[3]*/ulongRangeIndexSorter/*Type[3]*//*Compare[0]*//*Compare[0]*/
            {
                SkipCount = fixedIndex + skipCount,
                GetEndIndex = fixedIndex + skipCount + getCount - 1
            }.Sort(fixedIndex, fixedIndex + count - 1);
            TValueType[] newValues = new TValueType[getCount];
            writeIndex = fixedIndex + skipCount;
            for (int index = 0; index != newValues.Length; ++index) newValues[index] = values[(*writeIndex++).Index];
            return newValues;
        }
    }
}

namespace Laurent.Lee.CLB
{
    /// <summary>
    /// 数组扩展操作
    /// </summary>
    public static partial class TmphArrayExtension
    {
        /// <summary>
        /// 范围排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <param name="skipCount">跳过记录数</param>
        /// <param name="getCount">获取记录数,小于0表示所有</param>
        /// <returns>排序范围数组</returns>
        public static TmphSubArray</*Type[0]*/ulong/*Type[0]*/> rangeSort/*Compare[0]*//*Compare[0]*/
            (this /*Type[0]*/ulong/*Type[0]*/[] array, int skipCount, int getCount)
        {
            return Laurent.Lee.CLB.Algorithm.TmphQuickSort.RangeSort/*Compare[0]*//*Compare[0]*/(array, skipCount, getCount);
        }

        /// <summary>
        /// 范围排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <param name="skipCount">跳过记录数</param>
        /// <param name="getCount">获取记录数,小于0表示所有</param>
        /// <returns>排序范围数组</returns>
        public static TmphSubArray</*Type[0]*/ulong/*Type[0]*/> getRangeSort/*Compare[0]*//*Compare[0]*/
            (this /*Type[0]*/ulong/*Type[0]*/[] array, int skipCount, int getCount)
        {
            return Laurent.Lee.CLB.Algorithm.TmphQuickSort.GetRangeSort/*Compare[0]*//*Compare[0]*/(array, skipCount, getCount);
        }

        /// <summary>
        /// 范围排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">待排序数组</param>
        /// <param name="getKey">排序键</param>
        /// <param name="skipCount">跳过记录数</param>
        /// <param name="getCount">获取记录数,小于0表示所有</param>
        /// <returns>排序范围数组</returns>
        public static TValueType[] getRangeSort/*Compare[0]*//*Compare[0]*/<TValueType>
            (this TValueType[] array, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getKey, int skipCount, int getCount)
        {
            return Laurent.Lee.CLB.Algorithm.TmphQuickSort.GetRangeSort/*Compare[0]*//*Compare[0]*/(array, getKey, skipCount, getCount);
        }

        /// <summary>
        /// 范围排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">结束位置</param>
        /// <param name="skipCount">跳过记录数</param>
        /// <param name="getCount">获取记录数</param>
        /// <returns>排序范围数组</returns>
        public static TmphSubArray</*Type[0]*/ulong/*Type[0]*/> rangeSort/*Compare[0]*//*Compare[0]*/
            (this /*Type[0]*/ulong/*Type[0]*/[] array, int startIndex, int count, int skipCount, int getCount)
        {
            return Laurent.Lee.CLB.Algorithm.TmphQuickSort.RangeSort/*Compare[0]*//*Compare[0]*/(array, startIndex, count, skipCount, getCount);
        }

        /// <summary>
        /// 范围排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">结束位置</param>
        /// <param name="skipCount">跳过记录数</param>
        /// <param name="getCount">获取记录数</param>
        /// <returns>排序范围数组</returns>
        public static TmphSubArray</*Type[0]*/ulong/*Type[0]*/> getRangeSort/*Compare[0]*//*Compare[0]*/
            (this /*Type[0]*/ulong/*Type[0]*/[] array, int startIndex, int count, int skipCount, int getCount)
        {
            return Laurent.Lee.CLB.Algorithm.TmphQuickSort.GetRangeSort/*Compare[0]*//*Compare[0]*/(array, startIndex, count, skipCount, getCount);
        }

        /// <summary>
        /// 范围排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">结束位置</param>
        /// <param name="getKey">排序键</param>
        /// <param name="skipCount">跳过记录数</param>
        /// <param name="getCount">获取记录数</param>
        /// <returns>排序范围数组</returns>
        public static TValueType[] getRangeSort/*Compare[0]*//*Compare[0]*/<TValueType>
            (this TValueType[] array, int startIndex, int count, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getKey, int skipCount, int getCount)
        {
            return Laurent.Lee.CLB.Algorithm.TmphQuickSort.GetRangeSort/*Compare[0]*//*Compare[0]*/(array, startIndex, count, getKey, skipCount, getCount);
        }

        /// <summary>
        /// 分页排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <param name="pageSize">分页尺寸</param>
        /// <param name="currentPage">页号</param>
        /// <returns>分页排序数据</returns>
        public static TmphSubArray</*Type[0]*/ulong/*Type[0]*/> pageSort/*Compare[0]*//*Compare[0]*/
            (this /*Type[0]*/ulong/*Type[0]*/[] array, int pageSize, int currentPage)
        {
            TmphArray.TmphPage page = new TmphArray.TmphPage(array.length(), pageSize, currentPage);
            return Laurent.Lee.CLB.Algorithm.TmphQuickSort.RangeSort/*Compare[0]*//*Compare[0]*/(array, page.SkipCount, page.CurrentPageSize);
        }

        /// <summary>
        /// 分页排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <param name="pageSize">分页尺寸</param>
        /// <param name="currentPage">页号</param>
        /// <returns>分页排序数据</returns>
        public static TValueType[] getPageSort/*Compare[0]*//*Compare[0]*/<TValueType>
            (this TValueType[] array, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getKey, int pageSize, int currentPage)
        {
            TmphArray.TmphPage page = new TmphArray.TmphPage(array.length(), pageSize, currentPage);
            return Laurent.Lee.CLB.Algorithm.TmphQuickSort.GetRangeSort/*Compare[0]*//*Compare[0]*/(array, getKey, page.SkipCount, page.CurrentPageSize);
        }

        /// <summary>
        /// 分页排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <param name="pageSize">分页尺寸</param>
        /// <param name="currentPage">页号</param>
        /// <returns>分页排序数据</returns>
        public static TmphSubArray</*Type[0]*/ulong/*Type[0]*/> getPageSort/*Compare[0]*//*Compare[0]*/
            (this /*Type[0]*/ulong/*Type[0]*/[] array, int pageSize, int currentPage)
        {
            TmphArray.TmphPage page = new TmphArray.TmphPage(array.length(), pageSize, currentPage);
            return Laurent.Lee.CLB.Algorithm.TmphQuickSort.GetRangeSort/*Compare[0]*//*Compare[0]*/(array, page.SkipCount, page.CurrentPageSize);
        }
    }

    /// <summary>
    /// 数组子串扩展
    /// </summary>
    public static partial class TmphSubArrayExtension
    {
        /// <summary>
        /// 范围排序
        /// </summary>
        /// <param name="array">数组子串</param>
        /// <param name="skipCount">跳过记录数</param>
        /// <param name="getCount">获取记录数,小于0表示所有</param>
        /// <returns>排序范围数组</returns>
        public static TmphSubArray</*Type[0]*/ulong/*Type[0]*/> rangeSort/*Compare[0]*//*Compare[0]*/
            (this TmphSubArray</*Type[0]*/ulong/*Type[0]*/> array, int skipCount, int getCount)
        {
            if (array.Count != 0)
            {
                return Laurent.Lee.CLB.Algorithm.TmphQuickSort.RangeSort/*Compare[0]*//*Compare[0]*/(array.Array, array.StartIndex, array.Count, skipCount, getCount);
            }
            return default(TmphSubArray</*Type[0]*/ulong/*Type[0]*/>);
        }

        /// <summary>
        /// 范围排序
        /// </summary>
        /// <param name="array">数组子串</param>
        /// <param name="skipCount">跳过记录数</param>
        /// <param name="getCount">获取记录数,小于0表示所有</param>
        /// <returns>排序范围数组</returns>
        public static TmphSubArray</*Type[0]*/ulong/*Type[0]*/> getRangeSort/*Compare[0]*//*Compare[0]*/
            (this TmphSubArray</*Type[0]*/ulong/*Type[0]*/> array, int skipCount, int getCount)
        {
            if (array.Count != 0)
            {
                return Laurent.Lee.CLB.Algorithm.TmphQuickSort.GetRangeSort/*Compare[0]*//*Compare[0]*/(array.Array, array.StartIndex, array.Count, skipCount, getCount);
            }
            return default(TmphSubArray</*Type[0]*/ulong/*Type[0]*/>);
        }

        /// <summary>
        /// 范围排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组子串</param>
        /// <param name="getKey">排序键</param>
        /// <param name="skipCount">跳过记录数</param>
        /// <param name="getCount">获取记录数,小于0表示所有</param>
        /// <returns>排序范围数组</returns>
        public static TValueType[] getRangeSort/*Compare[0]*//*Compare[0]*/<TValueType>
            (this TmphSubArray<TValueType> array, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getKey, int skipCount, int getCount)
        {
            if (array.Count != 0)
            {
                return Laurent.Lee.CLB.Algorithm.TmphQuickSort.GetRangeSort/*Compare[0]*//*Compare[0]*/
                    (array.Array, array.StartIndex, array.Count, getKey, skipCount, getCount);
            }
            return TmphNullValue<TValueType>.Array;
        }

        /// <summary>
        /// 分页排序
        /// </summary>
        /// <param name="array">数组子串</param>
        /// <param name="pageSize">分页尺寸</param>
        /// <param name="currentPage">页号</param>
        /// <returns>分页排序数据</returns>
        public static TmphSubArray</*Type[0]*/ulong/*Type[0]*/> pageSort/*Compare[0]*//*Compare[0]*/
            (this TmphSubArray</*Type[0]*/ulong/*Type[0]*/> array, int pageSize, int currentPage)
        {
            TmphArray.TmphPage page = new TmphArray.TmphPage(array.Count, pageSize, currentPage);
            int count = page.CurrentPageSize;
            if (count != 0)
            {
                return Laurent.Lee.CLB.Algorithm.TmphQuickSort.RangeSort/*Compare[0]*//*Compare[0]*/(array.Array, array.StartIndex, array.Count, page.SkipCount, count);
            }
            return default(TmphSubArray</*Type[0]*/ulong/*Type[0]*/>);
        }

        /// <summary>
        /// 分页排序
        /// </summary>
        /// <param name="array">数组子串</param>
        /// <param name="pageSize">分页尺寸</param>
        /// <param name="currentPage">页号</param>
        /// <returns>分页排序数据</returns>
        public static TmphSubArray</*Type[0]*/ulong/*Type[0]*/> getPageSort/*Compare[0]*//*Compare[0]*/
            (this TmphSubArray</*Type[0]*/ulong/*Type[0]*/> array, int pageSize, int currentPage)
        {
            TmphArray.TmphPage page = new TmphArray.TmphPage(array.Count, pageSize, currentPage);
            int count = page.CurrentPageSize;
            if (count != 0)
            {
                return Laurent.Lee.CLB.Algorithm.TmphQuickSort.GetRangeSort/*Compare[0]*//*Compare[0]*/(array.Array, array.StartIndex, array.Count, page.SkipCount, count);
            }
            return default(TmphSubArray</*Type[0]*/ulong/*Type[0]*/>);
        }
    }
}