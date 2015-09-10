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

using System.Collections;
using System.Reflection;

namespace Laurent.Lee.CLB.Sql.Expression
{
    /// <summary>
    ///     函数调用表达式
    /// </summary>
    internal sealed class TmphMethodCallExpression : TmphExpression
    {
        /// <summary>
        ///     函数调用表达式
        /// </summary>
        private TmphMethodCallExpression()
        {
            NodeType = TmphExpressionType.Call;
            Arguments = TmphNullValue<TmphExpression>.Array;
            IsSimple = true;
        }

        /// <summary>
        ///     动态函数对象表达式
        /// </summary>
        public TmphExpression Instance { get; private set; }

        /// <summary>
        ///     函数信息
        /// </summary>
        public MethodInfo Method { get; private set; }

        /// <summary>
        ///     调用参数
        /// </summary>
        public TmphExpression[] Arguments { get; private set; }

        /// <summary>
        ///     简单表达式
        /// </summary>
        public override TmphExpression SimpleExpression
        {
            get
            {
                if (Method.ReflectedType != typeof(TmphExpressionCall))
                {
                    var value = Method.Invoke(Instance == null ? null : ((TmphConstantExpression)Instance).Value
                        , Arguments.getArray(argumentExpression => ((TmphConstantExpression)argumentExpression).Value));
                    PushPool();
                    return TmphConstantExpression.Get(value);
                }
                if (Method.Name == "In")
                {
                    var values = ((TmphConstantExpression)Arguments[1]).Value;
                    if (values != null)
                    {
                        var index = 0;
                        object firstValue = null;
                        foreach (var value in (IEnumerable)values)
                        {
                            if (index != 0)
                            {
                                TmphExpression left = Arguments[0], right = Arguments[1];
                                Arguments[0] = Arguments[1] = null;
                                PushPool();
                                return TmphBinaryExpression.Get(TmphExpressionType.InSet, left, right, null);
                            }
                            firstValue = value;
                            ++index;
                        }
                        if (index != 0)
                        {
                            var expression = Arguments[0];
                            Arguments[0] = null;
                            PushPool();
                            return
                                TmphBinaryExpression.Get(TmphExpressionType.Equal, expression,
                                    TmphConstantExpression.Get(firstValue), null).SimpleExpression;
                        }
                    }
                    PushPool();
                    return TmphLogicConstantExpression.Get(false);
                }
                return this;
            }
        }

        /// <summary>
        ///     函数调用表达式
        /// </summary>
        /// <param name="method">函数信息</param>
        /// <param name="instance">动态函数对象表达式</param>
        /// <param name="arguments">调用参数</param>
        private void set(MethodInfo method, TmphExpression instance, TmphExpression[] arguments)
        {
            Method = method;
            if (instance != null) ++(Instance = instance.SimpleExpression).ExpressionCount;
            if (arguments.length() != 0)
            {
                var index = 0;
                foreach (var expression in arguments)
                    ++(arguments[index++] = expression.SimpleExpression).ExpressionCount;
                Arguments = arguments;
            }
        }

        /// <summary>
        ///     添加到表达式池
        /// </summary>
        internal override void pushPool()
        {
            Method = null;
            if (Instance != null)
            {
                Instance.PushCountPool();
                Instance = null;
            }
            foreach (var expression in Arguments)
            {
                if (expression != null) expression.PushCountPool();
            }
            Arguments = TmphNullValue<TmphExpression>.Array;
            TmphTypePool<TmphMethodCallExpression>.Push(this);
        }

        /// <summary>
        ///     获取函数调用表达式
        /// </summary>
        /// <param name="method">函数信息</param>
        /// <param name="instance">动态函数对象表达式</param>
        /// <param name="arguments">调用参数</param>
        /// <returns>函数调用表达式</returns>
        internal static TmphMethodCallExpression Get(MethodInfo method, TmphExpression instance, TmphExpression[] arguments)
        {
            var expression = TmphTypePool<TmphMethodCallExpression>.Pop() ?? new TmphMethodCallExpression();
            expression.set(method, instance, arguments);
            return expression;
        }
    }
}