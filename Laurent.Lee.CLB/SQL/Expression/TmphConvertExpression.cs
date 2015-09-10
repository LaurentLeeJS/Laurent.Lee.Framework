/*
 *  Copyright 2015 Tony Lee

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
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