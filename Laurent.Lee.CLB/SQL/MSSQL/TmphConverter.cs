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

namespace Laurent.Lee.CLB.Sql.MsSql
{
    /// <summary>
    ///     委托关联表达式转SQL表达式
    /// </summary>
    internal class TmphConverter : Expression.TmphConverter
    {
        /// <summary>
        ///     创建SQL
        /// </summary>
        /// <param name="expression">委托关联表达式</param>
        /// <param name="sqlStream">SQL表达式流</param>
        protected override void create(TmphLambdaExpression expression, TmphCharStream stream)
        {
            this.stream = stream;
            TmphExpressionConverter.Default[expression.Body.NodeType](this, expression.Body);
        }

        /// <summary>
        ///     委托关联表达式转SQL表达式
        /// </summary>
        /// <param name="expression">委托关联表达式</param>
        /// <param name="sqlStream">SQL表达式流</param>
        /// <returns>参数成员名称</returns>
        public static string Convert(TmphLambdaExpression expression, TmphCharStream stream)
        {
            var converter = new TmphConverter();
            converter.create(expression, stream);
            return converter.FirstMemberName;
        }

        /// <summary>
        ///     委托关联表达式转SQL表达式
        /// </summary>
        /// <param name="expression">委托关联表达式</param>
        /// <returns>参数成员名称+SQL表达式</returns>
        public static TmphKeyValue<string, string> Convert(TmphLambdaExpression expression)
        {
            var converter = new TmphConverter();
            var sql = converter.Create(expression);
            return new TmphKeyValue<string, string>(converter.FirstMemberName, sql);
        }
    }
}