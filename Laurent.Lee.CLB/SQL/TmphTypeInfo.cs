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