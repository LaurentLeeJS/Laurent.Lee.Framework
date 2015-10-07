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
using System.Reflection;

namespace Laurent.Lee.CLB.Sql.Expression
{
    /// <summary>
    ///     一元表达式
    /// </summary>
    internal class TmphUnaryExpression : TmphExpression
    {
        /// <summary>
        ///     获取简单表达式
        /// </summary>
        private static readonly Func<TmphUnaryExpression, TmphExpression>[] getSimpleExpressions;

        static TmphUnaryExpression()
        {
            getSimpleExpressions =
                new Func<TmphUnaryExpression, TmphExpression>[TmphEnum.GetMaxValue<TmphExpressionType>(-1) + 1];
            getSimpleExpressions[(int)TmphExpressionType.Not] = getSimpleNot;
            //getSimpleExpressions[(int)expressionType.Negate] = getSimpleNegate;
            //getSimpleExpressions[(int)expressionType.NegateChecked] = getSimpleNegate;
            //getSimpleExpressions[(int)expressionType.UnaryPlus] = getSimpleUnaryPlus;
            getSimpleExpressions[(int)TmphExpressionType.IsTrue] = getSimpleIsTrue;
            getSimpleExpressions[(int)TmphExpressionType.IsFalse] = getSimpleIsFalse;
            //getSimpleExpressions[(int)expressionType.Convert] = getSimpleConvert;
            //getSimpleExpressions[(int)expressionType.ConvertChecked] = getSimpleConvert;
        }

        /// <summary>
        ///     表达式
        /// </summary>
        public TmphExpression Expression { get; private set; }

        /// <summary>
        ///     运算符重载函数
        /// </summary>
        public MethodInfo Method { get; private set; }

        /// <summary>
        ///     简单表达式
        /// </summary>
        public override TmphExpression SimpleExpression
        {
            get
            {
                if (Expression.IsConstant && Method != null)
                {
                    var parameters = TmphTypePool<TmphUnaryExpression, object[]>.Default.Pop() ?? new object[1];
                    object value;
                    try
                    {
                        parameters[0] = ((TmphConstantExpression)Expression).Value;
                        value = Method.Invoke(null, parameters);
                    }
                    finally
                    {
                        pushObjectArray(ref parameters);
                    }
                    PushPool();
                    return TmphConstantExpression.Get(value);
                }
                var getSimple = getSimpleExpressions[(int)NodeType];
                if (getSimple != null) return getSimple(this);
                return this;
            }
        }

        /// <summary>
        ///     添加对象数组
        /// </summary>
        /// <param name="value">对象数组</param>
        protected static void pushObjectArray(ref object[] value)
        {
            value[0] = null;
            TmphTypePool<TmphUnaryExpression, object[]>.Default.Push(ref value);
        }

        /// <summary>
        ///     一元表达式
        /// </summary>
        /// <param name="type">表达式类型</param>
        /// <param name="expression">表达式</param>
        /// <param name="method">运算符重载函数</param>
        protected void set(TmphExpressionType type, TmphExpression expression, MethodInfo method)
        {
            NodeType = type;
            Method = method;
            ++(Expression = expression.SimpleExpression).ExpressionCount;
        }

        /// <summary>
        ///     清除数据
        /// </summary>
        protected void clear()
        {
            Method = null;
            Expression.PushCountPool();
            Expression = null;
        }

        /// <summary>
        ///     添加到表达式池
        /// </summary>
        internal override void pushPool()
        {
            clear();
            TmphTypePool<TmphUnaryExpression>.Push(this);
        }

        /// <summary>
        ///     获取一元表达式
        /// </summary>
        /// <param name="type">表达式类型</param>
        /// <param name="expression">表达式</param>
        /// <param name="method">运算符重载函数</param>
        /// <returns>一元表达式</returns>
        internal static TmphUnaryExpression Get(TmphExpressionType type, TmphExpression expression, MethodInfo method)
        {
            var unaryExpression = TmphTypePool<TmphUnaryExpression>.Pop() ?? new TmphUnaryExpression();
            unaryExpression.set(type, expression, method);
            return unaryExpression;
        }

        /// <summary>
        ///     获取简单表达式
        /// </summary>
        /// <param name="binaryExpression">一元表达式</param>
        /// <returns>简单表达式</returns>
        private static TmphExpression getSimpleNot(TmphUnaryExpression binaryExpression)
        {
            if (binaryExpression.Expression.IsConstant)
            {
                var value = !(bool)((TmphConstantExpression)binaryExpression.Expression).Value;
                binaryExpression.PushPool();
                return TmphConstantExpression.Get(value);
            }
            return binaryExpression;
        }

        /// <summary>
        ///     获取简单表达式
        /// </summary>
        /// <param name="binaryExpression">一元表达式</param>
        /// <returns>简单表达式</returns>
        private static TmphExpression getSimpleIsTrue(TmphUnaryExpression binaryExpression)
        {
            if (binaryExpression.Expression.IsConstant)
            {
                var value = (bool)((TmphConstantExpression)binaryExpression.Expression).Value;
                binaryExpression.PushPool();
                return TmphLogicConstantExpression.Get(value);
            }
            return binaryExpression;
        }

        /// <summary>
        ///     获取简单表达式
        /// </summary>
        /// <param name="binaryExpression">一元表达式</param>
        /// <returns>简单表达式</returns>
        private static TmphExpression getSimpleIsFalse(TmphUnaryExpression binaryExpression)
        {
            if (binaryExpression.Expression.IsConstant)
            {
                var value = !(bool)((TmphConstantExpression)binaryExpression.Expression).Value;
                binaryExpression.PushPool();
                return TmphLogicConstantExpression.Get(value);
            }
            return binaryExpression;
        }
    }
}