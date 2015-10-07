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

using Laurent.Lee.CLB.Sql.Expression;

namespace Laurent.Lee.CLB.Sql.MySql
{
    /// <summary>
    ///     表达式转换
    /// </summary>
    internal class TmphExpressionConverter : MsSql.TmphExpressionConverter
    {
        /// <summary>
        ///     表达式转换
        /// </summary>
        internal new static readonly TmphExpressionConverter Default = new TmphExpressionConverter();

        /// <summary>
        ///     转换表达式
        /// </summary>
        /// <param name="converter">表达式转换器</param>
        /// <param name="expression">表达式</param>
        protected override void convertNotEqual(Expression.TmphConverter converter, TmphBinaryExpression expression)
        {
            convertBinaryExpression(converter, expression, '!', '=');
        }
    }
}