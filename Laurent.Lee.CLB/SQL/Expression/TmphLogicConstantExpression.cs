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