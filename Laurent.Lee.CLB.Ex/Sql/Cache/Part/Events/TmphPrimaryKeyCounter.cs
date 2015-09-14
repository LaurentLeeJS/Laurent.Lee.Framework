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

namespace Laurent.Lee.CLB.Sql.Cache.Part.Events
{
    /// <summary>
    /// 关键字缓存计数器
    /// </summary>
    /// <typeparam name="TValueType">表格绑定类型</typeparam>
    /// <typeparam name="TModelType">表格模型类型</typeparam>
    /// <typeparam name="TKeyType">关键字类型</typeparam>
    public sealed class TmphPrimaryKeyCounter<TValueType, TModelType, TKeyType> : TmphCounter<TValueType, TModelType, TKeyType>
        where TValueType : class, TModelType
        where TModelType : class
        where TKeyType : IEquatable<TKeyType>
    {
        /// <summary>
        /// 关键字缓存计数器
        /// </summary>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="group">数据分组</param>
        public TmphPrimaryKeyCounter
            (Laurent.Lee.CLB.Emit.TmphSqlTable<TValueType, TModelType, TKeyType> sqlTool, int group = 0)
            : base(sqlTool, group, sqlTool.GetPrimaryKey)
        {
        }
    }
}