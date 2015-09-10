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

using System;
using System.Data;

namespace Laurent.Lee.CLB.Sql
{
    /// <summary>
    ///     数据列
    /// </summary>
    internal sealed class TmphColumn
    {
        /// <summary>
        ///     数据库类型
        /// </summary>
        public SqlDbType DbType;

        /// <summary>
        ///     默认值
        /// </summary>
        public string DefaultValue;

        /// <summary>
        ///     是否允许为空
        /// </summary>
        public bool IsNull;

        /// <summary>
        ///     列名
        /// </summary>
        public string Name;

        /// <summary>
        ///     备注说明
        /// </summary>
        public string Remark;

        /// <summary>
        ///     列长
        /// </summary>
        public int Size;

        /// <summary>
        ///     表格列类型
        /// </summary>
        public Type SqlColumnType;

        /// <summary>
        ///     新增字段时的计算子查询
        /// </summary>
        public string UpdateValue;

        /// <summary>
        ///     判断是否匹配数据列
        /// </summary>
        /// <param name="value">数据列</param>
        /// <param name="isIgnoreCase">是否忽略大小写</param>
        /// <returns>是否匹配</returns>
        internal bool IsMatch(TmphColumn value, bool isIgnoreCase)
        {
            return value != null && (isIgnoreCase ? Name.ToLower() == value.Name.ToLower() : Name == value.Name) &&
                   DbType == value.DbType && Size == value.Size && IsNull == value.IsNull;
        }
    }
}