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