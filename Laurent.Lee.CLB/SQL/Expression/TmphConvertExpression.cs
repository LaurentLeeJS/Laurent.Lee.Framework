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
    ///     类型转换表达式
    /// </summary>
    internal sealed class TmphConvertExpression : TmphUnaryExpression
    {
        /// <summary>
        ///     强制类型转换
        /// </summary>
        private static readonly MethodInfo convertMethod = typeof(TmphConvertExpression).GetMethod("convert",
            BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     转换目标类型
        /// </summary>
        public Type ConvertType { get; private set; }

        /// <summary>
        ///     简单表达式
        /// </summary>
        public override TmphExpression SimpleExpression
        {
            get
            {
                if (Expression.IsConstant)
                {
                    var parameters = TmphTypePool<TmphUnaryExpression, object[]>.Default.Pop() ?? new object[1];
                    object value;
                    try
                    {
                        parameters[0] = ((TmphConstantExpression)Expression).Value;
                        value = (Method ?? convertMethod.MakeGenericMethod(ConvertType)).Invoke(null, parameters);
                    }
                    finally
                    {
                        pushObjectArray(ref parameters);
                    }
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
            ConvertType = null;
            clear();
            TmphTypePool<TmphConvertExpression>.Push(this);
        }

        /// <summary>
        ///     获取类型转换表达式
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="convertType">转换目标类型</param>
        /// <param name="method">运算符重载函数</param>
        /// <returns>类型转换表达式</returns>
        internal static TmphConvertExpression Get(TmphExpression expression, Type convertType, MethodInfo method)
        {
            var convertExpression = TmphTypePool<TmphConvertExpression>.Pop() ?? new TmphConvertExpression();
            convertExpression.ConvertType = convertType;
            convertExpression.set(TmphExpressionType.Convert, expression, method);
            return convertExpression;
        }

        /// <summary>
        ///     强制类型转换
        /// </summary>
        /// <typeparam name="TValueType">目标类型</typeparam>
        /// <param name="value">被转换的数据</param>
        /// <returns>转换后的数据</returns>
        private static TValueType convert<TValueType>(object value)
        {
            return (TValueType)value;
        }
    }
}