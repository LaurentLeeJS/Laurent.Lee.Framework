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