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

using Laurent.Lee.CLB.Code;
using Laurent.Lee.CLB.Reflection;
using Laurent.Lee.CLB.Sql;
using Laurent.Lee.CLB.Sql.Expression;
using Laurent.Lee.CLB.Threading;
using System;
using System.Data.Common;
using System.Reflection;
using System.Reflection.Emit;

namespace Laurent.Lee.CLB.Emit
{
    /// <summary>
    ///     SQL列配置
    /// </summary>
    public class TmphSqlColumn : Code.CSharp.TmphSqlModel
    {
        /// <summary>
        ///     默认空属性
        /// </summary>
        internal new static readonly TmphSqlColumn Default = new TmphSqlColumn();

        /// <summary>
        ///     获取成员名称与类型集合函数信息
        /// </summary>
        private static readonly MethodInfo getDataColumnsMethod = typeof(TmphSqlColumn).GetMethod("getDataColumns",
            BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     获取成员名称与类型集合
        /// </summary>
        /// <param name="name">列名前缀</param>
        /// <returns></returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static TmphKeyValue<string, Type>[] getDataColumns<TValueType>(string name)
        {
            return TmphSqlColumn<TValueType>.GetDataColumns(name);
        }

        /// <summary>
        ///     自定义类型标识配置
        /// </summary>
        public sealed class TmphCustom : Attribute
        {
        }

        /// <summary>
        ///     自定义类型处理接口
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        public interface TmphICustom<TValueType>
        {
            /// <summary>
            ///     设置字段值
            /// </summary>
            /// <param name="reader">字段读取器物理存储</param>
            /// <param name="value">目标数据</param>
            /// <param name="index">当前读取位置</param>
            void Set(DbDataReader reader, ref TValueType value, ref int index);

            /// <summary>
            ///     数据验证
            /// </summary>
            /// <param name="value"></param>
            /// <param name="sqlTool"></param>
            /// <param name="columnName"></param>
            /// <returns></returns>
            bool Verify(TValueType value, TmphSqlTable.TmphSqlTool sqlTool, string columnName);

            /// <summary>
            ///     获取,分割列名集合
            /// </summary>
            /// <param name="name">列名前缀</param>
            /// <returns></returns>
            string GetColumnNames(string name);

            /// <summary>
            ///     获取成员名称与类型集合
            /// </summary>
            /// <param name="name">列名前缀</param>
            /// <returns></returns>
            TmphKeyValue<string, Type>[] GetDataColumns(string name);

            /// <summary>
            ///     获取插入数据SQL表达式
            /// </summary>
            /// <param name="sqlStream">SQL表达式流</param>
            /// <param name="value">数据列</param>
            /// <param name="converter">SQL常量转换</param>
            void Insert(TmphCharStream sqlStream, TValueType value, TmphConstantConverter converter);

            /// <summary>
            ///     获取更新数据SQL表达式
            /// </summary>
            /// <param name="sqlStream">SQL表达式流</param>
            /// <param name="value">数据列</param>
            /// <param name="converter">SQL常量转换</param>
            /// <param name="columnName">列名前缀</param>
            void Update(TmphCharStream sqlStream, TValueType value, TmphConstantConverter converter, string columnName);

            /// <summary>
            ///     读取字段值
            /// </summary>
            /// <param name="value">数据列</param>
            /// <param name="values">目标数组</param>
            /// <param name="index">当前写入位置</param>
            void ToArray(TValueType value, object[] values, ref int index);

            /// <summary>
            ///     获取添加SQL表达式
            /// </summary>
            /// <param name="sqlStream">SQL表达式流</param>
            /// <param name="value">数据列</param>
            /// <param name="converter">SQL常量转换</param>
            /// <param name="columnName">列名前缀</param>
            void Where(TmphCharStream sqlStream, TValueType value, TmphConstantConverter converter, string columnName);
        }

        /// <summary>
        ///     动态函数
        /// </summary>
        public struct TmphVerifyDynamicMethod
        {
            /// <summary>
            ///     类型调用函数信息集合
            /// </summary>
            private static TmphInterlocked.TmphDictionary<Type, MethodInfo> typeVerifyers =
                new TmphInterlocked.TmphDictionary<Type, MethodInfo>(TmphDictionary.CreateOnly<Type, MethodInfo>());

            /// <summary>
            ///     数据验证函数信息
            /// </summary>
            private static readonly MethodInfo verifyMethod = typeof(TmphVerifyDynamicMethod).GetMethod("verify",
                BindingFlags.Static | BindingFlags.NonPublic);

            /// <summary>
            ///     获取列名委托集合
            /// </summary>
            private static TmphInterlocked.TmphDictionary<Type, Action<TmphList<string>, string>> getColumnNameMethods =
                new TmphInterlocked.TmphDictionary<Type, Action<TmphList<string>, string>>(
                    TmphDictionary.CreateOnly<Type, Action<TmphList<string>, string>>());

            /// <summary>
            ///     获取列名集合函数信息
            /// </summary>
            private static readonly MethodInfo getColumnNamesMethod =
                typeof(TmphVerifyDynamicMethod).GetMethod("getColumnNames", BindingFlags.Static | BindingFlags.NonPublic);

            /// <summary>
            ///     动态函数
            /// </summary>
            private readonly DynamicMethod dynamicMethod;

            /// <summary>
            /// </summary>
            private readonly ILGenerator generator;

            /// <summary>
            ///     动态函数
            /// </summary>
            /// <param name="type"></param>
            public TmphVerifyDynamicMethod(Type type)
            {
                dynamicMethod = new DynamicMethod("sqlColumnVerify", typeof(bool),
                    new[] { type, typeof(TmphSqlTable.TmphSqlTool), typeof(string[]) }, type, true);
                generator = dynamicMethod.GetILGenerator();
            }

            /// <summary>
            ///     添加字段
            /// </summary>
            /// <param name="field">字段信息</param>
            /// <param name="index">名称序号</param>
            public void Push(TmphFieldInfo field, int index)
            {
                var end = generator.DefineLabel();
                if (field.IsSqlColumn)
                {
                    generator.Emit(OpCodes.Ldarga_S, 0);
                    generator.Emit(OpCodes.Ldfld, field.Field);
                    generator.Emit(OpCodes.Ldarg_1);
                    generator.Emit(OpCodes.Ldarg_2);
                    generator.Emit(OpCodes.Ldc_I4, index);
                    generator.Emit(OpCodes.Ldelem_Ref);
                    generator.Emit(OpCodes.Call, GetTypeVerifyer(field.DataType));
                    generator.Emit(OpCodes.Brtrue_S, end);
                }
                else if (field.DataType == typeof(string))
                {
                    generator.Emit(OpCodes.Ldarg_1);
                    generator.Emit(OpCodes.Ldarg_2);
                    generator.Emit(OpCodes.Ldc_I4, index);
                    generator.Emit(OpCodes.Ldelem_Ref);
                    generator.Emit(OpCodes.Ldarga_S, 0);
                    generator.Emit(OpCodes.Ldfld, field.Field);
                    var castMethod = TmphPub.GetCastMethod(field.Field.FieldType, field.DataType);
                    if (castMethod != null) generator.Emit(OpCodes.Call, castMethod);
                    generator.Emit(OpCodes.Ldc_I4, field.DataMember.MaxStringLength);
                    generator.Emit(field.DataMember.IsAscii ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                    generator.Emit(field.DataMember.IsNull ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                    generator.Emit(OpCodes.Callvirt, TmphSqlTable.TmphSqlTool.StringVerifyMethod);
                    generator.Emit(OpCodes.Brtrue_S, end);
                }
                else
                {
                    generator.Emit(OpCodes.Ldarga_S, 0);
                    generator.Emit(OpCodes.Ldfld, field.Field);
                    var castMethod = TmphPub.GetCastMethod(field.Field.FieldType, field.DataType);
                    if (castMethod != null) generator.Emit(OpCodes.Call, castMethod);
                    generator.Emit(OpCodes.Brtrue_S, end);
                    generator.Emit(OpCodes.Ldarg_1);
                    generator.Emit(OpCodes.Ldarg_2);
                    generator.Emit(OpCodes.Ldc_I4, index);
                    generator.Emit(OpCodes.Ldelem_Ref);
                    generator.Emit(OpCodes.Callvirt, TmphSqlTable.TmphSqlTool.NullVerifyMethod);
                }
                generator.Emit(OpCodes.Ldc_I4_0);
                generator.Emit(OpCodes.Ret);
                generator.MarkLabel(end);
            }

            /// <summary>
            ///     创建web表单委托
            /// </summary>
            /// <returns>web表单委托</returns>
            public Delegate Create<delegateType>()
            {
                generator.Emit(OpCodes.Ldc_I4_1);
                generator.Emit(OpCodes.Ret);
                return dynamicMethod.CreateDelegate(typeof(delegateType));
            }

            /// <summary>
            ///     类型委托调用函数信息
            /// </summary>
            /// <param name="type">数组类型</param>
            /// <returns>类型委托调用函数信息</returns>
            public static MethodInfo GetTypeVerifyer(Type type)
            {
                MethodInfo method;
                if (typeVerifyers.TryGetValue(type, out method)) return method;
                typeVerifyers.Set(type, method = verifyMethod.MakeGenericMethod(type));
                return method;
            }

            /// <summary>
            ///     数据验证
            /// </summary>
            /// <param name="value"></param>
            /// <param name="sqlTool"></param>
            /// <param name="columnName"></param>
            /// <returns></returns>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static bool verify<TValueType>(TValueType value, TmphSqlTable.TmphSqlTool sqlTool, string columnName)
            {
                return TmphSqlColumn<TValueType>.TmphVerify.Verify(value, sqlTool, columnName);
            }

            /// <summary>
            ///     获取列名委托
            /// </summary>
            /// <param name="type">数据列类型</param>
            /// <returns>获取列名委托</returns>
            public static Action<TmphList<string>, string> GetColumnNames(Type type)
            {
                Action<TmphList<string>, string> getColumnName;
                if (getColumnNameMethods.TryGetValue(type, out getColumnName)) return getColumnName;
                getColumnName =
                    (Action<TmphList<string>, string>)
                        Delegate.CreateDelegate(typeof(Action<TmphList<string>, string>),
                            getColumnNamesMethod.MakeGenericMethod(type));
                getColumnNameMethods.Set(type, getColumnName);
                return getColumnName;
            }

            /// <summary>
            ///     获取列名集合
            /// </summary>
            /// <param name="names">列名集合</param>
            /// <param name="name">列名前缀</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void getColumnNames<TValueType>(TmphList<string> names, string name)
            {
                names.Add(TmphSqlColumn<TValueType>.TmphVerify.GetColumnNames(name));
            }
        }

        /// <summary>
        ///     动态函数
        /// </summary>
        public struct TmphSetDynamicMethod
        {
            /// <summary>
            ///     类型调用函数信息集合
            /// </summary>
            private static TmphInterlocked.TmphDictionary<Type, MethodInfo> typeSetters =
                new TmphInterlocked.TmphDictionary<Type, MethodInfo>(TmphDictionary.CreateOnly<Type, MethodInfo>());

            /// <summary>
            ///     设置字段值函数信息
            /// </summary>
            private static readonly MethodInfo setMethod = typeof(TmphSetDynamicMethod).GetMethod("set",
                BindingFlags.Static | BindingFlags.NonPublic);

            /// <summary>
            ///     动态函数
            /// </summary>
            private readonly DynamicMethod dynamicMethod;

            /// <summary>
            /// </summary>
            private readonly ILGenerator generator;

            /// <summary>
            ///     动态函数
            /// </summary>
            /// <param name="type"></param>
            public TmphSetDynamicMethod(Type type)
            {
                dynamicMethod = new DynamicMethod("sqlColumnSet", null,
                    new[] { typeof(DbDataReader), type.MakeByRefType(), TmphPub.RefIntType }, type, true);
                generator = dynamicMethod.GetILGenerator();
            }

            /// <summary>
            ///     添加字段
            /// </summary>
            /// <param name="field">字段信息</param>
            public void Push(TmphFieldInfo field)
            {
                if (field.DataReaderMethod == null)
                {
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldarg_1);
                    generator.Emit(OpCodes.Ldflda, field.Field);
                    generator.Emit(OpCodes.Ldarg_2);
                    generator.Emit(OpCodes.Call, GetTypeSetter(field.DataType));
                }
                else
                {
                    if (field.DataType == field.NullableDataType &&
                        (field.DataType.IsValueType || !field.DataMember.IsNull))
                    {
                        generator.Emit(OpCodes.Ldarg_1);
                        generator.Emit(OpCodes.Ldarg_0);
                        generator.Emit(OpCodes.Ldarg_2);
                        generator.Emit(OpCodes.Ldind_I4);
                        generator.Emit(OpCodes.Callvirt, field.DataReaderMethod);
                        generator.Emit(OpCodes.Stfld, field.Field);
                    }
                    else
                    {
                        Label notNull = generator.DefineLabel(), end = generator.DefineLabel();
                        generator.Emit(OpCodes.Ldarg_0);
                        generator.Emit(OpCodes.Ldarg_2);
                        generator.Emit(OpCodes.Ldind_I4);
                        generator.Emit(OpCodes.Callvirt, TmphPub.DataReaderIsDBNullMethod);
                        generator.Emit(OpCodes.Brfalse_S, notNull);

                        generator.Emit(OpCodes.Ldarg_1);
                        if (field.DataType == field.NullableDataType)
                        {
                            generator.Emit(OpCodes.Ldnull);
                            generator.Emit(OpCodes.Stfld, field.Field);
                        }
                        else
                        {
                            generator.Emit(OpCodes.Ldflda, field.Field);
                            generator.Emit(OpCodes.Initobj, field.Field.FieldType);
                        }
                        generator.Emit(OpCodes.Br_S, end);

                        generator.MarkLabel(notNull);
                        generator.Emit(OpCodes.Ldarg_1);
                        generator.Emit(OpCodes.Ldarg_0);
                        generator.Emit(OpCodes.Ldarg_2);
                        generator.Emit(OpCodes.Ldind_I4);
                        generator.Emit(OpCodes.Callvirt, field.DataReaderMethod);
                        if (field.DataType == field.NullableDataType)
                        {
                            var castMethod = TmphPub.GetCastMethod(field.DataType, field.Field.FieldType);
                            if (castMethod != null) generator.Emit(OpCodes.Call, castMethod);
                        }
                        else generator.Emit(OpCodes.Newobj, TmphPub.NullableConstructors[field.DataType]);
                        generator.Emit(OpCodes.Stfld, field.Field);
                        generator.MarkLabel(end);
                    }
                    generator.Emit(OpCodes.Ldarg_2);
                    generator.Emit(OpCodes.Dup);
                    generator.Emit(OpCodes.Ldind_I4);
                    generator.Emit(OpCodes.Ldc_I4_1);
                    generator.Emit(OpCodes.Add);
                    generator.Emit(OpCodes.Stind_I4);
                }
            }

            /// <summary>
            ///     创建web表单委托
            /// </summary>
            /// <returns>web表单委托</returns>
            public Delegate Create<delegateType>()
            {
                generator.Emit(OpCodes.Ret);
                return dynamicMethod.CreateDelegate(typeof(delegateType));
            }

            /// <summary>
            ///     类型委托调用函数信息
            /// </summary>
            /// <param name="type">数组类型</param>
            /// <returns>类型委托调用函数信息</returns>
            internal static MethodInfo GetTypeSetter(Type type)
            {
                MethodInfo method;
                if (typeSetters.TryGetValue(type, out method)) return method;
                typeSetters.Set(type, method = setMethod.MakeGenericMethod(type));
                return method;
            }

            /// <summary>
            ///     设置字段值
            /// </summary>
            /// <param name="reader">字段读取器物理存储</param>
            /// <param name="value">目标数据</param>
            /// <param name="index">当前读取位置</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void set<TValueType>(DbDataReader reader, ref TValueType value, ref int index)
            {
                TmphSqlColumn<TValueType>.TmphSet.Set(reader, ref value, ref index);
            }
        }

        /// <summary>
        ///     数据列转换数组动态函数
        /// </summary>
        public struct TmphToArrayDynamicMethod
        {
            /// <summary>
            ///     类型调用函数信息集合
            /// </summary>
            private static TmphInterlocked.TmphDictionary<Type, MethodInfo> typeToArrays =
                new TmphInterlocked.TmphDictionary<Type, MethodInfo>(TmphDictionary.CreateOnly<Type, MethodInfo>());

            /// <summary>
            ///     数据列转换数组函数信息
            /// </summary>
            private static readonly MethodInfo toArrayMethod = typeof(TmphToArrayDynamicMethod).GetMethod("toArray",
                BindingFlags.Static | BindingFlags.NonPublic);

            /// <summary>
            ///     获取列名与类型委托集合
            /// </summary>
            private static TmphInterlocked.TmphDictionary<Type, Func<string, TmphKeyValue<string, Type>[]>> getDataColumns =
                new TmphInterlocked.TmphDictionary<Type, Func<string, TmphKeyValue<string, Type>[]>>(
                    TmphDictionary.CreateOnly<Type, Func<string, TmphKeyValue<string, Type>[]>>());

            /// <summary>
            ///     动态函数
            /// </summary>
            private readonly DynamicMethod dynamicMethod;

            /// <summary>
            /// </summary>
            private readonly ILGenerator generator;

            /// <summary>
            ///     动态函数
            /// </summary>
            /// <param name="type"></param>
            public TmphToArrayDynamicMethod(Type type)
            {
                dynamicMethod = new DynamicMethod("sqlColumnToArray", null,
                    new[] { type, typeof(object[]), TmphPub.RefIntType }, type, true);
                generator = dynamicMethod.GetILGenerator();
            }

            /// <summary>
            ///     添加字段
            /// </summary>
            /// <param name="field">字段信息</param>
            public void Push(TmphFieldInfo field)
            {
                if (field.IsSqlColumn)
                {
                    generator.Emit(OpCodes.Ldarga_S, 0);
                    generator.Emit(OpCodes.Ldfld, field.Field);
                    generator.Emit(OpCodes.Ldarg_1);
                    generator.Emit(OpCodes.Ldarg_2);
                    generator.Emit(OpCodes.Call, GetTypeToArray(field.DataType));
                }
                else
                {
                    if (field.DataType == field.NullableDataType)
                    {
                        generator.Emit(OpCodes.Ldarg_1);
                        generator.Emit(OpCodes.Ldarg_2);
                        generator.Emit(OpCodes.Ldind_I4);
                        generator.Emit(OpCodes.Ldarga_S, 0);
                        generator.Emit(OpCodes.Ldfld, field.Field);
                        var castMethod = TmphPub.GetCastMethod(field.Field.FieldType, field.DataType);
                        if (castMethod != null) generator.Emit(OpCodes.Call, castMethod);
                        if (field.DataType.IsValueType) generator.Emit(OpCodes.Box, field.DataType);
                        generator.Emit(OpCodes.Stelem_Ref);
                    }
                    else
                    {
                        var end = generator.DefineLabel();
                        generator.Emit(OpCodes.Ldarga_S, 0);
                        generator.Emit(OpCodes.Ldflda, field.Field);
                        generator.Emit(OpCodes.Call, TmphPub.GetNullableHasValue(field.NullableDataType));
                        generator.Emit(OpCodes.Brtrue_S, end);
                        generator.Emit(OpCodes.Ldarg_1);
                        generator.Emit(OpCodes.Ldarg_2);
                        generator.Emit(OpCodes.Ldind_I4);
                        generator.Emit(OpCodes.Ldarga_S, 0);
                        generator.Emit(OpCodes.Ldflda, field.Field);
                        generator.Emit(OpCodes.Call, TmphPub.GetNullableValue(field.NullableDataType));
                        generator.Emit(OpCodes.Box, field.DataType);
                        generator.Emit(OpCodes.Stelem_Ref);
                        generator.MarkLabel(end);
                    }
                }
                generator.Emit(OpCodes.Ldarg_2);
                generator.Emit(OpCodes.Dup);
                generator.Emit(OpCodes.Ldind_I4);
                generator.Emit(OpCodes.Ldc_I4_1);
                generator.Emit(OpCodes.Add);
                generator.Emit(OpCodes.Stind_I4);
            }

            /// <summary>
            ///     创建web表单委托
            /// </summary>
            /// <returns>web表单委托</returns>
            public Delegate Create<delegateType>()
            {
                generator.Emit(OpCodes.Ret);
                return dynamicMethod.CreateDelegate(typeof(delegateType));
            }

            /// <summary>
            ///     类型委托调用函数信息
            /// </summary>
            /// <param name="type">数组类型</param>
            /// <returns>类型委托调用函数信息</returns>
            public static MethodInfo GetTypeToArray(Type type)
            {
                MethodInfo method;
                if (typeToArrays.TryGetValue(type, out method)) return method;
                typeToArrays.Set(type, method = toArrayMethod.MakeGenericMethod(type));
                return method;
            }

            /// <summary>
            ///     数据列转换数组
            /// </summary>
            /// <param name="values">目标数组</param>
            /// <param name="value">数据列</param>
            /// <param name="index">当前读取位置</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void toArray<TValueType>(TValueType value, object[] values, ref int index)
            {
                TmphSqlColumn<TValueType>.TmphToArray.ToArray(value, values, ref index);
            }

            /// <summary>
            ///     获取列名与类型委托
            /// </summary>
            /// <param name="type">数据列类型</param>
            /// <returns>获取列名与类型委托</returns>
            public static Func<string, TmphKeyValue<string, Type>[]> GetDataColumns(Type type)
            {
                Func<string, TmphKeyValue<string, Type>[]> getDataColumn;
                if (getDataColumns.TryGetValue(type, out getDataColumn)) return getDataColumn;
                getDataColumn =
                    (Func<string, TmphKeyValue<string, Type>[]>)
                        Delegate.CreateDelegate(typeof(Func<string, TmphKeyValue<string, Type>[]>),
                            getDataColumnsMethod.MakeGenericMethod(type));
                getDataColumns.Set(type, getDataColumn);
                return getDataColumn;
            }
        }

        /// <summary>
        ///     动态函数
        /// </summary>
        public struct TmphInsertDynamicMethod
        {
            /// <summary>
            ///     类型调用函数信息集合
            /// </summary>
            private static TmphInterlocked.TmphDictionary<Type, MethodInfo> typeInserts =
                new TmphInterlocked.TmphDictionary<Type, MethodInfo>(TmphDictionary.CreateOnly<Type, MethodInfo>());

            /// <summary>
            ///     获取插入数据SQL表达式函数信息
            /// </summary>
            private static readonly MethodInfo insertMethod = typeof(TmphInsertDynamicMethod).GetMethod("insert",
                BindingFlags.Static | BindingFlags.NonPublic);

            /// <summary>
            ///     获取列名委托集合
            /// </summary>
            private static TmphInterlocked.TmphDictionary<Type, Func<string, string>> getColumnNameMethods =
                new TmphInterlocked.TmphDictionary<Type, Func<string, string>>(
                    TmphDictionary.CreateOnly<Type, Func<string, string>>());

            /// <summary>
            ///     获取列名集合函数信息
            /// </summary>
            private static readonly MethodInfo getColumnNamesMethod =
                typeof(TmphInsertDynamicMethod).GetMethod("getColumnNames", BindingFlags.Static | BindingFlags.NonPublic);

            /// <summary>
            ///     动态函数
            /// </summary>
            private readonly DynamicMethod dynamicMethod;

            /// <summary>
            /// </summary>
            private readonly ILGenerator generator;

            /// <summary>
            /// </summary>
            private bool isNextMember;

            /// <summary>
            ///     动态函数
            /// </summary>
            /// <param name="type"></param>
            public TmphInsertDynamicMethod(Type type)
            {
                dynamicMethod = new DynamicMethod("sqlColumnInsert", null,
                    new[] { typeof(TmphCharStream), type, typeof(TmphConstantConverter) }, type, true);
                generator = dynamicMethod.GetILGenerator();
                isNextMember = false;
            }

            /// <summary>
            ///     添加字段
            /// </summary>
            /// <param name="field">字段信息</param>
            public void Push(TmphFieldInfo field)
            {
                if (isNextMember)
                {
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldc_I4_S, (byte)',');
                    generator.Emit(OpCodes.Callvirt, TmphPub.CharStreamWriteCharMethod);
                }
                else isNextMember = true;
                if (field.IsSqlColumn)
                {
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldarga_S, 1);
                    generator.Emit(OpCodes.Ldfld, field.Field);
                    generator.Emit(OpCodes.Ldarg_2);
                    generator.Emit(OpCodes.Call, GetTypeInsert(field.DataType));
                }
                else
                {
                    generator.Emit(OpCodes.Ldarg_2);
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldarga_S, 1);
                    generator.Emit(OpCodes.Ldfld, field.Field);
                    var castMethod = TmphPub.GetCastMethod(field.Field.FieldType, field.DataType);
                    if (castMethod != null) generator.Emit(OpCodes.Call, castMethod);
                    generator.Emit(OpCodes.Callvirt, TmphPub.GetSqlConverterMethod(field.DataType));
                }
            }

