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
    public static class TmphRadixSort
    {
#if MONO
        private static TmphMemoryPool countBuffer = TmphMemoryPool.GetPool(256 * 4 * sizeof(int));
#else
        private static readonly TmphMemoryPool countBuffer = TmphMemoryPoolProxy.GetPool(256 * 4 * sizeof(int));
#endif

        private static unsafe void sort(uint* arrayFixed, uint* newArrayFixed, uint* swapFixed, int length)
        {
            bool isNewCount;
            var count = countBuffer.Get(out isNewCount);
            fixed (byte* countFixed = count)
            {
                int* count0 = (int*)countFixed + 1,
                    count8 = count0 + 256,
                    count16 = count0 + 256 * 2,
                    count24 = count0 + (256 * 3 - 1);
                if (!isNewCount) Array.Clear(count, sizeof(int), (256 * 4 - 1) * sizeof(int));
                for (uint* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    ++count0[*(byte*)start];
                    ++count8[*((byte*)start + 1)];
                    ++count16[*((byte*)start + 2)];
                    var value = *((byte*)start + 3);
                    ++count24[++value];
                }
                var index = *count0;
                for (int* start = count0 + 1, end = count0 + 255; start != end; *start++ = index) index += *start;
                index = *count8;
                for (int* start = count8 + 1, end = count8 + 255; start != end; *start++ = index) index += *start;
                index = *count16;
                for (int* start = count16 + 1, end = count16 + 255; start != end; *start++ = index) index += *start;
                *count24 = 0;
                index = *(count24 + 1);
                for (int* start = count24 + 2, end = count24 + 256; start != end; *start++ = index) index += *start;
                *--count0 = 0;
                for (uint* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    swapFixed[count0[*(byte*)start]++] = *start;
                }
                *--count8 = 0;
                for (uint* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    newArrayFixed[count8[*((byte*)start + 1)]++] = *start;
                }
                *--count16 = 0;
                for (uint* start = newArrayFixed, end = newArrayFixed + length; start != end; ++start)
                {
                    swapFixed[count16[*((byte*)start + 2)]++] = *start;
                }
                for (uint* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    newArrayFixed[count24[*((byte*)start + 3)]++] = *start;
                }
            }
            countBuffer.Push(ref count);
        }

        public static unsafe void Sort(TmphUintSortIndex* arrayFixed, TmphUintSortIndex* swapFixed, int length)
        {
            bool isNewCount;
            var count = countBuffer.Get(out isNewCount);
            fixed (byte* countFixed = count)
            {
                int* count0 = (int*)countFixed + 1,
                    count8 = count0 + 256,
                    count16 = count0 + 256 * 2,
                    count24 = count0 + (256 * 3 - 1);
                if (!isNewCount) Array.Clear(count, sizeof(int), (256 * 4 - 1) * sizeof(int));
                for (TmphUintSortIndex* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    ++count0[*(byte*)start];
                    ++count8[*((byte*)start + 1)];
                    ++count16[*((byte*)start + 2)];
                    var value = *((byte*)start + 3);
                    ++count24[++value];
                }
                var index = *count0;
                for (int* start = count0 + 1, end = count0 + 255; start != end; *start++ = index) index += *start;
                index = *count8;
                for (int* start = count8 + 1, end = count8 + 255; start != end; *start++ = index) index += *start;
                index = *count16;
                for (int* start = count16 + 1, end = count16 + 255; start != end; *start++ = index) index += *start;
                *count24 = 0;
                index = *(count24 + 1);
                for (int* start = count24 + 2, end = count24 + 256; start != end; *start++ = index) index += *start;
                *--count0 = 0;
                for (TmphUintSortIndex* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    swapFixed[count0[*(byte*)start]++].Set((*start).Value, (*start).Index);
                }
                *--count8 = 0;
                for (TmphUintSortIndex* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    arrayFixed[count8[*((byte*)start + 1)]++].Set((*start).Value, (*start).Index);
                }
                *--count16 = 0;
                for (TmphUintSortIndex* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    swapFixed[count16[*((byte*)start + 2)]++].Set((*start).Value, (*start).Index);
                }
                for (TmphUintSortIndex* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    arrayFixed[count24[*((byte*)start + 3)]++].Set((*start).Value, (*start).Index);
                }
            }
            countBuffer.Push(ref count);
        }

        public static unsafe void Sort(uint[] array, int startIndex, int count)
        {
            var swapArray = new uint[count];
            fixed (uint* arrayFixed = array, swapFixed = swapArray)
            {
                var start = arrayFixed + startIndex;
                sort(start, start, swapFixed, count);
            }
        }

        public static unsafe uint[] GetSort(uint[] array, int startIndex, int count)
        {
            uint[] newArray = new uint[count], swapArray = new uint[count];
            fixed (uint* newArrayFixed = newArray, arrayFixed = array, swapFixed = swapArray)
            {
                sort(arrayFixed + startIndex, newArrayFixed, swapFixed, count);
            }
            return newArray;
        }

        private static unsafe void sortDesc(uint* arrayFixed, uint* newArrayFixed, uint* swapFixed, int length)
        {
            bool isNewCount;
            var count = countBuffer.Get(out isNewCount);
            fixed (byte* countFixed = count)
            {
                int* count0 = (int*)countFixed + 1,
                    count8 = count0 + 256,
                    count16 = count0 + 256 * 2,
                    count24 = count0 + (256 * 3 - 1);
                if (!isNewCount) Array.Clear(count, sizeof(int), (256 * 4 - 1) * sizeof(int));
                for (uint* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    ++count0[*(byte*)start];
                    ++count8[*((byte*)start + 1)];
                    ++count16[*((byte*)start + 2)];
                    var value = *((byte*)start + 3);
                    ++count24[++value];
                }
                var index = *count0;
                for (int* start = count0 + 1, end = count0 + 255; start != end; *start++ = index) index += *start;
                index = *count8;
                for (int* start = count8 + 1, end = count8 + 255; start != end; *start++ = index) index += *start;
                index = *count16;
                for (int* start = count16 + 1, end = count16 + 255; start != end; *start++ = index) index += *start;
                *count24 = length;
                index = 0;
                for (int* start = count24 + 1, end = count24 + 256; start != end; *start++ = length - index)
                    index += *start;
                *--count0 = 0;
                for (uint* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    swapFixed[count0[*(byte*)start]++] = *start;
                }
                *--count8 = 0;
                for (uint* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    newArrayFixed[count8[*((byte*)start + 1)]++] = *start;
                }
                *--count16 = 0;
                for (uint* start = newArrayFixed, end = newArrayFixed + length; start != end; ++start)
                {
                    swapFixed[count16[*((byte*)start + 2)]++] = *start;
                }
                for (uint* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    newArrayFixed[--count24[*((byte*)start + 3)]] = *start;
                }
            }
            countBuffer.Push(ref count);
        }

        /// <param name="length">数组数据长度</param>
        public static unsafe void SortDesc(TmphUintSortIndex* arrayFixed, TmphUintSortIndex* swapFixed, int length)
        {
            bool isNewCount;
            var count = countBuffer.Get(out isNewCount);
            fixed (byte* countFixed = count)
            {
                int* count0 = (int*)countFixed + 1,
                    count8 = count0 + 256,
                    count16 = count0 + 256 * 2,
                    count24 = count0 + (256 * 3 - 1);
                if (!isNewCount) Array.Clear(count, sizeof(int), (256 * 4 - 1) * sizeof(int));
                for (TmphUintSortIndex* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    ++count0[*(byte*)start];
                    ++count8[*((byte*)start + 1)];
                    ++count16[*((byte*)start + 2)];
                    var value = *((byte*)start + 3);
                    ++count24[++value];
                }
                var index = *count0;
                for (int* start = count0 + 1, end = count0 + 255; start != end; *start++ = index) index += *start;
                index = *count8;
                for (int* start = count8 + 1, end = count8 + 255; start != end; *start++ = index) index += *start;
                index = *count16;
                for (int* start = count16 + 1, end = count16 + 255; start != end; *start++ = index) index += *start;
                *count24 = length;
                index = 0;
                for (int* start = count24 + 1, end = count24 + 256; start != end; *start++ = length - index)
                    index += *start;
                *--count0 = 0;
                for (TmphUintSortIndex* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    swapFixed[count0[*(byte*)start]++].Set((*start).Value, (*start).Index);
                }
                *--count8 = 0;
                for (TmphUintSortIndex* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    arrayFixed[count8[*((byte*)start + 1)]++].Set((*start).Value, (*start).Index);
                }
                *--count16 = 0;
                for (TmphUintSortIndex* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    swapFixed[count16[*((byte*)start + 2)]++].Set((*start).Value, (*start).Index);
                }
                for (TmphUintSortIndex* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    arrayFixed[--count24[*((byte*)start + 3)]].Set((*start).Value, (*start).Index);
                }
            }
            countBuffer.Push(ref count);
        }

        public static unsafe void SortDesc(uint[] array, int startIndex, int count)
        {
            var swapArray = new uint[count];
            fixed (uint* arrayFixed = array, swapFixed = swapArray)
            {
                var start = arrayFixed + startIndex;
                sortDesc(start, start, swapFixed, count);
            }
        }

        public static unsafe uint[] GetSortDesc(uint[] array, int startIndex, int count)
        {
            uint[] newArray = new uint[count], swapArray = new uint[count];
            fixed (uint* newArrayFixed = newArray, arrayFixed = array, swapFixed = swapArray)
            {
                sortDesc(arrayFixed + startIndex, newArrayFixed, swapFixed, count);
            }
            return newArray;
        }

        private static unsafe void sort(int* arrayFixed, int* newArrayFixed, uint* swapFixed, int length)
        {
            bool isNewCount;
            var count = countBuffer.Get(out isNewCount);
            fixed (byte* countFixed = count)
            {
                int* count0 = (int*)countFixed + 1,
                    count8 = count0 + 256,
                    count16 = count0 + 256 * 2,
                    count24 = count0 + (256 * 3 - 1);
                if (!isNewCount) Array.Clear(count, sizeof(int), (256 * 4 - 1) * sizeof(int));
                for (int* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    ++count0[*(byte*)start];
                    ++count8[*((byte*)start + 1)];
                    ++count16[*((byte*)start + 2)];
                    var value = *((byte*)start + 3);
                    value ^= 0x80;
                    ++count24[++value];
                }
                var index = *count0;
                for (int* start = count0 + 1, end = count0 + 255; start != end; *start++ = index) index += *start;
                index = *count8;
                for (int* start = count8 + 1, end = count8 + 255; start != end; *start++ = index) index += *start;
                index = *count16;
                for (int* start = count16 + 1, end = count16 + 255; start != end; *start++ = index) index += *start;
                *count24 = 0;
                index = *(count24 + 1);
                for (int* start = count24 + 2, end = count24 + 256; start != end; *start++ = index) index += *start;
                *--count0 = 0;
                for (int* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    swapFixed[count0[*(byte*)start]++] = (uint)*start ^ 0x80000000U;
                }
                *--count8 = 0;
                for (uint* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    newArrayFixed[count8[*((byte*)start + 1)]++] = (int)*start;
                }
                *--count16 = 0;
                for (int* start = newArrayFixed, end = newArrayFixed + length; start != end; ++start)
                {
                    swapFixed[count16[*((byte*)start + 2)]++] = (uint)*start;
                }
                for (uint* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    newArrayFixed[count24[*((byte*)start + 3)]++] = (int)(*start ^ 0x80000000U);
                }
            }
            countBuffer.Push(ref count);
        }

        /// <summary>
        ///     数组排序
        /// </summary>
        /// <param name="arrayFixed">数组起始位置</param>
        /// <param name="arrayFixed">目标数组起始位置</param>
        /// <param name="swapFixed">临时数组起始位置</param>
        /// <param name="length">数组数据长度</param>
        public static unsafe void Sort(TmphIntSortIndex* arrayFixed, TmphUintSortIndex* swapFixed, int length)
        {
            bool isNewCount;
            var count = countBuffer.Get(out isNewCount);
            fixed (byte* countFixed = count)
            {
                int* count0 = (int*)countFixed + 1,
                    count8 = count0 + 256,
                    count16 = count0 + 256 * 2,
                    count24 = count0 + (256 * 3 - 1);
                if (!isNewCount) Array.Clear(count, sizeof(int), (256 * 4 - 1) * sizeof(int));
                for (TmphIntSortIndex* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    ++count0[*(byte*)start];
                    ++count8[*((byte*)start + 1)];
                    ++count16[*((byte*)start + 2)];
                    var value = *((byte*)start + 3);
                    value ^= 0x80;
                    ++count24[++value];
                }
                var index = *count0;
                for (int* start = count0 + 1, end = count0 + 255; start != end; *start++ = index) index += *start;
                index = *count8;
                for (int* start = count8 + 1, end = count8 + 255; start != end; *start++ = index) index += *start;
                index = *count16;
                for (int* start = count16 + 1, end = count16 + 255; start != end; *start++ = index) index += *start;
                *count24 = 0;
                index = *(count24 + 1);
                for (int* start = count24 + 2, end = count24 + 256; start != end; *start++ = index) index += *start;
                *--count0 = 0;
                for (TmphIntSortIndex* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    swapFixed[count0[*(byte*)start]++].Set((uint)(*start).Value ^ 0x80000000U, (*start).Index);
                }
                *--count8 = 0;
                for (TmphUintSortIndex* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    arrayFixed[count8[*((byte*)start + 1)]++].Set((int)(*start).Value, (*start).Index);
                }
                *--count16 = 0;
                for (TmphIntSortIndex* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    swapFixed[count16[*((byte*)start + 2)]++].Set((uint)(*start).Value, (*start).Index);
                }
                for (TmphUintSortIndex* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    arrayFixed[count24[*((byte*)start + 3)]++].Set((int)((*start).Value ^ 0x80000000U), (*start).Index);
                }
            }
            countBuffer.Push(ref count);
        }

        /// <summary>
        ///     数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">排序数据数量</param>
        public static unsafe void Sort(int[] array, int startIndex, int count)
        {
            var swapArray = new uint[count];
            fixed (int* arrayFixed = array)
            fixed (uint* swapFixed = swapArray)
            {
                var start = arrayFixed + startIndex;
                sort(start, start, swapFixed, count);
            }
        }

        /// <summary>
        ///     数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">排序数据数量</param>
        /// <returns>排序后的新数组</returns>
        public static unsafe int[] GetSort(int[] array, int startIndex, int count)
        {
            var newArray = new int[count];
            var swapArray = new uint[count];
            fixed (int* newArrayFixed = newArray, arrayFixed = array)
            fixed (uint* swapFixed = swapArray)
            {
                sort(arrayFixed + startIndex, newArrayFixed, swapFixed, count);
            }
            return newArray;
        }

        /// <summary>
        ///     数组排序
        /// </summary>
        /// <param name="arrayFixed">数组起始位置</param>
        /// <param name="newArrayFixed">目标数组起始位置</param>
        /// <param name="swapFixed">临时数组起始位置</param>
        /// <param name="length">数组数据长度</param>
        private static unsafe void sortDesc(int* arrayFixed, int* newArrayFixed, uint* swapFixed, int length)
        {
            bool isNewCount;
            var count = countBuffer.Get(out isNewCount);
            fixed (byte* countFixed = count)
            {
                int* count0 = (int*)countFixed + 1,
                    count8 = count0 + 256,
                    count16 = count0 + 256 * 2,
                    count24 = count0 + (256 * 3 - 1);
                if (!isNewCount) Array.Clear(count, sizeof(int), (256 * 4 - 1) * sizeof(int));
                for (int* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    ++count0[*(byte*)start];
                    ++count8[*((byte*)start + 1)];
                    ++count16[*((byte*)start + 2)];
                    var value = *((byte*)start + 3);
                    value ^= 0x80;
                    ++count24[++value];
                }
                var index = *count0;
                for (int* start = count0 + 1, end = count0 + 255; start != end; *start++ = index) index += *start;
                index = *count8;
                for (int* start = count8 + 1, end = count8 + 255; start != end; *start++ = index) index += *start;
                index = *count16;
                for (int* start = count16 + 1, end = count16 + 255; start != end; *start++ = index) index += *start;
                *count24 = length;
                index = 0;
                for (int* start = count24 + 1, end = count24 + 256; start != end; *start++ = length - index)
                    index += *start;
                *--count0 = 0;
                for (int* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    swapFixed[count0[*(byte*)start]++] = (uint)*start ^ 0x80000000U;
                }
                *--count8 = 0;
                for (uint* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    newArrayFixed[count8[*((byte*)start + 1)]++] = (int)*start;
                }
                *--count16 = 0;
                for (int* start = newArrayFixed, end = newArrayFixed + length; start != end; ++start)
                {
                    swapFixed[count16[*((byte*)start + 2)]++] = (uint)*start;
                }
                for (uint* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    newArrayFixed[--count24[*((byte*)start + 3)]] = (int)(*start ^ 0x80000000U);
                }
            }
            countBuffer.Push(ref count);
        }

        /// <summary>
        ///     数组排序
        /// </summary>
        /// <param name="arrayFixed">数组起始位置</param>
        /// <param name="arrayFixed">目标数组起始位置</param>
        /// <param name="swapFixed">临时数组起始位置</param>
        /// <param name="length">数组数据长度</param>
        public static unsafe void SortDesc(TmphIntSortIndex* arrayFixed, TmphUintSortIndex* swapFixed, int length)
        {
            bool isNewCount;
            var count = countBuffer.Get(out isNewCount);
            fixed (byte* countFixed = count)
            {
                int* count0 = (int*)countFixed + 1,
                    count8 = count0 + 256,
                    count16 = count0 + 256 * 2,
                    count24 = count0 + (256 * 3 - 1);
                if (!isNewCount) Array.Clear(count, sizeof(int), (256 * 4 - 1) * sizeof(int));
                for (TmphIntSortIndex* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    ++count0[*(byte*)start];
                    ++count8[*((byte*)start + 1)];
                    ++count16[*((byte*)start + 2)];
                    var value = *((byte*)start + 3);
                    value ^= 0x80;
                    ++count24[++value];
                }
                var index = *count0;
                for (int* start = count0 + 1, end = count0 + 255; start != end; *start++ = index) index += *start;
                index = *count8;
                for (int* start = count8 + 1, end = count8 + 255; start != end; *start++ = index) index += *start;
                index = *count16;
                for (int* start = count16 + 1, end = count16 + 255; start != end; *start++ = index) index += *start;
                *count24 = length;
                index = 0;
                for (int* start = count24 + 1, end = count24 + 256; start != end; *start++ = length - index)
                    index += *start;
                *--count0 = 0;
                for (TmphIntSortIndex* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    swapFixed[count0[*(byte*)start]++].Set((uint)(*start).Value ^ 0x80000000U, (*start).Index);
                }
                *--count8 = 0;
                for (TmphUintSortIndex* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    arrayFixed[count8[*((byte*)start + 1)]++].Set((int)(*start).Value, (*start).Index);
                }
                *--count16 = 0;
                for (TmphIntSortIndex* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    swapFixed[count16[*((byte*)start + 2)]++].Set((uint)(*start).Value, (*start).Index);
                }
                for (TmphUintSortIndex* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    arrayFixed[--count24[*((byte*)start + 3)]].Set((int)((*start).Value ^ 0x80000000U), (*start).Index);
                }
            }
            countBuffer.Push(ref count);
        }

        /// <summary>
        ///     数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">排序数据数量</param>
        public static unsafe void SortDesc(int[] array, int startIndex, int count)
        {
            var swapArray = new uint[count];
            fixed (int* arrayFixed = array)
            fixed (uint* swapFixed = swapArray)
            {
                var start = arrayFixed + startIndex;
                sortDesc(start, start, swapFixed, count);
            }
        }

        /// <summary>
        ///     数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">排序数据数量</param>
        /// <returns>排序后的新数组</returns>
        public static unsafe int[] GetSortDesc(int[] array, int startIndex, int count)
        {
            var newArray = new int[count];
            var swapArray = new uint[count];
            fixed (int* newArrayFixed = newArray, arrayFixed = array)
            fixed (uint* swapFixed = swapArray)
            {
                sortDesc(arrayFixed + startIndex, newArrayFixed, swapFixed, count);
            }
            return newArray;
        }

        /// <summary>
        ///     计数缓冲区
        /// </summary>
#if MONO
        private static TmphMemoryPool countBuffer64 = TmphMemoryPool.GetPool(256 * 8 * sizeof(int));
#else
        private static readonly TmphMemoryPool countBuffer64 = TmphMemoryPoolProxy.GetPool(256 * 8 * sizeof(int));
#endif

        /// <summary>
        ///     数组排序
        /// </summary>
        /// <param name="arrayFixed">数组起始位置</param>
        /// <param name="newArrayFixed">目标数组起始位置</param>
        /// <param name="swapFixed">临时数组起始位置</param>
        /// <param name="length">数组数据长度</param>
        private static unsafe void sort(ulong* arrayFixed, ulong* newArrayFixed, ulong* swapFixed, int length)
        {
            bool isNewCount;
            var count = countBuffer64.Get(out isNewCount);
            fixed (byte* countFixed = count)
            {
                int* count0 = (int*)countFixed + 1,
                    count8 = count0 + 256,
                    count16 = count0 + 256 * 2,
                    count24 = count0 + 256 * 3;
                int* count32 = count0 + 256 * 4,
                    count40 = count0 + 256 * 5,
                    count48 = count0 + 256 * 6,
                    count56 = count0 + (256 * 7 - 1);
                if (!isNewCount) Array.Clear(count, sizeof(int), (256 * 8 - 1) * sizeof(int));
                for (ulong* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    ++count0[*(byte*)start];
                    ++count8[*((byte*)start + 1)];
                    ++count16[*((byte*)start + 2)];
                    ++count24[*((byte*)start + 3)];
                    ++count32[*((byte*)start + 4)];
                    ++count40[*((byte*)start + 5)];
                    ++count48[*((byte*)start + 6)];
                    var value = *((byte*)start + 7);
                    ++count56[++value];
                }
                var index = *count0;
                for (int* start = count0 + 1, end = count0 + 255; start != end; *start++ = index) index += *start;
                index = *count8;
                for (int* start = count8 + 1, end = count8 + 255; start != end; *start++ = index) index += *start;
                index = *count16;
                for (int* start = count16 + 1, end = count16 + 255; start != end; *start++ = index) index += *start;
                index = *count24;
                for (int* start = count24 + 1, end = count24 + 255; start != end; *start++ = index) index += *start;
                index = *count32;
                for (int* start = count32 + 1, end = count32 + 255; start != end; *start++ = index) index += *start;
                index = *count40;
                for (int* start = count40 + 1, end = count40 + 255; start != end; *start++ = index) index += *start;
                index = *count48;
                for (int* start = count48 + 1, end = count48 + 255; start != end; *start++ = index) index += *start;
                *count56 = 0;
                index = *(count56 + 1);
                for (int* start = count56 + 2, end = count56 + 256; start != end; *start++ = index) index += *start;
                *--count0 = 0;
                for (ulong* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    swapFixed[count0[*(byte*)start]++] = *start;
                }
                *--count8 = 0;
                for (ulong* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    newArrayFixed[count8[*((byte*)start + 1)]++] = *start;
                }
                *--count16 = 0;
                for (ulong* start = newArrayFixed, end = newArrayFixed + length; start != end; ++start)
                {
                    swapFixed[count16[*((byte*)start + 2)]++] = *start;
                }
                *--count24 = 0;
                for (ulong* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    newArrayFixed[count24[*((byte*)start + 3)]++] = *start;
                }
                *--count32 = 0;
                for (ulong* start = newArrayFixed, end = newArrayFixed + length; start != end; ++start)
                {
                    swapFixed[count32[*((byte*)start + 4)]++] = *start;
                }
                *--count40 = 0;
                for (ulong* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    newArrayFixed[count40[*((byte*)start + 5)]++] = *start;
                }
                *--count48 = 0;
                for (ulong* start = newArrayFixed, end = newArrayFixed + length; start != end; ++start)
                {
                    swapFixed[count48[*((byte*)start + 6)]++] = *start;
                }
                for (ulong* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    newArrayFixed[count56[*((byte*)start + 7)]++] = *start;
                }
            }
            countBuffer64.Push(ref count);
        }

        /// <summary>
        ///     数组排序
        /// </summary>
        /// <param name="arrayFixed">数组起始位置</param>
        /// <param name="arrayFixed">目标数组起始位置</param>
        /// <param name="swapFixed">临时数组起始位置</param>
        /// <param name="length">数组数据长度</param>
        public static unsafe void Sort(ulongSortIndex* arrayFixed, ulongSortIndex* swapFixed, int length)
        {
            bool isNewCount;
            var count = countBuffer64.Get(out isNewCount);
            fixed (byte* countFixed = count)
            {
                int* count0 = (int*)countFixed + 1,
                    count8 = count0 + 256,
                    count16 = count0 + 256 * 2,
                    count24 = count0 + 256 * 3;
                int* count32 = count0 + 256 * 4,
                    count40 = count0 + 256 * 5,
                    count48 = count0 + 256 * 6,
                    count56 = count0 + (256 * 7 - 1);
                if (!isNewCount) Array.Clear(count, sizeof(int), (256 * 8 - 1) * sizeof(int));
                for (ulongSortIndex* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    ++count0[*(byte*)start];
                    ++count8[*((byte*)start + 1)];
                    ++count16[*((byte*)start + 2)];
                    ++count24[*((byte*)start + 3)];
                    ++count32[*((byte*)start + 4)];
                    ++count40[*((byte*)start + 5)];
                    ++count48[*((byte*)start + 6)];
                    var value = *((byte*)start + 7);
                    ++count56[++value];
                }
                var index = *count0;
                for (int* start = count0 + 1, end = count0 + 255; start != end; *start++ = index) index += *start;
                index = *count8;
                for (int* start = count8 + 1, end = count8 + 255; start != end; *start++ = index) index += *start;
                index = *count16;
                for (int* start = count16 + 1, end = count16 + 255; start != end; *start++ = index) index += *start;
                index = *count24;
                for (int* start = count24 + 1, end = count24 + 255; start != end; *start++ = index) index += *start;
                index = *count32;
                for (int* start = count32 + 1, end = count32 + 255; start != end; *start++ = index) index += *start;
                index = *count40;
                for (int* start = count40 + 1, end = count40 + 255; start != end; *start++ = index) index += *start;
                index = *count48;
                for (int* start = count48 + 1, end = count48 + 255; start != end; *start++ = index) index += *start;
                *count56 = 0;
                index = *(count56 + 1);
                for (int* start = count56 + 2, end = count56 + 256; start != end; *start++ = index) index += *start;
                *--count0 = 0;
                for (ulongSortIndex* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    swapFixed[count0[*(byte*)start]++].Set((*start).Value, (*start).Index);
                }
                *--count8 = 0;
                for (ulongSortIndex* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    arrayFixed[count8[*((byte*)start + 1)]++].Set((*start).Value, (*start).Index);
                }
                *--count16 = 0;
                for (ulongSortIndex* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    swapFixed[count16[*((byte*)start + 2)]++].Set((*start).Value, (*start).Index);
                }
                *--count24 = 0;
                for (ulongSortIndex* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    arrayFixed[count24[*((byte*)start + 3)]++].Set((*start).Value, (*start).Index);
                }
                *--count32 = 0;
                for (ulongSortIndex* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    swapFixed[count32[*((byte*)start + 4)]++].Set((*start).Value, (*start).Index);
                }
                *--count40 = 0;
                for (ulongSortIndex* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    arrayFixed[count40[*((byte*)start + 5)]++].Set((*start).Value, (*start).Index);
                }
                *--count48 = 0;
                for (ulongSortIndex* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    swapFixed[count48[*((byte*)start + 6)]++].Set((*start).Value, (*start).Index);
                }
                for (ulongSortIndex* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    arrayFixed[count56[*((byte*)start + 7)]++].Set((*start).Value, (*start).Index);
                }
            }
            countBuffer64.Push(ref count);
        }

        /// <summary>
        ///     数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">排序数据数量</param>
        public static unsafe void Sort(ulong[] array, int startIndex, int count)
        {
            var swapArray = new ulong[count];
            fixed (ulong* arrayFixed = array, swapFixed = swapArray)
            {
                var start = arrayFixed + startIndex;
                sort(start, start, swapFixed, count);
            }
        }

        /// <summary>
        ///     数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">排序数据数量</param>
        /// <returns>排序后的新数组</returns>
        public static unsafe ulong[] GetSort(ulong[] array, int startIndex, int count)
        {
            ulong[] newArray = new ulong[count], swapArray = new ulong[count];
            fixed (ulong* newArrayFixed = newArray, arrayFixed = array, swapFixed = swapArray)
            {
                sort(arrayFixed + startIndex, newArrayFixed, swapFixed, count);
            }
            return newArray;
        }

        /// <summary>
        ///     数组排序
        /// </summary>
        /// <param name="arrayFixed">数组起始位置</param>
        /// <param name="newArrayFixed">目标数组起始位置</param>
        /// <param name="swapFixed">临时数组起始位置</param>
        /// <param name="length">数组数据长度</param>
        private static unsafe void sortDesc(ulong* arrayFixed, ulong* newArrayFixed, ulong* swapFixed, int length)
        {
            bool isNewCount;
            var count = countBuffer64.Get(out isNewCount);
            fixed (byte* countFixed = count)
            {
                int* count0 = (int*)countFixed + 1,
                    count8 = count0 + 256,
                    count16 = count0 + 256 * 2,
                    count24 = count0 + 256 * 3;
                int* count32 = count0 + 256 * 4,
                    count40 = count0 + 256 * 5,
                    count48 = count0 + 256 * 6,
                    count56 = count0 + (256 * 7 - 1);
                if (!isNewCount) Array.Clear(count, sizeof(int), (256 * 8 - 1) * sizeof(int));
                for (ulong* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    ++count0[*(byte*)start];
                    ++count8[*((byte*)start + 1)];
                    ++count16[*((byte*)start + 2)];
                    ++count24[*((byte*)start + 3)];
                    ++count32[*((byte*)start + 4)];
                    ++count40[*((byte*)start + 5)];
                    ++count48[*((byte*)start + 6)];
                    var value = *((byte*)start + 7);
                    ++count56[++value];
                }
                var index = *count0;
                for (int* start = count0 + 1, end = count0 + 255; start != end; *start++ = index) index += *start;
                index = *count8;
                for (int* start = count8 + 1, end = count8 + 255; start != end; *start++ = index) index += *start;
                index = *count16;
                for (int* start = count16 + 1, end = count16 + 255; start != end; *start++ = index) index += *start;
                index = *count24;
                for (int* start = count24 + 1, end = count24 + 255; start != end; *start++ = index) index += *start;
                index = *count32;
                for (int* start = count32 + 1, end = count32 + 255; start != end; *start++ = index) index += *start;
                index = *count40;
                for (int* start = count40 + 1, end = count40 + 255; start != end; *start++ = index) index += *start;
                index = *count48;
                for (int* start = count48 + 1, end = count48 + 255; start != end; *start++ = index) index += *start;
                *count56 = length;
                index = 0;
                for (int* start = count56 + 1, end = count56 + 256; start != end; *start++ = length - index)
                    index += *start;
                *--count0 = 0;
                for (ulong* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    swapFixed[count0[*(byte*)start]++] = *start;
                }
                *--count8 = 0;
                for (ulong* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    newArrayFixed[count8[*((byte*)start + 1)]++] = *start;
                }
                *--count16 = 0;
                for (ulong* start = newArrayFixed, end = newArrayFixed + length; start != end; ++start)
                {
                    swapFixed[count16[*((byte*)start + 2)]++] = *start;
                }
                *--count24 = 0;
                for (ulong* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    newArrayFixed[count24[*((byte*)start + 3)]++] = *start;
                }
                *--count32 = 0;
                for (ulong* start = newArrayFixed, end = newArrayFixed + length; start != end; ++start)
                {
                    swapFixed[count32[*((byte*)start + 4)]++] = *start;
                }
                *--count40 = 0;
                for (ulong* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    newArrayFixed[count40[*((byte*)start + 5)]++] = *start;
                }
                *--count48 = 0;
                for (ulong* start = newArrayFixed, end = newArrayFixed + length; start != end; ++start)
                {
                    swapFixed[count48[*((byte*)start + 6)]++] = *start;
                }
                for (ulong* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    newArrayFixed[--count56[*((byte*)start + 7)]] = *start;
                }
            }
            countBuffer64.Push(ref count);
        }

        /// <summary>
        ///     数组排序
        /// </summary>
        /// <param name="arrayFixed">数组起始位置</param>
        /// <param name="arrayFixed">目标数组起始位置</param>
        /// <param name="swapFixed">临时数组起始位置</param>
        /// <param name="length">数组数据长度</param>
        public static unsafe void SortDesc(ulongSortIndex* arrayFixed, ulongSortIndex* swapFixed, int length)
        {
            bool isNewCount;
            var count = countBuffer64.Get(out isNewCount);
            fixed (byte* countFixed = count)
            {
                int* count0 = (int*)countFixed + 1,
                    count8 = count0 + 256,
                    count16 = count0 + 256 * 2,
                    count24 = count0 + 256 * 3;
                int* count32 = count0 + 256 * 4,
                    count40 = count0 + 256 * 5,
                    count48 = count0 + 256 * 6,
                    count56 = count0 + (256 * 7 - 1);
                if (!isNewCount) Array.Clear(count, sizeof(int), (256 * 8 - 1) * sizeof(int));
                for (ulongSortIndex* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    ++count0[*(byte*)start];
                    ++count8[*((byte*)start + 1)];
                    ++count16[*((byte*)start + 2)];
                    ++count24[*((byte*)start + 3)];
                    ++count32[*((byte*)start + 4)];
                    ++count40[*((byte*)start + 5)];
                    ++count48[*((byte*)start + 6)];
                    var value = *((byte*)start + 7);
                    ++count56[++value];
                }
                var index = *count0;
                for (int* start = count0 + 1, end = count0 + 255; start != end; *start++ = index) index += *start;
                index = *count8;
                for (int* start = count8 + 1, end = count8 + 255; start != end; *start++ = index) index += *start;
                index = *count16;
                for (int* start = count16 + 1, end = count16 + 255; start != end; *start++ = index) index += *start;
                index = *count24;
                for (int* start = count24 + 1, end = count24 + 255; start != end; *start++ = index) index += *start;
                index = *count32;
                for (int* start = count32 + 1, end = count32 + 255; start != end; *start++ = index) index += *start;
                index = *count40;
                for (int* start = count40 + 1, end = count40 + 255; start != end; *start++ = index) index += *start;
                index = *count48;
                for (int* start = count48 + 1, end = count48 + 255; start != end; *start++ = index) index += *start;
                *count56 = length;
                index = 0;
                for (int* start = count56 + 1, end = count56 + 256; start != end; *start++ = length - index)
                    index += *start;
                *--count0 = 0;
                for (ulongSortIndex* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    swapFixed[count0[*(byte*)start]++].Set((*start).Value, (*start).Index);
                }
                *--count8 = 0;
                for (ulongSortIndex* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    arrayFixed[count8[*((byte*)start + 1)]++].Set((*start).Value, (*start).Index);
                }
                *--count16 = 0;
                for (ulongSortIndex* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    swapFixed[count16[*((byte*)start + 2)]++].Set((*start).Value, (*start).Index);
                }
                *--count24 = 0;
                for (ulongSortIndex* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    arrayFixed[count24[*((byte*)start + 3)]++].Set((*start).Value, (*start).Index);
                }
                *--count32 = 0;
                for (ulongSortIndex* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    swapFixed[count32[*((byte*)start + 4)]++].Set((*start).Value, (*start).Index);
                }
                *--count40 = 0;
                for (ulongSortIndex* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    arrayFixed[count40[*((byte*)start + 5)]++].Set((*start).Value, (*start).Index);
                }
                *--count48 = 0;
                for (ulongSortIndex* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    swapFixed[count48[*((byte*)start + 6)]++].Set((*start).Value, (*start).Index);
                }
                for (ulongSortIndex* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    arrayFixed[--count56[*((byte*)start + 7)]].Set((*start).Value, (*start).Index);
                }
            }
            countBuffer64.Push(ref count);
        }

        /// <summary>
        ///     数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">排序数据数量</param>
        public static unsafe void SortDesc(ulong[] array, int startIndex, int count)
        {
            var swapArray = new ulong[count];
            fixed (ulong* arrayFixed = array, swapFixed = swapArray)
            {
                var start = arrayFixed + startIndex;
                sortDesc(start, start, swapFixed, count);
            }
        }

        /// <summary>
        ///     数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">排序数据数量</param>
        /// <returns>排序后的新数组</returns>
        public static unsafe ulong[] GetSortDesc(ulong[] array, int startIndex, int count)
        {
            ulong[] newArray = new ulong[count], swapArray = new ulong[count];
            fixed (ulong* newArrayFixed = newArray, arrayFixed = array, swapFixed = swapArray)
            {
                sortDesc(arrayFixed + startIndex, newArrayFixed, swapFixed, count);
            }
            return newArray;
        }

        /// <summary>
        ///     数组排序
        /// </summary>
        /// <param name="arrayFixed">数组起始位置</param>
        /// <param name="newArrayFixed">目标数组起始位置</param>
        /// <param name="swapFixed">临时数组起始位置</param>
        /// <param name="length">数组数据长度</param>
        private static unsafe void sort(long* arrayFixed, long* newArrayFixed, ulong* swapFixed, int length)
        {
            bool isNewCount;
            var count = countBuffer64.Get(out isNewCount);
            fixed (byte* countFixed = count)
            {
                int* count0 = (int*)countFixed + 1,
                    count8 = count0 + 256,
                    count16 = count0 + 256 * 2,
                    count24 = count0 + 256 * 3;
                int* count32 = count0 + 256 * 4,
                    count40 = count0 + 256 * 5,
                    count48 = count0 + 256 * 6,
                    count56 = count0 + (256 * 7 - 1);
                if (!isNewCount) Array.Clear(count, sizeof(int), (256 * 8 - 1) * sizeof(int));
                for (long* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    ++count0[*(byte*)start];
                    ++count8[*((byte*)start + 1)];
                    ++count16[*((byte*)start + 2)];
                    ++count24[*((byte*)start + 3)];
                    ++count32[*((byte*)start + 4)];
                    ++count40[*((byte*)start + 5)];
                    ++count48[*((byte*)start + 6)];
                    var value = *((byte*)start + 7);
                    value ^= 0x80;
                    ++count56[++value];
                }
                var index = *count0;
                for (int* start = count0 + 1, end = count0 + 255; start != end; *start++ = index) index += *start;
                index = *count8;
                for (int* start = count8 + 1, end = count8 + 255; start != end; *start++ = index) index += *start;
                index = *count16;
                for (int* start = count16 + 1, end = count16 + 255; start != end; *start++ = index) index += *start;
                index = *count24;
                for (int* start = count24 + 1, end = count24 + 255; start != end; *start++ = index) index += *start;
                index = *count32;
                for (int* start = count32 + 1, end = count32 + 255; start != end; *start++ = index) index += *start;
                index = *count40;
                for (int* start = count40 + 1, end = count40 + 255; start != end; *start++ = index) index += *start;
                index = *count48;
                for (int* start = count48 + 1, end = count48 + 255; start != end; *start++ = index) index += *start;
                *count56 = 0;
                index = *(count56 + 1);
                for (int* start = count56 + 2, end = count56 + 256; start != end; *start++ = index) index += *start;
                *--count0 = 0;
                for (long* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    swapFixed[count0[*(byte*)start]++] = (ulong)*start ^ 0x8000000000000000UL;
                    //byte* low = (byte*)(swapFixed + count0[*(byte*)start]++);
                    //*(uint*)low = *(uint*)start;
                    //*((uint*)(low + sizeof(uint))) = *((uint*)start + 1) ^ 0x80000000U;
                }
                *--count8 = 0;
                for (ulong* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    newArrayFixed[count8[*((byte*)start + 1)]++] = (long)*start;
                }
                *--count16 = 0;
                for (long* start = newArrayFixed, end = newArrayFixed + length; start != end; ++start)
                {
                    swapFixed[count16[*((byte*)start + 2)]++] = (ulong)*start;
                }
                *--count24 = 0;
                for (ulong* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    newArrayFixed[count24[*((byte*)start + 3)]++] = (long)*start;
                }
                *--count32 = 0;
                for (long* start = newArrayFixed, end = newArrayFixed + length; start != end; ++start)
                {
                    swapFixed[count32[*((byte*)start + 4)]++] = (ulong)*start;
                }
                *--count40 = 0;
                for (ulong* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    newArrayFixed[count40[*((byte*)start + 5)]++] = (long)*start;
                }
                *--count48 = 0;
                for (long* start = newArrayFixed, end = newArrayFixed + length; start != end; ++start)
                {
                    swapFixed[count48[*((byte*)start + 6)]++] = (ulong)*start;
                }
                for (ulong* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    newArrayFixed[count56[*((byte*)start + 7)]++] = (long)(*start ^ 0x8000000000000000UL);
                    //byte* low = (byte*)(newArrayFixed + count56[*((byte*)start + 7)]++);
                    //*(uint*)low = *(uint*)start;
                    //*((uint*)(low + sizeof(uint))) = *((uint*)start + 1) ^ 0x80000000U;
                }
            }
            countBuffer64.Push(ref count);
        }

        /// <summary>
        ///     数组排序
        /// </summary>
        /// <param name="arrayFixed">数组起始位置</param>
        /// <param name="arrayFixed">目标数组起始位置</param>
        /// <param name="swapFixed">临时数组起始位置</param>
        /// <param name="length">数组数据长度</param>
        public static unsafe void Sort(TmphLongSortIndex* arrayFixed, ulongSortIndex* swapFixed, int length)
        {
            bool isNewCount;
            var count = countBuffer64.Get(out isNewCount);
            fixed (byte* countFixed = count)
            {
                int* count0 = (int*)countFixed + 1,
                    count8 = count0 + 256,
                    count16 = count0 + 256 * 2,
                    count24 = count0 + 256 * 3;
                int* count32 = count0 + 256 * 4,
                    count40 = count0 + 256 * 5,
                    count48 = count0 + 256 * 6,
                    count56 = count0 + (256 * 7 - 1);
                if (!isNewCount) Array.Clear(count, sizeof(int), (256 * 8 - 1) * sizeof(int));
                for (TmphLongSortIndex* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    ++count0[*(byte*)start];
                    ++count8[*((byte*)start + 1)];
                    ++count16[*((byte*)start + 2)];
                    ++count24[*((byte*)start + 3)];
                    ++count32[*((byte*)start + 4)];
                    ++count40[*((byte*)start + 5)];
                    ++count48[*((byte*)start + 6)];
                    var value = *((byte*)start + 7);
                    value ^= 0x80;
                    ++count56[++value];
                }
                var index = *count0;
                for (int* start = count0 + 1, end = count0 + 255; start != end; *start++ = index) index += *start;
                index = *count8;
                for (int* start = count8 + 1, end = count8 + 255; start != end; *start++ = index) index += *start;
                index = *count16;
                for (int* start = count16 + 1, end = count16 + 255; start != end; *start++ = index) index += *start;
                index = *count24;
                for (int* start = count24 + 1, end = count24 + 255; start != end; *start++ = index) index += *start;
                index = *count32;
                for (int* start = count32 + 1, end = count32 + 255; start != end; *start++ = index) index += *start;
                index = *count40;
                for (int* start = count40 + 1, end = count40 + 255; start != end; *start++ = index) index += *start;
                index = *count48;
                for (int* start = count48 + 1, end = count48 + 255; start != end; *start++ = index) index += *start;
                *count56 = 0;
                index = *(count56 + 1);
                for (int* start = count56 + 2, end = count56 + 256; start != end; *start++ = index) index += *start;
                *--count0 = 0;
                for (TmphLongSortIndex* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    swapFixed[count0[*(byte*)start]++].Set((ulong)(*start).Value ^ 0x8000000000000000UL,
                        (*start).Index);
                    //byte* low = (byte*)(swapFixed + count0[*(byte*)start]++);
                    //*(uint*)low = *(uint*)start;
                    //*((uint*)(low + sizeof(uint))) = *((uint*)start + 1) ^ 0x80000000U;
                    //(*((ulongSortIndex*)low)).Index = (*start).Index;
                }
                *--count8 = 0;
                for (ulongSortIndex* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    arrayFixed[count8[*((byte*)start + 1)]++].Set((long)(*start).Value, (*start).Index);
                }
                *--count16 = 0;
                for (TmphLongSortIndex* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    swapFixed[count16[*((byte*)start + 2)]++].Set((ulong)(*start).Value, (*start).Index);
                }
                *--count24 = 0;
                for (ulongSortIndex* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    arrayFixed[count24[*((byte*)start + 3)]++].Set((long)(*start).Value, (*start).Index);
                }
                *--count32 = 0;
                for (TmphLongSortIndex* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    swapFixed[count32[*((byte*)start + 4)]++].Set((ulong)(*start).Value, (*start).Index);
                }
                *--count40 = 0;
                for (ulongSortIndex* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    arrayFixed[count40[*((byte*)start + 5)]++].Set((long)(*start).Value, (*start).Index);
                }
                *--count48 = 0;
                for (TmphLongSortIndex* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    swapFixed[count48[*((byte*)start + 6)]++].Set((ulong)(*start).Value, (*start).Index);
                }
                for (ulongSortIndex* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    arrayFixed[count56[*((byte*)start + 7)]++].Set((long)((*start).Value ^ 0x8000000000000000UL),
                        (*start).Index);
                    //byte* low = (byte*)(arrayFixed + count56[*((byte*)start + 7)]++);
                    //*(uint*)low = *(uint*)start;
                    //*((uint*)(low + sizeof(uint))) = *((uint*)start + 1) ^ 0x80000000U;
                    //(*((longSortIndex*)low)).Index = (*start).Index;
                }
            }
            countBuffer64.Push(ref count);
        }

        /// <summary>
        ///     数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">排序数据数量</param>
        public static unsafe void Sort(long[] array, int startIndex, int count)
        {
            var swapArray = new ulong[count];
            fixed (long* arrayFixed = array)
            fixed (ulong* swapFixed = swapArray)
            {
                var start = arrayFixed + startIndex;
                sort(start, start, swapFixed, count);
            }
        }

        /// <summary>
        ///     数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">排序数据数量</param>
        /// <returns>排序后的新数组</returns>
        public static unsafe long[] GetSort(long[] array, int startIndex, int count)
        {
            var newArray = new long[count];
            var swapArray = new ulong[count];
            fixed (long* newArrayFixed = newArray, arrayFixed = array)
            fixed (ulong* swapFixed = swapArray)
            {
                sort(arrayFixed + startIndex, newArrayFixed, swapFixed, count);
            }
            return newArray;
        }

        /// <summary>
        ///     数组排序
        /// </summary>
        /// <param name="arrayFixed">数组起始位置</param>
        /// <param name="newArrayFixed">目标数组起始位置</param>
        /// <param name="swapFixed">临时数组起始位置</param>
        /// <param name="length">数组数据长度</param>
        private static unsafe void sortDesc(long* arrayFixed, long* newArrayFixed, ulong* swapFixed, int length)
        {
            bool isNewCount;
            var count = countBuffer64.Get(out isNewCount);
            fixed (byte* countFixed = count)
            {
                int* count0 = (int*)countFixed + 1,
                    count8 = count0 + 256,
                    count16 = count0 + 256 * 2,
                    count24 = count0 + 256 * 3;
                int* count32 = count0 + 256 * 4,
                    count40 = count0 + 256 * 5,
                    count48 = count0 + 256 * 6,
                    count56 = count0 + (256 * 7 - 1);
                if (!isNewCount) Array.Clear(count, sizeof(int), (256 * 8 - 1) * sizeof(int));
                for (long* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    ++count0[*(byte*)start];
                    ++count8[*((byte*)start + 1)];
                    ++count16[*((byte*)start + 2)];
                    ++count24[*((byte*)start + 3)];
                    ++count32[*((byte*)start + 4)];
                    ++count40[*((byte*)start + 5)];
                    ++count48[*((byte*)start + 6)];
                    var value = *((byte*)start + 7);
                    value ^= 0x80;
                    ++count56[++value];
                }
                var index = *count0;
                for (int* start = count0 + 1, end = count0 + 255; start != end; *start++ = index) index += *start;
                index = *count8;
                for (int* start = count8 + 1, end = count8 + 255; start != end; *start++ = index) index += *start;
                index = *count16;
                for (int* start = count16 + 1, end = count16 + 255; start != end; *start++ = index) index += *start;
                index = *count24;
                for (int* start = count24 + 1, end = count24 + 255; start != end; *start++ = index) index += *start;
                index = *count32;
                for (int* start = count32 + 1, end = count32 + 255; start != end; *start++ = index) index += *start;
                index = *count40;
                for (int* start = count40 + 1, end = count40 + 255; start != end; *start++ = index) index += *start;
                index = *count48;
                for (int* start = count48 + 1, end = count48 + 255; start != end; *start++ = index) index += *start;
                *count56 = length;
                index = 0;
                for (int* start = count56 + 1, end = count56 + 256; start != end; *start++ = length - index)
                    index += *start;
                *--count0 = 0;
                for (long* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    swapFixed[count0[*(byte*)start]++] = (ulong)*start ^ 0x8000000000000000UL;
                    //byte* low = (byte*)(swapFixed + count0[*(byte*)start]++);
                    //*(uint*)low = *(uint*)start;
                    //*((uint*)(low + sizeof(uint))) = *((uint*)start + 1) ^ 0x80000000U;
                }
                *--count8 = 0;
                for (ulong* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    newArrayFixed[count8[*((byte*)start + 1)]++] = (long)*start;
                }
                *--count16 = 0;
                for (long* start = newArrayFixed, end = newArrayFixed + length; start != end; ++start)
                {
                    swapFixed[count16[*((byte*)start + 2)]++] = (ulong)*start;
                }
                *--count24 = 0;
                for (ulong* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    newArrayFixed[count24[*((byte*)start + 3)]++] = (long)*start;
                }
                *--count32 = 0;
                for (long* start = newArrayFixed, end = newArrayFixed + length; start != end; ++start)
                {
                    swapFixed[count32[*((byte*)start + 4)]++] = (ulong)*start;
                }
                *--count40 = 0;
                for (ulong* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    newArrayFixed[count40[*((byte*)start + 5)]++] = (long)*start;
                }
                *--count48 = 0;
                for (long* start = newArrayFixed, end = newArrayFixed + length; start != end; ++start)
                {
                    swapFixed[count48[*((byte*)start + 6)]++] = (ulong)*start;
                }
                for (ulong* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    newArrayFixed[--count56[*((byte*)start + 7)]] = (long)(*start ^ 0x8000000000000000UL);
                    //byte* low = (byte*)(newArrayFixed + --count56[*((byte*)start + 7)]);
                    //*(uint*)low = *(uint*)start;
                    //*((uint*)(low + sizeof(uint))) = *((uint*)start + 1) ^ 0x80000000U;
                }
            }
            countBuffer64.Push(ref count);
        }

        /// <summary>
        ///     数组排序
        /// </summary>
        /// <param name="arrayFixed">数组起始位置</param>
        /// <param name="arrayFixed">目标数组起始位置</param>
        /// <param name="swapFixed">临时数组起始位置</param>
        /// <param name="length">数组数据长度</param>
        public static unsafe void SortDesc(TmphLongSortIndex* arrayFixed, ulongSortIndex* swapFixed, int length)
        {
            bool isNewCount;
            var count = countBuffer64.Get(out isNewCount);
            fixed (byte* countFixed = count)
            {
                int* count0 = (int*)countFixed + 1,
                    count8 = count0 + 256,
                    count16 = count0 + 256 * 2,
                    count24 = count0 + 256 * 3;
                int* count32 = count0 + 256 * 4,
                    count40 = count0 + 256 * 5,
                    count48 = count0 + 256 * 6,
                    count56 = count0 + (256 * 7 - 1);
                if (!isNewCount) Array.Clear(count, sizeof(int), (256 * 8 - 1) * sizeof(int));
                for (TmphLongSortIndex* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    ++count0[*(byte*)start];
                    ++count8[*((byte*)start + 1)];
                    ++count16[*((byte*)start + 2)];
                    ++count24[*((byte*)start + 3)];
                    ++count32[*((byte*)start + 4)];
                    ++count40[*((byte*)start + 5)];
                    ++count48[*((byte*)start + 6)];
                    var value = *((byte*)start + 7);
                    value ^= 0x80;
                    ++count56[++value];
                }
                var index = *count0;
                for (int* start = count0 + 1, end = count0 + 255; start != end; *start++ = index) index += *start;
                index = *count8;
                for (int* start = count8 + 1, end = count8 + 255; start != end; *start++ = index) index += *start;
                index = *count16;
                for (int* start = count16 + 1, end = count16 + 255; start != end; *start++ = index) index += *start;
                index = *count24;
                for (int* start = count24 + 1, end = count24 + 255; start != end; *start++ = index) index += *start;
                index = *count32;
                for (int* start = count32 + 1, end = count32 + 255; start != end; *start++ = index) index += *start;
                index = *count40;
                for (int* start = count40 + 1, end = count40 + 255; start != end; *start++ = index) index += *start;
                index = *count48;
                for (int* start = count48 + 1, end = count48 + 255; start != end; *start++ = index) index += *start;
                *count56 = length;
                index = 0;
                for (int* start = count56 + 1, end = count56 + 256; start != end; *start++ = length - index)
                    index += *start;
                *--count0 = 0;
                for (TmphLongSortIndex* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    swapFixed[count0[*(byte*)start]++].Set((ulong)(*start).Value ^ 0x8000000000000000UL,
                        (*start).Index);
                    //byte* low = (byte*)(swapFixed + count0[*(byte*)start]++);
                    //*(uint*)low = *(uint*)start;
                    //*((uint*)(low + sizeof(uint))) = *((uint*)start + 1) ^ 0x80000000U;
                    //(*((ulongSortIndex*)low)).Index = (*start).Index;
                }
                *--count8 = 0;
                for (ulongSortIndex* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    arrayFixed[count8[*((byte*)start + 1)]++].Set((long)(*start).Value, (*start).Index);
                }
                *--count16 = 0;
                for (TmphLongSortIndex* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    swapFixed[count16[*((byte*)start + 2)]++].Set((ulong)(*start).Value, (*start).Index);
                }
                *--count24 = 0;
                for (ulongSortIndex* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    arrayFixed[count24[*((byte*)start + 3)]++].Set((long)(*start).Value, (*start).Index);
                }
                *--count32 = 0;
                for (TmphLongSortIndex* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    swapFixed[count32[*((byte*)start + 4)]++].Set((ulong)(*start).Value, (*start).Index);
                }
                *--count40 = 0;
                for (ulongSortIndex* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    arrayFixed[count40[*((byte*)start + 5)]++].Set((long)(*start).Value, (*start).Index);
                }
                *--count48 = 0;
                for (TmphLongSortIndex* start = arrayFixed, end = arrayFixed + length; start != end; ++start)
                {
                    swapFixed[count48[*((byte*)start + 6)]++].Set((ulong)(*start).Value, (*start).Index);
                }
                for (ulongSortIndex* start = swapFixed, end = swapFixed + length; start != end; ++start)
                {
                    arrayFixed[--count56[*((byte*)start + 7)]].Set((long)((*start).Value ^ 0x8000000000000000UL),
                        (*start).Index);
                    //byte* low = (byte*)(arrayFixed + --count56[*((byte*)start + 7)]);
                    //*(uint*)low = *(uint*)start;
                    //*((uint*)(low + sizeof(uint))) = *((uint*)start + 1) ^ 0x80000000U;
                    //(*((longSortIndex*)low)).Index = (*start).Index;
                }
            }
            countBuffer64.Push(ref count);
        }

        /// <summary>
        ///     数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">排序数据数量</param>
        public static unsafe void SortDesc(long[] array, int startIndex, int count)
        {
            var swapArray = new ulong[count];
            fixed (long* arrayFixed = array)
            fixed (ulong* swapFixed = swapArray)
            {
                var start = arrayFixed + startIndex;
                sortDesc(start, start, swapFixed, count);
            }
        }

        /// <summary>
        ///     数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">排序数据数量</param>
        /// <returns>排序后的新数组</returns>
        public static unsafe long[] GetSortDesc(long[] array, int startIndex, int count)
        {
            var newArray = new long[count];
            var swapArray = new ulong[count];
            fixed (long* newArrayFixed = newArray, arrayFixed = array)
            fixed (ulong* swapFixed = swapArray)
            {
                sortDesc(arrayFixed + startIndex, newArrayFixed, swapFixed, count);
            }
            return newArray;
        }

        /// <summary>
        ///     数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">排序数据数量</param>
        public static unsafe void Sort(DateTime[] array, int startIndex, int count)
        {
            var swapArray = new ulong[count];
            fixed (DateTime* arrayFixed = array)
            fixed (ulong* swapFixed = swapArray)
            {
                var start = (long*)arrayFixed + startIndex;
                sort(start, start, swapFixed, count);
            }
        }

        /// <summary>
        ///     数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">排序数据数量</param>
        /// <returns>排序后的新数组</returns>
        public static unsafe DateTime[] GetSort(DateTime[] array, int startIndex, int count)
        {
            var newArray = new DateTime[count];
            var swapArray = new ulong[count];
            fixed (DateTime* newArrayFixed = newArray, arrayFixed = array)
            fixed (ulong* swapFixed = swapArray)
            {
                sort((long*)arrayFixed + startIndex, (long*)newArrayFixed, swapFixed, count);
            }
            return newArray;
        }

        /// <summary>
        ///     数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">排序数据数量</param>
        public static unsafe void SortDesc(DateTime[] array, int startIndex, int count)
        {
            var swapArray = new ulong[count];
            fixed (DateTime* arrayFixed = array)
            fixed (ulong* swapFixed = swapArray)
            {
                var start = (long*)arrayFixed + startIndex;
                sortDesc(start, start, swapFixed, count);
            }
        }

        /// <summary>
        ///     数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">排序数据数量</param>
        /// <returns>排序后的新数组</returns>
        public static unsafe DateTime[] GetSortDesc(DateTime[] array, int startIndex, int count)
        {
            var newArray = new DateTime[count];
            var swapArray = new ulong[count];
            fixed (DateTime* newArrayFixed = newArray, arrayFixed = array)
            fixed (ulong* swapFixed = swapArray)
            {
                sortDesc((long*)arrayFixed + startIndex, (long*)newArrayFixed, swapFixed, count);
            }
            return newArray;
        }
    }
}