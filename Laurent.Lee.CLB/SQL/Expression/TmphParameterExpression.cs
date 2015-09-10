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

using System;
using System.Linq.Expressions;

namespace Laurent.Lee.CLB.Sql.Expression
{
    /// <summary>
    ///     参数表达式
    /// </summary>
    internal class TmphParameterExpression : TmphExpression
    {
        /// <summary>
        ///     参数表达式
        /// </summary>
        private TmphParameterExpression()
        {
            NodeType = TmphExpressionType.Parameter;
        }

        /// <summary>
        ///     参数类型
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        ///     参数名称
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        ///     添加到表达式池
        /// </summary>
        internal override void pushPool()
        {
            Type = null;
            Name = null;
            TmphTypePool<TmphParameterExpression>.Push(this);
        }

        /// <summary>
        ///     获取参数表达式
        /// </summary>
        /// <param name="type">参数类型</param>
        /// <param name="name">参数名称</param>
        /// <returns>参数表达式</returns>
        internal static TmphParameterExpression Get(Type type, string name)
        {
            var expression = TmphTypePool<TmphParameterExpression>.Pop() ?? new TmphParameterExpression();
            expression.Type = type;
            expression.Name = name;
            return expression;
        }

        /// <summary>
        ///     表达式转换
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>表达式</returns>
        internal static TmphParameterExpression convert(ParameterExpression expression)
        {
            return Get(expression.Type, expression.Name);
        }
    }
}