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
    /// 自增ID整表数组缓存
    /// </summary>
    /// <typeparam name="TValueType">表格绑定类型</typeparam>
    /// <typeparam name="TModelType">表格模型类型</typeparam>
    public sealed class TmphIdentityArrayWhere<TValueType, TModelType> : TmphIdentityArray<TValueType, TModelType>
        where TValueType : class, TModelType
        where TModelType : class
    {
        /// <summary>
        /// 数据匹配器
        /// </summary>
        private Func<TValueType, bool> isValue;

        /// <summary>
        /// 自增ID整表数组缓存
        /// </summary>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="isValue">数据匹配器,必须保证更新数据的匹配一致性</param>
        /// <param name="addLength">数组长度递增</param>
        /// <param name="baseIdentity">基础ID</param>
        /// <param name="group">数据分组</param>
        public TmphIdentityArrayWhere(Laurent.Lee.CLB.Emit.TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, Func<TValueType, bool> isValue
            , int group = 0, int baseIdentity = 0, int addLength = 0)
            : base(sqlTool, group, baseIdentity, addLength, false)
        {
            if (isValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            this.isValue = isValue;

            sqlTool.OnInsertedLock += onInserted;
            sqlTool.OnUpdatedLock += onUpdated;
            sqlTool.OnDeletedLock += onDeleted;

            resetLock();
        }

        /// <summary>
        /// 重新加载数据
        /// </summary>
        protected override void reset()
        {
            reset(SqlTool.Where(null, memberMap).getFindArray(isValue));
        }

        /// <summary>
        /// 增加数据
        /// </summary>
        /// <param name="value">新增的数据</param>
        private new void onInserted(TValueType value)
        {
            if (isValue(value)) base.onInserted(value);
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="value">更新后的数据</param>
        /// <param name="oldValue">更新前的数据</param>
        /// <param name="memberMap">更新成员位图</param>
        private new void onUpdated(TValueType value, TValueType oldValue, Laurent.Lee.CLB.Code.TmphMemberMap memberMap)
        {
            if (isValue(value)) base.onUpdated(value, oldValue, memberMap);
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="value">被删除的数据</param>
        private new void onDeleted(TValueType value)
        {
            if (isValue(value)) base.onDeleted(value);
        }
    }
}