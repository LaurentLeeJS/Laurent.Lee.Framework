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

namespace Laurent.Lee.CLB
{
    /// <summary>
    ///     静态哈希索引
    /// </summary>
    public abstract class TmphStaticHashIndex
    {
        /// <summary>
        ///     默认索引集合尺寸二进制位数
        /// </summary>
        protected const int DefaultArrayLengthBits = 4;

        /// <summary>
        ///     默认索引集合
        /// </summary>
        protected static TmphRange[] DefaultIndexs = new TmphRange[1 << DefaultArrayLengthBits];

        /// <summary>
        ///     索引和值
        /// </summary>
        protected int IndexAnd;

        /// <summary>
        ///     索引集合
        /// </summary>
        protected TmphRange[] Indexs;

        /// <summary>
        ///     获取哈希值
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="value">数据</param>
        /// <returns>哈希值</returns>
        protected static int GetHashCode<TValueType>(TValueType value)
        {
            if (value != null)
            {
                var code = value.GetHashCode();
                return code ^ (code >> DefaultArrayLengthBits);
            }
            return 0;
        }

        /// <summary>
        ///     获取哈希数据数组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="values">数据集合</param>
        /// <param name="hashs">哈希集合</param>
        /// <returns>哈希数据数组</returns>
        protected unsafe TValueType[] GetValues<TValueType>(TValueType[] values, int* hashs)
        {
            var indexBits = ((uint)values.Length).bits();
            if (indexBits < DefaultArrayLengthBits) indexBits = DefaultArrayLengthBits;
            else if ((1 << (indexBits - 1)) == values.Length) --indexBits;
            IndexAnd = 1 << indexBits;
            Indexs = new TmphRange[IndexAnd--];
            fixed (TmphRange* indexFixed = Indexs)
            {
                for (var hash = hashs + values.Length; hash != hashs; ++indexFixed[*--hash & IndexAnd].StartIndex)
                {
                }
                var startIndex = 0;
                for (TmphRange* index = indexFixed, endIndex = indexFixed + IndexAnd + 1; index != endIndex; ++index)
                {
                    var nextIndex = startIndex + (*index).StartIndex;
                    (*index).StartIndex = (*index).EndIndex = startIndex;
                    startIndex = nextIndex;
                }
                var newValues = new TValueType[values.Length];
                foreach (var value in values) newValues[indexFixed[*hashs++ & IndexAnd].EndIndex++] = value;
                //for (int index = 0; index != values.Length; newValues[indexFixed[*hashs++ & indexAnd].EndIndex++] = values[index++]) ;
                return newValues;
            }
        }

        /// <summary>
        ///     索引范围
        /// </summary>
        protected struct TmphRange
        {
            /// <summary>
            ///     结束位置
            /// </summary>
            public int EndIndex;

            /// <summary>
            ///     起始位置
            /// </summary>
            public int StartIndex;

            /// <summary>
            ///     重置索引范围
            /// </summary>
            /// <param name="startIndex">起始位置</param>
            /// <param name="endIndex">结束位置</param>
            public void Set(int startIndex, int endIndex)
            {
                StartIndex = startIndex;
                EndIndex = endIndex;
            }
        }
    }
}