using System;

namespace Laurent.Lee.CLB
{
    /// <summary>
    /// 数组扩展操作
    /// </summary>
    public static partial class TmphArrayExtension
    {
        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <returns>排序后的数组</returns>
        public static ulong[] sort(this ulong[] array)
        {
            if (array != null)
            {
                if (array.Length >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) Algorithm.TmphRadixSort.Sort(array, 0, array.Length);
                else Algorithm.TmphQuickSort.Sort(array);
                return array;
            }
            return TmphNullValue<ulong>.Array;
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">数量</param>
        /// <returns>排序后的数组</returns>
        public static TmphSubArray<ulong> sort(this ulong[] array, int startIndex, int count)
        {
            return new TmphSubArray<ulong>(array, startIndex, count).sort();
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <returns>排序后的数组</returns>
        public static ulong[] sortDesc(this ulong[] array)
        {
            if (array != null)
            {
                if (array.Length >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) Algorithm.TmphRadixSort.SortDesc(array, 0, array.Length);
                else Algorithm.TmphQuickSort.SortDesc(array);
                return array;
            }
            return TmphNullValue<ulong>.Array;
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">数量</param>
        /// <returns>排序后的数组</returns>
        public static TmphSubArray<ulong> sortDesc(this ulong[] array, int startIndex, int count)
        {
            return new TmphSubArray<ulong>(array, startIndex, count).sortDesc();
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <returns>排序后的新数组</returns>
        public static ulong[] getSort(this ulong[] array)
        {
            return new TmphSubArray<ulong>(array).getSort();
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">数量</param>
        /// <returns>排序后的数组</returns>
        public static ulong[] getSort(this ulong[] array, int startIndex, int count)
        {
            return new TmphSubArray<ulong>(array, startIndex, count).getSort();
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <returns>排序后的新数组</returns>
        public static ulong[] getSortDesc(this ulong[] array)
        {
            return new TmphSubArray<ulong>(array).getSortDesc();
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">数量</param>
        /// <returns>排序后的数组</returns>
        public static ulong[] getSortDesc(this ulong[] array, int startIndex, int count)
        {
            return new TmphSubArray<ulong>(array, startIndex, count).getSortDesc();
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">待排序数组</param>
        /// <param name="getKey">排序键</param>
        /// <returns>排序后的数组</returns>
        public static unsafe TValueType[] getSort<TValueType>(this TValueType[] array, Func<TValueType, ulong> getKey)
        {
            return new TmphSubArray<TValueType>(array).getSort(getKey);
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">待排序数组</param>
        /// <param name="getKey">排序键</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">数量</param>
        /// <returns>排序后的数组</returns>
        public static unsafe TValueType[] getSort<TValueType>(this TValueType[] array, Func<TValueType, ulong> getKey, int startIndex, int count)
        {
            return new TmphSubArray<TValueType>(array, startIndex, count).getSort(getKey);
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">待排序数组</param>
        /// <param name="getKey">排序键</param>
        /// <returns>排序后的数组</returns>
        public static unsafe TValueType[] getSortDesc<TValueType>(this TValueType[] array, Func<TValueType, ulong> getKey)
        {
            return new TmphSubArray<TValueType>(array).getSortDesc(getKey);
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">待排序数组</param>
        /// <param name="getKey">排序键</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">数量</param>
        /// <returns>排序后的数组</returns>
        public static unsafe TValueType[] getSortDesc<TValueType>(this TValueType[] array, Func<TValueType, ulong> getKey, int startIndex, int count)
        {
            return new TmphSubArray<TValueType>(array, startIndex, count).getSortDesc(getKey);
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <returns>排序后的数组</returns>
        public static long[] sort(this long[] array)
        {
            if (array != null)
            {
                if (array.Length >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) Algorithm.TmphRadixSort.Sort(array, 0, array.Length);
                else Algorithm.TmphQuickSort.Sort(array);
                return array;
            }
            return TmphNullValue<long>.Array;
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">数量</param>
        /// <returns>排序后的数组</returns>
        public static TmphSubArray<long> sort(this long[] array, int startIndex, int count)
        {
            return new TmphSubArray<long>(array, startIndex, count).sort();
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <returns>排序后的数组</returns>
        public static long[] sortDesc(this long[] array)
        {
            if (array != null)
            {
                if (array.Length >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) Algorithm.TmphRadixSort.SortDesc(array, 0, array.Length);
                else Algorithm.TmphQuickSort.SortDesc(array);
                return array;
            }
            return TmphNullValue<long>.Array;
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">数量</param>
        /// <returns>排序后的数组</returns>
        public static TmphSubArray<long> sortDesc(this long[] array, int startIndex, int count)
        {
            return new TmphSubArray<long>(array, startIndex, count).sortDesc();
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <returns>排序后的新数组</returns>
        public static long[] getSort(this long[] array)
        {
            return new TmphSubArray<long>(array).getSort();
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">数量</param>
        /// <returns>排序后的数组</returns>
        public static long[] getSort(this long[] array, int startIndex, int count)
        {
            return new TmphSubArray<long>(array, startIndex, count).getSort();
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <returns>排序后的新数组</returns>
        public static long[] getSortDesc(this long[] array)
        {
            return new TmphSubArray<long>(array).getSortDesc();
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">数量</param>
        /// <returns>排序后的数组</returns>
        public static long[] getSortDesc(this long[] array, int startIndex, int count)
        {
            return new TmphSubArray<long>(array, startIndex, count).getSortDesc();
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">待排序数组</param>
        /// <param name="getKey">排序键</param>
        /// <returns>排序后的数组</returns>
        public static unsafe TValueType[] getSort<TValueType>(this TValueType[] array, Func<TValueType, long> getKey)
        {
            return new TmphSubArray<TValueType>(array).getSort(getKey);
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">待排序数组</param>
        /// <param name="getKey">排序键</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">数量</param>
        /// <returns>排序后的数组</returns>
        public static unsafe TValueType[] getSort<TValueType>(this TValueType[] array, Func<TValueType, long> getKey, int startIndex, int count)
        {
            return new TmphSubArray<TValueType>(array, startIndex, count).getSort(getKey);
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">待排序数组</param>
        /// <param name="getKey">排序键</param>
        /// <returns>排序后的数组</returns>
        public static unsafe TValueType[] getSortDesc<TValueType>(this TValueType[] array, Func<TValueType, long> getKey)
        {
            return new TmphSubArray<TValueType>(array).getSortDesc(getKey);
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">待排序数组</param>
        /// <param name="getKey">排序键</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">数量</param>
        /// <returns>排序后的数组</returns>
        public static unsafe TValueType[] getSortDesc<TValueType>(this TValueType[] array, Func<TValueType, long> getKey, int startIndex, int count)
        {
            return new TmphSubArray<TValueType>(array, startIndex, count).getSortDesc(getKey);
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <returns>排序后的数组</returns>
        public static uint[] sort(this uint[] array)
        {
            if (array != null)
            {
                if (array.Length >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) Algorithm.TmphRadixSort.Sort(array, 0, array.Length);
                else Algorithm.TmphQuickSort.Sort(array);
                return array;
            }
            return TmphNullValue<uint>.Array;
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">数量</param>
        /// <returns>排序后的数组</returns>
        public static TmphSubArray<uint> sort(this uint[] array, int startIndex, int count)
        {
            return new TmphSubArray<uint>(array, startIndex, count).sort();
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <returns>排序后的数组</returns>
        public static uint[] sortDesc(this uint[] array)
        {
            if (array != null)
            {
                if (array.Length >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) Algorithm.TmphRadixSort.SortDesc(array, 0, array.Length);
                else Algorithm.TmphQuickSort.SortDesc(array);
                return array;
            }
            return TmphNullValue<uint>.Array;
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">数量</param>
        /// <returns>排序后的数组</returns>
        public static TmphSubArray<uint> sortDesc(this uint[] array, int startIndex, int count)
        {
            return new TmphSubArray<uint>(array, startIndex, count).sortDesc();
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <returns>排序后的新数组</returns>
        public static uint[] getSort(this uint[] array)
        {
            return new TmphSubArray<uint>(array).getSort();
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">数量</param>
        /// <returns>排序后的数组</returns>
        public static uint[] getSort(this uint[] array, int startIndex, int count)
        {
            return new TmphSubArray<uint>(array, startIndex, count).getSort();
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <returns>排序后的新数组</returns>
        public static uint[] getSortDesc(this uint[] array)
        {
            return new TmphSubArray<uint>(array).getSortDesc();
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">数量</param>
        /// <returns>排序后的数组</returns>
        public static uint[] getSortDesc(this uint[] array, int startIndex, int count)
        {
            return new TmphSubArray<uint>(array, startIndex, count).getSortDesc();
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">待排序数组</param>
        /// <param name="getKey">排序键</param>
        /// <returns>排序后的数组</returns>
        public static unsafe TValueType[] getSort<TValueType>(this TValueType[] array, Func<TValueType, uint> getKey)
        {
            return new TmphSubArray<TValueType>(array).getSort(getKey);
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">待排序数组</param>
        /// <param name="getKey">排序键</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">数量</param>
        /// <returns>排序后的数组</returns>
        public static unsafe TValueType[] getSort<TValueType>(this TValueType[] array, Func<TValueType, uint> getKey, int startIndex, int count)
        {
            return new TmphSubArray<TValueType>(array, startIndex, count).getSort(getKey);
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">待排序数组</param>
        /// <param name="getKey">排序键</param>
        /// <returns>排序后的数组</returns>
        public static unsafe TValueType[] getSortDesc<TValueType>(this TValueType[] array, Func<TValueType, uint> getKey)
        {
            return new TmphSubArray<TValueType>(array).getSortDesc(getKey);
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">待排序数组</param>
        /// <param name="getKey">排序键</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">数量</param>
        /// <returns>排序后的数组</returns>
        public static unsafe TValueType[] getSortDesc<TValueType>(this TValueType[] array, Func<TValueType, uint> getKey, int startIndex, int count)
        {
            return new TmphSubArray<TValueType>(array, startIndex, count).getSortDesc(getKey);
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <returns>排序后的数组</returns>
        public static int[] sort(this int[] array)
        {
            if (array != null)
            {
                if (array.Length >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) Algorithm.TmphRadixSort.Sort(array, 0, array.Length);
                else Algorithm.TmphQuickSort.Sort(array);
                return array;
            }
            return TmphNullValue<int>.Array;
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">数量</param>
        /// <returns>排序后的数组</returns>
        public static TmphSubArray<int> sort(this int[] array, int startIndex, int count)
        {
            return new TmphSubArray<int>(array, startIndex, count).sort();
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <returns>排序后的数组</returns>
        public static int[] sortDesc(this int[] array)
        {
            if (array != null)
            {
                if (array.Length >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) Algorithm.TmphRadixSort.SortDesc(array, 0, array.Length);
                else Algorithm.TmphQuickSort.SortDesc(array);
                return array;
            }
            return TmphNullValue<int>.Array;
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">数量</param>
        /// <returns>排序后的数组</returns>
        public static TmphSubArray<int> sortDesc(this int[] array, int startIndex, int count)
        {
            return new TmphSubArray<int>(array, startIndex, count).sortDesc();
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <returns>排序后的新数组</returns>
        public static int[] getSort(this int[] array)
        {
            return new TmphSubArray<int>(array).getSort();
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">数量</param>
        /// <returns>排序后的数组</returns>
        public static int[] getSort(this int[] array, int startIndex, int count)
        {
            return new TmphSubArray<int>(array, startIndex, count).getSort();
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <returns>排序后的新数组</returns>
        public static int[] getSortDesc(this int[] array)
        {
            return new TmphSubArray<int>(array).getSortDesc();
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">数量</param>
        /// <returns>排序后的数组</returns>
        public static int[] getSortDesc(this int[] array, int startIndex, int count)
        {
            return new TmphSubArray<int>(array, startIndex, count).getSortDesc();
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">待排序数组</param>
        /// <param name="getKey">排序键</param>
        /// <returns>排序后的数组</returns>
        public static unsafe TValueType[] getSort<TValueType>(this TValueType[] array, Func<TValueType, int> getKey)
        {
            return new TmphSubArray<TValueType>(array).getSort(getKey);
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">待排序数组</param>
        /// <param name="getKey">排序键</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">数量</param>
        /// <returns>排序后的数组</returns>
        public static unsafe TValueType[] getSort<TValueType>(this TValueType[] array, Func<TValueType, int> getKey, int startIndex, int count)
        {
            return new TmphSubArray<TValueType>(array, startIndex, count).getSort(getKey);
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">待排序数组</param>
        /// <param name="getKey">排序键</param>
        /// <returns>排序后的数组</returns>
        public static unsafe TValueType[] getSortDesc<TValueType>(this TValueType[] array, Func<TValueType, int> getKey)
        {
            return new TmphSubArray<TValueType>(array).getSortDesc(getKey);
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">待排序数组</param>
        /// <param name="getKey">排序键</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">数量</param>
        /// <returns>排序后的数组</returns>
        public static unsafe TValueType[] getSortDesc<TValueType>(this TValueType[] array, Func<TValueType, int> getKey, int startIndex, int count)
        {
            return new TmphSubArray<TValueType>(array, startIndex, count).getSortDesc(getKey);
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <returns>排序后的数组</returns>
        public static DateTime[] sort(this DateTime[] array)
        {
            if (array != null)
            {
                if (array.Length >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) Algorithm.TmphRadixSort.Sort(array, 0, array.Length);
                else Algorithm.TmphQuickSort.Sort(array);
                return array;
            }
            return TmphNullValue<DateTime>.Array;
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">数量</param>
        /// <returns>排序后的数组</returns>
        public static TmphSubArray<DateTime> sort(this DateTime[] array, int startIndex, int count)
        {
            return new TmphSubArray<DateTime>(array, startIndex, count).sort();
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <returns>排序后的数组</returns>
        public static DateTime[] sortDesc(this DateTime[] array)
        {
            if (array != null)
            {
                if (array.Length >= Laurent.Lee.CLB.TmphPub.RadixSortSize64) Algorithm.TmphRadixSort.SortDesc(array, 0, array.Length);
                else Algorithm.TmphQuickSort.SortDesc(array);
                return array;
            }
            return TmphNullValue<DateTime>.Array;
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">数量</param>
        /// <returns>排序后的数组</returns>
        public static TmphSubArray<DateTime> sortDesc(this DateTime[] array, int startIndex, int count)
        {
            return new TmphSubArray<DateTime>(array, startIndex, count).sortDesc();
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <returns>排序后的新数组</returns>
        public static DateTime[] getSort(this DateTime[] array)
        {
            return new TmphSubArray<DateTime>(array).getSort();
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">数量</param>
        /// <returns>排序后的数组</returns>
        public static DateTime[] getSort(this DateTime[] array, int startIndex, int count)
        {
            return new TmphSubArray<DateTime>(array, startIndex, count).getSort();
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <returns>排序后的新数组</returns>
        public static DateTime[] getSortDesc(this DateTime[] array)
        {
            return new TmphSubArray<DateTime>(array).getSortDesc();
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">数量</param>
        /// <returns>排序后的数组</returns>
        public static DateTime[] getSortDesc(this DateTime[] array, int startIndex, int count)
        {
            return new TmphSubArray<DateTime>(array, startIndex, count).getSortDesc();
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">待排序数组</param>
        /// <param name="getKey">排序键</param>
        /// <returns>排序后的数组</returns>
        public static unsafe TValueType[] getSort<TValueType>(this TValueType[] array, Func<TValueType, DateTime> getKey)
        {
            return new TmphSubArray<TValueType>(array).getSort(getKey);
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">待排序数组</param>
        /// <param name="getKey">排序键</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">数量</param>
        /// <returns>排序后的数组</returns>
        public static unsafe TValueType[] getSort<TValueType>(this TValueType[] array, Func<TValueType, DateTime> getKey, int startIndex, int count)
        {
            return new TmphSubArray<TValueType>(array, startIndex, count).getSort(getKey);
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">待排序数组</param>
        /// <param name="getKey">排序键</param>
        /// <returns>排序后的数组</returns>
        public static unsafe TValueType[] getSortDesc<TValueType>(this TValueType[] array, Func<TValueType, DateTime> getKey)
        {
            return new TmphSubArray<TValueType>(array).getSortDesc(getKey);
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">待排序数组</param>
        /// <param name="getKey">排序键</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">数量</param>
        /// <returns>排序后的数组</returns>
        public static unsafe TValueType[] getSortDesc<TValueType>(this TValueType[] array, Func<TValueType, DateTime> getKey, int startIndex, int count)
        {
            return new TmphSubArray<TValueType>(array, startIndex, count).getSortDesc(getKey);
        }
    }
}