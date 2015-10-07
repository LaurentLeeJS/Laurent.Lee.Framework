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
    ///     二元表达式
    /// </summary>
    internal class TmphBinaryExpression : TmphExpression
    {
        /// <summary>
        ///     获取简单表达式
        /// </summary>
        private static readonly Func<TmphBinaryExpression, TmphExpression>[] getSimpleExpressions;

        static TmphBinaryExpression()
        {
            getSimpleExpressions =
                new Func<TmphBinaryExpression, TmphExpression>[TmphEnum.GetMaxValue<TmphExpressionType>(-1) + 1];
            getSimpleExpressions[(int)TmphExpressionType.OrElse] = getSimpleOrElse;
            getSimpleExpressions[(int)TmphExpressionType.AndAlso] = getSimpleAndAlso;
            getSimpleExpressions[(int)TmphExpressionType.Equal] = getSimpleEqual;
            getSimpleExpressions[(int)TmphExpressionType.NotEqual] = getSimpleNotEqual;
            getSimpleExpressions[(int)TmphExpressionType.GreaterThanOrEqual] = getSimpleGreaterThanOrEqual;
            getSimpleExpressions[(int)TmphExpressionType.GreaterThan] = getSimpleGreaterThan;
            getSimpleExpressions[(int)TmphExpressionType.LessThan] = getSimpleLessThan;
            getSimpleExpressions[(int)TmphExpressionType.LessThanOrEqual] = getSimpleLessThanOrEqual;
            //getSimpleExpressions[(int)expressionType.Add] = getSimpleAdd;
            //getSimpleExpressions[(int)expressionType.AddChecked] = getSimpleAdd;
            //getSimpleExpressions[(int)expressionType.Subtract] = getSimpleSubtract;
            //getSimpleExpressions[(int)expressionType.SubtractChecked] = getSimpleSubtract;
            //getSimpleExpressions[(int)expressionType.Multiply] = getSimpleMultiply;
            //getSimpleExpressions[(int)expressionType.MultiplyChecked] = getSimpleMultiply;
            //getSimpleExpressions[(int)expressionType.Divide] = getSimpleDivide;
            //getSimpleExpressions[(int)expressionType.Modulo] = getSimpleModulo;
            //getSimpleExpressions[(int)expressionType.Power] = getSimplePower;
            //getSimpleExpressions[(int)expressionType.Or] = getSimpleOr;
            //getSimpleExpressions[(int)expressionType.And] = getSimpleAnd;
            //getSimpleExpressions[(int)expressionType.ExclusiveOr] = getSimpleExclusiveOr;
            //getSimpleExpressions[(int)expressionType.LeftShift] = getSimpleLeftShift;
            //getSimpleExpressions[(int)expressionType.RightShift] = getSimpleRightShift;
        }

        /// <summary>
        ///     左表达式
        /// </summary>
        public TmphExpression Left { get; private set; }

        /// <summary>
        ///     右表达式
        /// </summary>
        public TmphExpression Right { get; private set; }

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
                if (Left.IsConstant && Right.IsConstant && Method != null)
                {
                    var parameters = TmphTypePool<TmphBinaryExpression, object[]>.Default.Pop() ?? new object[2];
                    object value;
                    try
                    {
                        parameters[0] = ((TmphConstantExpression)Left).Value;
                        parameters[1] = ((TmphConstantExpression)Right).Value;
                        value = Method.Invoke(null, parameters);
                    }
                    finally
                    {
                        pushObjectArray2(ref parameters);
                    }
                    PushPool();
                    return TmphConstantExpression.Get(value);
                }
                var getSimpleExpression = getSimpleExpressions[(int)NodeType];
                if (getSimpleExpression != null) return getSimpleExpression(this);
                return this;
            }
        }

        /// <summary>
        ///     添加对象数组
        /// </summary>
        /// <param name="value">对象数组</param>
        private static void pushObjectArray2(ref object[] value)
        {
            value[0] = value[1] = null;
            TmphTypePool<TmphBinaryExpression, object[]>.Default.Push(ref value);
        }

        /// <summary>
        ///     添加到表达式池
        /// </summary>
        internal override void pushPool()
        {
            Method = null;
            if (Left != null)
            {
                Left.PushCountPool();
                Left = null;
            }
            if (Right != null)
            {
                Right.PushCountPool();
                Right = null;
            }
            TmphTypePool<TmphBinaryExpression>.Push(this);
        }

        /// <summary>
        ///     获取二元表达式
        /// </summary>
        /// <param name="type">表达式类型</param>
        /// <param name="left">左表达式</param>
        /// <param name="right">右表达式</param>
        /// <param name="method">运算符重载函数</param>
        /// <returns>二元表达式</returns>
        internal static TmphBinaryExpression Get(TmphExpressionType type, TmphExpression left, TmphExpression right,
            MethodInfo method)
        {
            var expression = TmphTypePool<TmphBinaryExpression>.Pop() ?? new TmphBinaryExpression();
            expression.NodeType = type;
            expression.Method = method;
            ++(expression.Left = left.SimpleExpression).ExpressionCount;
            ++(expression.Right = right.SimpleExpression).ExpressionCount;
            return expression;
        }

        /// <summary>
        ///     获取简单表达式
        /// </summary>
        /// <param name="binaryExpression">二元表达式</param>
        /// <returns>简单表达式</returns>
        private static TmphExpression getSimpleOrElse(TmphBinaryExpression binaryExpression)
        {
            if (binaryExpression.Left.NodeType == TmphExpressionType.LogicConstant)
            {
                TmphExpression expression;
                if (((TmphLogicConstantExpression)binaryExpression.Left).Value)
                {
                    expression = binaryExpression.Left;
                    binaryExpression.Left = null;
                }
                else
                {
                    expression = binaryExpression.Right;
                    binaryExpression.Right = null;
                }
                --expression.ExpressionCount;
                binaryExpression.PushPool();
                return expression;
            }
            if (binaryExpression.Right.NodeType == TmphExpressionType.LogicConstant)
            {
                TmphExpression expression;
                if (((TmphLogicConstantExpression)binaryExpression.Right).Value)
                {
                    expression = binaryExpression.Right;
                    binaryExpression.Right = null;
                }
                else
                {
                    expression = binaryExpression.Left;
                    binaryExpression.Left = null;
                }
                --expression.ExpressionCount;
                binaryExpression.PushPool();
                return expression;
            }
            return binaryExpression;
        }

        /// <summary>
        ///     获取简单表达式
        /// </summary>
        /// <param name="binaryExpression">二元表达式</param>
        /// <returns>简单表达式</returns>
        private static TmphExpression getSimpleAndAlso(TmphBinaryExpression binaryExpression)
        {
            if (binaryExpression.Left.NodeType == TmphExpressionType.LogicConstant)
            {
                TmphExpression expression;
                if (((TmphLogicConstantExpression)binaryExpression.Left).Value)
                {
                    expression = binaryExpression.Right;
                    binaryExpression.Right = null;
                }
                else
                {
                    expression = binaryExpression.Left;
                    binaryExpression.Left = null;
                }
                --expression.ExpressionCount;
                binaryExpression.PushPool();
                return expression;
            }
            if (binaryExpression.Right.NodeType == TmphExpressionType.LogicConstant)
            {
                TmphExpression expression;
                if (((TmphLogicConstantExpression)binaryExpression.Right).Value)
                {
                    expression = binaryExpression.Left;
                    binaryExpression.Left = null;
                }
                else
                {
                    expression = binaryExpression.Right;
                    binaryExpression.Right = null;
                }
                --expression.ExpressionCount;
                binaryExpression.PushPool();
                return expression;
            }
            return binaryExpression;
        }

        /// <summary>
        ///     获取简单表达式
        /// </summary>
        /// <param name="binaryExpression">二元表达式</param>
        /// <returns>简单表达式</returns>
        private static TmphExpression getSimpleEqual(TmphBinaryExpression binaryExpression)
        {
            if (binaryExpression.Left.IsConstant && binaryExpression.Right.IsConstant)
            {
                var leftValue = ((TmphConstantExpression)binaryExpression.Left).Value;
                var rightValue = ((TmphConstantExpression)binaryExpression.Right).Value;
                binaryExpression.PushPool();
                return
                    TmphLogicConstantExpression.Get(leftValue == null ? rightValue == null : leftValue.Equals(rightValue));
            }
            return binaryExpression;
        }

        /// <summary>
        ///     获取简单表达式
        /// </summary>
        /// <param name="binaryExpression">二元表达式</param>
        /// <returns>简单表达式</returns>
        private static TmphExpression getSimpleNotEqual(TmphBinaryExpression binaryExpression)
        {
            if (binaryExpression.Left.IsConstant && binaryExpression.Right.IsConstant)
            {
                var leftValue = ((TmphConstantExpression)binaryExpression.Left).Value;
                var rightValue = ((TmphConstantExpression)binaryExpression.Right).Value;
                binaryExpression.PushPool();
                return
                    TmphLogicConstantExpression.Get(leftValue == null ? rightValue != null : !leftValue.Equals(rightValue));
            }
            return binaryExpression;
        }

        /// <summary>
        ///     获取简单表达式
        /// </summary>
        /// <param name="binaryExpression">二元表达式</param>
        /// <returns>简单表达式</returns>
        private static TmphExpression getSimpleGreaterThanOrEqual(TmphBinaryExpression binaryExpression)
        {
            if (binaryExpression.Left.IsConstant && binaryExpression.Right.IsConstant)
            {
                var value = ((IComparable)((TmphConstantExpression)binaryExpression.Left).Value)
                    .CompareTo(((TmphConstantExpression)binaryExpression.Right).Value);
                binaryExpression.PushPool();
                return TmphLogicConstantExpression.Get(value >= 0);
            }
            return binaryExpression;
        }

        /// <summary>
        ///     获取简单表达式
        /// </summary>
        /// <param name="binaryExpression">二元表达式</param>
        /// <returns>简单表达式</returns>
        private static TmphExpression getSimpleGreaterThan(TmphBinaryExpression binaryExpression)
        {
            if (binaryExpression.Left.IsConstant && binaryExpression.Right.IsConstant)
            {
                var value = ((IComparable)((TmphConstantExpression)binaryExpression.Left).Value)
                    .CompareTo(((TmphConstantExpression)binaryExpression.Right).Value);
                binaryExpression.PushPool();
                return TmphLogicConstantExpression.Get(value > 0);
            }
            return binaryExpression;
        }

        /// <summary>
        ///     获取简单表达式
        /// </summary>
        /// <param name="binaryExpression">二元表达式</param>
        /// <returns>简单表达式</returns>
        private static TmphExpression getSimpleLessThan(TmphBinaryExpression binaryExpression)
        {
            if (binaryExpression.Left.IsConstant && binaryExpression.Right.IsConstant)
            {
                var value = ((IComparable)((TmphConstantExpression)binaryExpression.Left).Value)
                    .CompareTo(((TmphConstantExpression)binaryExpression.Right).Value);
                binaryExpression.PushPool();
                return TmphLogicConstantExpression.Get(value < 0);
            }
            return binaryExpression;
        }

        /// <summary>
        ///     获取简单表达式
        /// </summary>
        /// <param name="binaryExpression">二元表达式</param>
        /// <returns>简单表达式</returns>
        private static TmphExpression getSimpleLessThanOrEqual(TmphBinaryExpression binaryExpression)
        {
            if (binaryExpression.Left.IsConstant && binaryExpression.Right.IsConstant)
            {
                var value = ((IComparable)((TmphConstantExpression)binaryExpression.Left).Value)
                    .CompareTo(((TmphConstantExpression)binaryExpression.Right).Value);
                binaryExpression.PushPool();
                return TmphLogicConstantExpression.Get(value <= 0);
            }
            return binaryExpression;
        }
    }
}