            /// <summary>
            ///     创建web表单委托
            /// </summary>
            /// <returns>web表单委托</returns>
            public Delegate Create<delegateType>()
            {
                generator.Emit(OpCodes.Ret);
                return dynamicMethod.CreateDelegate(typeof(delegateType));
            }

            /// <summary>
            ///     类型委托调用函数信息
            /// </summary>
            /// <param name="type">数组类型</param>
            /// <returns>类型委托调用函数信息</returns>
            public static MethodInfo GetTypeInsert(Type type)
            {
                MethodInfo method;
                if (typeInserts.TryGetValue(type, out method)) return method;
                typeInserts.Set(type, method = insertMethod.MakeGenericMethod(type));
                return method;
            }

            /// <summary>
            ///     获取插入数据SQL表达式
            /// </summary>
            /// <param name="sqlStream">SQL表达式流</param>
            /// <param name="value">数据列</param>
            /// <param name="converter">SQL常量转换</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void insert<TValueType>(TmphCharStream sqlStream, TValueType value, TmphConstantConverter converter)
            {
                TmphSqlColumn<TValueType>.TmphInsert.Insert(sqlStream, value, converter);
            }

            /// <summary>
            ///     获取列名委托
            /// </summary>
            /// <param name="type">数据列类型</param>
            /// <returns>获取列名委托</returns>
            public static Func<string, string> GetColumnNames(Type type)
            {
                Func<string, string> getColumnName;
                if (getColumnNameMethods.TryGetValue(type, out getColumnName)) return getColumnName;
                getColumnName =
                    (Func<string, string>)
                        Delegate.CreateDelegate(typeof(Func<string, string>),
                            getColumnNamesMethod.MakeGenericMethod(type));
                getColumnNameMethods.Set(type, getColumnName);
                return getColumnName;
            }

