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
    ///     条件表达式
    /// </summary>
    internal class TmphConditionalExpression : TmphExpression
    {
        /// <summary>
        ///     条件表达式
        /// </summary>
        private TmphConditionalExpression()
        {
            NodeType = TmphExpressionType.Conditional;
        }

        /// <summary>
        ///     测试条件
        /// </summary>
        public TmphExpression Test { get; private set; }

        /// <summary>
        ///     真表达式
        /// </summary>
        public TmphExpression IfTrue { get; private set; }

        /// <summary>
        ///     假表达式
        /// </summary>
        public TmphExpression IfFalse { get; private set; }

        /// <summary>
        ///     简单表达式
        /// </summary>
        public override TmphExpression SimpleExpression
        {
            get
            {
                if (Test.IsConstant)
                {
                    TmphExpression expression;
                    var value = ((TmphConstantExpression)Test).Value;
                    if (value != null && (bool)value)
                    {
                        expression = IfTrue;
                        IfTrue = null;
                    }
                    else
                    {
                        expression = IfFalse;
                        IfFalse = null;
                    }
                    PushPool();
                    return expression;
                }
                return this;
            }
        }

        /// <summary>
        ///     添加到表达式池
        /// </summary>
        internal override void pushPool()
        {
            Test.PushCountPool();
            Test = null;
            if (IfTrue != null)
            {
                IfTrue.PushCountPool();
                IfTrue = null;
            }
            if (Test != null)
            {
                IfFalse.PushCountPool();
                IfFalse = null;
            }
            TmphTypePool<TmphConditionalExpression>.Push(this);
        }

        /// <summary>
        ///     获取条件表达式
        /// </summary>
        /// <param name="test">测试条件</param>
        /// <param name="ifTrue">真表达式</param>
        /// <param name="ifFalse">假表达式</param>
        /// <returns>条件表达式</returns>
        internal static TmphConditionalExpression Get(TmphExpression test, TmphExpression ifTrue, TmphExpression ifFalse)
        {
            var expression = TmphTypePool<TmphConditionalExpression>.Pop() ?? new TmphConditionalExpression();
            ++(expression.Test = test.SimpleExpression).ExpressionCount;
            ++(expression.IfTrue = ifTrue.SimpleExpression).ExpressionCount;
            ++(expression.IfFalse = ifFalse.SimpleExpression).ExpressionCount;
            return expression;
        }
    }
}