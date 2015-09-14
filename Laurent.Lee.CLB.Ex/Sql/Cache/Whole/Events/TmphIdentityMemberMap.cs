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

namespace Laurent.Lee.CLB.Sql.Cache.Whole.Events
{
    /// <summary>
    /// 自增ID整表缓存
    /// </summary>
    /// <typeparam name="TValueType">表格类型</typeparam>
    /// <typeparam name="TModelType">模型类型</typeparam>
    public abstract class TmphIdentityMemberMap<TValueType, TModelType> : TmphCache<TValueType, TModelType>
        where TValueType : class, TModelType
        where TModelType : class
    {
        /// <summary>
        /// 自增ID获取器
        /// </summary>
        protected Func<TModelType, int> getIdentity;

        /// <summary>
        /// 基础ID
        /// </summary>
        protected int baseIdentity;

        /// <summary>
        /// 缓存数据数量
        /// </summary>
        public int Count { get; protected set; }

        /// <summary>
        /// SQL操作缓存
        /// </summary>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="baseIdentity">基础ID</param>
        /// <param name="group">数据分组</param>
        protected TmphIdentityMemberMap(Laurent.Lee.CLB.Emit.TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, int group, int baseIdentity)
            : base(sqlTool, group)
        {
            getIdentity = Laurent.Lee.CLB.Emit.TmphSqlModel<TModelType>.IdentityGetter(this.baseIdentity = baseIdentity);
        }
    }
}