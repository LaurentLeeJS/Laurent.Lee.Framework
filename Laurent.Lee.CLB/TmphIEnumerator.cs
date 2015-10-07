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

using System.Collections;
using System.Collections.Generic;

namespace Laurent.Lee.CLB
{
    /// <summary>
    ///     枚举器
    /// </summary>
    /// <typeparam name="TValueType"></typeparam>
    internal static class TmphIEnumerator<TValueType>
    {
        /// <summary>
        ///     空枚举实例
        /// </summary>
        internal static readonly IEnumerator<TValueType> Empty = new TmphEmpty();

        /// <summary>
        ///     空枚举器
        /// </summary>
        private struct TmphEmpty : IEnumerator<TValueType>
        {
            /// <summary>
            ///     当前数据元素
            /// </summary>
            TValueType IEnumerator<TValueType>.Current
            {
                get
                {
                    TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
                    return default(TValueType);
                }
            }

            /// <summary>
            ///     当前数据元素
            /// </summary>
            object IEnumerator.Current
            {
                get
                {
                    TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
                    return default(TValueType);
                }
            }

            /// <summary>
            ///     转到下一个数据元素
            /// </summary>
            /// <returns>是否存在下一个数据元素</returns>
            public bool MoveNext()
            {
                return false;
            }

            /// <summary>
            ///     重置枚举器状态
            /// </summary>
            public void Reset()
            {
            }

            /// <summary>
            ///     释放枚举器
            /// </summary>
            public void Dispose()
            {
            }
        }

        /// <summary>
        ///     数组枚举器
        /// </summary>
        internal struct TmphArray : IEnumerator<TValueType>
        {
            /// <summary>
            ///     结束位置
            /// </summary>
            private readonly int endIndex;

            /// <summary>
            ///     起始位置
            /// </summary>
            private readonly int startIndex;

            /// <summary>
            ///     当前位置
            /// </summary>
            private int currentIndex;

            /// <summary>
            ///     被枚举数组
            /// </summary>
            private TValueType[] values;

            /// <summary>
            ///     数组枚举器
            /// </summary>
            /// <param name="value">双向动态数据</param>
            public TmphArray(TmphCollection<TValueType> value)
            {
                values = value.array;
                startIndex = value.StartIndex;
                endIndex = value.EndIndex;
                currentIndex = startIndex - 1;
                if (endIndex == 0) endIndex = values.Length;
            }

            /// <summary>
            ///     数组枚举器
            /// </summary>
            /// <param name="value">单向动态数据</param>
            public TmphArray(TmphList<TValueType> value)
            {
                values = value.array;
                startIndex = 0;
                endIndex = value.Count;
                currentIndex = -1;
            }

            /// <summary>
            ///     数组枚举器
            /// </summary>
            /// <param name="value">数组子串</param>
            public TmphArray(TmphSubArray<TValueType> value)
            {
                values = value.array;
                startIndex = value.StartIndex;
                endIndex = value.EndIndex;
                currentIndex = startIndex - 1;
            }

            /// <summary>
            ///     当前数据元素
            /// </summary>
            TValueType IEnumerator<TValueType>.Current
            {
                get { return values[currentIndex]; }
            }

            /// <summary>
            ///     当前数据元素
            /// </summary>
            object IEnumerator.Current
            {
                get { return values[currentIndex]; }
            }

            /// <summary>
            ///     转到下一个数据元素
            /// </summary>
            /// <returns>是否存在下一个数据元素</returns>
            public bool MoveNext()
            {
                if (++currentIndex != endIndex) return true;
                --currentIndex;
                return false;
            }

            /// <summary>
            ///     重置枚举器状态
            /// </summary>
            public void Reset()
            {
                currentIndex = startIndex - 1;
            }

            /// <summary>
            ///     释放枚举器
            /// </summary>
            public void Dispose()
            {
                values = null;
            }
        }
    }
}