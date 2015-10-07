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

namespace Laurent.Lee.CLB.Unsafe
{
    /// <summary>
    ///     数组扩展操作(非安全,请自行确保数据可靠性)
    /// </summary>
    public static class TmphArray
    {
        /// <summary>
        ///     移动数据块
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">待处理数组</param>
        /// <param name="index">原始数据位置</param>
        /// <param name="writeIndex">目标数据位置</param>
        /// <param name="count">移动数据数量</param>
        public static void Move<TValueType>(TValueType[] array, int index, int writeIndex, int count)
        {
            var endIndex = index + count;
            if (index < writeIndex && endIndex > writeIndex)
            {
                for (var writeEndIndex = writeIndex + count;
                    endIndex != index;
                    array[--writeEndIndex] = array[--endIndex])
                    ;
            }
            else Array.Copy(array, index, array, writeIndex, count);
        }

        /// <summary>
        ///     移除数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="index">移除数据位置</param>
        /// <returns>移除数据后的数组</returns>
        public static TValueType[] GetRemoveAt<TValueType>(TValueType[] array, int index)
        {
            var newValues = new TValueType[array.Length - 1];
            Array.Copy(array, 0, newValues, 0, index);
            Array.Copy(array, index + 1, newValues, index, array.Length - index - 1);
            return newValues;
        }
    }
}