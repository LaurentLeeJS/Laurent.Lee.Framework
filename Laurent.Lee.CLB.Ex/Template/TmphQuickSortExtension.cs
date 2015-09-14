using System;

/*Type:double,doubleSortIndex;float,floatSortIndex*/
/*Compare:,>,<;Desc,<,>*/

namespace Laurent.Lee.CLB.Template
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
        public static /*Type[0]*/double/*Type[0]*/[] sort/*Compare[0]*//*Compare[0]*/(this /*Type[0]*/double/*Type[0]*/[] array)
        {
            if (array != null)
            {
                Laurent.Lee.CLB.Algorithm.TmphQuickSort.Sort/*Compare[0]*//*Compare[0]*/(array);
                return array;
            }
            return TmphNullValue</*Type[0]*/double/*Type[0]*/>.Array;
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <returns>排序后的新数组</returns>
        public static /*Type[0]*/double/*Type[0]*/[] getSort/*Compare[0]*//*Compare[0]*/(this /*Type[0]*/double/*Type[0]*/[] array)
        {
            if (array != null)
            {
                return Laurent.Lee.CLB.Algorithm.TmphQuickSort.GetSort/*Compare[0]*//*Compare[0]*/(array);
            }
            return TmphNullValue</*Type[0]*/double/*Type[0]*/>.Array;
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">待排序数组</param>
        /// <param name="getKey">排序键</param>
        /// <returns>排序后的数组</returns>
        public static TValueType[] getSort/*Compare[0]*//*Compare[0]*/<TValueType>
            (this TValueType[] array, Func<TValueType, /*Type[0]*/double/*Type[0]*/> getKey)
        {
            if (getKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            if (array != null)
            {
                if (array.Length > 1) return Laurent.Lee.CLB.Algorithm.TmphQuickSort.GetSort/*Compare[0]*//*Compare[0]*/(array, getKey);
                if (array.Length != 0) return new TValueType[] { array[0] };
            }
            return TmphNullValue<TValueType>.Array;
        }
    }

    /// <summary>
    /// 数组子串扩展
    /// </summary>
    public static partial class TmphSubArrayExtension
    {
        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <returns>排序后的数组</returns>
        public static TmphSubArray</*Type[0]*/double/*Type[0]*/> sort/*Compare[0]*//*Compare[0]*/(this TmphSubArray</*Type[0]*/double/*Type[0]*/> array)
        {
            if (array.Count > 1) Laurent.Lee.CLB.Algorithm.TmphQuickSort.Sort/*Compare[0]*//*Compare[0]*/(array.Array, array.StartIndex, array.Count);
            return array;
        }

        /// <summary>
        /// 数组排序
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <returns>排序后的新数组</returns>
        public static /*Type[0]*/double/*Type[0]*/[] getSort/*Compare[0]*//*Compare[0]*/(this TmphSubArray</*Type[0]*/double/*Type[0]*/> array)
        {
            return Laurent.Lee.CLB.Algorithm.TmphQuickSort.GetSort/*Compare[0]*//*Compare[0]*/(array.Array, array.StartIndex, array.Count);
        }

        /// <summary>
        /// 排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">待排序数组</param>
        /// <param name="getKey">排序键</param>
        /// <returns>排序后的数组</returns>
        public static TValueType[] getSort/*Compare[0]*//*Compare[0]*/<TValueType>(this TmphSubArray<TValueType> array, Func<TValueType, /*Type[0]*/double/*Type[0]*/> getKey)
        {
            if (array.Count > 1) return Laurent.Lee.CLB.Algorithm.TmphQuickSort.GetSort/*Compare[0]*//*Compare[0]*/(array.Array, getKey, array.StartIndex, array.Count);
            return array.Count == 0 ? TmphNullValue<TValueType>.Array : new TValueType[] { array.Array[array.StartIndex] };
        }
    }
}