            /// <summary>
            ///     获取列名集合
            /// </summary>
            /// <param name="name">列名前缀</param>
            /// <returns></returns>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static string getColumnNames<TValueType>(string name)
            {
                return TmphSqlColumn<TValueType>.TmphInsert.GetColumnNames(name);
            }
        }

        /// <summary>
        ///     更新数据动态函数
        /// </summary>
        public struct TmphUpdateDynamicMethod
        {
            /// <summary>
            ///     类型调用函数信息集合
            /// </summary>
            private static TmphInterlocked.TmphDictionary<Type, MethodInfo> typeUpdates =
                new TmphInterlocked.TmphDictionary<Type, MethodInfo>(TmphDictionary.CreateOnly<Type, MethodInfo>());

            /// <summary>
            ///     获取更新数据SQL表达式函数信息
            /// </summary>
            private static readonly MethodInfo updateMethod = typeof(TmphUpdateDynamicMethod).GetMethod("update",
                BindingFlags.Static | BindingFlags.NonPublic);

            /// <summary>
            ///     获取列名委托集合
            /// </summary>
            private static TmphInterlocked.TmphDictionary<Type, Action<TmphList<string>, string>> getColumnNameMethods =
                new TmphInterlocked.TmphDictionary<Type, Action<TmphList<string>, string>>(
                    TmphDictionary.CreateOnly<Type, Action<TmphList<string>, string>>());

