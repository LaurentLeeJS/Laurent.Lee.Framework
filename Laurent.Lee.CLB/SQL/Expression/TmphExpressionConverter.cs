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

namespace Laurent.Lee.CLB.Sql.Expression
{
    /// <summary>
    ///     表达式转换
    /// </summary>
    internal abstract class TmphExpressionConverter
    {
        /// <summary>
        ///     表达式转换处理集合
        /// </summary>
        protected Action<TmphConverter, TmphExpression>[] converters;

        /// <summary>
        ///     表达式转换
        /// </summary>
        protected TmphExpressionConverter()
        {
            converters = new Action<TmphConverter, TmphExpression>[TmphEnum.GetMaxValue<TmphExpressionType>(-1) + 1];
            converters[(int)TmphExpressionType.OrElse] = convertOrElse;
            converters[(int)TmphExpressionType.AndAlso] = convertAndAlso;
            converters[(int)TmphExpressionType.Equal] = convertEqual;
            converters[(int)TmphExpressionType.NotEqual] = convertNotEqual;
            converters[(int)TmphExpressionType.GreaterThanOrEqual] = convertGreaterThanOrEqual;
            converters[(int)TmphExpressionType.GreaterThan] = convertGreaterThan;
            converters[(int)TmphExpressionType.LessThan] = convertLessThan;
            converters[(int)TmphExpressionType.LessThanOrEqual] = convertLessThanOrEqual;
            converters[(int)TmphExpressionType.Add] = convertAdd;
            converters[(int)TmphExpressionType.AddChecked] = convertAdd;
            converters[(int)TmphExpressionType.Subtract] = convertSubtract;
            converters[(int)TmphExpressionType.SubtractChecked] = convertSubtract;
            converters[(int)TmphExpressionType.Multiply] = convertMultiply;
            converters[(int)TmphExpressionType.MultiplyChecked] = convertMultiply;
            converters[(int)TmphExpressionType.Divide] = convertDivide;
            converters[(int)TmphExpressionType.Modulo] = convertModulo;
            converters[(int)TmphExpressionType.Power] = convertPower;
            converters[(int)TmphExpressionType.Or] = convertOr;
            converters[(int)TmphExpressionType.And] = convertAnd;
            converters[(int)TmphExpressionType.ExclusiveOr] = convertExclusiveOr;
            converters[(int)TmphExpressionType.LeftShift] = convertLeftShift;
            converters[(int)TmphExpressionType.RightShift] = convertRightShift;
            converters[(int)TmphExpressionType.FieldAccess] = convertFieldAccess;
            converters[(int)TmphExpressionType.PropertyAccess] = convertPropertyAccess;
            converters[(int)TmphExpressionType.Not] = convertNot;
            converters[(int)TmphExpressionType.Unbox] = convertUnbox;
            converters[(int)TmphExpressionType.Negate] = convertNegate;
            converters[(int)TmphExpressionType.NegateChecked] = convertNegate;
            converters[(int)TmphExpressionType.UnaryPlus] = convertUnaryPlus;
            converters[(int)TmphExpressionType.IsTrue] = convertIsTrue;
            converters[(int)TmphExpressionType.IsFalse] = convertIsFalse;
            converters[(int)TmphExpressionType.Convert] = convertConvert;
            converters[(int)TmphExpressionType.ConvertChecked] = convertConvert;
            converters[(int)TmphExpressionType.Conditional] = convertConditional;
            converters[(int)TmphExpressionType.Call] = convertCall;
        }

        /// <summary>
        ///     获取常量转换处理函数
        /// </summary>
        /// <param name="type">数据类型</param>
        /// <returns>失败返回null</returns>
        public Action<TmphConverter, TmphExpression> this[TmphExpressionType type]
        {
            get { return converters[(int)type]; }
        }

        /// <summary>
        ///     转换表达式
        /// </summary>
        /// <param name="converter">表达式转换器</param>
        /// <param name="expression">表达式</param>
        private void convertOrElse(TmphConverter converter, TmphExpression expression)
        {
            convertBinaryExpression(converter, (TmphBinaryExpression)expression, "or");
        }

        /// <summary>
        ///     转换表达式
        /// </summary>
        /// <param name="converter">表达式转换器</param>
        /// <param name="expression">表达式</param>
        private void convertAndAlso(TmphConverter converter, TmphExpression expression)
        {
            convertBinaryExpression(converter, (TmphBinaryExpression)expression, "and");
        }

