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
    ///     属性表达式
    /// </summary>
    internal class TmphPropertyExpression : TmphMemberExpression
    {
        /// <summary>
        ///     属性表达式
        /// </summary>
        private TmphPropertyExpression()
        {
            NodeType = TmphExpressionType.PropertyAccess;
        }

        /// <summary>
        ///     属性信息
        /// </summary>
        public PropertyInfo PropertyInfo { get; private set; }

        /// <summary>
        ///     简单表达式
        /// </summary>
        public override TmphExpression SimpleExpression
        {
            get
            {
                if (Expression.IsConstant)
                {
                    var value = PropertyInfo.GetValue(((TmphConstantExpression)Expression).Value, null);
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
            PropertyInfo = null;
            clear();
            TmphTypePool<TmphPropertyExpression>.Push(this);
        }

        /// <summary>
        ///     获取属性表达式
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="property">属性信息</param>
        /// <returns>属性表达式</returns>
        internal static TmphPropertyExpression Get(TmphExpression expression, PropertyInfo property)
        {
            var propertyExpression = TmphTypePool<TmphPropertyExpression>.Pop() ?? new TmphPropertyExpression();
            propertyExpression.PropertyInfo = property;
            propertyExpression.set(expression);
            return propertyExpression;
        }
    }
}