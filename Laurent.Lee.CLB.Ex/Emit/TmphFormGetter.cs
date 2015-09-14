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

using Laurent.Lee.CLB.Reflection;
using System;
using System.Collections.Specialized;
using System.Reflection;
using System.Reflection.Emit;

namespace Laurent.Lee.CLB.Emit
{
    /// <summary>
    /// web表单生成
    /// </summary>
    public static class TmphFormGetter
    {
        /// <summary>
        /// 动态函数
        /// </summary>
        public struct TmphMemberDynamicMethod
        {
            /// <summary>
            /// 动态函数
            /// </summary>
            private DynamicMethod dynamicMethod;

            /// <summary>
            ///
            /// </summary>
            private ILGenerator generator;

            /// <summary>
            /// 是否值类型
            /// </summary>
            private bool isValueType;

            /// <summary>
            /// 动态函数
            /// </summary>
            /// <param name="type"></param>
            public TmphMemberDynamicMethod(Type type)
            {
                dynamicMethod = new DynamicMethod("formGetter", null, new Type[] { type, typeof(NameValueCollection) }, type, true);
                generator = dynamicMethod.GetILGenerator();
                isValueType = type.IsValueType;
            }

            /// <summary>
            /// 添加字段
            /// </summary>
            /// <param name="field">字段信息</param>
            public void Push(FieldInfo field)
            {
                Type type = field.FieldType;
                if (type.IsValueType)
                {
                    Type nullType = type.nullableType();
                    if (nullType == null) push(field);
                    else
                    {
                        Label end = generator.DefineLabel();
                        if (isValueType) generator.Emit(OpCodes.Ldarga_S, 0);
                        else generator.Emit(OpCodes.Ldarg_0);
                        generator.Emit(OpCodes.Ldflda, field);
                        generator.Emit(OpCodes.Call, TmphPubExtension.GetNullableHasValue(type));
                        generator.Emit(OpCodes.Brfalse_S, end);
                        push(field, nullType);
                        generator.MarkLabel(end);
                    }
                }
                else pushNull(field);
            }

            /// <summary>
            /// 添加字段
            /// </summary>
            /// <param name="field">字段信息</param>
            private void pushNull(FieldInfo field)
            {
                Label end = generator.DefineLabel();
                if (isValueType) generator.Emit(OpCodes.Ldarga_S, 0);
                else generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldfld, field);
                generator.Emit(OpCodes.Brfalse_S, end);
                push(field);
                generator.MarkLabel(end);
            }

            /// <summary>
            /// 添加字段
            /// </summary>
            /// <param name="field">字段信息</param>
            /// <param name="nullType">可空类型</param>
            private void push(FieldInfo field, Type nullType = null)
            {
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Ldstr, field.Name);
                if (isValueType) generator.Emit(OpCodes.Ldarga_S, 0);
                else generator.Emit(OpCodes.Ldarg_0);
                Type type = nullType ?? field.FieldType;
                if (type == typeof(string)) generator.Emit(OpCodes.Ldfld, field);
                else
                {
                    MethodInfo method = TmphPubExtension.GetNumberToStringMethod(type);
                    if (method == null)
                    {
                        generator.Emit(OpCodes.Ldflda, field);
                        generator.Emit(type.IsValueType ? OpCodes.Call : OpCodes.Callvirt, TmphPubExtension.GetToStringMethod(type));
                    }
                    else
                    {
                        if (nullType == null) generator.Emit(OpCodes.Ldfld, field);
                        else
                        {
                            generator.Emit(OpCodes.Ldflda, field);
                            generator.Emit(OpCodes.Call, TmphPubExtension.GetNullableValue(field.FieldType));
                        }
                        generator.Emit(OpCodes.Call, method);
                    }
                }
                generator.Emit(OpCodes.Callvirt, TmphPubExtension.NameValueCollectionAddMethod);
            }

            /// <summary>
            /// 创建web表单委托
            /// </summary>
            /// <returns>web表单委托</returns>
            public Delegate Create<delegateType>()
            {
                generator.Emit(OpCodes.Ret);
                return dynamicMethod.CreateDelegate(typeof(delegateType));
            }
        }
    }

    /// <summary>
    /// web表单生成
    /// </summary>
    /// <typeparam name="TValueType">对象类型</typeparam>
    public static class TmphFormGetter<TValueType>
    {
        /// <summary>
        /// web表单生成委托
        /// </summary>
        private static readonly Action<TValueType, NameValueCollection> getter;

        /// <summary>
        /// 成员数量
        /// </summary>
        private static readonly int memberCount;

        /// <summary>
        /// 获取POST表单
        /// </summary>
        /// <param name="value">查询对象</param>
        /// <returns>POST表单</returns>
        public static NameValueCollection Get(TValueType value)
        {
            if (getter != null)
            {
                NameValueCollection form = new NameValueCollection(memberCount);
                getter(value, form);
                return form;
            }
            return null;
        }

        /// <summary>
        /// 获取POST表单
        /// </summary>
        /// <param name="value">查询对象</param>
        /// <param name="form">POST表单</param>
        public static void Get(TValueType value, NameValueCollection form)
        {
            if (getter != null)
            {
                if (form == null) TmphLog.Default.Throw(TmphLog.TmphExceptionType.Null);
                getter(value, form);
            }
        }

        static TmphFormGetter()
        {
            Type type = typeof(TValueType);
            if (type.IsArray || type.IsEnum || type.IsPointer || type.IsInterface) return;
            foreach (Laurent.Lee.CLB.Code.TmphAttributeMethod methodInfo in Laurent.Lee.CLB.Code.TmphAttributeMethod.GetStatic(type))
            {
                if (methodInfo.Method.ReturnType == typeof(void))
                {
                    ParameterInfo[] parameters = methodInfo.Method.GetParameters();
                    if (parameters.Length == 2 && parameters[0].ParameterType == type && parameters[1].ParameterType == typeof(NameValueCollection))
                    {
                        if (methodInfo.GetAttribute<TmphForm.TmphCustom>(true) != null)
                        {
                            getter = (Action<TValueType, NameValueCollection>)Delegate.CreateDelegate(typeof(Action<TValueType, NameValueCollection>), methodInfo.Method);
                            return;
                        }
                    }
                }
            }
            TmphForm attribute = Laurent.Lee.CLB.Code.TmphTypeAttribute.GetAttribute<TmphForm>(type, true, true) ?? TmphForm.AllMember;
            TmphSubArray<FieldInfo> fields = TmphPubExtension.GetFields<TValueType, TmphForm.TmphMember>(attribute.MemberFilter, attribute.IsAllMember);
            if ((memberCount = fields.Count) != 0)
            {
                TmphFormGetter.TmphMemberDynamicMethod dynamicMethod = new TmphFormGetter.TmphMemberDynamicMethod(type);
                foreach (FieldInfo member in fields) dynamicMethod.Push(member);
                getter = (Action<TValueType, NameValueCollection>)dynamicMethod.Create<Action<TValueType, NameValueCollection>>();
            }
        }
    }
}