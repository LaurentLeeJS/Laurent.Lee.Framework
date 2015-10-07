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

namespace Laurent.Lee.CLB
{
    /// <summary>
    ///     唯一静态哈希
    /// </summary>
    /// <typeparam name="TValueType">数据类型</typeparam>
    public sealed class TmphUniqueHashSet<TValueType> where TValueType : struct, IEquatable<TValueType>
    {
        /// <summary>
        ///     哈希数据数组
        /// </summary>
        private readonly TValueType[] _array;

        /// <summary>
        ///     唯一静态哈希
        /// </summary>
        /// <param name="values">数据集合</param>
        /// <param name="size">哈希容器尺寸</param>
        public unsafe TmphUniqueHashSet(TValueType[] values, int size)
        {
            if (values.length() > size || size <= 0) TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            _array = new TValueType[size];
            var length = ((size + 31) >> 5) << 2;
            byte* isValue = stackalloc byte[length];
            var map = new TmphFixedMap(isValue, length);
            foreach (var value in values)
            {
                var index = value.GetHashCode();
                if ((uint)index >= size) TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
                if (map.Get(index)) TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
                map.Set(index);
                _array[index] = value;
            }
        }

        /// <summary>
        ///     判断是否存在某值
        /// </summary>
        /// <param name="value">待匹配值</param>
        /// <returns>是否存在某值</returns>
        public bool Contains(TValueType value)
        {
            var index = value.GetHashCode();
            return (uint)index < _array.Length && value.Equals(_array[index]);
        }
    }
}