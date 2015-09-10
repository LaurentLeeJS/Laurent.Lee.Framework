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

using Laurent.Lee.CLB.Reflection;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Laurent.Lee.CLB.Sql.Expression
{
    /// <summary>
    ///     表达式
    /// </summary>
    internal abstract class TmphExpression
    {
        /// <summary>
        ///     表达式类型转换集合
        /// </summary>
        private static readonly TmphExpressionType[] expressionTypes;

        /// <summary>
        ///     表达式类型集合
        /// </summary>
        private static readonly Func<System.Linq.Expressions.Expression, TmphExpression>[] converters;

        /// <summary>
        ///     表达式引用计数
        /// </summary>
        internal int ExpressionCount;

        static TmphExpression()
        {
            #region 表达式类型转换集合

            expressionTypes = new TmphExpressionType[TmphEnum.GetMaxValue<ExpressionType>(-1) + 1];
            expressionTypes[(int)ExpressionType.Add] = TmphExpressionType.Add;
            expressionTypes[(int)ExpressionType.AddChecked] = TmphExpressionType.AddChecked;
            expressionTypes[(int)ExpressionType.And] = TmphExpressionType.And;
            expressionTypes[(int)ExpressionType.AndAlso] = TmphExpressionType.AndAlso;
            expressionTypes[(int)ExpressionType.ArrayLength] = TmphExpressionType.ArrayLength;
            expressionTypes[(int)ExpressionType.ArrayIndex] = TmphExpressionType.ArrayIndex;
            expressionTypes[(int)ExpressionType.Call] = TmphExpressionType.Call;
            expressionTypes[(int)ExpressionType.Coalesce] = TmphExpressionType.Coalesce;
            expressionTypes[(int)ExpressionType.Conditional] = TmphExpressionType.Conditional;
            expressionTypes[(int)ExpressionType.Constant] = TmphExpressionType.Constant;
            expressionTypes[(int)ExpressionType.Convert] = TmphExpressionType.Convert;
            expressionTypes[(int)ExpressionType.ConvertChecked] = TmphExpressionType.ConvertChecked;
            expressionTypes[(int)ExpressionType.Divide] = TmphExpressionType.Divide;
            expressionTypes[(int)ExpressionType.Equal] = TmphExpressionType.Equal;
            expressionTypes[(int)ExpressionType.ExclusiveOr] = TmphExpressionType.ExclusiveOr;
            expressionTypes[(int)ExpressionType.GreaterThan] = TmphExpressionType.GreaterThan;
            expressionTypes[(int)ExpressionType.GreaterThanOrEqual] = TmphExpressionType.GreaterThanOrEqual;
            expressionTypes[(int)ExpressionType.Invoke] = TmphExpressionType.Invoke;
            expressionTypes[(int)ExpressionType.Lambda] = TmphExpressionType.Lambda;
            expressionTypes[(int)ExpressionType.LeftShift] = TmphExpressionType.LeftShift;
            expressionTypes[(int)ExpressionType.LessThan] = TmphExpressionType.LessThan;
            expressionTypes[(int)ExpressionType.LessThanOrEqual] = TmphExpressionType.LessThanOrEqual;
            expressionTypes[(int)ExpressionType.ListInit] = TmphExpressionType.ListInit;
            expressionTypes[(int)ExpressionType.MemberAccess] = TmphExpressionType.MemberAccess;
            expressionTypes[(int)ExpressionType.MemberInit] = TmphExpressionType.MemberInit;
            expressionTypes[(int)ExpressionType.Modulo] = TmphExpressionType.Modulo;
            expressionTypes[(int)ExpressionType.Multiply] = TmphExpressionType.Multiply;
            expressionTypes[(int)ExpressionType.MultiplyChecked] = TmphExpressionType.MultiplyChecked;
            expressionTypes[(int)ExpressionType.Negate] = TmphExpressionType.Negate;
            expressionTypes[(int)ExpressionType.UnaryPlus] = TmphExpressionType.UnaryPlus;
            expressionTypes[(int)ExpressionType.NegateChecked] = TmphExpressionType.NegateChecked;
            expressionTypes[(int)ExpressionType.New] = TmphExpressionType.New;
            expressionTypes[(int)ExpressionType.NewArrayInit] = TmphExpressionType.NewArrayInit;
            expressionTypes[(int)ExpressionType.NewArrayBounds] = TmphExpressionType.NewArrayBounds;
            expressionTypes[(int)ExpressionType.Not] = TmphExpressionType.Not;
            expressionTypes[(int)ExpressionType.NotEqual] = TmphExpressionType.NotEqual;
            expressionTypes[(int)ExpressionType.Or] = TmphExpressionType.Or;
            expressionTypes[(int)ExpressionType.OrElse] = TmphExpressionType.OrElse;
            expressionTypes[(int)ExpressionType.Parameter] = TmphExpressionType.Parameter;
            expressionTypes[(int)ExpressionType.Power] = TmphExpressionType.Power;
            expressionTypes[(int)ExpressionType.Quote] = TmphExpressionType.Quote;
            expressionTypes[(int)ExpressionType.RightShift] = TmphExpressionType.RightShift;
            expressionTypes[(int)ExpressionType.Subtract] = TmphExpressionType.Subtract;
            expressionTypes[(int)ExpressionType.SubtractChecked] = TmphExpressionType.SubtractChecked;
            expressionTypes[(int)ExpressionType.TypeAs] = TmphExpressionType.TypeAs;
            expressionTypes[(int)ExpressionType.TypeIs] = TmphExpressionType.TypeIs;

            #endregion 表达式类型转换集合

            #region 表达式类型集合

            converters =
                new Func<System.Linq.Expressions.Expression, TmphExpression>[TmphEnum.GetMaxValue<TmphExpressionType>(-1) + 1];
            converters[(int)TmphExpressionType.Add] = expression =>
           {
               var binaryExpression = (BinaryExpression)expression;
               if (binaryExpression.Method == null)
               {
                   return Add(convert(binaryExpression.Left), convert(binaryExpression.Right));
               }
               return Add(convert(binaryExpression.Left), convert(binaryExpression.Right), binaryExpression.Method);
           };
            converters[(int)TmphExpressionType.AddChecked] = expression =>
           {
               var binaryExpression = (BinaryExpression)expression;
               if (binaryExpression.Method == null)
               {
                   return AddChecked(convert(binaryExpression.Left), convert(binaryExpression.Right));
               }
               return AddChecked(convert(binaryExpression.Left), convert(binaryExpression.Right),
                   binaryExpression.Method);
           };
            converters[(int)TmphExpressionType.And] = expression =>
           {
               var binaryExpression = (BinaryExpression)expression;
               if (binaryExpression.Method == null)
               {
                   return And(convert(binaryExpression.Left), convert(binaryExpression.Right));
               }
               return And(convert(binaryExpression.Left), convert(binaryExpression.Right), binaryExpression.Method);
           };
            converters[(int)TmphExpressionType.AndAlso] = expression =>
           {
               var binaryExpression = (BinaryExpression)expression;
               if (binaryExpression.Method == null)
               {
                   return AndAlso(convert(binaryExpression.Left), convert(binaryExpression.Right));
               }
               return AndAlso(convert(binaryExpression.Left), convert(binaryExpression.Right), binaryExpression.Method);
           };
            converters[(int)TmphExpressionType.Call] = expression =>
           {
               var methodCallExpression = (MethodCallExpression)expression;
               if (methodCallExpression.Object == null)
               {
                   return Call(methodCallExpression.Method,
                       methodCallExpression.Arguments.GetArray(value => convert(value)));
               }
               if (methodCallExpression.Arguments.Count() == 0)
               {
                   return Call(convert(methodCallExpression.Object), methodCallExpression.Method);
               }
               return Call(convert(methodCallExpression.Object), methodCallExpression.Method,
                   methodCallExpression.Arguments.GetArray(value => convert(value)));
           };
            converters[(int)TmphExpressionType.Conditional] = expression =>
           {
               var conditionalExpression = (ConditionalExpression)expression;
               if (conditionalExpression.Type == null)
               {
                   return Condition(convert(conditionalExpression.Test), convert(conditionalExpression.IfTrue),
                       convert(conditionalExpression.IfFalse));
               }
               return Condition(convert(conditionalExpression.Test), convert(conditionalExpression.IfTrue),
                   convert(conditionalExpression.IfFalse), conditionalExpression.Type);
           };
            converters[(int)TmphExpressionType.Constant] = expression =>
           {
               var constantExpression = (ConstantExpression)expression;
               if (constantExpression.Type == null)
               {
                   return Constant(constantExpression.Value);
               }
               return Constant(constantExpression.Value, constantExpression.Type);
           };
            converters[(int)TmphExpressionType.Convert] = expression =>
           {
               var unaryExpression = (UnaryExpression)expression;
               if (unaryExpression.Method == null)
               {
                   return Convert(convert(unaryExpression.Operand), unaryExpression.Type);
               }
               return Convert(convert(unaryExpression.Operand), unaryExpression.Type, unaryExpression.Method);
           };
            converters[(int)TmphExpressionType.ConvertChecked] = expression =>
           {
               var unaryExpression = (UnaryExpression)expression;
               if (unaryExpression.Method == null)
               {
                   return ConvertChecked(convert(unaryExpression.Operand), unaryExpression.Type);
               }
               return ConvertChecked(convert(unaryExpression.Operand), unaryExpression.Type, unaryExpression.Method);
           };
            converters[(int)TmphExpressionType.Divide] = expression =>
           {
               var binaryExpression = (BinaryExpression)expression;
               if (binaryExpression.Method == null)
               {
                   return Divide(convert(binaryExpression.Left), convert(binaryExpression.Right));
               }
               return Divide(convert(binaryExpression.Left), convert(binaryExpression.Right), binaryExpression.Method);
           };
            converters[(int)TmphExpressionType.Equal] = expression =>
           {
               var binaryExpression = (BinaryExpression)expression;
               if (binaryExpression.Method == null)
               {
                   return Equal(convert(binaryExpression.Left), convert(binaryExpression.Right));
               }
               return Equal(convert(binaryExpression.Left), convert(binaryExpression.Right),
                   binaryExpression.IsLiftedToNull, binaryExpression.Method);
           };
            converters[(int)TmphExpressionType.ExclusiveOr] = expression =>
           {
               var binaryExpression = (BinaryExpression)expression;
               if (binaryExpression.Method == null)
               {
                   return ExclusiveOr(convert(binaryExpression.Left), convert(binaryExpression.Right));
               }
               return ExclusiveOr(convert(binaryExpression.Left), convert(binaryExpression.Right),
                   binaryExpression.Method);
           };
            converters[(int)TmphExpressionType.GreaterThan] = expression =>
           {
               var binaryExpression = (BinaryExpression)expression;
               if (binaryExpression.Method == null)
               {
                   return GreaterThan(convert(binaryExpression.Left), convert(binaryExpression.Right));
               }
               return GreaterThan(convert(binaryExpression.Left), convert(binaryExpression.Right),
                   binaryExpression.IsLiftedToNull, binaryExpression.Method);
           };
            converters[(int)TmphExpressionType.GreaterThanOrEqual] = expression =>
           {
               var binaryExpression = (BinaryExpression)expression;
               if (binaryExpression.Method == null)
               {
                   return GreaterThanOrEqual(convert(binaryExpression.Left), convert(binaryExpression.Right));
               }
               return GreaterThanOrEqual(convert(binaryExpression.Left), convert(binaryExpression.Right),
                   binaryExpression.IsLiftedToNull, binaryExpression.Method);
           };
            converters[(int)TmphExpressionType.Lambda] = expression =>
           {
               var lambdaExpression = (LambdaExpression)expression;
               return TmphLambdaExpression.Get(convert(lambdaExpression.Body),
                   lambdaExpression.Parameters.GetArray(value => TmphParameterExpression.convert(value)));
           };
            converters[(int)TmphExpressionType.LeftShift] = expression =>
           {
               var binaryExpression = (BinaryExpression)expression;
               if (binaryExpression.Method == null)
               {
                   return LeftShift(convert(binaryExpression.Left), convert(binaryExpression.Right));
               }
               return LeftShift(convert(binaryExpression.Left), convert(binaryExpression.Right),
                   binaryExpression.Method);
           };
            converters[(int)TmphExpressionType.LessThan] = expression =>
           {
               var binaryExpression = (BinaryExpression)expression;
               if (binaryExpression.Method == null)
               {
                   return LessThan(convert(binaryExpression.Left), convert(binaryExpression.Right));
               }
               return LessThan(convert(binaryExpression.Left), convert(binaryExpression.Right),
                   binaryExpression.IsLiftedToNull, binaryExpression.Method);
           };
            converters[(int)TmphExpressionType.LessThanOrEqual] = expression =>
           {
               var binaryExpression = (BinaryExpression)expression;
               if (binaryExpression.Method == null)
               {
                   return LessThanOrEqual(convert(binaryExpression.Left), convert(binaryExpression.Right));
               }
               return LessThanOrEqual(convert(binaryExpression.Left), convert(binaryExpression.Right),
                   binaryExpression.IsLiftedToNull, binaryExpression.Method);
           };
            converters[(int)TmphExpressionType.MemberAccess] = expression =>
           {
               var memberExpression = (MemberExpression)expression;
               var field = memberExpression.Member as FieldInfo;
               if (field != null)
               {
                   return Field(convert(memberExpression.Expression), field);
               }
               return Property(convert(memberExpression.Expression), (PropertyInfo)memberExpression.Member);
           };
            converters[(int)TmphExpressionType.Modulo] = expression =>
           {
               var binaryExpression = (BinaryExpression)expression;
               if (binaryExpression.Method == null)
               {
                   return Modulo(convert(binaryExpression.Left), convert(binaryExpression.Right));
               }
               return Modulo(convert(binaryExpression.Left), convert(binaryExpression.Right), binaryExpression.Method);
           };
            converters[(int)TmphExpressionType.Multiply] = expression =>
           {
               var binaryExpression = (BinaryExpression)expression;
               if (binaryExpression.Method == null)
               {
                   return Multiply(convert(binaryExpression.Left), convert(binaryExpression.Right));
               }
               return Multiply(convert(binaryExpression.Left), convert(binaryExpression.Right), binaryExpression.Method);
           };
            converters[(int)TmphExpressionType.MultiplyChecked] = expression =>
           {
               var binaryExpression = (BinaryExpression)expression;
               if (binaryExpression.Method == null)
               {
                   return MultiplyChecked(convert(binaryExpression.Left), convert(binaryExpression.Right));
               }
               return MultiplyChecked(convert(binaryExpression.Left), convert(binaryExpression.Right),
                   binaryExpression.Method);
           };
            converters[(int)TmphExpressionType.Negate] = expression =>
           {
               var unaryExpression = (UnaryExpression)expression;
               if (unaryExpression.Method == null)
               {
                   return Negate(convert(unaryExpression.Operand));
               }
               return Negate(convert(unaryExpression.Operand), unaryExpression.Method);
           };
            converters[(int)TmphExpressionType.UnaryPlus] = expression =>
           {
               var unaryExpression = (UnaryExpression)expression;
               if (unaryExpression.Method == null)
               {
                   return UnaryPlus(convert(unaryExpression.Operand));
               }
               return UnaryPlus(convert(unaryExpression.Operand), unaryExpression.Method);
           };
            converters[(int)TmphExpressionType.NegateChecked] = expression =>
           {
               var unaryExpression = (UnaryExpression)expression;
               if (unaryExpression.Method == null)
               {
                   return NegateChecked(convert(unaryExpression.Operand));
               }
               return NegateChecked(convert(unaryExpression.Operand), unaryExpression.Method);
           };
            converters[(int)TmphExpressionType.Not] = expression =>
           {
               var unaryExpression = (UnaryExpression)expression;
               if (unaryExpression.Method == null)
               {
                   return Not(convert(unaryExpression.Operand));
               }
               return Not(convert(unaryExpression.Operand), unaryExpression.Method);
           };
            converters[(int)TmphExpressionType.NotEqual] = expression =>
           {
               var binaryExpression = (BinaryExpression)expression;
               if (binaryExpression.Method == null)
               {
                   return NotEqual(convert(binaryExpression.Left), convert(binaryExpression.Right));
               }
               return NotEqual(convert(binaryExpression.Left), convert(binaryExpression.Right),
                   binaryExpression.IsLiftedToNull, binaryExpression.Method);
           };
            converters[(int)TmphExpressionType.Or] = expression =>
           {
               var binaryExpression = (BinaryExpression)expression;
               if (binaryExpression.Method == null)
               {
                   return Or(convert(binaryExpression.Left), convert(binaryExpression.Right));
               }
               return Or(convert(binaryExpression.Left), convert(binaryExpression.Right), binaryExpression.Method);
           };
            converters[(int)TmphExpressionType.OrElse] = expression =>
           {
               var binaryExpression = (BinaryExpression)expression;
               if (binaryExpression.Method == null)
               {
                   return OrElse(convert(binaryExpression.Left), convert(binaryExpression.Right));
               }
               return OrElse(convert(binaryExpression.Left), convert(binaryExpression.Right), binaryExpression.Method);
           };
            converters[(int)TmphExpressionType.Parameter] = expression =>
           {
               var parameterExpression = (ParameterExpression)expression;
               return Parameter(parameterExpression.Type, parameterExpression.Name);
           };
            converters[(int)TmphExpressionType.Power] = expression =>
           {
               var binaryExpression = (BinaryExpression)expression;
               if (binaryExpression.Method == null)
               {
                   return Power(convert(binaryExpression.Left), convert(binaryExpression.Right));
               }
               return Power(convert(binaryExpression.Left), convert(binaryExpression.Right), binaryExpression.Method);
           };
            converters[(int)TmphExpressionType.RightShift] = expression =>
           {
               var binaryExpression = (BinaryExpression)expression;
               if (binaryExpression.Method == null)
               {
                   return RightShift(convert(binaryExpression.Left), convert(binaryExpression.Right));
               }
               return RightShift(convert(binaryExpression.Left), convert(binaryExpression.Right),
                   binaryExpression.Method);
           };
            converters[(int)TmphExpressionType.Subtract] = expression =>
           {
               var binaryExpression = (BinaryExpression)expression;
               if (binaryExpression.Method == null)
               {
                   return Subtract(convert(binaryExpression.Left), convert(binaryExpression.Right));
               }
               return Subtract(convert(binaryExpression.Left), convert(binaryExpression.Right), binaryExpression.Method);
           };
            converters[(int)TmphExpressionType.SubtractChecked] = expression =>
           {
               var binaryExpression = (BinaryExpression)expression;
               if (binaryExpression.Method == null)
               {
                   return SubtractChecked(convert(binaryExpression.Left), convert(binaryExpression.Right));
               }
               return SubtractChecked(convert(binaryExpression.Left), convert(binaryExpression.Right),
                   binaryExpression.Method);
           };

            #endregion 表达式类型集合
        }

        /// <summary>
        ///     表达式类型
        /// </summary>
        public TmphExpressionType NodeType { get; protected set; }

        /// <summary>
        ///     是否简单表达式
        /// </summary>
        public bool IsSimple { get; protected set; }

        /// <summary>
        ///     是否常量表达式
        /// </summary>
        public bool IsConstant { get; protected set; }

        /// <summary>
        ///     常量值是否为null
        /// </summary>
        public bool IsConstantNull
        {
            get { return IsConstant && ((TmphConstantExpression)this).Value == null; }
        }

        /// <summary>
        ///     简单表达式
        /// </summary>
        public virtual TmphExpression SimpleExpression
        {
            get { return this; }
        }

        /// <summary>
        ///     添加到表达式池
        /// </summary>
        internal void PushPool()
        {
            if (ExpressionCount == 0) pushPool();
        }

        /// <summary>
        ///     添加到表达式池
        /// </summary>
        internal void PushCountPool()
        {
            if (--ExpressionCount == 0) pushPool();
        }

        /// <summary>
        ///     添加到表达式池
        /// </summary>
        internal abstract void pushPool();

        /// <summary>
        ///     lambda隐式转换成表达式
        /// </summary>
        /// <typeparam name="TDelegateType">委托类型</typeparam>
        /// <param name="body">表达式主体</param>
        /// <param name="parameters">参数</param>
        /// <returns>委托关联表达式</returns>
        public static TmphExpression<TDelegateType> Lambda<TDelegateType>(TmphExpression body,
            params TmphParameterExpression[] parameters)
        {
            return TmphExpression<TDelegateType>.Get(body, parameters);
        }

        /// <summary>
        ///     表达式参数
        /// </summary>
        /// <param name="type">参数类型</param>
        /// <param name="name">参数名称</param>
        /// <returns>表达式参数</returns>
        public static TmphParameterExpression Parameter(Type type, string name)
        {
            return TmphParameterExpression.Get(type, name);
        }

        /// <summary>
        ///     创建字段表达式
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="field">字段信息</param>
        /// <returns>字段表达式</returns>
        public static TmphMemberExpression Field(TmphExpression expression, FieldInfo field)
        {
            if (field.IsStatic ^ expression == null)
            {
                TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
            }
            return TmphFieldExpression.Get(expression, field);
        }

        /// <summary>
        ///     创建字段表达式
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="type">字段所属类型</param>
        /// <param name="fieldName">字段名称</param>
        /// <returns>字段表达式</returns>
        public static TmphMemberExpression Field(TmphExpression expression, Type type, string fieldName)
        {
            FieldInfo field = type.getField(fieldName);
            if (field == null) TmphLog.Error.Throw(type + " 未找到字段 " + fieldName, true, true);
            return Field(expression, field);
        }

        /// <summary>
        ///     创建属性表达式
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="property">属性信息</param>
        /// <returns>属性表达式</returns>
        public static TmphMemberExpression Property(TmphExpression expression, PropertyInfo property)
        {
            var methodInfo = property.GetGetMethod(true);
            if (methodInfo == null || (methodInfo.IsStatic ^ expression == null))
            {
                TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
            }
            return TmphPropertyExpression.Get(expression, property);
        }

        /// <summary>
        ///     创建属性表达式
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="type">属性所属类型</param>
        /// <param name="propertyName">属性名称</param>
        /// <returns>属性表达式</returns>
        public static TmphMemberExpression Property(TmphExpression expression, Type type, string propertyName)
        {
            PropertyInfo property = type.getProperty(propertyName);
            if (property == null) TmphLog.Error.Throw(type + " 未找到属性 " + propertyName, true, true);
            return Property(expression, property);
        }

        /// <summary>
        ///     创建属性表达式
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="propertyAccessor">属性关联函数信息</param>
        /// <returns>属性表达式</returns>
        public static TmphMemberExpression Property(TmphExpression expression, MethodInfo propertyAccessor)
        {
            return Property(expression, getProperty(propertyAccessor));
        }

        /// <summary>
        ///     根据函数信息获取关联属性信息
        /// </summary>
        /// <param name="method">函数信息</param>
        /// <returns>属性信息</returns>
        private static PropertyInfo getProperty(MethodInfo method)
        {
            foreach (
                var property in
                    method.DeclaringType.GetProperties(BindingFlags.NonPublic | BindingFlags.Public |
                                                       (method.IsStatic ? BindingFlags.Static : BindingFlags.Instance)))
            {
                if (property.CanRead && checkMethod(method, property.GetGetMethod(true))) return property;
            }
            TmphLog.Error.Throw(method.DeclaringType.fullName() + "." + method.Name + " 未找到对应属性", true, true);
            return null;
        }

        /// <summary>
        ///     检测函数信息与属性函数信息是否匹配
        /// </summary>
        /// <param name="method">函数信息</param>
        /// <param name="propertyMethod">属性函数信息</param>
        /// <returns>是否关联属性</returns>
        private static bool checkMethod(MethodInfo method, MethodInfo propertyMethod)
        {
            if (method == propertyMethod) return true;
            var declaringType = method.DeclaringType;
            return ((declaringType.IsInterface && (method.Name == propertyMethod.Name)) &&
                    (declaringType.GetMethod(method.Name) == propertyMethod));
        }

        /// <summary>
        ///     创建二元||表达式
        /// </summary>
        /// <param name="left">左表达式</param>
        /// <param name="right">右表达式</param>
        /// <returns>二元||表达式</returns>
        public static TmphBinaryExpression OrElse(TmphExpression left, TmphExpression right)
        {
            return TmphBinaryExpression.Get(TmphExpressionType.OrElse, left, right, null);
        }

        /// <summary>
        ///     创建||二元表达式
        /// </summary>
        /// <param name="left">左表达式</param>
        /// <param name="right">右表达式</param>
        /// <param name="method">运算符重载函数</param>
        /// <returns>二元||表达式</returns>
        public static TmphBinaryExpression OrElse(TmphExpression left, TmphExpression right, MethodInfo method)
        {
            return TmphBinaryExpression.Get(TmphExpressionType.OrElse, left, right, method);
        }

        /// <summary>
        ///     创建二元&&表达式
        /// </summary>
        /// <param name="left">左表达式</param>
        /// <param name="right">右表达式</param>
        /// <returns>二元&&表达式</returns>
        public static TmphBinaryExpression AndAlso(TmphExpression left, TmphExpression right)
        {
            return TmphBinaryExpression.Get(TmphExpressionType.AndAlso, left, right, null);
        }

        /// <summary>
        ///     创建&&二元表达式
        /// </summary>
        /// <param name="left">左表达式</param>
        /// <param name="right">右表达式</param>
        /// <param name="method">运算符重载函数</param>
        /// <returns>二元&&表达式</returns>
        public static TmphBinaryExpression AndAlso(TmphExpression left, TmphExpression right, MethodInfo method)
        {
            return TmphBinaryExpression.Get(TmphExpressionType.AndAlso, left, right, method);
        }

        /// <summary>
        ///     创建!一元表达式
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>!一元表达式</returns>
        public static TmphUnaryExpression Not(TmphExpression expression)
        {
            return TmphUnaryExpression.Get(TmphExpressionType.Not, expression, null);
        }

        /// <summary>
        ///     创建!二元表达式
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="method">运算符重载函数</param>
        /// <returns>二元!表达式</returns>
        public static TmphUnaryExpression Not(TmphExpression expression, MethodInfo method)
        {
            return TmphUnaryExpression.Get(TmphExpressionType.Not, expression, method);
        }

        /// <summary>
        ///     创建==二元表达式
        /// </summary>
        /// <param name="left">左表达式</param>
        /// <param name="right">右表达式</param>
        /// <returns>二元==表达式</returns>
        public static TmphBinaryExpression Equal(TmphExpression left, TmphExpression right)
        {
            return TmphBinaryExpression.Get(TmphExpressionType.Equal, left, right, null);
        }

        /// <summary>
        ///     创建==二元表达式
        /// </summary>
        /// <param name="left">左表达式</param>
        /// <param name="right">右表达式</param>
        /// <param name="liftToNull">是否可空类型</param>
        /// <param name="method">运算符重载函数</param>
        /// <returns>二元==表达式</returns>
        public static TmphBinaryExpression Equal(TmphExpression left, TmphExpression right, bool liftToNull, MethodInfo method)
        {
            return TmphBinaryExpression.Get(TmphExpressionType.Equal, left, right, method);
        }

        /// <summary>
        ///     创建!=二元表达式
        /// </summary>
        /// <param name="left">左表达式</param>
        /// <param name="right">右表达式</param>
        /// <returns>二元!=表达式</returns>
        public static TmphBinaryExpression NotEqual(TmphExpression left, TmphExpression right)
        {
            return TmphBinaryExpression.Get(TmphExpressionType.NotEqual, left, right, null);
        }

        /// <summary>
        ///     创建!=二元表达式
        /// </summary>
        /// <param name="left">左表达式</param>
        /// <param name="right">右表达式</param>
        /// <param name="liftToNull">是否可空类型</param>
        /// <param name="method">运算符重载函数</param>
        /// <returns>二元!=表达式</returns>
        public static TmphBinaryExpression NotEqual(TmphExpression left, TmphExpression right, bool liftToNull,
            MethodInfo method)
        {
            return TmphBinaryExpression.Get(TmphExpressionType.NotEqual, left, right, method);
        }

        /// <summary>
        ///     创建>=二元表达式
        /// </summary>
        /// <param name="left">左表达式</param>
        /// <param name="right">右表达式</param>
        /// <returns>二元>=表达式</returns>
        public static TmphBinaryExpression GreaterThanOrEqual(TmphExpression left, TmphExpression right)
        {
            return TmphBinaryExpression.Get(TmphExpressionType.GreaterThanOrEqual, left, right, null);
        }

        /// <summary>
        ///     创建>=二元表达式
        /// </summary>
        /// <param name="left">左表达式</param>
        /// <param name="right">右表达式</param>
        /// <param name="liftToNull">是否可空类型</param>
        /// <param name="method">运算符重载函数</param>
        /// <returns>二元>=表达式</returns>
        public static TmphBinaryExpression GreaterThanOrEqual(TmphExpression left, TmphExpression right, bool liftToNull,
            MethodInfo method)
        {
            return TmphBinaryExpression.Get(TmphExpressionType.GreaterThanOrEqual, left, right, method);
        }

        /// <summary>
        ///     创建>二元表达式
        /// </summary>
        /// <param name="left">左表达式</param>
        /// <param name="right">右表达式</param>
        /// <returns>二元>表达式</returns>
        public static TmphBinaryExpression GreaterThan(TmphExpression left, TmphExpression right)
        {
            return TmphBinaryExpression.Get(TmphExpressionType.GreaterThan, left, right, null);
        }

        /// <summary>
        ///     创建>二元表达式
        /// </summary>
        /// <param name="left">左表达式</param>
        /// <param name="right">右表达式</param>
        /// <param name="liftToNull">是否可空类型</param>
        /// <param name="method">运算符重载函数</param>
        /// <returns>二元>表达式</returns>
        public static TmphBinaryExpression GreaterThan(TmphExpression left, TmphExpression right, bool liftToNull,
            MethodInfo method)
        {
            return TmphBinaryExpression.Get(TmphExpressionType.GreaterThan, left, right, method);
        }

        /// <summary>
        ///     创建<二元表达式
        /// </summary>
        /// <param name="left">左表达式</param>
        /// <param name="right">右表达式</param>
        /// <returns>二元<表达式</returns>
        public static TmphBinaryExpression LessThan(TmphExpression left, TmphExpression right)
        {
            return TmphBinaryExpression.Get(TmphExpressionType.LessThan, left, right, null);
        }

        /// <summary>
        ///     创建<二元表达式
        /// </summary>
        /// <param name="left">左表达式</param>
        /// <param name="right">右表达式</param>
        /// <param name="liftToNull">是否可空类型</param>
        /// <param name="method">运算符重载函数</param>
        /// <returns>二元<表达式</returns>
        public static TmphBinaryExpression LessThan(TmphExpression left, TmphExpression right, bool liftToNull,
            MethodInfo method)
        {
            return TmphBinaryExpression.Get(TmphExpressionType.LessThan, left, right, method);
        }

        /// <summary>
        ///     创建<=二元表达式
        /// </summary>
        /// <param name="left">左表达式</param>
        /// <param name="right">右表达式</param>
        /// <returns>
        ///     二元<=表达式</returns>
        public static TmphBinaryExpression LessThanOrEqual(TmphExpression left, TmphExpression right)
        {
            return TmphBinaryExpression.Get(TmphExpressionType.LessThanOrEqual, left, right, null);
        }

        /// <summary>
        ///     创建<=二元表达式
        /// </summary>
        /// <param name="left">左表达式</param>
        /// <param name="right">右表达式</param>
        /// <param name="liftToNull">是否可空类型</param>
        /// <param name="method">运算符重载函数</param>
        /// <returns>
        ///     二元<=表达式</returns>
        public static TmphBinaryExpression LessThanOrEqual(TmphExpression left, TmphExpression right, bool liftToNull,
            MethodInfo method)
        {
            return TmphBinaryExpression.Get(TmphExpressionType.LessThanOrEqual, left, right, method);
        }

        /// <summary>
        ///     创建常量表达式
        /// </summary>
        /// <param name="value">数据</param>
        /// <returns>运算符表达式</returns>
        public static TmphConstantExpression Constant(object value)
        {
            return TmphConstantExpression.Get(value);
        }

        /// <summary>
        ///     创建常量表达式
        /// </summary>
        /// <param name="value">数据</param>
        /// <param name="type">数据类型</param>
        /// <returns>运算符表达式</returns>
        public static TmphConstantExpression Constant(object value, Type type)
        {
            return TmphConstantExpression.Get(value);
        }

        /// <summary>
        ///     创建拆箱表达式
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="type">数据类型</param>
        /// <returns>拆箱表达式</returns>
        public static TmphUnaryExpression Unbox(TmphExpression expression, Type type)
        {
            return TmphUnaryExpression.Get(TmphExpressionType.Unbox, expression, null);
        }

        /// <summary>
        ///     创建-一元表达式
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>一元-表达式</returns>
        public static TmphUnaryExpression Negate(TmphExpression expression)
        {
            return Negate(expression, null);
        }

        /// <summary>
        ///     创建-一元表达式
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="method">运算符重载函数</param>
        /// <returns>一元-表达式</returns>
        public static TmphUnaryExpression Negate(TmphExpression expression, MethodInfo method)
        {
            return TmphUnaryExpression.Get(TmphExpressionType.Negate, expression, method);
        }

        /// <summary>
        ///     创建-一元表达式
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>一元-表达式</returns>
        public static TmphUnaryExpression NegateChecked(TmphExpression expression)
        {
            return NegateChecked(expression, null);
        }

        /// <summary>
        ///     创建-一元表达式
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="method">运算符重载函数</param>
        /// <returns>一元-表达式</returns>
        public static TmphUnaryExpression NegateChecked(TmphExpression expression, MethodInfo method)
        {
            return TmphUnaryExpression.Get(TmphExpressionType.NegateChecked, expression, method);
        }

        /// <summary>
        ///     创建+一元表达式
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>一元+表达式</returns>
        public static TmphUnaryExpression UnaryPlus(TmphExpression expression)
        {
            return UnaryPlus(expression, null);
        }

        /// <summary>
        ///     创建+一元表达式
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="method">运算符重载函数</param>
        /// <returns>一元+表达式</returns>
        public static TmphUnaryExpression UnaryPlus(TmphExpression expression, MethodInfo method)
        {
            return TmphUnaryExpression.Get(TmphExpressionType.UnaryPlus, expression, method);
        }

        /// <summary>
        ///     创建+二元表达式
        /// </summary>
        /// <param name="left">左表达式</param>
        /// <param name="right">右表达式</param>
        /// <returns>二元+表达式</returns>
        public static TmphBinaryExpression Add(TmphExpression left, TmphExpression right)
        {
            return Add(left, right, null);
        }

        /// <summary>
        ///     创建+二元表达式
        /// </summary>
        /// <param name="left">左表达式</param>
        /// <param name="right">右表达式</param>
        /// <param name="method">运算符重载函数</param>
        /// <returns>二元+表达式</returns>
        public static TmphBinaryExpression Add(TmphExpression left, TmphExpression right, MethodInfo method)
        {
            return TmphBinaryExpression.Get(TmphExpressionType.Add, left, right, method);
        }

        /// <summary>
        ///     创建+二元表达式
        /// </summary>
        /// <param name="left">左表达式</param>
        /// <param name="right">右表达式</param>
        /// <returns>二元+表达式</returns>
        public static TmphBinaryExpression AddChecked(TmphExpression left, TmphExpression right)
        {
            return AddChecked(left, right, null);
        }

        /// <summary>
        ///     创建+二元表达式
        /// </summary>
        /// <param name="left">左表达式</param>
        /// <param name="right">右表达式</param>
        /// <param name="method">运算符重载函数</param>
        /// <returns>二元+表达式</returns>
        public static TmphBinaryExpression AddChecked(TmphExpression left, TmphExpression right, MethodInfo method)
        {
            return TmphBinaryExpression.Get(TmphExpressionType.AddChecked, left, right, method);
        }

        /// <summary>
        ///     创建-二元表达式
        /// </summary>
        /// <param name="left">左表达式</param>
        /// <param name="right">右表达式</param>
        /// <returns>二元-表达式</returns>
        public static TmphBinaryExpression Subtract(TmphExpression left, TmphExpression right)
        {
            return Subtract(left, right, null);
        }

        /// <summary>
        ///     创建-二元表达式
        /// </summary>
        /// <param name="left">左表达式</param>
        /// <param name="right">右表达式</param>
        /// <param name="method">运算符重载函数</param>
        /// <returns>二元-表达式</returns>
        public static TmphBinaryExpression Subtract(TmphExpression left, TmphExpression right, MethodInfo method)
        {
            return TmphBinaryExpression.Get(TmphExpressionType.Subtract, left, right, method);
        }

        /// <summary>
        ///     创建-二元表达式
        /// </summary>
        /// <param name="left">左表达式</param>
        /// <param name="right">右表达式</param>
        /// <returns>二元-表达式</returns>
        public static TmphBinaryExpression SubtractChecked(TmphExpression left, TmphExpression right)
        {
            return SubtractChecked(left, right, null);
        }

        /// <summary>
        ///     创建-二元表达式
        /// </summary>
        /// <param name="left">左表达式</param>
        /// <param name="right">右表达式</param>
        /// <param name="method">运算符重载函数</param>
        /// <returns>二元-表达式</returns>
        public static TmphBinaryExpression SubtractChecked(TmphExpression left, TmphExpression right, MethodInfo method)
        {
            return TmphBinaryExpression.Get(TmphExpressionType.SubtractChecked, left, right, method);
        }

        /// <summary>
        ///     创建*二元表达式
        /// </summary>
        /// <param name="left">左表达式</param>
        /// <param name="right">右表达式</param>
        /// <returns>二元*表达式</returns>
        public static TmphBinaryExpression Multiply(TmphExpression left, TmphExpression right)
        {
            return Multiply(left, right, null);
        }

        /// <summary>
        ///     创建*二元表达式
        /// </summary>
        /// <param name="left">左表达式</param>
        /// <param name="right">右表达式</param>
        /// <param name="method">运算符重载函数</param>
        /// <returns>二元*表达式</returns>
        public static TmphBinaryExpression Multiply(TmphExpression left, TmphExpression right, MethodInfo method)
        {
            return TmphBinaryExpression.Get(TmphExpressionType.Multiply, left, right, method);
        }

        /// <summary>
        ///     创建*二元表达式
        /// </summary>
        /// <param name="left">左表达式</param>
        /// <param name="right">右表达式</param>
        /// <returns>二元*表达式</returns>
        public static TmphBinaryExpression MultiplyChecked(TmphExpression left, TmphExpression right)
        {
            return MultiplyChecked(left, right, null);
        }

        /// <summary>
        ///     创建*二元表达式
        /// </summary>
        /// <param name="left">左表达式</param>
        /// <param name="right">右表达式</param>
        /// <param name="method">运算符重载函数</param>
        /// <returns>二元*表达式</returns>
        public static TmphBinaryExpression MultiplyChecked(TmphExpression left, TmphExpression right, MethodInfo method)
        {
            return TmphBinaryExpression.Get(TmphExpressionType.MultiplyChecked, left, right, method);
        }

        /// <summary>
        ///     创建/二元表达式
        /// </summary>
        /// <param name="left">左表达式</param>
        /// <param name="right">右表达式</param>
        /// <returns>二元/表达式</returns>
        public static TmphBinaryExpression Divide(TmphExpression left, TmphExpression right)
        {
            return Divide(left, right, null);
        }

        /// <summary>
        ///     创建/二元表达式
        /// </summary>
        /// <param name="left">左表达式</param>
        /// <param name="right">右表达式</param>
        /// <param name="method">运算符重载函数</param>
        /// <returns>二元/表达式</returns>
        public static TmphBinaryExpression Divide(TmphExpression left, TmphExpression right, MethodInfo method)
        {
            return TmphBinaryExpression.Get(TmphExpressionType.Divide, left, right, method);
        }

        /// <summary>
        ///     创建%二元表达式
        /// </summary>
        /// <param name="left">左表达式</param>
        /// <param name="right">右表达式</param>
        /// <returns>二元%表达式</returns>
        public static TmphBinaryExpression Modulo(TmphExpression left, TmphExpression right)
        {
            return Modulo(left, right, null);
        }

        /// <summary>
        ///     创建%二元表达式
        /// </summary>
        /// <param name="left">左表达式</param>
        /// <param name="right">右表达式</param>
        /// <param name="method">运算符重载函数</param>
        /// <returns>二元%表达式</returns>
        public static TmphBinaryExpression Modulo(TmphExpression left, TmphExpression right, MethodInfo method)
        {
            return TmphBinaryExpression.Get(TmphExpressionType.Modulo, left, right, method);
        }

        /// <summary>
        ///     创建**二元表达式
        /// </summary>
        /// <param name="left">左表达式</param>
        /// <param name="right">右表达式</param>
        /// <returns>二元**表达式</returns>
        public static TmphBinaryExpression Power(TmphExpression left, TmphExpression right)
        {
            return Power(left, right, null);
        }

        /// <summary>
        ///     创建**二元表达式
        /// </summary>
        /// <param name="left">左表达式</param>
        /// <param name="right">右表达式</param>
        /// <param name="method">运算符重载函数</param>
        /// <returns>二元**表达式</returns>
        public static TmphBinaryExpression Power(TmphExpression left, TmphExpression right, MethodInfo method)
        {
            return TmphBinaryExpression.Get(TmphExpressionType.Power, left, right, method);
        }

        /// <summary>
        ///     创建|二元表达式
        /// </summary>
        /// <param name="left">左表达式</param>
        /// <param name="right">右表达式</param>
        /// <returns>二元|表达式</returns>
        public static TmphBinaryExpression Or(TmphExpression left, TmphExpression right)
        {
            return Or(left, right, null);
        }

        /// <summary>
        ///     创建|二元表达式
        /// </summary>
        /// <param name="left">左表达式</param>
        /// <param name="right">右表达式</param>
        /// <param name="method">运算符重载函数</param>
        /// <returns>二元|表达式</returns>
        public static TmphBinaryExpression Or(TmphExpression left, TmphExpression right, MethodInfo method)
        {
            return TmphBinaryExpression.Get(TmphExpressionType.Or, left, right, method);
        }

        /// <summary>
        ///     创建&二元表达式
        /// </summary>
        /// <param name="left">左表达式</param>
        /// <param name="right">右表达式</param>
        /// <returns>二元&表达式</returns>
        public static TmphBinaryExpression And(TmphExpression left, TmphExpression right)
        {
            return And(left, right, null);
        }

        /// <summary>
        ///     创建&二元表达式
        /// </summary>
        /// <param name="left">左表达式</param>
        /// <param name="right">右表达式</param>
        /// <param name="method">运算符重载函数</param>
        /// <returns>二元&表达式</returns>
        public static TmphBinaryExpression And(TmphExpression left, TmphExpression right, MethodInfo method)
        {
            return TmphBinaryExpression.Get(TmphExpressionType.And, left, right, method);
        }

        /// <summary>
        ///     创建^二元表达式
        /// </summary>
        /// <param name="left">左表达式</param>
        /// <param name="right">右表达式</param>
        /// <returns>二元^表达式</returns>
        public static TmphBinaryExpression ExclusiveOr(TmphExpression left, TmphExpression right)
        {
            return ExclusiveOr(left, right, null);
        }

        /// <summary>
        ///     创建^二元表达式
        /// </summary>
        /// <param name="left">左表达式</param>
        /// <param name="right">右表达式</param>
        /// <param name="method">运算符重载函数</param>
        /// <returns>二元^表达式</returns>
        public static TmphBinaryExpression ExclusiveOr(TmphExpression left, TmphExpression right, MethodInfo method)
        {
            return TmphBinaryExpression.Get(TmphExpressionType.ExclusiveOr, left, right, method);
        }

        /// <summary>
        ///     创建<<二元表达式
        /// </summary>
        /// <param name="left">左表达式</param>
        /// <param name="right">右表达式</param>
        /// <returns>
        ///     二元<<表达式
        /// </returns>
        public static TmphBinaryExpression LeftShift(TmphExpression left, TmphExpression right)
        {
            return LeftShift(left, right, null);
        }

        /// <summary>
        ///     创建<<二元表达式
        /// </summary>
        /// <param name="left">左表达式</param>
        /// <param name="right">右表达式</param>
        /// <param name="method">运算符重载函数</param>
        /// <returns>
        ///     二元<<表达式
        /// </returns>
        public static TmphBinaryExpression LeftShift(TmphExpression left, TmphExpression right, MethodInfo method)
        {
            return TmphBinaryExpression.Get(TmphExpressionType.LeftShift, left, right, method);
        }

        /// <summary>
        ///     创建>>二元表达式
        /// </summary>
        /// <param name="left">左表达式</param>
        /// <param name="right">右表达式</param>
        /// <returns>二元>>表达式</returns>
        public static TmphBinaryExpression RightShift(TmphExpression left, TmphExpression right)
        {
            return RightShift(left, right, null);
        }

        /// <summary>
        ///     创建>>二元表达式
        /// </summary>
        /// <param name="left">左表达式</param>
        /// <param name="right">右表达式</param>
        /// <param name="method">运算符重载函数</param>
        /// <returns>二元>>表达式</returns>
        public static TmphBinaryExpression RightShift(TmphExpression left, TmphExpression right, MethodInfo method)
        {
            return TmphBinaryExpression.Get(TmphExpressionType.RightShift, left, right, method);
        }

        /// <summary>
        ///     创建类型转换一元表达式
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="type">目标类型</param>
        /// <returns>类型转换一元表达式</returns>
        public static TmphUnaryExpression Convert(TmphExpression expression, Type type)
        {
            return TmphConvertExpression.Get(expression, type, null);
        }

        /// <summary>
        ///     创建类型转换一元表达式
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="type">目标类型</param>
        /// <param name="method">运算符重载函数</param>
        /// <returns>一元类型转换表达式</returns>
        public static TmphUnaryExpression Convert(TmphExpression expression, Type type, MethodInfo method)
        {
            return TmphConvertExpression.Get(expression, type, method);
        }

        /// <summary>
        ///     创建类型转换一元表达式
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="type">目标类型</param>
        /// <returns>类型转换一元表达式</returns>
        public static TmphUnaryExpression ConvertChecked(TmphExpression expression, Type type)
        {
            return TmphUnaryExpression.Get(TmphExpressionType.ConvertChecked, expression, null);
        }

        /// <summary>
        ///     创建类型转换一元表达式
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="type">目标类型</param>
        /// <param name="method">运算符重载函数</param>
        /// <returns>一元类型转换表达式</returns>
        public static TmphUnaryExpression ConvertChecked(TmphExpression expression, Type type, MethodInfo method)
        {
            return TmphUnaryExpression.Get(TmphExpressionType.ConvertChecked, expression, method);
        }

        /// <summary>
        ///     创建条件表达式
        /// </summary>
        /// <param name="test">测试条件</param>
        /// <param name="ifTrue">真表达式</param>
        /// <param name="ifFalse">假表达式</param>
        /// <returns>条件表达式</returns>
        public static TmphConditionalExpression Condition(TmphExpression test, TmphExpression ifTrue, TmphExpression ifFalse)
        {
            return TmphConditionalExpression.Get(test, ifTrue, ifFalse);
        }

        /// <summary>
        ///     创建条件表达式
        /// </summary>
        /// <param name="test">测试条件</param>
        /// <param name="ifTrue">真表达式</param>
        /// <param name="ifFalse">假表达式</param>
        /// <param name="type">表达式结果类型</param>
        /// <returns>条件表达式</returns>
        public static TmphConditionalExpression Condition(TmphExpression test, TmphExpression ifTrue, TmphExpression ifFalse,
            Type type)
        {
            return TmphConditionalExpression.Get(test, ifTrue, ifFalse);
        }

        /// <summary>
        ///     创建真值判定一元表达式
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>真值判定一元表达式</returns>
        public static TmphUnaryExpression IsTrue(TmphExpression expression)
        {
            return TmphUnaryExpression.Get(TmphExpressionType.IsTrue, expression, null);
        }

        /// <summary>
        ///     创建真值判定一元表达式
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="method">运算符重载函数</param>
        /// <returns>一元真值判定表达式</returns>
        public static TmphUnaryExpression IsTrue(TmphExpression expression, MethodInfo method)
        {
            return TmphUnaryExpression.Get(TmphExpressionType.IsTrue, expression, method);
        }

        /// <summary>
        ///     创建假值判定一元表达式
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>假值判定一元表达式</returns>
        public static TmphUnaryExpression IsFalse(TmphExpression expression)
        {
            return TmphUnaryExpression.Get(TmphExpressionType.IsFalse, expression, null);
        }

        /// <summary>
        ///     创建假值判定一元表达式
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="method">运算符重载函数</param>
        /// <returns>假元真值判定表达式</returns>
        public static TmphUnaryExpression IsFalse(TmphExpression expression, MethodInfo method)
        {
            return TmphUnaryExpression.Get(TmphExpressionType.IsFalse, expression, method);
        }

        /// <summary>
        ///     创建函数调用表达式
        /// </summary>
        /// <param name="instance">动态函数对象表达式</param>
        /// <param name="method">函数信息</param>
        /// <returns>函数调用表达式</returns>
        public static TmphMethodCallExpression Call(TmphExpression instance, MethodInfo method)
        {
            return TmphMethodCallExpression.Get(method, instance, null);
        }

        /// <summary>
        ///     创建函数调用表达式
        /// </summary>
        /// <param name="method">函数信息</param>
        /// <param name="arguments">调用参数</param>
        /// <returns>函数调用表达式</returns>
        public static TmphMethodCallExpression Call(MethodInfo method, params TmphExpression[] arguments)
        {
            return TmphMethodCallExpression.Get(method, null, arguments);
        }

        /// <summary>
        ///     创建函数调用表达式
        /// </summary>
        /// <param name="instance">动态函数对象表达式</param>
        /// <param name="method">函数信息</param>
        /// <param name="arguments">调用参数</param>
        /// <returns>函数调用表达式</returns>
        public static TmphMethodCallExpression Call(TmphExpression instance, MethodInfo method,
            params TmphExpression[] arguments)
        {
            return TmphMethodCallExpression.Get(method, instance, arguments);
        }

        /// <summary>
        ///     创建函数调用表达式
        /// </summary>
        /// <param name="instance">动态函数对象表达式</param>
        /// <param name="method">函数信息</param>
        /// <param name="arguments">调用参数</param>
        /// <returns>函数调用表达式</returns>
        public static TmphMethodCallExpression Call(TmphExpression instance, MethodInfo method,
            IEnumerable<TmphExpression> arguments)
        {
            return TmphMethodCallExpression.Get(method, instance, arguments.getArray());
        }

        /// <summary>
        ///     表达式转换
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>表达式</returns>
        internal static TmphExpression convert(System.Linq.Expressions.Expression expression)
        {
            var converter = converters[(int)expressionTypes[(int)expression.NodeType]];
            if (converter == null) TmphLog.Default.Throw("不可识别的表达式类型 : " + expression.NodeType, false, true);
            return converter(expression);
        }
    }

    /// <summary>
    ///     委托关联表达式
    /// </summary>
    /// <typeparam name="TDelegateType">委托类型</typeparam>
    internal sealed class TmphExpression<TDelegateType> : TmphLambdaExpression
    {
        /// <summary>
        ///     添加到表达式池
        /// </summary>
        internal override void pushPool()
        {
            clear();
            TmphTypePool<TmphExpression<TDelegateType>>.Push(this);
        }

        /// <summary>
        ///     获取委托关联表达式
        /// </summary>
        /// <param name="body">表达式主体</param>
        /// <param name="parameters">参数</param>
        /// <returns>委托关联表达式</returns>
        internal new static TmphExpression<TDelegateType> Get(TmphExpression body, TmphParameterExpression[] parameters)
        {
            var expression = TmphTypePool<TmphExpression<TDelegateType>>.Pop() ?? new TmphExpression<TDelegateType>();
            expression.set(body, parameters);
            return expression;
        }
    }
}