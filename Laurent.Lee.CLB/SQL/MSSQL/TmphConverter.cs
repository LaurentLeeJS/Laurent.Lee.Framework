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