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