        /// <summary>
        ///     转换表达式
        /// </summary>
        /// <param name="converter">表达式转换器</param>
        /// <param name="expression">表达式</param>
        private void convertEqual(TmphConverter converter, TmphExpression expression)
        {
            var binaryExpression = (TmphBinaryExpression)expression;
            if (binaryExpression.Left.IsConstantNull)
            {
                var stream = converter.Stream;
                if (binaryExpression.Right.IsSimple)
                {
                    converters[(int)binaryExpression.Right.NodeType](converter, binaryExpression.Right);
                }
                else
                {
                    stream.Write('(');
                    converters[(int)binaryExpression.Right.NodeType](converter, binaryExpression.Right);
                    stream.Write(')');
                }
                stream.WriteNotNull(" is null");
            }
            else if (binaryExpression.Right.IsConstantNull)
            {
                var stream = converter.Stream;
                if (binaryExpression.Left.IsSimple)
                {
                    converters[(int)binaryExpression.Left.NodeType](converter, binaryExpression.Left);
                }
                else
                {
                    stream.Write('(');
                    converters[(int)binaryExpression.Left.NodeType](converter, binaryExpression.Left);
                    stream.Write(')');
                }
                stream.WriteNotNull(" is null");
            }
            else convertBinaryExpression(converter, (TmphBinaryExpression)expression, '=');
        }

        /// <summary>
        ///     转换表达式
        /// </summary>
        /// <param name="converter">表达式转换器</param>
        /// <param name="expression">表达式</param>
        private void convertNotEqual(TmphConverter converter, TmphExpression expression)
        {
            var binaryExpression = (TmphBinaryExpression)expression;
            if (binaryExpression.Left.IsConstantNull)
            {
                var stream = converter.Stream;
                if (binaryExpression.Right.IsSimple)
                {
                    converters[(int)binaryExpression.Right.NodeType](converter, binaryExpression.Right);
                }
                else
                {
                    stream.Write('(');
                    converters[(int)binaryExpression.Right.NodeType](converter, binaryExpression.Right);
                    stream.Write(')');
                }
                stream.WriteNotNull(" is not null");
            }
            else if (binaryExpression.Right.IsConstantNull)
            {
                var stream = converter.Stream;
                if (binaryExpression.Left.IsSimple)
                {
                    converters[(int)binaryExpression.Left.NodeType](converter, binaryExpression.Left);
                }
                else
                {
                    stream.Write('(');
                    converters[(int)binaryExpression.Left.NodeType](converter, binaryExpression.Left);
                    stream.Write(')');
                }
                stream.WriteNotNull(" is not null");
            }
            else convertNotEqual(converter, (TmphBinaryExpression)expression);
        }

        /// <summary>
        ///     转换表达式
        /// </summary>
        /// <param name="converter">表达式转换器</param>
        /// <param name="expression">表达式</param>
        protected virtual void convertNotEqual(TmphConverter converter, TmphBinaryExpression expression)
        {
            convertBinaryExpression(converter, expression, '<', '>');
        }

        /// <summary>
        ///     转换表达式
        /// </summary>
        /// <param name="converter">表达式转换器</param>
        /// <param name="expression">表达式</param>
        private void convertGreaterThanOrEqual(TmphConverter converter, TmphExpression expression)
        {
            convertBinaryExpression(converter, (TmphBinaryExpression)expression, '>', '=');
        }

        /// <summary>
        ///     转换表达式
        /// </summary>
        /// <param name="converter">表达式转换器</param>
        /// <param name="expression">表达式</param>
        private void convertGreaterThan(TmphConverter converter, TmphExpression expression)
        {
            convertBinaryExpression(converter, (TmphBinaryExpression)expression, '>');
        }

        /// <summary>
        ///     转换表达式
        /// </summary>
        /// <param name="converter">表达式转换器</param>
        /// <param name="expression">表达式</param>
        private void convertLessThan(TmphConverter converter, TmphExpression expression)
        {
            convertBinaryExpression(converter, (TmphBinaryExpression)expression, '<');
        }

        /// <summary>
        ///     转换表达式
        /// </summary>
        /// <param name="converter">表达式转换器</param>
        /// <param name="expression">表达式</param>
        private void convertLessThanOrEqual(TmphConverter converter, TmphExpression expression)
        {
            convertBinaryExpression(converter, (TmphBinaryExpression)expression, '<', '=');
        }

