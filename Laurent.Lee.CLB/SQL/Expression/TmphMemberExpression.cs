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

namespace Laurent.Lee.CLB.Sql.Expression
{
    /// <summary>
    ///     成员表达式
    /// </summary>
    internal abstract class TmphMemberExpression : TmphExpression
    {
        /// <summary>
        ///     表达式
        /// </summary>
        public TmphExpression Expression { get; private set; }

        /// <summary>
        ///     成员表达式
        /// </summary>
        /// <param name="expression">表达式</param>
        protected void set(TmphExpression expression)
        {
            ++(Expression = expression.SimpleExpression).ExpressionCount;
            IsSimple = true;
            IsConstant = Expression.IsConstant;
        }

        /// <summary>
        ///     清除数据
        /// </summary>
        protected void clear()
        {
            Expression.PushCountPool();
            Expression = null;
        }
    }
}