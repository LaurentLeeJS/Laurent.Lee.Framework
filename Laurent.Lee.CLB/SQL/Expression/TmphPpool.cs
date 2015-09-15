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

namespace Laurent.Lee.CLB.Sql.Expression
{
    /// <summary>
    ///     表达式
    /// </summary>
    internal static class TmphPpool
    {
        /// <summary>
        ///     添加到表达式池
        /// </summary>
        /// <param name="expression">表达式</param>
        internal static void PushPool(TmphExpression expression)
        {
            if (expression != null) expression.PushPool();
        }

        /// <summary>
        ///     添加到表达式池
        /// </summary>
        /// <param name="expression">表达式</param>
        internal static void PushPool<expressionType>(ref expressionType expression)
            where expressionType : TmphExpression
        {
            if (expression != null)
            {
                expression.PushPool();
                expression = null;
            }
        }
    }
}