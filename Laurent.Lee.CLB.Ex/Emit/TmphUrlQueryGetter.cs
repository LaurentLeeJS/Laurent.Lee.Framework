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
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Laurent.Lee.CLB.Emit
{
    /// <summary>
    /// URL查询字符串生成
    /// </summary>
    /// <typeparam name="TValueType">对象类型</typeparam>
    public static class TmphUrlQueryGetter
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
                dynamicMethod = new DynamicMethod("urlQueryGetter", null, new Type[] { type, typeof(TmphCharStream), typeof(Encoding) }, type, true);
                generator = dynamicMethod.GetILGenerator();
                isValueType = type.IsValueType;
            }

            /// <summary>
            /// 添加字段
            /// </summary>
            /// <param name="field">字段信息</param>
            public void PushFrist(FieldInfo field)
            {
                name(field.Name);
                pushValue(field);
            }

            /// <summary>
            /// 添加字段
            /// </summary>
            /// <param name="field"></param>
            private void pushValue(FieldInfo field)
            {
                MethodInfo method = TmphPubExtension.GetNumberToCharStreamMethod(field.FieldType);
                if (method == null)
                {
                    generator.Emit(OpCodes.Ldarg_1);
                    if (isValueType) generator.Emit(OpCodes.Ldarga_S, 0);
                    else generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldflda, field);
                    generator.Emit(OpCodes.Call, TmphPubExtension.GetToStringMethod(field.FieldType));
                    generator.Emit(OpCodes.Ldarg_2);
                    generator.Emit(OpCodes.Call, TmphPubExtension.UrlEncodeMethod);
                    generator.Emit(OpCodes.Call, TmphPubExtension.CharStreamWriteNotNullMethod);
                }
                else
                {
                    if (isValueType) generator.Emit(OpCodes.Ldarga_S, 0);
                    else generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldfld, field);
                    generator.Emit(OpCodes.Ldarg_1);
                    generator.Emit(OpCodes.Call, method);
                }
            }

            /// <summary>
            /// 添加名称
            /// </summary>
            /// <param name="name"></param>
            private void name(string name)
            {
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Ldstr, name + "=");
                generator.Emit(OpCodes.Call, TmphPubExtension.CharStreamWriteNotNullMethod);
            }

            /// <summary>
            /// 添加名称
            /// </summary>
            /// <param name="name"></param>
            private void nextName(string name)
            {
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Ldstr, "&" + name + "=");
                generator.Emit(OpCodes.Call, TmphPubExtension.CharStreamWriteNotNullMethod);
            }

            /// <summary>
            /// 添加字段
            /// </summary>
            /// <param name="field">字段信息</param>
            public void PushNext(FieldInfo field)
            {
                Type type = field.FieldType;
                if (type.IsValueType)
                {
                    Type nullType = type.nullableType();
                    if (nullType == null)
                    {
                        nextName(field.Name);
                        pushValue(field);
                    }
                    else
                    {
                        Label end = generator.DefineLabel();
                        if (isValueType) generator.Emit(OpCodes.Ldarga_S, 0);
                        else generator.Emit(OpCodes.Ldarg_0);
                        generator.Emit(OpCodes.Ldflda, field);
                        generator.Emit(OpCodes.Call, TmphPubExtension.GetNullableHasValue(type));
                        generator.Emit(OpCodes.Brfalse_S, end);
                        nextName(field.Name);
                        push(field, nullType);
                        generator.MarkLabel(end);
                    }
                }
                else
                {
                    Label end = generator.DefineLabel();
                    if (isValueType) generator.Emit(OpCodes.Ldarga_S, 0);
                    else generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldfld, field);
                    generator.Emit(OpCodes.Brfalse_S, end);
                    nextName(field.Name);
                    push(field);
                    generator.MarkLabel(end);
                }
            }

            /// <summary>
            /// 添加字段
            /// </summary>
            /// <param name="field">字段信息</param>
            /// <param name="nullType">可空类型</param>
            private void push(FieldInfo field, Type nullType = null)
            {
                Type type = nullType ?? field.FieldType;
                if (type == typeof(string))
                {
                    generator.Emit(OpCodes.Ldarg_1);
                    if (isValueType) generator.Emit(OpCodes.Ldarga_S, 0);
                    else generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldfld, field);
                    generator.Emit(OpCodes.Ldarg_2);
                    generator.Emit(OpCodes.Call, TmphPubExtension.UrlEncodeMethod);
                    generator.Emit(OpCodes.Call, TmphPubExtension.CharStreamWriteNotNullMethod);
                }
                else
                {
                    MethodInfo method = TmphPubExtension.GetNumberToCharStreamMethod(type);
                    if (method == null)
                    {
                        generator.Emit(OpCodes.Ldarg_1);
                        if (isValueType) generator.Emit(OpCodes.Ldarga_S, 0);
                        else generator.Emit(OpCodes.Ldarg_0);
                        generator.Emit(OpCodes.Ldflda, field);
                        generator.Emit(type.IsValueType ? OpCodes.Call : OpCodes.Callvirt, TmphPubExtension.GetToStringMethod(type));
                        generator.Emit(OpCodes.Ldarg_2);
                        generator.Emit(OpCodes.Call, TmphPubExtension.UrlEncodeMethod);
                        generator.Emit(OpCodes.Call, TmphPubExtension.CharStreamWriteNotNullMethod);
                    }
                    else
                    {
                        if (isValueType) generator.Emit(OpCodes.Ldarga_S, 0);
                        else generator.Emit(OpCodes.Ldarg_0);
                        if (nullType == null) generator.Emit(OpCodes.Ldfld, field);
                        else
                        {
                            generator.Emit(OpCodes.Ldflda, field);
                            generator.Emit(OpCodes.Call, TmphPubExtension.GetNullableValue(field.FieldType));
                        }
                        generator.Emit(OpCodes.Ldarg_1);
                        generator.Emit(OpCodes.Call, method);
                    }
                }
            }

            /// <summary>
            /// 定义局部变量
            /// </summary>
            public void DeclareIsNext()
            {
                generator.DeclareLocal(typeof(bool));
                generator.Emit(OpCodes.Ldc_I4_0);
                generator.Emit(OpCodes.Stloc_0);
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
                    Label end = generator.DefineLabel();
                    if (isValueType) generator.Emit(OpCodes.Ldarga_S, 0);
                    else generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldflda, field);
                    generator.Emit(OpCodes.Call, TmphPubExtension.GetNullableHasValue(type));
                    generator.Emit(OpCodes.Brfalse_S, end);
                    name(field);
                    push(field, type.nullableType());
                    generator.MarkLabel(end);
                }
                else
                {
                    Label end = generator.DefineLabel();
                    if (isValueType) generator.Emit(OpCodes.Ldarga_S, 0);
                    else generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldfld, field);
                    generator.Emit(OpCodes.Brfalse_S, end);
                    name(field);
                    push(field);
                    generator.MarkLabel(end);
                }
            }

            /// <summary>
            /// 添加名称
            /// </summary>
            /// <param name="field"></param>
            private void name(FieldInfo field)
            {
                Label notNext = generator.DefineLabel(), end = generator.DefineLabel();
                generator.Emit(OpCodes.Ldloc_0);
                generator.Emit(OpCodes.Brfalse_S, notNext);
                nextName(field.Name);
                generator.Emit(OpCodes.Br_S, end);
                generator.MarkLabel(notNext);
                name(field.Name);
                generator.Emit(OpCodes.Ldc_I4_1);
                generator.Emit(OpCodes.Stloc_0);
                generator.MarkLabel(end);
            }

            /// <summary>
            /// 创建委托
            /// </summary>
            /// <returns>委托</returns>
            public Delegate Create<delegateType>()
            {
                generator.Emit(OpCodes.Ret);
                return dynamicMethod.CreateDelegate(typeof(delegateType));
            }
        }
    }

    /// <summary>
    /// URL查询字符串生成
    /// </summary>
    /// <typeparam name="TValueType">对象类型</typeparam>
    public static class TmphUrlQueryGetter<TValueType>
    {
        /// <summary>
        /// URL查询字符串生成委托
        /// </summary>
        private static readonly Action<TValueType, TmphCharStream, Encoding> getter;

        /// <summary>
        /// 获取URL查询字符串
        /// </summary>
        /// <param name="value">查询对象</param>
        /// <param name="encoding">URL编码</param>
        public unsafe static string Get(TValueType value, Encoding encoding)
        {
            if (getter != null)
            {
                TmphPointer buffer = CLB.TmphUnmanagedPool.TinyBuffers.Get();
                try
                {
                    using (TmphCharStream stream = new TmphCharStream(buffer.Char, CLB.TmphUnmanagedPool.TinyBuffers.Size >> 1))
                    {
                        getter(value, stream, encoding);
                        return stream.ToString();
                    }
                }
                finally { CLB.TmphUnmanagedPool.TinyBuffers.Push(ref buffer); }
            }
            return null;
        }

        /// <summary>
        /// 获取URL查询字符串
        /// </summary>
        /// <param name="value">查询对象</param>
        /// <param name="stream">URL查询字符串</param>
        /// <param name="encoding">URL编码</param>
        public static void Get(TValueType value, TmphCharStream stream, Encoding encoding)
        {
            if (getter != null)
            {
                if (stream == null) TmphLog.Default.Throw(TmphLog.TmphExceptionType.Null);
                getter(value, stream, encoding);
            }
        }

        static TmphUrlQueryGetter()
        {
            Type type = typeof(TValueType);
            if (type.IsArray || type.IsEnum || type.IsPointer || type.IsInterface) return;
            foreach (Laurent.Lee.CLB.Code.TmphAttributeMethod methodInfo in Laurent.Lee.CLB.Code.TmphAttributeMethod.GetStatic(type))
            {
                if (methodInfo.Method.ReturnType == typeof(void))
                {
                    ParameterInfo[] parameters = methodInfo.Method.GetParameters();
                    if (parameters.Length == 3 && parameters[0].ParameterType == type && parameters[1].ParameterType == typeof(TmphCharStream) && parameters[2].ParameterType == typeof(Encoding))
                    {
                        if (methodInfo.GetAttribute<TmphUrlQuery.TmphCustom>(true) != null)
                        {
                            getter = (Action<TValueType, TmphCharStream, Encoding>)Delegate.CreateDelegate(typeof(Action<TValueType, TmphCharStream, Encoding>), methodInfo.Method);
                            return;
                        }
                    }
                }
            }
            TmphUrlQuery attribute = Laurent.Lee.CLB.Code.TmphTypeAttribute.GetAttribute<TmphUrlQuery>(type, true, true) ?? TmphUrlQuery.AllMember;
            TmphSubArray<FieldInfo> fields = TmphPubExtension.GetFields<TValueType, TmphUrlQuery.TmphMember>(attribute.MemberFilter, attribute.IsAllMember);
            if (fields.Count != 0)
            {
                FieldInfo valueField = null;
                TmphUrlQueryGetter.TmphMemberDynamicMethod dynamicMethod = new TmphUrlQueryGetter.TmphMemberDynamicMethod(type);
                foreach (FieldInfo member in fields)
                {
                    if (member.FieldType.IsValueType && member.FieldType.nullableType() == null)
                    {
                        dynamicMethod.PushFrist(valueField = member);
                        break;
                    }
                }
                if (valueField == null)
                {
                    dynamicMethod.DeclareIsNext();
                    foreach (FieldInfo member in fields) dynamicMethod.Push(member);
                }
                else
                {
                    foreach (FieldInfo member in fields)
                    {
                        if (valueField != member) dynamicMethod.PushNext(member);
                    }
                }
                getter = (Action<TValueType, TmphCharStream, Encoding>)dynamicMethod.Create<Action<TValueType, TmphCharStream, Encoding>>();
            }
        }
    }
}