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
using Laurent.Lee.CLB.Web;
using System;
using System.Collections;

namespace Laurent.Lee.CLB.Sql.MsSql
{
    /// <summary>
    ///     表达式转换
    /// </summary>
    internal class TmphExpressionConverter : Expression.TmphExpressionConverter
    {
        /// <summary>
        ///     表达式转换
        /// </summary>
        internal static readonly TmphExpressionConverter Default = new TmphExpressionConverter();

        /// <summary>
        ///     表达式转换
        /// </summary>
        protected TmphExpressionConverter()
        {
            converters[(int)TmphExpressionType.Constant] = convertConstant;
            converters[(int)TmphExpressionType.InSet] = convertInSet;
        }

        /// <summary>
        ///     转换表达式
        /// </summary>
        /// <param name="converter">表达式转换器</param>
        /// <param name="expression">表达式</param>
        private void convertConstant(Expression.TmphConverter converter, TmphExpression expression)
        {
            var value = ((TmphConstantExpression)expression).Value;
            if (value != null)
            {
                var toString = TmphConstantConverter.Default[value.GetType()];
                if (toString != null) toString(converter.Stream, value);
                else TmphConstantConverter.ConvertConstantStringQuote(converter.Stream, value.ToString());
            }
            else TmphAjax.WriteNull(converter.Stream);
        }

        /// <summary>
        ///     转换表达式
        /// </summary>
        /// <param name="converter">表达式转换器</param>
        /// <param name="expression">表达式</param>
        private void convertInSet(Expression.TmphConverter converter, TmphExpression expression)
        {
            var stream = converter.Stream;
            var binaryExpression = (TmphBinaryExpression)expression;
            var left = binaryExpression.Left;
            converters[(int)left.NodeType](converter, left);
            stream.WriteNotNull(" In(");
            Action<TmphCharStream, object> toString = null;
            var index = -1;
            foreach (var value in (IEnumerable)((TmphConstantExpression)binaryExpression.Right).Value)
            {
                if (++index == 0) toString = TmphConstantConverter.Default[value.GetType()];
                else stream.Write(',');
                if (toString == null) TmphConstantConverter.ConvertConstantStringQuote(stream, value.ToString());
                else toString(stream, value);
            }
            stream.Write(')');
        }
    }
}