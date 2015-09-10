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

using Laurent.Lee.CLB.Code;
using Laurent.Lee.CLB.Reflection;
using Laurent.Lee.CLB.Sql;
using Laurent.Lee.CLB.Sql.Expression;
using Laurent.Lee.CLB.Threading;
using System;
using System.Data.Common;
using System.Reflection.Emit;

namespace Laurent.Lee.CLB.Emit
{
    /// <summary>
    ///     数据库表格模型
    /// </summary>
    internal static class TmphSqlModel
    {
        /// <summary>
        ///     数据列验证动态函数
        /// </summary>
        public struct TmphVerifyDynamicMethod
        {
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
                dynamicMethod = new DynamicMethod("sqlModelVerify", typeof(bool),
                    new[] { type, typeof(TmphMemberMap), typeof(TmphSqlTable.TmphSqlTool) }, type, true);
                generator = dynamicMethod.GetILGenerator();
            }

            /// <summary>
            ///     添加字段
            /// </summary>
            /// <param name="field">字段信息</param>
            public void Push(Code.CSharp.TmphSqlModel.TmphFieldInfo field)
            {
                var end = generator.DefineLabel();
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Ldc_I4, field.MemberMapIndex);
                generator.Emit(OpCodes.Callvirt, TmphPub.MemberMapIsMemberMethod);
                generator.Emit(OpCodes.Brfalse_S, end);
                if (field.IsSqlColumn)
                {
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldfld, field.Field);
                    generator.Emit(OpCodes.Ldarg_2);
                    generator.Emit(OpCodes.Ldstr, field.Field.Name);
                    generator.Emit(OpCodes.Call, TmphSqlColumn.TmphVerifyDynamicMethod.GetTypeVerifyer(field.DataType));
                    generator.Emit(OpCodes.Brtrue_S, end);
                }
                else if (field.DataType == typeof(string))
                {
                    generator.Emit(OpCodes.Ldarg_2);
                    generator.Emit(OpCodes.Ldstr, field.Field.Name);
                    generator.Emit(OpCodes.Ldarg_0);
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
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldfld, field.Field);
                    var castMethod = TmphPub.GetCastMethod(field.Field.FieldType, field.DataType);
                    if (castMethod != null) generator.Emit(OpCodes.Call, castMethod);
                    generator.Emit(OpCodes.Brtrue_S, end);
                    generator.Emit(OpCodes.Ldarg_2);
                    generator.Emit(OpCodes.Ldstr, field.Field.Name);
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
        }

        /// <summary>
        ///     数据库模型设置动态函数
        /// </summary>
        public struct TmphSetDynamicMethod
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
            private readonly LocalBuilder indexMember;

            /// <summary>
            ///     动态函数
            /// </summary>
            /// <param name="type"></param>
            public TmphSetDynamicMethod(Type type)
            {
                dynamicMethod = new DynamicMethod("sqlModelSet", null,
                    new[] { typeof(DbDataReader), type, typeof(TmphMemberMap) }, type, true);
                generator = dynamicMethod.GetILGenerator();
                indexMember = generator.DeclareLocal(typeof(int));
                generator.Emit(OpCodes.Ldc_I4_0);
                generator.Emit(OpCodes.Stloc_0);
            }

            /// <summary>
            ///     添加字段
            /// </summary>
            /// <param name="field">字段信息</param>
            public void Push(Code.CSharp.TmphSqlModel.TmphFieldInfo field)
            {
                var notMember = generator.DefineLabel();
                generator.Emit(OpCodes.Ldarg_2);
                generator.Emit(OpCodes.Ldc_I4, field.MemberMapIndex);
                generator.Emit(OpCodes.Callvirt, TmphPub.MemberMapIsMemberMethod);
                generator.Emit(OpCodes.Brfalse_S, notMember);
                if (field.DataReaderMethod == null)
                {
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldarg_1);
                    generator.Emit(OpCodes.Ldflda, field.Field);
                    generator.Emit(OpCodes.Ldloca_S, indexMember);
                    generator.Emit(OpCodes.Call, TmphSqlColumn.TmphSetDynamicMethod.GetTypeSetter(field.DataType));
                }
                else
                {
                    if (field.DataType == field.NullableDataType &&
                        (field.DataType.IsValueType || !field.DataMember.IsNull))
                    {
                        generator.Emit(OpCodes.Ldarg_1);
                        generator.Emit(OpCodes.Ldarg_0);
                        generator.Emit(OpCodes.Ldloc_0);
                        generator.Emit(OpCodes.Callvirt, field.DataReaderMethod);
                        var castMethod = TmphPub.GetCastMethod(field.DataType, field.Field.FieldType);
                        if (castMethod != null) generator.Emit(OpCodes.Call, castMethod);
                        generator.Emit(OpCodes.Stfld, field.Field);
                    }
                    else
                    {
                        Label notNull = generator.DefineLabel(), end = generator.DefineLabel();
                        generator.Emit(OpCodes.Ldarg_0);
                        generator.Emit(OpCodes.Ldloc_0);
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
                        generator.Emit(OpCodes.Ldloc_0);
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
                    generator.Emit(OpCodes.Ldloc_0);
                    generator.Emit(OpCodes.Ldc_I4_1);
                    generator.Emit(OpCodes.Add);
                    generator.Emit(OpCodes.Stloc_0);
                }
                generator.MarkLabel(notMember);
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

        /// <summary>
        ///     数据列转换数组动态函数
        /// </summary>
        public struct TmphToArrayDynamicMethod
        {
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
                dynamicMethod = new DynamicMethod("sqlModelToArray", null,
                    new[] { type, typeof(object[]), TmphPub.RefIntType }, type, true);
                generator = dynamicMethod.GetILGenerator();
            }

            /// <summary>
            ///     添加字段
            /// </summary>
            /// <param name="field">字段信息</param>
            public void Push(Code.CSharp.TmphSqlModel.TmphFieldInfo field)
            {
                if (field.IsSqlColumn)
                {
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldfld, field.Field);
                    generator.Emit(OpCodes.Ldarg_1);
                    generator.Emit(OpCodes.Ldarg_2);
                    generator.Emit(OpCodes.Call, TmphSqlColumn.TmphToArrayDynamicMethod.GetTypeToArray(field.DataType));
                }
                else
                {
                    if (field.DataType == field.NullableDataType)
                    {
                        generator.Emit(OpCodes.Ldarg_1);
                        generator.Emit(OpCodes.Ldarg_2);
                        generator.Emit(OpCodes.Ldind_I4);
                        generator.Emit(OpCodes.Ldarg_0);
                        generator.Emit(OpCodes.Ldfld, field.Field);
                        var castMethod = TmphPub.GetCastMethod(field.Field.FieldType, field.DataType);
                        if (castMethod != null) generator.Emit(OpCodes.Call, castMethod);
                        if (field.DataType.IsValueType) generator.Emit(OpCodes.Box, field.DataType);
                        generator.Emit(OpCodes.Stelem_Ref);
                    }
                    else
                    {
                        var end = generator.DefineLabel();
                        generator.Emit(OpCodes.Ldarg_0);
                        generator.Emit(OpCodes.Ldflda, field.Field);
                        generator.Emit(OpCodes.Call, TmphPub.GetNullableHasValue(field.NullableDataType));
                        generator.Emit(OpCodes.Brtrue_S, end);
                        generator.Emit(OpCodes.Ldarg_1);
                        generator.Emit(OpCodes.Ldarg_2);
                        generator.Emit(OpCodes.Ldind_I4);
                        generator.Emit(OpCodes.Ldarg_0);
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
        }

        /// <summary>
        ///     添加数据动态函数
        /// </summary>
        public struct TmphInsertDynamicMethod
        {
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
            public TmphInsertDynamicMethod(Type type)
            {
                dynamicMethod = new DynamicMethod("sqlModelInsert", null,
                    new[] { typeof(TmphCharStream), typeof(TmphMemberMap), type, typeof(TmphConstantConverter) }, type, true);
                generator = dynamicMethod.GetILGenerator();
                generator.DeclareLocal(typeof(int));
                generator.Emit(OpCodes.Ldc_I4_0);
                generator.Emit(OpCodes.Stloc_0);
            }

            /// <summary>
            ///     添加字段
            /// </summary>
            /// <param name="field">字段信息</param>
            public void Push(Code.CSharp.TmphSqlModel.TmphFieldInfo field)
            {
                Label end = generator.DefineLabel(), isNext = generator.DefineLabel(), insert = generator.DefineLabel();
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Ldc_I4, field.MemberMapIndex);
                generator.Emit(OpCodes.Callvirt, TmphPub.MemberMapIsMemberMethod);
                generator.Emit(OpCodes.Brfalse_S, end);
                generator.Emit(OpCodes.Ldloc_0);
                generator.Emit(OpCodes.Brtrue_S, isNext);
                generator.Emit(OpCodes.Ldc_I4_1);
                generator.Emit(OpCodes.Stloc_0);
                generator.Emit(OpCodes.Br_S, insert);
                generator.MarkLabel(isNext);
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldc_I4_S, (byte)',');
                generator.Emit(OpCodes.Callvirt, TmphPub.CharStreamWriteCharMethod);
                generator.MarkLabel(insert);
                if (field.IsSqlColumn)
                {
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldarg_2);
                    generator.Emit(OpCodes.Ldfld, field.Field);
                    generator.Emit(OpCodes.Ldarg_3);
                    generator.Emit(OpCodes.Call, TmphSqlColumn.TmphInsertDynamicMethod.GetTypeInsert(field.DataType));
                }
                else
                {
                    generator.Emit(OpCodes.Ldarg_3);
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldarg_2);
                    generator.Emit(OpCodes.Ldfld, field.Field);
                    var castMethod = TmphPub.GetCastMethod(field.Field.FieldType, field.DataType);
                    if (castMethod != null) generator.Emit(OpCodes.Call, castMethod);
                    generator.Emit(OpCodes.Callvirt, TmphPub.GetSqlConverterMethod(field.DataType));
                }
                generator.MarkLabel(end);
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

        /// <summary>
        ///     更新数据动态函数
        /// </summary>
        public struct TmphUpdateDynamicMethod
        {
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
            public TmphUpdateDynamicMethod(Type type)
            {
                dynamicMethod = new DynamicMethod("sqlModelUpdate", null,
                    new[] { typeof(TmphCharStream), typeof(TmphMemberMap), type, typeof(TmphConstantConverter) }, type, true);
                generator = dynamicMethod.GetILGenerator();
                generator.DeclareLocal(typeof(int));
                generator.Emit(OpCodes.Ldc_I4_0);
                generator.Emit(OpCodes.Stloc_0);
            }

            /// <summary>
            ///     添加字段
            /// </summary>
            /// <param name="field">字段信息</param>
            public void Push(Code.CSharp.TmphSqlModel.TmphFieldInfo field)
            {
                Label end = generator.DefineLabel(), isNext = generator.DefineLabel(), update = generator.DefineLabel();
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Ldc_I4, field.MemberMapIndex);
                generator.Emit(OpCodes.Callvirt, TmphPub.MemberMapIsMemberMethod);
                generator.Emit(OpCodes.Brfalse_S, end);
                generator.Emit(OpCodes.Ldloc_0);
                generator.Emit(OpCodes.Brtrue_S, isNext);
                generator.Emit(OpCodes.Ldc_I4_1);
                generator.Emit(OpCodes.Stloc_0);
                generator.Emit(OpCodes.Br_S, update);
                generator.MarkLabel(isNext);
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldc_I4_S, (byte)',');
                generator.Emit(OpCodes.Callvirt, TmphPub.CharStreamWriteCharMethod);
                generator.MarkLabel(update);
                if (field.IsSqlColumn)
                {
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldarg_2);
                    generator.Emit(OpCodes.Ldfld, field.Field);
                    generator.Emit(OpCodes.Ldarg_3);
                    generator.Emit(OpCodes.Ldstr, field.Field.Name);
                    generator.Emit(OpCodes.Call, TmphSqlColumn.TmphUpdateDynamicMethod.GetTypeUpdate(field.DataType));
                }
                else
                {
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldstr, field.Field.Name + "=");
                    generator.Emit(OpCodes.Callvirt, TmphPub.CharStreamWriteNotNullMethod);
                    generator.Emit(OpCodes.Ldarg_3);
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldarg_2);
                    generator.Emit(OpCodes.Ldfld, field.Field);
                    var castMethod = TmphPub.GetCastMethod(field.Field.FieldType, field.DataType);
                    if (castMethod != null) generator.Emit(OpCodes.Call, castMethod);
                    generator.Emit(OpCodes.Callvirt, TmphPub.GetSqlConverterMethod(field.DataType));
                }
                generator.MarkLabel(end);
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

        /// <summary>
        ///     关键字条件动态函数
        /// </summary>
        public struct TmphPrimaryKeyWhereDynamicMethod
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
            ///     动态函数
            /// </summary>
            /// <param name="type"></param>
            public TmphPrimaryKeyWhereDynamicMethod(Type type)
            {
                dynamicMethod = new DynamicMethod("sqlModelPrimaryKeyWhere", null,
                    new[] { typeof(TmphCharStream), type, typeof(TmphConstantConverter) }, type, true);
                generator = dynamicMethod.GetILGenerator();
                isNextMember = false;
            }

            /// <summary>
            ///     添加字段
            /// </summary>
            /// <param name="field">字段信息</param>
            public void Push(Code.CSharp.TmphSqlModel.TmphFieldInfo field)
            {
                if (isNextMember)
                {
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldstr, " and ");
                    generator.Emit(OpCodes.Call, TmphPub.CharStreamWriteNotNullMethod);
                }
                else isNextMember = true;
                if (field.IsSqlColumn)
                {
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldarg_1);
                    generator.Emit(OpCodes.Ldfld, field.Field);
                    generator.Emit(OpCodes.Ldarg_2);
                    generator.Emit(OpCodes.Ldstr, field.Field.Name);
                    generator.Emit(OpCodes.Call, TmphSqlColumn.TmphUpdateDynamicMethod.GetTypeUpdate(field.DataType));
                }
                else
                {
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldstr, field.Field.Name + "=");
                    generator.Emit(OpCodes.Call, TmphPub.CharStreamWriteNotNullMethod);
                    generator.Emit(OpCodes.Ldarg_2);
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldarg_1);
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
        }
    }

    /// <summary>
    ///     数据库表格模型
    /// </summary>
    /// <typeparam name="TValueType">数据类型</typeparam>
    public abstract class TmphSqlModel<TValueType> : TmphDatabaseModel<TValueType>
    {
        /// <summary>
        ///     数据库表格模型配置
        /// </summary>
        private static readonly Code.CSharp.TmphSqlModel attribute;

        /// <summary>
        ///     字段集合
        /// </summary>
        internal static readonly Code.CSharp.TmphSqlModel.TmphFieldInfo[] Fields;

        /// <summary>
        ///     自增字段
        /// </summary>
        internal static readonly Code.CSharp.TmphSqlModel.TmphFieldInfo Identity;

        /// <summary>
        ///     关键字字段集合
        /// </summary>
        internal static readonly Code.CSharp.TmphSqlModel.TmphFieldInfo[] PrimaryKeys;

        /// <summary>
        ///     SQL数据成员
        /// </summary>
        internal static readonly TmphMemberMap MemberMap;

        /// <summary>
        ///     分组数据成员位图
        /// </summary>
        private static TmphKeyValue<TmphMemberMap, int>[] groupMemberMaps;

        /// <summary>
        ///     分组数据成员位图访问锁
        /// </summary>
        private static int groupMemberMapLock;

        /// <summary>
        ///     自增标识获取器
        /// </summary>
        public static readonly Func<TValueType, long> GetIdentity;

        /// <summary>
        ///     自增标识获取器
        /// </summary>
        public static readonly Func<TValueType, int> GetIdentity32;

        /// <summary>
        ///     设置自增标识
        /// </summary>
        internal static readonly Action<TValueType, long> SetIdentity;

        static TmphSqlModel()
        {
            var type = typeof(TValueType);
            attribute = TmphTypeAttribute.GetAttribute<Code.CSharp.TmphSqlModel>(type, true, true) ??
                        Code.CSharp.TmphSqlModel.Default;
            Fields =
                Code.CSharp.TmphSqlModel.GetFields(TmphMemberIndexGroup<TValueType>.GetFields(attribute.MemberFilter))
                    .ToArray();
            Identity = Code.CSharp.TmphSqlModel.GetIdentity(Fields);
            PrimaryKeys = Code.CSharp.TmphSqlModel.GetPrimaryKeys(Fields).ToArray();
            MemberMap = TmphMemberMap<TValueType>.New();
            foreach (var field in Fields) MemberMap.SetMember(field.MemberMapIndex);
            if (Identity != null)
            {
                var dynamicMethod = new DynamicMethod("GetSqlIdentity", typeof(long), new[] { type }, type, true);
                var generator = dynamicMethod.GetILGenerator();
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldfld, Identity.Field);
                if (Identity.Field.FieldType != typeof(long) && Identity.Field.FieldType != typeof(ulong))
                    generator.Emit(OpCodes.Conv_I8);
                generator.Emit(OpCodes.Ret);
                GetIdentity = (Func<TValueType, long>)dynamicMethod.CreateDelegate(typeof(Func<TValueType, long>));

                dynamicMethod = new DynamicMethod("SetSqlIdentity", null, new[] { type, typeof(long) }, type, true);
                generator = dynamicMethod.GetILGenerator();
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldarg_1);
                if (Identity.Field.FieldType != typeof(long) && Identity.Field.FieldType != typeof(ulong))
                    generator.Emit(OpCodes.Conv_I4);
                generator.Emit(OpCodes.Stfld, Identity.Field);
                generator.Emit(OpCodes.Ret);
                SetIdentity = (Action<TValueType, long>)dynamicMethod.CreateDelegate(typeof(Action<TValueType, long>));

                GetIdentity32 = getIdentityGetter32("GetSqlIdentity32", Identity.Field);
            }
        }

        /// <summary>
        ///     SQL数据成员
        /// </summary>
        public static TmphMemberMap CopyMemberMap
        {
            get { return MemberMap.Copy(); }
        }

        /// <summary>
        ///     获取以逗号分割的名称集合
        /// </summary>
        /// <param name="sqlStream"></param>
        /// <param name="memberMap"></param>
        internal static void GetNames(TmphCharStream sqlStream, TmphMemberMap memberMap)
        {
            var isNext = 0;
            foreach (var field in Fields)
            {
                if (memberMap.IsMember(field.MemberMapIndex))
                {
                    if (isNext == 0) isNext = 1;
                    else sqlStream.Write(',');
                    if (field.IsSqlColumn) sqlStream.WriteNotNull(field.GetSqlColumnName());
                    else sqlStream.WriteNotNull(field.Field.Name);
                }
            }
        }

        /// <summary>
        ///     获取表格信息
        /// </summary>
        /// <param name="type">SQL绑定类型</param>
        /// <param name="sqlTable">SQL表格信息</param>
        /// <returns>表格信息</returns>
        internal static TmphTable GetTable(Type type, TmphSqlTable sqlTable)
        {
            var TmphClient = TmphConnection.GetConnection(sqlTable.ConnectionType).Client;
            var table = new TmphTable { Columns = new THColumnCollection { Name = sqlTable.GetTableName(type) } };
            var columns = new TmphColumn[Fields.Length];
            var primaryKeyColumns = new TmphColumn[PrimaryKeys.Length];
            int index = 0, primaryKeyIndex = 0;
            foreach (var member in Fields)
            {
                var column = TmphClient.GetColumn(member.Field.Name, member.Field.FieldType, member.DataMember);
                columns[index++] = column;
                if (Identity == member) table.Identity = column;
                if (member.DataMember.PrimaryKeyIndex != 0) primaryKeyColumns[primaryKeyIndex++] = column;
            }
            table.Columns.Columns = columns;
            if (primaryKeyColumns.Length != 0)
            {
                table.PrimaryKey = new THColumnCollection
                {
                    Columns =
                        PrimaryKeys.getArray(
                            value => primaryKeyColumns.firstOrDefault(column => column.Name == value.Field.Name))
                };
            }
            table.Columns.Name = sqlTable.GetTableName(type);
            return table;
        }

        /// <summary>
        ///     获取分组数据成员位图
        /// </summary>
        /// <param name="group">分组</param>
        /// <returns>分组数据成员位图</returns>
        private static TmphMemberMap getGroupMemberMap(int group)
        {
            if (groupMemberMaps == null)
            {
                var memberMaps = new TmphSubArray<TmphKeyValue<TmphMemberMap, int>>();
                memberMaps.Add(new TmphKeyValue<TmphMemberMap, int>(MemberMap, 0));
                TmphInterlocked.NoCheckCompareSetSleep0(ref groupMemberMapLock);
                if (groupMemberMaps == null)
                {
                    try
                    {
                        foreach (var field in Fields)
                        {
                            if (field.DataMember.Group != 0)
                            {
                                var index = memberMaps.Count;
                                foreach (var memberMap in memberMaps.array)
                                {
                                    if (memberMap.Value == field.DataMember.Group || --index == 0) break;
                                }
                                if (index == 0)
                                {
                                    var memberMap = TmphMemberMap<TValueType>.New();
                                    memberMaps.Add(new TmphKeyValue<TmphMemberMap, int>(memberMap, field.DataMember.Group));
                                    memberMap.SetMember(field.MemberMapIndex);
                                }
                                else memberMaps.array[memberMaps.Count - index].Key.SetMember(field.MemberMapIndex);
                            }
                        }
                        if (memberMaps.Count != 1)
                        {
                            var memberMap = memberMaps.array[0].Key = TmphMemberMap<TValueType>.New();
                            foreach (var field in Fields)
                            {
                                if (field.DataMember.Group == 0) memberMap.SetMember(field.MemberMapIndex);
                            }
                        }
                        groupMemberMaps = memberMaps.ToArray();
                    }
                    finally
                    {
                        groupMemberMapLock = 0;
                    }
                }
                else groupMemberMapLock = 0;
            }
            foreach (var memberMap in groupMemberMaps)
            {
                if (memberMap.Value == group) return memberMap.Key;
            }
            TmphLog.Error.Add(typeof(TValueType).fullName() + " 缺少缓存分组 " + group.toString(), false, false);
            return null;
        }

        /// <summary>
        ///     获取分组数据成员位图
        /// </summary>
        /// <param name="group">分组</param>
        /// <returns>分组数据成员位图</returns>
        public static TmphMemberMap GetCacheMemberMap(int group)
        {
            var memberMap = getGroupMemberMap(group);
            if (memberMap != null)
            {
                memberMap = memberMap.Copy();
                if (Identity != null) memberMap.SetMember(Identity.MemberMapIndex);
                else if (PrimaryKeys.Length != 0)
                {
                    foreach (var field in PrimaryKeys) memberMap.SetMember(field.MemberMapIndex);
                }
                return memberMap;
            }
            return null;
        }

        /// <summary>
        ///     获取自增标识获取器
        /// </summary>
        /// <param name="baseIdentity"></param>
        /// <returns></returns>
        public static Func<TValueType, int> IdentityGetter(int baseIdentity)
        {
            if (baseIdentity == 0) return GetIdentity32;
            var dynamicMethod = new DynamicMethod("GetIdentity32_" + baseIdentity.toString(), typeof(int),
                new[] { typeof(TValueType) }, typeof(TValueType), true);
            var generator = dynamicMethod.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, Identity.Field);
            if (Identity.Field.FieldType != typeof(int) && Identity.Field.FieldType != typeof(uint))
                generator.Emit(OpCodes.Conv_I4);
            generator.Emit(OpCodes.Ldc_I4, baseIdentity);
            generator.Emit(OpCodes.Sub);
            generator.Emit(OpCodes.Ret);
            return (Func<TValueType, int>)dynamicMethod.CreateDelegate(typeof(Func<TValueType, int>));
        }

        /// <summary>
        ///     数据列验证
        /// </summary>
        internal static class TmphVerify
        {
            /// <summary>
            ///     数据验证
            /// </summary>
            private static readonly Func<TValueType, TmphMemberMap, TmphSqlTable.TmphSqlTool, bool> verifyer;

            static TmphVerify()
            {
                if (attribute != null)
                {
                    var verifyFields = Fields.getFind(value => value.IsVerify);
                    if (verifyFields.Count != 0)
                    {
                        var dynamicMethod = new TmphSqlModel.TmphVerifyDynamicMethod(typeof(TValueType));
                        foreach (var member in verifyFields) dynamicMethod.Push(member);
                        verifyer =
                            (Func<TValueType, TmphMemberMap, TmphSqlTable.TmphSqlTool, bool>)
                                dynamicMethod.Create<Func<TValueType, TmphMemberMap, TmphSqlTable.TmphSqlTool, bool>>();
                    }
                }
            }

            /// <summary>
            ///     数据验证
            /// </summary>
            /// <param name="value"></param>
            /// <param name="memberMap"></param>
            /// <param name="sqlTool"></param>
            /// <returns></returns>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public static bool Verify(TValueType value, TmphMemberMap memberMap, TmphSqlTable.TmphSqlTool sqlTool)
            {
                return verifyer == null || verifyer(value, memberMap, sqlTool);
            }
        }

        /// <summary>
        ///     数据库模型设置
        /// </summary>
        internal static class TmphSet
        {
            /// <summary>
            ///     默认数据列设置
            /// </summary>
            private static readonly Action<DbDataReader, TValueType, TmphMemberMap> setter;

            static TmphSet()
            {
                if (attribute != null)
                {
                    var dynamicMethod = new TmphSqlModel.TmphSetDynamicMethod(typeof(TValueType));
                    foreach (var member in Fields) dynamicMethod.Push(member);
                    setter =
                        (Action<DbDataReader, TValueType, TmphMemberMap>)
                            dynamicMethod.Create<Action<DbDataReader, TValueType, TmphMemberMap>>();
                }
            }

            /// <summary>
            ///     设置字段值
            /// </summary>
            /// <param name="reader">字段读取器物理存储</param>
            /// <param name="value">目标数据</param>
            /// <param name="memberMap">成员位图</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public static void Set(DbDataReader reader, TValueType value, TmphMemberMap memberMap)
            {
                if (setter != null) setter(reader, value, memberMap);
            }
        }

        /// <summary>
        ///     数据列转换数组
        /// </summary>
        internal static class TmphToArray
        {
            /// <summary>
            ///     导入数据列集合
            /// </summary>
            private static TmphKeyValue<string, Type>[] dataColumns;

            /// <summary>
            ///     数据列转换数组
            /// </summary>
            private static readonly writer defaultWriter;

            static TmphToArray()
            {
                if (attribute != null)
                {
                    var dynamicMethod = new TmphSqlModel.TmphToArrayDynamicMethod(typeof(TValueType));
                    foreach (var member in Fields) dynamicMethod.Push(member);
                    defaultWriter = (writer)dynamicMethod.Create<writer>();
                }
            }

            /// <summary>
            ///     导入数据列集合
            /// </summary>
            internal static TmphKeyValue<string, Type>[] DataColumns
            {
                get
                {
                    if (dataColumns == null)
                    {
                        var columns = new TmphSubArray<TmphKeyValue<string, Type>>(Fields.Length);
                        foreach (var field in Fields)
                        {
                            if (field.IsSqlColumn)
                                columns.Add(
                                    TmphSqlColumn.TmphToArrayDynamicMethod.GetDataColumns(field.DataType)(field.Field.Name));
                            else columns.Add(new TmphKeyValue<string, Type>(field.Field.Name, field.DataType));
                        }
                        dataColumns = columns.ToArray();
                    }
                    return dataColumns;
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
            }

            /// <summary>
            ///     数据列转换数组
            /// </summary>
            /// <param name="values">目标数组</param>
            /// <param name="value">数据列</param>
            /// <param name="index">当前读取位置</param>
            private delegate void writer(TValueType value, object[] values, ref int index);
        }

        /// <summary>
        ///     添加数据
        /// </summary>
        internal static class TmphInsert
        {
            /// <summary>
            ///     获取插入数据SQL表达式
            /// </summary>
            private static readonly Action<TmphCharStream, TmphMemberMap, TValueType, TmphConstantConverter> inserter;

            static TmphInsert()
            {
                if (attribute != null)
                {
                    var dynamicMethod = new TmphSqlModel.TmphInsertDynamicMethod(typeof(TValueType));
                    foreach (var member in Fields) dynamicMethod.Push(member);
                    inserter =
                        (Action<TmphCharStream, TmphMemberMap, TValueType, TmphConstantConverter>)
                            dynamicMethod.Create<Action<TmphCharStream, TmphMemberMap, TValueType, TmphConstantConverter>>();
                }
            }

            /// <summary>
            ///     获取逗号分割的列名集合
            /// </summary>
            /// <param name="sqlStream"></param>
            /// <param name="memberMap"></param>
            public static void GetColumnNames(TmphCharStream sqlStream, TmphMemberMap memberMap)
            {
                var isNext = 0;
                foreach (var member in Fields)
                {
                    if (memberMap.IsMember(member.MemberMapIndex) || member == Identity ||
                        member.DataMember.PrimaryKeyIndex != 0)
                    {
                        if (isNext == 0) isNext = 1;
                        else sqlStream.Write(',');
                        if (member.IsSqlColumn)
                            sqlStream.WriteNotNull(
                                TmphSqlColumn.TmphInsertDynamicMethod.GetColumnNames(member.Field.FieldType)(
                                    member.Field.Name));
                        else sqlStream.WriteNotNull(member.Field.Name);
                    }
                }
            }

            /// <summary>
            ///     获取插入数据SQL表达式
            /// </summary>
            /// <param name="sqlStream">SQL表达式流</param>
            /// <param name="memberMap">成员位图</param>
            /// <param name="value">数据</param>
            /// <param name="converter">SQL常量转换</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public static void Insert(TmphCharStream sqlStream, TmphMemberMap memberMap, TValueType value,
                TmphConstantConverter converter)
            {
                if (inserter != null) inserter(sqlStream, memberMap, value, converter);
            }
        }

        /// <summary>
        ///     数据列更新SQL流
        /// </summary>
        internal static class TmphUpdate
        {
            /// <summary>
            ///     获取更新数据SQL表达式
            /// </summary>
            private static readonly Action<TmphCharStream, TmphMemberMap, TValueType, TmphConstantConverter> updater;

            static TmphUpdate()
            {
                if (attribute != null)
                {
                    var dynamicMethod = new TmphSqlModel.TmphUpdateDynamicMethod(typeof(TValueType));
                    foreach (var member in Fields) dynamicMethod.Push(member);
                    updater =
                        (Action<TmphCharStream, TmphMemberMap, TValueType, TmphConstantConverter>)
                            dynamicMethod.Create<Action<TmphCharStream, TmphMemberMap, TValueType, TmphConstantConverter>>();
                }
            }

            /// <summary>
            ///     获取更新数据SQL表达式
            /// </summary>
            /// <param name="sqlStream">SQL表达式流</param>
            /// <param name="memberMap">更新成员位图</param>
            /// <param name="value">数据</param>
            /// <param name="converter">SQL常量转换</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public static void Update(TmphCharStream sqlStream, TmphMemberMap memberMap, TValueType value,
                TmphConstantConverter converter)
            {
                if (updater != null) updater(sqlStream, memberMap, value, converter);
            }
        }

        /// <summary>
        ///     关键字条件
        /// </summary>
        internal static class TmphPrimaryKeyWhere
        {
            /// <summary>
            ///     关键字条件SQL流
            /// </summary>
            private static readonly Action<TmphCharStream, TValueType, TmphConstantConverter> where;

            static TmphPrimaryKeyWhere()
            {
                if (attribute != null && PrimaryKeys.Length != 0)
                {
                    var dynamicMethod = new TmphSqlModel.TmphPrimaryKeyWhereDynamicMethod(typeof(TValueType));
                    foreach (var member in PrimaryKeys) dynamicMethod.Push(member);
                    where =
                        (Action<TmphCharStream, TValueType, TmphConstantConverter>)
                            dynamicMethod.Create<Action<TmphCharStream, TValueType, TmphConstantConverter>>();
                }
            }

            /// <summary>
            ///     关键字条件SQL流
            /// </summary>
            /// <param name="sqlStream">SQL表达式流</param>
            /// <param name="value">数据列</param>
            /// <param name="converter">SQL常量转换</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public static void Where(TmphCharStream sqlStream, TValueType value, TmphConstantConverter converter)
            {
                if (where != null) where(sqlStream, value, converter);
            }
        }
    }
}