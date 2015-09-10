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

using Laurent.Lee.CLB.Config;
using Laurent.Lee.CLB.Emit;
using Laurent.Lee.CLB.Reflection;
using Laurent.Lee.CLB.Threading;
using System;
using System.Reflection;

namespace Laurent.Lee.CLB.Code.CSharp
{
    /// <summary>
    ///     数据库表格模型配置
    /// </summary>
    public class TmphSqlModel : TmphDataModel
    {
        /// <summary>
        ///     空自增列成员索引
        /// </summary>
        internal const int NullIdentityMemberIndex = -1;

        /// <summary>
        ///     默认空属性
        /// </summary>
        internal static readonly TmphSqlModel Default = new TmphSqlModel();

        /// <summary>
        ///     获取字段成员集合
        /// </summary>
        /// <returns>字段成员集合</returns>
        internal static TmphSubArray<TmphFieldInfo> GetFields(TmphFieldIndex[] fields)
        {
            var values = new TmphSubArray<TmphFieldInfo>(fields.Length);
            foreach (var field in fields)
            {
                var type = field.Member.FieldType;
                if (!type.IsPointer && (!type.IsArray || type.GetArrayRank() == 1) && !field.IsIgnore)
                {
                    var attribute = field.GetAttribute<TmphDataMember>(true, true);
                    if (attribute == null || attribute.IsSetup)
                    {
                        var fieldInfo = new TmphFieldInfo(field, attribute);
                        if (fieldInfo.IsField) values.Add(fieldInfo);
                    }
                }
            }
            return values;
        }

        /// <summary>
        ///     获取关键字集合
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        internal static TmphSubArray<TmphFieldInfo> GetPrimaryKeys(TmphFieldInfo[] fields)
        {
            return fields.getFind(value => value.DataMember.PrimaryKeyIndex != 0)
                .Sort((left, right) =>
                {
                    var value = left.DataMember.PrimaryKeyIndex - right.DataMember.PrimaryKeyIndex;
                    return value == 0
                        ? string.Compare(left.Field.Name, right.Field.Name, StringComparison.Ordinal)
                        : value;
                });
        }

        /// <summary>
        ///     获取自增标识
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public static TmphFieldInfo GetIdentity(TmphFieldInfo[] fields)
        {
            TmphFieldInfo identity = null;
            var isCase = 0;
            foreach (var field in fields)
            {
                if (field.DataMember.IsIdentity) return field;
                if (isCase == 0 && field.Field.Name == TmphSql.Default.DefaultIdentityName)
                {
                    identity = field;
                    isCase = 1;
                }
                else if (identity == null && field.Field.Name.ToLower() == TmphSql.Default.DefaultIdentityName)
                    identity = field;
            }
            return identity;
        }

        /// <summary>
        ///     获取数据库成员信息集合
        /// </summary>
        /// <param name="type">数据库绑定类型</param>
        /// <param name="database">数据库配置</param>
        /// <returns>数据库成员信息集合</returns>
        internal static TmphKeyValue<TmphMemberIndex, TmphDataMember>[] GetMemberIndexs<TAttributeType>(Type type,
            TAttributeType database)
            where TAttributeType : TmphMemberFilter
        {
            //showjim
            return GetMembers(TmphMemberIndexGroup.Get(type).Find<TmphDataMember>(database));
        }

        /// <summary>
        ///     获取数据库成员信息集合
        /// </summary>
        /// <typeparam name="TMemberType">成员类型</typeparam>
        /// <param name="members">成员集合</param>
        /// <returns>数据库成员信息集合</returns>
        public static TmphKeyValue<TMemberType, TmphDataMember>[] GetMembers<TMemberType>(TMemberType[] members)
            where TMemberType : TmphMemberIndex
        {
            return members.getFind(value => value.CanSet && value.CanGet)
                .GetArray(value => new TmphKeyValue<TMemberType, TmphDataMember>(value, TmphDataMember.Get(value)));
        }

        /// <summary>
        ///     字段信息
        /// </summary>
        public class TmphFieldInfo
        {
            /// <summary>
            ///     数据列验证类型集合
            /// </summary>
            private static readonly TmphInterlocked.TmphDictionary<Type, bool> VerifyTypes =
                new TmphInterlocked.TmphDictionary<Type, bool>(TmphDictionary.CreateOnly<Type, bool>());

            /// <summary>
            ///     数据列类型集合
            /// </summary>
            private static readonly TmphInterlocked.TmphDictionary<Type, bool> SqlColumnTypes =
                new TmphInterlocked.TmphDictionary<Type, bool>(TmphDictionary.CreateOnly<Type, bool>());

