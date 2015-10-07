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

using System.Linq.Expressions;

namespace Laurent.Lee.CLB.Sql.Expression
{
    /// <summary>
    ///     lambda表达式
    /// </summary>
    internal class TmphLambdaExpression : TmphExpression
    {
        /// <summary>
        ///     委托关联表达式
        /// </summary>
        protected TmphLambdaExpression()
        {
            NodeType = TmphExpressionType.Lambda;
        }

        /// <summary>
        ///     表达式主体
        /// </summary>
        public TmphExpression Body { get; private set; }

        /// <summary>
        ///     参数
        /// </summary>
        public TmphParameterExpression[] Parameters { get; private set; }

        /// <summary>
        ///     是否逻辑常量表达式
        /// </summary>
        public bool IsLogicConstantExpression
        {
            get { return Body.NodeType == TmphExpressionType.LogicConstant; }
        }

        /// <summary>
        ///     逻辑常量值
        /// </summary>
        public bool LogicConstantValue
        {
            get { return ((TmphLogicConstantExpression)Body).Value; }
        }

        /// <summary>
        ///     清除数据
        /// </summary>
        protected void clear()
        {
            Body.PushCountPool();
            Body = null;
            if (Parameters != null)
            {
                foreach (var parameter in Parameters) parameter.PushCountPool();
                Parameters = null;
            }
        }

        /// <summary>
        ///     委托关联表达式
        /// </summary>
        /// <param name="body">表达式主体</param>
        /// <param name="parameters">参数</param>
        protected void set(TmphExpression body, TmphParameterExpression[] parameters)
        {
            ++(Body = body.SimpleExpression).ExpressionCount;
            Parameters = parameters;
            if (parameters != null)
            {
                foreach (var parameter in parameters) ++parameter.ExpressionCount;
            }
        }

        ///// <summary>
        ///// 根据参数名称获取参数表达式
        ///// </summary>
        ///// <param name="name">参数名称</param>
        ///// <returns>参数表达式</returns>
        //public ParameterExpression GetParameter(string name)
        //{
        //    if (Parameters != null)
        //    {
        //        foreach (ParameterExpression parameter in Parameters)
        //        {
        //            if (parameter.Name == name) return parameter;
        //        }
        //    }
        //    return null;
        //}
        /// <summary>
        ///     添加到表达式池
        /// </summary>
        internal override void pushPool()
        {
            clear();
            TmphTypePool<TmphLambdaExpression>.Push(this);
        }

        /// <summary>
        ///     获取委托关联表达式
        /// </summary>
        /// <param name="body">表达式主体</param>
        /// <param name="parameters">参数</param>
        /// <returns>委托关联表达式</returns>
        internal static TmphLambdaExpression Get(TmphExpression body, TmphParameterExpression[] parameters)
        {
            var expression = TmphTypePool<TmphLambdaExpression>.Pop() ?? new TmphLambdaExpression();
            expression.set(body, parameters);
            return expression;
        }

        /// <summary>
        ///     表达式转换
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>表达式</returns>
        internal static TmphLambdaExpression convert(LambdaExpression expression)
        {
            return Get(convert(expression.Body)
                , expression.Parameters.GetArray(value => TmphParameterExpression.convert(value)));
        }
    }
}