            /// <summary>
            ///     获取列名集合函数信息
            /// </summary>
            private static readonly MethodInfo getColumnNamesMethod =
                typeof(TmphUpdateDynamicMethod).GetMethod("getColumnNames", BindingFlags.Static | BindingFlags.NonPublic);

            /// <summary>
            ///     动态函数
            /// </summary>
            private readonly DynamicMethod dynamicMethod;

            /// <summary>
            /// </summary>
            private readonly ILGenerator generator;

            /// <summary>
            /// </summary>
            private bool isNextMember;

            /// <summary>
            ///     动态函数
            /// </summary>
            /// <param name="type"></param>
            public TmphUpdateDynamicMethod(Type type)
            {
                dynamicMethod = new DynamicMethod("sqlColumnUpdate", null,
                    new[] { typeof(TmphCharStream), type, typeof(TmphConstantConverter), typeof(string[]) }, type, true);
                generator = dynamicMethod.GetILGenerator();
                isNextMember = false;
            }

            /// <summary>
            ///     动态函数
            /// </summary>
            /// <param name="dynamicMethod"></param>
            /// <param name="generator"></param>
            public TmphUpdateDynamicMethod(DynamicMethod dynamicMethod, ILGenerator generator)
            {
                this.dynamicMethod = dynamicMethod;
                this.generator = generator;
                isNextMember = false;
            }

