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

using Laurent.Lee.CLB.Sql.Expression;
using Laurent.Lee.CLB.Sql.MsSql;
using Laurent.Lee.CLB.Threading;
using System;

namespace Laurent.Lee.CLB.Sql
{
    /// <summary>
    ///     SQL类型信息
    /// </summary>
    internal sealed class TmphTypeInfo : Attribute
    {
        /// <summary>
        ///     SQL常量转换处理类型集合
        /// </summary>
        private static TmphInterlocked.TmphDictionary<Type, TmphConstantConverter> converters =
            new TmphInterlocked.TmphDictionary<Type, TmphConstantConverter>(
                TmphDictionary.CreateOnly<Type, TmphConstantConverter>());

        /// <summary>
        ///     SQL客户端处理类型
        /// </summary>
        public Type ClientType;

        /// <summary>
        ///     SQL常量转换处理类型
        /// </summary>
        public Type ConverterType;

        /// <summary>
        ///     名称是否忽略大小写
        /// </summary>
        public bool IgnoreCase;

        /// <summary>
        ///     SQL常量转换处理
        /// </summary>
        public TmphConstantConverter Converter
        {
            get
            {
                if (ConverterType == null || ConverterType == typeof(TmphConstantConverter))
                    return TmphConstantConverter.Default;
                TmphConstantConverter value;
                if (converters.TryGetValue(ConverterType, out value)) return value;
                converters.Set(ConverterType,
                    value = (TmphConstantConverter)ConverterType.GetConstructor(null).Invoke(null));
                return value;
            }
        }
    }

    /// <summary>
    ///     SQL类型
    /// </summary>
    public enum TmphType
    {
        /// <summary>
        ///     SQL Server2000
        /// </summary>
        [TmphTypeInfo(ClientType = typeof(TmphSql2000), ConverterType = typeof(TmphConstantConverter), IgnoreCase = true)]
        Sql2000,

        /// <summary>
        ///     SQL Server2005
        /// </summary>
        [TmphTypeInfo(ClientType = typeof(TmphSql2005), IgnoreCase = true)]
        Sql2005,

        /// <summary>
        ///     SQL Server2008
        /// </summary>
        [TmphTypeInfo(ClientType = typeof(TmphSql2005), IgnoreCase = true)]
        Sql2008,

        /// <summary>
        ///     SQL Server2012
        /// </summary>
        [TmphTypeInfo(ClientType = typeof(TmphSql2005), IgnoreCase = true)]
        Sql2012,

        /// <summary>
        ///     Excel
        /// </summary>
        [TmphTypeInfo(ClientType = typeof(Excel.TmphClient))]
        Excel

#if MYSQL
    /// <summary>
    /// MySql
    /// </summary>
        [typeInfo(ClientType = typeof(mySql.TmphClient), IgnoreCase = true)]
        MySql,
#endif
    }
}