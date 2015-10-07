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