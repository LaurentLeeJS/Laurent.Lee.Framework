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
using Laurent.Lee.CLB.IO;
using Laurent.Lee.CLB.Reflection;
using Laurent.Lee.CLB.Sql;
using System;

namespace Laurent.Lee.CLB.Emit
{
    /// <summary>
    ///     数据库成员信息
    /// </summary>
    public sealed class TmphDataMember : TmphIgnoreMember
    {
        /// <summary>
        ///     数据库成员信息空值
        /// </summary>
        internal static readonly TmphDataMember NullDataMember = new TmphDataMember();

        /// <summary>
        ///     数据库成员类型
        /// </summary>
        private TmphMemberType dataMemberType;

        /// <summary>
        ///     数据库类型
        /// </summary>
        public Type DataType;

        /// <summary>
        ///     默认值
        /// </summary>
        public string DefaultValue;

        /// <summary>
        ///     分组标识
        /// </summary>
        public int Group;

        /// <summary>
        ///     字符串是否ASCII
        /// </summary>
        public bool IsAscii;

        /// <summary>
        ///     是否固定长度
        /// </summary>
        public bool IsFixedLength;

        /// <summary>
        ///     是否自增
        /// </summary>
        public bool IsIdentity;

        /// <summary>
        ///     是否允许空值
        /// </summary>
        public bool IsNull;

        ///// <summary>
        ///// 正则验证,不可用
        ///// </summary>
        //public string RegularVerify;
        /// <summary>
        ///     字符串最大长度验证
        /// </summary>
        public int MaxStringLength;

        /// <summary>
        ///     主键索引,0标识非主键
        /// </summary>
        public int PrimaryKeyIndex;

        /// <summary>
        ///     新增字段时的计算子查询
        /// </summary>
        public string UpdateValue;

        /// <summary>
        ///     数据库成员类型
        /// </summary>
        public TmphMemberType DataMemberType
        {
            get
            {
                if (DataType == null) return null;
                if (dataMemberType == null) dataMemberType = DataType;
                return dataMemberType;
            }
        }

        /// <summary>
        ///     枚举真实类型
        /// </summary>
        public TmphMemberType TEnumType { get; private set; }

        /// <summary>
        ///     是否数据库成员信息空值
        /// </summary>
        internal bool IsDefaultMember
        {
            get { return this == NullDataMember; }
        }

        /// <summary>
        ///     格式化数据库成员信息
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static TmphDataMember FormatSql(TmphDataMember value, Type type, ref bool isSqlColumn)
        {
            if (type.IsEnum)
            {
                var TEnumType = Enum.GetUnderlyingType(type);
                if (value == null) return new TmphDataMember { DataType = Enum.GetUnderlyingType(type) };
                if (value.DataType == null) value.DataType = Enum.GetUnderlyingType(type);
                else if (TEnumType != value.DataType) value.TEnumType = TEnumType;
                return value;
            }
            Type nullableType = null;
            if (type.IsGenericType)
            {
                var genericType = type.GetGenericTypeDefinition();
                if (genericType == typeof(TmphJsonMember<>))
                {
                    if (value == null) return new TmphDataMember { DataType = typeof(string) };
                    value.DataType = typeof(string);
                    return value;
                }
                if (genericType == typeof(TmphFileBlockMember<>))
                {
                    if (value == null) return new TmphDataMember { DataType = typeof(TmphFileBlockStream.TmphIndex) };
                    value.DataType = typeof(TmphFileBlockStream.TmphIndex);
                    return value;
                }
                if (genericType == typeof(Nullable<>)) nullableType = type.GetGenericArguments()[0];
            }
            else if (TmphTypeAttribute.GetAttribute<TmphSqlColumn>(type, false, false) != null)
            {
                isSqlColumn = true;
                return NullDataMember;
            }
            if (value == null || value.DataType == null)
            {
                var sqlMember = TmphTypeAttribute.GetAttribute<TmphDataMember>(type, false, false);
                if (sqlMember != null && sqlMember.DataType != null)
                {
                    if (value == null) value = new TmphDataMember();
                    value.DataType = sqlMember.DataType;
                    if (sqlMember.DataType.IsValueType && sqlMember.DataType.IsGenericType &&
                        sqlMember.DataType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        value.IsNull = true;
                }
            }
            if (value == null)
            {
                if (nullableType == null)
                {
                    var dataType = type.formCSharpType().toCSharpType();
                    if (dataType != type) value = new TmphDataMember { DataType = dataType };
                }
                else
                {
                    value = new TmphDataMember { IsNull = true };
                    var dataType = nullableType.formCSharpType().toCSharpType();
                    if (dataType != nullableType) value.DataType = dataType.toNullableType();
                }
            }
            return value ?? NullDataMember;
        }

        /// <summary>
        ///     获取数据库成员信息
        /// </summary>
        /// <param name="member">成员信息</param>
        /// <returns>数据库成员信息</returns>
        internal static TmphDataMember Get(TmphMemberIndex member)
        {
            var value = member.GetAttribute<TmphDataMember>(true, false);
            if (value == null || value.DataType == null)
            {
                if (member.Type.IsEnum)
                {
                    if (value == null) value = new TmphDataMember();
                    value.DataType = Enum.GetUnderlyingType(member.Type);
                }
                else
                {
                    var sqlMember = TmphTypeAttribute.GetAttribute<TmphDataMember>(member.Type, false, false);
                    if (sqlMember != null && sqlMember.DataType != null)
                    {
                        if (value == null) value = new TmphDataMember();
                        value.DataType = sqlMember.DataType;
                        if (sqlMember.DataType.nullableType() != null) value.IsNull = true;
                    }
                }
            }
            else if (member.Type.IsEnum)
            {
                var TEnumType = Enum.GetUnderlyingType(member.Type);
                if (TEnumType != value.DataType) value.TEnumType = TEnumType;
            }
            if (value == null)
            {
                var nullableType = member.Type.nullableType();
                if (nullableType == null)
                {
                    if (TmphTypeAttribute.GetAttribute<TmphSqlColumn>(member.Type, false, false) == null)
                    {
                        var dataType = member.Type.formCSharpType().toCSharpType();
                        if (dataType != member.Type)
                        {
                            value = new TmphDataMember();
                            value.DataType = dataType;
                        }
                    }
                }
                else
                {
                    value = new TmphDataMember();
                    value.IsNull = true;
                    var dataType = nullableType.formCSharpType().toCSharpType();
                    if (dataType != nullableType) value.DataType = dataType.toNullableType();
                }
            }
            return value ?? NullDataMember;
        }
    }
}