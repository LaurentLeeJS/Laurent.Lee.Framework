using Laurent.Lee.CLB.Algorithm;
using System;

namespace Laurent.Lee.CLB
{
    public static partial class TmphSubArrayExtension
    {
        public static TmphSubArray<ulong> sort(this TmphSubArray<ulong> array)
        {
            if (array.Count > 1)
            {
                if (array.Count >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) Algorithm.TmphRadixSort.Sort(array.Array, array.StartIndex, array.Count);
                else Algorithm.TmphQuickSort.Sort(array.Array, array.StartIndex, array.Count);
            }
            return array;
        }

        public static TmphSubArray<ulong> sort(this TmphSubArray<ulong> array, int startIndex, int count)
        {
            if (array.Count != 0)
            {
                TmphArray.TmphRange range = new TmphArray.TmphRange(array.Count, startIndex, count);
                if (range.GetCount >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) Algorithm.TmphRadixSort.Sort(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
                else if (range.GetCount > 1) Algorithm.TmphQuickSort.Sort(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
                return TmphSubArray<ulong>.Unsafe(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
            }
            return array;
        }

        public static TmphSubArray<ulong> sortDesc(this TmphSubArray<ulong> array)
        {
            if (array.Count > 1)
            {
                if (array.Count >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) Algorithm.TmphRadixSort.SortDesc(array.Array, array.StartIndex, array.Count);
                else Algorithm.TmphQuickSort.SortDesc(array.Array, array.StartIndex, array.Count);
            }
            return array;
        }

        public static TmphSubArray<ulong> sortDesc(this TmphSubArray<ulong> array, int startIndex, int count)
        {
            if (array.Count != 0)
            {
                TmphArray.TmphRange range = new TmphArray.TmphRange(array.Count, startIndex, count);
                if (range.GetCount >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) Algorithm.TmphRadixSort.SortDesc(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
                else if (range.GetCount > 1) Algorithm.TmphQuickSort.SortDesc(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
                return TmphSubArray<ulong>.Unsafe(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
            }
            return array;
        }

        public static ulong[] getSort(this TmphSubArray<ulong> array)
        {
            if (array.Count > 1)
            {
                if (array.Count >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) return Algorithm.TmphRadixSort.GetSort(array.Array, array.StartIndex, array.Count);
                return Algorithm.TmphQuickSort.GetSort(array.Array, array.StartIndex, array.Count);
            }
            return array.GetArray();
        }

        public static ulong[] getSort(this TmphSubArray<ulong> array, int startIndex, int count)
        {
            if (array.Count != 0)
            {
                TmphArray.TmphRange range = new TmphArray.TmphRange(array.Count, startIndex, count);
                if (range.GetCount >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) return Algorithm.TmphRadixSort.GetSort(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
                return Algorithm.TmphQuickSort.GetSort(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
            }
            return TmphNullValue<ulong>.Array;
        }

        public static ulong[] getSortDesc(this TmphSubArray<ulong> array)
        {
            if (array.Count > 1)
            {
                if (array.Count >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) return Algorithm.TmphRadixSort.GetSortDesc(array.Array, array.StartIndex, array.Count);
                return Algorithm.TmphQuickSort.GetSortDesc(array.Array, array.StartIndex, array.Count);
            }
            return array.GetArray();
        }

        public static ulong[] getSortDesc(this TmphSubArray<ulong> array, int startIndex, int count)
        {
            if (array.Count != 0)
            {
                TmphArray.TmphRange range = new TmphArray.TmphRange(array.Count, startIndex, count);
                if (range.GetCount >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) return Algorithm.TmphRadixSort.GetSortDesc(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
                return Algorithm.TmphQuickSort.GetSortDesc(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
            }
            return TmphNullValue<ulong>.Array;
        }

        public static unsafe TValueType[] getSort<TValueType>(this TmphSubArray<TValueType> array, Func<TValueType, ulong> getKey)
        {
            if (getKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            if (array.Count > 1)
            {
                if (array.Count >= Laurent.Lee.CLB.TmphPub.RadixSortSize64)
                {
                    ulongSortIndex[] indexs = new ulongSortIndex[array.Count << 1];
                    fixed (ulongSortIndex* indexFixed = indexs)
                    {
                        ulongSortIndex.Create(indexFixed, array.Array, getKey, array.StartIndex, array.Count);
                        Algorithm.TmphRadixSort.Sort(indexFixed, indexFixed + array.Count, array.Count);
                        return ulongSortIndex.Create(indexFixed, array.Array, array.Count);
                    }
                }
                return Algorithm.TmphQuickSort.GetSort(array.Array, getKey, array.StartIndex, array.Count);
            }
            return array.GetArray();
        }

        public static unsafe TValueType[] getSort<TValueType>(this TmphSubArray<TValueType> array, Func<TValueType, ulong> getKey, int startIndex, int count)
        {
            if (getKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            if (array.Count != 0)
            {
                TmphArray.TmphRange range = new TmphArray.TmphRange(array.Count, startIndex, count);
                if (range.GetCount >= Laurent.Lee.CLB.TmphPub.RadixSortSize64)
                {
                    ulongSortIndex[] indexs = new ulongSortIndex[range.GetCount << 1];
                    fixed (ulongSortIndex* indexFixed = indexs)
                    {
                        ulongSortIndex.Create(indexFixed, array.Array, getKey, array.StartIndex + range.SkipCount, range.GetCount);
                        Algorithm.TmphRadixSort.Sort(indexFixed, indexFixed + range.GetCount, range.GetCount);
                        return ulongSortIndex.Create(indexFixed, array.Array, range.GetCount);
                    }
                }
                if (range.GetCount > 1) return Algorithm.TmphQuickSort.GetSort(array.Array, getKey, array.StartIndex + range.SkipCount, range.GetCount);
                if (range.GetCount != 0) return new TValueType[] { array.Array[array.StartIndex + range.SkipCount] };
            }
            return TmphNullValue<TValueType>.Array;
        }

        public static unsafe TValueType[] getSortDesc<TValueType>(this TmphSubArray<TValueType> array, Func<TValueType, ulong> getKey)
        {
            if (getKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            if (array.Count > 1)
            {
                if (array.Count >= Laurent.Lee.CLB.TmphPub.RadixSortSize64)
                {
                    ulongSortIndex[] indexs = new ulongSortIndex[array.Count << 1];
                    fixed (ulongSortIndex* indexFixed = indexs)
                    {
                        ulongSortIndex.Create(indexFixed, array.Array, getKey, array.StartIndex, array.Count);
                        Algorithm.TmphRadixSort.SortDesc(indexFixed, indexFixed + array.Count, array.Count);
                        return ulongSortIndex.Create(indexFixed, array.Array, array.Count);
                    }
                }
                return Algorithm.TmphQuickSort.GetSortDesc(array.Array, getKey, array.StartIndex, array.Count);
            }
            return array.GetArray();
        }

        public static unsafe TValueType[] getSortDesc<TValueType>(this TmphSubArray<TValueType> array, Func<TValueType, ulong> getKey, int startIndex, int count)
        {
            if (getKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            if (array.Count != 0)
            {
                TmphArray.TmphRange range = new TmphArray.TmphRange(array.Count, startIndex, count);
                if (range.GetCount >= Laurent.Lee.CLB.TmphPub.RadixSortSize64)
                {
                    ulongSortIndex[] indexs = new ulongSortIndex[range.GetCount << 1];
                    fixed (ulongSortIndex* indexFixed = indexs)
                    {
                        ulongSortIndex.Create(indexFixed, array.Array, getKey, array.StartIndex + range.SkipCount, range.GetCount);
                        Algorithm.TmphRadixSort.SortDesc(indexFixed, indexFixed + range.GetCount, range.GetCount);
                        return ulongSortIndex.Create(indexFixed, array.Array, range.GetCount);
                    }
                }
                if (range.GetCount > 1) return Algorithm.TmphQuickSort.GetSortDesc(array.Array, getKey, array.StartIndex + range.SkipCount, range.GetCount);
                if (range.GetCount != 0) return new TValueType[] { array.Array[array.StartIndex + range.SkipCount] };
            }
            return TmphNullValue<TValueType>.Array;
        }

        public static TmphSubArray<long> sort(this TmphSubArray<long> array)
        {
            if (array.Count > 1)
            {
                if (array.Count >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) Algorithm.TmphRadixSort.Sort(array.Array, array.StartIndex, array.Count);
                else Algorithm.TmphQuickSort.Sort(array.Array, array.StartIndex, array.Count);
            }
            return array;
        }

        public static TmphSubArray<long> sort(this TmphSubArray<long> array, int startIndex, int count)
        {
            if (array.Count != 0)
            {
                TmphArray.TmphRange range = new TmphArray.TmphRange(array.Count, startIndex, count);
                if (range.GetCount >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) Algorithm.TmphRadixSort.Sort(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
                else if (range.GetCount > 1) Algorithm.TmphQuickSort.Sort(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
                return TmphSubArray<long>.Unsafe(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
            }
            return array;
        }

        public static TmphSubArray<long> sortDesc(this TmphSubArray<long> array)
        {
            if (array.Count > 1)
            {
                if (array.Count >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) Algorithm.TmphRadixSort.SortDesc(array.Array, array.StartIndex, array.Count);
                else Algorithm.TmphQuickSort.SortDesc(array.Array, array.StartIndex, array.Count);
            }
            return array;
        }

        public static TmphSubArray<long> sortDesc(this TmphSubArray<long> array, int startIndex, int count)
        {
            if (array.Count != 0)
            {
                TmphArray.TmphRange range = new TmphArray.TmphRange(array.Count, startIndex, count);
                if (range.GetCount >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) Algorithm.TmphRadixSort.SortDesc(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
                else if (range.GetCount > 1) Algorithm.TmphQuickSort.SortDesc(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
                return TmphSubArray<long>.Unsafe(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
            }
            return array;
        }

        public static long[] getSort(this TmphSubArray<long> array)
        {
            if (array.Count > 1)
            {
                if (array.Count >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) return Algorithm.TmphRadixSort.GetSort(array.Array, array.StartIndex, array.Count);
                return Algorithm.TmphQuickSort.GetSort(array.Array, array.StartIndex, array.Count);
            }
            return array.GetArray();
        }

        public static long[] getSort(this TmphSubArray<long> array, int startIndex, int count)
        {
            if (array.Count != 0)
            {
                TmphArray.TmphRange range = new TmphArray.TmphRange(array.Count, startIndex, count);
                if (range.GetCount >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) return Algorithm.TmphRadixSort.GetSort(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
                return Algorithm.TmphQuickSort.GetSort(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
            }
            return TmphNullValue<long>.Array;
        }

        public static long[] getSortDesc(this TmphSubArray<long> array)
        {
            if (array.Count > 1)
            {
                if (array.Count >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) return Algorithm.TmphRadixSort.GetSortDesc(array.Array, array.StartIndex, array.Count);
                return Algorithm.TmphQuickSort.GetSortDesc(array.Array, array.StartIndex, array.Count);
            }
            return array.GetArray();
        }

        public static long[] getSortDesc(this TmphSubArray<long> array, int startIndex, int count)
        {
            if (array.Count != 0)
            {
                TmphArray.TmphRange range = new TmphArray.TmphRange(array.Count, startIndex, count);
                if (range.GetCount >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) return Algorithm.TmphRadixSort.GetSortDesc(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
                return Algorithm.TmphQuickSort.GetSortDesc(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
            }
            return TmphNullValue<long>.Array;
        }

        public static unsafe TValueType[] getSort<TValueType>(this TmphSubArray<TValueType> array, Func<TValueType, long> getKey)
        {
            if (getKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            if (array.Count > 1)
            {
                if (array.Count >= Laurent.Lee.CLB.TmphPub.RadixSortSize64)
                {
                    TmphLongSortIndex[] indexs = new TmphLongSortIndex[array.Count << 1];
                    fixed (TmphLongSortIndex* indexFixed = indexs)
                    {
                        TmphLongSortIndex.Create(indexFixed, array.Array, getKey, array.StartIndex, array.Count);
                        Algorithm.TmphRadixSort.Sort(indexFixed, (ulongSortIndex*)(indexFixed + array.Count), array.Count);
                        return TmphLongSortIndex.Create(indexFixed, array.Array, array.Count);
                    }
                }
                return Algorithm.TmphQuickSort.GetSort(array.Array, getKey, array.StartIndex, array.Count);
            }
            return array.GetArray();
        }

        public static unsafe TValueType[] getSort<TValueType>(this TmphSubArray<TValueType> array, Func<TValueType, long> getKey, int startIndex, int count)
        {
            if (getKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            if (array.Count != 0)
            {
                TmphArray.TmphRange range = new TmphArray.TmphRange(array.Count, startIndex, count);
                if (range.GetCount >= Laurent.Lee.CLB.TmphPub.RadixSortSize64)
                {
                    TmphLongSortIndex[] indexs = new TmphLongSortIndex[range.GetCount << 1];
                    fixed (TmphLongSortIndex* indexFixed = indexs)
                    {
                        TmphLongSortIndex.Create(indexFixed, array.Array, getKey, array.StartIndex + range.SkipCount, range.GetCount);
                        Algorithm.TmphRadixSort.Sort(indexFixed, (ulongSortIndex*)(indexFixed + range.GetCount), range.GetCount);
                        return TmphLongSortIndex.Create(indexFixed, array.Array, range.GetCount);
                    }
                }
                if (range.GetCount > 1) return Algorithm.TmphQuickSort.GetSort(array.Array, getKey, array.StartIndex + range.SkipCount, range.GetCount);
                if (range.GetCount != 0) return new TValueType[] { array.Array[array.StartIndex + range.SkipCount] };
            }
            return TmphNullValue<TValueType>.Array;
        }

        public static unsafe TValueType[] getSortDesc<TValueType>(this TmphSubArray<TValueType> array, Func<TValueType, long> getKey)
        {
            if (getKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            if (array.Count > 1)
            {
                if (array.Count >= Laurent.Lee.CLB.TmphPub.RadixSortSize64)
                {
                    TmphLongSortIndex[] indexs = new TmphLongSortIndex[array.Count << 1];
                    fixed (TmphLongSortIndex* indexFixed = indexs)
                    {
                        TmphLongSortIndex.Create(indexFixed, array.Array, getKey, array.StartIndex, array.Count);
                        Algorithm.TmphRadixSort.SortDesc(indexFixed, (ulongSortIndex*)(indexFixed + array.Count), array.Count);
                        return TmphLongSortIndex.Create(indexFixed, array.Array, array.Count);
                    }
                }
                return Algorithm.TmphQuickSort.GetSortDesc(array.Array, getKey, array.StartIndex, array.Count);
            }
            return array.GetArray();
        }

        public static unsafe TValueType[] getSortDesc<TValueType>(this TmphSubArray<TValueType> array, Func<TValueType, long> getKey, int startIndex, int count)
        {
            if (getKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            if (array.Count != 0)
            {
                TmphArray.TmphRange range = new TmphArray.TmphRange(array.Count, startIndex, count);
                if (range.GetCount >= Laurent.Lee.CLB.TmphPub.RadixSortSize64)
                {
                    TmphLongSortIndex[] indexs = new TmphLongSortIndex[range.GetCount << 1];
                    fixed (TmphLongSortIndex* indexFixed = indexs)
                    {
                        TmphLongSortIndex.Create(indexFixed, array.Array, getKey, array.StartIndex + range.SkipCount, range.GetCount);
                        Algorithm.TmphRadixSort.SortDesc(indexFixed, (ulongSortIndex*)(indexFixed + range.GetCount), range.GetCount);
                        return TmphLongSortIndex.Create(indexFixed, array.Array, range.GetCount);
                    }
                }
                if (range.GetCount > 1) return Algorithm.TmphQuickSort.GetSortDesc(array.Array, getKey, array.StartIndex + range.SkipCount, range.GetCount);
                if (range.GetCount != 0) return new TValueType[] { array.Array[array.StartIndex + range.SkipCount] };
            }
            return TmphNullValue<TValueType>.Array;
        }

        public static TmphSubArray<uint> sort(this TmphSubArray<uint> array)
        {
            if (array.Count > 1)
            {
                if (array.Count >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) Algorithm.TmphRadixSort.Sort(array.Array, array.StartIndex, array.Count);
                else Algorithm.TmphQuickSort.Sort(array.Array, array.StartIndex, array.Count);
            }
            return array;
        }

        public static TmphSubArray<uint> sort(this TmphSubArray<uint> array, int startIndex, int count)
        {
            if (array.Count != 0)
            {
                TmphArray.TmphRange range = new TmphArray.TmphRange(array.Count, startIndex, count);
                if (range.GetCount >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) Algorithm.TmphRadixSort.Sort(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
                else if (range.GetCount > 1) Algorithm.TmphQuickSort.Sort(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
                return TmphSubArray<uint>.Unsafe(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
            }
            return array;
        }

        public static TmphSubArray<uint> sortDesc(this TmphSubArray<uint> array)
        {
            if (array.Count > 1)
            {
                if (array.Count >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) Algorithm.TmphRadixSort.SortDesc(array.Array, array.StartIndex, array.Count);
                else Algorithm.TmphQuickSort.SortDesc(array.Array, array.StartIndex, array.Count);
            }
            return array;
        }

        public static TmphSubArray<uint> sortDesc(this TmphSubArray<uint> array, int startIndex, int count)
        {
            if (array.Count != 0)
            {
                TmphArray.TmphRange range = new TmphArray.TmphRange(array.Count, startIndex, count);
                if (range.GetCount >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) Algorithm.TmphRadixSort.SortDesc(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
                else if (range.GetCount > 1) Algorithm.TmphQuickSort.SortDesc(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
                return TmphSubArray<uint>.Unsafe(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
            }
            return array;
        }

        public static uint[] getSort(this TmphSubArray<uint> array)
        {
            if (array.Count > 1)
            {
                if (array.Count >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) return Algorithm.TmphRadixSort.GetSort(array.Array, array.StartIndex, array.Count);
                return Algorithm.TmphQuickSort.GetSort(array.Array, array.StartIndex, array.Count);
            }
            return array.GetArray();
        }

        public static uint[] getSort(this TmphSubArray<uint> array, int startIndex, int count)
        {
            if (array.Count != 0)
            {
                TmphArray.TmphRange range = new TmphArray.TmphRange(array.Count, startIndex, count);
                if (range.GetCount >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) return Algorithm.TmphRadixSort.GetSort(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
                return Algorithm.TmphQuickSort.GetSort(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
            }
            return TmphNullValue<uint>.Array;
        }

        public static uint[] getSortDesc(this TmphSubArray<uint> array)
        {
            if (array.Count > 1)
            {
                if (array.Count >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) return Algorithm.TmphRadixSort.GetSortDesc(array.Array, array.StartIndex, array.Count);
                return Algorithm.TmphQuickSort.GetSortDesc(array.Array, array.StartIndex, array.Count);
            }
            return array.GetArray();
        }

        public static uint[] getSortDesc(this TmphSubArray<uint> array, int startIndex, int count)
        {
            if (array.Count != 0)
            {
                TmphArray.TmphRange range = new TmphArray.TmphRange(array.Count, startIndex, count);
                if (range.GetCount >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) return Algorithm.TmphRadixSort.GetSortDesc(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
                return Algorithm.TmphQuickSort.GetSortDesc(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
            }
            return TmphNullValue<uint>.Array;
        }

        public static unsafe TValueType[] getSort<TValueType>(this TmphSubArray<TValueType> array, Func<TValueType, uint> getKey)
        {
            if (getKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            if (array.Count > 1)
            {
                if (array.Count >= Laurent.Lee.CLB.TmphPub.RadixSortSize64)
                {
                    TmphUintSortIndex[] indexs = new TmphUintSortIndex[array.Count << 1];
                    fixed (TmphUintSortIndex* indexFixed = indexs)
                    {
                        TmphUintSortIndex.Create(indexFixed, array.Array, getKey, array.StartIndex, array.Count);
                        Algorithm.TmphRadixSort.Sort(indexFixed, indexFixed + array.Count, array.Count);
                        return TmphUintSortIndex.Create(indexFixed, array.Array, array.Count);
                    }
                }
                return Algorithm.TmphQuickSort.GetSort(array.Array, getKey, array.StartIndex, array.Count);
            }
            return array.GetArray();
        }

        public static unsafe TValueType[] getSort<TValueType>(this TmphSubArray<TValueType> array, Func<TValueType, uint> getKey, int startIndex, int count)
        {
            if (getKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            if (array.Count != 0)
            {
                TmphArray.TmphRange range = new TmphArray.TmphRange(array.Count, startIndex, count);
                if (range.GetCount >= Laurent.Lee.CLB.TmphPub.RadixSortSize64)
                {
                    TmphUintSortIndex[] indexs = new TmphUintSortIndex[range.GetCount << 1];
                    fixed (TmphUintSortIndex* indexFixed = indexs)
                    {
                        TmphUintSortIndex.Create(indexFixed, array.Array, getKey, array.StartIndex + range.SkipCount, range.GetCount);
                        Algorithm.TmphRadixSort.Sort(indexFixed, indexFixed + range.GetCount, range.GetCount);
                        return TmphUintSortIndex.Create(indexFixed, array.Array, range.GetCount);
                    }
                }
                if (range.GetCount > 1) return Algorithm.TmphQuickSort.GetSort(array.Array, getKey, array.StartIndex + range.SkipCount, range.GetCount);
                if (range.GetCount != 0) return new TValueType[] { array.Array[array.StartIndex + range.SkipCount] };
            }
            return TmphNullValue<TValueType>.Array;
        }

        public static unsafe TValueType[] getSortDesc<TValueType>(this TmphSubArray<TValueType> array, Func<TValueType, uint> getKey)
        {
            if (getKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            if (array.Count > 1)
            {
                if (array.Count >= Laurent.Lee.CLB.TmphPub.RadixSortSize64)
                {
                    TmphUintSortIndex[] indexs = new TmphUintSortIndex[array.Count << 1];
                    fixed (TmphUintSortIndex* indexFixed = indexs)
                    {
                        TmphUintSortIndex.Create(indexFixed, array.Array, getKey, array.StartIndex, array.Count);
                        Algorithm.TmphRadixSort.SortDesc(indexFixed, indexFixed + array.Count, array.Count);
                        return TmphUintSortIndex.Create(indexFixed, array.Array, array.Count);
                    }
                }
                return Algorithm.TmphQuickSort.GetSortDesc(array.Array, getKey, array.StartIndex, array.Count);
            }
            return array.GetArray();
        }

        public static unsafe TValueType[] getSortDesc<TValueType>(this TmphSubArray<TValueType> array, Func<TValueType, uint> getKey, int startIndex, int count)
        {
            if (getKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            if (array.Count != 0)
            {
                TmphArray.TmphRange range = new TmphArray.TmphRange(array.Count, startIndex, count);
                if (range.GetCount >= Laurent.Lee.CLB.TmphPub.RadixSortSize64)
                {
                    TmphUintSortIndex[] indexs = new TmphUintSortIndex[range.GetCount << 1];
                    fixed (TmphUintSortIndex* indexFixed = indexs)
                    {
                        TmphUintSortIndex.Create(indexFixed, array.Array, getKey, array.StartIndex + range.SkipCount, range.GetCount);
                        Algorithm.TmphRadixSort.SortDesc(indexFixed, indexFixed + range.GetCount, range.GetCount);
                        return TmphUintSortIndex.Create(indexFixed, array.Array, range.GetCount);
                    }
                }
                if (range.GetCount > 1) return Algorithm.TmphQuickSort.GetSortDesc(array.Array, getKey, array.StartIndex + range.SkipCount, range.GetCount);
                if (range.GetCount != 0) return new TValueType[] { array.Array[array.StartIndex + range.SkipCount] };
            }
            return TmphNullValue<TValueType>.Array;
        }

        public static TmphSubArray<int> sort(this TmphSubArray<int> array)
        {
            if (array.Count > 1)
            {
                if (array.Count >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) Algorithm.TmphRadixSort.Sort(array.Array, array.StartIndex, array.Count);
                else Algorithm.TmphQuickSort.Sort(array.Array, array.StartIndex, array.Count);
            }
            return array;
        }

        public static TmphSubArray<int> sort(this TmphSubArray<int> array, int startIndex, int count)
        {
            if (array.Count != 0)
            {
                TmphArray.TmphRange range = new TmphArray.TmphRange(array.Count, startIndex, count);
                if (range.GetCount >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) Algorithm.TmphRadixSort.Sort(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
                else if (range.GetCount > 1) Algorithm.TmphQuickSort.Sort(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
                return TmphSubArray<int>.Unsafe(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
            }
            return array;
        }

        public static TmphSubArray<int> sortDesc(this TmphSubArray<int> array)
        {
            if (array.Count > 1)
            {
                if (array.Count >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) Algorithm.TmphRadixSort.SortDesc(array.Array, array.StartIndex, array.Count);
                else Algorithm.TmphQuickSort.SortDesc(array.Array, array.StartIndex, array.Count);
            }
            return array;
        }

        public static TmphSubArray<int> sortDesc(this TmphSubArray<int> array, int startIndex, int count)
        {
            if (array.Count != 0)
            {
                TmphArray.TmphRange range = new TmphArray.TmphRange(array.Count, startIndex, count);
                if (range.GetCount >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) Algorithm.TmphRadixSort.SortDesc(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
                else if (range.GetCount > 1) Algorithm.TmphQuickSort.SortDesc(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
                return TmphSubArray<int>.Unsafe(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
            }
            return array;
        }

        public static int[] getSort(this TmphSubArray<int> array)
        {
            if (array.Count > 1)
            {
                if (array.Count >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) return Algorithm.TmphRadixSort.GetSort(array.Array, array.StartIndex, array.Count);
                return Algorithm.TmphQuickSort.GetSort(array.Array, array.StartIndex, array.Count);
            }
            return array.GetArray();
        }

        public static int[] getSort(this TmphSubArray<int> array, int startIndex, int count)
        {
            if (array.Count != 0)
            {
                TmphArray.TmphRange range = new TmphArray.TmphRange(array.Count, startIndex, count);
                if (range.GetCount >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) return Algorithm.TmphRadixSort.GetSort(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
                return Algorithm.TmphQuickSort.GetSort(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
            }
            return TmphNullValue<int>.Array;
        }

        public static int[] getSortDesc(this TmphSubArray<int> array)
        {
            if (array.Count > 1)
            {
                if (array.Count >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) return Algorithm.TmphRadixSort.GetSortDesc(array.Array, array.StartIndex, array.Count);
                return Algorithm.TmphQuickSort.GetSortDesc(array.Array, array.StartIndex, array.Count);
            }
            return array.GetArray();
        }

        public static int[] getSortDesc(this TmphSubArray<int> array, int startIndex, int count)
        {
            if (array.Count != 0)
            {
                TmphArray.TmphRange range = new TmphArray.TmphRange(array.Count, startIndex, count);
                if (range.GetCount >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) return Algorithm.TmphRadixSort.GetSortDesc(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
                return Algorithm.TmphQuickSort.GetSortDesc(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
            }
            return TmphNullValue<int>.Array;
        }

        public static unsafe TValueType[] getSort<TValueType>(this TmphSubArray<TValueType> array, Func<TValueType, int> getKey)
        {
            if (getKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            if (array.Count > 1)
            {
                if (array.Count >= Laurent.Lee.CLB.TmphPub.RadixSortSize64)
                {
                    TmphIntSortIndex[] indexs = new TmphIntSortIndex[array.Count << 1];
                    fixed (TmphIntSortIndex* indexFixed = indexs)
                    {
                        TmphIntSortIndex.Create(indexFixed, array.Array, getKey, array.StartIndex, array.Count);
                        Algorithm.TmphRadixSort.Sort(indexFixed, (TmphUintSortIndex*)(indexFixed + array.Count), array.Count);
                        return TmphIntSortIndex.Create(indexFixed, array.Array, array.Count);
                    }
                }
                return Algorithm.TmphQuickSort.GetSort(array.Array, getKey, array.StartIndex, array.Count);
            }
            return array.GetArray();
        }

        public static unsafe TValueType[] getSort<TValueType>(this TmphSubArray<TValueType> array, Func<TValueType, int> getKey, int startIndex, int count)
        {
            if (getKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            if (array.Count != 0)
            {
                TmphArray.TmphRange range = new TmphArray.TmphRange(array.Count, startIndex, count);
                if (range.GetCount >= Laurent.Lee.CLB.TmphPub.RadixSortSize64)
                {
                    TmphIntSortIndex[] indexs = new TmphIntSortIndex[range.GetCount << 1];
                    fixed (TmphIntSortIndex* indexFixed = indexs)
                    {
                        TmphIntSortIndex.Create(indexFixed, array.Array, getKey, array.StartIndex + range.SkipCount, range.GetCount);
                        Algorithm.TmphRadixSort.Sort(indexFixed, (TmphUintSortIndex*)(indexFixed + range.GetCount), range.GetCount);
                        return TmphIntSortIndex.Create(indexFixed, array.Array, range.GetCount);
                    }
                }
                if (range.GetCount > 1) return Algorithm.TmphQuickSort.GetSort(array.Array, getKey, array.StartIndex + range.SkipCount, range.GetCount);
                if (range.GetCount != 0) return new TValueType[] { array.Array[array.StartIndex + range.SkipCount] };
            }
            return TmphNullValue<TValueType>.Array;
        }

        public static unsafe TValueType[] getSortDesc<TValueType>(this TmphSubArray<TValueType> array, Func<TValueType, int> getKey)
        {
            if (getKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            if (array.Count > 1)
            {
                if (array.Count >= Laurent.Lee.CLB.TmphPub.RadixSortSize64)
                {
                    TmphIntSortIndex[] indexs = new TmphIntSortIndex[array.Count << 1];
                    fixed (TmphIntSortIndex* indexFixed = indexs)
                    {
                        TmphIntSortIndex.Create(indexFixed, array.Array, getKey, array.StartIndex, array.Count);
                        Algorithm.TmphRadixSort.SortDesc(indexFixed, (TmphUintSortIndex*)(indexFixed + array.Count), array.Count);
                        return TmphIntSortIndex.Create(indexFixed, array.Array, array.Count);
                    }
                }
                return Algorithm.TmphQuickSort.GetSortDesc(array.Array, getKey, array.StartIndex, array.Count);
            }
            return array.GetArray();
        }

        public static unsafe TValueType[] getSortDesc<TValueType>(this TmphSubArray<TValueType> array, Func<TValueType, int> getKey, int startIndex, int count)
        {
            if (getKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            if (array.Count != 0)
            {
                TmphArray.TmphRange range = new TmphArray.TmphRange(array.Count, startIndex, count);
                if (range.GetCount >= Laurent.Lee.CLB.TmphPub.RadixSortSize64)
                {
                    TmphIntSortIndex[] indexs = new TmphIntSortIndex[range.GetCount << 1];
                    fixed (TmphIntSortIndex* indexFixed = indexs)
                    {
                        TmphIntSortIndex.Create(indexFixed, array.Array, getKey, array.StartIndex + range.SkipCount, range.GetCount);
                        Algorithm.TmphRadixSort.SortDesc(indexFixed, (TmphUintSortIndex*)(indexFixed + range.GetCount), range.GetCount);
                        return TmphIntSortIndex.Create(indexFixed, array.Array, range.GetCount);
                    }
                }
                if (range.GetCount > 1) return Algorithm.TmphQuickSort.GetSortDesc(array.Array, getKey, array.StartIndex + range.SkipCount, range.GetCount);
                if (range.GetCount != 0) return new TValueType[] { array.Array[array.StartIndex + range.SkipCount] };
            }
            return TmphNullValue<TValueType>.Array;
        }

        public static TmphSubArray<DateTime> sort(this TmphSubArray<DateTime> array)
        {
            if (array.Count > 1)
            {
                if (array.Count >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) Algorithm.TmphRadixSort.Sort(array.Array, array.StartIndex, array.Count);
                else Algorithm.TmphQuickSort.Sort(array.Array, array.StartIndex, array.Count);
            }
            return array;
        }

        public static TmphSubArray<DateTime> sort(this TmphSubArray<DateTime> array, int startIndex, int count)
        {
            if (array.Count != 0)
            {
                TmphArray.TmphRange range = new TmphArray.TmphRange(array.Count, startIndex, count);
                if (range.GetCount >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) Algorithm.TmphRadixSort.Sort(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
                else if (range.GetCount > 1) Algorithm.TmphQuickSort.Sort(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
                return TmphSubArray<DateTime>.Unsafe(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
            }
            return array;
        }

        public static TmphSubArray<DateTime> sortDesc(this TmphSubArray<DateTime> array)
        {
            if (array.Count > 1)
            {
                if (array.Count >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) Algorithm.TmphRadixSort.SortDesc(array.Array, array.StartIndex, array.Count);
                else Algorithm.TmphQuickSort.SortDesc(array.Array, array.StartIndex, array.Count);
            }
            return array;
        }

        public static TmphSubArray<DateTime> sortDesc(this TmphSubArray<DateTime> array, int startIndex, int count)
        {
            if (array.Count != 0)
            {
                TmphArray.TmphRange range = new TmphArray.TmphRange(array.Count, startIndex, count);
                if (range.GetCount >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) Algorithm.TmphRadixSort.SortDesc(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
                else if (range.GetCount > 1) Algorithm.TmphQuickSort.SortDesc(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
                return TmphSubArray<DateTime>.Unsafe(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
            }
            return array;
        }

        public static DateTime[] getSort(this TmphSubArray<DateTime> array)
        {
            if (array.Count > 1)
            {
                if (array.Count >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) return Algorithm.TmphRadixSort.GetSort(array.Array, array.StartIndex, array.Count);
                return Algorithm.TmphQuickSort.GetSort(array.Array, array.StartIndex, array.Count);
            }
            return array.GetArray();
        }

        public static DateTime[] getSort(this TmphSubArray<DateTime> array, int startIndex, int count)
        {
            if (array.Count != 0)
            {
                TmphArray.TmphRange range = new TmphArray.TmphRange(array.Count, startIndex, count);
                if (range.GetCount >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) return Algorithm.TmphRadixSort.GetSort(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
                return Algorithm.TmphQuickSort.GetSort(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
            }
            return TmphNullValue<DateTime>.Array;
        }

        public static DateTime[] getSortDesc(this TmphSubArray<DateTime> array)
        {
            if (array.Count > 1)
            {
                if (array.Count >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) return Algorithm.TmphRadixSort.GetSortDesc(array.Array, array.StartIndex, array.Count);
                return Algorithm.TmphQuickSort.GetSortDesc(array.Array, array.StartIndex, array.Count);
            }
            return array.GetArray();
        }

        public static DateTime[] getSortDesc(this TmphSubArray<DateTime> array, int startIndex, int count)
        {
            if (array.Count != 0)
            {
                TmphArray.TmphRange range = new TmphArray.TmphRange(array.Count, startIndex, count);
                if (range.GetCount >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) return Algorithm.TmphRadixSort.GetSortDesc(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
                return Algorithm.TmphQuickSort.GetSortDesc(array.Array, array.StartIndex + range.SkipCount, range.GetCount);
            }
            return TmphNullValue<DateTime>.Array;
        }

        public static unsafe TValueType[] getSort<TValueType>(this TmphSubArray<TValueType> array, Func<TValueType, DateTime> getKey)
        {
            if (getKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            if (array.Count > 1)
            {
                if (array.Count >= Laurent.Lee.CLB.TmphPub.RadixSortSize64)
                {
                    TmphLongSortIndex[] indexs = new TmphLongSortIndex[array.Count << 1];
                    fixed (TmphLongSortIndex* indexFixed = indexs)
                    {
                        TmphDateTimeSortIndex.Create(indexFixed, array.Array, getKey, array.StartIndex, array.Count);
                        Algorithm.TmphRadixSort.Sort(indexFixed, (ulongSortIndex*)(indexFixed + array.Count), array.Count);
                        return TmphLongSortIndex.Create(indexFixed, array.Array, array.Count);
                    }
                }
                return Algorithm.TmphQuickSort.GetSort(array.Array, getKey, array.StartIndex, array.Count);
            }
            return array.GetArray();
        }

        public static unsafe TValueType[] getSort<TValueType>(this TmphSubArray<TValueType> array, Func<TValueType, DateTime> getKey, int startIndex, int count)
        {
            if (getKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            if (array.Count != 0)
            {
                TmphArray.TmphRange range = new TmphArray.TmphRange(array.Count, startIndex, count);
                if (range.GetCount >= Laurent.Lee.CLB.TmphPub.RadixSortSize64)
                {
                    TmphLongSortIndex[] indexs = new TmphLongSortIndex[range.GetCount << 1];
                    fixed (TmphLongSortIndex* indexFixed = indexs)
                    {
                        TmphDateTimeSortIndex.Create(indexFixed, array.Array, getKey, array.StartIndex + range.SkipCount, range.GetCount);
                        Algorithm.TmphRadixSort.Sort(indexFixed, (ulongSortIndex*)(indexFixed + range.GetCount), range.GetCount);
                        return TmphLongSortIndex.Create(indexFixed, array.Array, range.GetCount);
                    }
                }
                if (range.GetCount > 1) return Algorithm.TmphQuickSort.GetSort(array.Array, getKey, array.StartIndex + range.SkipCount, range.GetCount);
                if (range.GetCount != 0) return new TValueType[] { array.Array[array.StartIndex + range.SkipCount] };
            }
            return TmphNullValue<TValueType>.Array;
        }

        public static unsafe TValueType[] getSortDesc<TValueType>(this TmphSubArray<TValueType> array, Func<TValueType, DateTime> getKey)
        {
            if (getKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            if (array.Count > 1)
            {
                if (array.Count >= Laurent.Lee.CLB.TmphPub.RadixSortSize64)
                {
                    TmphLongSortIndex[] indexs = new TmphLongSortIndex[array.Count << 1];
                    fixed (TmphLongSortIndex* indexFixed = indexs)
                    {
                        TmphDateTimeSortIndex.Create(indexFixed, array.Array, getKey, array.StartIndex, array.Count);
                        Algorithm.TmphRadixSort.SortDesc(indexFixed, (ulongSortIndex*)(indexFixed + array.Count), array.Count);
                        return TmphLongSortIndex.Create(indexFixed, array.Array, array.Count);
                    }
                }
                return Algorithm.TmphQuickSort.GetSortDesc(array.Array, getKey, array.StartIndex, array.Count);
            }
            return array.GetArray();
        }

        public static unsafe TValueType[] getSortDesc<TValueType>(this TmphSubArray<TValueType> array, Func<TValueType, DateTime> getKey, int startIndex, int count)
        {
            if (getKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            if (array.Count != 0)
            {
                TmphArray.TmphRange range = new TmphArray.TmphRange(array.Count, startIndex, count);
                if (range.GetCount >= Laurent.Lee.CLB.TmphPub.RadixSortSize64)
                {
                    TmphLongSortIndex[] indexs = new TmphLongSortIndex[range.GetCount << 1];
                    fixed (TmphLongSortIndex* indexFixed = indexs)
                    {
                        TmphDateTimeSortIndex.Create(indexFixed, array.Array, getKey, array.StartIndex + range.SkipCount, range.GetCount);
                        Algorithm.TmphRadixSort.SortDesc(indexFixed, (ulongSortIndex*)(indexFixed + range.GetCount), range.GetCount);
                        return TmphLongSortIndex.Create(indexFixed, array.Array, range.GetCount);
                    }
                }
                if (range.GetCount > 1) return Algorithm.TmphQuickSort.GetSortDesc(array.Array, getKey, array.StartIndex + range.SkipCount, range.GetCount);
                if (range.GetCount != 0) return new TValueType[] { array.Array[array.StartIndex + range.SkipCount] };
            }
            return TmphNullValue<TValueType>.Array;
        }
    }
}