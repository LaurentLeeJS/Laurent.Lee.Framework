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

using System.Reflection;

namespace Laurent.Lee.CLB.Sql.Expression
{
    /// <summary>
    ///     字段表达式
    /// </summary>
    internal class TmphFieldExpression : TmphMemberExpression
    {
        /// <summary>
        ///     字段表达式
        /// </summary>
        private TmphFieldExpression()
        {
            NodeType = TmphExpressionType.FieldAccess;
        }

        /// <summary>
        ///     字段信息
        /// </summary>
        public FieldInfo FieldInfo { get; private set; }

        /// <summary>
        ///     简单表达式
        /// </summary>
        public override TmphExpression SimpleExpression
        {
            get
            {
                if (Expression.IsConstant)
                {
                    var value = FieldInfo.GetValue(((TmphConstantExpression)Expression).Value);
                    PushPool();
                    return TmphConstantExpression.Get(value);
                }
                return this;
            }
        }

        /// <summary>
        ///     添加到表达式池
        /// </summary>
        internal override void pushPool()
        {
            FieldInfo = null;
            clear();
            TmphTypePool<TmphFieldExpression>.Push(this);
        }

        /// <summary>
        ///     获取字段表达式
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="field">字段信息</param>
        /// <returns>字段表达式</returns>
        internal static TmphFieldExpression Get(TmphExpression expression, FieldInfo field)
        {
            var fieldExpression = TmphTypePool<TmphFieldExpression>.Pop() ?? new TmphFieldExpression();
            fieldExpression.FieldInfo = field;
            fieldExpression.set(expression);
            return fieldExpression;
        }
    }
}