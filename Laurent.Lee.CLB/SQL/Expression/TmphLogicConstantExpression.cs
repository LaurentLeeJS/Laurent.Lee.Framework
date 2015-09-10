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
    ///     逻辑常量表达式
    /// </summary>
    internal class TmphLogicConstantExpression : TmphExpression
    {
        /// <summary>
        ///     逻辑常量表达式
        /// </summary>
        private TmphLogicConstantExpression()
        {
            NodeType = TmphExpressionType.LogicConstant;
            IsSimple = IsConstant = true;
        }

        /// <summary>
        ///     逻辑值
        /// </summary>
        public bool Value { get; private set; }

        /// <summary>
        ///     添加到表达式池
        /// </summary>
        internal override void pushPool()
        {
            TmphTypePool<TmphLogicConstantExpression>.Push(this);
        }

        /// <summary>
        ///     获取逻辑常量表达式
        /// </summary>
        /// <param name="value">逻辑值</param>
        /// <returns>逻辑常量表达式</returns>
        internal static TmphLogicConstantExpression Get(bool value)
        {
            var expression = TmphTypePool<TmphLogicConstantExpression>.Pop() ?? new TmphLogicConstantExpression();
            expression.Value = value;
            return expression;
        }
    }
}