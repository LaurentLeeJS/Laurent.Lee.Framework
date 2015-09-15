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