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
    ///     常量表达式
    /// </summary>
    internal class TmphConstantExpression : TmphExpression
    {
        /// <summary>
        ///     常量表达式
        /// </summary>
        private TmphConstantExpression()
        {
            NodeType = TmphExpressionType.Constant;
            IsSimple = IsConstant = true;
        }

        /// <summary>
        ///     数据
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        ///     添加到表达式池
        /// </summary>
        internal override void pushPool()
        {
            Value = null;
            TmphTypePool<TmphConstantExpression>.Push(this);
        }

        /// <summary>
        ///     获取常量表达式
        /// </summary>
        /// <param name="value">数据</param>
        /// <returns>常量表达式</returns>
        internal static TmphConstantExpression Get(object value)
        {
            var expression = TmphTypePool<TmphConstantExpression>.Pop() ?? new TmphConstantExpression();
            expression.Value = value;
            return expression;
        }
    }
}