﻿/*
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
using System.Data;

namespace Laurent.Lee.CLB.Sql.MySql
{
    /// <summary>
    ///     SQL数据类型相关操作
    /// </summary>
    internal static class TmphSqlDbType
    {
        /// <summary>
        ///     数据类型集合
        /// </summary>
        private static readonly string[] sqlTypeNames;

        /// <summary>
        ///     默认值集合
        /// </summary>
        private static readonly string[] defaultValues;

        /// <summary>
        ///     数据类型集合唯一哈希
        /// </summary>
        private static readonly TmphUniqueDictionary<TmphSqlTypeName, TmphKeyValue<SqlDbType, int>> sqlTypes;

        static unsafe TmphSqlDbType()
        {
            #region 数据类型集合

            sqlTypeNames = new string[TmphEnum.GetMaxValue<SqlDbType>(-1) + 1];
            sqlTypeNames[(int)SqlDbType.BigInt] = "BIGINT";
            //SqlTypeNames[(int)SqlDbType.Binary] = typeof(byte[]);
            sqlTypeNames[(int)SqlDbType.Bit] = "BIT";
            sqlTypeNames[(int)SqlDbType.Char] = "CHAR";
            sqlTypeNames[(int)SqlDbType.DateTime] = "DATETIME";
            sqlTypeNames[(int)SqlDbType.Decimal] = "DECIMAL";
            sqlTypeNames[(int)SqlDbType.Float] = "DOUBLE";
            //SqlTypeNames[(int)SqlDbType.Image] = typeof(byte[]);
            sqlTypeNames[(int)SqlDbType.Int] = "INT";
            sqlTypeNames[(int)SqlDbType.Money] = "DECIMAL";
            sqlTypeNames[(int)SqlDbType.NChar] = "CHAR";
            sqlTypeNames[(int)SqlDbType.NText] = "TEXT";
            sqlTypeNames[(int)SqlDbType.NVarChar] = "VARCHAR";
            sqlTypeNames[(int)SqlDbType.Real] = "FLOAT";
            //SqlTypeNames[(int)SqlDbType.UniqueIdentifier] = typeof(Guid);
            sqlTypeNames[(int)SqlDbType.SmallDateTime] = "DATETIME";
            sqlTypeNames[(int)SqlDbType.SmallInt] = "SMALLINT";
            sqlTypeNames[(int)SqlDbType.SmallMoney] = "DECIMAL";
            sqlTypeNames[(int)SqlDbType.Text] = "TEXT";
            //SqlTypeNames[(int)SqlDbType.Timestamp] = typeof(byte[]);
            sqlTypeNames[(int)SqlDbType.TinyInt] = "TINYINT UNSIGNED";
            //SqlTypeNames[(int)SqlDbType.VarBinary] = typeof(byte[]);
            sqlTypeNames[(int)SqlDbType.VarChar] = "VARCHAR";
            //SqlTypeNames[(int)SqlDbType.Variant] = typeof(object);

            #endregion 数据类型集合

            #region 默认值集合

            defaultValues = new string[TmphEnum.GetMaxValue<SqlDbType>(0) + 1];
            defaultValues[(int)SqlDbType.BigInt] = "0";
            defaultValues[(int)SqlDbType.Bit] = "0";
            defaultValues[(int)SqlDbType.Char] = "''";
            defaultValues[(int)SqlDbType.DateTime] = "now()";
            defaultValues[(int)SqlDbType.Decimal] = "0";
            defaultValues[(int)SqlDbType.Float] = "0";
            defaultValues[(int)SqlDbType.Int] = "0";
            defaultValues[(int)SqlDbType.Money] = "0";
            defaultValues[(int)SqlDbType.NChar] = "''";
            defaultValues[(int)SqlDbType.NText] = "''";
            defaultValues[(int)SqlDbType.NVarChar] = "''";
            defaultValues[(int)SqlDbType.Real] = "0";
            defaultValues[(int)SqlDbType.SmallDateTime] = "now()";
            defaultValues[(int)SqlDbType.SmallInt] = "0";
            defaultValues[(int)SqlDbType.SmallMoney] = "0";
            defaultValues[(int)SqlDbType.Text] = "''";
            defaultValues[(int)SqlDbType.TinyInt] = "0";
            defaultValues[(int)SqlDbType.VarChar] = "''";

            #endregion 默认值集合

            #region 数据类型集合唯一哈希

            var names = new TmphSubArray<TmphKeyValue<TmphSqlTypeName, TmphKeyValue<SqlDbType, int>>>(12);
            names.UnsafeAdd(new TmphKeyValue<TmphSqlTypeName, TmphKeyValue<SqlDbType, int>>("bigint",
                new TmphKeyValue<SqlDbType, int>(SqlDbType.BigInt, sizeof(long))));
            names.UnsafeAdd(new TmphKeyValue<TmphSqlTypeName, TmphKeyValue<SqlDbType, int>>("bit",
                new TmphKeyValue<SqlDbType, int>(SqlDbType.Bit, sizeof(bool))));
            names.UnsafeAdd(new TmphKeyValue<TmphSqlTypeName, TmphKeyValue<SqlDbType, int>>("char",
                new TmphKeyValue<SqlDbType, int>(SqlDbType.Char, sizeof(char))));
            names.UnsafeAdd(new TmphKeyValue<TmphSqlTypeName, TmphKeyValue<SqlDbType, int>>("datetime",
                new TmphKeyValue<SqlDbType, int>(SqlDbType.DateTime, sizeof(DateTime))));
            names.UnsafeAdd(new TmphKeyValue<TmphSqlTypeName, TmphKeyValue<SqlDbType, int>>("decimal",
                new TmphKeyValue<SqlDbType, int>(SqlDbType.Decimal, sizeof(decimal))));
            names.UnsafeAdd(new TmphKeyValue<TmphSqlTypeName, TmphKeyValue<SqlDbType, int>>("double",
                new TmphKeyValue<SqlDbType, int>(SqlDbType.Float, sizeof(double))));
            names.UnsafeAdd(new TmphKeyValue<TmphSqlTypeName, TmphKeyValue<SqlDbType, int>>("int",
                new TmphKeyValue<SqlDbType, int>(SqlDbType.Int, sizeof(int))));
            names.UnsafeAdd(new TmphKeyValue<TmphSqlTypeName, TmphKeyValue<SqlDbType, int>>("text",
                new TmphKeyValue<SqlDbType, int>(SqlDbType.Text, int.MinValue)));
            names.UnsafeAdd(new TmphKeyValue<TmphSqlTypeName, TmphKeyValue<SqlDbType, int>>("varchar",
                new TmphKeyValue<SqlDbType, int>(SqlDbType.VarChar, int.MinValue)));
            names.UnsafeAdd(new TmphKeyValue<TmphSqlTypeName, TmphKeyValue<SqlDbType, int>>("float",
                new TmphKeyValue<SqlDbType, int>(SqlDbType.Real, sizeof(float))));
            names.UnsafeAdd(new TmphKeyValue<TmphSqlTypeName, TmphKeyValue<SqlDbType, int>>("smallint",
                new TmphKeyValue<SqlDbType, int>(SqlDbType.SmallInt, sizeof(short))));
            names.UnsafeAdd(new TmphKeyValue<TmphSqlTypeName, TmphKeyValue<SqlDbType, int>>("tinyint",
                new TmphKeyValue<SqlDbType, int>(SqlDbType.TinyInt, sizeof(byte))));
            sqlTypes = new TmphUniqueDictionary<TmphSqlTypeName, TmphKeyValue<SqlDbType, int>>(names.Array, 16);

            #endregion 数据类型集合唯一哈希
        }

        /// <summary>
        ///     获取数据类型名称
        /// </summary>
        /// <param name="type">数据类型</param>
        /// <returns>数据类型名称</returns>
        public static string getSqlTypeName(this SqlDbType type)
        {
            return sqlTypeNames.get((int)type, null);
        }

        /// <summary>
        ///     获取默认值
        /// </summary>
        /// <param name="type">数据类型</param>
        /// <returns>默认值</returns>
        public static string getDefaultValue(this SqlDbType type)
        {
            return defaultValues.get((int)type, null);
        }

        /// <summary>
        ///     格式化数据类型
        /// </summary>
        /// <param name="typeString">数据类型字符串</param>
        /// <param name="size">列长</param>
        /// <returns>数据类型</returns>
        public static unsafe SqlDbType FormatDbType(string typeString, out int size)
        {
            fixed (char* typeFixed = typeString)
            {
                char* end = typeFixed + typeString.Length,
                    typeEnd = *(end - 1) == ')' ? Unsafe.TmphString.Find(typeFixed, end, '(') : end;
                var typeName = new TmphSqlTypeName { TypeName = typeString, Length = (int)(typeEnd - typeFixed) };
                var value = sqlTypes.Get(typeName, new TmphKeyValue<SqlDbType, int>((SqlDbType)(-1), int.MinValue));
                if (value.Value == int.MinValue)
                {
                    size = 0;
                    if (typeEnd != end)
                    {
                        for (--end; ++typeEnd != end; size += *typeEnd - '0') size *= 10;
                    }
                }
                else size = value.Value;
                return value.Key;
            }
        }

        /// <summary>
        ///     数据类型名称唯一哈希
        /// </summary>
        private struct TmphSqlTypeName : IEquatable<TmphSqlTypeName>
        {
            /// <summary>
            ///     数据类型长度
            /// </summary>
            public int Length;

            /// <summary>
            ///     数据类型名称
            /// </summary>
            public string TypeName;

            /// <summary>
            ///     判断是否相等
            /// </summary>
            /// <param name="other">待匹配数据</param>
            /// <returns>是否相等</returns>
            public bool Equals(TmphSqlTypeName other)
            {
                return Length == other.Length && TypeName.EqualCase(other.TypeName, Length);
            }

            /// <summary>
            ///     隐式转换
            /// </summary>
            /// <param name="name">数据类型名称</param>
            /// <returns>数据类型名称唯一哈希</returns>
            public static implicit operator TmphSqlTypeName(string name)
            {
                return new TmphSqlTypeName { TypeName = name, Length = name.Length };
            }

            /// <summary>
            ///     获取哈希值
            /// </summary>
            /// <returns>哈希值</returns>
            public override unsafe int GetHashCode()
            {
                if (TypeName.Length <= 2) return 1;
                fixed (char* nameFixed = TypeName)
                {
                    return ((nameFixed[2] << 2) ^ ((nameFixed[Length >> 2] >> 2) | 0x20)) & (int)((1U << 4) - 1);
                }
            }

            /// <summary>
            ///     判断是否相等
            /// </summary>
            /// <param name="obj">待匹配数据</param>
            /// <returns>是否相等</returns>
            public override bool Equals(object obj)
            {
                return Equals((TmphSqlTypeName)obj);
            }
        }
    }
}