            /// <summary>
            ///     添加字段
            /// </summary>
            /// <param name="field">字段信息</param>
            /// <param name="index">字段名称序号</param>
            public void Push(TmphFieldInfo field, int index)
            {
                if (isNextMember)
                {
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldc_I4_S, (byte)',');
                    generator.Emit(OpCodes.Callvirt, TmphPub.CharStreamWriteCharMethod);
                }
                else isNextMember = true;
                PushOnly(field, index);
            }

            /// <summary>
            ///     添加字段
            /// </summary>
            /// <param name="field">字段信息</param>
            /// <param name="index">字段名称序号</param>
            public void PushOnly(TmphFieldInfo field, int index)
            {
                if (field.IsSqlColumn)
                {
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldarga_S, 1);
                    generator.Emit(OpCodes.Ldfld, field.Field);
                    generator.Emit(OpCodes.Ldarg_2);
                    generator.Emit(OpCodes.Ldarg_3);
                    generator.Emit(OpCodes.Ldc_I4, index);
                    generator.Emit(OpCodes.Ldelem_Ref);
                    generator.Emit(OpCodes.Call, GetTypeUpdate(field.DataType));
                }
                else
                {
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldarg_3);
                    generator.Emit(OpCodes.Ldc_I4, index);
                    generator.Emit(OpCodes.Ldelem_Ref);
                    generator.Emit(OpCodes.Call, TmphPub.CharStreamWriteNotNullMethod);
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldc_I4_S, (byte)'=');
                    generator.Emit(OpCodes.Callvirt, TmphPub.CharStreamWriteCharMethod);

                    generator.Emit(OpCodes.Ldarg_2);
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldarga_S, 1);
                    generator.Emit(OpCodes.Ldfld, field.Field);
                    var castMethod = TmphPub.GetCastMethod(field.Field.FieldType, field.DataType);
                    if (castMethod != null) generator.Emit(OpCodes.Call, castMethod);
                    generator.Emit(OpCodes.Callvirt, TmphPub.GetSqlConverterMethod(field.DataType));
                }
            }

            /// <summary>
            ///     创建web表单委托
            /// </summary>
            /// <returns>web表单委托</returns>
            public Delegate Create<delegateType>()
            {
                generator.Emit(OpCodes.Ret);
                return dynamicMethod.CreateDelegate(typeof(delegateType));
            }

            /// <summary>
            ///     类型委托调用函数信息
            /// </summary>
            /// <param name="type">数组类型</param>
            /// <returns>类型委托调用函数信息</returns>
            public static MethodInfo GetTypeUpdate(Type type)
            {
                MethodInfo method;
                if (typeUpdates.TryGetValue(type, out method)) return method;
                typeUpdates.Set(type, method = updateMethod.MakeGenericMethod(type));
                return method;
            }

            /// <summary>
            ///     获取更新数据SQL表达式
            /// </summary>
            /// <param name="sqlStream">SQL表达式流</param>
            /// <param name="value">数据列</param>
            /// <param name="converter">SQL常量转换</param>
            /// <param name="columnName">列名前缀</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void update<TValueType>(TmphCharStream sqlStream, TValueType value, TmphConstantConverter converter,
                string columnName)
            {
                TmphSqlColumn<TValueType>.TmphUpdate.Update(sqlStream, value, converter, columnName);
            }

            /// <summary>
            ///     获取列名委托
            /// </summary>
            /// <param name="type">数据列类型</param>
            /// <returns>获取列名委托</returns>
            public static Action<TmphList<string>, string> GetColumnNames(Type type)
            {
                //showjim
                Action<TmphList<string>, string> getColumnName;
                if (getColumnNameMethods.TryGetValue(type, out getColumnName)) return getColumnName;
                getColumnName =
                    (Action<TmphList<string>, string>)
                        Delegate.CreateDelegate(typeof(Action<TmphList<string>, string>),
                            getColumnNamesMethod.MakeGenericMethod(type));
                getColumnNameMethods.Set(type, getColumnName);
                return getColumnName;
            }

            /// <summary>
            ///     获取列名集合
            /// </summary>
            /// <param name="names">列名集合</param>
            /// <param name="name">列名前缀</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void getColumnNames<TValueType>(TmphList<string> names, string name)
            {
                names.Add(TmphSqlColumn<TValueType>.TmphUpdate.GetColumnNames(name));
            }
        }

        /// <summary>
        ///     关键字条件动态函数
        /// </summary>
        public struct TmphWhereDynamicMethod
        {
            /// <summary>
            ///     动态函数
            /// </summary>
            private readonly DynamicMethod dynamicMethod;

            /// <summary>
            /// </summary>
            private readonly ILGenerator generator;

            /// <summary>
            /// </summary>
            private bool isNextMember;

            /// <summary>
            ///     更新数据动态函数
            /// </summary>
            private TmphUpdateDynamicMethod updateDynamicMethod;

            /// <summary>
            ///     动态函数
            /// </summary>
            /// <param name="type"></param>
            public TmphWhereDynamicMethod(Type type)
            {
                dynamicMethod = new DynamicMethod("sqlColumnWhere", null,
                    new[] { typeof(TmphCharStream), type, typeof(TmphConstantConverter), typeof(string[]) }, type, true);
                generator = dynamicMethod.GetILGenerator();
                isNextMember = false;
                updateDynamicMethod = new TmphUpdateDynamicMethod(dynamicMethod, generator);
            }

            /// <summary>
            ///     添加字段
            /// </summary>
            /// <param name="field">字段信息</param>
            /// <param name="index">字段名称序号</param>
            public void Push(TmphFieldInfo field, int index)
            {
                if (isNextMember)
                {
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldstr, " and ");
                    generator.Emit(OpCodes.Call, TmphPub.CharStreamWriteNotNullMethod);
                }
                else isNextMember = true;
                updateDynamicMethod.PushOnly(field, index);
            }

            /// <summary>
            ///     创建web表单委托
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
    ///     数据列
    /// </summary>
    /// <typeparam name="TValueType">数据类型</typeparam>
    internal static class TmphSqlColumn<TValueType>
    {
        /// <summary>
        ///     数据列名与类型集合
        /// </summary>
        private static TmphInterlocked.TmphLastDictionary<TmphHashString, TmphKeyValue<string, Type>[]> dataColumns =
            new TmphInterlocked.TmphLastDictionary<TmphHashString, TmphKeyValue<string, Type>[]>(
                TmphDictionary.CreateHashString<TmphKeyValue<string, Type>[]>());

        /// <summary>
        ///     SQL列配置
        /// </summary>
        private static readonly TmphSqlColumn attribute;

        /// <summary>
        ///     自定义类型处理接口
        /// </summary>
        private static readonly TmphSqlColumn.TmphICustom<TValueType> custom;

        /// <summary>
        ///     字段集合
        /// </summary>
        private static readonly Code.CSharp.TmphSqlModel.TmphFieldInfo[] fields;

        static TmphSqlColumn()
        {
            var type = typeof(TValueType);
            if (type.IsEnum || !type.IsValueType)
            {
                TmphLog.Error.Add(type.fullName() + " 非值类型，不能用作数据列", false, false);
                return;
            }
            attribute = TmphTypeAttribute.GetAttribute<TmphSqlColumn>(type, true, true) ?? TmphSqlColumn.Default;
            foreach (var method in TmphAttributeMethod.GetStatic(type))
            {
                if (typeof(TmphSqlColumn.TmphICustom<TValueType>).IsAssignableFrom(method.Method.ReflectedType)
                    && method.Method.GetParameters().Length == 0 &&
                    method.GetAttribute<TmphSqlColumn.TmphCustom>(true) != null)
                {
                    var customValue = method.Method.Invoke(null, null);
                    if (customValue != null)
                    {
                        custom = (TmphSqlColumn.TmphICustom<TValueType>)customValue;
                        return;
                    }
                }
            }
            fields =
                Code.CSharp.TmphSqlModel.GetFields(TmphMemberIndexGroup<TValueType>.GetFields(attribute.MemberFilter))
                    .ToArray();
        }

        /// <summary>
        ///     获取成员名称与类型集合
        /// </summary>
        /// <param name="name">列名前缀</param>
        /// <returns></returns>
        internal static TmphKeyValue<string, Type>[] GetDataColumns(string name)
        {
            if (custom != null) return custom.GetDataColumns(name);
            if (fields != null)
            {
                TmphKeyValue<string, Type>[] values;
                TmphHashString nameKey = name;
                if (dataColumns.TryGetValue(nameKey, out values)) return values;
                var columns = new TmphSubArray<TmphKeyValue<string, Type>>(fields.Length);
                foreach (var field in fields)
                {
                    if (field.IsSqlColumn)
                        TmphSqlColumn.TmphToArrayDynamicMethod.GetDataColumns(field.DataType)(name + "_" + field.Field.Name);
                    else columns.Add(new TmphKeyValue<string, Type>(name + "_" + field.Field.Name, field.DataType));
                }
                values = columns.ToArray();
                dataColumns.Set(nameKey, values);
                return values;
            }
            return null;
        }

        /// <summary>
        ///     数据列验证
        /// </summary>
        internal static class TmphVerify
        {
            /// <summary>
            ///     数据列名集合
            /// </summary>
            private static TmphInterlocked.TmphLastDictionary<TmphHashString, string[]> columnNames;

            /// <summary>
            ///     字段集合
            /// </summary>
            private static readonly Code.CSharp.TmphSqlModel.TmphFieldInfo[] verifyFields;

            /// <summary>
            ///     数据验证
            /// </summary>
            private static readonly Func<TValueType, TmphSqlTable.TmphSqlTool, string[], bool> verifyer;

            static TmphVerify()
            {
                if (attribute != null && custom == null && fields != null)
                {
                    var verifyFields = fields.getFind(value => value.IsVerify);
                    if (verifyFields.Count != 0)
                    {
                        columnNames =
                            new TmphInterlocked.TmphLastDictionary<TmphHashString, string[]>(
                                TmphDictionary.CreateHashString<string[]>());
                        var index = 0;
                        TmphVerify.verifyFields = verifyFields.ToArray();
                        var dynamicMethod = new TmphSqlColumn.TmphVerifyDynamicMethod(typeof(TValueType));
                        foreach (var member in TmphVerify.verifyFields) dynamicMethod.Push(member, index++);
                        verifyer =
                            (Func<TValueType, TmphSqlTable.TmphSqlTool, string[], bool>)
                                dynamicMethod.Create<Func<TValueType, TmphSqlTable.TmphSqlTool, string[], bool>>();
                    }
                }
            }

            /// <summary>
            ///     获取列名集合
            /// </summary>
            /// <param name="name">列名前缀</param>
            /// <returns></returns>
            internal static string[] GetColumnNames(string name)
            {
                string[] names;
                TmphHashString nameKey = name;
                if (columnNames.TryGetValue(nameKey, out names)) return names;
                var nameList = new TmphList<string>(verifyFields.Length);
                foreach (var field in verifyFields)
                {
                    if (field.IsSqlColumn)
                        TmphSqlColumn.TmphVerifyDynamicMethod.GetColumnNames(field.Field.FieldType)(nameList,
                            name + "_" + field.Field.Name);
                    else nameList.Add(name + "_" + field.Field.Name);
                }
                columnNames.Set(nameKey, names = nameList.ToArray());
                return names;
            }

            /// <summary>
            ///     数据验证
            /// </summary>
            /// <param name="value"></param>
            /// <param name="sqlTool"></param>
            /// <param name="columnName"></param>
            /// <returns></returns>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public static bool Verify(TValueType value, TmphSqlTable.TmphSqlTool sqlTool, string columnName)
            {
                if (verifyer != null) return verifyer(value, sqlTool, GetColumnNames(columnName));
                return custom == null || custom.Verify(value, sqlTool, columnName);
            }
        }

        /// <summary>
        ///     数据列设置
        /// </summary>
        internal static class TmphSet
        {
            /// <summary>
            ///     默认数据列设置
            /// </summary>
            private static readonly setter defaultSetter;

            static TmphSet()
            {
                if (attribute != null && custom == null && fields != null)
                {
                    var dynamicMethod = new TmphSqlColumn.TmphSetDynamicMethod(typeof(TValueType));
                    foreach (var member in fields) dynamicMethod.Push(member);
                    defaultSetter = (setter)dynamicMethod.Create<setter>();
                }
            }

            /// <summary>
            ///     设置字段值
            /// </summary>
            /// <param name="reader">字段读取器物理存储</param>
            /// <param name="value">目标数据</param>
            /// <param name="index">当前读取位置</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public static void Set(DbDataReader reader, ref TValueType value, ref int index)
            {
                if (defaultSetter != null) defaultSetter(reader, ref value, ref index);
                else if (custom != null) custom.Set(reader, ref value, ref index);
            }

            /// <summary>
            ///     设置字段值
            /// </summary>
            /// <param name="reader">字段读取器物理存储</param>
            /// <param name="value">目标数据</param>
            /// <param name="index">当前读取位置</param>
            private delegate void setter(DbDataReader reader, ref TValueType value, ref int index);
        }

        /// <summary>
        ///     数据列转换数组
        /// </summary>
        internal static class TmphToArray
        {
            /// <summary>
            ///     数据列转换数组
            /// </summary>
            private static readonly TmphWriter defaultWriter;

            static TmphToArray()
            {
                if (attribute != null && custom == null && fields != null)
                {
                    var dynamicMethod = new TmphSqlColumn.TmphToArrayDynamicMethod(typeof(TValueType));
                    foreach (var member in fields) dynamicMethod.Push(member);
                    defaultWriter = (TmphWriter)dynamicMethod.Create<TmphWriter>();
                }
            }

            /// <summary>
            ///     数据列转换数组
            /// </summary>
            /// <param name="values">目标数组</param>
            /// <param name="value">数据列</param>
            /// <param name="index">当前读取位置</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public static void ToArray(TValueType value, object[] values, ref int index)
            {
                if (defaultWriter != null) defaultWriter(value, values, ref index);
                else if (custom != null) custom.ToArray(value, values, ref index);
            }

            /// <summary>
            ///     数据列转换数组
            /// </summary>
            /// <param name="values">目标数组</param>
            /// <param name="value">数据列</param>
            /// <param name="index">当前读取位置</param>
            private delegate void TmphWriter(TValueType value, object[] values, ref int index);
        }

        /// <summary>
        ///     数据列添加SQL流
        /// </summary>
        internal static class TmphInsert
        {
            /// <summary>
            ///     数据列名集合
            /// </summary>
            private static TmphInterlocked.TmphLastDictionary<TmphHashString, string> columnNames;

            /// <summary>
            ///     获取插入数据SQL表达式
            /// </summary>
            private static readonly Action<TmphCharStream, TValueType, TmphConstantConverter> inserter;

            static TmphInsert()
            {
                if (attribute != null && custom == null && fields != null)
                {
                    columnNames =
                        new TmphInterlocked.TmphLastDictionary<TmphHashString, string>(TmphDictionary.CreateHashString<string>());
                    var dynamicMethod = new TmphSqlColumn.TmphInsertDynamicMethod(typeof(TValueType));
                    foreach (var member in fields) dynamicMethod.Push(member);
                    inserter =
                        (Action<TmphCharStream, TValueType, TmphConstantConverter>)
                            dynamicMethod.Create<Action<TmphCharStream, TValueType, TmphConstantConverter>>();
                }
            }

            /// <summary>
            ///     获取列名集合
            /// </summary>
            /// <param name="name">列名前缀</param>
            /// <returns></returns>
            public static unsafe string GetColumnNames(string name)
            {
                if (custom != null) return custom.GetColumnNames(name);
                if (!columnNames.IsNull)
                {
                    string names;
                    TmphHashString nameKey = name;
                    if (columnNames.TryGetValue(nameKey, out names)) return names;
                    var isNext = 0;
                    var TmphBuffer = TmphClient.SqlBuffers.Get();
                    try
                    {
                        using (var sqlStream = new TmphCharStream(TmphBuffer.Char, TmphClient.SqlBufferSize))
                        {
                            foreach (var field in fields)
                            {
                                if (field.IsSqlColumn)
                                {
                                    if (
                                        (names =
                                            TmphSqlColumn.TmphInsertDynamicMethod.GetColumnNames(field.Field.FieldType)(
                                                name + "_" + field.Field.Name)) != null)
                                    {
                                        if (isNext == 0) isNext = 1;
                                        else sqlStream.Write(',');
                                        sqlStream.Write(names);
                                    }
                                }
                                else
                                {
                                    if (isNext == 0) isNext = 1;
                                    else sqlStream.Write(',');
                                    sqlStream.PrepLength(name.Length + field.Field.Name.Length + 1);
                                    sqlStream.WriteNotNull(name);
                                    sqlStream.Write('_');
                                    sqlStream.WriteNotNull(field.Field.Name);
                                }
                            }
                            names = sqlStream.Length == 0 ? null : sqlStream.ToString();
                            columnNames.Set(nameKey, names);
                        }
                    }
                    finally
                    {
                        TmphClient.SqlBuffers.Push(ref TmphBuffer);
                    }
                    return names;
                }
                return null;
            }

            /// <summary>
            ///     获取插入数据SQL表达式
            /// </summary>
            /// <param name="sqlStream">SQL表达式流</param>
            /// <param name="value">数据列</param>
            /// <param name="converter">SQL常量转换</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public static void Insert(TmphCharStream sqlStream, TValueType value, TmphConstantConverter converter)
            {
                if (inserter != null) inserter(sqlStream, value, converter);
                else if (custom != null) custom.Insert(sqlStream, value, converter);
            }
        }

        /// <summary>
        ///     数据列更新SQL流
        /// </summary>
        internal static class TmphUpdate
        {
            /// <summary>
            ///     数据列名集合
            /// </summary>
            private static TmphInterlocked.TmphLastDictionary<TmphHashString, string[]> columnNames;

            /// <summary>
            ///     获取更新数据SQL表达式
            /// </summary>
            private static readonly Action<TmphCharStream, TValueType, TmphConstantConverter, string[]> updater;

            static TmphUpdate()
            {
                if (attribute != null && custom == null && fields != null)
                {
                    columnNames =
                        new TmphInterlocked.TmphLastDictionary<TmphHashString, string[]>(
                            TmphDictionary.CreateHashString<string[]>());
                    var index = 0;
                    var dynamicMethod = new TmphSqlColumn.TmphUpdateDynamicMethod(typeof(TValueType));
                    foreach (var member in fields) dynamicMethod.Push(member, index++);
                    updater =
                        (Action<TmphCharStream, TValueType, TmphConstantConverter, string[]>)
                            dynamicMethod.Create<Action<TmphCharStream, TValueType, TmphConstantConverter, string[]>>();
                }
            }

            /// <summary>
            ///     获取列名集合
            /// </summary>
            /// <param name="name">列名前缀</param>
            /// <returns></returns>
            public static string[] GetColumnNames(string name)
            {
                string[] names;
                TmphHashString nameKey = name;
                if (columnNames.TryGetValue(nameKey, out names)) return names;
                var nameList = new TmphList<string>(fields.Length);
                foreach (var field in fields)
                {
                    if (field.IsSqlColumn)
                        TmphSqlColumn.TmphUpdateDynamicMethod.GetColumnNames(field.Field.FieldType)(nameList,
                            name + "_" + field.Field.Name);
                    else nameList.Add(name + "_" + field.Field.Name);
                }
                columnNames.Set(nameKey, names = nameList.ToArray());
                return names;
            }

            /// <summary>
            ///     获取更新数据SQL表达式
            /// </summary>
            /// <param name="sqlStream">SQL表达式流</param>
            /// <param name="value">数据列</param>
            /// <param name="converter">SQL常量转换</param>
            /// <param name="columnName">列名前缀</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public static void Update(TmphCharStream sqlStream, TValueType value, TmphConstantConverter converter,
                string columnName)
            {
                if (updater != null) updater(sqlStream, value, converter, GetColumnNames(columnName));
                else if (custom != null) custom.Update(sqlStream, value, converter, columnName);
            }
        }

        /// <summary>
        ///     条件
        /// </summary>
        internal static class TmphWhere
        {
            /// <summary>
            ///     条件SQL流
            /// </summary>
            private static readonly Action<TmphCharStream, TValueType, TmphConstantConverter, string[]> getWhere;

            static TmphWhere()
            {
                if (attribute != null && custom == null && fields != null)
                {
                    var index = 0;
                    var dynamicMethod = new TmphSqlColumn.TmphWhereDynamicMethod(typeof(TValueType));
                    foreach (var member in fields) dynamicMethod.Push(member, index++);
                    getWhere =
                        (Action<TmphCharStream, TValueType, TmphConstantConverter, string[]>)
                            dynamicMethod.Create<Action<TmphCharStream, TValueType, TmphConstantConverter, string[]>>();
                }
            }

            /// <summary>
            ///     条件SQL流
            /// </summary>
            /// <param name="sqlStream">SQL表达式流</param>
            /// <param name="value">数据列</param>
            /// <param name="converter">SQL常量转换</param>
            /// <param name="columnName">列名前缀</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public static void Where(TmphCharStream sqlStream, TValueType value, TmphConstantConverter converter,
                string columnName)
            {
                if (getWhere != null) getWhere(sqlStream, value, converter, TmphUpdate.GetColumnNames(columnName));
                else if (custom != null) custom.Where(sqlStream, value, converter, columnName);
            }
        }
    }
}