            /// <summary>
            ///     数据读取函数
            /// </summary>
            internal MethodInfo DataReaderMethod;

            /// <summary>
            ///     数据库数据类型
            /// </summary>
            internal Type DataType;

            /// <summary>
            ///     获取数据列名称
            /// </summary>
            internal Func<string, string> GetSqlColumnsName;

            /// <summary>
            ///     是否有效字段
            /// </summary>
            internal bool IsField;

            /// <summary>
            ///     是否数据列
            /// </summary>
            internal bool IsSqlColumn;

            /// <summary>
            ///     成员位图索引
            /// </summary>
            internal int MemberMapIndex;

            /// <summary>
            ///     可空类型数据库数据类型
            /// </summary>
            internal Type NullableDataType;

            /// <summary>
            ///     字段信息
            /// </summary>
            /// <param name="field">字段信息</param>
            /// <param name="attribute">数据库成员信息</param>
            internal TmphFieldInfo(TmphFieldIndex field, TmphDataMember attribute)
            {
                Field = field.Member;
                MemberMapIndex = field.MemberIndex;
                DataMember = TmphDataMember.FormatSql(attribute, Field.FieldType, ref IsSqlColumn);
                if ((NullableDataType = DataMember.DataType) == null) NullableDataType = Field.FieldType;
                if (
                    (DataReaderMethod =
                        Emit.TmphPub.GetDataReaderMethod(DataType = NullableDataType.nullableType() ?? NullableDataType)) ==
                    null)
                {
                    if (IsSqlColumn && IsSqlColumnTrue(DataType)) IsField = true;
                }
                else IsField = true;
            }

            /// <summary>
            ///     字段信息
            /// </summary>
            public FieldInfo Field { get; private set; }

            /// <summary>
            ///     数据库成员信息
            /// </summary>
            public TmphDataMember DataMember { get; private set; }

            /// <summary>
            ///     是否需要验证
            /// </summary>
            internal bool IsVerify
            {
                get
                {
                    if (IsSqlColumn)
                    {
                        bool isVerify;
                        if (VerifyTypes.TryGetValue(DataType, out isVerify)) return isVerify;
                        var fieldInfo = typeof(TmphSqlColumn<>).MakeGenericType(DataType)
                            .GetField("custom", BindingFlags.Static | BindingFlags.NonPublic);
                        if (
                            fieldInfo != null)
                        {
                            var field = typeof(TmphSqlColumn<>.TmphVerify).MakeGenericType(DataType)
                                .GetField("verifyer", BindingFlags.Static | BindingFlags.NonPublic);
                            if (field != null)
                                isVerify =
                                    field
                                        .GetValue(null) != null
                                    || fieldInfo.GetValue(null) != null;
                        }
                        VerifyTypes.Set(DataType, isVerify);
                        return isVerify;
                    }
                    if (!DataMember.IsDefaultMember)
                    {
                        if (DataType == typeof(string)) return DataMember.MaxStringLength > 0;
                        return DataType.IsClass && !DataMember.IsNull;
                    }
                    return false;
                }
            }

            /// <summary>
            ///     获取数据列名称
            /// </summary>
            /// <returns></returns>
            public string GetSqlColumnName()
            {
                if (GetSqlColumnsName == null)
                    GetSqlColumnsName = TmphSqlColumn.TmphInsertDynamicMethod.GetColumnNames(Field.FieldType);
                return GetSqlColumnsName(Field.Name);
            }

            /// <summary>
            ///     是否有效数据列
            /// </summary>
            /// <param name="type"></param>
            /// <returns></returns>
            private static bool IsSqlColumnTrue(Type type)
            {
                bool isType;
                if (SqlColumnTypes.TryGetValue(type, out isType)) return isType;
                var fieldInfo = typeof(TmphSqlColumn<>).MakeGenericType(type)
                    .GetField("custom", BindingFlags.Static | BindingFlags.NonPublic);
                if (
                    fieldInfo != null)
                {
                    var field = typeof(TmphSqlColumn<>.TmphSet).MakeGenericType(type)
                        .GetField("defaultSetter", BindingFlags.Static | BindingFlags.NonPublic);
                    if (field != null)
                        isType =
                            field
                                .GetValue(null) != null
                            || fieldInfo.GetValue(null) != null;
                }
                SqlColumnTypes.Set(type, isType);
                return isType;
            }
        }
    }
}