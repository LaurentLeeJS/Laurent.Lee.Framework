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

namespace Laurent.Lee.CLB.Algorithm
{
    internal static class TmphQuickSort
    {
        public static void Sort<TValueType>(TValueType[] values, Func<TValueType, TValueType, int> comparer)
        {
            if (values != null && values.Length > 1)
            {
                if (comparer == null)
                    TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                new TmphSorter<TValueType> { Array = values, Comparer = comparer }.Sort(0, values.Length - 1);
            }
        }

        public static TValueType[] GetSort<TValueType>(TValueType[] values, Func<TValueType, TValueType, int> comparer)
        {
            if (values.Length != 0)
            {
                var sorter = new TmphSorter<TValueType> { Array = values.copy(), Comparer = comparer };
                if (values.Length > 1)
                {
                    if (comparer == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                    sorter.Sort(0, values.Length - 1);
                }
                return sorter.Array;
            }
            return values.notNull();
        }

        public static void Sort<TValueType>(TValueType[] values, Func<TValueType, TValueType, int> comparer,
            int startIndex, int count)
        {
            var range = new TmphArray.TmphRange(values.length(), startIndex, count);
            if (range.GetCount > 1)
            {
                if (comparer == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                new TmphSorter<TValueType> { Array = values, Comparer = comparer }.Sort(range.SkipCount, range.EndIndex - 1);
            }
        }

        public static TValueType[] GetSort<TValueType>(TValueType[] values, Func<TValueType, TValueType, int> comparer,
            int startIndex, int count)
        {
            var range = new TmphArray.TmphRange(values.length(), startIndex, count);
            if ((count = range.GetCount) != 0)
            {
                var newValues = new TValueType[count];
                Array.Copy(values, range.SkipCount, newValues, 0, count);
                if (--count > 0)
                {
                    if (comparer == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                    new TmphSorter<TValueType> { Array = newValues, Comparer = comparer }.Sort(0, count);
                }
                return newValues;
            }
            return values.notNull();
        }

        public static TmphSubArray<TValueType> RangeSort<TValueType>
            (TValueType[] values, Func<TValueType, TValueType, int> comparer, int skipCount, int getCount)
        {
            var range = new TmphArray.TmphRange(values.length(), skipCount, getCount);
            if ((getCount = range.GetCount) != 0)
            {
                if (comparer == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                new TmphRangeSorter<TValueType>
                {
                    Array = values,
                    Comparer = comparer,
                    SkipCount = range.SkipCount,
                    GetEndIndex = range.EndIndex - 1
                }.Sort(0, values.Length - 1);
                return TmphSubArray<TValueType>.Unsafe(values, range.SkipCount, getCount);
            }
            return default(TmphSubArray<TValueType>);
        }

        public static TmphSubArray<TValueType> GetRangeSort<TValueType>
            (TValueType[] values, Func<TValueType, TValueType, int> comparer, int skipCount, int getCount)
        {
            var range = new TmphArray.TmphRange(values.length(), skipCount, getCount);
            if ((getCount = range.GetCount) != 0)
            {
                if (comparer == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                var sorter = new TmphRangeSorter<TValueType>
                {
                    Array = values.copy(),
                    Comparer = comparer,
                    SkipCount = range.SkipCount,
                    GetEndIndex = range.EndIndex - 1
                };
                sorter.Sort(0, values.Length - 1);
                return TmphSubArray<TValueType>.Unsafe(sorter.Array, range.SkipCount, getCount);
            }
            return default(TmphSubArray<TValueType>);
        }

        public static TmphSubArray<TValueType> RangeSort<TValueType>
            (TValueType[] values, int startIndex, int count, Func<TValueType, TValueType, int> comparer, int skipCount,
                int getCount)
        {
            var range = new TmphArray.TmphRange(values.length(), startIndex, count);
            if ((count = range.GetCount) != 0)
            {
                var getRange = new TmphArray.TmphRange(count, skipCount, getCount);
                if ((getCount = getRange.GetCount) != 0)
                {
                    if (comparer == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                    skipCount = range.SkipCount + getRange.SkipCount;
                    new TmphRangeSorter<TValueType>
                    {
                        Array = values,
                        Comparer = comparer,
                        SkipCount = skipCount,
                        GetEndIndex = skipCount + getCount - 1
                    }.Sort(range.SkipCount, range.SkipCount + --count);
                    return TmphSubArray<TValueType>.Unsafe(values, skipCount, getCount);
                }
            }
            return default(TmphSubArray<TValueType>);
        }

        public static TmphSubArray<TValueType> GetRangeSort<TValueType>
            (TValueType[] values, int startIndex, int count, Func<TValueType, TValueType, int> comparer, int skipCount,
                int getCount)
        {
            var range = new TmphArray.TmphRange(values.length(), startIndex, count);
            if ((count = range.GetCount) != 0)
            {
                var getRange = new TmphArray.TmphRange(count, skipCount, getCount);
                if ((getCount = getRange.GetCount) != 0)
                {
                    var newValues = new TValueType[count];
                    Array.Copy(values, range.SkipCount, newValues, 0, count);
                    return RangeSort(newValues, comparer, getRange.SkipCount, getCount);
                }
            }
            return default(TmphSubArray<TValueType>);
        }

        public static TmphSubArray<TValueType> GetTop<TValueType>(TValueType[] values,
            Func<TValueType, TValueType, int> comparer, int count)
        {
            if (values == null) return default(TmphSubArray<TValueType>);
            if (comparer == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            if (count > 0)
            {
                if (count < values.Length)
                {
                    if (count <= values.Length >> 1) return GetTopN(values, comparer, count);
                    values = GetRemoveTopN(values, comparer, count);
                }
                else
                {
                    var newValues = new TValueType[values.Length];
                    Array.Copy(values, 0, newValues, 0, values.Length);
                    values = newValues;
                }
                return TmphSubArray<TValueType>.Unsafe(values, 0, values.Length);
            }
            return default(TmphSubArray<TValueType>);
        }

        public static TmphSubArray<TValueType> Top<TValueType>(TValueType[] values,
            Func<TValueType, TValueType, int> comparer, int count)
        {
            if (values == null) return default(TmphSubArray<TValueType>);
            if (comparer == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            if (count > 0)
            {
                if (count < values.Length)
                {
                    if (count <= values.Length >> 1) return GetTopN(values, comparer, count);
                    values = GetRemoveTopN(values, comparer, count);
                }
                return TmphSubArray<TValueType>.Unsafe(values, 0, values.Length);
            }
            return default(TmphSubArray<TValueType>);
        }

        private static TmphSubArray<TValueType> GetTopN<TValueType>(TValueType[] values,
            Func<TValueType, TValueType, int> comparer, int count)
        {
            uint sqrtMod;
            var length = Math.Min(Math.Max(count << 2, count + (int)((uint)values.Length).sqrt(out sqrtMod)),
                values.Length);
            var newValues = new TValueType[length];
            int readIndex = values.Length - length, writeIndex = count;
            Array.Copy(values, readIndex, newValues, 0, length);
            var sort = new TmphRangeSorter<TValueType>
            {
                Array = newValues,
                Comparer = comparer,
                SkipCount = count - 1,
                GetEndIndex = count - 1
            };
            sort.Sort(0, --length);
            for (var maxValue = newValues[sort.GetEndIndex]; readIndex != 0;)
            {
                if (comparer(values[--readIndex], maxValue) < 0)
                {
                    newValues[writeIndex] = values[readIndex];
                    if (writeIndex == length)
                    {
                        sort.Sort(0, length);
                        writeIndex = count;
                        maxValue = newValues[sort.GetEndIndex];
                    }
                    else ++writeIndex;
                }
            }
            if (writeIndex != count) sort.Sort(0, writeIndex - 1);
            Array.Clear(newValues, count, newValues.Length - count);
            return TmphSubArray<TValueType>.Unsafe(newValues, 0, count);
        }

        /// <summary>
        ///     排序去除Top N
        /// </summary>
        /// <typeparam name="TValueType">排序数据类型</typeparam>
        /// <param name="values">待排序数组</param>
        /// <param name="comparer">排序比较器</param>
        /// <param name="count">排序数据数量</param>
        /// <returns>排序后的数据</returns>
        private static TValueType[] GetRemoveTopN<TValueType>(TValueType[] values,
            Func<TValueType, TValueType, int> comparer, int count)
        {
            var newValues = new TValueType[count];
            count = values.Length - count;
            uint sqrtMod;
            var length = Math.Min(Math.Max(count << 2, count + (int)((uint)values.Length).sqrt(out sqrtMod)),
                values.Length);
            var removeValues = new TValueType[length];
            int readIndex = values.Length - length,
                copyCount = length - count,
                removeIndex = copyCount,
                writeIndex = copyCount;
            Array.Copy(values, readIndex, removeValues, 0, length);
            var sort
                = new TmphRangeSorter<TValueType>
                {
                    Array = removeValues,
                    Comparer = comparer,
                    SkipCount = copyCount,
                    GetEndIndex = copyCount
                };
            sort.Sort(0, --length);
            Array.Copy(removeValues, 0, newValues, 0, copyCount);
            for (var maxValue = removeValues[copyCount]; readIndex != 0;)
            {
                if (comparer(values[--readIndex], maxValue) <= 0) newValues[writeIndex++] = values[readIndex];
                else
                {
                    removeValues[--removeIndex] = values[readIndex];
                    if (removeIndex == 0)
                    {
                        sort.Sort(0, length);
                        removeIndex = copyCount;
                        maxValue = removeValues[copyCount];
                        Array.Copy(removeValues, 0, newValues, writeIndex, copyCount);
                        writeIndex += copyCount;
                    }
                }
            }
            if (removeIndex != copyCount)
            {
                sort.Sort(removeIndex, length);
                Array.Copy(removeValues, removeIndex, newValues, writeIndex, copyCount - removeIndex);
            }
            return newValues;
        }

        /// <summary>
        ///     排序器
        /// </summary>
        /// <typeparam name="TValueType">排序数据类型</typeparam>
        private struct TmphSorter<TValueType>
        {
            /// <summary>
            ///     待排序数组
            /// </summary>
            public TValueType[] Array;

            /// <summary>
            ///     排序比较器
            /// </summary>
            public Func<TValueType, TValueType, int> Comparer;

            /// <summary>
            ///     范围排序
            /// </summary>
            /// <param name="startIndex">起始位置</param>
            /// <param name="endIndex">结束位置-1</param>
            public void Sort(int startIndex, int endIndex)
            {
                do
                {
                    TValueType leftValue = Array[startIndex], rightValue = Array[endIndex];
                    int average = (endIndex - startIndex) >> 1;
                    if (average == 0)
                    {
                        if (Comparer(leftValue, rightValue) > 0)
                        {
                            Array[startIndex] = rightValue;
                            Array[endIndex] = leftValue;
                        }
                        break;
                    }
                    int leftIndex = startIndex, rightIndex = endIndex;
                    TValueType value = Array[average += startIndex];
                    if (Comparer(leftValue, value) <= 0)
                    {
                        if (Comparer(value, rightValue) > 0)
                        {
                            Array[rightIndex] = value;
                            if (Comparer(leftValue, rightValue) <= 0)
                                Array[average] = value = rightValue;
                            else
                            {
                                Array[leftIndex] = rightValue;
                                Array[average] = value = leftValue;
                            }
                        }
                    }
                    else if (Comparer(leftValue, rightValue) <= 0)
                    {
                        Array[leftIndex] = value;
                        Array[average] = value = leftValue;
                    }
                    else
                    {
                        Array[rightIndex] = leftValue;
                        if (Comparer(value, rightValue) <= 0)
                        {
                            Array[leftIndex] = value;
                            Array[average] = value = rightValue;
                        }
                        else
                            Array[leftIndex] = rightValue;
                    }
                    ++leftIndex;
                    --rightIndex;
                    do
                    {
                        while (Comparer(Array[leftIndex], value) < 0)
                            ++leftIndex;
                        while (Comparer(value, Array[rightIndex]) < 0)
                            --rightIndex;
                        if (leftIndex < rightIndex)
                        {
                            leftValue = Array[leftIndex];
                            Array[leftIndex] = Array[rightIndex];
                            Array[rightIndex] = leftValue;
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
                    } while (++leftIndex <= --rightIndex);
                    if (rightIndex - startIndex <= endIndex - leftIndex)
                    {
                        if (startIndex < rightIndex)
                            Sort(startIndex, rightIndex);
                        startIndex = leftIndex;
                    }
                    else
                    {
                        if (leftIndex < endIndex)
                            Sort(leftIndex, endIndex);
                        endIndex = rightIndex;
                    }
                } while (startIndex < endIndex);
            }
        }

        private struct TmphRangeSorter<TValueType>
        {
            public TValueType[] Array;
            public Func<TValueType, TValueType, int> Comparer;
            public int GetEndIndex;
            public int SkipCount;

            public void Sort(int startIndex, int endIndex)
            {
                do
                {
                    TValueType leftValue = Array[startIndex], rightValue = Array[endIndex];
                    var average = (endIndex - startIndex) >> 1;
                    if (average == 0)
                    {
                        if (Comparer(leftValue, rightValue) > 0)
                        {
                            Array[startIndex] = rightValue;
                            Array[endIndex] = leftValue;
                        }
                        break;
                    }
                    average += startIndex;
                    int leftIndex = startIndex, rightIndex = endIndex;
                    var value = Array[average];
                    if (Comparer(leftValue, value) <= 0)
                    {
                        if (Comparer(value, rightValue) > 0)
                        {
                            Array[rightIndex] = value;
                            if (Comparer(leftValue, rightValue) <= 0)
                                Array[average] = value = rightValue;
                            else
                            {
                                Array[leftIndex] = rightValue;
                                Array[average] = value = leftValue;
                            }
                        }
                    }
                    else if (Comparer(leftValue, rightValue) <= 0)
                    {
                        Array[leftIndex] = value;
                        Array[average] = value = leftValue;
                    }
                    else
                    {
                        Array[rightIndex] = leftValue;
                        if (Comparer(value, rightValue) <= 0)
                        {
                            Array[leftIndex] = value;
                            Array[average] = value = rightValue;
                        }
                        else Array[leftIndex] = rightValue;
                    }
                    ++leftIndex;
                    --rightIndex;
                    do
                    {
                        while (Comparer(Array[leftIndex], value) < 0)
                            ++leftIndex;
                        while (Comparer(value, Array[rightIndex]) < 0)
                            --rightIndex;
                        if (leftIndex < rightIndex)
                        {
                            leftValue = Array[leftIndex];
                            Array[leftIndex] = Array[rightIndex];
                            Array[rightIndex] = leftValue;
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
                    } while (++leftIndex <= --rightIndex);
                    if (rightIndex - startIndex <= endIndex - leftIndex)
                    {
                        if (startIndex < rightIndex && rightIndex >= SkipCount)
                            Sort(startIndex, rightIndex);
                        if (leftIndex > GetEndIndex)
                            break;
                        startIndex = leftIndex;
                    }
                    else
                    {
                        if (leftIndex < endIndex && leftIndex <= GetEndIndex)
                            Sort(leftIndex, endIndex);
                        if (rightIndex < SkipCount)
                            break;
                        endIndex = rightIndex;
                    }
                } while (startIndex < endIndex);
            }
        }
    }
}