        /// <summary>
        ///     转换表达式
        /// </summary>
        /// <param name="converter">表达式转换器</param>
        /// <param name="expression">表达式</param>
        private void convertAdd(TmphConverter converter, TmphExpression expression)
        {
            convertBinaryExpression(converter, (TmphBinaryExpression)expression, '+');
        }

        /// <summary>
        ///     转换表达式
        /// </summary>
        /// <param name="converter">表达式转换器</param>
        /// <param name="expression">表达式</param>
        private void convertSubtract(TmphConverter converter, TmphExpression expression)
        {
            convertBinaryExpression(converter, (TmphBinaryExpression)expression, '-');
        }

        /// <summary>
        ///     转换表达式
        /// </summary>
        /// <param name="converter">表达式转换器</param>
        /// <param name="expression">表达式</param>
        private void convertMultiply(TmphConverter converter, TmphExpression expression)
        {
            convertBinaryExpression(converter, (TmphBinaryExpression)expression, '*');
        }

        /// <summary>
        ///     转换表达式
        /// </summary>
        /// <param name="converter">表达式转换器</param>
        /// <param name="expression">表达式</param>
        private void convertDivide(TmphConverter converter, TmphExpression expression)
        {
            convertBinaryExpression(converter, (TmphBinaryExpression)expression, '/');
        }

        /// <summary>
        ///     转换表达式
        /// </summary>
        /// <param name="converter">表达式转换器</param>
        /// <param name="expression">表达式</param>
        private void convertModulo(TmphConverter converter, TmphExpression expression)
        {
            convertBinaryExpression(converter, (TmphBinaryExpression)expression, '%');
        }

        /// <summary>
        ///     转换表达式
        /// </summary>
        /// <param name="converter">表达式转换器</param>
        /// <param name="expression">表达式</param>
        private void convertPower(TmphConverter converter, TmphExpression expression)
        {
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
        }

        /// <summary>
        ///     转换表达式
        /// </summary>
        /// <param name="converter">表达式转换器</param>
        /// <param name="expression">表达式</param>
        private void convertOr(TmphConverter converter, TmphExpression expression)
        {
            convertBinaryExpression(converter, (TmphBinaryExpression)expression, '|');
        }

        /// <summary>
        ///     转换表达式
        /// </summary>
        /// <param name="converter">表达式转换器</param>
        /// <param name="expression">表达式</param>
        private void convertAnd(TmphConverter converter, TmphExpression expression)
        {
            convertBinaryExpression(converter, (TmphBinaryExpression)expression, '&');
        }

        /// <summary>
        ///     转换表达式
        /// </summary>
        /// <param name="converter">表达式转换器</param>
        /// <param name="expression">表达式</param>
        private void convertExclusiveOr(TmphConverter converter, TmphExpression expression)
        {
            convertBinaryExpression(converter, (TmphBinaryExpression)expression, '^');
        }

        /// <summary>
        ///     转换表达式
        /// </summary>
        /// <param name="converter">表达式转换器</param>
        /// <param name="expression">表达式</param>
        private void convertLeftShift(TmphConverter converter, TmphExpression expression)
        {
            convertBinaryExpression(converter, (TmphBinaryExpression)expression, '<', '<');
        }

        /// <summary>
        ///     转换表达式
        /// </summary>
        /// <param name="converter">表达式转换器</param>
        /// <param name="expression">表达式</param>
        private void convertRightShift(TmphConverter converter, TmphExpression expression)
        {
            convertBinaryExpression(converter, (TmphBinaryExpression)expression, '>', '>');
        }

        /// <summary>
        ///     转换表达式
        /// </summary>
        /// <param name="converter">表达式转换器</param>
        /// <param name="expression">表达式</param>
        /// <param name="char1">操作字符1</param>
        /// <param name="char2">操作字符2</param>
        protected void convertBinaryExpression(TmphConverter converter, TmphBinaryExpression binaryExpression,
            char char1, char char2 = ' ')
        {
            var stream = converter.Stream;
            TmphExpression left = binaryExpression.Left, right = binaryExpression.Right;
            if (left.IsSimple)
            {
                converters[(int)left.NodeType](converter, left);
            }
            else
            {
                stream.Write('(');
                converters[(int)left.NodeType](converter, left);
                stream.Write(')');
            }
            stream.Write(char1);
            stream.Write(char2);
            if (right.IsSimple)
            {
                converters[(int)right.NodeType](converter, right);
            }
            else
            {
                stream.Write('(');
                converters[(int)right.NodeType](converter, right);
                stream.Write(')');
            }
        }

