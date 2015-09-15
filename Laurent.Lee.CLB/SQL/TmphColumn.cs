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