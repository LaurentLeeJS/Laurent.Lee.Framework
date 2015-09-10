/*
 *  Copyright 2015 Tony Lee

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
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