        /// <summary>
        ///     转换表达式
        /// </summary>
        /// <param name="converter">表达式转换器</param>
        /// <param name="expression">表达式</param>
        /// <param name="type">操作字符串</param>
        private void convertBinaryExpression(TmphConverter converter, TmphBinaryExpression binaryExpression, string type)
        {
            var stream = converter.Stream;
            TmphExpression left = binaryExpression.Left, right = binaryExpression.Right;
            stream.Write('(');
            if (left.IsSimple)
            {
                converters[(int)left.NodeType](converter, left);
                stream.Write('=');
                stream.Write('1');
            }
            else converters[(int)left.NodeType](converter, left);
            stream.Write(')');
            stream.WriteNotNull(type);
            stream.Write('(');
            if (right.IsSimple)
            {
                converters[(int)right.NodeType](converter, right);
                stream.Write('=');
                stream.Write('1');
            }
            else converters[(int)right.NodeType](converter, right);
            stream.Write(')');
        }

        /// <summary>
        ///     转换表达式
        /// </summary>
        /// <param name="converter">表达式转换器</param>
        /// <param name="expression">表达式</param>
        /// <param name="name">成员名称</param>
        private void convertMemberAccess(TmphConverter converter, TmphMemberExpression memberExpression, string name)
        {
            if (memberExpression.Expression.GetType() == typeof(TmphParameterExpression))
            {
                if (converter.FirstMemberName == null) converter.FirstMemberName = name;
                converter.Stream.WriteNotNull(name);
            }
            else TmphLog.Error.Throw("未知成员表达式类型 " + memberExpression.Expression.GetType().Name, false, true);
        }

        /// <summary>
        ///     转换表达式
        /// </summary>
        /// <param name="converter">表达式转换器</param>
        /// <param name="expression">表达式</param>
        private void convertFieldAccess(TmphConverter converter, TmphExpression expression)
        {
            var fieldExpression = (TmphFieldExpression)expression;
            convertMemberAccess(converter, fieldExpression, fieldExpression.FieldInfo.Name);
        }

        /// <summary>
        ///     转换表达式
        /// </summary>
        /// <param name="converter">表达式转换器</param>
        /// <param name="expression">表达式</param>
        private void convertPropertyAccess(TmphConverter converter, TmphExpression expression)
        {
            var propertyExpression = (TmphPropertyExpression)expression;
            convertMemberAccess(converter, propertyExpression, propertyExpression.PropertyInfo.Name);
        }

        /// <summary>
        ///     转换表达式
        /// </summary>
        /// <param name="converter">表达式转换器</param>
        /// <param name="expression">表达式</param>
        private void convertNot(TmphConverter converter, TmphExpression expression)
        {
            expression = ((TmphUnaryExpression)expression).Expression;
            if (expression.IsSimple)
            {
                converters[(int)expression.NodeType](converter, expression);
                converter.Stream.WriteNotNull("=0");
            }
            else
            {
                converter.Stream.Write('(');
                converters[(int)expression.NodeType](converter, expression);
                converter.Stream.WriteNotNull(")=0");
            }
        }

        /// <summary>
        ///     转换表达式
        /// </summary>
        /// <param name="converter">表达式转换器</param>
        /// <param name="unaryExpression">表达式</param>
        private void convertUnbox(TmphConverter converter, TmphExpression expression)
        {
            expression = ((TmphUnaryExpression)expression).Expression;
            if (expression.IsSimple)
            {
                converters[(int)expression.NodeType](converter, expression);
            }
            else
            {
                converter.Stream.Write('(');
                converters[(int)expression.NodeType](converter, expression);
                converter.Stream.Write(')');
            }
        }

        /// <summary>
        ///     转换表达式
        /// </summary>
        /// <param name="converter">表达式转换器</param>
        /// <param name="unaryExpression">表达式</param>
        private void convertNegate(TmphConverter converter, TmphExpression expression)
        {
            expression = ((TmphUnaryExpression)expression).Expression;
            converter.Stream.WriteNotNull("-(");
            converters[(int)expression.NodeType](converter, expression);
            converter.Stream.Write(')');
        }

