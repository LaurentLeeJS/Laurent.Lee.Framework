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