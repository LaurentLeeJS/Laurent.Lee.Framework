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
using System.Linq.Expressions;
using System.Reflection;
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