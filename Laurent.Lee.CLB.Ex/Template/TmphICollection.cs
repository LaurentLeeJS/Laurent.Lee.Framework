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

/*Type:ulong;long;uint;int;ushort;short;byte;sbyte;double;float;DateTime*/

namespace Laurent.Lee.CLB.Template
{
    /// <summary>
    /// 集合相关扩展
    /// </summary>
    public static partial class TmphICollection
    {
        /// <summary>
        /// 根据集合内容返回数组
        /// </summary>
        /// <param name="values">值集合</param>
        /// <returns>数组</returns>
        public unsafe static /*Type[0]*/ulong/*Type[0]*/[] getArray
            (this System.Collections.Generic.ICollection</*Type[0]*/ulong/*Type[0]*/> values)
        {
            if (values.count() != 0)
            {
                /*Type[0]*/
                ulong/*Type[0]*/[] newValues = new /*Type[0]*/ulong/*Type[0]*/[values.Count];
                fixed (/*Type[0]*/ulong/*Type[0]*/* newValueFixed = newValues)
                {
                    /*Type[0]*/
                    ulong/*Type[0]*/* write = newValueFixed;
                    foreach (/*Type[0]*/ulong/*Type[0]*/ value in values) *write++ = value;
                }
                return newValues;
            }
            return TmphNullValue</*Type[0]*/ulong/*Type[0]*/>.Array;
        }

