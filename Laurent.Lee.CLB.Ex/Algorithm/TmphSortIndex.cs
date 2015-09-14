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
using System.Runtime.InteropServices;

namespace Laurent.Lee.CLB.Algorithm
{
    /// <summary>
    ///     排序索引
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct TmphDateTimeSortIndex
    {
        /// <summary>
        ///     数值
        /// </summary>
        [FieldOffset(0)]
        public DateTime Value;

        /// <summary>
        ///     位置索引
        /// </summary>
        [FieldOffset(sizeof(long))]
        public int Index;

        /// <summary>
        ///     设置排序索引
        /// </summary>
        /// <param name="value">数值</param>
        /// <param name="index">位置索引</param>
        public void Set(DateTime value, int index)
        {
            Value = value;
            Index = index;
        }

        /// <summary>
        ///     根据数组获取排序索引
        /// </summary>
        /// <typeparam name="TValueType">数组类型</typeparam>
        /// <param name="indexFixed">排序索引数组</param>
        /// <param name="values">数组</param>
        /// <param name="getValue">数据排序值获取器</param>
        public static unsafe void Create<TValueType>(TmphDateTimeSortIndex* indexFixed, TValueType[] values,
            Func<TValueType, DateTime> getValue)
        {
            var index = 0;
            foreach (var value in values) (*indexFixed++).Set(getValue(value), index++);
        }

        /// <summary>
        ///     根据数组获取排序索引
        /// </summary>
        /// <typeparam name="TValueType">数组类型</typeparam>
        /// <param name="indexFixed">排序索引数组</param>
        /// <param name="values">数组</param>
        /// <param name="getValue">数据排序值获取器</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">排序数据数量</param>
        public static unsafe void Create<TValueType>(TmphDateTimeSortIndex* indexFixed, TValueType[] values,
            Func<TValueType, DateTime> getValue, int startIndex, int count)
        {
            for (var endIndex = startIndex + count;
                startIndex != endIndex;
                (*indexFixed++).Set(getValue(values[startIndex]), startIndex++))
            {
            }
        }

        /// <summary>
        ///     根据排序索引获取数组
        /// </summary>
        /// <typeparam name="TValueType">数组类型</typeparam>
        /// <param name="indexFixed">排序索引数组</param>
        /// <param name="values">数组</param>
        /// <param name="count">排序数据数量</param>
        /// <returns>排序后的数组</returns>
        public static unsafe TValueType[] Create<TValueType>(TmphDateTimeSortIndex* indexFixed, TValueType[] values,
            int count)
        {
            var newValues = new TValueType[count];
            for (var index = 0; index != count; ++index) newValues[index] = values[(*indexFixed++).Index];
            return newValues;
        }

        /// <summary>
        ///     根据数组获取排序索引
        /// </summary>
        /// <typeparam name="TValueType">数组类型</typeparam>
        /// <param name="indexFixed">排序索引数组</param>
        /// <param name="values">数组</param>
        /// <param name="getValue">数据排序值获取器</param>
        public static unsafe void Create<TValueType>(TmphLongSortIndex* indexFixed, TValueType[] values,
            Func<TValueType, DateTime> getValue)
        {
            var index = 0;
            foreach (var value in values) (*indexFixed++).Set(getValue(value).Ticks, index++);
        }

        /// <summary>
        ///     根据数组获取排序索引
        /// </summary>
        /// <typeparam name="TValueType">数组类型</typeparam>
        /// <param name="indexFixed">排序索引数组</param>
        /// <param name="values">数组</param>
        /// <param name="getValue">数据排序值获取器</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="count">排序数据数量</param>
        public static unsafe void Create<TValueType>(TmphLongSortIndex* indexFixed, TValueType[] values,
            Func<TValueType, DateTime> getValue, int startIndex, int count)
        {
            for (var endIndex = startIndex + count;
                startIndex != endIndex;
                (*indexFixed++).Set(getValue(values[startIndex]).Ticks, startIndex++))
            {
            }
        }
    }
}