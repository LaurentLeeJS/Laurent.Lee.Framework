/*
-------------------------------------------------- -----------------------------------------
The frame content is protected by copyright law. In order to facilitate individual learning,
allows to download the program source information, but does not allow individuals or a third
party for profit, the commercial use of the source information. Without consent,
does not allow any form (even if partial, or modified) database storage,
copy the source of information. If the source content provided by third parties,
which corresponds to the third party content is also protected by copyright.

If you are found to have infringed copyright behavior, please give me a hint. THX!

Here in particular it emphasized that the third party is not allowed to contact addresses
published in this "version copyright statement" to send advertising material.
I will take legal means to resist sending spam.
-------------------------------------------------- ----------------------------------------
The framework under the GNU agreement, Detail View GNU License.
If you think about this item affection join the development team,
Please contact me: LaurentLeeJS@gmail.com
-------------------------------------------------- ----------------------------------------
Laurent.Lee.Framework Coded by Laurent Lee
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