        /// <summary>
        ///     转换表达式
        /// </summary>
        /// <param name="converter">表达式转换器</param>
        /// <param name="unaryExpression">表达式</param>
        private void convertUnaryPlus(TmphConverter converter, TmphExpression expression)
        {
            expression = ((TmphUnaryExpression)expression).Expression;
            converter.Stream.WriteNotNull("+(");
            converters[(int)expression.NodeType](converter, expression);
            converter.Stream.Write(')');
        }

        /// <summary>
        ///     转换表达式
        /// </summary>
        /// <param name="converter">表达式转换器</param>
        /// <param name="unaryExpression">表达式</param>
        private void convertIsTrue(TmphConverter converter, TmphExpression expression)
        {
            expression = ((TmphUnaryExpression)expression).Expression;
            if (expression.IsSimple)
            {
                converters[(int)expression.NodeType](converter, expression);
                converter.Stream.WriteNotNull("=1");
            }
            else
            {
                converter.Stream.Write('(');
                converters[(int)expression.NodeType](converter, expression);
                converter.Stream.WriteNotNull(")=1");
            }
        }

        /// <summary>
        ///     转换表达式
        /// </summary>
        /// <param name="converter">表达式转换器</param>
        /// <param name="unaryExpression">表达式</param>
        private void convertIsFalse(TmphConverter converter, TmphExpression expression)
        {
            expression = ((TmphUnaryExpression)expression).Expression;
            if (expression.IsSimple)
            {
                converters[(int)expression.NodeType](converter, expression);
                converter.Stream.WriteNotNull("=0");
            }
            else
            {
                converter.Stream.Write('(');
                converters[(int)expression.NodeType](converter, expression);
                converter.Stream.WriteNotNull(")=0");
            }
        }

        /// <summary>
        ///     转换表达式
        /// </summary>
        /// <param name="converter">表达式转换器</param>
        /// <param name="unaryExpression">表达式</param>
        private void convertConvert(TmphConverter converter, TmphExpression expression)
        {
            var convertExpression = (TmphConvertExpression)expression;
            converter.Stream.WriteNotNull("cast(");
            converters[(int)convertExpression.Expression.NodeType](converter, convertExpression.Expression);
            converter.Stream.WriteNotNull(" as ");
            converter.Stream.WriteNotNull(convertExpression.ConvertType.formCSharpType().ToString());
            converter.Stream.Write(')');
        }

        /// <summary>
        ///     转换表达式
        /// </summary>
        /// <param name="converter">表达式转换器</param>
        /// <param name="expression">表达式</param>
        private void convertConditional(TmphConverter converter, TmphExpression expression)
        {
            var stream = converter.Stream;
            var conditionalExpression = (TmphConditionalExpression)expression;
            TmphExpression test = conditionalExpression.Test,
                ifTrue = conditionalExpression.IfTrue,
                ifFalse = conditionalExpression.IfFalse;
            stream.WriteNotNull("case when ");
            if (test.IsSimple)
            {
                converters[(int)test.NodeType](converter, test);
                stream.Write('=');
                stream.Write('1');
            }
            else converters[(int)test.NodeType](converter, test);
            stream.WriteNotNull(" then ");
            if (ifTrue.IsSimple) converters[(int)ifTrue.NodeType](converter, ifTrue);
            else
            {
                stream.Write('(');
                converters[(int)ifTrue.NodeType](converter, ifTrue);
                stream.Write(')');
            }
            stream.WriteNotNull(" else ");
            if (ifFalse.IsSimple) converters[(int)ifFalse.NodeType](converter, ifFalse);
            else
            {
                stream.Write('(');
                converters[(int)ifFalse.NodeType](converter, ifFalse);
                stream.Write(')');
            }
            stream.WriteNotNull(" end");
        }

        /// <summary>
        ///     转换表达式
        /// </summary>
        /// <param name="converter">表达式转换器</param>
        /// <param name="expression">表达式</param>
        private void convertCall(TmphConverter converter, TmphExpression expression)
        {
            var methodCallExpression = (TmphMethodCallExpression)expression;
            var stream = converter.Stream;
            stream.WriteNotNull(methodCallExpression.Method.Name);
            stream.Write('(');
            if (methodCallExpression.Arguments != null)
            {
                var isNext = false;
                foreach (var argumentExpression in methodCallExpression.Arguments)
                {
                    if (isNext) stream.Write(',');
                    converters[(int)argumentExpression.NodeType](converter, argumentExpression);
                    isNext = true;
                }
            }
            stream.Write(')');
        }
    }
}