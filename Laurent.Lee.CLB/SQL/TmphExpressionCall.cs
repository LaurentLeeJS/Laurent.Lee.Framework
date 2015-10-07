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

namespace Laurent.Lee.CLB.Sql
{
    /// <summary>
    ///     SQL函数调用
    /// </summary>
    internal static class TmphExpressionCall
    {
        /// <summary>
        ///     计数
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="value"></param>
        /// <returns>计数</returns>
        public static int Count<TValueType>(TValueType value)
        {
            return 0;
        }

        /// <summary>
        ///     求和
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="value"></param>
        /// <returns>求和</returns>
        public static int Sum<TValueType>(TValueType value)
        {
            return 0;
        }

        /// <summary>
        ///     获取当前时间
        /// </summary>
        /// <returns>当前时间</returns>
        public static DateTime GetDate()
        {
            return TmphDate.NowSecond;
        }

        /// <summary>
        ///     IN表达式
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="value">数值</param>
        /// <param name="values">数值集合</param>
        /// <returns>是否包含数值</returns>
        public static bool In<TValueType>(TValueType value, params TValueType[] values)
        {
            return false;
        }
    }
}