        /// <summary>
        /// 根据集合内容返回数组
        /// </summary>
        /// <typeparam name="TValueType">枚举值类型</typeparam>
        /// <param name="values">值集合</param>
        /// <param name="getValue">获取数组值的委托</param>
        /// <returns>数组</returns>
        public unsafe static /*Type[0]*/ulong/*Type[0]*/[] getArray<TValueType>
            (this System.Collections.Generic.ICollection<TValueType> values, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getValue)
        {
            if (values.count() != 0)
            {
                if (getValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                /*Type[0]*/
                ulong/*Type[0]*/[] newValues = new /*Type[0]*/ulong/*Type[0]*/[values.Count];
                fixed (/*Type[0]*/ulong/*Type[0]*/* newValueFixed = newValues)
                {
                    /*Type[0]*/
                    ulong/*Type[0]*/* write = newValueFixed;
                    foreach (TValueType value in values) *write++ = getValue(value);
                }
                return newValues;
            }
            return TmphNullValue</*Type[0]*/ulong/*Type[0]*/>.Array;
        }

        /// <summary>
        /// 根据集合内容返回单向列表
        /// </summary>
        /// <typeparam name="TValueType">枚举值类型</typeparam>
        /// <param name="values">值集合</param>
        /// <param name="getValue">获取数组值的委托</param>
        /// <returns>单向列表</returns>
        public static TmphList</*Type[0]*/ulong/*Type[0]*/> getList<TValueType>
            (this System.Collections.Generic.ICollection<TValueType> values, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getValue)
        {
            return values != null ? new TmphList</*Type[0]*/ulong/*Type[0]*/>(getArray(values, getValue), true) : null;
        }

        /// <summary>
        /// 根据集合内容返回单向列表
        /// </summary>
        /// <param name="values">值集合</param>
        /// <returns>单向列表</returns>
        public static TmphList</*Type[0]*/ulong/*Type[0]*/> getList(this System.Collections.Generic.ICollection</*Type[0]*/ulong/*Type[0]*/> values)
        {
            return values != null ? new TmphList</*Type[0]*/ulong/*Type[0]*/>(getArray(values), true) : null;
        }

        /// <summary>
        /// 根据集合内容返回双向列表
        /// </summary>
        /// <typeparam name="TValueType">枚举值类型</typeparam>
        /// <param name="values">值集合</param>
        /// <param name="getValue">获取数组值的委托</param>
        /// <returns>双向列表</returns>
        public static TmphCollection</*Type[0]*/ulong/*Type[0]*/> getCollection<TValueType>
            (this System.Collections.Generic.ICollection<TValueType> values, Func<TValueType, /*Type[0]*/ulong/*Type[0]*/> getValue)
        {
            return values != null ? new TmphCollection</*Type[0]*/ulong/*Type[0]*/>(getArray(values, getValue), true) : null;
        }

        /// <summary>
        /// 根据集合内容返回双向列表
        /// </summary>
        /// <param name="values">值集合</param>
        /// <returns>双向列表</returns>
        public static TmphCollection</*Type[0]*/ulong/*Type[0]*/> getCollection
            (this System.Collections.Generic.ICollection</*Type[0]*/ulong/*Type[0]*/> values)
        {
            return values != null ? new TmphCollection</*Type[0]*/ulong/*Type[0]*/>(getArray(values), true) : null;
        }

        /// <summary>
        /// 查找符合条件的记录集合
        /// </summary>
        /// <param name="values">值集合</param>
        /// <param name="isValue">判断记录是否符合条件的委托</param>
        /// <returns>符合条件的记录集合</returns>
        public unsafe static TmphSubArray</*Type[0]*/ulong/*Type[0]*/> getFind
            (this System.Collections.Generic.ICollection</*Type[0]*/ulong/*Type[0]*/> values, Func</*Type[0]*/ulong/*Type[0]*/, bool> isValue)
        {
            if (values.count() != 0)
            {
                if (isValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                /*Type[0]*/
                ulong/*Type[0]*/[] newValues = new /*Type[0]*/ulong/*Type[0]*/[values.Count];
                fixed (/*Type[0]*/ulong/*Type[0]*/* newValueFixed = newValues)
                {
                    /*Type[0]*/
                    ulong/*Type[0]*/* write = newValueFixed;
                    foreach (/*Type[0]*/ulong/*Type[0]*/ value in values)
                    {
                        if (isValue(value)) *write++ = value;
                    }
                    return TmphSubArray</*Type[0]*/ulong/*Type[0]*/>.Unsafe(newValues, 0, (int)(write - newValueFixed));
                }
            }
            return default(TmphSubArray</*Type[0]*/ulong/*Type[0]*/>);
        }

        /// <summary>
        /// 查找符合条件的记录集合
        /// </summary>
        /// <param name="values">值集合</param>
        /// <param name="isValue">判断记录是否符合条件的委托</param>
        /// <returns>符合条件的记录集合</returns>
        public unsafe static TmphSubArray</*Type[0]*/ulong/*Type[0]*/> getFind
            (this System.Collections.ICollection values, Func</*Type[0]*/ulong/*Type[0]*/, bool> isValue)
        {
            if (values != null)
            {
                if (isValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                /*Type[0]*/
                ulong/*Type[0]*/[] newValues = new /*Type[0]*/ulong/*Type[0]*/[values.Count];
                fixed (/*Type[0]*/ulong/*Type[0]*/* newValueFixed = newValues)
                {
                    /*Type[0]*/
                    ulong/*Type[0]*/* write = newValueFixed;
                    foreach (/*Type[0]*/ulong/*Type[0]*/ value in values)
                    {
                        if (isValue(value)) *write++ = value;
                    }
                    return TmphSubArray</*Type[0]*/ulong/*Type[0]*/>.Unsafe(newValues, 0, (int)(write - newValueFixed));
                }
            }
            return default(TmphSubArray</*Type[0]*/ulong/*Type[0]*/>);
        }

        /// <summary>
        /// 查找符合条件的记录集合
        /// </summary>
        /// <param name="values">值集合</param>
        /// <param name="isValue">判断记录是否符合条件的委托</param>
        /// <returns>符合条件的记录集合</returns>
        public static /*Type[0]*/ulong/*Type[0]*/[] getFindArray
            (this System.Collections.Generic.ICollection</*Type[0]*/ulong/*Type[0]*/> values, Func</*Type[0]*/ulong/*Type[0]*/, bool> isValue)
        {
            return values.getFind(isValue).ToArray().notNull();
        }

        /// <summary>
        /// 查找符合条件的记录集合
        /// </summary>
        /// <param name="values">值集合</param>
        /// <param name="isValue">判断记录是否符合条件的委托</param>
        /// <returns>符合条件的记录集合</returns>
        public static /*Type[0]*/ulong/*Type[0]*/[] getFindArray
            (this System.Collections.ICollection values, Func</*Type[0]*/ulong/*Type[0]*/, bool> isValue)
        {
            return values.getFind(isValue).ToArray().notNull();
        }
    }
}