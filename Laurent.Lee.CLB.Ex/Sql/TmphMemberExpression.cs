using System;
using System.Reflection;
using System.Linq.Expressions;
using System.Reflection.Emit;

namespace Laurent.Lee.CLB.Sql
{
    /// <summary>
    /// 成员表达式
    /// </summary>
    /// <typeparam name="TTargetType"></typeparam>
    /// <typeparam name="TValueType"></typeparam>
    public struct TmphMemberExpression<TTargetType, TValueType>
        where TTargetType : class
        where TValueType : class
    {
        /// <summary>
        /// 字段成员
        /// </summary>
        public FieldInfo Field;
        /// <summary>
        /// 获取成员
        /// </summary>
        public Func<TTargetType, TValueType> GetMember
        {
            get
            {
                DynamicMethod dynamicMethod = new DynamicMethod("getMember" + Field.Name, typeof(TValueType), new Type[] { typeof(TTargetType) }, typeof(TTargetType), true);
                ILGenerator generator = dynamicMethod.GetILGenerator();
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldfld, Field);
                generator.Emit(OpCodes.Ret);
                return (Func<TTargetType, TValueType>)dynamicMethod.CreateDelegate(typeof(Func<TTargetType, TValueType>));
            }
        }
        /// <summary>
        /// 设置成员
        /// </summary>
        public Action<TTargetType, TValueType> SetMember
        {
            get
            {
                DynamicMethod dynamicMethod = new DynamicMethod("setMember" + Field.Name, null, new Type[] { typeof(TTargetType), typeof(TValueType) }, typeof(TTargetType), true);
                ILGenerator generator = dynamicMethod.GetILGenerator();
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Stfld, Field);
                generator.Emit(OpCodes.Ret);
                return (Action<TTargetType, TValueType>)dynamicMethod.CreateDelegate(typeof(Action<TTargetType, TValueType>));
            }
        }
        /// <summary>
        /// 成员表达式
        /// </summary>
        /// <param name="member">成员表达式</param>
        public TmphMemberExpression(Expression<Func<TTargetType, TValueType>> member)
        {
            System.Linq.Expressions.Expression expression = member.Body;
            if (expression.NodeType == ExpressionType.MemberAccess)
            {
                FieldInfo field = ((MemberExpression)expression).Member as FieldInfo;
                if (field != null && !field.IsStatic && field.DeclaringType.IsAssignableFrom(typeof(TTargetType)) && field.FieldType == typeof(TValueType)) Field = field;
                else Field = null;
            }
            else Field = null;
        }
    }
}
