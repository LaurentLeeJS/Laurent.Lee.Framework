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

namespace Laurent.Lee.CLB.Sql.Cache
{
    /// <summary>
    /// SQL操作缓存
    /// </summary>
    /// <typeparam name="TValueType">表格类型</typeparam>
    /// <typeparam name="TModelType">模型类型</typeparam>
    public abstract class TmphSqlTool<TValueType, TModelType> : IDisposable
        where TValueType : class, TModelType
        where TModelType : class
    {
        /// <summary>
        /// SQL操作工具
        /// </summary>
        public Laurent.Lee.CLB.Emit.TmphSqlTable.TmphSqlTool<TValueType, TModelType> SqlTool { get; private set; }

        /// <summary>
        ///
        /// </summary>
        protected Laurent.Lee.CLB.Code.TmphMemberMap memberMap;

        /// <summary>
        /// 数据成员位图
        /// </summary>
        public Laurent.Lee.CLB.Code.TmphMemberMap MemberMap
        {
            get { return memberMap; }
        }

        /// <summary>
        /// 成员分组
        /// </summary>
        protected readonly int memberGroup;

        /// <summary>
        /// SQL操作缓存
        /// </summary>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="group">数据分组</param>
        protected TmphSqlTool(Laurent.Lee.CLB.Emit.TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, int group)
        {
            if (sqlTool == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            memberGroup = group;
            SqlTool = sqlTool;
            memberMap = Laurent.Lee.CLB.Emit.TmphSqlModel<TModelType>.GetCacheMemberMap(group);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public virtual void Dispose()
        {
            TmphPub.Dispose(ref memberMap);
        }

        /// <summary>
        /// 获取更新成员位图
        /// </summary>
        /// <param name="memberMap">更新成员位图</param>
        /// <returns>更新成员位图</returns>
        protected Laurent.Lee.CLB.Code.TmphMemberMap updateMemberMap(Laurent.Lee.CLB.Code.TmphMemberMap memberMap)
        {
            Laurent.Lee.CLB.Code.TmphMemberMap newMemberMap = this.memberMap.Copy();
            newMemberMap.And(memberMap);
            return newMemberMap;
        }
    }
}